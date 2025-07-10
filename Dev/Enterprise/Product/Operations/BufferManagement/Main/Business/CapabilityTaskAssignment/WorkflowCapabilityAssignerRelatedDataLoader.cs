using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.CalendarArithmetic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class WorkflowCapabilityAssignerRelatedDataLoader : IWorkflowCapabilityAssignerRelatedDataLoader
	{
		BusinessObjectFactory relatedDataFactory = new ReadOnlyBusinessObjectFactory { NameForDebugging = nameof(WorkflowCapabilityAssignerRelatedDataLoader) };

		#region IWorkflowCapabilityAssignerRelatedDataLoader Members

		public IEnumerable<BMComponentReleaseGroupLink> GetComponentReleaseGroupLinks(ZGuid? bufferComponentPk)
		{
			return relatedDataFactory.Load<BMComponentReleaseGroupLink>(new ZQuery(BMComponentReleaseGroupLinkSchema.FO_FC_Component, bufferComponentPk));
		}

		public IEnumerable<ProcessTask> GetWorkflowTasksForAutoAssignment(ProcessHeader workflow)
		{
			var tasks = workflow.GetTasksWithoutAccessingWorkflowParent().ToArray();

			foreach (var task in tasks)
			{
				relatedDataFactory.AddFetchHint(GlbCapabilitySchema.PK, task.P9_G4_RequiredCapability);
			}

			return tasks;
		}

		public DateTime GetWorkflowReleaseLocalTime(WorkingTimeContext workingTimeContext, ProcessHeader workflow)
		{
			return workingTimeContext.ToLocalTime(workflow.FH_ReleaseDateTime, relatedDataFactory).ToDateTime();
		}

		public DateTime GetCurrentLocalTime(WorkingTimeContext workingTimeContext)
		{
			return workingTimeContext.GetCurrentLocalTime(relatedDataFactory).ToDateTime();
		}

		public IWorkTimeArithmetic GetWorkTimeArithmetic(WorkingTimeContext workingTimeContext)
		{
			return workingTimeContext.GetWorkTimeArithmetic(relatedDataFactory);
		}

		public IEnumerable<GlbStaff> GetWorkingResourcesWithCapability(ZGuid capabilityPk, WorkingTimeContext workingTimeContext, ZGuid releaseGroupPk)
		{
			return WorkingResourcesCalculator.GetWorkingResourcesWithCapability(capabilityPk, relatedDataFactory, workingTimeContext, releaseGroupPk);
		}

		public bool DifRestrictionsExistInRegistry() => TaskAssignmentHelper.DifRestrictionsExistInRegistry(relatedDataFactory);

		#endregion

		#region Disposable

		bool disposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					//We call garbage collector here for the reason - 'high memory usage' error happened on the service task run when 
					//memory consumption exceeds the value set in the registry.
					//it can happens on the large databases with the big amount of data.
					GCWrapper.ReclaimMemory(ref relatedDataFactory);
				}

				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			System.GC.SuppressFinalize(this);
		}

		#endregion
	}
}
