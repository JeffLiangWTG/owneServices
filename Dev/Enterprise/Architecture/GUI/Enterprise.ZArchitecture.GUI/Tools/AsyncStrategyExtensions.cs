using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Threading;
using CargoWise.Async;

namespace Enterprise.ZArchitecture.GUI
{
	public static class AsyncStrategyExtensions
	{
		public static Thread RunInAnotherWinformsThreadAsync(this IAsyncStrategy asyncStrategy, Action action, IThreadSentry threadSentry = null, [CallerMemberName] string threadName = "")
		{
			return asyncStrategy.DoAsyncAsThread(() =>
			{
				SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
				action();
			}, ApartmentState.STA, threadSentry, threadName);
		}
	}
}
