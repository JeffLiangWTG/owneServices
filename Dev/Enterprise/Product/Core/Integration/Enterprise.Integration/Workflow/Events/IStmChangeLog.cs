using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface IStmChangeLog : IWorkflowTriggerSource
	{
		ZString SY_Changes { get; }
		ZString SY_GS_NKUser { get; }
		ZGuid SY_ParentID { get; }
		ZString SY_ParentTableCode { get; }
		ZDateTime SY_PostedTimeUtc { get; }
		ZByte SY_RetryCount { get; }
		ZString SY_Status { get; }
	}
}