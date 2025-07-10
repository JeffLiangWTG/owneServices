using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	class StmServiceTaskFilterHelperTest : TestCaseWithFactory
	{
		public void TestDecompositeFilters()
		{
			var filters = new ZQuery();
			filters.AddToFilter(new ZQuery(StmServiceTaskSchema.SST_ServiceTaskCode, "SST_ServiceTaskCode"));
			filters.AddToFilter(new ZQuery(StmServiceTaskSchema.SST_GB_Branch, Guid.Empty));
			filters.AddToFilter(new ZQuery(StmServiceTaskSchema.SST_Active, true));

			filters.AddToFilter(new ZQuery(StmServiceTaskStatusColumnProvider.PlaceInQueue, "PlaceInQueue"));
			filters.AddToFilter(new ZQuery(StmServiceTaskStatusColumnProvider.SecondsRunning, "SecondsRunning"));
			filters.AddToFilter(new ZQuery(StmServiceTaskStatusColumnProvider.SecondsInQueue, "SecondsInQueue"));
			filters.AddToFilter(new ZQuery(StmServiceTaskStatusColumnProvider.RegisteredOnHosts, "RegisteredOnHosts"));
			filters.AddToFilter(new ZQuery(StmServiceTaskStatusColumnProvider.ProcessId, "ProcessId"));
			filters.AddToFilter(new ZQuery(StmServiceTaskStatusColumnProvider.StatusString, "StatusString"));

			filters.AddToFilter(new ZQuery(StmServiceTaskStatusColumnProvider.BindingsCount, 1));
			filters.AddToFilter(new ZQuery(StmServiceTaskStatusColumnProvider.BindingTypes, "BindingTypes"));
			filters.AddToFilter(new ZQuery(StmServiceTaskStatusColumnProvider.NextRunTime, DateTime.UtcNow));
			filters.AddToFilter(new ZQuery(StmServiceTaskStatusColumnProvider.LastRunTime, DateTime.UtcNow));
			filters.AddToFilter(new ZQuery(StmServiceTaskStatusColumnProvider.LastErrorTime, DateTime.UtcNow));
			filters.AddToFilter(new ZQuery(StmServiceTaskStatusColumnProvider.ErrorCountLast24Hours, 0));

			var decompositedFilters = new StmServiceTaskFilterHelper(filters, Factory);
			AssertEquals(11, decompositedFilters.CustomFilters.Params.Length);
			AssertEquals(4, decompositedFilters.TableSchemaFilters.Params.Length);
			AssertNotNull(decompositedFilters.TableSchemaFilters.Params.FirstOrDefault((par) => string.Equals(par.SchemaColumn.Name, StmServiceTaskStatusColumnProvider.NextRunTime.Name, StringComparison.OrdinalIgnoreCase)));

			var dummyStatusCollection = new Dictionary<string, TaskInstanceStatus>();
			dummyStatusCollection.Add("BLA", new TaskInstanceStatus());
			decompositedFilters = new StmServiceTaskFilterHelper(filters, Enumerable.Empty<IHostedServiceAttribute>(), dummyStatusCollection, Factory);
			AssertEquals(12, decompositedFilters.CustomFilters.Params.Length);
			AssertEquals(3, decompositedFilters.TableSchemaFilters.Params.Length);
			AssertNotNull(decompositedFilters.CustomFilters.Params.FirstOrDefault((par) => string.Equals(par.SchemaColumn.Name, StmServiceTaskStatusColumnProvider.NextRunTime.Name, StringComparison.OrdinalIgnoreCase)));

			decompositedFilters = new StmServiceTaskFilterHelper(new ZQuery(), Factory);
			AssertEquals(0, decompositedFilters.CustomFilters.Params.Length);
			AssertEquals(0, decompositedFilters.TableSchemaFilters.Params.Length);
		}

		public void TestHasDateFilters()
		{
			var filters = new ZQuery();
			filters.AddToFilter(new ZQuery(StmServiceTaskStatusColumnProvider.LastRunTime, SQLComparisonOperator.Equal, null));
			filters.AddToFilter(new ZQuery(StmServiceTaskStatusColumnProvider.LastErrorTime, SQLComparisonOperator.NotEqual, null));

			var decompositedFilters = new StmServiceTaskFilterHelper(filters, Factory);
			AssertEquals(string.Empty, decompositedFilters.TableSchemaFilters.ParameterisedText.LiteralTextSql);
			AssertEquals(filters.ParameterisedText.LiteralTextSql, decompositedFilters.CustomFilters.ParameterisedText.LiteralTextSql);
		}

		[TestDate(2024, 7, 29, 0, 0, 0)]
		public void TestGetFilteredServiceCodes()
		{
			var time1 = ZDateTime.UtcNow;
			var time2 = time1.AddMinutes(2);

			var attributes = Enumerable.Range(1, 9)
				.Select(i =>
					Mock.Of<IHostedServiceAttribute>(o =>
						o.Code == string.Format("T{0:D2}", i) &&
						o.Description == "Dummy Description" &&
						o.Category == "TST" &&
						o.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
						o.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "15minutes")))
				.ToList();
			attributes.Add(Mock.Of<IHostedServiceAttribute>(o =>
				o.Code == "T10" &&
				o.Description == "Dummy Description" &&
				o.Category == "AAA" &&
				o.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.Upgrade &&
				o.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "15minutes")));

			var statusCollection = new Dictionary<string, TaskInstanceStatus>()
			{
				{ "T01", new TaskInstanceStatus() { ProcessIDsString = "ProcessId", StatusString = "StatusString", BindingCount = 1, BindingTypes = "BindingTypes", NextRunTime = time2.ToDateTime() }  },
				{ "T02", new TaskInstanceStatus() { ProcessIDsString = "ProcessId", BindingCount = 1, BindingTypes = "BindingTypes", NextRunTime = time2.ToDateTime() } },
				{ "T03", new TaskInstanceStatus() { ProcessIDsString = "ProcessId", StatusString = "StatusString", BindingTypes = "BindingTypes", NextRunTime = time2.ToDateTime() } },
				{ "T04", new TaskInstanceStatus() { ProcessIDsString = "ProcessId", StatusString = "StatusString", BindingCount = 1, NextRunTime = time2.ToDateTime() } },
				{ "T05", new TaskInstanceStatus() { StatusString = "StatusString", BindingCount = 1, BindingTypes = "BindingTypes", NextRunTime = time1.ToDateTime() } },
				{ "T06", new TaskInstanceStatus() { ProcessIDsString = "ProcessId", BindingCount = 1, BindingTypes = "BindingTypes", NextRunTime = time2.ToDateTime() } },
				{ "T07", new TaskInstanceStatus() { ProcessIDsString = "ProcessId", StatusString = "StatusString", BindingTypes = "BindingTypes", NextRunTime = time2.ToDateTime() } },
				{ "T08", new TaskInstanceStatus() { ProcessIDsString = "ProcessId", StatusString = "StatusString", BindingCount = 1, NextRunTime = time2.ToDateTime() } },
				{ "T09", new TaskInstanceStatus() { ProcessIDsString = "test", StatusString = "StatusString", BindingCount = 1, BindingTypes = "BindingTypes", NextRunTime = time2.ToDateTime() } },
			};

			var filterHelper = new StmServiceTaskFilterHelper(new ZQuery(), attributes, statusCollection, Factory);
			AssertEquals(10, filterHelper.GetFilteredServiceCodes().Count);

			filterHelper = new StmServiceTaskFilterHelper(new ZQuery().AddToFilter(StmServiceTaskMetadataColumnProvider.Category, "AAA"), attributes, statusCollection, Factory);
			AssertEquals(1, filterHelper.GetFilteredServiceCodes().Count);

			filterHelper = new StmServiceTaskFilterHelper(new ZQuery().AddToFilter(StmServiceTaskMetadataColumnProvider.MutuallyExclusiveGroup, MutuallyExclusiveServiceTaskGroups.Upgrade), attributes, statusCollection, Factory);
			AssertEquals(1, filterHelper.GetFilteredServiceCodes().Count);

			var filters = new ZQuery();

			filters.AddToFilter(new ZQuery(StmServiceTaskStatusColumnProvider.NextRunTime, ZDateTime.UtcNow.AddMinutes(2)));
			filterHelper = new StmServiceTaskFilterHelper(filters, attributes, statusCollection, Factory);
			AssertEquals(8, filterHelper.GetFilteredServiceCodes().Count);

			filters.AddToFilter(new ZQuery(StmServiceTaskStatusColumnProvider.ProcessId, "ProcessId"));
			filterHelper = new StmServiceTaskFilterHelper(filters, attributes, statusCollection, Factory);
			AssertEquals(7, filterHelper.GetFilteredServiceCodes().Count);

			filters.AddToFilter(new ZQuery(StmServiceTaskStatusColumnProvider.StatusString, "StatusString"));
			filterHelper = new StmServiceTaskFilterHelper(filters, attributes, statusCollection, Factory);
			AssertEquals(5, filterHelper.GetFilteredServiceCodes().Count);

			filters.AddToFilter(new ZQuery(StmServiceTaskStatusColumnProvider.BindingsCount, 1));
			filterHelper = new StmServiceTaskFilterHelper(filters, attributes, statusCollection, Factory);
			AssertEquals(3, filterHelper.GetFilteredServiceCodes().Count);

			filters.AddToFilter(new ZQuery(StmServiceTaskStatusColumnProvider.BindingTypes, "BindingTypes"));
			filterHelper = new StmServiceTaskFilterHelper(filters, attributes, statusCollection, Factory);
			AssertEquals(1, filterHelper.GetFilteredServiceCodes().Count);
		}

		public void TestCustomTextFilterNameConstants()
		{
			var statusColumns = StmServiceTaskCustomFilterNameConstants.ServiceStatusCustomFilterCollection;
			AssertEquals(13, statusColumns.Count);
			AssertContainsDataColumn<string>(statusColumns, StmServiceTaskCustomFilterNameConstants.RegisteredOnHosts);
			AssertContainsDataColumn<string>(statusColumns, StmServiceTaskCustomFilterNameConstants.ProcessId);
			AssertContainsDataColumn<string>(statusColumns, StmServiceTaskCustomFilterNameConstants.SecondsInQueue);
			AssertContainsDataColumn<string>(statusColumns, StmServiceTaskCustomFilterNameConstants.SecondsRunning);
			AssertContainsDataColumn<string>(statusColumns, StmServiceTaskCustomFilterNameConstants.PlaceInQueue);
			AssertContainsDataColumn<int>(statusColumns, StmServiceTaskCustomFilterNameConstants.RunningCount);
			AssertContainsDataColumn<string>(statusColumns, StmServiceTaskCustomFilterNameConstants.StatusString);
			AssertContainsDataColumn<string>(statusColumns, StmServiceTaskCustomFilterNameConstants.BindingTypes);
			AssertContainsDataColumn<int>(statusColumns, StmServiceTaskCustomFilterNameConstants.BindingsCount);
			AssertContainsDataColumn<DateTime>(statusColumns, StmServiceTaskCustomFilterNameConstants.NextRunTime);
			AssertContainsDataColumn<DateTime>(statusColumns, StmServiceTaskCustomFilterNameConstants.LastRunTime);
			AssertContainsDataColumn<DateTime>(statusColumns, StmServiceTaskCustomFilterNameConstants.LastErrorTime);
			AssertContainsDataColumn<int>(statusColumns, StmServiceTaskCustomFilterNameConstants.ErrorCountLast24Hours);

			var metadataColumns = StmServiceTaskCustomFilterNameConstants.MetadataCustomFilterCollection;
			AssertEquals(3, metadataColumns.Count);
			AssertContainsDataColumn<string>(metadataColumns, StmServiceTaskCustomFilterNameConstants.Category);
			AssertContainsDataColumn<string>(metadataColumns, StmServiceTaskCustomFilterNameConstants.Description);
			AssertContainsDataColumn<string>(metadataColumns, StmServiceTaskCustomFilterNameConstants.MutuallyExclusiveGroup);
		}

		void AssertContainsDataColumn<T>(List<DataColumn> dataColumns, string columnName)
		{
			var columns = dataColumns.Where((dc) => string.Equals(dc.ColumnName, columnName, StringComparison.OrdinalIgnoreCase));
			var dataType = typeof(T);
			AssertEquals(1, columns.Count());
			AssertEquals(dataType, columns.First().DataType);
		}
	}
}
