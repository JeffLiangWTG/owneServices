using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CargoWise.Common.Async
{
	public class DynamicParallel<T>
	{
		readonly IEnumerable<T> itemsEnumerable;
		readonly TimeSpan interval;
		readonly Action<T> processingAction;
		readonly int maxThreadCount;
		readonly CancellationToken cancellationToken;
		readonly List<Task> workers;

		DynamicParallel(IEnumerable<T> items, TimeSpan interval, Action<T> processingAction, int maxThreadCount, CancellationToken cancellationToken)
		{
			this.itemsEnumerable = items;
			this.interval = interval;
			this.processingAction = processingAction;
			this.maxThreadCount = maxThreadCount;
			this.cancellationToken = cancellationToken;
			this.workers = new List<Task>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Concurrency", "CW1024:Bad Concurrent Collection Access", Justification = "Concurrent threads stopped by time of modification")]
		void ProcessItems()
		{
			var errors = new ConcurrentBag<Exception>();
			var items = new ConcurrentBag<T>(itemsEnumerable);

			Action worker = () =>
			{
				while (items.TryTake(out var item))
				{
					try
					{
						if (cancellationToken.IsCancellationRequested)
						{
							break;
						}

						processingAction(item);
					}
					catch (Exception ex)
					{
						errors.Add(ex);
					}
				}
			};

			Action scheduler = () =>
			{
				while (items.Any() && workers.Count < maxThreadCount - 1)
				{
					if (!SpinWait.SpinUntil(() => !items.Any(), interval))
					{
						var workerTask = Task.Run(worker);
						workers.Add(workerTask);
					}
				}
			};

			var schedulerTask = Task.Run(scheduler);
			worker();
			schedulerTask.Wait();
			Task.WaitAll(workers.ToArray());

			if (errors.Any())
			{
				if (cancellationToken.IsCancellationRequested)
				{
					errors.Add(new OperationCanceledException());
				}
				throw new AggregateException("One or more errors occurred during processing", errors);
			}
			cancellationToken.ThrowIfCancellationRequested();
		}

		public static void Invoke(IEnumerable<T> items, Action<T> processingAction, TimeSpan interval, int maxThreadCount, CancellationToken cancellationToken)
		{
			if (interval < TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be larger than zero seconds");
			}

			if (maxThreadCount < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(maxThreadCount), "Max thread count must be larger than one");
			}

			if (!items.Any())
			{
				throw new ArgumentException("Unable to process empty items", nameof(items));
			}

			var instance = new DynamicParallel<T>(items, interval, processingAction, maxThreadCount, cancellationToken);
			instance.ProcessItems();
		}
	}
}
