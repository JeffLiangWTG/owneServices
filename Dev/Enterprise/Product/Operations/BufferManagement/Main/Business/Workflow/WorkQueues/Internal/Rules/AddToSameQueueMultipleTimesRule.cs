
namespace Enterprise.BufferManagement.Business
{
	class AddToSameQueueMultipleTimesRule : IQueueMembershipValidationRule
	{
		bool IQueueMembershipValidationRule.CanAdd(WorkQueue queue, ProcessHeader processHeader, out string message)
		{
			if (queue.ContainsMember(processHeader))
			{
				message = Res.GetString("2f459b01-2ca4-419c-bbfd-5d46e12e97eb", "Cannot add an item to same queue more than once.");
				return false;
			}
			else
			{
				message = null;
				return true;
			}
		}
	}
}
