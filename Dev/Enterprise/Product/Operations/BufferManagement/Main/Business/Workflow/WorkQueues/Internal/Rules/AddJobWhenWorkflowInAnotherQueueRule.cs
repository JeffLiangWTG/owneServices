using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.Business
{
	class AddJobWhenWorkflowInAnotherQueueRule : IQueueMembershipValidationRule
	{
		bool IQueueMembershipValidationRule.CanAdd(WorkQueue queue, ProcessHeader processHeader, out string message)
		{
			var jobHeader = processHeader as ProcessJobHeader;
			if (jobHeader != null)
			{
				var workflowsInQueues = GetWorkflowsAlreadyInAQueue(jobHeader).ToArray();
				if (workflowsInQueues.Length > 0)
				{
					message = Res.GetString("0c054292-c2b2-41ef-a62c-0007a1e85463", "Cannot add the job of a workflow already in a queue. The following workflows are already present in another queue:");
					message += System.Environment.NewLine + "\t" + string.Join(System.Environment.NewLine + "\t", workflowsInQueues.Select(x => GetWorkflowAndQueueLine(x)));

					return false;
				}
			}

			message = null;
			return true;
		}

		static IEnumerable<Tuple<IProcessHeader, WorkQueue>> GetWorkflowsAlreadyInAQueue(ProcessJobHeader jobHeader)
		{
			foreach (var workflow in jobHeader.ProcessHeaders)
			{
				var queue = workflow.TagLinks.Select(l => l.TagMagnitude).OfType<WorkQueue>().WhereNotNull().SingleOrDefault();
				if (queue != null)
				{
					yield return Tuple.Create(workflow, queue);
				}
			}
		}

		static string GetWorkflowAndQueueLine(Tuple<IProcessHeader, WorkQueue> tuple)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", tuple.Item1.FH_CompletionStatement, tuple.Item2.DisplayText);
		}
	}
}
