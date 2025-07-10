using System;
using System.Collections.Generic;
using System.Threading;

namespace CargoWise.Common.Async
{
	public static class ParallelExtensions
	{
		/// <summary>
		/// Functions similarly Parallel.ForEach.
		/// Runs processor action for each item in the collection in a separate thread.
		/// If processing has not completed for every item upon an interval a new thread will be actioned to perform work.
		/// </summary>
		/// <exception cref="AggregateException">Aggregate of all exceptions across threads</exception>
		/// <exception cref="ArgumentException">When interval or max threads is invalid</exception>
		/// <exception cref="OperationCanceledException">When cancellation token is cancelled</exception>
		/// <typeparam name="T">Anything</typeparam>
		/// <param name="collection"></param>
		/// <param name="processor"></param>
		/// <param name="processingInterval"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="maxThreadCount"></param>
		public static void ParallelForEachDynamic<T>(this IEnumerable<T> collection, Action<T> processor, TimeSpan processingInterval, CancellationToken cancellationToken, int maxThreadCount = 10)
		{
			DynamicParallel<T>.Invoke(collection, processor, processingInterval, maxThreadCount, cancellationToken);
		}
	}
}
