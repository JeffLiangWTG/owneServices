using System;
using System.Threading;

namespace Enterprise.ServiceManager.Host
{
	public interface IActionQueue
	{
		void Enqueue(Action action);
		void Enqueue(TimeSpan delaySpan, Action action);
	}

	public interface IBackgroundThreadActionQueue : IActionQueue, IDisposable
	{
		void SetMainThreadId();
		bool WaitForEnqueue(TimeSpan timeout, CancellationToken cancellationToken);
		void Wake();
		void InvokeActions();
		void InvokeActionsWhileWaiting(TimeSpan upToTimeout, Func<bool> exitCondition);

		IDisposable CreateTimer(Action timerAction, TimeSpan dueTime, TimeSpan period);
	}
}
