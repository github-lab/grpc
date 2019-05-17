#region Copyright notice and license

// Copyright 2015 gRPC authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Grpc.Core.Utils
{
    /// <summary>
    /// Extension methods that simplify work with gRPC streaming calls.
    /// </summary>
    public static class AsyncStreamExtensions
    {
        /// <summary>
        /// Advances the stream reader to the next element in the sequence, returning the result asynchronously.
        /// </summary>
        /// <typeparam name="T">The message type.</typeparam>
        /// <param name="streamReader">The stream reader.</param>
        /// <returns>
        /// Task containing the result of the operation: true if the reader was successfully advanced
        /// to the next element; false if the reader has passed the end of the sequence.
        /// </returns>
        public static Task<bool> MoveNext<T>(this IAsyncStreamReader<T> streamReader)
            where T : class
        {
            if (streamReader == null)
            {
                throw new ArgumentNullException(nameof(streamReader));
            }

            return streamReader.MoveNext(CancellationToken.None);
        }

        /// <summary>
        /// Reads the entire stream and executes an async action for each element.
        /// </summary>
        public static async Task ForEachAsync<T>(this IAsyncStreamReader<T> streamReader, Func<T, Task> asyncAction)
            where T : class
        {
            if (streamReader == null)
            {
                throw new ArgumentNullException(nameof(streamReader));
            }
            if (asyncAction == null)
            {
                throw new ArgumentNullException(nameof(asyncAction));
            }

            while (await streamReader.MoveNext().ConfigureAwait(false))
            {
                await asyncAction(streamReader.Current).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Reads the entire stream and creates a list containing all the elements read.
        /// </summary>
        public static async Task<List<T>> ToListAsync<T>(this IAsyncStreamReader<T> streamReader)
            where T : class
        {
            if (streamReader == null)
            {
                throw new ArgumentNullException(nameof(streamReader));
            }

            var result = new List<T>();
            while (await streamReader.MoveNext().ConfigureAwait(false))
            {
                result.Add(streamReader.Current);
            }
            return result;
        }

        /// <summary>
        /// Writes all elements from given enumerable to the stream.
        /// Completes the stream afterwards unless close = false.
        /// </summary>
        public static async Task WriteAllAsync<T>(this IClientStreamWriter<T> streamWriter, IEnumerable<T> elements, bool complete = true)
            where T : class
        {
            if (streamWriter == null)
            {
                throw new ArgumentNullException(nameof(streamWriter));
            }
            if (elements == null)
            {
                throw new ArgumentNullException(nameof(elements));
            }

            foreach (var element in elements)
            {
                await streamWriter.WriteAsync(element).ConfigureAwait(false);
            }
            if (complete)
            {
                await streamWriter.CompleteAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Writes all elements from given enumerable to the stream.
        /// </summary>
        public static async Task WriteAllAsync<T>(this IServerStreamWriter<T> streamWriter, IEnumerable<T> elements)
            where T : class
        {
            if (streamWriter == null)
            {
                throw new ArgumentNullException(nameof(streamWriter));
            }
            if (elements == null)
            {
                throw new ArgumentNullException(nameof(elements));
            }

            foreach (var element in elements)
            {
                await streamWriter.WriteAsync(element).ConfigureAwait(false);
            }
        }
    }
}
