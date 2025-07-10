namespace Enterprise.BufferManagement.Business
{
	interface IQueueMembershipValidationRule
	{
		bool CanAdd(WorkQueue queue, ProcessHeader processHeader, out string message);
	}
}
