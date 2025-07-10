using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class WorkflowUpdatedOperation : IBoardRefreshContext
	{
		public WorkflowUpdatedOperation(ZGuid[] taskPks, ZGuid[] workflowPks, BusinessObjectFactory factory)
			: this(taskPks, workflowPks, System.Array.Empty<CellContent>(), factory)
		{
		}

		public WorkflowUpdatedOperation(ZGuid[] taskPks, ZGuid[] workflowPks, CellContent[] cells, BusinessObjectFactory factory)
		{
			TaskPKs = taskPks;
			WorkflowPKs = workflowPks;

			MainThreadFactory = factory;
			Cells = cells;
		}

		public BusinessObjectFactory MainThreadFactory { get; }
		public ICollection<ZGuid> TaskPKs { get; }
		public ICollection<ZGuid> WorkflowPKs { get; }
		public ICollection<CellContent> Cells { get; }

		internal bool IsSavingDetailedTicket { get; set; }

		internal ZGuid[] GetRelatedEntityPKs(BMBoardSectionViewModel viewModel, BusinessObjectFactory factory)
		{
			if (viewModel.ShowWorkflowCards)
			{
				return GetRelatedWorkflowPKs(factory);
			}
			if (viewModel.ShowJobCards)
			{
				var jobHeader = GetRelatedJobWorkflow(factory);

				return jobHeader != null ? new[] { jobHeader.PK } : System.Array.Empty<ZGuid>();
			}
			else
			{
				return GetRelatedTaskPKs(factory);
			}
		}

		internal ZGuid[] GetRelatedWorkflowPKs(BusinessObjectFactory factory)
		{
			return GetRelatedWorkflows(factory).Select(s => s.PK).Concat(WorkflowPKs).Distinct().ToArray();
		}

		internal ZGuid[] GetRelatedTaskPKs(BusinessObjectFactory factory)
		{
			return GetRelatedTasks(factory).Select(s => s.PK).Concat(TaskPKs).Distinct().ToArray();
		}

		ProcessHeader GetRelatedJobWorkflow(BusinessObjectFactory factory)
		{
			var workflowPK = ZGuid.Empty;
			if (WorkflowPKs.Any())
			{
				workflowPK = WorkflowPKs.FirstOrDefault();
			}
			else
			{
				var relatedWorkflowPks = GetRelatedWorkflowPKs(factory);
				if (relatedWorkflowPks.Any())
				{
					workflowPK = GetRelatedWorkflowPKs(factory).FirstOrDefault();
				}
			}

			if (!workflowPK.IsEmpty)
			{
				var workflow = factory.Load<ProcessHeader>(workflowPK);
				if (workflow != null)
				{
					return !workflow.IsWorkflow ? workflow : workflow.ParentHeader;
				}
			}
			return null;
		}

		internal ProcessHeader[] GetRelatedWorkflows(BusinessObjectFactory factory)
		{
			return factory.Load<ProcessHeader>(GetRelatedWorkflowQuery(TaskPKs, WorkflowPKs));
		}

		internal ProcessTask[] GetRelatedTasks(BusinessObjectFactory factory)
		{
			return factory.Load<ProcessTask>(GetRelatedTasksQuery(TaskPKs, WorkflowPKs));
		}

		static ZQuery GetRelatedWorkflowQuery(IEnumerable<ZGuid> taskPks, IEnumerable<ZGuid> workflowPks)
		{
			var hasTasks = taskPks.Any();
			var hasWorkflows = workflowPks.Any();

			if (hasTasks || hasWorkflows)
			{
				var query = new ZDBOnlyQuery(typeof(ProcessHeader));

				var workflowQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
				workflowQuery.AddToFilter(ProcessHeaderSchema.PK, workflowPks);

				var taskQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
				var taskSubQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_FH_ProcessHeader);
				taskSubQuery.AddToFilter(ProcessTasksSchema.PK, taskPks);
				taskQuery.AddSubQuery(taskSubQuery, JoinCondition.And);

				if (!hasWorkflows)
				{
					query.AddSubQuery(taskQuery, JoinCondition.And);
				}
				else if (!hasTasks)
				{
					query.AddSubQuery(workflowQuery, JoinCondition.And);
				}
				else
				{
					taskQuery.AddAsUnionQuery(workflowQuery, true);
					query.AddSubQuery(taskQuery, JoinCondition.And);
				}

				return query;
			}
			else
			{
				return ZQuery.NoResultQuery;
			}
		}

		static ZQuery GetRelatedTasksQuery(IEnumerable<ZGuid> taskPks, IEnumerable<ZGuid> workflowPks)
		{
			var hasTasks = taskPks.Any();
			var hasWorkflows = workflowPks.Any();

			if (hasTasks || hasWorkflows)
			{
				var query = new ZDBOnlyQuery(typeof(ProcessTask));

				var taskQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.PK);
				taskQuery.AddToFilter(ProcessTasksSchema.PK, taskPks);

				var workflowQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.PK);
				workflowQuery.AddToFilter(ProcessTasksSchema.P9_FH_ProcessHeader, workflowPks);

				if (!hasWorkflows)
				{
					query.AddSubQuery(taskQuery, JoinCondition.And);
				}
				else if (!hasTasks)
				{
					query.AddSubQuery(workflowQuery, JoinCondition.And);
				}
				else
				{
					taskQuery.AddAsUnionQuery(workflowQuery, true);
					query.AddSubQuery(taskQuery, JoinCondition.And);
				}

				return query;
			}
			else
			{
				return ZQuery.NoResultQuery;
			}
		}

		#region IBoardRefreshContext Members

		public BoardRefreshType RefreshType
		{
			get { return BoardRefreshType.Partial; }
		}

		#endregion
	}
}
