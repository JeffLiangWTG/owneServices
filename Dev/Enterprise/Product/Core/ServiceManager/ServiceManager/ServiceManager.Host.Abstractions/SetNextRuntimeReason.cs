using Microsoft.Extensions.Logging;

namespace ServiceManager.Host.Abstractions
{
	public enum SetNextRuntimeReason
	{
		[LogInfo(LogLevel.Debug, "we are loading the initial value from the database")]
		LoadingInitialValue,

		[LogInfo(LogLevel.Debug, "an update was received from a peer controller")]
		UpdateReceivedFromPeerController,

		[LogInfo(LogLevel.Debug, "the task was scheduled to run")]
		ScheduledToRun,

		[LogInfo(LogLevel.Debug, "a set request was received")]
		SetRequestReceived,

		[LogInfo(LogLevel.Information, "there was a failed run attempt")]
		FailedRunAttempt,

		[LogInfo(LogLevel.Information, "we failed to calculate the next run time")]
		FailedToCalculate,

		[LogInfo(LogLevel.Information, "a newer value was in the persisted XML file")]
		LoadingFromLocalXml,
	}
}
