
namespace Enterprise.BufferManagement.Business
{
	class AddWorkflowWhenJobAlreadyInQueueRule : IQueueMembershipValidationRule
	{
		bool IQueueMembershipValidationRule.CanAdd(WorkQueue queue, ProcessHeader processHeader, out string message)
		{
			if (processHeader.IsWorkflow)
			{
				var jobHeader = processHeader.JobHeader;
				if (jobHeader != null)
				{
					if (queue.ContainsMember(jobHeader))
					{
						message = Res.GetString("3db2864c-3ca9-4557-a211-f5340457accb", "Cannot add a workflow of a job already in the queue.");
						return false;
					}
				}
			}

			message = null;
			return true;
		}
	}
}
