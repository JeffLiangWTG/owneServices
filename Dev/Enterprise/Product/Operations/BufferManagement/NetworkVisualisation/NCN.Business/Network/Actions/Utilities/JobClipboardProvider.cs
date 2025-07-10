using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	static class JobClipboardProvider
	{
		public static IEnumerable<WorkflowClipboardResult> GetJobHeadersFromClipboard(this IJobNetwork network, BusinessObjectFactory factory)
		{
			var jobsInClipboard = network.Controller.GetJobsFromClipboard(factory);

			if (jobsInClipboard != null)
			{
				foreach (var jobInClipboard in jobsInClipboard)
				{
					switch (jobInClipboard)
					{
						case ProcessHeader workflow:
							yield return new WorkflowClipboardResult(workflow, workflow);
							break;

						case BMNCNShape shape:
							yield return new WorkflowClipboardResult(shape, shape);
							break;

						case IWorkflowProvider workflowProvider:
							var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(workflowProvider, factory);
							yield return new WorkflowClipboardResult(jobHeader, (BusinessObject)workflowProvider);
							break;
					}
				}
			}
		}
	}

	class WorkflowClipboardResult
	{
		public WorkflowClipboardResult(BusinessObject entityToLink, BusinessObject entityForLinkDisplayName)
		{
			EntityToLink = entityToLink;
			EntityForLinkDisplayName = entityForLinkDisplayName;
		}

		public bool IsValid => EntityToLink != null;

		public string Name
		{
			get
			{
				if (EntityForLinkDisplayName is ProcessHeader workflow)
				{
					return workflow.Name;
				}

				try
				{
					return DescriptionPropertyAttribute.DescriptionFromBusinessObject(EntityForLinkDisplayName);
				}
				catch (NoCodePropertyException)
				{
					return Res.GetString("7144cbe9-e986-42d3-a683-0221c709d34c", "Job in clipboard");
				}
			}
		}

		public BusinessObject EntityToLink { get; }
		BusinessObject EntityForLinkDisplayName { get; }
	}
}
