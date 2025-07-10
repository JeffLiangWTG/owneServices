using System.Linq;
using CargoWise.Common;

namespace Enterprise.BufferManagement.Business
{
	class AddWorkflowWhenJobAlreadyInAnotherQueueRule : IQueueMembershipValidationRule
	{
		bool IQueueMembershipValidationRule.CanAdd(WorkQueue queue, ProcessHeader processHeader, out string message)
		{
			if (processHeader.IsWorkflow)
			{
				var jobHeader = processHeader.JobHeader;
				if (jobHeader != null)
				{
					var otherQueue = jobHeader.TagLinks.Cast<TagLink>().Select(l => l.Magnitude).OfType<WorkQueue>().WhereNotNull().SingleOrDefault();
					if (otherQueue != null && otherQueue.ContainsMember(jobHeader))
					{
						message = Res.GetString("06085038-d4f8-4ce8-b59d-0937b8732cd0", "Cannot add a workflow of a job already in another queue ({0}).", otherQueue.DisplayText);
						return false;
					}
				}
			}

			message = null;
			return true;
		}
	}
}
