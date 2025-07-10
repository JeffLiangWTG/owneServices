using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;
using ServiceManagerProto;

namespace Enterprise.ServiceManager.Host.Testing.Helpers
{
	public static class UnableToRunReasonHelper
	{
		public static readonly IEnumerable<(UnableToRunReason reason, LogLevel logLevel, string message)> Source = new (UnableToRunReason reason, LogLevel logType, string message)[]
		{
			(UnableToRunReason.LockToUpdateNextRunTimeFailed, LogLevel.Warning, "The lock to update next run time could not be obtained"),
			(UnableToRunReason.OtherHostIsSchedulingTheTask, LogLevel.Debug, "Another host was already scheduling this task"),
			(UnableToRunReason.ConfigurationError, LogLevel.Warning, "Configuration error on the task"),
			(UnableToRunReason.TaskIsInactive, LogLevel.Information, "The task is inactive"),
			(UnableToRunReason.RunnerProcessDidNotStart, LogLevel.Warning, "The runner process did not start"),
			(UnableToRunReason.RunnerProcessExited, LogLevel.Information, "The runner process exited"),
			(UnableToRunReason.RunnerCouldNotObtainLockForSingleInstanceTaskDueToServiceTaskLock, LogLevel.Information, "Service task lock for single instance service task could not be obtained"),
			(UnableToRunReason.RunnerCouldNotObtainLockForSingleInstanceTaskDueToMutualGroupLock, LogLevel.Information, "Mutual group lock for service task could not be obtained"),
			(UnableToRunReason.RunnerWasCancelled, LogLevel.Information, "Runner was cancelled"),
			(UnableToRunReason.HostDidNotReceiveRunnerProcessingFinishedCallback, LogLevel.Warning, "The runner did not notify the host it had completed processing before exit"),
			(UnableToRunReason.RunnerIsInProcessOfShuttingDown, LogLevel.Debug, "The runner is in the process of shutting down"),
			(UnableToRunReason.TaskAlreadyCompleted, LogLevel.Debug, "The task has already completed its run"),
		};

		public static FailureReasonType ConvertToFailureReasonType(UnableToRunReason? unableToRunReason)
		{
			switch (unableToRunReason)
			{
				case UnableToRunReason.RunnerCouldNotObtainLockForSingleInstanceTaskDueToServiceTaskLock:
					return FailureReasonType.RunnerCouldNotObtainLockForSingleInstanceTaskDueToServiceTaskLock;
				case UnableToRunReason.RunnerCouldNotObtainLockForSingleInstanceTaskDueToMutualGroupLock:
					return FailureReasonType.RunnerCouldNotObtainLockForSingleInstanceTaskDueToMutualGroupLock;
				case UnableToRunReason.RunnerWasCancelled:
					return FailureReasonType.RunnerWasCancelled;
				default:
					throw new ArgumentOutOfRangeException($"Unable to convert {unableToRunReason} to FailureReasonType.");
			}
		}
	}
}
