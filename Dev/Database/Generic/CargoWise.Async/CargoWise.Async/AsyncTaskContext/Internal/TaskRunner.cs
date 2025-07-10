using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CargoWise.Async.AsyncTaskContext.Public;

namespace CargoWise.Async.AsyncTaskContext.Internal
{
	struct TaskRunner : ITaskRunner
	{
		readonly ITaskContextInternal taskManager;
		readonly bool trackCausality;

		public TaskRunner(ITaskContextInternal taskManager, bool trackCausality)
		{
			this.taskManager = taskManager;
			this.trackCausality = trackCausality;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public Task<T> EnqueueTask<T>(Func<T> func)
		{
			var completion = default(AsyncTaskMethodBuilder<T>);
			var result = completion.Task;
			var callStack = this.trackCausality ? Environment.StackTrace : string.Empty;
			var runner = this;

			EnqueueTaskToManager(() =>
			{
				try
				{
					completion.SetResult(func.Invoke());
				}
				catch (Exception ex)
				{
					if (runner.trackCausality)
					{
						ex.AddDependencyChainedStack(callStack);
					}

					completion.SetException(ex);
				}
			});

			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public Task<T> EnqueueTask<T>(Func<Task<T>> func)
		{
			var completion = default(AsyncTaskMethodBuilder<T>);
			var result = completion.Task;
			var callStack = this.trackCausality ? Environment.StackTrace : string.Empty;
			var runner = this;

			EnqueueTaskToManager(async () =>
			{
				try
				{
					completion.SetResult(await func.Invoke());
				}
				catch (Exception ex)
				{
					if (runner.trackCausality)
					{
						ex.AddDependencyChainedStack(callStack);
					}

					completion.SetException(ex);
				}
			});

			return result;
		}

		public Task EnqueueTask(Action func)
		{
			return EnqueueTask(() => { func.Invoke(); return 0; });
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public Task EnqueueTask(Func<Task> func)
		{
			var completion = default(AsyncTaskMethodBuilder);
			var result = completion.Task;
			var callStack = this.trackCausality ? Environment.StackTrace : string.Empty;
			var runner = this;

			EnqueueTaskToManager(async () =>
			{
				try
				{
					await func.Invoke();
					completion.SetResult();
				}
				catch (Exception ex)
				{
					if (runner.trackCausality)
					{
						ex.AddDependencyChainedStack(callStack);
					}

					completion.SetException(ex);
				}
			});

			return result;
		}

		void EnqueueTaskToManager(Action runTask)
		{
			taskManager.EnqueueTask(runTask);
		}
	}
}
