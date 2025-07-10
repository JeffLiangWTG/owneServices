using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace CargoWise.Async
{
	public delegate Task AutoRefreshAction(CancellationToken cancellationToken);

	public interface IAsyncStrategy
	{
		Task DoAsync(Action action, IThreadSentry threadSentry = null, [CallerMemberName] string threadName = "");
		Thread DoAsyncAsThread(Action action, ApartmentState apartmentState = ApartmentState.MTA, IThreadSentry threadSentry = null, [CallerMemberName] string threadName = "");
		Task<T> GetAsync<T>(Func<T> func, IThreadSentry threadSentry = null, [CallerMemberName] string threadName = "", CancellationTokenSource cancellationTokenSource = null);
		void ParallelForEach<T>(IEnumerable<T> source, Action<T> body);

		IAutoRefresher GetAutoRefresher(AutoRefreshAction updateAction, TimeSpan delay, Func<bool> shouldRefresh = null);
	}
}
