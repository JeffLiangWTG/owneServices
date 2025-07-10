using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CargoWise.Async;
using WinzorFramework;

public static class AsyncStrategyExtensions
{
	public static async Task RunInAnotherWinformsThreadAsync(this IAsyncStrategy asyncStrategy, Action action, IThreadSentry threadSentry = null, [CallerMemberName] string threadName = "")
	{
		var formOpener = WinzorDispatcher.Current.FormOpener;
		var formRegister = WinzorDispatcher.Current.FormInstanceRegister;
		var previousDispatcherContext = WinzorDispatcher.Current.CurrentContext;

		using var winzorDispatcher = new WinzorDispatcher(formOpener, formRegister, threadName, isBackgroundThread: true);
		await winzorDispatcher.InvokeAsync(() =>
		{
			using (winzorDispatcher.WithContext(previousDispatcherContext))
			{
				action();
			}
		});
	}
}
