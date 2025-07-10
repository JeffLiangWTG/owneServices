using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;

namespace CargoWise.Pipes.Test
{
	public interface IUniversalDispatcher : IAsyncStrategy, IDispatcher
	{
		void DispatchAll();
	}

	public class MockDispatcher : IUniversalDispatcher
	{
		readonly Queue<Action> dispatchQueue = new Queue<Action>();

		public bool TryDispatchOne()
		{
			if (dispatchQueue.Count > 0)
			{
				dispatchQueue.Dequeue().Invoke();
				return true;
			}
			else
			{
				return false;
			}
		}

		public void DispatchAll()
		{
			while (TryDispatchOne())
			{
				// Doopydoop doop.
			}
		}

		#region Interface Implementation

		void IDispatcher.Dispatch(Delegate method, params object[] args)
		{
			dispatchQueue.Enqueue(() => method.DynamicInvoke(args));
		}

		Task IAsyncStrategy.DoAsync(Action action, IThreadSentry threadSentry, string threadName)
		{
			var tuple = WrapWithTask<object>(action);
			dispatchQueue.Enqueue(tuple.Item2);
			return tuple.Item1;
		}

		Thread IAsyncStrategy.DoAsyncAsThread(Action action, ApartmentState apartmentState, IThreadSentry threadSentry, string threadName)
		{
			dispatchQueue.Enqueue(action);
			return Thread.CurrentThread;
		}

		Task<T> IAsyncStrategy.GetAsync<T>(Func<T> func, IThreadSentry threadSentry, string threadName, CancellationTokenSource cancellationTokenSource)
		{
			var tuple = WrapWithTask<T>(func);
			dispatchQueue.Enqueue(tuple.Item2);
			return tuple.Item1;
		}

		IAutoRefresher IAsyncStrategy.GetAutoRefresher(AutoRefreshAction updateAction, TimeSpan delay, Func<bool> shouldRefresh)
		{
			throw new NotImplementedException();
		}

		void IAsyncStrategy.ParallelForEach<T>(IEnumerable<T> source, Action<T> body)
		{
			dispatchQueue.Enqueue(() =>
			{
				foreach (var entity in source)
				{
					body(entity);
				}
			});
		}

		Tuple<Task<T>, Action> WrapWithTask<T>(Delegate del)
		{
			Thread.CurrentThread.RequireMainThread();

			var tcs = new TaskCompletionSource<T>();
			var wrapperAction = new Action(() =>
			{
				Thread.CurrentThread.RequireMainThread();

				try
				{
					tcs.SetResult((T)del.DynamicInvoke());
				}
#pragma warning disable ENT0001
				catch (Exception e) // Test
#pragma warning restore ENT0001
				{
					tcs.SetException(e);
					throw;
				}
			});

			return Tuple.Create(tcs.Task, wrapperAction);
		}

		#endregion
	}
}
