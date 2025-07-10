using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;
using static Enterprise.ZArchitecture.Business.ModuleNumberRangeFilter;

namespace Enterprise.ServiceManager.Module.Testing
{
	[TestedType(typeof(StmServiceTaskFilterBusinessObject))]
	class StmServiceTaskFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			nextRunTime = ZDateTimeOffset.Now;
		}

		public void TestCustomizedStatusFilter()
		{
			var task1a = CreateTask("T1A", true);
			var task1b = CreateTask("T1B", false);
			var task1c = CreateTask("T1C", true);
			var task2a = CreateTask("T2A", false);
			var task2b = CreateTask("T2B", true);
			Factory.Save();

			using (ObjectFactory.Substitute(Mock.Of<IClientHostedServiceAttributeProvider>(provider =>
					   provider.GetClientHostedServiceAttributes() == MetadataAttributes)))
			{
				var collection = new StmServiceTaskCollection(Factory, ServiceTaskScheduleStatusProvider);
				var filterBizObj = new StmServiceTaskFilterBusinessObject();

				var statusFilter = (ModuleTextFilter)filterBizObj["Status"];
				var codeFilter = (ModuleTextFilter)filterBizObj["Code"];
				var registeredOnHostsFilter = (ModuleTextFilter)filterBizObj["RegisteredOnHosts"];
				var bindingTypesFilter = (ModuleTextFilter)filterBizObj["BindingTypes"];
				var processIdFilter = (ModuleTextFilter)filterBizObj["ProcessID"];
				var secondsInQueueFilter = (ModuleTextFilter)filterBizObj["SecondsInQueue"];
				var secondsRunningFilter = (ModuleTextFilter)filterBizObj["SecondsRunning"];
				var placeInQueueFilter = (ModuleTextFilter)filterBizObj["PlaceInQueue"];
				var bindingsCountFilter = (ModuleNumberRangeFilter)filterBizObj["BindingsCount"];
				var runningCountFilter = (ModuleNumberRangeFilter)filterBizObj["RunningCount"];
				var branchFilter = filterBizObj["Branch"];
				var nextRunTimeFilter = (ModuleDateFilter)filterBizObj["Next Run Time"];
				var lastRunTimeFilter = (ModuleDateFilter)filterBizObj["Last Run Time"];
				var lastErrorTimeFilter = (ModuleDateFilter)filterBizObj["Last Error Time"];
				var errorCountLast24HoursFilter = (ModuleNumberRangeFilter)filterBizObj["ErrorCountLast24Hours"];

				AssertEquals(statusFilter.IsOrCategoryReadOnly, false);
				AssertEquals(statusFilter.IsGroupOrCategoryReadOnly, false);
				AssertEquals(statusFilter.MaxLength, StatusColumnProvider.StatusString.MaxLength);
				AssertEquals(codeFilter.IsOrCategoryReadOnly, false);
				AssertEquals(codeFilter.IsGroupOrCategoryReadOnly, false);
				AssertEquals(codeFilter.MaxLength, StmServiceTaskSchema.SST_ServiceTaskCode.MaxLength);
				AssertEquals(registeredOnHostsFilter.IsOrCategoryReadOnly, false);
				AssertEquals(registeredOnHostsFilter.IsGroupOrCategoryReadOnly, false);
				AssertEquals(statusFilter.MaxLength, StatusColumnProvider.RegisteredOnHosts.MaxLength);
				AssertEquals(bindingTypesFilter.IsOrCategoryReadOnly, false);
				AssertEquals(bindingTypesFilter.IsGroupOrCategoryReadOnly, false);
				AssertEquals(bindingTypesFilter.MaxLength, StatusColumnProvider.BindingTypes.MaxLength);
				AssertEquals(processIdFilter.IsOrCategoryReadOnly, false);
				AssertEquals(processIdFilter.IsGroupOrCategoryReadOnly, false);
				AssertEquals(processIdFilter.MaxLength, StatusColumnProvider.ProcessId.MaxLength);
				AssertEquals(secondsInQueueFilter.IsOrCategoryReadOnly, false);
				AssertEquals(secondsInQueueFilter.IsGroupOrCategoryReadOnly, false);
				AssertEquals(secondsInQueueFilter.MaxLength, StatusColumnProvider.SecondsInQueue.MaxLength);
				AssertEquals(secondsRunningFilter.IsOrCategoryReadOnly, false);
				AssertEquals(secondsRunningFilter.IsGroupOrCategoryReadOnly, false);
				AssertEquals(secondsRunningFilter.MaxLength, StatusColumnProvider.SecondsRunning.MaxLength);
				AssertEquals(placeInQueueFilter.IsOrCategoryReadOnly, false);
				AssertEquals(placeInQueueFilter.IsGroupOrCategoryReadOnly, false);
				AssertEquals(placeInQueueFilter.MaxLength, StatusColumnProvider.PlaceInQueue.MaxLength);
				AssertEquals(bindingsCountFilter.IsOrCategoryReadOnly, false);
				AssertEquals(bindingsCountFilter.IsGroupOrCategoryReadOnly, false);
				AssertEquals(runningCountFilter.IsOrCategoryReadOnly, false);
				AssertEquals(runningCountFilter.IsGroupOrCategoryReadOnly, false);
				AssertEquals(branchFilter.IsOrCategoryReadOnly, false);
				AssertEquals(branchFilter.IsGroupOrCategoryReadOnly, false);
				AssertEquals(nextRunTimeFilter.IsOrCategoryReadOnly, false);
				AssertEquals(nextRunTimeFilter.IsGroupOrCategoryReadOnly, false);
				AssertEquals(nextRunTimeFilter.ConvertFromLocalToUTC, true);
				AssertEquals(lastRunTimeFilter.IsOrCategoryReadOnly, false);
				AssertEquals(lastRunTimeFilter.IsGroupOrCategoryReadOnly, false);
				AssertEquals(lastRunTimeFilter.ConvertFromLocalToUTC, true);
				AssertEquals(lastErrorTimeFilter.IsOrCategoryReadOnly, false);
				AssertEquals(lastErrorTimeFilter.IsGroupOrCategoryReadOnly, false);
				AssertEquals(lastErrorTimeFilter.ConvertFromLocalToUTC, true);
				AssertEquals(errorCountLast24HoursFilter.IsOrCategoryReadOnly, false);
				AssertEquals(errorCountLast24HoursFilter.IsGroupOrCategoryReadOnly, false);

				statusFilter.Property = "idle";
				statusFilter.IsActive = true;
				collection.Load(new ZQuery(filterBizObj.Filter));
				AssertEquals(collection.Count, 3);
				Assert(collection.Contains(task1b.PK));
				Assert(collection.Contains(task1c.PK));
				Assert(collection.Contains(task2b.PK));
				statusFilter.IsActive = false;

				registeredOnHostsFilter.IsActive = true;
				registeredOnHostsFilter.Property = "host1";
				collection.Load(new ZQuery(filterBizObj.Filter));
				AssertEquals(collection.Count, 3);
				Assert(collection.Contains(task1a.PK));
				Assert(collection.Contains(task1b.PK));
				Assert(collection.Contains(task1c.PK));
				registeredOnHostsFilter.IsActive = false;

				placeInQueueFilter.IsActive = true;
				placeInQueueFilter.Property = "host2";
				collection.Load(new ZQuery(filterBizObj.Filter));
				AssertEquals(collection.Count, 2);
				Assert(collection.Contains(task2a.PK));
				Assert(collection.Contains(task2b.PK));
				placeInQueueFilter.IsActive = false;

				secondsRunningFilter.IsActive = true;
				secondsRunningFilter.Property = "host2";
				collection.Load(new ZQuery(filterBizObj.Filter));
				AssertEquals(collection.Count, 2);
				Assert(collection.Contains(task2a.PK));
				Assert(collection.Contains(task2b.PK));
				secondsRunningFilter.IsActive = false;

				processIdFilter.IsActive = true;
				processIdFilter.Property = "host2";
				collection.Load(new ZQuery(filterBizObj.Filter));
				AssertEquals(collection.Count, 2);
				Assert(collection.Contains(task2a.PK));
				Assert(collection.Contains(task2b.PK));
				processIdFilter.IsActive = false;

				bindingTypesFilter.IsActive = true;
				bindingTypesFilter.Property = "EDIInterchange";
				collection.Load(new ZQuery(filterBizObj.Filter));
				AssertEquals(collection.Count, 2);
				Assert(collection.Contains(task1b.PK));
				Assert(collection.Contains(task2b.PK));
				bindingTypesFilter.IsActive = false;

				bindingsCountFilter.IsActive = true;
				bindingsCountFilter.MinValue = 1;
				bindingsCountFilter.PropertySearch = new ZString(SearchTexts.GreaterThanOrEqualTo.GetUnresolvedString());
				collection.Load(new ZQuery(filterBizObj.Filter));
				AssertEquals(collection.Count, 5);
				Assert(collection.Contains(task1a.PK));
				Assert(collection.Contains(task2a.PK));
				Assert(collection.Contains(task2b.PK));

				bindingsCountFilter.MaxValue = 1;
				bindingsCountFilter.PropertySearch = new ZString(SearchTexts.Between.GetUnresolvedString());
				collection.Load(new ZQuery(filterBizObj.Filter));
				AssertEquals(collection.Count, 3);
				Assert(collection.Contains(task1a.PK));
				Assert(collection.Contains(task2a.PK));
				Assert(collection.Contains(task2b.PK));
				bindingsCountFilter.IsActive = false;
			}
		}

		public void TestActiveStatusFilter()
		{
			var task1a = CreateTask("T1A", true);
			var task1b = CreateTask("T1B", false);
			var task1c = CreateTask("T1C", true);
			var task2a = CreateTask("T2A", false);
			var task2b = CreateTask("T2B", true);
			Factory.Save();

			var filterBizObj = new StmServiceTaskFilterBusinessObject();
			var activeStatusFilter = (ModuleTextFilter)filterBizObj["Active Status"];
			var collection = new StmServiceTaskCollection(Factory, ServiceTaskScheduleStatusProvider);

			AssertEquals(activeStatusFilter.IsOrCategoryReadOnly, false);
			AssertEquals(activeStatusFilter.IsGroupOrCategoryReadOnly, false);

			using (ObjectFactory.Substitute(Mock.Of<IClientHostedServiceAttributeProvider>(provider =>
					   provider.GetClientHostedServiceAttributes() == MetadataAttributes)))
			{
				collection.Load(new ZQuery(filterBizObj.Filter));

				Assert(collection.Contains(task1a.PK));
				Assert(collection.Contains(task1b.PK));
				Assert(collection.Contains(task1c.PK));
				Assert(collection.Contains(task2a.PK));
				Assert(collection.Contains(task2b.PK));

				activeStatusFilter.IsActive = true;

				AllLanguages.ForEach(lan =>
				{
					using (Res.TemporarilySwitchLanguage(lan))
					{
						activeStatusFilter.Property = FilterStripBusinessObject.StatusActive;
						collection.Load(new ZQuery(filterBizObj.Filter));
						Assert(collection.Contains(task1a.PK));
						Assert(!collection.Contains(task1b.PK));
						Assert(collection.Contains(task1c.PK));
						Assert(!collection.Contains(task2a.PK));
						Assert(collection.Contains(task2b.PK));

						activeStatusFilter.Property = FilterStripBusinessObject.StatusInactive;
						collection.Load(new ZQuery(filterBizObj.Filter));
						Assert(!collection.Contains(task1a.PK));
						Assert(collection.Contains(task1b.PK));
						Assert(!collection.Contains(task1c.PK));
						Assert(collection.Contains(task2a.PK));
						Assert(!collection.Contains(task2b.PK));

						activeStatusFilter.Property = FilterStripBusinessObject.StatusAll;
						collection.Load(new ZQuery(filterBizObj.Filter));
						Assert(collection.Contains(task1a.PK));
						Assert(collection.Contains(task1b.PK));
						Assert(collection.Contains(task1c.PK));
						Assert(collection.Contains(task2a.PK));
						Assert(collection.Contains(task2b.PK));
					}
				});

				if (ErrorReporter.LastMessageReported.StartsWith("Problem requesting status") && ErrorReporter.TotalErrorCount == 1)
				{
					ErrorReporter.Clear();
				}
			}
		}

		public void TestFiltersMatchFilters_ShouldNotSupportFiltersMatch()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.StmServiceTask))
			{
				var filterBizo = module.FilterBusinessObject;
				var filtersThatActuallySupportFiltersMatch = filterBizo.ModuleFilters.OfType<IModuleFilterWithSelectedFilters>().Where(x => x.AllowedComparisonOperators.Contains(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.FiltersMatch));

				AssertContainsExactElementsInAnyOrder("Filters match doesn't work in the Service Task module because of a bunch of complicated things that happen when a search is performed, so this option should be disabled until someone fixes that."
													+ "If you remove DisableFiltersMatchForAuditFilters and run the ZModuleBasherTest.TestFiltersMatchOperator_ShouldGenerateValidQueries for this module, you will see the errors that would need to be fixed.",
					Array.Empty<string>(), filtersThatActuallySupportFiltersMatch.Select(x => x.Description));
			}
		}

		public void TestCategories_WhenInProductivityWiseMode_ShouldOnlyShowProductivityWiseCategories()
		{
			var filterBizo = new StmServiceTaskFilterBusinessObject();
			Assert("All the categories should be visible in normal mode. SAD!", filterBizo.Categories.Count > 25);

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			filterBizo = new StmServiceTaskFilterBusinessObject();
			AssertContainsExactElementsInAnyOrder("Only the ProductivityWise-compatible service task categories should be included. SAD!", new[] { "ACC", "BI", "BMS", "BP", "DBM", "DDC", "DOC", "ESV", "MAI", "SAL", "SYS", "WFL", "TST" }, filterBizo.Categories.GetAllCodes());
		}

		public void TestCategories_ShouldCheckRegistryForProductivityWiseModeOnlyOnce()
		{
			var filterBizo = new StmServiceTaskFilterBusinessObject();

			using (TestConnection.TrackExecutedCommands())
			{
				_ = filterBizo.Categories;
				AssertContainsExactElementsInAnyOrder("The first call to Categories will require a registry check.", new[] { $@"[{Db.DatabaseName}].[dbo].[DataRegGetValueNOD]
Params
@Name: 'ProductivityWiseModeEnabled'" }, TestConnection.ExecutedCommands.Select(x => x.Trim()));
			}

			using (TestConnection.TrackExecutedCommands())
			{
				_ = filterBizo.Categories;
				AssertContainsExactElementsInAnyOrder("Subsequent calls to Categories should not require a registry check. SAD!", Array.Empty<string>(), TestConnection.ExecutedCommands);
			}
		}

		public void TestMutuallyExclusiveGroupFilters()
		{
			var task1a = CreateTask("T1A", true);
			var task1b = CreateTask("T1B", false);
			var task2a = CreateTask("T2A", true);
			var task2b = CreateTask("T2B", false);

			var hostedServiceConfigMock = new Mock<IHostedServiceAttribute>();

			Factory.Save();
			AssertEquals(Enum.GetNames(typeof(MutuallyExclusiveServiceTaskGroups)).Length, 4);

			var filterBizObj = new StmServiceTaskFilterBusinessObject();
			var groupFilter = (ModuleTextFilter)filterBizObj["Mutually Exclusive Group"];
			var collection = new StmServiceTaskCollection(Factory, ServiceTaskScheduleStatusProvider);
			AssertEquals(groupFilter.IsOrCategoryReadOnly, false);
			AssertEquals(groupFilter.IsGroupOrCategoryReadOnly, false);

			using (ObjectFactory.Substitute(Mock.Of<IClientHostedServiceAttributeProvider>(provider =>
					   provider.GetClientHostedServiceAttributes() == MetadataAttributes)))
			{
				collection.Load(new ZQuery(filterBizObj.Filter));
			}
			Assert(collection.Contains(task1a.PK));
			Assert(collection.Contains(task1b.PK));
			Assert(collection.Contains(task2a.PK));
			Assert(collection.Contains(task2b.PK));

			CombineAssertions(() => {
				TestFilter(task1a, MutuallyExclusiveServiceTaskGroups.NoGroup);
				TestFilter(task1b, MutuallyExclusiveServiceTaskGroups.Upgrade);
				TestFilter(task2a, MutuallyExclusiveServiceTaskGroups.BiEdw);
				TestFilter(task2b, MutuallyExclusiveServiceTaskGroups.BiAudit);
			});

			void TestFilter(StmServiceTask task, MutuallyExclusiveServiceTaskGroups group)
			{
				hostedServiceConfigMock
					.SetupGet(attribute => attribute.MutuallyExclusiveTaskGroup)
					.Returns(group);
				using (ObjectFactory.Substitute(Mock.Of<IClientHostedServiceAttributeProvider>(provider =>
						   provider.GetClientHostedServiceAttribute(It.IsAny<string>()) ==
						   hostedServiceConfigMock.Object)))
				{
					AssertEquals(task.MutuallyExclusiveGroup, new ZString(group.ToString()));
				}
			}
		}

		public void TestMutuallyExclusiveGroupIsNoGroupWhenNull()
		{
			var task = CreateTask("T1A", true);
			Factory.Save();

			var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(o =>
				o.Description == "Dummy Description" && o.Category == "TST" &&
				o.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
				o.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "15minutes"));

			using (ObjectFactory.Substitute(Mock.Of<IClientHostedServiceAttributeProvider>(provider =>
					   provider.GetClientHostedServiceAttribute(It.IsAny<string>()) == hostedServiceConfigMock && provider.GetClientHostedServiceAttributes() == MetadataAttributes)))
			{
				var filterBizObj = new StmServiceTaskFilterBusinessObject();
				var groupFilter = (ModuleTextFilter)filterBizObj["Mutually Exclusive Group"];
				var collection = new StmServiceTaskCollection(Factory, ServiceTaskScheduleStatusProvider);
				AssertEquals(groupFilter.IsOrCategoryReadOnly, false);
				AssertEquals(groupFilter.IsGroupOrCategoryReadOnly, false);

				collection.Load(new ZQuery(filterBizObj.Filter));
				Assert(collection.Contains(task.PK));

				AssertEquals(task.MutuallyExclusiveGroup, new ZString("NoGroup"));
			}
		}

		public void TestProcessHostFilter()
		{
			for (int i = 0; i < 4; i++)
			{
				var host = Factory.New<StmServiceHost>();
				host.SH_HostName = $"host{i}";
			}

			var task1a = CreateTask("T1A", true);
			var task1b = CreateTask("T1B", true);
			var task2a = CreateTask("T2A", true);
			var task2b = CreateTask("T2B", true);

			Factory.Save();

			var mockServiceTaskScheduleStatusProvider = new Mock<IServiceTaskScheduleStatusProvider>();
			mockServiceTaskScheduleStatusProvider
				.Setup(m => m.GetServiceStatus())
				.Returns(
					new Dictionary<string, TaskInstanceStatus>
					{
						{ "T1A", new TaskInstanceStatus { StatusString = "running", PlaceInQueueString = "host1:1", ProcessIDsString = "host1:56", RegisteredOnHosts = "host1;host2;host3;host4", RunningCount = 2, SecondsInQueueString = "host1:25", SecondsRunningString = "host1:3", BindingCount = 1, BindingTypes = "EDIMessage" } },
						{ "T1B", new TaskInstanceStatus { StatusString = "idle", PlaceInQueueString = "host1:2", ProcessIDsString = "host1:78", RegisteredOnHosts = "host2;host3;host4", RunningCount = 2, SecondsInQueueString = "host1:49", SecondsRunningString = "host1:6", BindingCount = 2, BindingTypes = "EDIInterchange" } },
						{ "T2A", new TaskInstanceStatus { StatusString = "running", PlaceInQueueString = "host2:1", ProcessIDsString = "host2:65", RegisteredOnHosts = "host3;host4", RunningCount = 2, SecondsInQueueString = "host2:54", SecondsRunningString = "host2:132", BindingCount = 1, BindingTypes = "EDIMessage" } },
						{ "T2B", new TaskInstanceStatus { StatusString = "idle", PlaceInQueueString = "host2:2", ProcessIDsString = "host2:97", RegisteredOnHosts = "host4", RunningCount = 2, SecondsInQueueString = "host2:64", SecondsRunningString = "host2:46", BindingCount = 1, BindingTypes = "EDIInterchange" } }
					}
				);

			var mockClientHostedServiceAttributeProvider = new Mock<IClientHostedServiceAttributeProvider>();
			mockClientHostedServiceAttributeProvider
				.Setup(p => p.GetClientHostedServiceAttributes())
				.Returns(new List<IHostedServiceAttribute>
				{
					new HostedServiceAttribute { Code = "T1A", Description = "Description T1A", Category = "TST", MutuallyExclusiveTaskGroup = MutuallyExclusiveServiceTaskGroups.NoGroup },
					new HostedServiceAttribute { Code = "T1B", Description = "Description T1B", Category = "TST", MutuallyExclusiveTaskGroup = MutuallyExclusiveServiceTaskGroups.NoGroup },
					new HostedServiceAttribute { Code = "T2A", Description = "Description T2A", Category = "TST", MutuallyExclusiveTaskGroup = MutuallyExclusiveServiceTaskGroups.NoGroup },
					new HostedServiceAttribute { Code = "T2B", Description = "Description T2B", Category = "TST", MutuallyExclusiveTaskGroup = MutuallyExclusiveServiceTaskGroups.NoGroup }
				}
				);

			using (ObjectFactory.Substitute(mockClientHostedServiceAttributeProvider.Object))
			{
				var filterBizObj = new StmServiceTaskFilterBusinessObject();
				var processHostFilter = (ModuleTextFilter)filterBizObj["ProcessHost"];
				processHostFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				var collection = new StmServiceTaskCollection(Factory, mockServiceTaskScheduleStatusProvider.Object);

				AssertEquals(processHostFilter.IsOrCategoryReadOnly, false);
				AssertEquals(processHostFilter.IsGroupOrCategoryReadOnly, false);

				collection.Load(new ZQuery(filterBizObj.Filter));
				AssertContainsExactElementsInAnyOrder(new[] { task1a, task1b, task2a, task2b }, collection);

				processHostFilter.IsActive = true;
				processHostFilter.Property = "";
				collection.Load(new ZQuery(filterBizObj.Filter));
				AssertContainsExactElementsInAnyOrder(new[] { task1a, task1b, task2a, task2b }, collection);

				processHostFilter.Property = "host4";
				collection.Load(new ZQuery(filterBizObj.Filter));
				AssertContainsExactElementsInAnyOrder(new[] { task1a, task1b, task2a, task2b }, collection);

				processHostFilter.Property = "host3";
				collection.Load(new ZQuery(filterBizObj.Filter));
				AssertContainsExactElementsInAnyOrder(new[] { task1a, task1b, task2a }, collection);

				processHostFilter.Property = "host2";
				collection.Load(new ZQuery(filterBizObj.Filter));
				AssertContainsExactElementsInAnyOrder(new[] { task1a, task1b }, collection);

				processHostFilter.Property = "host1";
				collection.Load(new ZQuery(filterBizObj.Filter));
				AssertContainsExactElementsInAnyOrder(new[] { task1a }, collection);

				processHostFilter.Property = "host100";
				collection.Load(new ZQuery(filterBizObj.Filter));
				Assert(!collection.Any());
			}

			StmServiceTask CreateTask(ZString scheduleType, ZBool isActive)
			{
				var task = Factory.New<StmServiceTask>();
				task.SST_ServiceTaskCode = scheduleType;
				task.SST_Active = isActive;
				task.SST_NextRunTime = (ZDateTime.Now + TimeSpan.FromDays(1)).ToOffset();

				return task;
			}
		}

		static IServiceTaskScheduleStatusProvider ServiceTaskScheduleStatusProvider
		{
			get
			{
				var instanceStatus = new Dictionary<string, TaskInstanceStatus>
				{
					{ "T1A", new TaskInstanceStatus { StatusString = "running", PlaceInQueueString = "host1:1", ProcessIDsString = "host1:56", RegisteredOnHosts = "host1", RunningCount = 2, SecondsInQueueString = "host1:25", SecondsRunningString = "host1:3", BindingCount = 1, BindingTypes = "EDIMessage" } },
					{ "T1B", new TaskInstanceStatus { StatusString = "idle", PlaceInQueueString = "host1:2", ProcessIDsString = "host1:78", RegisteredOnHosts = "host1", RunningCount = 2, SecondsInQueueString = "host1:49", SecondsRunningString = "host1:6", BindingCount = 2, BindingTypes = "EDIInterchange" } },
					{ "T1C", new TaskInstanceStatus { StatusString = "idle", PlaceInQueueString = "host1:3", ProcessIDsString = "host1:97", RegisteredOnHosts = "host1", RunningCount = 2, SecondsInQueueString = "host1:534", SecondsRunningString = "host1:8", BindingCount = 2, BindingTypes = "EDIMessage" } },
					{ "T2A", new TaskInstanceStatus { StatusString = "running", PlaceInQueueString = "host2:1", ProcessIDsString = "host2:65", RegisteredOnHosts = "host2", RunningCount = 2, SecondsInQueueString = "host2:54", SecondsRunningString = "host2:132", BindingCount = 1, BindingTypes = "EDIMessage" } },
					{ "T2B", new TaskInstanceStatus { StatusString = "idle", PlaceInQueueString = "host2:2", ProcessIDsString = "host2:97", RegisteredOnHosts = "host2", RunningCount = 2, SecondsInQueueString = "host2:64", SecondsRunningString = "host2:46", BindingCount = 1, BindingTypes = "EDIInterchange" } }
				};

				return Mock.Of<IServiceTaskScheduleStatusProvider>(o => o.GetServiceStatus() == instanceStatus);
			}
		}

		static IEnumerable<IHostedServiceAttribute> MetadataAttributes
		{
			get
			{
				return new[]
					{
						"T1A",
						"T1B",
						"T1C",
						"T2A",
						"T2B",
					}
					.Select(c =>
						Mock.Of<IHostedServiceAttribute>(o =>
							o.Code == c &&
							o.Description == "Dummy Description" &&
							o.Category == "TST" &&
							o.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
							o.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "15minutes")));
			}
		}

		StmServiceTask CreateTask(ZString scheduleType, ZBool isActive)
		{
			var task = Factory.New<StmServiceTask>();
			task.SST_ServiceTaskCode = scheduleType;
			task.SST_Active = isActive;
			task.SST_NextRunTime = nextRunTime;
			return task;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new StmServiceTaskFilterBusinessObject();
		}

		ZDateTimeOffset nextRunTime;
	}
}
