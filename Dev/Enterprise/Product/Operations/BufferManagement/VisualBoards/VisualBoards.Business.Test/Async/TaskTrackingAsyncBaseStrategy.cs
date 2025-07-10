using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Async;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.VisualBoards.Business.Test
{
	// To synchronise threads in unit tests and all threads end when the test ends
	public class TaskTrackingAsyncBaseStrategy : DBConnectionDisposalAsyncStrategy, IDisposable
	{
		public TaskTrackingAsyncBaseStrategy(bool createTriggerableAutoRefresher = false) : base()
		{
			this.createTriggerableAutoRefresher = createTriggerableAutoRefresher;
		}

		readonly bool createTriggerableAutoRefresher;

		protected override IAutoRefresher GetAutoRefresherCore(AutoRefreshAction task, TimeSpan delay, Func<bool> shouldRefresh)
		{
			return createTriggerableAutoRefresher
				? new TriggerableAutoRefresher(task, delay)
				: new AutoRefresher(task, delay, shouldRefresh);
		}

		readonly ConcurrentQueue<Task> tasks = new ConcurrentQueue<Task>();

		protected override void OnTaskCreated(Task task)
		{
			base.OnTaskCreated(task);
			tasks.Enqueue(task);
		}

		[SuppressMessage("CargoWiseOne", "CW1049:Using Application.DoEvents", Justification = "For Unit Test")]
		public void AwaitAll(Task taskToIgnore, Action additionalWaitAction = null)
		{
			AwaitAll(t => t != taskToIgnore, () => additionalWaitAction?.Invoke());
		}

		/// <summary>
		/// The dirty dirty way of guaranteeing that Tasks created by this strategy complete.
		/// </summary>
		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess", Justification = "Relax. This pattern of access works just fine.")]
		protected void AwaitAll(Func<Task, bool> filter, Action additionalWaitAction)
		{
			try
			{
				do
				{
					while (tasks.TryDequeue(out var task))
					{
						if (filter(task))
						{
							task.Wait();
						}
					}
					additionalWaitAction();
				}
				while (tasks.TryPeek(out var _));
			}
			catch (AggregateException ex) when (ex.InnerException is TaskCanceledException || ex.Flatten().InnerExceptions.All(e => e is TaskCanceledException))
			{
				// in case a task is cancelled and the TaskCanceledException is nested in AggregateException.
			}
			catch (TaskCanceledException)
			{
				// Nom. Sometimes this exception is thrown when control is disposed. We do not care here though.
			}
			catch (AggregateException ex) when (ex.InnerException is ObjectDisposedException || ex.Flatten().InnerExceptions.All(e => e is ObjectDisposedException))
			{
				// in case a task is cancelled and the ObjectDisposedException is nested in AggregateException.
			}
			catch (ObjectDisposedException)
			{
				// Nom. Sometimes this exception is thrown when control is disposed. We do not care here though.
			}
		}

		public void Dispose()
		{
			AwaitAll(null);
		}
	}
}
