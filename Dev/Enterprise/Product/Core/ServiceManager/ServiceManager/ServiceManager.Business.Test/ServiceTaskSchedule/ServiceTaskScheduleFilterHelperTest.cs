using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	class ServiceTaskScheduleFilterHelperTest : TestCase
	{
		public void TestDecompositeFilters()
		{
			var filters = new ZQuery();
			filters.AddToFilter(new ZQuery(StmScheduleTaskSchema.S5_ScheduleType, "S5_ScheduleType"));
			filters.AddToFilter(new ZQuery(StmScheduleTaskSchema.S5_ScheduleDescription, "S5_ScheduleDescription"));
			filters.AddToFilter(new ZQuery(StmScheduleTaskSchema.S5_TypeOfDocument, "S5_TypeOfDocument"));
			filters.AddToFilter(new ZQuery(StmScheduleTaskSchema.S5_GB, Guid.Empty));

			filters.AddToFilter(new ZQuery(StatusColumnProvider.PlaceInQueue, "PlaceInQueue"));
			filters.AddToFilter(new ZQuery(StatusColumnProvider.SecondsRunning, "SecondsRunning"));
			filters.AddToFilter(new ZQuery(StatusColumnProvider.SecondsInQueue, "SecondsInQueue"));
			filters.AddToFilter(new ZQuery(StatusColumnProvider.RegisteredOnHosts, "RegisteredOnHosts"));
			filters.AddToFilter(new ZQuery(StatusColumnProvider.ProcessId, "ProcessId"));
			filters.AddToFilter(new ZQuery(StatusColumnProvider.StatusString, "StatusString"));

			filters.AddToFilter(new ZQuery(StatusColumnProvider.BindingsCount, 1));
			filters.AddToFilter(new ZQuery(StatusColumnProvider.BindingTypes, "BindingTypes"));
			filters.AddToFilter(new ZQuery(StatusColumnProvider.NextRunTime, DateTime.UtcNow));
			filters.AddToFilter(new ZQuery(StatusColumnProvider.LastRunTime, DateTime.UtcNow));
			filters.AddToFilter(new ZQuery(StatusColumnProvider.LastErrorTime, DateTime.UtcNow));
			filters.AddToFilter(new ZQuery(StatusColumnProvider.ErrorCountLast24Hours, 0));

			var decompositedFilters = new ServiceTaskScheduleFilterHelper(filters, new Dictionary<string, TaskInstanceStatus>(), new BusinessObjectFactory());
			AssertEquals(11, decompositedFilters.ServiceStatusFilters.Params.Length);
			AssertEquals(5, decompositedFilters.TableSchemaFilters.Params.Length);
			AssertNotNull(decompositedFilters.TableSchemaFilters.Params.FirstOrDefault((par) => string.Equals(par.SchemaColumn.Name, StatusColumnProvider.NextRunTime.Name, StringComparison.OrdinalIgnoreCase)));

			var dummyStatusCollection = new Dictionary<string, TaskInstanceStatus>();
			dummyStatusCollection.Add("BLA", new TaskInstanceStatus());
			decompositedFilters = new ServiceTaskScheduleFilterHelper(filters, dummyStatusCollection, new BusinessObjectFactory());
			AssertEquals(12, decompositedFilters.ServiceStatusFilters.Params.Length);
			AssertEquals(4, decompositedFilters.TableSchemaFilters.Params.Length);
			AssertNotNull(decompositedFilters.ServiceStatusFilters.Params.FirstOrDefault((par) => string.Equals(par.SchemaColumn.Name, StatusColumnProvider.NextRunTime.Name, StringComparison.OrdinalIgnoreCase)));

			decompositedFilters = new ServiceTaskScheduleFilterHelper(new ZQuery(), new Dictionary<string, TaskInstanceStatus>(), new BusinessObjectFactory());
			AssertEquals(0, decompositedFilters.ServiceStatusFilters.Params.Length);
			AssertEquals(0, decompositedFilters.TableSchemaFilters.Params.Length);
		}

		public void TestHasDateFilters()
		{
			var filters = new ZQuery();
			filters.AddToFilter(new ZQuery(StatusColumnProvider.LastRunTime, SQLComparisonOperator.Equal, null));
			filters.AddToFilter(new ZQuery(StatusColumnProvider.LastErrorTime, SQLComparisonOperator.NotEqual, null));

			var decompositedFilters = new ServiceTaskScheduleFilterHelper(filters, new Dictionary<string, TaskInstanceStatus>(), new BusinessObjectFactory());
			AssertEquals(string.Empty, decompositedFilters.TableSchemaFilters.ParameterisedText.LiteralTextSql);
			AssertEquals(filters.ParameterisedText.LiteralTextSql, decompositedFilters.ServiceStatusFilters.ParameterisedText.LiteralTextSql);
		}

		[TestDate(2016, 4, 29, 0, 0, 0)]
		public void TestGetFilteredServiceCodes()
		{
			var time1 = ZDateTime.UtcNow;
			var time2 = time1.AddMinutes(2);
			var statusCollection = new Dictionary<string, TaskInstanceStatus>()
			{
				{ "Code1", new TaskInstanceStatus() { ProcessIDsString = "ProcessId", StatusString = "StatusString", BindingTypes = "BindingTypes", NextRunTime = time2.ToDateTime() }  },
				{ "Code2", new TaskInstanceStatus() { ProcessIDsString = "ProcessId", BindingTypes = "BindingTypes", NextRunTime = time2.ToDateTime() } },
				{ "Code3", new TaskInstanceStatus() { ProcessIDsString = "ProcessId", StatusString = "StatusString", BindingTypes = "BindingTypes", NextRunTime = time2.ToDateTime() } },
				{ "Code4", new TaskInstanceStatus() { ProcessIDsString = "ProcessId", StatusString = "StatusString", NextRunTime = time2.ToDateTime() } },
				{ "Code5", new TaskInstanceStatus() { StatusString = "StatusString", BindingTypes = "BindingTypes", NextRunTime = time1.ToDateTime() } },
				{ "Code6", new TaskInstanceStatus() { ProcessIDsString = "ProcessId", BindingTypes = "BindingTypes", NextRunTime = time2.ToDateTime() } },
				{ "Code7", new TaskInstanceStatus() { ProcessIDsString = "ProcessId", StatusString = "StatusString", BindingTypes = "BindingTypes", NextRunTime = time2.ToDateTime() } },
				{ "Code8", new TaskInstanceStatus() { ProcessIDsString = "ProcessId", StatusString = "StatusString", NextRunTime = time2.ToDateTime() } },
				{ "Code9", new TaskInstanceStatus() { ProcessIDsString = "test", StatusString = "StatusString", BindingTypes = "BindingTypes", NextRunTime = time2.ToDateTime() } },
				{ "Code10", new TaskInstanceStatus() { ProcessIDsString = "ProcessId", StatusString = "test", BindingTypes = "BindingTypes", NextRunTime = time2.ToDateTime() } }
			};

			var filters = new ZQuery();
			filters.AddToFilter(new ZQuery(StatusColumnProvider.NextRunTime, ZDateTime.UtcNow.AddMinutes(2)));
			var filterHelper = new ServiceTaskScheduleFilterHelper(filters, statusCollection, new BusinessObjectFactory());
			AssertEquals(9, filterHelper.GetFilteredServiceCodes().Count);

			filters.AddToFilter(new ZQuery(StatusColumnProvider.ProcessId, "ProcessId"));
			filterHelper = new ServiceTaskScheduleFilterHelper(filters, statusCollection, new BusinessObjectFactory());
			AssertEquals(8, filterHelper.GetFilteredServiceCodes().Count);

			filters.AddToFilter(new ZQuery(StatusColumnProvider.StatusString, "StatusString"));
			filterHelper = new ServiceTaskScheduleFilterHelper(filters, statusCollection, new BusinessObjectFactory());
			AssertEquals(5, filterHelper.GetFilteredServiceCodes().Count);

			filters.AddToFilter(new ZQuery(StatusColumnProvider.BindingTypes, "BindingTypes"));
			filterHelper = new ServiceTaskScheduleFilterHelper(filters, statusCollection, new BusinessObjectFactory());
			AssertEquals(3, filterHelper.GetFilteredServiceCodes().Count);

			filterHelper = new ServiceTaskScheduleFilterHelper(new ZQuery(), new Dictionary<string, TaskInstanceStatus>(), new BusinessObjectFactory());
			AssertEquals(0, filterHelper.GetFilteredServiceCodes().Count);
		}

		class FilterHelperTestsWithFactory : TestCaseWithFactory
		{
			public void TestGetFilteredServiceCodes_CorrectlyFiltersBindingCount_GreaterThanOrEqualTo_1Binding() => AssertFilteredServiceCodesCount(new ZQuery(StatusColumnProvider.BindingsCount, SQLComparisonOperator.GreaterThanOrEqualTo, 1), 4);
			public void TestGetFilteredServiceCodes_CorrectlyFiltersBindingCount_GreaterThanOrEqualTo_2Bindings() => AssertFilteredServiceCodesCount(new ZQuery(StatusColumnProvider.BindingsCount, SQLComparisonOperator.GreaterThanOrEqualTo, 2), 2);
			public void TestGetFilteredServiceCodes_CorrectlyFiltersBindingCount_GreaterThanOrEqualTo_3Bindings() => AssertFilteredServiceCodesCount(new ZQuery(StatusColumnProvider.BindingsCount, SQLComparisonOperator.GreaterThanOrEqualTo, 3), 1);
			public void TestGetFilteredServiceCodes_CorrectlyFiltersBindingCount_LessThanOrEqualTo_1Binding() => AssertFilteredServiceCodesCount(new ZQuery(StatusColumnProvider.BindingsCount, SQLComparisonOperator.LessThanOrEqualTo, 1), 2);
			public void TestGetFilteredServiceCodes_CorrectlyFiltersBindingCount_LessThanOrEqualTo_2Binding() => AssertFilteredServiceCodesCount(new ZQuery(StatusColumnProvider.BindingsCount, SQLComparisonOperator.LessThanOrEqualTo, 2), 3);
			public void TestGetFilteredServiceCodes_CorrectlyFiltersBindingCount_LessThanOrEqualTo_3Bindings() => AssertFilteredServiceCodesCount(new ZQuery(StatusColumnProvider.BindingsCount, SQLComparisonOperator.LessThanOrEqualTo, 3), 4);
			public void TestGetFilteredServiceCodes_CorrectlyFiltersBindingCount_EqualTo_1Binding() => AssertFilteredServiceCodesCount(new ZQuery(StatusColumnProvider.BindingsCount, SQLComparisonOperator.Equal, 1), 2);
			public void TestGetFilteredServiceCodes_CorrectlyFiltersBindingCount_EqualTo_3Binding() => AssertFilteredServiceCodesCount(new ZQuery(StatusColumnProvider.BindingsCount, SQLComparisonOperator.Equal, 3), 1);
			public void TestGetFilteredServiceCodes_CorrectlyFiltersBindingCount_EqualTo_5Binding() => AssertFilteredServiceCodesCount(new ZQuery(StatusColumnProvider.BindingsCount, SQLComparisonOperator.Equal, 5), 0);

			void AssertFilteredServiceCodesCount(ZQuery filterQuery, int expectedBindingsCount)
			{
				//Arrange
				var statusCollection =
					Enumerable.Range(1, 4)
					.ToDictionary(x => $"CD{x}",
						x =>
						{
							return new TaskInstanceStatus() { ProcessIDsString = "ProcessId", StatusString = "StatusString", BindingCount = 1, BindingTypes = "dummybindingtypes", NextRunTime = ZDateTime.UtcNow.ToDateTime() };
							//The actual values of the status dop
						});

				var provider = new Mock<IHostedServiceBusinessObjectBindingsProvider>();
				provider.Setup(p => p.BusinessObjectBindings).Returns(new List<HostedServiceBusinessObjectBindingAttribute>
				{
					new HostedServiceBusinessObjectBindingAttribute("CD1", "dummyTable1", Array.Empty<string>(), "dummyqueuename"),
					new HostedServiceBusinessObjectBindingAttribute("CD1", "dummyTable2", Array.Empty<string>(), "dummyqueuename"),
					new HostedServiceBusinessObjectBindingAttribute("CD1", "dummyTable3", Array.Empty<string>(), "dummyqueuename"),
					new HostedServiceBusinessObjectBindingAttribute("CD2", "dummyTable4", Array.Empty<string>(), "dummyqueuename"),
					new HostedServiceBusinessObjectBindingAttribute("CD2", "dummyTable5", Array.Empty<string>(), "dummyqueuename"),
					new HostedServiceBusinessObjectBindingAttribute("CD3", "dummyTable6", Array.Empty<string>(), "dummyqueuename"),
					new HostedServiceBusinessObjectBindingAttribute("CD4", "dummyTable7", Array.Empty<string>(), "dummyqueuename")
				});

				using (ObjectFactory.Substitute(provider.Object))
				{
					for (int i = 1; i <= 4; i++)
					{
						var serviceTaskSchedule = Factory.New<ServiceTaskSchedule>();
						serviceTaskSchedule.S5_ScheduleType = $"CD{i}";
					}

					Factory.Save();

					var filters = new ZQuery();
					filters.AddToFilter(filterQuery);
					var filterHelper = new ServiceTaskScheduleFilterHelper(filters, statusCollection, Factory);

					//Act
					var filteredServiceCodes = filterHelper.GetFilteredServiceCodes();

					//Assert
					AssertEquals(expectedBindingsCount, filteredServiceCodes.Count);
				}
			}
		}

		public void TestCustomTextFilterNameConstants()
		{
			var columns = CustomTextFilterNameConstants.GetServiceStatusCustomFilterCollection();
			AssertEquals(14, columns.Count);
			AssertContainsDataColumn(columns, CustomTextFilterNameConstants.RegisteredOnHosts, typeof(string));
			AssertContainsDataColumn(columns, CustomTextFilterNameConstants.ProcessId, typeof(string));
			AssertContainsDataColumn(columns, CustomTextFilterNameConstants.SecondsInQueue, typeof(string));
			AssertContainsDataColumn(columns, CustomTextFilterNameConstants.SecondsRunning, typeof(string));
			AssertContainsDataColumn(columns, CustomTextFilterNameConstants.PlaceInQueue, typeof(string));
			AssertContainsDataColumn(columns, CustomTextFilterNameConstants.RunningCount, typeof(int));
			AssertContainsDataColumn(columns, CustomTextFilterNameConstants.StatusString, typeof(string));
			AssertContainsDataColumn(columns, CustomTextFilterNameConstants.BindingTypes, typeof(string));
			AssertContainsDataColumn(columns, CustomTextFilterNameConstants.BindingsCount, typeof(int));
			AssertContainsDataColumn(columns, CustomTextFilterNameConstants.NextRunTime, typeof(DateTime));
			AssertContainsDataColumn(columns, CustomTextFilterNameConstants.LastRunTime, typeof(DateTime));
			AssertContainsDataColumn(columns, CustomTextFilterNameConstants.LastErrorTime, typeof(DateTime));
			AssertContainsDataColumn(columns, CustomTextFilterNameConstants.ErrorCountLast24Hours, typeof(int));
			AssertContainsDataColumn(columns, CustomTextFilterNameConstants.MutuallyExclusiveGroup, typeof(string));
		}

		void AssertContainsDataColumn(List<DataColumn> dataColumns, string columnName, Type dataType)
		{
			var columns = dataColumns.Where((dc) => string.Equals(dc.ColumnName, columnName, StringComparison.OrdinalIgnoreCase));
			AssertEquals(1, columns.Count());
			AssertEquals(dataType, columns.First().DataType);
		}
	}
}
