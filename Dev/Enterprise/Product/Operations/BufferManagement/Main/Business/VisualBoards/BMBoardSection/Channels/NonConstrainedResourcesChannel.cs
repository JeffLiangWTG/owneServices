using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public class NonConstrainedResourcesChannel : UnchanneledChannel
	{
		internal NonConstrainedResourcesChannel(IEnumerable<GlbStaff> constrainedResources)
			: this(constrainedResources.Select(r => r.GS_Code))
		{
		}

		NonConstrainedResourcesChannel(IEnumerable<ZString> constrainedResources)
			: base(ChannelTypeList.Codes.Resource)
		{
			this.constrainedResources = constrainedResources.ToArray();
		}

		readonly ZString[] constrainedResources;

		protected override string DisplayName => BMConstants.NonConstrainedResourcesChannelDisplayName;

		protected override bool IsInResourceChannel(ProcessTask task, bool showJobWorkflow)
		{
			var workflowFromTask = task.GetProcessHeader();

			if (showJobWorkflow)
			{
				var jobWorkflow = workflowFromTask.JobHeader;
				return NoConstrainedResourceInWorkflow(jobWorkflow) &&
					(workflowFromTask.IsReleased || jobWorkflow.ProcessHeaders.All(h => !h.IsReleased));
			}
			else
			{
				return NoConstrainedResourceInWorkflow(workflowFromTask);
			}
		}

		bool NoConstrainedResourceInWorkflow(ProcessHeader workflow)
		{
			return constrainedResources.All(code => !workflow.GetTasksWithoutAccessingWorkflowParent().Any(t => string.Equals(t.P9_GS_NKAssignedStaffMember, code, StringComparison.CurrentCultureIgnoreCase)));
		}

		public override IVisualBoardChannel CreateCopy(BusinessObjectFactory factory)
		{
			return new NonConstrainedResourcesChannel(constrainedResources);
		}
	}
}
