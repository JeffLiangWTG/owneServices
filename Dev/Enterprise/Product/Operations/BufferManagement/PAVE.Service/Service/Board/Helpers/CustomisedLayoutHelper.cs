using System.Collections.Generic;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.Service
{
	internal static class CustomisedLayoutHelper
	{
		internal static BMControlCustomisation GetLayoutForRelatedJobType(Dictionary<string, BMControlCustomisation> layouts, ProcessHeader workflow)
		{
			var jobType = ((IWorkflow)workflow.JobHeader).WorkflowType;
			return GetLayoutForRelatedJobType(layouts, jobType, jobType)
				?? GetLayoutForRelatedJobType(layouts, string.Empty, jobType);
		}

		internal static BMControlCustomisation GetLayoutForRelatedJobType(Dictionary<string, BMControlCustomisation> layouts, string requiredLinkJobType, string requiredLayoutJobType)
		{
			if (layouts == null)
			{
				return null;
			}

			layouts.TryGetValue(requiredLinkJobType, out var layout);

			if (layout != null && layout.FM_JobType != requiredLayoutJobType && layout.FM_JobType != string.Empty)
			{
				return null;
			}
			return layout;
		}
	}
}
