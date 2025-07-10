//Moved from CapacityCalculatorImpl
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	class CapacityContentProvider
	{
		internal static ICollection<WorkflowDTO> Load(BMComponent component, IEnumerable<GlbStaff> resources, ILogger logger = null)
		{
			var workflows = new Dictionary<ZGuid, WorkflowDTO>();
			var buffersBySize = new Dictionary<int, IBuffer>();
			var batchSize = BMSRegistry.Instance.CapacityCalculatorStaffBatchSize.Value;
			var activeResources = resources.Where(x => x.GS_IsActive).ToArray();
			int position = 0;

			do
			{
				var resourcesForBatch = activeResources.Skip(position).Take(batchSize).ToArray();
				ReadComponentContents(workflows, buffersBySize, component, resourcesForBatch);
				position += batchSize;
			} while (position < activeResources.Length);

			return workflows.Values;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static void ReadComponentContents(Dictionary<ZGuid, WorkflowDTO> workflows, Dictionary<int, IBuffer> buffersBySize, BMComponent component, GlbStaff[] resources)
		{
			TagRuleRunStrategyBase.DropTempTable((NoResString)"#WorkFlowDurations"); // SQL table name

			var hasRestrictedResources = resources.Any();

			var sql = GetCapacityByResourceSQL(component, hasRestrictedResources);
			var command = Db.Connection.Command(sql); // Calculating capacity in the business layer is pretty slow.
			command.AddParameterBasedOnDbColumn("@BMComponentPK", component.PK.ToGuid(), ProcessHeaderSchema.FH_FC_CurrentComponent); // SQL parameter name

			if (hasRestrictedResources)
			{
				command.AddTableValuedParameter("@StaffCodes", GlbStaffSchema.GS_Code, resources.Select(x => x.GS_Code));
				command.AddTableValuedParameter("@StaffPKs", GlbStaffSchema.PK, resources.Select(x => x.PK));
			}

			using (var result = command.ExecuteReader(CommandBehavior.SequentialAccess))
			{
				while (result.Read())
				{
					AddResult(workflows, buffersBySize, component, result);
				}
			}
		}

		static void AddResult(IDictionary<ZGuid, WorkflowDTO> workflows, IDictionary<int, IBuffer> buffersBySize, BMComponent component, IDataRecord result)
		{
			var taskPK = result.IsDBNull(0) ? Guid.Empty : result.GetGuid(0);
			var workflowPK = result.IsDBNull(1) ? Guid.Empty : result.GetGuid(1);
			var staff = result.IsDBNull(2) ? string.Empty : result.GetString(2);
			var taskEstimate = result.IsDBNull(3) ? 0.0 : result.GetDouble(3);
			var requiredCapability = result.IsDBNull(4) ? Guid.Empty : result.GetGuid(4);
			var releaseGroup = result.IsDBNull(5) ? Guid.Empty : result.GetGuid(5);
			var releaseDate = result.IsDBNull(6) ? ZDateTime.Empty : new ZDateTime(result.GetDateTime(6), DateTimeKind.Utc);
			var workflowPlannedDuration = result.IsDBNull(7) ? 0 : result.GetInt32(7);
			var workflowRemainingEstimate = result.IsDBNull(8) ? 0.0 : result.GetDouble(8);
			var smallestCCPMBufferSize = result.IsDBNull(9) ? 0 : result.GetInt32(9);
			var shapeForZoneCalculation = result.IsDBNull(10) ? null : new ZGuid?(result.GetGuid(10));
			var parentWorkflowForZoneCalculation = result.IsDBNull(11) ? null : new ZGuid?(result.GetGuid(11));
			var taskGroupPK = result.IsDBNull(12) ? Guid.Empty : result.GetGuid(12);

			WorkflowDTO workflow;
			if (workflows.ContainsKey(workflowPK))
			{
				workflow = workflows[workflowPK];
			}
			else
			{
				IBuffer buffer = null;

				if (smallestCCPMBufferSize > 0)
				{
					buffer = buffersBySize.ContainsKey(smallestCCPMBufferSize)
						? buffersBySize[smallestCCPMBufferSize]
						: buffersBySize[smallestCCPMBufferSize] = new BufferDTO { SizeInMinutes = smallestCCPMBufferSize };
				}
				else if (component.IsBuffer)
				{
					buffer = component;
				}

				workflow = workflows[workflowPK] = new WorkflowDTO(workflowPK, buffer)
				{
					ReleaseGroup = releaseGroup,
					ReleaseDate = releaseDate,
					EstimateHours = (decimal)workflowRemainingEstimate,
					PlannedDurationMinutes = workflowPlannedDuration,
					ShapeForZoneCalculation = shapeForZoneCalculation ?? ZGuid.Empty,
					ParentWorkflowForZoneCalculation = parentWorkflowForZoneCalculation ?? ZGuid.Empty,
				};
			}

			if (workflow.Tasks.All(task => task.PK != taskPK))
			{
				workflow.Tasks.Add(new TaskDTO(taskPK, workflow)
				{
					StaffCode = staff.Trim(),
					EstimateHours = (decimal)taskEstimate,
					RequiredCapability = requiredCapability,
					Group = taskGroupPK,
				});
			}
		}

		static string GetCapacityByResourceSQL(BMComponent component, bool hasRestrictedResources)
		{
			const string CapabilityQuery = @"
P9_GS_NKAssignedStaffMember = ''
AND P9_G4_RequiredCapability is not null
AND P9_G4_RequiredCapability in (
	select * from Capabilities)";

			var staffCapabilityQuery = "1 = 1";
			var staffTaskQuery = ProcessTasksSchema.P9_GS_NKAssignedStaffMember.Name + " <> ''"; // Part of SQL statement
			string taskAndCapabilityQuery;

			if (hasRestrictedResources)
			{
				staffCapabilityQuery = GlbResourceCapabilityPivotSchema.G5_GS_Resource.Name + (NoResString)" IN (SELECT value FROM @StaffPKs)";  // SQL parameter name
				staffTaskQuery = ProcessTasksSchema.P9_GS_NKAssignedStaffMember.Name + (NoResString)" IN (SELECT value FROM @StaffCodes)"; // SQL parameter name
			}

			taskAndCapabilityQuery = FormattableString.Invariant($"{staffTaskQuery} OR ({CapabilityQuery})"); // SQL parameter name

			return FormattableString.Invariant($@"
--CAPACITY CALCULATION OLD
DECLARE @WorkflowsNeedingDurations TABLE 
(
       FH_PK uniqueidentifier,
       FH_GG_ReleaseGroup uniqueidentifier,
       FH_FC_CurrentComponent uniqueidentifier,
       FH_ReleaseDateTime datetime,
       FH_PlannedDurationInMinutes int,
       FH_FH_ParentHeader uniqueidentifier
);

WITH Capabilities AS (
       SELECT G5_G4_Capability FROM dbo.GlbResourceCapabilityPivot where {staffCapabilityQuery}
)
INSERT INTO @WorkflowsNeedingDurations
SELECT
       FH_PK,
       FH_GG_ReleaseGroup,
       FH_FC_CurrentComponent,
       FH_ReleaseDateTime,
       FH_PlannedDurationInMinutes,
       FH_FH_ParentHeader
FROM
(
       SELECT * FROM dbo.ProcessHeader
       WHERE FH_PK IN
       (
              SELECT P9_FH_ProcessHeader
              FROM dbo.ProcessTasks
              WHERE
              (
			{taskAndCapabilityQuery}
              )
		AND P9_Type <> 'MIL' AND P9_Type <> 'EXC' AND P9_Type <> 'TRG' AND P9_Status IN ('ASN', 'OPN', 'WRK', 'SUS')-- Status and type filters needed for index.
       )
) headers
WHERE FH_FC_CurrentComponent = @BMComponentPK;

WITH Capabilities AS (
       SELECT G5_G4_Capability FROM dbo.GlbResourceCapabilityPivot where {staffCapabilityQuery}
),
WorkflowsWithDurations AS (
       SELECT
              workflows.FH_PK,
              workflows.FH_GG_ReleaseGroup,
              workflows.FH_ReleaseDateTime,
              workflows.FH_PlannedDurationInMinutes,
              workflows.FH_FH_ParentHeader,
              tasks.*,
              ROUND(taskDuration.Value * 24, 3) taskDurationValueInHours,
              SUM(ROUND((CASE WHEN P9_EstimatedTimeToComplete IS NOT NULL THEN taskDuration.Value * 24 ELSE standardEstimate.Value * 24 END), 3))
                     OVER (PARTITION BY FH_PK)
                     as WorkflowRemainingEstimateInHours
       FROM @WorkflowsNeedingDurations workflows
	JOIN (select * from dbo.ProcessTasks WHERE P9_Status in ('ASN', 'OPN', 'WRK', 'SUS') AND P9_Type <> 'MIL' and P9_Type <> 'EXC' and P9_Type <> 'TRG') tasks on FH_PK = P9_FH_ProcessHeader
       CROSS APPLY dbo.GetDurationAsFloat(COALESCE(P9_EstimatedTimeToComplete, P9_EstDuration)) taskDuration
       CROSS APPLY dbo.GetStandardEstimate(taskDuration.Value, P9_EstimateVariationFactor) standardEstimate
),
WorkflowDurations AS (
       SELECT
              P9_PK,
              P9_GS_NKAssignedStaffMember, 
              FH_PK,
                     (CASE WHEN P9_EstimatedTimeToComplete IS NOT NULL THEN taskDurationValueInHours ELSE standardEstimate.Value END)
			* (CASE WHEN P9_GS_NKAssignedStaffMember = '' THEN 1 / scaleFactor ELSE 1 END) -- When no resource is assigned, consider estimate at a reduced percentage
              TaskDurationInHours,
              P9_G4_RequiredCapability,
              FH_GG_ReleaseGroup,
              FH_ReleaseDateTime,
              FH_PlannedDurationInMinutes,
              FH_FH_ParentHeader,
              WorkflowRemainingEstimateInHours,
              P9_GG_AssignedGroup
       FROM WorkflowsWithDurations
       OUTER APPLY
       (
              SELECT Value AS scaleFactor
              FROM dbo.MaximumOfPair
              (
                     (
                           SELECT Value FROM dbo.MinimumOfPair
                           (
					              {component.FC_BufferTimespanInMinutes} / (CASE WHEN taskDurationValueInHours = 0 THEN 1 ELSE taskDurationValueInHours END), 
                                  (
                                         SELECT COUNT(*)
                                                              FROM dbo.GlbResourceCapabilityPivot
                                                              JOIN dbo.GlbStaff ON GS_PK = G5_GS_Resource
                                                              JOIN dbo.GlbCapability ON  G4_PK = G5_G4_Capability
                                                              WHERE P9_G4_RequiredCapability = G5_G4_Capability AND GS_IsActive = 1
                                                              AND 
                                                              (
										G4_CapacityScope = 'GLB'
                                                                     OR 
                                                                     FH_GG_ReleaseGroup IS NULL AND P9_GG_AssignedGroup IS NULL
                                                                     OR 
                                                                     (
                                                                           G4_CapacityScope = 'GRP' 
                                                                           AND 
                                                                           (
                                                                                  (P9_GG_AssignedGroup IS NULL AND GS_PK IN (SELECT GK_GS FROM dbo.GlbGroupLink WHERE GK_GG = FH_GG_ReleaseGroup))
                                                                                  OR
                                                                                  (GS_PK IN (SELECT GK_GS FROM dbo.GlbGroupLink WHERE GK_GG = P9_GG_AssignedGroup))
                                                                           )
                                                                     )
                                                             )
                                  )
                           )
                     ), 1.0
              )
       ) minmaxval
       CROSS APPLY dbo.GetStandardEstimate(taskDurationValueInHours, P9_EstimateVariationFactor) standardEstimate
       WHERE
              taskDurationValueInHours is not null
		and taskDurationValueInHours <> ''
		and ({taskAndCapabilityQuery})
)
SELECT * INTO #WorkFlowDurations FROM WorkflowDurations;

SELECT
       wd.P9_PK,
       wd.FH_PK,
       wd.P9_GS_NKAssignedStaffMember,
       wd.TaskDurationInHours,
       wd.P9_G4_RequiredCapability,
       wd.FH_GG_ReleaseGroup,
       COALESCE(wflShape.VWS_ScheduledStartTimeUtc, wflInheritedShape.VWS_ScheduledStartTimeUtc, jobShape.VWS_ScheduledStartTimeUtc, jobInheritedShape.VWS_ScheduledStartTimeUtc, overHeader.FH_ReleaseDateTime, wd.FH_ReleaseDateTime) as StartableTime,
       CASE
              WHEN wflShape.VWS_ScheduledStartTimeUtc is not null THEN wflShape.VWS_ExplicitDurationMinutes
              WHEN wflInheritedShape.VWS_ScheduledStartTimeUtc is not null THEN wflInheritedShape.VWS_ExplicitDurationMinutes
              WHEN jobShape.VWS_ScheduledStartTimeUtc is not null THEN jobShape.VWS_ExplicitDurationMinutes
              WHEN jobInheritedShape.VWS_ScheduledStartTimeUtc is not null THEN jobInheritedShape.VWS_ExplicitDurationMinutes
              ELSE COALESCE(overHeader.FH_PlannedDurationInMinutes, wd.FH_PlannedDurationInMinutes)
       END as PlannedDurationInMinutes,
       wd.WorkflowRemainingEstimateInHours,
       COALESCE(wflShape.VWS_PenetratingBufferSizeInMinutes, wflInheritedShape.VWS_PenetratingBufferSizeInMinutes, jobShape.VWS_PenetratingBufferSizeInMinutes, jobInheritedShape.VWS_PenetratingBufferSizeInMinutes) as SmallestCCPMBufferSizeInMinutes,
       COALESCE(wflShape.VWS_BNS_Shape, wflInheritedShape.VWS_BNS_Shape, jobShape.VWS_BNS_Shape, jobInheritedShape.VWS_BNS_Shape) as BNS_ApprovedShapeForZoneCalculation,
       overHeader.FH_PK ParentWorkflowForZoneCalculation,
       wd.P9_GG_AssignedGroup
FROM #WorkFlowDurations wd
LEFT JOIN dbo.ProcessHeader overHeader on (SELECT TOP 1 FH_PK FROM dbo.GetAncestorWorkflows(wd.FH_PK, 1, {BMSRegistry.Instance.MaximumDepthOfAnalyzedWorkflowHierarchy.Value}) order by RowNum desc) = overHeader.FH_PK
LEFT JOIN dbo.ViewApprovedWorkflowSchedule wflShape on 1 = 1
       AND wflShape.VWS_PK = wd.FH_PK
	AND wflShape.VWS_ApprovedScheduleType = 'CCPM'
LEFT JOIN dbo.ViewApprovedWorkflowSchedule wflInheritedShape on 1 = 1
       AND wflInheritedShape.VWS_PK = overHeader.FH_PK
       AND wflInheritedShape.VWS_ApprovedScheduleType = 'CCPM'
LEFT JOIN dbo.ViewApprovedWorkflowSchedule jobShape on 1 = 1
       AND wflShape.VWS_PK IS NULL
       AND jobShape.VWS_PK = wd.FH_FH_ParentHeader
       AND jobShape.VWS_ApprovedScheduleType = 'CCPM'
LEFT JOIN dbo.ViewApprovedWorkflowSchedule jobInheritedShape on 1 = 1
       AND wflShape.VWS_PK IS NULL
       AND jobInheritedShape.VWS_PK = overHeader.FH_FH_ParentHeader
       AND jobInheritedShape.VWS_ApprovedScheduleType = 'CCPM'
");
		}
	}
}
