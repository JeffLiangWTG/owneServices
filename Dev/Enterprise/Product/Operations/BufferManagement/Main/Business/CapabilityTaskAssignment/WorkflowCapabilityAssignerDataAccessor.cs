using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class WorkflowCapabilityAssignerDataAccessor : ServiceTaskFactoryProviderWrapper, IWorkflowCapabilityAssignerDataAccessor
	{
		public WorkflowCapabilityAssignerDataAccessor(ILogger logger)
			: base(logger)
		{
		}

		#region Loading Workflows Belonging To Component To Auto Assign

		public IWorkflowCapabilityAssignerBatchProcessor GetWorkflowBatchProcessor(BMComponent component, string workflowBatchNameForLogging)
		{
			return new WorkflowCapabilityAssignerBatchProcessor(() => GetWorkflowsQuery(component), GetWorkflowBatchLogger(workflowBatchNameForLogging));
		}

		#region Batch Logger

		protected virtual BatchLogger GetWorkflowBatchLogger(string workflowBatchNameForLogging) => new BatchLogger(Logger, workflowBatchNameForLogging);

		#endregion

		#region Workflow Query

		ZQuery GetWorkflowsQuery(BMComponent component) => GetWorkflowsQueryCore(component);

		protected virtual ZQuery GetWorkflowsQueryCore(BMComponent component)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessHeader));
			query.AddToFilter(ProcessHeaderSchema.FH_FC_CurrentComponent, component.PK);

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@ComponentPK", component.PK, BMComponentSchema.PK);
			query.AddFilterAndZSQLParameterCollection(WorkflowsSQL, parameters);

			return query;
		}

		const string WorkflowsSQL = @"
-- WorkflowCapabilityAssignerDataAccessor
FH_PK in (
  SELECT FH_PK
  FROM dbo.ProcessHeader
  JOIN dbo.ProcessTasks ON P9_FH_ProcessHeader = FH_PK
  LEFT JOIN dbo.GlbCapability ON G4_PK = P9_G4_RequiredCapability
  LEFT JOIN dbo.BMComponentReleaseGroupLink ON 
		FO_GG_ReleaseGroup = FH_GG_ReleaseGroup
		AND FO_FC_Component = FH_FC_CurrentComponent
  LEFT JOIN dbo.BMComponent ON FC_PK = FH_FC_CurrentComponent 
	WHERE 
		FH_Status <> 'CLS'
		AND FH_FC_CurrentComponent = @ComponentPK
		AND FH_ReleaseDateTime IS NOT NULL
		AND FH_IsActive = 1 
		AND FH_P0_Template IS NULL
		AND FH_AllowTaskAutoAssignment = 1
		AND P9_FH_ProcessHeader IS NOT NULL
		AND P9_GS_NKAssignedStaffMember = ''
		AND P9_Type NOT IN ('TRG', 'MIL', 'EXC')
		AND P9_Status IN ('OPN', 'ASN', 'SUS')
		AND P9_ParentTableCode != 'P0'
		AND
		(
			G4_AutoAssignTasksAge IS NOT NULL
			OR FO_AutoAssignTasksAge IS NOT NULL
			OR FC_AutoAssignTasksAge IS NOT NULL
		)
)";

		#endregion

		#region For Testing
#if DEBUG

		public ZQuery GetWorkflowsQuery_ExposedForTest(BMComponent component) => GetWorkflowsQuery(component);

#endif
		#endregion

		#endregion

		#region Loading Workflows By PKs To Auto Assign

		public IEnumerable<ProcessHeader> LoadWorkflowsByPKs(IEnumerable<Guid> workflowPKs)
		{
			var factory = new BusinessObjectFactory() { NameForDebugging = $"{nameof(WorkflowCapabilityAssignerDataAccessor)}.{nameof(LoadWorkflowsByPKs)}" };
			var workflows = factory.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.PK, workflowPKs))
				.Where(w =>
					w.FH_IsActive
					&& w.FH_AllowTaskAutoAssignment
					&& w.CurrentComponent.FC_Type == BMComponentTypeList.Codes.Buffer)
#if DEBUG
				.OrderBy(w => w.FH_CompletionStatement)
#endif
				.ToArray();

			var taskQuery = new ZQuery(ProcessTasksSchema.P9_FH_ProcessHeader, workflows.Select(w => w.PK));
			factory.AddFetchHint(typeof(ProcessTask), taskQuery);

			return workflows;
		}

		#endregion

		#region Related Data

		public IWorkflowCapabilityAssignerRelatedDataLoader GetRelatedDataLoader()
		{
			return new WorkflowCapabilityAssignerRelatedDataLoader();
		}

		#endregion
	}
}
