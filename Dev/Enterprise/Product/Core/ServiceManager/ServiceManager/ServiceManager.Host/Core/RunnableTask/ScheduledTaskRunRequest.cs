using System;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	sealed class ScheduledTaskRunRequest : TaskRunRequest, IScheduledTaskRunRequest
	{
		public ScheduledTaskRunRequest(IRunnableServiceTask task) : this(task, new StopwatchProxy(), new StopwatchProxy())
		{
		}

		internal ScheduledTaskRunRequest(IRunnableServiceTask task, IStopwatch stepDurationStopwatch, IStopwatch totalDurationStopwatch) : base(task, stepDurationStopwatch, totalDurationStopwatch)
		{
			ExpectedNextRunTime = task.NextRunTime;
		}

		public override bool Equals(ITaskRunRequest other)
		{
			return other is ScheduledTaskRunRequest && base.Equals(other);
		}

		protected override string RequestTypeForMessage => "Scheduled run request";

		public DateTimeOffset? ExpectedNextRunTime { get; }

		public DateTimeOffset? NextRunTime => nextRunTime ?? throw new InvalidOperationException($"Invalid {nameof(NextRunTime)}");

		public void TakeNextRunTimeFromTask()
		{
			if (nextRunTime == null)
			{
				nextRunTime = Task.NextRunTime;
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		DateTimeOffset? nextRunTime;
	}
}
