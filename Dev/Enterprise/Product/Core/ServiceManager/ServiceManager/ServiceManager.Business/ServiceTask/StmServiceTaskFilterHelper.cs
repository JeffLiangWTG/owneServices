using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;

#if NETFRAMEWORK
using Enterprise.ZArchitecture.Core;
#endif

namespace Enterprise.ServiceManager.Business
{
	class StmServiceTaskFilterHelper
	{
		public StmServiceTaskFilterHelper(ZQuery filters, BusinessObjectFactory factory)
			: this(
				filters,
				Enumerable.Empty<IHostedServiceAttribute>(),
				new Dictionary<string, TaskInstanceStatus>(),
				factory)
		{
		}

		public StmServiceTaskFilterHelper(ZQuery filters, IEnumerable<IHostedServiceAttribute> metadataAttributes, IDictionary<string, TaskInstanceStatus> statusCollection, BusinessObjectFactory factory)
		{
			this.metadataAttributes = metadataAttributes;
			this.statusCollection = statusCollection;
			this.factory = factory;
			customFilters = new ZQuery { IsNoResultQuery = filters.IsNoResultQuery };
			tableSchemaFilters = new ZQuery { IsNoResultQuery = filters.IsNoResultQuery };

			var compositeQueryParts = filters.GetCompositeParts();
			foreach (var filter in compositeQueryParts)
			{
				if (IsServiceStatusFilter(filter.LiteralTextADO) || IsMetadataFilter(filter.LiteralTextADO))
				{
					customFilters.AddToFilter(filter);
				}
				else
				{
					tableSchemaFilters.AddToFilter(filter);
				}
			}
		}

		readonly IEnumerable<IHostedServiceAttribute> metadataAttributes;
		readonly IDictionary<string, TaskInstanceStatus> statusCollection;
		readonly BusinessObjectFactory factory;

		readonly ZQuery customFilters;
		public ZQuery CustomFilters => customFilters;

		readonly ZQuery tableSchemaFilters;
		public ZQuery TableSchemaFilters => tableSchemaFilters;

		public ICollection<string> GetFilteredServiceCodes()
		{
			using (var dataTable = new ZDataTable())
			{
				CreateDataTable(dataTable);
				return dataTable.Rows.Count == 0
					? new List<string>()
					: dataTable.Select(CustomFilters.LiteralTextADO).Select(row => row["Code"].ToString()).ToList();
			}
		}

		bool IsServiceStatusFilter(string filter) =>
			!(statusCollection.Count <= 0 && IsFilterInStatusAndDB(filter))
			&& StmServiceTaskCustomFilterNameConstants.ServiceStatusCustomFilterCollection.Exists((dc) =>
				filter.Contains(dc.ColumnName, StringComparison.OrdinalIgnoreCase));

		bool IsMetadataFilter(string filter) =>
			StmServiceTaskCustomFilterNameConstants.MetadataCustomFilterCollection.Exists((dc) =>
				filter.Contains(dc.ColumnName, StringComparison.OrdinalIgnoreCase));

		static bool IsFilterInStatusAndDB(string filter) => filter.Contains(StmServiceTaskCustomFilterNameConstants.NextRunTime, StringComparison.OrdinalIgnoreCase);

		void CreateDataTable(ZDataTable dataTable)
		{
			dataTable.Columns.Add("Code");
			dataTable.Columns.AddRange(StmServiceTaskCustomFilterNameConstants.MetadataCustomFilterCollection.ToArray());
			dataTable.Columns.AddRange(StmServiceTaskCustomFilterNameConstants.ServiceStatusCustomFilterCollection.ToArray());
			dataTable.Columns.Add(StmServiceTaskSchema.Constants.SST_Active);
			dataTable.Columns.Add(StmServiceTaskSchema.Constants.SST_GB_Branch);
			dataTable.Columns.Add(StmServiceTaskSchema.Constants.SST_ServiceTaskCode);

			foreach (var attribute in metadataAttributes)
			{
				var row = dataTable.NewRow();

				row["Code"] = attribute.Code;

				row[StmServiceTaskCustomFilterNameConstants.MutuallyExclusiveGroup] = attribute.MutuallyExclusiveTaskGroup;
				row[StmServiceTaskCustomFilterNameConstants.Description] = attribute.Description;
				row[StmServiceTaskCustomFilterNameConstants.Category] = attribute.Category;
				var serviceTask =
					factory.LoadTop1<StmServiceTask>(new ZQuery(StmServiceTaskSchema.SST_ServiceTaskCode, attribute.Code));
				// when host is not running or for an inactive service task, status won't exist
				if (statusCollection.TryGetValue(attribute.Code, out var status))
				{
					row[StmServiceTaskCustomFilterNameConstants.PlaceInQueue] = status.PlaceInQueueString;
					row[StmServiceTaskCustomFilterNameConstants.ProcessId] = status.ProcessIDsString;
					row[StmServiceTaskCustomFilterNameConstants.RegisteredOnHosts] = status.RegisteredOnHosts;
					row[StmServiceTaskCustomFilterNameConstants.SecondsInQueue] = status.SecondsInQueueString;
					row[StmServiceTaskCustomFilterNameConstants.SecondsRunning] = status.SecondsRunningString;
					row[StmServiceTaskCustomFilterNameConstants.RunningCount] = status.RunningCount;
					row[StmServiceTaskCustomFilterNameConstants.StatusString] = status.StatusString;
					row[StmServiceTaskCustomFilterNameConstants.BindingsCount] = status.BindingCount;
					row[StmServiceTaskCustomFilterNameConstants.BindingTypes] = status.BindingTypes;
					row[StmServiceTaskCustomFilterNameConstants.NextRunTime] = status.NextRunTime.HasValue ? status.NextRunTime : DBNull.Value;
					row[StmServiceTaskCustomFilterNameConstants.LastRunTime] = status.LastRunTime.HasValue ? status.LastRunTime : DBNull.Value;
					row[StmServiceTaskCustomFilterNameConstants.LastErrorTime] = status.LastErrorTime.HasValue ? status.LastErrorTime : DBNull.Value;
					row[StmServiceTaskCustomFilterNameConstants.ErrorCountLast24Hours] = status.ErrorCountLast24Hours;
					row[StmServiceTaskSchema.Constants.SST_Active] = serviceTask?.SST_Active ?? false ? 1 : 0;
					row[StmServiceTaskSchema.Constants.SST_GB_Branch] = serviceTask?.SST_GB_Branch;
					row[StmServiceTaskSchema.Constants.SST_ServiceTaskCode] = serviceTask?.SST_ServiceTaskCode;
				}

				dataTable.Rows.Add(row);
			}
		}
	}
}

