using System;
using System.Threading;
using CargoWise.Common;

namespace CargoWise.Async
{
	public static class ThreadsInvoke
	{
		public static void ActionByThread(int threadId, Action action)
		{
			Argument.NotNull(action, nameof(action));
			if (threadId == Thread.CurrentThread.ManagedThreadId)
			{
				action();

				return;
			}

			var dispatcher = RegisterDispatcher.GetDispatcher(threadId);
			if (dispatcher == null)
			{
				return;
			}

			dispatcher.BeginInvoke(action);
		}

		public static void ActionByAllPossibleThreads(Action action)
		{
			Argument.NotNull(action, nameof(action));
			foreach (var threadId in RegisterDispatcher.RegisteredThreadIds)
			{
				ActionByThread(threadId, action);
			}
		}
	}
}
