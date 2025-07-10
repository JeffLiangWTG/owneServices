using System;
using System.Threading;
using CargoWise.Async;

namespace Enterprise.VisualBoards.Business
{
	public static class VisualBoardsExtensionMethods
	{
		#region Thread

		public static void RequireBackgroundThread(this Thread thread)
		{
			if (AsyncStrategy.Default is DefaultAsyncStrategy && RegisterDispatcher.IsDispatcherRegistered(thread.ManagedThreadId))
			{
				throw new CrossThreadAccessException("This code path must be accessed on a background thread only");
			}
		}

		public static void RequireNotOnVisualBoardFormThread(this Thread thread)
		{
			if (AsyncStrategy.Default is DefaultAsyncStrategy && thread.Name != null && thread.Name.StartsWith(VisualBoardThreadHelper.VisualBoardFormThreadNamePrefix, StringComparison.Ordinal))
			{
				throw new CrossThreadAccessException("This code path must not be accessed on the VisualBoardForm thread");
			}
		}

		#endregion
	}
}
