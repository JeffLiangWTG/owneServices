using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ServiceManager.Business;
using Microsoft.Extensions.Logging;
using ServiceManager.Common.Abstractions;
using ServiceManager.Common.CW;
using ServiceManager.Host.Abstractions;
using ServiceManager.Logging.CW;
using ServiceManager.Shared.Abstractions;
using static System.FormattableString;
using ILoggerFactory = ServiceManager.Integration.ServiceTasks.CW.ILoggerFactory;

namespace Enterprise.ServiceManager.Host
{
	public class RunnableServiceTask : IRunnableServiceTask
	{
		readonly IBackgroundThreadActionQueue actionQueue;
		readonly ITaskQueue taskQueue;
		readonly Lazy<Integration.ILogger> lazyServiceLogger;
		DateTimeOffset? lastScheduledTime;
		readonly IErrorReporterProxy errorReporterProxy;
		readonly ITransactionAdapter transactionAdapter;
		readonly IHostLogger hostLogger;
		IServiceTaskGovernor governor;
		ManualResetEvent taskRunning;
		readonly IServiceTaskScheduleStatusProvider serviceTaskScheduleStatusProvider;

		public RunnableServiceTask(
			ServiceTaskInfo info,
			IServiceTask schedule,
			IBackgroundThreadActionQueue actionQueue,
			ITaskQueue taskQueue,
			IHostLogger hostLogger,
			IErrorReporterProxy errorReporterProxy,
			IHostRegistrySettings hostRegistry,
			ITransactionAdapter transactionAdapter,
			ILoggerFactory loggerFactory,
			IServiceTaskScheduleStatusProvider serviceTaskScheduleStatusProvider)
		{
			_ = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
			this.hostRegistry = hostRegistry ?? throw new ArgumentNullException(nameof(hostRegistry));
			Info = info ?? throw new ArgumentNullException(nameof(info));
			TimeSinceLastEnqueued = new Stopwatch();
			TimeSinceLastDequeued = new Stopwatch();
			TimeSinceLastStarted = new Stopwatch();
			TimeRunning = new Stopwatch();
			this.hostLogger = hostLogger ?? throw new ArgumentNullException(nameof(hostLogger));
			this.actionQueue = actionQueue ?? throw new ArgumentNullException(nameof(actionQueue));
			this.taskQueue = taskQueue ?? throw new ArgumentNullException(nameof(taskQueue));
			this.errorReporterProxy = errorReporterProxy ?? throw new ArgumentNullException(nameof(errorReporterProxy));
			this.transactionAdapter = transactionAdapter ?? throw new ArgumentNullException(nameof(transactionAdapter));
			this.serviceTaskScheduleStatusProvider = serviceTaskScheduleStatusProvider ?? throw new ArgumentNullException(nameof(serviceTaskScheduleStatusProvider));
			Schedule = schedule;

			if (schedule is not null)
			{
				governor = schedule.AssignedGovernor;
			}

			lazyServiceLogger = new Lazy<Integration.ILogger>(() => loggerFactory.NewServiceTaskLogger(Db.ServerName, Db.DatabaseName, Info?.Code));
		}

		public string Code => Info.Code;
		public string Branch { get; private set; }
		public IServiceTaskInfo Info { get; }

		public void UpdateStatus()
		{
			var statusCopySource = governor?.UpdateStatus();
			if (statusCopySource != null && statusCopySource.NextRunTime.HasValue)
			{
				var newNextRunTime = statusCopySource.NextRunTime;
				if (NextScheduledRunTime == null || (newNextRunTime > NextScheduledRunTime))
				{
					SetNextRunTime(newNextRunTime, SetNextRuntimeReason.UpdateReceivedFromPeerController);
				}
			}
		}

		public TaskRunRequestResult ValidateForRun()
		{
			if (!IsActive)
			{
				hostLogger.Log(LogLevel.Information, Info.HostedServiceAttribute, "Skipped running task - is not active");
				return TaskRunRequestResult.Inactive;
			}

			if (IsDisabled)
			{
				hostLogger.Log(LogLevel.Information, Info.HostedServiceAttribute, "Skipped running task - is disabled");
				return TaskRunRequestResult.TaskIsDisabled;
			}

			return TaskRunRequestResult.Success;
		}

		public void UpdateSchedule(IEnumerable<IServiceTask> schedules, bool reEnableMandatory)
		{
			var scheduleToUpdate = schedules
				.FirstOrDefault(s => s.Code.Equals(Code, StringComparison.OrdinalIgnoreCase));

			if (scheduleToUpdate != null)
			{
				Schedule = scheduleToUpdate;

				if (!HasSchedule)
				{
					return;
				}

				if (governor.ResetScheduleToDefault(reEnableMandatory, hostLogger))
				{
					transactionAdapter.Commit();
				}
			}
		}

