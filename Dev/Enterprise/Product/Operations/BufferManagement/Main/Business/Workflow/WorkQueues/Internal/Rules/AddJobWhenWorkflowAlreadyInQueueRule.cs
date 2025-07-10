using System.Linq;

namespace Enterprise.BufferManagement.Business
{
	class AddJobWhenWorkflowAlreadyInQueueRule : IQueueMembershipValidationRule
	{
		bool IQueueMembershipValidationRule.CanAdd(WorkQueue queue, ProcessHeader processHeader, out string message)
		{
			var jobHeader = processHeader as ProcessJobHeader;
			if (jobHeader != null)
			{
				var workflowsInQueue = jobHeader.ProcessHeaders.Where(w => queue.ContainsMember(w)).ToArray();
				if (workflowsInQueue.Length > 0)
				{
					message = Res.GetString("869352b6-5f9e-412c-a76f-93d481843808", "Cannot add the job of a workflow already in the queue. The following workflows are already present in this queue:");
					message += System.Environment.NewLine + "\t" + string.Join(System.Environment.NewLine + "\t", workflowsInQueue.Select(w => w.FH_CompletionStatement));

					return false;
				}
			}

			message = null;
			return true;
		}
	}
}
