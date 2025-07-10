using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async.AsyncTaskContext.Public;

namespace CargoWise.Async.AsyncTaskContext.Internal
{
	sealed class PumpedTaskManager : ITaskContextInternal, IPumpedTaskContext
	{
		readonly ConcurrentQueue<(Action Task, AsyncTaskMethodBuilder Completion, bool SelfCompleting)> tasks = new ConcurrentQueue<(Action, AsyncTaskMethodBuilder, bool)>();
		readonly Action<Exception> exceptionHandler;
		readonly Func<bool> enableCausalityTracking;
		readonly AutoResetEvent pumpingRequired;

		public string ContextName { get; } = "Pumped Task Manager " + Guid.NewGuid(); // Log information

		public bool CausalityTracking
		{
			get
			{
				return enableCausalityTracking();
			}
		}

		public EventWaitHandle PumpingRequired
		{
			get
			{
				return pumpingRequired;
			}
		}

		public PumpedTaskManager(Action<Exception> exceptionHandler, Func<bool> enableCausalityTracking)
		{
			this.exceptionHandler = exceptionHandler;
			this.enableCausalityTracking = enableCausalityTracking;
			this.pumpingRequired = new AutoResetEvent(false);
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule", Justification = "Ensures each pump is a fixed batch of tasks, and not a potentially increasing set")]
		public void PumpTasks()
		{
			var taskCount = tasks.Count;

			for (int i = 0; i < taskCount; ++i)
			{
				if (tasks.TryDequeue(out var task))
				{
					try
					{
						task.Task.Invoke();
						task.Completion.SetResult();
					}
					catch (Exception ex)
					{
						if (!task.SelfCompleting)
						{
							task.Completion.SetException(ex);
						}
						else
						{
							this.exceptionHandler(ex);
						}
					}
				}
			}
		}

		public Task EnqueueTask(Action runTask)
		{
			return InternalEnqueueTask(runTask, false);
		}

		public void EnqueueSelfObservingTask(Action runTask)
		{
			InternalEnqueueTask(runTask, true);
		}

		Task InternalEnqueueTask(Action runTask, bool selfCompleting)
		{
			var completion = default(AsyncTaskMethodBuilder);
			tasks.Enqueue((runTask, completion, selfCompleting));
			pumpingRequired.Set();
			return completion.Task;
		}

		public void Dispose()
		{
			PumpTasks();
			pumpingRequired.Dispose();
		}
	}
}
