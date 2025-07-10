using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface IAssignedWorkflowItem : IWorkflowItem
	{
		ZString AssignedStaffCode { get; }
		ZGuid AssignedGroupPK { get; }
	}
}
