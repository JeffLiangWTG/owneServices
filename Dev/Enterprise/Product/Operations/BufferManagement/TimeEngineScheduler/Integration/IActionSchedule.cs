using CargoWise.Types;

namespace Enterprise.TimeEngineScheduler.Integration
{
	public enum TimeActionSchedulingState
	{
		Loaded = 0,
		Scheduled = 1,
		Rescheduled = 2,
	}

	public interface IActionSchedule
	{
		ZGuid PK { get; }
		ZString ActionCode { get; set; }
		ZDateTime ExecutionDateTimeUtc { get; set; }
		ZString ExecutionResult { get; set; }
		ZString ExecutionStatus { get; set; }
		ZString JsonParameter { get; set; }
		ZByte RetryAttempts { get; set; }
		ZGuid TargetPK { get; set; }
		ZString TargetTableCode { get; set; }
		ZDateTime SystemCreateTimeUtc { get; }
		ZGuid ExecutionBranch { get; set; }
		ZGuid ExecutionDepartment { get; set; }
		ZString Token { get; set; }
		TimeActionSchedulingState NonPersistentSchedulingState { get; }
	}
}
