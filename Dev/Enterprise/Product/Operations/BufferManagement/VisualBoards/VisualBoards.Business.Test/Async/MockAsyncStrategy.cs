using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;

namespace Enterprise.VisualBoards.Business.Test
{
	public class MockAsyncStrategy : IAsyncStrategy
	{
		public Task DoAsync(Action action, IThreadSentry threadSentry = null, string threadName = "")
		{
			action();
			return Task.FromResult(false);
		}

		public Thread DoAsyncAsThread(Action action, ApartmentState apartmentState = ApartmentState.MTA, IThreadSentry threadSentry = null, string threadName = "")
		{
			action();
			return Thread.CurrentThread;
		}

		public Task<T> GetAsync<T>(Func<T> func, IThreadSentry threadSentry = null, string threadName = "", CancellationTokenSource cancellationTokenSource = null)
		{
			return Task.FromResult(func());
		}

		public void ParallelForEach<T>(IEnumerable<T> source, Action<T> body)
		{
			foreach (var item in source)
			{
				body(item);
			}
		}

		public IAutoRefresher GetAutoRefresher(AutoRefreshAction updateAction, TimeSpan delay, Func<bool> shouldRefresh = null)
		{
			return new TriggerableAutoRefresher(updateAction, delay, shouldRefresh);
		}
	}
}
