using System;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	sealed class DirectTaskRunRequest : TaskRunRequest, IDirectTaskRunRequest
	{
		uint timesRun;
		readonly uint echoes;

		public DirectTaskRunRequest(IRunnableServiceTask task, bool echoes = true) : this(task, new StopwatchProxy(), new StopwatchProxy(), echoes)
		{
		}

		internal DirectTaskRunRequest(IRunnableServiceTask task, IStopwatch stepDurationStopwatch, IStopwatch totalDurationStopwatch, bool echoes = true) : base(task, stepDurationStopwatch, totalDurationStopwatch)
		{
			this.echoes = echoes ? 1u : 0;
		}

		public bool HasRunsRemaining => timesRun <= echoes;

		public TimeSpan NextRunDelay
		{
			get
			{
#if DEBUG
				if (OverrideNextRunDelay_ForTest.HasValue)
				{
					return OverrideNextRunDelay_ForTest.Value;
				}
#endif
				return TimeSpan.FromSeconds(30);
			}
		}

#if DEBUG
		public TimeSpan? OverrideNextRunDelay_ForTest { get; set; }
#endif

		public bool HasMoreRetriesRemained(IDirectTaskRunRequest directTaskRunRequest)
		{
			var other = directTaskRunRequest as DirectTaskRunRequest;
			return other == null
					|| (int)echoes - timesRun > (int)other.echoes - other.timesRun;
		}

		protected override string RequestTypeForMessage => "Nudge run request";

		public override bool Equals(ITaskRunRequest other)
		{
			return other is DirectTaskRunRequest directOther
					&& base.Equals(other)
					&& directOther.timesRun == timesRun
					&& directOther.echoes == echoes;
		}

		public override void OnSuccessfulRun()
		{
			++timesRun;
			base.OnSuccessfulRun();
		}

		protected override string PrintLogMessage(LogMessageStage logMessageStage, TimeSpan stepDuration, params object[] values)
		{
			switch (logMessageStage)
			{
				case LogMessageStage.EnqueuedRequest:
					return $"[{Task.Code}/{Id}] {RequestTypeForMessage} is enqueued {++TimesQueued} time, echo {timesRun}/{echoes}.";
				case LogMessageStage.ReprocessRequest:
					return $"[{Task.Code}/{Id}] Requeueing {RequestTypeForMessage} after an attempt failed due to reason: {GetParam<UnableToRunReason>(0, logMessageStage, values).LogInfo().Message}. [Step Duration: {stepDuration:dd\\:hh\\:mm\\:ss\\.fff}]";
				case LogMessageStage.NoRunsAttemptsRemaining:
					return $"[{Task.Code}/{Id}] {RequestTypeForMessage} has no attempts remaining, queued {TimesQueued} times, echo {timesRun}/{echoes}.";
				default:
					return base.PrintLogMessage(logMessageStage, stepDuration, values);
			}
		}
	}
}
