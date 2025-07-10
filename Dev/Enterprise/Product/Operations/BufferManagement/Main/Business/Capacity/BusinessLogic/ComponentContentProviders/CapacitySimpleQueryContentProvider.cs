using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Implementation;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	static class CapacitySimpleQueryContentProvider
	{
		#region SQLs

		const string StaffIndexHint = "WITH(INDEX(NR_RX__P9_FC_CurrentComponent_P9_GS_NKAssignedStaffMember))";
		const string CapabilityIndexHint = "WITH(INDEX(NR_RX__P9_FC_CurrentComponent_P9_G4_RequiredCapability))";

		const string ParentHeaderColumns = @",
	COALESCE(parentHeader.FH_ReleaseDateTime, header.FH_ReleaseDateTime) as ReleaseDate,
	COALESCE(parentHeader.FH_PlannedDurationInMinutes, header.FH_PlannedDurationInMinutes) as PlannedDurationInMinutes,
	parentHeader.FH_PK as ParentWorkflowForZoneCalculation";
		const string LeftJoinParentHeader = "LEFT JOIN dbo.ProcessHeader parentHeader ON (SELECT TOP 1 FH_PK FROM dbo.GetAncestorWorkflows(header.FH_PK , 1, 10) order by RowNum desc) = parentHeader.FH_PK";

		const string TaskEstimatesSQL = @"
SELECT
	P9_PK,
	P9_FH_ProcessHeader,
	P9_GS_NKAssignedStaffMember,
	P9_G4_RequiredCapability,
	P9_GG_AssignedGroup,
	header.FH_GG_ReleaseGroup,
	P9_EstimatedTimeToComplete,
	P9_EstDuration,
	P9_EstimateVariationFactor{2}
FROM dbo.ProcessTasks task {0}
INNER JOIN dbo.ProcessHeader header ON header.FH_PK = task.P9_FH_ProcessHeader
{3}
WHERE P9_Status in ('ASN', 'OPN', 'WRK', 'SUS') AND P9_Type <> 'MIL' and P9_Type <> 'EXC' and P9_Type <> 'TRG' AND
(P9_EstimatedTimeToComplete IS NOT NULL OR P9_EstDuration IS NOT NULL) AND
task.P9_FC_CurrentComponent = @BMComponentPK AND {1}
";

		const string CapabilityPerStaffSQL = @"
--CAPACITY CALCULATION GetNumberOfStaffInCapabilities
WITH capabilityPKs AS (
	SELECT G5_G4_Capability FROM dbo.GlbResourceCapabilityPivot where G5_GS_Resource in (SELECT value FROM @StaffPKs)
)
SELECT G5_G4_Capability, NULL, G4_CapacityScope, COUNT(*)
FROM dbo.GlbResourceCapabilityPivot
INNER JOIN dbo.GlbStaff ON GS_PK = G5_GS_Resource
INNER JOIN dbo.GlbCapability ON  G4_PK = G5_G4_Capability
WHERE G5_G4_Capability IN (SELECT G5_G4_Capability FROM capabilityPKs) AND GS_IsActive = 1
GROUP BY G5_G4_Capability, G4_CapacityScope
UNION
SELECT G5_G4_Capability, GK_GG, G4_CapacityScope, COUNT(*)
FROM dbo.GlbResourceCapabilityPivot
INNER JOIN dbo.GlbCapability ON  G4_PK = G5_G4_Capability
INNER JOIN dbo.GlbStaff ON GS_PK = G5_GS_Resource
INNER JOIN dbo.GlbGroupLink ON GK_GS =  GS_PK 
WHERE G4_CapacityScope = 'GRP' AND G5_G4_Capability IN (SELECT G5_G4_Capability FROM capabilityPKs) AND GS_IsActive = 1
GROUP BY G5_G4_Capability, G4_CapacityScope, GK_GG";

		#endregion

		#region SqlResult

		class SqlResult
		{
			public Guid TaskPK { get; set; }
			public Guid WorkflowPK { get; set; }
			public string StaffCode { get; set; }
			public Guid Capability { get; set; }
			public Guid TaskGroup { get; set; }
			public Guid WorkflowGroup { get; set; }
			public ZDateTime EstimatedTimeToComplete { get; set; }
			public ZDateTime EstDuration { get; set; }
			public decimal EstimateVariationFactor { get; set; }
			public ZDateTime ReleaseDate { get; set; }
#pragma warning disable CW1050 // Use System.TimeSpan Type For A Duration
			public int PlannedDurationInMinutes { get; set; }
#pragma warning restore CW1050 // Use System.TimeSpan Type For A Duration
			public Guid parentWorkflowForZoneCalculation { get; set; }
		}

		#endregion

		internal static ICollection<WorkflowDTO> Load(BMComponent component, IEnumerable<GlbStaff> staffList, ILogger logger = null)
		{
			if (component == null || !staffList.Any())
			{
				return Enumerable.Empty<WorkflowDTO>().ToArray();
			}

			var enableZoneMultipliers = !ExperimentalSettingsProvider.ZoneMultipliersDisabled(component);
			var parrentHeaderColumns = enableZoneMultipliers ? ParentHeaderColumns : string.Empty;
			var parentHeaderLeftJoing = enableZoneMultipliers ? LeftJoinParentHeader : string.Empty;

			var staffCodes = staffList.Select(r => r.GS_Code).ToArray();
			var staffFilter = $"P9_GS_NKAssignedStaffMember IN ({staffCodes.ListToParamString()})"; // SQL parameter name
			var staffTaskEstimatesSQL = string.Format("--CAPACITY CALCULATION GetTaskEstimates" + TaskEstimatesSQL, StaffIndexHint, staffFilter, parrentHeaderColumns, parentHeaderLeftJoing); // SQL parameter name

			var staffPKs = staffList.Select(r => r.PK.ToGuid()).ToArray();
			var numberOfStaffsInCapabilities = GetNumberOfStaffInCapabilities(staffPKs, logger);
			var capabilityPKs = numberOfStaffsInCapabilities.Keys.Select(k => k.capabilityPK).Distinct();

			var taskEstimatesSQL = $"--CAPACITY CALCULATION SIMPLE QUERY\n{staffTaskEstimatesSQL}"; // SQL parameter name

			if (capabilityPKs.Any())
			{
				var capabilityFilter = $"P9_G4_RequiredCapability IN ({capabilityPKs.ListToParamString()})"; // SQL parameter name
				var capabilityTaskEstimatesSQL = string.Format(TaskEstimatesSQL, CapabilityIndexHint, capabilityFilter, parrentHeaderColumns, parentHeaderLeftJoing);

				taskEstimatesSQL += $"UNION ALL{capabilityTaskEstimatesSQL}"; // SQL parameter name
			}

#pragma warning disable CW1107 // Do Not Use Db.Connection Methods
			var command = Db.Connection.Command(taskEstimatesSQL); // Calculating capacity
#pragma warning restore CW1107 // Do Not Use Db.Connection Methods

			staffList = staffList.Where(r => r.GS_IsActive);

			command.AddParameter("@BMComponentPK", SqlDbType.UniqueIdentifier, component.PK.ToGuid()); // SQL parameter name

			var workflows = new Dictionary<Guid, WorkflowDTO>();
			var allTasks = new Dictionary<Guid, TaskDTO>();
			IBuffer buffer = component.IsBuffer ? component : null;

			var results = new List<SqlResult>();
			var taskPKs = new HashSet<Guid>();

			var startingTime = ZDateTime.UtcNow;
			using (var reader = command.ExecuteReader(CommandBehavior.SequentialAccess))
			{
				while (reader.Read())
				{
					var taskPK = reader.GetGuid(0);
					if (!taskPKs.Add(taskPK))
					{
						continue;
					}

					var result = new SqlResult
					{
						TaskPK = taskPK,
						WorkflowPK = reader.GetGuid(1),
						StaffCode = reader.GetValueSafe(2, string.Empty),
						Capability = reader.GetGuidSafe(3),
						TaskGroup = reader.GetGuidSafe(4),
						WorkflowGroup = reader.GetGuidSafe(5),
						EstimatedTimeToComplete = reader.GetZDateTime(6),
						EstDuration = reader.GetZDateTime(7),
						EstimateVariationFactor = reader.GetValueSafe<decimal>(8)
					};

					if (enableZoneMultipliers)
					{
						result.ReleaseDate = reader.GetZDateTime(9);
						result.PlannedDurationInMinutes = reader.GetInt32(10);
						result.parentWorkflowForZoneCalculation = reader.GetGuidSafe(11);
					}

					results.Add(result);
				}
			}

			var finishedTime = ZDateTime.UtcNow;
			logger?.Log(LogType.Information, $"Task Estimate query executed in {finishedTime - startingTime}");

			foreach (var result in results)
			{
				var workflow = workflows.GetOrAdd(result.WorkflowPK, () => new WorkflowDTO(result.WorkflowPK, buffer)
				{
					ReleaseGroup = result.WorkflowGroup,
					ReleaseDate = result.ReleaseDate,
					PlannedDurationMinutes = result.PlannedDurationInMinutes,
					ParentWorkflowForZoneCalculation = result.parentWorkflowForZoneCalculation,
				});

				var estimateHours = TaskDurationCalculator.GetRelevantEstimateHours(result.EstimatedTimeToComplete, result.EstDuration, result.EstimateVariationFactor);
				var task = new TaskDTO(result.TaskPK, workflow)
				{
					StaffCode = result.StaffCode.Trim(),
					EstimateHours = (decimal)estimateHours,
					RequiredCapability = result.Capability,
					Group = result.TaskGroup,
				};

				allTasks.Add(task.PK, task);

				workflow.Tasks.Add(task);
			}

			foreach (var workflow in workflows.Values)
			{
				workflow.EstimateHours = workflow.Tasks.Sum(t => t.EstimateHours);
			}

			SpreadEstimateOfCapabilitiesTasks(allTasks, numberOfStaffsInCapabilities);

			return workflows.Values;
		}

		static void SpreadEstimateOfCapabilitiesTasks(Dictionary<Guid, TaskDTO> tasks, IDictionary<(Guid capabilityPK, Guid? groupPK), (string scope, int count)> numberOfStaffInCapabilities)
		{
			var capabilityTasks = tasks.Values
				.Where(task => string.IsNullOrEmpty(task.StaffCode) && task.RequiredCapability != Guid.Empty);

			foreach (var task in capabilityTasks)
			{
				var (scope, numberOfStaffsInCapability) = numberOfStaffInCapabilities[(task.RequiredCapability, null)];

				if (scope == GlbCapabilityScopeList.Codes.GroupScope)
				{
					var group = task.Group != Guid.Empty ? task.Group : task.Workflow.ReleaseGroup;

					if (group != Guid.Empty && numberOfStaffInCapabilities.TryGetValue((task.RequiredCapability, group), out var capability))
					{
						numberOfStaffsInCapability = capability.count;
					}
				}

				task.EstimateHours /= numberOfStaffsInCapability;
			}
		}

		static IDictionary<(Guid capabilityPK, Guid? groupPK), (string scope, int count)> GetNumberOfStaffInCapabilities(IEnumerable<Guid> staffPKs, ILogger logger = null)
		{
#pragma warning disable CW1107 // Do Not Use Db.Connection Methods
			var command = Db.Connection.Command(CapabilityPerStaffSQL); // Calculating capacity
#pragma warning restore CW1107 // Do Not Use Db.Connection Methods

			command.AddTableValuedParameter("@StaffPKs", GlbStaffSchema.PK, staffPKs);

			var numberOfStaffInCapabilities = new Dictionary<(Guid capabilityPK, Guid? groupPK), (string scope, int count)>();

			var startingTime = ZDateTime.UtcNow;
			using (var result = command.ExecuteReader(CommandBehavior.SequentialAccess))
			{
				while (result.Read())
				{
					numberOfStaffInCapabilities.Add(
						 (result.GetGuid(0), GetValueSafe<Guid?>(result, 1)),
						 (result.GetString(2), result.GetInt32(3)));
				}
			}
			var finishedTime = ZDateTime.UtcNow;
			logger?.Log(LogType.Information, $"Staff Capabilities query executed in {finishedTime - startingTime}");

			return numberOfStaffInCapabilities;
		}

		#region Helpers

		static T GetValueSafe<T>(this IDataReader reader, int position, T ifNull = default) => reader.IsDBNull(position) ? ifNull : (T)reader.GetValue(position);

		static ZDateTime GetZDateTime(this IDataReader reader, int position) => reader.IsDBNull(position) ? ZDateTime.Empty : new ZDateTime(reader.GetDateTime(position), DateTimeKind.Utc);

		static Guid GetGuidSafe(this IDataReader reader, int position) => GetValueSafe<Guid>(reader, position);

		#endregion
	}
}
