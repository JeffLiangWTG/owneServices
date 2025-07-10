using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(ServiceTaskScheduleCollection))]
	class ServiceTaskScheduleCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestLoadWithNullParent()
		{
			CleanUpServiceTasks();
			var schedule = Factory.New<ServiceTaskSchedule>();
			var collection = new ServiceTaskScheduleCollection(Factory, ServiceTaskScheduleCollection.RemoteStatus.WithoutStatus);
			collection.Load();
			AssertEquals(1, collection.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ServiceTaskScheduleCollection(Factory, ServiceTaskScheduleCollection.RemoteStatus.WithStatus);
		}

		void CleanUpServiceTasks()
		{
			var cleanupTasks = Factory.Load<StmScheduleTask>(new ZQuery());
			foreach (var task in cleanupTasks)
			{
				task.Delete();
			}
		}

		public void TestBindingsCountAndType()
		{
			using (ObjectFactory.Substitute(Mock.Of<IHostedServiceBusinessObjectBindingsProvider>(provider =>
				provider.BusinessObjectBindings == new[]
			{
				new HostedServiceBusinessObjectBindingAttribute("111", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("222", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("333", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("111", "DummyBizo2", Array.Empty<string>(), null)
			})))
			{
				var schemaResolver = new Mock<IApplicationSchemaResolver>();
				schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
				schemaResolver.Setup(x => x.GetTableSchema("DummyBizo2")).Returns(DummyBizoSchema.Instance);
				
				CleanUpServiceTasks();
				var schedule = Factory.New<ServiceTaskSchedule>();
				schedule.S5_ScheduleType = "111";
				var statusProvider = new Mock<IServiceTaskScheduleStatusProvider>();
				statusProvider.Setup(sp => sp.GetServiceStatus()).Returns(new Dictionary<string, TaskInstanceStatus>());
				var collection = new ServiceTaskScheduleCollection(Factory, ServiceTaskScheduleCollection.RemoteStatus.WithStatus, statusProvider.Object);
				collection.Load();

				AssertEquals(2, schedule.ServiceTaskBindingsCount);
				AssertEquals("DummyBizo,DummyBizo2", schedule.ServiceTaskBindingTypesString);
			}
		}

		public void TestLoadWaitForServiceStatus()
		{
			// Arrange
			var timeout = TimeSpan.FromSeconds(5);
			var getServiceStatusIsFinished = false;
			var serviceProviderMock = new Mock<IServiceTaskScheduleStatusProvider>();
			serviceProviderMock
				.Setup(provider => provider.GetServiceStatus())
				.Returns(() =>
				{
					Thread.Sleep(timeout);
					getServiceStatusIsFinished = true;
					return new Dictionary<string, TaskInstanceStatus>();
				});

			CleanUpServiceTasks();
			const string scheduleType = "111";
			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = scheduleType;
			var collection = new ServiceTaskScheduleCollection(Factory, ServiceTaskScheduleCollection.RemoteStatus.WithStatus, serviceProviderMock.Object);

			// Act
			collection.Load();

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals($"{nameof(IServiceTaskScheduleStatusProvider.GetServiceStatus)} should be awaited", true, getServiceStatusIsFinished);
				AssertContainsExactElementsInAnyOrder(new[] { scheduleType }, collection.Select(taskSchedule => taskSchedule.S5_ScheduleType));
			});
		}

		public void TestPopulateStatusData()
		{
			var webStatus = new Dictionary<string, TaskInstanceStatus>
			{
				{ "111", new TaskInstanceStatus() { StatusString = "status", PlaceInQueueString = "place in queue", ProcessIDsString = "process id", RegisteredOnHosts = "registered on hosts", RunningCount = 2, SecondsInQueueString = "seconds in queue", SecondsRunningString = "seconds running", NextRunTime = new DateTime(2020, 3, 8) } }
			};

			var serviceProvider = new Mock<IServiceTaskScheduleStatusProvider>();
			serviceProvider.Setup(x => x.GetServiceStatus()).Returns(webStatus);

			CleanUpServiceTasks();
			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = "111";
			var collection = new ServiceTaskScheduleCollection(Factory, ServiceTaskScheduleCollection.RemoteStatus.WithStatus, serviceProvider.Object);
			collection.Load();

			AssertEquals(collection[0].StatusString, "status");
			AssertEquals(collection[0].PlaceInQueueString, "place in queue");
			AssertEquals(collection[0].ProcessIDsString, "process id");
			AssertEquals(collection[0].RegisteredOnHosts, "registered on hosts");
			AssertEquals(collection[0].RunningCount, 2);
			AssertEquals(collection[0].SecondsInQueueString, "seconds in queue");
			AssertEquals(collection[0].SecondsRunningString, "seconds running");
			AssertEquals(collection[0].S5_NextScheduledPrintRunTimeUtc, new ZDateTime(new DateTime(2020, 3, 8)));
		}

		public void TestPopulateStatusDataFilterByHasDate()
		{
			var webStatus = new Dictionary<string, TaskInstanceStatus>
			{
				{ "111", new TaskInstanceStatus() { StatusString = "status", PlaceInQueueString = "place in queue", ProcessIDsString = "process id", RegisteredOnHosts = "registered on hosts", RunningCount = 2, SecondsInQueueString = "seconds in queue", SecondsRunningString = "seconds running" } },
				{ "222", new TaskInstanceStatus() { StatusString = "status", PlaceInQueueString = "place in queue", ProcessIDsString = "process id", RegisteredOnHosts = "registered on hosts", RunningCount = 2, SecondsInQueueString = "seconds in queue", SecondsRunningString = "seconds running", LastRunTime = new DateTime(2020, 3, 8) } }
			};

			var serviceProvider = new Mock<IServiceTaskScheduleStatusProvider>();
			serviceProvider.Setup(x => x.GetServiceStatus()).Returns(webStatus);

			CleanUpServiceTasks();
			var schedule1 = Factory.New<ServiceTaskSchedule>();
			schedule1.S5_ScheduleType = "111";
			var schedule2 = Factory.New<ServiceTaskSchedule>();
			schedule2.S5_ScheduleType = "222";
			var collection = new ServiceTaskScheduleCollection(Factory, ServiceTaskScheduleCollection.RemoteStatus.WithStatus, serviceProvider.Object);
			var filter1 = new ZQuery();
			filter1.AddToFilter(new ZQuery(StatusColumnProvider.LastRunTime, SQLComparisonOperator.Equal, null));
			collection.Load(filter1);

			AssertEquals(1, collection.Count);
			AssertEquals("111", collection[0].S5_ScheduleType);
			AssertEquals(ZDateTime.Empty, collection[0].LastRunTime);

			var filter2 = new ZQuery();
			filter2.AddToFilter(new ZQuery(StatusColumnProvider.LastRunTime, SQLComparisonOperator.NotEqual, null));
			collection.Load(filter2);

			AssertEquals(1, collection.Count);
			AssertEquals("222", collection[0].S5_ScheduleType);
			AssertEquals(new ZDateTime(new DateTime(2020, 3, 8)), collection[0].LastRunTime);
		}

		public void TestLoad_WithProductivityWiseModeEnabled_ShouldLoadOnlyTasksInProductivityWiseCategories()
		{
			var normalTask = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			normalTask.S5_ScheduleType = "AAA";
			normalTask.S5_TypeOfDocument = "AUC";

			var productivityTask = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			productivityTask.S5_ScheduleType = "BBB";
			productivityTask.S5_TypeOfDocument = "BMS";

			var collection = new ServiceTaskScheduleCollection(Factory, ServiceTaskScheduleCollection.RemoteStatus.WithStatus);
			collection.Load();

			AssertContainsExactElementsInAnyOrder(new[] { "AAA", "BBB" }, collection.Select(x => x.S5_ScheduleType));

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			collection = new ServiceTaskScheduleCollection(Factory, ServiceTaskScheduleCollection.RemoteStatus.WithStatus);
			collection.Load();

			AssertContainsExactElementsInAnyOrder(new[] { "BBB" }, collection.Select(x => x.S5_ScheduleType));
		}

		public void TestLoadWithNoResultQuery()
		{
			var task = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			task.S5_ScheduleType = "AAA";
			task.S5_TypeOfDocument = "AUC";

			var filter = new ZQuery();
			filter.IsNoResultQuery = true;

			var collection = new ServiceTaskScheduleCollection(Factory, ServiceTaskScheduleCollection.RemoteStatus.WithStatus);
			collection.Load(filter);

			AssertEquals("Should have no elements", 0, collection.Count);
		}

		public void TestSortSortsCorrectly_PlaceInQueue()
		{
			var inputNums = new[]
				{
					new Dictionary<string, int[]>
					{
						{ "au2co-sprc-402.wtg.zone" , new[] { 1 } }
					},
					new Dictionary<string, int[]>
					{
						{ "au2co-sprc-402.wtg.zone" , new[] { 2 } }, { "au2co-sprc-403.wtg.zone" , new[] { 3 } }
					},
					new Dictionary<string, int[]>
					{
						{ "au2co-sprc-402.wtg.zone" , new[] { 1 } }, { "au2co-sprc-403.wtg.zone" , new[] { 5 } }
					},
					new Dictionary<string, int[]>
					{
						{ "au2co-sprc-403.wtg.zone" , new[] { 3 } }, { "au2co-sprc-402.wtg.zone" , new[] { 4 } }
					},
					new Dictionary<string, int[]>
					{
						{ "au2co-sprc-402.wtg.zone" , new[] { 9, 1 } }, { "au2co-sprc-403.wtg.zone" , new[] { 8 } }
					}
				};

			var expectedAscending = new[]
				{
					"[au2co-sprc-402.wtg.zone: 1]",
					"[au2co-sprc-402.wtg.zone: 1];[au2co-sprc-403.wtg.zone: 5]",
					"[au2co-sprc-402.wtg.zone: 9,1];[au2co-sprc-403.wtg.zone: 8]",
					"[au2co-sprc-402.wtg.zone: 2];[au2co-sprc-403.wtg.zone: 3]",
					"[au2co-sprc-403.wtg.zone: 3];[au2co-sprc-402.wtg.zone: 4]"
				};

			var expectedDescending = new[]
				{
					"[au2co-sprc-402.wtg.zone: 9,1];[au2co-sprc-403.wtg.zone: 8]",
					"[au2co-sprc-402.wtg.zone: 1];[au2co-sprc-403.wtg.zone: 5]",
					"[au2co-sprc-403.wtg.zone: 3];[au2co-sprc-402.wtg.zone: 4]",
					"[au2co-sprc-402.wtg.zone: 2];[au2co-sprc-403.wtg.zone: 3]",
					"[au2co-sprc-402.wtg.zone: 1]"
				};

			AssertCollectionSortWorks(CustomTextFilterNameConstants.PlaceInQueue + "String", (ServiceTaskSchedule s) => (string)s.PlaceInQueueString, inputNums, expectedAscending, expectedDescending);
		}

		public void TestSortSortsCorrectly_ProcessId()
		{
			var inputNums = new[]
				{
					new Dictionary<string, int[]>
					{
						{ "au2co-sprc-404.wtg.zone" , new[] { 1 } }, { "au2co-sprc-402.wtg.zone" , new[] { 2 } }
					},
					new Dictionary<string, int[]>
					{
						{ "au2co-sprc-404.wtg.zone" , new[] { 4 } }
					},
					new Dictionary<string, int[]>
					{
						{ "au2co-sprc-402.wtg.zone" , new[] { 1, 4, 6 } }, { "au2co-sprc-404.wtg.zone" , new[] { 2, 3, 7 } }
					},
					new Dictionary<string, int[]>
					{
						{ "au2co-sprc-408.wtg.zone" , new[] { 1 } }, { "au2co-sprc-409.wtg.zone" , new[] { 8 } }, { "au2co-sprc-404.wtg.zone" , new[] { 9 } }
					}
				};

			var expectedAscending = new[]
				{
					"[au2co-sprc-404.wtg.zone: 1];[au2co-sprc-402.wtg.zone: 2]",
					"[au2co-sprc-402.wtg.zone: 1,4,6];[au2co-sprc-404.wtg.zone: 2,3,7]",
					"[au2co-sprc-408.wtg.zone: 1];[au2co-sprc-409.wtg.zone: 8];[au2co-sprc-404.wtg.zone: 9]",
					"[au2co-sprc-404.wtg.zone: 4]"
				};

			var expectedDescending = new[]
				{
					"[au2co-sprc-408.wtg.zone: 1];[au2co-sprc-409.wtg.zone: 8];[au2co-sprc-404.wtg.zone: 9]",
					"[au2co-sprc-402.wtg.zone: 1,4,6];[au2co-sprc-404.wtg.zone: 2,3,7]",
					"[au2co-sprc-404.wtg.zone: 4]",
					"[au2co-sprc-404.wtg.zone: 1];[au2co-sprc-402.wtg.zone: 2]"
				};

			AssertCollectionSortWorks(CustomTextFilterNameConstants.ProcessId + "sString", (ServiceTaskSchedule s) => (string)s.ProcessIDsString, inputNums, expectedAscending, expectedDescending);
		}

		public void TestSortSortsCorrectly_SecondsInQueue()
		{
			var inputNums = new[]
				{
					new Dictionary<string, int[]>(),
					new Dictionary<string, int[]>
					{
						{ "au2co-sprc-402.wtg.zone", new[] { 4 } }, { "au2co-sprc-403.wtg.zone", new[] { 5 } }, { "au2co-sprc-404.wtg.zone", new[] { 6 } }
					},
					new Dictionary<string, int[]>
					{
						{ "au2co-sprc-402.wtg.zone", new[] { 1, 9 } }
					},
					new Dictionary<string, int[]>
					{
						{ "au2co-sprc-402.wtg.zone", new[] { 2 } }, { "au2co-sprc-403.wtg.zone", new[] { 9 } }
					}
				};

			var expectedAscending = new[]
				{
					"[au2co-sprc-402.wtg.zone: 1,9]",
					"[au2co-sprc-402.wtg.zone: 2];[au2co-sprc-403.wtg.zone: 9]",
					"[au2co-sprc-402.wtg.zone: 4];[au2co-sprc-403.wtg.zone: 5];[au2co-sprc-404.wtg.zone: 6]",
					""
				};

			var expectedDescending = new[]
				{
					"[au2co-sprc-402.wtg.zone: 2];[au2co-sprc-403.wtg.zone: 9]",
					"[au2co-sprc-402.wtg.zone: 1,9]",
					"[au2co-sprc-402.wtg.zone: 4];[au2co-sprc-403.wtg.zone: 5];[au2co-sprc-404.wtg.zone: 6]",
					""
				};

			AssertCollectionSortWorks(CustomTextFilterNameConstants.SecondsInQueue + "String", (ServiceTaskSchedule s) => (string)s.SecondsInQueueString, inputNums, expectedAscending, expectedDescending);
		}

		public void TestSortSortsCorrectly_SecondsRunning()
		{
			var inputNums = new[]
				{
					new Dictionary<string, int[]> 
					{
						{ "au2co-sprc-402.wtg.zone", new[] { 1 } }
					},
					new Dictionary<string, int[]>
					{
						{ "au2co-sprc-402.wtg.zone", new[] { 2 } }
					},
					new Dictionary<string, int[]>
					{
						{ "au2co-sprc-402.wtg.zone", new[] { 3 } }
					},
					new Dictionary<string, int[]>
					{
						{ "au2co-sprc-402.wtg.zone", new[] { 1, 3, 5, 7, 9 } },
						{ "au2co-sprc-409.wtg.zone", new[] { 6, 8 } },
					}
				};

			var expectedAscending = new[]
				{
					"[au2co-sprc-402.wtg.zone: 1]",
					"[au2co-sprc-402.wtg.zone: 1,3,5,7,9];[au2co-sprc-409.wtg.zone: 6,8]",
					"[au2co-sprc-402.wtg.zone: 2]",
					"[au2co-sprc-402.wtg.zone: 3]"
				};

			var expectedDescending = new[]
				{
					"[au2co-sprc-402.wtg.zone: 1,3,5,7,9];[au2co-sprc-409.wtg.zone: 6,8]",
					"[au2co-sprc-402.wtg.zone: 3]",
					"[au2co-sprc-402.wtg.zone: 2]",
					"[au2co-sprc-402.wtg.zone: 1]"
				};

			AssertCollectionSortWorks(CustomTextFilterNameConstants.SecondsRunning + "String", (ServiceTaskSchedule s) => (string)s.SecondsRunningString, inputNums, expectedAscending, expectedDescending);
		}

		void AssertCollectionSortWorks(string columnName, Func<ServiceTaskSchedule, string> columnSelect, Dictionary<string, int[]>[] inputNums, string[] expectedAscending, string[] expectedDescending)
		{
			//Arrange
			SetUpNumericalStringServiceTasks(inputNums);

			var collection = new ServiceTaskScheduleCollection(Factory, ServiceTaskScheduleCollection.RemoteStatus.WithoutStatus);
			collection.Load();
			Assert(collection.Count > 0);

			//Act - Ascending
			collection.Sort(columnName, System.ComponentModel.ListSortDirection.Ascending);
			var result = collection.Select(columnSelect).ToList();

			//Assert - Ascending
			AssertContainsExactElementsInExactOrder("For Ascending:", expectedAscending, result);

			//Act - Descending
			collection.Sort(columnName, System.ComponentModel.ListSortDirection.Descending);
			result = collection.Select(columnSelect).ToList();

			//Assert - Descending
			AssertContainsExactElementsInExactOrder("For Descending:", expectedDescending, result);
		}

		void SetUpNumericalStringServiceTasks(Dictionary<string, int[]>[] numLists)
		{
			var provider = new Mock<IHostedServiceBusinessObjectBindingsProvider>();
			provider
				.Setup(p => p.BusinessObjectBindings)
				.Returns(Enumerable.Range(1, numLists.Length)
					.Select(x => new HostedServiceBusinessObjectBindingAttribute($"CD{x}", $"DummyTable{x}", Array.Empty<string>(), null)).ToList());

			CleanUpServiceTasks();

			var currentServiceTaskCodeNum = 0;

			foreach (var numList in numLists)
			{
				AddTask(currentServiceTaskCodeNum, numList);

				currentServiceTaskCodeNum++;
			}

			Factory.Save();

			void AddTask(int serviceTaskCodeNum, Dictionary<string, int[]> numList)
			{
				var numericalString = string.Join(";", numList.Select(numListGroup => $"[{numListGroup.Key}: {string.Join(",", numListGroup.Value)}]"));

				var task = Factory.New<ServiceTaskSchedule>();
				task.S5_ScheduleType = $"CD{serviceTaskCodeNum}";
				task.S5_IsActive = true;
				task.S5_ParentTableCode = StmServiceHostSchema.Constants.Prefix;
				task.PlaceInQueueString = numericalString;
				task.ProcessIDsString = numericalString;
				task.SecondsInQueueString = numericalString;
				task.SecondsRunningString = numericalString;
			}
		}
	}
}
