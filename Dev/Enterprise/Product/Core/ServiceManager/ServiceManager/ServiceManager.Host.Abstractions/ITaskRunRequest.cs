using System;

namespace ServiceManager.Host.Abstractions
{
	public interface ITaskRunRequest : IEquatable<ITaskRunRequest>
	{
		Guid Id { get; }
		IRunnableServiceTask Task { get; }

		void OnSuccessfulRunAttempt();
		void OnUnableToRun(UnableToRunReason failureReason, bool retry, bool failedPostScheduleUpdate);
		void OnSuccessfulRun();
		void OnQueuedResponse(IServiceRunner serviceRunner);

		string FormatRequestToLogMessage(LogMessageStage logMessageStage, params object[] values);
	}

	public interface IDirectTaskRunRequest : ITaskRunRequest
	{
		bool HasRunsRemaining { get; }
		TimeSpan NextRunDelay { get; }
		bool HasMoreRetriesRemained(IDirectTaskRunRequest directTaskRunRequest);
	}

	public interface IScheduledTaskRunRequest : ITaskRunRequest
	{
		DateTimeOffset? ExpectedNextRunTime { get; }
		DateTimeOffset? NextRunTime { get; }

		void TakeNextRunTimeFromTask();
	}
}
