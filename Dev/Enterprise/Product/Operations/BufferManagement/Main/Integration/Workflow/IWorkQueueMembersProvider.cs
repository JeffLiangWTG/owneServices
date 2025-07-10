using CargoWise.Types;

namespace Enterprise.BufferManagement.Integration
{
	public interface IWorkQueueMembersProvider
	{
		ZString JobCreatedBy { get; }
		ZDateTime JobCreatedDate { get; }
		ZString JobCriteria1 { get; }
		ZString JobCriteria2 { get; }
		ZString JobCriteria3 { get; }
		ZString JobCriteria4 { get; }
		ZString JobCriteria5 { get; }
	}
}
