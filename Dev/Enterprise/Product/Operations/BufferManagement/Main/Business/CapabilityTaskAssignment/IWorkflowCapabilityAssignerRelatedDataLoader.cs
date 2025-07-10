using System;
using System.Collections.Generic;
using CargoWise.CalendarArithmetic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public interface IWorkflowCapabilityAssignerRelatedDataLoader : IDisposable
	{
		IEnumerable<BMComponentReleaseGroupLink> GetComponentReleaseGroupLinks(ZGuid? bufferComponentPk);

		IEnumerable<ProcessTask> GetWorkflowTasksForAutoAssignment(ProcessHeader workflow);

		DateTime GetWorkflowReleaseLocalTime(WorkingTimeContext workingTimeContext, ProcessHeader workflow);

		DateTime GetCurrentLocalTime(WorkingTimeContext workingTimeContext);

		IWorkTimeArithmetic GetWorkTimeArithmetic(WorkingTimeContext workingTimeContext);

		IEnumerable<GlbStaff> GetWorkingResourcesWithCapability(ZGuid capabilityPk, WorkingTimeContext workingTimeContext, ZGuid releaseGroupPk);

		bool DifRestrictionsExistInRegistry();
	}
}
