using Microsoft.Extensions.Logging;

namespace ServiceManager.Host.Abstractions
{
	public enum UnableToRunReason
	{
		[LogInfo(LogLevel.Warning, "The lock to update next run time could not be obtained")]
		LockToUpdateNextRunTimeFailed,

		[LogInfo(LogLevel.Debug, "Another host was already scheduling this task")]
		OtherHostIsSchedulingTheTask,

		[LogInfo(LogLevel.Warning, "Configuration error on the task")]
		ConfigurationError,

		[LogInfo(LogLevel.Information, "The task is inactive")]
		TaskIsInactive,

		[LogInfo(LogLevel.Warning, "The runner process did not start")]
		RunnerProcessDidNotStart,

		[LogInfo(LogLevel.Information, "The runner process exited")]
		RunnerProcessExited,

		[LogInfo(LogLevel.Information, "Service task lock for single instance service task could not be obtained")]
		RunnerCouldNotObtainLockForSingleInstanceTaskDueToServiceTaskLock,

		[LogInfo(LogLevel.Information, "Mutual group lock for service task could not be obtained")]
		RunnerCouldNotObtainLockForSingleInstanceTaskDueToMutualGroupLock,

		[LogInfo(LogLevel.Information, "Runner was cancelled")]
		RunnerWasCancelled,

		[LogInfo(LogLevel.Warning, "The runner did not notify the host it had completed processing before exit")]
		HostDidNotReceiveRunnerProcessingFinishedCallback,

		[LogInfo(LogLevel.Debug, "The runner is in the process of shutting down")]
		RunnerIsInProcessOfShuttingDown,

		[LogInfo(LogLevel.Debug, "The task has already completed its run")]
		TaskAlreadyCompleted,
	}
}
