using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class RoadRunnerTaskCache
	{
		public RoadRunnerTaskCache(ZGuid componentPK, params GlbStaff[] resources)
		{
			this.componentPK = componentPK;
			this.resources = resources;
			this.factory = resources[0].Factory;
		}

		readonly ZGuid componentPK;
		readonly GlbStaff[] resources;
		readonly BusinessObjectFactory factory;

		internal ProcessTask GetWorkingTaskInAnyBuffer(GlbStaff resource)
		{
			return GetFromCacheOrLoad(resource, WorkingTasksInAnyBuffer, GetWorkingTasksInAnyBuffer).FirstOrDefault();
		}

		internal ProcessTask GetWorkingTaskInThisComponent(GlbStaff resource)
		{
			return GetFromCacheOrLoad(resource, WorkingTasksInThisComponent, GetWorkingTasksInThisComponent).FirstOrDefault();
		}

		internal ProcessTask GetWorkingTask(GlbStaff resource)
		{
			return GetFromCacheOrLoad(resource, WorkingTasks, GetWorkingTasks).FirstOrDefault();
		}

		internal ProcessTask GetSuspendedTask(GlbStaff resource)
		{
			return GetFromCacheOrLoad(resource, SuspendedTasks, GetSuspendedTasks).FirstOrDefault();
		}

		internal ProcessTask GetClosedTask(GlbStaff resource)
		{
			return GetFromCacheOrLoad(resource, ClosedTasks, GetClosedTasks).FirstOrDefault();
		}

		#region Task Queries

		ProcessTask[] GetWorkingTasksInAnyBuffer()
		{
			var query = GetBaseQueryForResources();

			var processHeaderSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessTasksSchema.P9_FH_ProcessHeader);
			var componentSubQuery = new ZDBOnlySubQuery(typeof(BMComponent), ProcessHeaderSchema.FH_FC_CurrentComponent);
			componentSubQuery.AddToFilter(BMComponentSchema.FC_Type, BMComponentTypeList.Codes.Buffer);

			processHeaderSubQuery.AddSubQuery(componentSubQuery, JoinCondition.And);
			query.AddSubQuery(processHeaderSubQuery, JoinCondition.And);

			return factory.Load<ProcessTask>(query);
		}

		ProcessTask[] GetWorkingTasksInThisComponent()
		{
			if (componentPK.IsValid)
			{
				var query = GetBaseQueryForResources();

				var processHeaderSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessTasksSchema.P9_FH_ProcessHeader);
				processHeaderSubQuery.AddToFilter(ProcessHeaderSchema.FH_FC_CurrentComponent, componentPK);
				var componentSubQuery = new ZDBOnlySubQuery(typeof(BMComponent), ProcessHeaderSchema.FH_FC_CurrentComponent);
				componentSubQuery.AddToFilter(BMComponentSchema.FC_Type, BMComponentTypeList.Codes.Bucket);

				processHeaderSubQuery.AddSubQuery(componentSubQuery, JoinCondition.And);
				query.AddSubQuery(processHeaderSubQuery, JoinCondition.And);

				return factory.Load<ProcessTask>(query);
			}
			else
			{
				return Array.Empty<ProcessTask>();
			}
		}

		ProcessTask[] GetWorkingTasks()
		{
			var query = GetBaseQueryForResources();
			return factory.Load<ProcessTask>(query);
		}

		ZDBOnlyQuery GetBaseQueryForResources()
		{
			var query = new ZDBOnlyQuery(typeof(ProcessTask)) { AllowTableValuedParameters = true };
			query.AddToFilter(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, ResourceCodes);
			query.AddToFilter(ProcessTasksSchema.P9_Status, ProcessTaskStatusCodeList.Codes.Working);
			return query;
		}

		#region Query For One Row Per Resource

		const string ResourceCodeParameterName = "@StaffCodes";

		ZDBOnlyQuery QueryForOneOrderedRow(string status)
		{
			ZString orderByColumn;
			switch (status)
			{
				case ProcessTaskStatusCodeList.Codes.Closed:
					orderByColumn = ProcessTasks.Schema.P9_CompletedTime;
					break;

				case ProcessTaskStatusCodeList.Codes.Suspended:
					orderByColumn = ProcessTasks.Schema.P9_SuspendedAt;
					break;

				default:
					throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Cannot use {0} status since it doesn't have a column to order by to find most recent row", status), nameof(status));
			}

			var resourceCodesParam = ZSqlParameter.New(ResourceCodeParameterName, ResourceCodes.ToArray(), ProcessTasksSchema.P9_GS_NKAssignedStaffMember, isTableValued: true);
			var queryText = FormattableString.Invariant($@"
			P9_PK in
			(
				SELECT
				(
					SELECT TOP 1 P9_PK FROM dbo.ProcessTasks
					WHERE P9_GS_NKAssignedStaffMember = Value -- uses the TableValuedParameter for ResourceCodes
					and P9_Status = '{status}' and P9_Type <> 'MIL' and P9_Type <> 'TRG' and P9_Type <> 'EXC' -- Ensures we use filtered index
					ORDER BY {orderByColumn} DESC
				)
				FROM
				(
					Select Value from {ResourceCodeParameterName}
				) ResourceCodes
			)");

			var query = new ZDBOnlyQuery(typeof(ProcessTask));
			query.AddFilterAndZSQLParameterCollection(queryText, new ZSqlParameterCollection(resourceCodesParam));

			return query;
		}

		#endregion

		ProcessTask[] GetProcessTasksOneRowPerResource(string status)
		{
			if (!BMSRegistry.Instance.ShowIdleTimeOnBoards.Value)
			{
				ErrorReporter.ReportOnce("This query is costly and has a registry item to kill it when performance becomes a problem. Somehow this code path is not respecting that registry item.");
			}

			return factory.Load<ProcessTask>(QueryForOneOrderedRow(status));
		}

		ProcessTask[] GetSuspendedTasks()
		{
			return GetProcessTasksOneRowPerResource(ProcessTaskStatusCodeList.Codes.Suspended);
		}

		ProcessTask[] GetClosedTasks()
		{
			return GetProcessTasksOneRowPerResource(ProcessTaskStatusCodeList.Codes.Closed);
		}

		IEnumerable<ZString> ResourceCodes
		{
			get { return resources.Select(s => s.GS_Code); }
		}

		#endregion

		#region Cache Implementation

		const string WorkingTasksInAnyBuffer = "WorkingTasksInAnyBuffer";
		const string WorkingTasksInThisComponent = "WorkingTasksInThisComponent";
		const string WorkingTasks = "WorkingTasks";
		const string SuspendedTasks = "SuspendedTasks";
		const string ClosedTasks = "ClosedTasks";

		ProcessTask[] GetFromCacheOrLoad(GlbStaff staff, string cacheKey, Func<ProcessTask[]> loaderFunc)
		{
			return taskCache.GetOrAdd(cacheKey, loaderFunc).Where(t => staff == null || string.Equals(t.P9_GS_NKAssignedStaffMember, staff.GS_Code, StringComparison.CurrentCultureIgnoreCase)).ToArray();
		}

		readonly Dictionary<string, ProcessTask[]> taskCache = new Dictionary<string, ProcessTask[]>();

		#endregion
	}
}
