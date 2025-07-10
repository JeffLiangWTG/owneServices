using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;

namespace CargoWise.Async
{
	public abstract class AsyncronousActionExecutionStrategyBase : IActionExecutionStrategy
	{
		protected AsyncronousActionExecutionStrategyBase(bool cancelrunningTasksOnNewTask)
		{
			this.cancelrunningTasksOnNewTask = cancelrunningTasksOnNewTask; // Note: this niavely assumes that only one thing is running async per form.
			this.runningTasks = new List<Tuple<Task, CancellationTokenSource>>();
		}
		readonly List<Tuple<Task, CancellationTokenSource>> runningTasks;
		readonly protected bool cancelrunningTasksOnNewTask;

		#region Manage Tasks

		protected void AwaitRunningTasks()
		{
			var tasks = runningTasks.Select(t => t.Item1).Where(task => task != null && !task.IsCanceled && !task.IsCompleted).ToArray();
			Task.WaitAll(tasks);
			CleanSpawnedTasks();
		}

		protected void AddTask(Task<int> task, CancellationTokenSource cancellationTokenSource)
		{
			Argument.NotNull(task, nameof(task));

			if (cancelrunningTasksOnNewTask)
			{
				CancelSpawnedTasks();
			}

			runningTasks.Add(Tuple.Create((Task)task, cancellationTokenSource));
			CleanSpawnedTasks();
		}

		protected void DisposeTasks()
		{
			CancelSpawnedTasks();
		}

		void CancelSpawnedTasks()
		{
			foreach (var tuple in runningTasks)
			{
				if (tuple.Item2 != null && tuple.Item1 != null && !tuple.Item1.IsCompleted)
				{
					tuple.Item2.Cancel();
				}
			}
		}

		void CleanSpawnedTasks()
		{
			runningTasks.RemoveAll(t => t.Item1.IsCompleted || t.Item1.IsCanceled);
		}

		#endregion

		#region IActionExecutionStrategy

		protected abstract void SynchroniseIntoMainContextCore(Action action);

		protected abstract IAsyncStrategy AsyncStrategy { get; }

		public void StartOperation(Action action)
		{
			action();
		}

		void IActionExecutionStrategy.DoParallelisableTransform(Action action, CancellationTokenSource cancellationTokenSource)
		{
			DoParallelisableTransformCore(action, cancellationTokenSource);
		}

		void DoParallelisableTransformCore(Action action, CancellationTokenSource cancellationTokenSource)
		{
			var task = AsyncStrategy.GetAsync(WrapAction(action), cancellationTokenSource: cancellationTokenSource);
			AddTask(task, cancellationTokenSource);
		}

		void IActionExecutionStrategy.SynchroniseIntoMainContext(Action action)
		{
			SynchroniseIntoMainContextCore(action);
		}

		#endregion

		#region Utilities

		static Func<int> WrapAction(Action action)
		{
			return () =>
			{
				action();
				return 0;
			};
		}

		#endregion
	}
}