using System;
using System.Collections.Immutable;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Business
{
	public class BMComponentLinkHelper : IBMComponentLinkHelper
	{
		public ImmutableHashSet<string> GetFiltersAllowedToSkip()
		{
			return ImmutableHashSet.Create(StringComparer.Ordinal, new[]
			{
					FilterDescriptions.ActiveStatus,
					ProcessHeader.ModuleFilterConstants.AutoAssignTasks,
					ProcessHeader.ModuleFilterConstants.CompletionStatement,
					FilterDescriptions.CreatedOnWeb,
					ProcessHeader.ModuleFilterConstants.CriticalHandover,
					ProcessHeader.ModuleFilterConstants.EarliestStartDate,
					ProcessHeader.ModuleFilterConstants.JobOrWorkflow,
					ProcessHeader.ModuleFilterConstants.LastTransferType,
					ProcessHeader.ModuleFilterConstants.PrerequisiteStatus,
					ProcessHeader.ModuleFilterConstants.ReleaseGroup,
					ProcessHeader.ModuleFilterConstants.StandbyTask,
					ProcessHeader.ModuleFilterConstants.TagDefinitionCode,
					ProcessHeader.ModuleFilterConstants.TagMagnitude,
					ProcessHeader.ModuleFilterConstants.TaskAssigned,
					ProcessHeader.ModuleFilterConstants.TaskOpenEstimateRange,
					ProcessHeader.ModuleFilterConstants.Template,
					ProcessHeader.ModuleFilterConstants.WorkflowCategory,
					ProcessHeader.ModuleFilterConstants.WorkflowStatus,
					ProcessHeader.ModuleFilterConstants.WorkflowType,
			});
		}
	}
}
