using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
#if NETFRAMEWORK
using Enterprise.ZArchitecture.Core;
#endif
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business
{
	public class ServiceTaskScheduleFilterHelper
	{
		public ServiceTaskScheduleFilterHelper(ZQuery filters, IDictionary<string, TaskInstanceStatus> statusCollection, BusinessObjectFactory factory)
		{
			this.statusCollection = statusCollection;
			this.factory = factory;
			serviceStatusFilters = new ZQuery { IsNoResultQuery = filters.IsNoResultQuery };
			tableSchemaFilters = new ZQuery { IsNoResultQuery = filters.IsNoResultQuery };

			var compositeQueryParts = filters.GetCompositeParts();
			foreach (var filter in compositeQueryParts)
			{
				if (CustomTextFilterNameConstants.IsServiceStatusFilter(filter.LiteralTextADO, statusCollection))
				{
					serviceStatusFilters.AddToFilter(filter);
				}
				else
				{
					tableSchemaFilters.AddToFilter(filter);
				}
			}
		}

		readonly IDictionary<string, TaskInstanceStatus> statusCollection;
		readonly BusinessObjectFactory factory;

		readonly ZQuery serviceStatusFilters;
		public ZQuery ServiceStatusFilters => serviceStatusFilters;

		readonly ZQuery tableSchemaFilters;
		public ZQuery TableSchemaFilters => tableSchemaFilters;

		public ICollection<string> GetFilteredServiceCodes()
		{
			using (var statusDataTable = new ZDataTable())
			{
				CreateStatusDataTable(statusDataTable);
				if (statusDataTable.Rows.Count == 0)
				{
					return new List<string>();
				}
				return statusDataTable.Select(serviceStatusFilters.LiteralTextADO).Select(row => row["Code"].ToString()).ToList();
			}
		}

		void CreateStatusDataTable(ZDataTable statusDataTable)
		{
			statusDataTable.Columns.Add("Code");
			statusDataTable.Columns.AddRange(CustomTextFilterNameConstants.GetServiceStatusCustomFilterCollection().ToArray());
			statusDataTable.Columns.Add(StmScheduleTaskSchema.Constants.S5_ParentTableCode);
			statusDataTable.Columns.Add(StmScheduleTaskSchema.Constants.S5_GB);
			statusDataTable.Columns.Add(StmScheduleTaskSchema.Constants.S5_GS_NKPrintUser);
			statusDataTable.Columns.Add(StmScheduleTaskSchema.Constants.S5_IsActive);
			statusDataTable.Columns.Add(StmScheduleTaskSchema.Constants.S5_ScheduleType);
			statusDataTable.Columns.Add(StmScheduleTaskSchema.Constants.S5_ScheduleDescription);
			statusDataTable.Columns.Add(StmScheduleTaskSchema.Constants.S5_TypeOfDocument);
			foreach (var status in statusCollection)
			{
				var schedule =
					factory.LoadTop1<ServiceTaskSchedule>(new ZQuery(StmScheduleTaskSchema.S5_ScheduleType, status.Key));
				var row = statusDataTable.NewRow();
				row["Code"] = status.Key;
				row[CustomTextFilterNameConstants.PlaceInQueue] = status.Value.PlaceInQueueString;
				row[CustomTextFilterNameConstants.ProcessId] = status.Value.ProcessIDsString;
				row[CustomTextFilterNameConstants.RegisteredOnHosts] = status.Value.RegisteredOnHosts;
				row[CustomTextFilterNameConstants.SecondsInQueue] = status.Value.SecondsInQueueString;
				row[CustomTextFilterNameConstants.SecondsRunning] = status.Value.SecondsRunningString;
				row[CustomTextFilterNameConstants.RunningCount] = status.Value.RunningCount;
				row[CustomTextFilterNameConstants.StatusString] = status.Value.StatusString;
				row[CustomTextFilterNameConstants.BindingsCount] = (int?)schedule?.ServiceTaskBindingsCount ?? 0;
				row[CustomTextFilterNameConstants.BindingTypes] = status.Value.BindingTypes;
				row[CustomTextFilterNameConstants.NextRunTime] = status.Value.NextRunTime.HasValue ? status.Value.NextRunTime : DBNull.Value;
				row[CustomTextFilterNameConstants.LastRunTime] = status.Value.LastRunTime.HasValue ? status.Value.LastRunTime : DBNull.Value;
				row[CustomTextFilterNameConstants.LastErrorTime] = status.Value.LastErrorTime.HasValue ? status.Value.LastErrorTime : DBNull.Value;
				row[CustomTextFilterNameConstants.ErrorCountLast24Hours] = status.Value.ErrorCountLast24Hours;
				row[CustomTextFilterNameConstants.MutuallyExclusiveGroup] = schedule?.StaticServiceAttributes?.MutuallyExclusiveTaskGroup;
				row[StmScheduleTaskSchema.Constants.S5_ParentTableCode] = schedule?.S5_ParentTableCode;
				row[StmScheduleTaskSchema.Constants.S5_GB] = schedule?.S5_GB;
				row[StmScheduleTaskSchema.Constants.S5_GS_NKPrintUser] = schedule?.S5_GS_NKPrintUser;
				row[StmScheduleTaskSchema.Constants.S5_IsActive] = schedule?.S5_IsActive ?? false ? 1 : 0;
				row[StmScheduleTaskSchema.Constants.S5_ScheduleType] = schedule?.S5_ScheduleType;
				row[StmScheduleTaskSchema.Constants.S5_ScheduleDescription] = schedule?.S5_ScheduleDescription;
				row[StmScheduleTaskSchema.Constants.S5_TypeOfDocument] = schedule?.S5_TypeOfDocument;
				statusDataTable.Rows.Add(row);
			}
		}
	}

	public static class CustomTextFilterNameConstants
	{
		public const string RegisteredOnHosts = "RegisteredOnHosts";
		public const string BindingTypes = "BindingTypes";
		public const string BindingsCount = "BindingsCount";
		public const string ProcessId = "ProcessID";
		public const string SecondsInQueue = "SecondsInQueue";
		public const string SecondsRunning = "SecondsRunning";
		public const string PlaceInQueue = "PlaceInQueue";
		public const string RunningCount = "S5_ScheduleActualRunCount";
		public const string StatusString = "StatusString";
		public const string NextRunTime = "S5_NextScheduledPrintRunTimeUtc";
		public const string LastRunTime = "LastRunTime";
		public const string LastErrorTime = "LastErrorTime";
		public const string ErrorCountLast24Hours = "ErrorCountLast24Hours";
		public const string MutuallyExclusiveGroup = "MutuallyExclusiveGroup";
		public const string ProcessHost = "ProcessHost";

		public static List<DataColumn> GetServiceStatusCustomFilterCollection()
		{
			return new List<DataColumn> {
				new DataColumn(RegisteredOnHosts, typeof(string)),
				new DataColumn(ProcessId, typeof(string)),
				new DataColumn(SecondsInQueue, typeof(string)),
				new DataColumn(SecondsRunning, typeof(string)),
				new DataColumn(PlaceInQueue, typeof(string)),
				new DataColumn(RunningCount, typeof(int)),
				new DataColumn(StatusString, typeof(string)),
				new DataColumn(BindingTypes, typeof(string)),
				new DataColumn(BindingsCount, typeof(int)),
				new DataColumn(NextRunTime, typeof(DateTime)),
				new DataColumn(LastRunTime, typeof(DateTime)),
				new DataColumn(LastErrorTime, typeof(DateTime)),
				new DataColumn(ErrorCountLast24Hours, typeof(int)),
				new DataColumn(MutuallyExclusiveGroup, typeof(string)),
			};
		}

		public static bool IsServiceStatusFilter(string filter, IDictionary<string, TaskInstanceStatus> statusCollection)
		{
			if (statusCollection.Count <= 0 && IsFilterInStatusAndDB(filter))
			{
				return false;
			}

			return GetServiceStatusCustomFilterCollection().Exists((dc) => filter.Contains(dc.ColumnName, StringComparison.OrdinalIgnoreCase));
		}

		static bool IsFilterInStatusAndDB(string filter)
		{
			return filter.Contains(NextRunTime, StringComparison.OrdinalIgnoreCase);
		}
	}
}

