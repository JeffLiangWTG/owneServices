using System;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	abstract class TaskRunRequest : ITaskRunRequest
	{
		protected TaskRunRequest(IRunnableServiceTask task, IStopwatch stepDurationStopwatch, IStopwatch totalDurationStopwatch)
		{
			Task = task;
			Id = Guid.NewGuid();
			previousLogStage = null;
			stepDurationWatcher = stepDurationStopwatch ?? throw new ArgumentNullException(nameof(stepDurationStopwatch));
			totalDurationWatcher = totalDurationStopwatch ?? throw new ArgumentNullException(nameof(totalDurationStopwatch));
			totalDurationWatcher.Start();
		}

		public Guid Id { get; }
		protected int TimesQueued { get; set; }
		public IRunnableServiceTask Task { get; }

		public virtual void OnSuccessfulRun()
		{
			Task.OnSuccessfulRun(this);
		}

		public virtual void OnSuccessfulRunAttempt()
		{
			Task.OnSuccessfulRunAttempt(this);
		}

		public void OnUnableToRun(UnableToRunReason failureReason, bool retry, bool failedPostScheduleUpdate)
		{
			Task.HandleUnableToRun(failureReason, this, retry, failedPostScheduleUpdate);
		}

		public void OnQueuedResponse(IServiceRunner serviceRunner)
		{
			Task.OnQueuedResponse(serviceRunner, this);
		}

		public override int GetHashCode() => new { Task, GetType().FullName }.GetHashCode();
		public virtual bool Equals(ITaskRunRequest other) => other != null && Task == other.Task;
		public override bool Equals(object obj) => obj is ITaskRunRequest trr && Equals(trr);
		protected abstract string RequestTypeForMessage { get; }

		readonly IStopwatch totalDurationWatcher;
		readonly IStopwatch stepDurationWatcher;
		LogMessageStage? previousLogStage;

		public string FormatRequestToLogMessage(LogMessageStage logMessageStage, params object[] values)
		{
			var stepDuration = stepDurationWatcher.Elapsed;
			if (previousLogStage != logMessageStage)
			{
				stepDurationWatcher.Restart();
				previousLogStage = logMessageStage;
			}

			return PrintLogMessage(logMessageStage, stepDuration, values);
		}

		protected virtual string PrintLogMessage(LogMessageStage logMessageStage, TimeSpan stepDuration, params object[] values)
		{
			switch (logMessageStage)
			{
				case LogMessageStage.RequestIsCreated:
					return $"[{Task.Code}/{Id}] {RequestTypeForMessage} is created.";
				case LogMessageStage.DelayedRequestIsCreated:
					return $"[{Task.Code}/{Id}] Delayed {RequestTypeForMessage} is created.";
				case LogMessageStage.EnqueuedRequest:
					return $"[{Task.Code}/{Id}] {RequestTypeForMessage} is enqueued {++TimesQueued} time.";
				case LogMessageStage.AbsorbedByAnotherRequest:
					return $"[{Task.Code}/{Id}] {RequestTypeForMessage} is absorbed by existing request [{GetParam<TaskRunRequest>(0, logMessageStage, values).Id}].";
				case LogMessageStage.ReplacesRequest:
					return $"[{Task.Code}/{Id}] {RequestTypeForMessage} replaces existing request [{GetParam<TaskRunRequest>(0, logMessageStage, values).Id}].";
				case LogMessageStage.DequeuedRequest:
					return $"[{Task.Code}/{Id}] {RequestTypeForMessage} is dequeued. [Time in the queue {stepDuration:dd\\:hh\\:mm\\:ss\\.fff}]";
				case LogMessageStage.RequestIsOverDue:
					return $"[{Task.Code}/{Id}] Rescheduling {RequestTypeForMessage} because it was scheduled to run over {Task.OverdueDuration} ago.";
				case LogMessageStage.ReprocessRequest:
					return $"[{Task.Code}/{Id}] Rescheduling {RequestTypeForMessage} after an attempt failed due to reason: {GetParam<UnableToRunReason>(0, logMessageStage, values).LogInfo().Message}. [Step Duration: {stepDuration:dd\\:hh\\:mm\\:ss\\.fff}]";
				case LogMessageStage.IgnoreRequest:
					return $"[{Task.Code}/{Id}] Ignoring {RequestTypeForMessage} after an attempt failed due to reason: {GetParam<UnableToRunReason>(0, logMessageStage, values).LogInfo().Message}. [Step Duration: {stepDuration:dd\\:hh\\:mm\\:ss\\.fff}]";
				case LogMessageStage.RequestIsSentToRunner:
					return $"[{Task.Code}/{Id}] {RequestTypeForMessage} is sent to Runner with PID={GetParam<IServiceRunner>(0, logMessageStage, values).ProcessId}. [Total Processing Duration: {totalDurationWatcher.Elapsed:dd\\:hh\\:mm\\:ss\\.fff}]";
				case LogMessageStage.TypeNameIsEmpty:
					return $"[{Task.Code}/{Id}] Unable to run task as the TypeName is empty.";
				case LogMessageStage.PoolKeyError:
					return $"[{Task.Code}/{Id}] Unable to run task, please specify the Branch in the task settings.";
				case LogMessageStage.ObtainingRunner:
					return $"[{Task.Code}/{Id}] Obtaining Runner.";
				case LogMessageStage.RunnerIsFound:
					return $"[{Task.Code}/{Id}] Idle Runner found in Runner Pool with Info: {GetParam<IServiceRunner>(0, logMessageStage, values)}. [Step Duration: {stepDuration:dd\\:hh\\:mm\\:ss\\.fff}]";
				case LogMessageStage.RunnerIsCreated:
					return $"[{Task.Code}/{Id}] New Runner is created with Info: {GetParam<IServiceRunner>(0, logMessageStage, values)}. [Step Duration: {stepDuration:dd\\:hh\\:mm\\:ss\\.fff}]";
				case LogMessageStage.StoppingAccociatedRunners:
					return $"[{Task.Code}/{Id}] Stopping all associated Runners due to inactive task.";
				case LogMessageStage.ValidatingRequest:
					return $"[{Task.Code}/{Id}] Validating {RequestTypeForMessage}.";
				case LogMessageStage.ValidatedRequest:
					return $"[{Task.Code}/{Id}] {RequestTypeForMessage} is validated.";
				case LogMessageStage.RequestIsQueuedByRunner:
					return $"[{Task.Code}/{Id}] Received queued response from Runner with PID={GetParam<IServiceRunner>(0, logMessageStage, values).ProcessId}. [Step Duration: {stepDuration:dd\\:hh\\:mm\\:ss\\.fff}]";
				case LogMessageStage.NoRunsAttemptsRemaining:
					return $"[{Task.Code}/{Id}] {RequestTypeForMessage} has no attempts remaining, queued {TimesQueued} times.";
				default:
					throw new ArgumentOutOfRangeException(nameof(logMessageStage));
			}
		}

		protected static T GetParam<T>(int paramNumber, LogMessageStage logMessageStage, params object[] values)
		{
			if (values.Length <= paramNumber)
			{
				throw new ArgumentOutOfRangeException($"please provide parameter number {paramNumber} of {typeof(T)} for {logMessageStage}");
			}
			return (T)values[paramNumber];
		}
	}
}