		public void ReloadConfigurationAsync()
		{
			actionQueue.Enqueue(() =>
			{
				var scheduleLocal = Schedule;
				if (scheduleLocal != null)
				{
					governor.Reload();
					Schedule = governor.GovernedTask;
				}
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public void RecordLastRunError()
		{
			Info.ErrorOnLastRun = true;
		}

		public void OnErrorReported() => serviceHostErrorTracker.Add();

		public void OnSuccessfulRun(ITaskRunRequest request)
		{
			if (request is IDirectTaskRunRequest)
			{
				hostLogger.Log(LogLevel.Debug, $"[{request.Task.Code}/{request.Id}] - Requeue Lock: Run succeeded, releasing lock");
				taskRunning?.Set();
				taskRunning = null;
			}
		}

		public void OnSuccessfulRunAttempt(ITaskRunRequest request)
		{
			if (request is IDirectTaskRunRequest directRequest)
			{
				if (taskRunning != null)
				{
					taskRunning.Set();
					hostLogger.Log(LogLevel.Debug, $"[{request.Task.Code}/{request.Id}] - Requeue Lock: Separate request has overridden current run, release lock to prevent blocking of new service task run");
				}
				hostLogger.Log(LogLevel.Debug, $"[{request.Task.Code}/{request.Id}] - Requeue Lock: Acquire lock on task to prevent unnecessary retries while task has not returned from runner");
				taskRunning = new ManualResetEvent(false);
				ReEnqueueIfTaskNotRunning(directRequest.NextRunDelay, directRequest, taskRunning, 0);
			}
			else
			{
				ClearFailedRunAttempt();
			}

			LastRunTime = ZDateTimeOffset.UtcNow.ToDateTimeOffset();

			var scheduleLocal = Schedule;
			if (scheduleLocal != null)
			{
				governor.SetLastRunTime(LastRunTime.Value);
			}

			UpdateUnderlyingScheduleNextRunTime();
		}

		public void ClearFailedRunAttempt()
		{
			FailedScheduleRetryTime = null;
		}

		public void HandleUnableToRun(UnableToRunReason unableToRunReason, ITaskRunRequest request, bool retry, bool failedPostScheduleUpdate)
		{
			Log();
			if (request is IDirectTaskRunRequest)
			{
				hostLogger.Log(LogLevel.Debug, $"[{request.Task.Code}/{request.Id}] - Requeue Lock: Run failed, releasing lock");
				taskRunning?.Set();
				taskRunning = null;
			}
			if (retry)
			{
				if (request is IDirectTaskRunRequest directRequest)
				{
					ReEnqueueDelayed(TimeSpan.FromSeconds(30), directRequest);
				}
				else
				{
					if (failedPostScheduleUpdate)
					{
						SetNextRunTime(lastScheduledTime, SetNextRuntimeReason.FailedRunAttempt);
						serviceTaskScheduleStatusProvider.SetServiceTaskNextRuntime(Code, lastScheduledTime.HasValue ? lastScheduledTime.Value.UtcDateTime : null, revertingAfterFailedRun: true);
					}
					FailedScheduleRetryTime = ZDateTimeOffset.UtcNow.ToDateTimeOffset().Add(TaskScheduler.FailedScheduleRetryTime);
				}
			}
			else
			{
				FailedScheduleRetryTime = null;
			}

			void Log()
			{
				var taskName = retry
					? LogMessageStage.ReprocessRequest
					: LogMessageStage.IgnoreRequest;
				var logInfo = unableToRunReason.LogInfo();
				hostLogger.Log(logInfo.LogLevel, request.FormatRequestToLogMessage(taskName, unableToRunReason));
			}
		}

		public void OnQueuedResponse(IServiceRunner serviceRunner, ITaskRunRequest request)
		{
			hostLogger.Log(LogLevel.Debug, request.FormatRequestToLogMessage(LogMessageStage.RequestIsQueuedByRunner, serviceRunner));
		}

		public void SetNextRunTime(DateTimeOffset? newTime, SetNextRuntimeReason operation, bool? revertingAfterFailedRun = null)
		{
			if (NextScheduledRunTime != newTime)
			{
				hostLogger.Log(operation.LogInfo().LogLevel, Info.HostedServiceAttribute, Invariant($"Setting Next Runtime from {NextScheduledRunTime} to {newTime}, as {operation.LogInfo().Message}."));
				NextScheduledRunTime = newTime;
				actionQueue.Wake();
			}

			if (revertingAfterFailedRun.HasValue && revertingAfterFailedRun.Value)
			{
				FailedScheduleRetryTime = ZDateTimeOffset.UtcNow.ToDateTimeOffset().Add(TaskScheduler.FailedScheduleRetryTime);
			}
		}

		public void SetNextRunTimeBasedOnRecurrence()
		{
			var scheduleLocal = Schedule;
			if (scheduleLocal != null)
			{
				try
				{
					lastScheduledTime = NextScheduledRunTime;
					UpdateUnderlyingScheduleNextRunTime(); // Ensure the calculation is done based off the latest configuration.
					SetNextRunTime(scheduleLocal.CalculateNextRunTime(hostLogger).UtcDateTime, SetNextRuntimeReason.ScheduledToRun);
				}
				catch (InvalidOperationException ex)
				{
					errorReporterProxy.ReportOnce(Invariant($"SetNextRunTimeBasedOnRecurrence: {ex.Message}"), ex);
					RecordLastRunError();
					hostLogger.Log(LogLevel.Error, Invariant($"Calculation of next run time failed. Reason was: {ex.Message}"));
					SetNextRunTime(ZDateTimeOffset.UtcNow.ToDateTimeOffset().AddMinutes(1), SetNextRuntimeReason.FailedToCalculate);
				}
			}
		}

		void ReEnqueueDelayed(TimeSpan delaySpan, ITaskRunRequest request)
		{
			actionQueue.Enqueue(delaySpan, () =>
			{
				EnqueueNow(request);
			});
		}

		void ReEnqueueIfTaskNotRunning(TimeSpan delaySpan, IDirectTaskRunRequest request, ManualResetEvent taskRunningLocal, int attempt)
		{
			hostLogger.Log(LogLevel.Debug, $"[{request.Task.Code}/{request.Id}] - Re-enqueue with delay, attempt [{attempt++}], request has runs remaining [{request.HasRunsRemaining}]]");
			actionQueue.Enqueue(delaySpan, () =>
			{
				if (!taskRunningLocal.WaitOne(0))
				{
					ReEnqueueIfTaskNotRunning(delaySpan, request, taskRunningLocal, attempt);
				}
				else
				{
					hostLogger.Log(LogLevel.Debug, $"[{request.Task.Code}/{request.Id}] - Completed after [{attempt++}] requeue attempts, enqueue now");
					EnqueueNow(request);
				}
			});
		}

		public void EnqueueDelayed(TimeSpan delaySpan, bool echoes = true)
		{
			hostLogger.Log(LogLevel.Debug, $"Delay {delaySpan} before creating Nudge run request for task [{Code}].");
			actionQueue.Enqueue(delaySpan, () =>
			{
				EnqueueNow(LogMessageStage.DelayedRequestIsCreated, echoes);
			});
		}

		public bool Enqueue(bool echoes = true)
		{
			var result = EnqueueNow(LogMessageStage.RequestIsCreated, echoes);
			actionQueue.Wake();
			return result;
		}

		bool EnqueueNow(LogMessageStage logMessageStage, bool echoes = true)
		{
			var outputRequest = new DirectTaskRunRequest(this, echoes);
			hostLogger.Log(LogLevel.Debug, outputRequest.FormatRequestToLogMessage(logMessageStage));
			return EnqueueNow(outputRequest);
		}

		bool EnqueueNow(ITaskRunRequest request)
		{
			return taskQueue.EnqueueTask(request);
		}

		public void UpdateUnderlyingScheduleNextRunTime()
		{
			var scheduleLocal = Schedule;
			if (scheduleLocal != null && NextScheduledRunTime != null)
			{
				governor.SetNextRunTime((DateTimeOffset)NextScheduledRunTime);
			}
		}

		public bool IsLastRunFailed => Info.ErrorOnLastRun; // Test

		IServiceTask Schedule
		{
			get
			{
				var scheduleLocal = schedule;
				if (scheduleLocal != null)
				{
					scheduleLocal.EnsureThreadSafety();
				}
				return scheduleLocal;
			}
			set
			{
				schedule = value;
				Branch = schedule?.BranchName ?? string.Empty;

				if (schedule is not null)
				{
					governor = schedule.AssignedGovernor;

					var nextRunTime = (DateTimeOffset?)(schedule.NextRunTime == DateTimeOffset.MinValue
						? null
						: schedule.NextRunTime);

					SetNextRunTime(nextRunTime, SetNextRuntimeReason.LoadingInitialValue);
				}
				else
				{
					governor = null;
					SetNextRunTime(DateTimeOffset.Now, SetNextRuntimeReason.LoadingInitialValue);
				}
			}
		}

		IServiceTask schedule;
		readonly IHostRegistrySettings hostRegistry;

		public Stopwatch TimeSinceLastEnqueued { get; set; }

		public Stopwatch TimeSinceLastDequeued { get; set; }

		public Stopwatch TimeSinceLastStarted { get; set; }
		public Stopwatch TimeRunning { get; set; }

		public int MaxSecondaryRunningCount => schedule?.SecondaryProcessesMaxCount ?? 0;

		public bool IsActive => schedule?.IsActive ?? false;

		public bool IsDisabled => RunnableServiceTaskHelper.IsTaskDisabled(Code, hostRegistry.ForcefullyDisabledTasks);

		public bool IsOverdue
		{
			get
			{
				var overdueDuration = OverdueDuration;
				if (overdueDuration == TimeSpan.Zero)
				{
					return false;
				}

				return ZDateTimeOffset.UtcNow.ToDateTimeOffset() - NextScheduledRunTime > overdueDuration;
			}
		}

		public TimeSpan OverdueDuration => schedule?.OverdueDuration ?? TimeSpan.MinValue;

		public bool NextRunTimeIsInFuture
		{
			get
			{
				return schedule == null || NextScheduledRunTime > ZDateTimeOffset.UtcNow.ToDateTimeOffset();
			}
		}

		public bool HasSchedule => schedule is not null;

		public DateTimeOffset? NextScheduledRunTime { get; private set; }

		public DateTimeOffset? LastRunTime { get; internal set; }

		public DateTimeOffset? LastErrorTime => serviceHostErrorTracker.Latest;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Not a duration")]
		public int ErrorCountLast24Hours => serviceHostErrorTracker.Count;
		public string ConfigString => schedule?.ConfigString ?? string.Empty;
		public string ScheduleDescription => schedule?.Description;
		public string TypeOfDocument => schedule?.Category;
		public TimeSpan SchedulePeriodDuration => schedule?.SchedulePeriodDuration ?? TimeSpan.MinValue;
		public DateTimeOffset? FailedScheduleRetryTime { get; private set; }
		public DateTimeOffset? NextRunTimeAllowingForLocalSchedulingFailures => FailedScheduleRetryTime == null ? NextScheduledRunTime : FailedScheduleRetryTime;

		public DateTimeOffset? NextRunTime => NextRunTimeAllowingForLocalSchedulingFailures;

		public bool HasScheduleUpdates(IServiceTask serviceTask)
		{
			_ = serviceTask ?? throw new ArgumentNullException(nameof(serviceTask));
			var scheduleLocal = Schedule;
			return scheduleLocal == null
					|| !(
						scheduleLocal.NextRunTime == serviceTask.NextRunTime
						&& scheduleLocal.IsActive == serviceTask.IsActive
						&& scheduleLocal.DailyEndTime == serviceTask.DailyEndTime
						&& scheduleLocal.DailyStartTime == serviceTask.DailyStartTime
						&& scheduleLocal.ScheduleDaysOfWeek.EqualIgnoringOrder(serviceTask.ScheduleDaysOfWeek)
						&& scheduleLocal.ScheduleDayNumber == serviceTask.ScheduleDayNumber
						&& scheduleLocal.BranchPk == serviceTask.BranchPk
						&& scheduleLocal.ScheduleMonth == serviceTask.ScheduleMonth
						&& scheduleLocal.SettingsXml == serviceTask.SettingsXml
						&& scheduleLocal.ScheduleStartDate == serviceTask.ScheduleStartDate
						&& scheduleLocal.ScheduleOccurrence.Equals(serviceTask.ScheduleOccurrence)
						&& scheduleLocal.ScheduleFrequency == serviceTask.ScheduleFrequency
						&& scheduleLocal.WeekDaysOnly == serviceTask.WeekDaysOnly
					);
		}

		public void Log(LogLevel logLevel, string message)
		{
			if (lazyServiceLogger.Value != null)
			{
				lazyServiceLogger.Value.Log(logLevel.ToLogType(), message);
			}
			else
			{
				hostLogger.Log(logLevel, message);
			}
		}

		readonly OccurrenceTracker serviceHostErrorTracker = new(TimeSpan.FromDays(1), () => ZDateTime.UtcNow.ToDateTime());
	}
}

