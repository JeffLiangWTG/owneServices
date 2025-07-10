using CargoWise.Types;

namespace Enterprise.Integration.ServiceManager
{
	public interface IStmScheduleTask
	{
		ZGuid S5_GB { get; set; }
		ZString S5_ParentTableCode { get; set; }
		ZString S5_ScheduleDescription { get; set; }
		ZString S5_ScheduleType { get; set; }
		ZBool S5_IsActive { get; set; }
		ZDateTime S5_NextScheduledPrintRunTimeUtc { get; set; }
	}
}
