using System.Linq;

namespace Enterprise.BufferManagement.Business
{
	class AddToMultipleQueuesRule : IQueueMembershipValidationRule
	{
		bool IQueueMembershipValidationRule.CanAdd(WorkQueue queue, ProcessHeader processHeader, out string message)
		{
			if (processHeader.TagLinks.Any(l => l.TagMagnitude is WorkQueue && l.TagMagnitude != queue))
			{
				message = Res.GetString("c6a33889-cb40-4a6a-8223-fa6c1f36cd39", "Cannot add an item to multiple queues.");
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
