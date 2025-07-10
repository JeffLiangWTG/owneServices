using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalCopy.Business
{
	public class ScheduleDeactivator
	{
		public ScheduleDeactivator()
		{
		}

		IEnumerable<ZGuid> GetPKsForWorkflow(IProcessHeader workflow)
		{
			return workflow.IsWorkflow ?
				workflow.GetChildWorkflowsDownTheHierarchy().Where(x => x.FH_ParentId == workflow.FH_ParentId).Select(x => x.PK).Concat(new[] { workflow.PK }) :
				(workflow as IProcessJobHeader).ProcessHeaders.ToArray().Select(x => x.PK).Concat(new[] { workflow.PK });
		}

		internal IEnumerable<StmUniversalCopyScheduleTask> GetRelatedCopySchedules(BusinessObject bo)
		{
			ZQuery query = null;

			if (bo is IProcessHeader workflow)
			{
				query = GetRelatedCopySchedulesQuery(workflow);
			}
			else if (bo is ProcessTask task)
			{
				query = GetRelatedCopySchedulesQuery(task);
			}
			else
			{
				query = GetRelatedCopySchedulesQuery(bo);
			}

			return bo.Factory.Load<StmUniversalCopy>(query).Select(item => item.ScheduleTask).Where(scheduleTask => scheduleTask != null && scheduleTask.S5_IsActive);
		}

		ZQuery GetRelatedCopySchedulesQuery(IProcessHeader processHeader)
		{
			var query = new ZDBOnlyQuery(typeof(StmUniversalCopy));
			var workflowPKs = GetPKsForWorkflow(processHeader).ToArray();
			query.AddToFilter(StmUniversalCopySchema.SUC_CopyObjectId, workflowPKs);

			var unionQuery = new ZDBOnlyQuery(typeof(StmUniversalCopy));
			var processTaskSubQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.PK);
			processTaskSubQuery.AddToFilter(ProcessTasksSchema.P9_FH_ProcessHeader, workflowPKs);
			unionQuery.AddSubQuery(StmUniversalCopySchema.SUC_CopyObjectId, processTaskSubQuery, JoinCondition.And);

			query.AddToFilter(unionQuery, JoinCondition.Union);

			return query;
		}

		ZQuery GetRelatedCopySchedulesQuery(ProcessTask processTask)
		{
			var query = new ZDBOnlyQuery(typeof(StmUniversalCopy));
			query.AddToFilter(StmUniversalCopySchema.SUC_CopyObjectId, processTask.PK);
			return query;
		}

		ZQuery GetRelatedCopySchedulesQuery(BusinessObject job)
		{
			var query = new ZDBOnlyQuery(typeof(StmUniversalCopy));
			var processTaskSubQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.PK);
			query.AddToFilter(StmUniversalCopySchema.SUC_CopyObjectId, job.PK);

			var processHeaderSubQuery = new ZDBOnlySubQuery(typeof(IProcessHeader), ProcessHeaderSchema.PK);
			processHeaderSubQuery.AddToFilter(ProcessHeaderSchema.FH_ParentId, job.PK);

			processTaskSubQuery.AddToFilter(ProcessTasksSchema.P9_ParentID, job.PK);

			query.AddSubQuery(StmUniversalCopySchema.SUC_CopyObjectId, processHeaderSubQuery, JoinCondition.Union);
			query.AddSubQuery(StmUniversalCopySchema.SUC_CopyObjectId, processTaskSubQuery, JoinCondition.Union);

			return query;
		}

		public bool ConfirmCancellationAndMaybeDeactivateCopySchedules(BusinessObject bo)
		{
			var schedules = GetRelatedCopySchedules(bo);
			if (schedules.Any())
			{
				if (!Globals.IsUserInteractive)
				{
					if (RawDataRegistry.Instance.DefaultRelatedCopyScheduleDeactivationBehavior.Value == RawDataRegistry.DefaultRelatedCopyScheduleDeactivationBehaviorCodes.DeactivateAllCopySchedules)
					{
						DeactivateSchedules(schedules);
					}

					return true;
				}

				var viewModel = new ScheduleDeactivatorViewModel(schedules);
				var view = ObjectFactory.Get<IScheduleDeactivatorView>(nameof(IScheduleDeactivatorView), viewModel);
				var response = view.GetResponseFromUser();
				var shouldProceedToCancel = response != ScheduleDeactivatorResponse.DoNotCancelOrDeactivate;

				if (response == ScheduleDeactivatorResponse.CancelAndDeactivate)
				{
					DeactivateSchedules(schedules);
				}

				return shouldProceedToCancel;
			}

			return true;
		}

		public void DeactivateSchedules(IEnumerable<StmUniversalCopyScheduleTask> schedules)
		{
			schedules.ForEach(schedule => schedule.S5_IsActive = false);
		}
	}
}
