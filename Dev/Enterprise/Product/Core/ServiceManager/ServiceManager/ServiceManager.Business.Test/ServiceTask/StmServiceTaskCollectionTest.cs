using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(StmServiceTaskCollection))]
	class StmServiceTaskCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmServiceTaskCollection(Factory);
		}

		void CleanUpServiceTasks()
		{
			var cleanupTasks = Factory.Load<StmServiceTask>(new ZQuery());
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
				var schedule = Factory.New<StmServiceTask>();
				schedule.SST_ServiceTaskCode = "111";
				var statusProvider = new Mock<IServiceTaskScheduleStatusProvider>();
				statusProvider.Setup(sp => sp.GetServiceStatus()).Returns(new Dictionary<string, TaskInstanceStatus>());
				var collection = new StmServiceTaskCollection(Factory, statusProvider.Object);
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
			var schedule = Factory.New<StmServiceTask>();
			schedule.SST_ServiceTaskCode = scheduleType;
			var collection = new StmServiceTaskCollection(Factory, serviceProviderMock.Object);

			var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(o =>
				o.Code == scheduleType &&
				o.Description == "Dummy Description" &&
				o.Category == "TST" &&
				o.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
				o.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "15minutes"));

			using (ObjectFactory.Substitute(Mock.Of<IClientHostedServiceAttributeProvider>(provider =>
					provider.GetClientHostedServiceAttributes() == new[]
					{
						hostedServiceConfigMock,
					})))
			{
				// Act
				collection.Load();
			}

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals($"{nameof(IServiceTaskScheduleStatusProvider.GetServiceStatus)} should be awaited", true, getServiceStatusIsFinished);
				AssertContainsExactElementsInAnyOrder(new[] { scheduleType }, collection.Select(taskSchedule => taskSchedule.SST_ServiceTaskCode));
			});
		}

		public void TestPopulateStatusData()
		{
			// Arrange
			var webStatus = new Dictionary<string, TaskInstanceStatus>
			{
				{ "111", new TaskInstanceStatus() { StatusString = "status", PlaceInQueueString = "place in queue", ProcessIDsString = "process id", RegisteredOnHosts = "registered on hosts", RunningCount = 2, SecondsInQueueString = "seconds in queue", SecondsRunningString = "seconds running", NextRunTime = new DateTime(2020, 3, 8) } }
			};

			var serviceProvider = new Mock<IServiceTaskScheduleStatusProvider>();
			serviceProvider.Setup(x => x.GetServiceStatus()).Returns(webStatus);

			CleanUpServiceTasks();
			var schedule = Factory.New<StmServiceTask>();
			schedule.SST_ServiceTaskCode = "111";
			var collection = new StmServiceTaskCollection(Factory, serviceProvider.Object);

			var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(o =>
				o.Code == "111" &&
				o.Description == "Dummy Description" &&
				o.Category == "TST" &&
				o.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
				o.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "15minutes"));

			using (ObjectFactory.Substitute(Mock.Of<IClientHostedServiceAttributeProvider>(provider =>
					   provider.GetClientHostedServiceAttributes() == new[]
					   {
						   hostedServiceConfigMock,
					   })))
			{
				// Act
				collection.Load();
			}

			// Assert
			AssertEquals(collection[0].StatusString, "status");
			AssertEquals(collection[0].PlaceInQueueString, "place in queue");
			AssertEquals(collection[0].ProcessIDsString, "process id");
			AssertEquals(collection[0].RegisteredOnHosts, "registered on hosts");
			AssertEquals(collection[0].RunningCount, 2);
			AssertEquals(collection[0].SecondsInQueueString, "seconds in queue");
			AssertEquals(collection[0].SecondsRunningString, "seconds running");
		}

		public void TestPopulateStatusDataFilterByHasDate()
		{
			var webStatus = new Dictionary<string, TaskInstanceStatus>
			{
				{ "111", new TaskInstanceStatus() { StatusString = "status", PlaceInQueueString = "place in queue", ProcessIDsString = "process id", RegisteredOnHosts = "registered on hosts", RunningCount = 2, SecondsInQueueString = "seconds in queue", SecondsRunningString = "seconds running" } },
				{ "222", new TaskInstanceStatus() { StatusString = "status", PlaceInQueueString = "place in queue", ProcessIDsString = "process id", RegisteredOnHosts = "registered on hosts", RunningCount = 2, SecondsInQueueString = "seconds in queue", SecondsRunningString = "seconds running", LastRunTime = new DateTime(2020, 3, 8) } }
			};

			var serviceProvider = Mock.Of<IServiceTaskScheduleStatusProvider>(o => o.GetServiceStatus() == webStatus);

			var hostedServiceConfigMock1 = Mock.Of<IHostedServiceAttribute>(o =>
				o.Code == "111" &&
				o.Description == "Dummy Description" &&
				o.Category == "TST" &&
				o.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
				o.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "15minutes"));

			var hostedServiceConfigMock2 = Mock.Of<IHostedServiceAttribute>(o =>
				o.Code == "222" &&
				o.Description == "Dummy Description" &&
				o.Category == "TST" &&
				o.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
				o.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "15minutes"));

			using (ObjectFactory.Substitute(Mock.Of<IClientHostedServiceAttributeProvider>(provider =>
					   provider.GetClientHostedServiceAttributes() == new[]
					   {
							hostedServiceConfigMock1,
							hostedServiceConfigMock2,
					   })))
			{
				CleanUpServiceTasks();
				var schedule1 = Factory.New<StmServiceTask>();
				schedule1.SST_ServiceTaskCode = "111";
				var schedule2 = Factory.New<StmServiceTask>();
				schedule2.SST_ServiceTaskCode = "222";
				var collection = new StmServiceTaskCollection(Factory, serviceProvider);
				var filter1 = new ZQuery();
				filter1.AddToFilter(new ZQuery(StatusColumnProvider.LastRunTime, SQLComparisonOperator.Equal, null));
				collection.Load(filter1);

				AssertEquals(1, collection.Count);
				AssertEquals("111", collection[0].SST_ServiceTaskCode);
				AssertEquals(ZDateTime.Empty, collection[0].LastRunTime);

				var filter2 = new ZQuery();
				filter2.AddToFilter(new ZQuery(StatusColumnProvider.LastRunTime, SQLComparisonOperator.NotEqual, null));
				collection.Load(filter2);

				AssertEquals(1, collection.Count);
				AssertEquals("222", collection[0].SST_ServiceTaskCode);
			}
		}

		public void TestLoadWithNoResultQuery()
		{
			var task = Factory.NewWithValidTestData<StmServiceTask>();
			task.SST_ServiceTaskCode = "AAA";

			var filter = new ZQuery();
			filter.IsNoResultQuery = true;

			var collection = new StmServiceTaskCollection(Factory);
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

			AssertCollectionSortWorks(CustomTextFilterNameConstants.PlaceInQueue + "String", (StmServiceTask s) => (string)s.PlaceInQueueString, inputNums, expectedAscending, expectedDescending);
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

			AssertCollectionSortWorks(CustomTextFilterNameConstants.ProcessId + "sString", (StmServiceTask s) => (string)s.ProcessIDsString, inputNums, expectedAscending, expectedDescending);
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

			AssertCollectionSortWorks(CustomTextFilterNameConstants.SecondsInQueue + "String", (StmServiceTask s) => (string)s.SecondsInQueueString, inputNums, expectedAscending, expectedDescending);
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

			AssertCollectionSortWorks(CustomTextFilterNameConstants.SecondsRunning + "String", (StmServiceTask s) => (string)s.SecondsRunningString, inputNums, expectedAscending, expectedDescending);
		}

		void AssertCollectionSortWorks(string columnName, Func<StmServiceTask, string> columnSelect, Dictionary<string, int[]>[] inputNums, string[] expectedAscending, string[] expectedDescending)
		{
			//Arrange
			SetUpNumericalStringServiceTasks(inputNums);

			var clientHostedServiceAttributeBindingsProvider = new Mock<IClientHostedServiceAttributeProvider>();
			clientHostedServiceAttributeBindingsProvider
				.Setup(p => p.GetClientHostedServiceAttributes())
				.Returns(Enumerable.Range(0, inputNums.Length)
					.Select(x => new HostedServiceAttribute { Code = $"CD{x}", Description = $"Description {x}", Category = "TST", MutuallyExclusiveTaskGroup = MutuallyExclusiveServiceTaskGroups.NoGroup }));

			using (ObjectFactory.Substitute(clientHostedServiceAttributeBindingsProvider.Object))
			{
				var collection = new StmServiceTaskCollection(Factory);
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
		}

		void SetUpNumericalStringServiceTasks(Dictionary<string, int[]>[] numLists)
		{
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

				var task = Factory.New<StmServiceTask>();
				task.SST_ServiceTaskCode = $"CD{serviceTaskCodeNum}";
				task.SST_Active = true;
				task.PlaceInQueueString = numericalString;
				task.ProcessIDsString = numericalString;
				task.SecondsInQueueString = numericalString;
				task.SecondsRunningString = numericalString;
				task.SST_NextRunTime = (ZDateTime.Now + TimeSpan.FromDays(1)).ToOffset();
			}
		}
	}
}
