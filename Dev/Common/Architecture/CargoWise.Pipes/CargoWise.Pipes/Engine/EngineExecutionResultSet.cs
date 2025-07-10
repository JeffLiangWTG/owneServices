using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CargoWise.Pipes
{
	public class EngineExecutionResultSet : IDisposable
	{
		internal EngineExecutionResultSet(EngineCachingStrategy engineCachingStrategy)
		{
			Cache = engineCachingStrategy;
			engineCachingStrategy.Initialise();
		}

		internal EngineCachingStrategy Cache { get; }
		public bool IsDisposed { get; private set; }

		internal CancellationTokenSource CancellationTokenSource
		{
			get { return cancellationTokenSource; }
		}

		readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

		internal ConcurrentDictionary<Guid, IPipeHandoverWrapper> Handovers { get; } = new ConcurrentDictionary<Guid, IPipeHandoverWrapper>();

		readonly HashSet<Task> tasks = new HashSet<Task>();

		internal void AddTask(Task task)
		{
			lock (tasks)
			{
				Task continuation = null;
				continuation = task.ContinueWith(t =>
				{
					lock (tasks)
					{
						tasks.Remove(task);
						tasks.Remove(continuation);
					}
				});

				if (!task.IsCompleted && !task.IsFaulted)
				{
					tasks.Add(task);
					tasks.Add(continuation);
				}
			}
		}

		public T GetResult<T>(IPipeDataSource<T> pipe)
		{
			return (T)Cache.GetResult(pipe);
		}

		public void AwaitAll()
		{
			while (GetRunningTasks().Length > 0)
			{
				Task.WaitAll(GetRunningTasks());
			}
		}

		Task[] GetRunningTasks()
		{
			lock (tasks)
			{
				return tasks.ToArray();
			}
		}

		#region IDisposable Support

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					cancellationTokenSource.Cancel();
					cancellationTokenSource.Dispose();
				}

				IsDisposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
