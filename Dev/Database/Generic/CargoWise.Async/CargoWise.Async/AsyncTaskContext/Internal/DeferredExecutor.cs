using System;
using CargoWise.Async.AsyncTaskContext.Public;

namespace CargoWise.Async.AsyncTaskContext.Internal
{
	sealed class DeferredExecutor : IDeferredExecutor
	{
		readonly ITaskContextInternal taskManager;
		readonly bool trackCausality;

		public DeferredExecutor(ITaskContextInternal taskManager, bool trackCausality)
		{
			this.taskManager = taskManager;
			this.trackCausality = trackCausality;
		}

		public void EnqueueTask(Action runTask)
		{
			var callStack = this.trackCausality ? Environment.StackTrace : string.Empty;

			this.taskManager.EnqueueSelfObservingTask(() =>
			{
				try
				{
					runTask.Invoke();
				}
				catch (Exception ex)
				{
					if (trackCausality)
					{
						ex.AddDependencyChainedStack(callStack);
					}

					throw;
				}
			});
		}
	}
}
