using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace CargoWise.Async
{
	public static class AsyncTaskSynchronizer
	{
		[SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public static T Run<T>(Func<Task<T>> func)
		{
			var oldContext = SynchronizationContext.Current;
			T result = default;
			using (var syncContext = new ExclusiveSynchronizationContext())
			{
				SynchronizationContext.SetSynchronizationContext(syncContext);
				syncContext.Post(async _ =>
				{
					try
					{
						result = await func();
					}
					catch (Exception e)
					{
						syncContext.InnerException = e;
						throw;
					}
					finally
					{
						syncContext.EndWorkItemsLoop();
					}
				}, null);
				syncContext.BeginWorkItemsLoop();
			}
			SynchronizationContext.SetSynchronizationContext(oldContext);
			return result;
		}

		public static void Run(Func<Task> func)
		{
			var oldContext = SynchronizationContext.Current;
			using (var syncContext = new ExclusiveSynchronizationContext())
			{
				SynchronizationContext.SetSynchronizationContext(syncContext);
				syncContext.Post(async _ =>
				{
					try
					{
						await func();
					}
					catch (Exception e)
					{
						syncContext.InnerException = e;
						throw;
					}
					finally
					{
						syncContext.EndWorkItemsLoop();
					}
				}, null);

				syncContext.BeginWorkItemsLoop();
			}

			SynchronizationContext.SetSynchronizationContext(oldContext);
		}

		class WorkItem
		{
			public WorkItem(SendOrPostCallback task, object args)
			{
				Task = task;
				Args = args;
			}

			public SendOrPostCallback Task { get; }
			public object Args { get; }
		}

		class ExclusiveSynchronizationContext : SynchronizationContext, IDisposable, IThreadSentryPostingControl
		{
			bool done;
			internal Exception InnerException { get; set; }
			readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
			readonly Queue<WorkItem> items =
				new Queue<WorkItem>();

			public override void Send(SendOrPostCallback x, object y)
			{
				throw new NotSupportedException("We cannot send to our same thread");
			}

			[SuppressMessage("Microsoft.Contracts", "Nonnull-11-0")]
			[SuppressMessage("Microsoft.Contracts", "Nonnull-71-0")]
			[SuppressMessage("Microsoft.Contracts", "Nonnull-6-0")]
			public override void Post(SendOrPostCallback task, object state)
			{
				if (workItemsWaiting.SafeWaitHandle.IsClosed)
				{
					base.Post(task, state);
				}
				else
				{
					if (task == null)
					{
						throw new ArgumentNullException(nameof(task));
					}
					lock (items)
					{
						items.Enqueue(new WorkItem(task, state));
					}

					workItemsWaiting.Set();
				}
			}
			internal void EndWorkItemsLoop()
			{
				Post(_ => done = true, null);
			}

			[SuppressMessage("Microsoft.Contracts", "Nonnull-118-0")]
			[SuppressMessage("Microsoft.Contracts", "Nonnull-27-0")]
			internal void BeginWorkItemsLoop()
			{
				while (!done)
				{
					WorkItem workItem = null;
					lock (items)
					{
						if (items.Count > 0)
						{
							workItem = items.Dequeue();
						}
					}
					if (workItem?.Task != null)
					{
						workItem.Task(workItem.Args);
						if (InnerException != null)
						{
							throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
						}
					}
					else
					{
						workItemsWaiting.WaitOne();
					}
				}
			}

			public override SynchronizationContext CreateCopy()
			{
				return this;
			}

			public void Dispose()
			{
				if (workItemsWaiting != null)
				{
					workItemsWaiting.Dispose();
				}
			}

			bool IThreadSentryPostingControl.EnablePosting => false;
		}
	}
}
