using System;
using System.Threading;
using CargoWise.Common;

namespace CargoWise.Async
{
	public static class ExtensionMethods
	{
		public static void ThrowExceptionOnMainThread(this Exception ex)
		{
			if (ApplicationDispatcher.Current == null)
			{
				ErrorReporter.ReportOnce("Application dispatcher was null - could not marshall this exception to the main thread exception handler", ex);
			}
			else
			{
				ApplicationDispatcher.Current.BeginInvoke(new Action(() =>
				{
					throw new BackgroundThreadException($"An exception occurred on another thread. Current ThreadID: {Thread.CurrentThread.ManagedThreadId}", ex);
				}));
			}
		}
	}
}
