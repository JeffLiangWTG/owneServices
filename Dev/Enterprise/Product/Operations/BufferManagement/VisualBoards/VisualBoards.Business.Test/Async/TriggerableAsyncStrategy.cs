using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.Common;

namespace Enterprise.VisualBoards.Business.Test
{
	public class TriggerableAsyncStrategy : IAsyncStrategy
	{
		readonly Queue<Action> asyncQueue = new Queue<Action>();

		public Queue<Action> Actions
		{
			get { return asyncQueue; }
		}

		public Task DoAsync(Action action, IThreadSentry threadSentry = null, string threadName = "")
		{
			var pair = WrapActionInTask<object>(action);
			asyncQueue.Enqueue(pair.Item1);
			return pair.Item2;
		}

		public Thread DoAsyncAsThread(Action action, ApartmentState apartmentState = ApartmentState.MTA, IThreadSentry threadSentry = null, string threadName = "")
		{
			asyncQueue.Enqueue(action);
			return Thread.CurrentThread;
		}

		public Task<T> GetAsync<T>(Func<T> func, IThreadSentry threadSentry = null, string threadName = "", CancellationTokenSource cancellationTokenSource = null)
		{
			asyncQueue.Enqueue(() => func());
			var pair = WrapActionInTask<T>(func);
			asyncQueue.Enqueue(pair.Item1);
			return pair.Item2;
		}

		public void ParallelForEach<T>(IEnumerable<T> source, Action<T> body)
		{
			var action = new Action(() =>
				{
					foreach (var item in source)
					{
						body(item);
					}
				});

			asyncQueue.Enqueue(action);
		}

		public void DoFirstAction()
		{
			var action = asyncQueue.Dequeue();
			action();
		}

		public void DoAllActions()
		{
			while (asyncQueue.Count > 0)
			{
				DoFirstAction();
			}
		}

		public IAutoRefresher GetAutoRefresher(AutoRefreshAction task, TimeSpan delay, Func<bool> shouldRefresh = null)
		{
			return new TriggerableAutoRefresher(task, delay);
		}

		#region Implementation

		Tuple<Action, Task<T>> WrapActionInTask<T>(Delegate input)
		{
			var tcs = new TaskCompletionSource<T>();

			var action = new Action(() =>
			{
				try
				{
					tcs.SetResult((T)input.DynamicInvoke());
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					tcs.SetException(e);
				}
			});

			return Tuple.Create(action, tcs.Task);
		}

		#endregion
	}
}
