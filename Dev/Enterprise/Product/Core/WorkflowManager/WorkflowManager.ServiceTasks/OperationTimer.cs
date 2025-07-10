using System;
using System.Diagnostics;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	interface IStopwatch
	{
		void Start();
		void Stop();
		long ElapsedMilliseconds { get; }
	}

	class StopwatchWrapper : IStopwatch
	{
		readonly Stopwatch stopwatch;

		public StopwatchWrapper()
		{
			stopwatch = new Stopwatch();
		}

		public void Start() => stopwatch.Start();
		public void Stop() => stopwatch.Stop();
		public long ElapsedMilliseconds => stopwatch.ElapsedMilliseconds;
	}

	public class OperationTimer : IDisposable
	{
		readonly IStopwatch stopwatch;
		readonly Action<long> onLimitExceededAction;
		readonly long timeLimitInMilliseconds;

		public OperationTimer(long timeLimitInMilliseconds, Action<long> onLimitExceededAction)
			: this(timeLimitInMilliseconds, onLimitExceededAction, new StopwatchWrapper())
		{
		}

		internal OperationTimer(long timeLimitInMilliseconds, Action<long> onLimitExceededAction, IStopwatch stopwatch)
		{
			this.stopwatch = stopwatch;
			this.stopwatch.Start();
			this.onLimitExceededAction = onLimitExceededAction;
			this.timeLimitInMilliseconds = timeLimitInMilliseconds;
		}

		void IDisposable.Dispose()
		{
			stopwatch.Stop();
			if (stopwatch.ElapsedMilliseconds > timeLimitInMilliseconds)
			{
				onLimitExceededAction(stopwatch.ElapsedMilliseconds);
			}
		}
	}
}
