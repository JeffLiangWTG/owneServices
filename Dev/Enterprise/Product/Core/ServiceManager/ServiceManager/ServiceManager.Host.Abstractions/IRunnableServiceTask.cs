using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using ServiceManager.Shared.Abstractions;

namespace ServiceManager.Host.Abstractions
{
	public interface IRunnableServiceTask
	{
		bool HasSchedule { get; }

		string Code { get; }
		string Branch { get; }
		IServiceTaskInfo Info { get; }
		void UpdateStatus();
		bool IsLastRunFailed { get; }
		int MaxSecondaryRunningCount { get; }
		bool IsActive { get; }
		bool IsDisabled { get; }
		Stopwatch TimeSinceLastEnqueued { get; set; }
		Stopwatch TimeSinceLastDequeued { get; set; }
		Stopwatch TimeSinceLastStarted { get; set; }
		Stopwatch TimeRunning { get; set; }
		bool IsOverdue { get; }
		TimeSpan OverdueDuration { get; }
		bool NextRunTimeIsInFuture { get; }
		DateTimeOffset? NextScheduledRunTime { get; }
		string ConfigString { get; }
		string ScheduleDescription { get; }
		string TypeOfDocument { get; }
		TimeSpan SchedulePeriodDuration { get; }
		DateTimeOffset? NextRunTimeAllowingForLocalSchedulingFailures { get; }
		DateTimeOffset? NextRunTime { get; }
		DateTimeOffset? FailedScheduleRetryTime { get; }
		DateTimeOffset? LastRunTime { get; }
		DateTimeOffset? LastErrorTime { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Not a duration")]
		int ErrorCountLast24Hours { get; }
		TaskRunRequestResult ValidateForRun();
		void UpdateSchedule(IEnumerable<IServiceTask> schedules, bool reEnableMandatory);
		void ReloadConfigurationAsync();
		void OnSuccessfulRunAttempt(ITaskRunRequest request);
		void OnSuccessfulRun(ITaskRunRequest request);
		void HandleUnableToRun(UnableToRunReason failedRunAttemptReason, ITaskRunRequest request, bool retry, bool failedPostScheduleUpdate);
		void OnQueuedResponse(IServiceRunner serviceRunner, ITaskRunRequest request);
		void ClearFailedRunAttempt();
		void OnErrorReported();
		void RecordLastRunError();
		void SetNextRunTime(DateTimeOffset? value, SetNextRuntimeReason operation, bool? revertingAfterFailedRun = null);
		void SetNextRunTimeBasedOnRecurrence();
		bool Enqueue(bool echoes = true);
		void EnqueueDelayed(TimeSpan delaySpan, bool echoes = true);
		void UpdateUnderlyingScheduleNextRunTime();

		bool HasScheduleUpdates(IServiceTask serviceTaskSchedule);
		void Log(LogLevel logLevel, string message);
	}
}
