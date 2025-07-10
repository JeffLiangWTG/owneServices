using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;
using ServiceManager.Integration.ServiceHostClient.Abstractions.Exceptions;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	public class ServiceManagerQuerierTest : TestCaseWithFactory
	{
		public void TestServiceManagerQuerierAndFakerTest()
		{
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				ServiceTaskStatus statusResult;
				var serviceHostsCache = new Mock<IServiceHostsCache>();
				var bizOf = new ReadOnlyBusinessObjectFactory();

				var fakeActiveTaskStatusDTO = new TaskStatusDTO(serviceTaskCode, "BNE", new List<string> { "B1", "B2" }, "description", "category",
					isActive: true, 1, new List<int> { 8080, 8081 }, 1, new DateTime(2017, 6, 1), new TimeSpan(100), 1, new TimeSpan(950), new TimeSpan(230),
					new DateTime(2014, 10, 5), new DateTime(2012, 2, 23), 57);
				serviceHostsCache
						.Setup(cache => cache.CallAllConfiguredHostsUntilFirstSuccess(It.IsAny<Func<IServiceHostClient, TaskStatusDTO>>()))
						.Returns(new AggregateResult<TaskStatusDTO>(new[]
						{
						new AggregateResultEntry<TaskStatusDTO>(new ServiceHostName("Host"), fakeActiveTaskStatusDTO)
						}));

				statusResult = new ServiceManagerQuerier(serviceHostsCache.Object, bizOf).CheckStateOfNamedServiceTask("XXX"); // made up
				AssertEquals("'No such service' is expected for a bogus task", ServiceTaskStatus.NoSuchTaskIsInstalledInThisDb, statusResult);

				var faker1 = new ServiceManagerQuerierHelperForTesting(serviceTaskCode);
				faker1.DeactivateServiceForTesting();
				statusResult = new ServiceManagerQuerier(serviceHostsCache.Object, bizOf).CheckStateOfNamedServiceTask(serviceTaskCode);
				AssertEquals("'Inactive' is expected for a fake task", ServiceTaskStatus.ServiceTaskIsInactive, statusResult);

				faker1.ActivateServiceForTesting();
				statusResult = new ServiceManagerQuerier(serviceHostsCache.Object, bizOf).CheckStateOfNamedServiceTask(serviceTaskCode);
				AssertEquals("'Healthy' is expected for a fake task", ServiceTaskStatus.AtLeastOneHostIsRunningHealthily, statusResult);
				serviceHostsCache.VerifyAll();

				var serviceHostsCache2 = new Mock<IServiceHostsCache>();
				serviceHostsCache2
					.Setup(cache => cache.CallAllConfiguredHostsUntilFirstSuccess(It.IsAny<Func<IServiceHostClient, TaskStatusDTO>>()))
					.Returns(new AggregateResult<TaskStatusDTO>(new[]
					{
					new AggregateResultEntry<TaskStatusDTO>(new ServiceHostName("Host"), new ServiceHostCommunicationException(string.Empty))
					}));
				statusResult = new ServiceManagerQuerier(serviceHostsCache2.Object, bizOf).CheckStateOfNamedServiceTask(serviceTaskCode);
				AssertEquals("No hosts available to run the task", ServiceTaskStatus.NoAvailableHosts, statusResult);

				faker1.Dispose();
			}
		}

		public void TestStmServiceManagerQuerierAndFakerTest()
		{
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ServiceTaskStatus statusResult;
				var serviceHostsCache = new Mock<IServiceHostsCache>();
				var bizOf = new ReadOnlyBusinessObjectFactory();

				var fakeActiveTaskStatusDTO = new TaskStatusDTO(serviceTaskCode, "BNE", new List<string> { "B1", "B2" }, "description", "category",
					isActive: true, 1, new List<int> { 8080, 8081 }, 1, new DateTime(2017, 6, 1), new TimeSpan(100), 1, new TimeSpan(950), new TimeSpan(230),
					new DateTime(2014, 10, 5), new DateTime(2012, 2, 23), 57);

				serviceHostsCache
					.Setup(cache => cache.CallAllConfiguredHostsUntilFirstSuccess(It.IsAny<Func<IServiceHostClient, TaskStatusDTO>>()))
					.Returns(new AggregateResult<TaskStatusDTO>(new[]
					{
					new AggregateResultEntry<TaskStatusDTO>(new ServiceHostName("Host"), fakeActiveTaskStatusDTO)
					}));

				statusResult = new ServiceManagerQuerier(serviceHostsCache.Object, bizOf).CheckStateOfNamedServiceTask("XXX"); // made up
				AssertEquals("'No such service' is expected for a bogus task", ServiceTaskStatus.NoSuchTaskIsInstalledInThisDb, statusResult);

				var faker1 = Factory.NewWithValidTestData<StmServiceTask>();
				faker1.SST_ServiceTaskCode = serviceTaskCode;
				faker1.SST_Active = false;
				faker1.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"45\" /></ScheduleConfig>";
				Factory.Save();

				statusResult = new ServiceManagerQuerier(serviceHostsCache.Object, bizOf).CheckStateOfNamedServiceTask(serviceTaskCode);
				AssertEquals("'Inactive' is expected for a fake task", ServiceTaskStatus.ServiceTaskIsInactive, statusResult);

				faker1.SST_Active = true;
				Factory.Save();

				statusResult = new ServiceManagerQuerier(serviceHostsCache.Object, bizOf).CheckStateOfNamedServiceTask(serviceTaskCode);
				AssertEquals("'Healthy' is expected for a fake task", ServiceTaskStatus.AtLeastOneHostIsRunningHealthily, statusResult);
				serviceHostsCache.VerifyAll();

				var serviceHostsCache2 = new Mock<IServiceHostsCache>();
				serviceHostsCache2
					.Setup(cache => cache.CallAllConfiguredHostsUntilFirstSuccess(It.IsAny<Func<IServiceHostClient, TaskStatusDTO>>()))
					.Returns(new AggregateResult<TaskStatusDTO>(new[]
					{
					new AggregateResultEntry<TaskStatusDTO>(new ServiceHostName("Host"), new ServiceHostCommunicationException(string.Empty))
					}));

				statusResult = new ServiceManagerQuerier(serviceHostsCache2.Object, bizOf).CheckStateOfNamedServiceTask(serviceTaskCode);
				AssertEquals("No hosts available to run the task", ServiceTaskStatus.NoAvailableHosts, statusResult);
			}
		}

		public class CheckStateOfNamedServiceTaskTest : TestCaseWithFactory
		{
			public void TestCheckStateOfNamedServiceTask()
			{
				RunTest(CreateTaskStatus(taskCode: serviceTaskCode, isActive: true), ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);
				RunTest(CreateTaskStatus(taskCode: serviceTaskCode, isActive: false), ServiceTaskStatus.ServiceTaskIsInactive);
				RunTest(null, ServiceTaskStatus.ServiceTaskIsInactive);

				static void RunTest(TaskStatusDTO taskStatus, ServiceTaskStatus expectedStatus)
				{
					// Arrange
					using var serviceTask = new ServiceManagerQuerierHelperForTesting(serviceTaskCode);
					serviceTask.ActivateServiceForTesting();

					var serviceHostsCache = new Mock<IServiceHostsCache>();
					serviceHostsCache
						.Setup(cache => cache.CallAllConfiguredHostsUntilFirstSuccess(It.IsAny<Func<IServiceHostClient, TaskStatusDTO>>()))
						.Returns(new AggregateResult<TaskStatusDTO>(new[]
						{
							new AggregateResultEntry<TaskStatusDTO>(new ServiceHostName(string.Empty), taskStatus),
						}));

					using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
					{
						// Act
						var statusResult = new ServiceManagerQuerier(serviceHostsCache.Object, new ReadOnlyBusinessObjectFactory()).CheckStateOfNamedServiceTask(serviceTaskCode);

						// Assert
						AssertEquals(expectedStatus, statusResult);
					}
				}

				static TaskStatusDTO CreateTaskStatus(string taskCode, bool isActive)
				{
					return new TaskStatusDTO(
						taskCode,
						"BNE",
						new List<string> { "B1", "B2" },
						"description",
						"category",
						isActive,
						1,
						new List<int> { 8080, 8081 },
						1,
						new DateTime(2017, 6, 1),
						new TimeSpan(100), 1,
						new TimeSpan(950),
						new TimeSpan(230),
						new DateTime(2014, 10, 5),
						new DateTime(2012, 2, 23), 57);
				}
			}

			public void TestCheckStateOfNamedServiceTask_WithUnsuccessfulResult_ReturnsNoAvailableHosts()
			{
				// Arrange
				using var serviceTask = new ServiceManagerQuerierHelperForTesting(serviceTaskCode);
				serviceTask.ActivateServiceForTesting();

				var serviceHostsCache = new Mock<IServiceHostsCache>();
				serviceHostsCache
					.Setup(cache => cache.CallAllConfiguredHostsUntilFirstSuccess(It.IsAny<Func<IServiceHostClient, TaskStatusDTO>>()))
					.Returns(new AggregateResult<TaskStatusDTO>(new[]
					{
						new AggregateResultEntry<TaskStatusDTO>(new ServiceHostName(string.Empty), exception: new ServiceHostCommunicationException(string.Empty)),
					}));

				using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					// Act
					var statusResult = new ServiceManagerQuerier(serviceHostsCache.Object, new ReadOnlyBusinessObjectFactory()).CheckStateOfNamedServiceTask(serviceTaskCode);

					// Assert
					AssertEquals(ServiceTaskStatus.NoAvailableHosts, statusResult);
				}
			}
		}

		class GetServiceTaskNextRunTimeTest : TestCaseWithFactory
		{
			public void TestNoDbRecord()
			{
				// Arrange
				var bizOf = new ReadOnlyBusinessObjectFactory();

				using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					// Act
					var success = new ServiceManagerQuerier(Mock.Of<IServiceHostsCache>(), bizOf)
						.TryGetServiceTaskNextRunTime("XXX", out var nextRunTime);

					// Assert
					Assert(!success);
					AssertNull(nextRunTime);
				}
			}

			public void TestValidRecord()
			{
				// Arrange
				using var testTaskInDb = new ServiceManagerQuerierHelperForTesting(serviceTaskCode);
				var bizOf = new ReadOnlyBusinessObjectFactory();
				var testTaskNextRunTime = ZDateTimeOffset.UtcNow.AddHours(2);
				testTaskInDb.SetNextRunTimeForTesting(testTaskNextRunTime.ToUtcZDateTime());

				using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					// Act
					var success = new ServiceManagerQuerier(Mock.Of<IServiceHostsCache>(), bizOf)
						.TryGetServiceTaskNextRunTime(serviceTaskCode, out var nextRunTime);

					// Assert
					Assert(success);
					AssertEquals("Expect correct result returned", testTaskNextRunTime.ToDateTimeOffsetSafe(), nextRunTime);
				}
			}
		}

		class GetServiceTaskScheduleStateTest : TestCaseWithFactory
		{
			public void TestNoDbRecord()
			{
				// Arrange
				var bizOf = new ReadOnlyBusinessObjectFactory();

				using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					// Act
					var success = new ServiceManagerQuerier(Mock.Of<IServiceHostsCache>(), bizOf)
						.TryGetServiceTaskScheduleState("XXX", out var scheduleState);

					// Assert
					Assert(!success);
					AssertNullOrEmpty(scheduleState);
				}
			}

			public void TestValidRecord()
			{
				// Arrange
				using var testTaskInDb = new ServiceManagerQuerierHelperForTesting(serviceTaskCode);
				var bizOf = new ReadOnlyBusinessObjectFactory();
				ZString testTaskScheduleState = "<xml></xml>";
				testTaskInDb.SetScheduleStateForTesting(testTaskScheduleState);

				using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					// Act
					var success = new ServiceManagerQuerier(Mock.Of<IServiceHostsCache>(), bizOf)
						.TryGetServiceTaskScheduleState(serviceTaskCode, out var scheduleState);

					// Assert
					Assert(success);
					AssertEquals("Expect correct result returned", testTaskScheduleState, scheduleState);
				}
			}
		}

		class GetServiceTaskBranchPKTest : TestCaseWithFactory
		{
			public void TestNoDbRecord()
			{
				// Arrange
				var bizOf = new ReadOnlyBusinessObjectFactory();

				using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					// Act
					var success = new ServiceManagerQuerier(Mock.Of<IServiceHostsCache>(), bizOf)
						.TryGetServiceTaskBranchPK("XXX", out var branchPK);

					// Assert
					Assert(!success);
					AssertEquals(Guid.Empty, branchPK);
				}
			}

			public void TestValidRecord()
			{
				// Arrange
				using var testTaskInDb = new ServiceManagerQuerierHelperForTesting(serviceTaskCode);
				var bizOf = new ReadOnlyBusinessObjectFactory();

				var newCompany = Factory.New<GlbCompany>();
				newCompany.GC_Code = "~TC";
				newCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Barbados;
				var newBranch = newCompany.Branches.AddNew();
				newBranch.GB_Code = "~TB";
				newBranch.GB_RN_NKCountryCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, newCompany.GC_RN_NKCountryCode)).RL_RN_NKCountryCode;
				Factory.Save();

				testTaskInDb.SetBranchPKForTesting(newBranch.PK);

				using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					// Act
					var success = new ServiceManagerQuerier(Mock.Of<IServiceHostsCache>(), bizOf)
						.TryGetServiceTaskBranchPK(serviceTaskCode, out var branchPK);

					// Assert
					Assert(success);
					AssertEquals("Expect correct result returned", newBranch.PK, branchPK);
				}
			}

			public void TestNullBranch()
			{
				// Arrange
				using var testTaskInDb = new ServiceManagerQuerierHelperForTesting(serviceTaskCode);
				var bizOf = new ReadOnlyBusinessObjectFactory();

				testTaskInDb.SetBranchPKForTesting(new ZGuid(null));

				using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					// Act
					var success = new ServiceManagerQuerier(Mock.Of<IServiceHostsCache>(), bizOf)
						.TryGetServiceTaskBranchPK(serviceTaskCode, out var branchPK);

					// Assert
					Assert(!success);
					AssertEquals(Guid.Empty, branchPK);
				}
			}
		}

		public class CheckStateOfNamedStmServiceTaskTest : TestCaseWithFactory
		{
			public void TestCheckStateOfNamedStmServiceTask_Active() => AssertCheckStateOfNamedStmServiceTask(serviceTaskCode, CreateTaskStatus(taskCode: serviceTaskCode, isActive: true), ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);
			public void TestCheckStateOfNamedStmServiceTask_InActive() => AssertCheckStateOfNamedStmServiceTask(serviceTaskCode, CreateTaskStatus(taskCode: serviceTaskCode, isActive: false), ServiceTaskStatus.ServiceTaskIsInactive);
			public void TestCheckStateOfNamedStmServiceTask_Null() => AssertCheckStateOfNamedStmServiceTask(serviceTaskCode, null, ServiceTaskStatus.ServiceTaskIsInactive);

			void AssertCheckStateOfNamedStmServiceTask(string taskCode, TaskStatusDTO taskStatus, ServiceTaskStatus expectedStatus)
			{
				// Arrange
				using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					CreateTestStmServiceTask(Factory, taskCode);

					var serviceHostsCache = new Mock<IServiceHostsCache>();
					serviceHostsCache
						.Setup(cache => cache.CallAllConfiguredHostsUntilFirstSuccess(It.IsAny<Func<IServiceHostClient, TaskStatusDTO>>()))
						.Returns(new AggregateResult<TaskStatusDTO>(new[]
						{
						new AggregateResultEntry<TaskStatusDTO>(new ServiceHostName(string.Empty), taskStatus),
						}));

					// Act
					var statusResult = new ServiceManagerQuerier(serviceHostsCache.Object, new ReadOnlyBusinessObjectFactory()).CheckStateOfNamedServiceTask(taskCode);

					// Assert
					AssertEquals(expectedStatus, statusResult);
				}
			}

			static TaskStatusDTO CreateTaskStatus(string taskCode, bool isActive)
			{
				return new TaskStatusDTO(
					taskCode,
					"BNE",
					new List<string> { "B1", "B2" },
					"description",
					"category",
					isActive,
					1,
					new List<int> { 8080, 8081 },
					1,
					new DateTime(2017, 6, 1),
					new TimeSpan(100), 1,
					new TimeSpan(950),
					new TimeSpan(230),
					new DateTime(2014, 10, 5),
					new DateTime(2012, 2, 23), 57);
			}

			public void TestCheckStateOfNamedServiceTask_WithUnsuccessfulResult_ReturnsNoAvailableHosts()
			{
				// Arrange
				using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					CreateTestStmServiceTask(Factory, serviceTaskCode);

					var serviceHostsCache = new Mock<IServiceHostsCache>();
					serviceHostsCache
						.Setup(cache => cache.CallAllConfiguredHostsUntilFirstSuccess(It.IsAny<Func<IServiceHostClient, TaskStatusDTO>>()))
						.Returns(new AggregateResult<TaskStatusDTO>(new[]
						{
						new AggregateResultEntry<TaskStatusDTO>(new ServiceHostName(string.Empty), exception: new ServiceHostCommunicationException(string.Empty)),
						}));

					// Act
					var statusResult = new ServiceManagerQuerier(serviceHostsCache.Object, new ReadOnlyBusinessObjectFactory()).CheckStateOfNamedServiceTask(serviceTaskCode);

					// Assert
					AssertEquals(ServiceTaskStatus.NoAvailableHosts, statusResult);
				}
			}
		}

		class GetStmServiceTaskNextRunTimeTest : TestCaseWithFactory
		{
			public void TestNoDbRecord()
			{
				// Arrange
				var bizOf = new ReadOnlyBusinessObjectFactory();

				using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					// Act
					var success = new ServiceManagerQuerier(Mock.Of<IServiceHostsCache>(), bizOf)
						.TryGetServiceTaskNextRunTime("XXX", out var nextRunTime);

					// Assert
					Assert(!success);
					AssertNull(nextRunTime);
				}
			}

			[TestDate(2025, 1, 1, 1, 1, 1)]
			public void TestValidRecord()
			{
				// Arrange
				var bizOf = new ReadOnlyBusinessObjectFactory();
				var testTaskNextRunTime = ZDateTimeOffset.UtcNow.AddHours(2);
				CreateTestStmServiceTask(Factory, serviceTaskCode, false, testTaskNextRunTime);

				using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					// Act
					var success = new ServiceManagerQuerier(Mock.Of<IServiceHostsCache>(), bizOf)
						.TryGetServiceTaskNextRunTime(serviceTaskCode, out var nextRunTime);

					// Assert
					Assert(success);
					AssertEquals("Expect correct result returned", testTaskNextRunTime.ToDateTimeOffset(), nextRunTime.Value);
				}
			}
		}

		class GetStmServiceTaskScheduleStateTest : TestCaseWithFactory
		{
			public void TestNoDbRecord()
			{
				// Arrange
				var bizOf = new ReadOnlyBusinessObjectFactory();

				using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					// Act
					var success = new ServiceManagerQuerier(Mock.Of<IServiceHostsCache>(), bizOf)
						.TryGetServiceTaskScheduleState("XXX", out var scheduleState);

					// Assert
					Assert(!success);
					AssertNullOrEmpty(scheduleState);
				}
			}

			public void TestValidRecord()
			{
				// Arrange
				var bizOf = new ReadOnlyBusinessObjectFactory();
				var testTaskScheduleState = "<xml></xml>";
				CreateTestStmServiceTask(Factory, serviceTaskCode, false, null, null, testTaskScheduleState);

				using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					// Act
					var success = new ServiceManagerQuerier(Mock.Of<IServiceHostsCache>(), bizOf)
						.TryGetServiceTaskScheduleState(serviceTaskCode, out var scheduleState);

					// Assert
					Assert(success);
					AssertEquals("Expect correct result returned", testTaskScheduleState, scheduleState);
				}
			}
		}

		class GetStmServiceTaskBranchPKTest : TestCaseWithFactory
		{
			public void TestNoDbRecord()
			{
				// Arrange
				var bizOf = new ReadOnlyBusinessObjectFactory();

				using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					// Act
					var success = new ServiceManagerQuerier(Mock.Of<IServiceHostsCache>(), bizOf)
						.TryGetServiceTaskBranchPK("XXX", out var branchPK);

					// Assert
					Assert(!success);
					AssertEquals(Guid.Empty, branchPK);
				}
			}

			public void TestValidRecord()
			{
				// Arrange
				var newCompany = Factory.New<GlbCompany>();
				newCompany.GC_Code = "~TC";
				newCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Barbados;
				var newBranch = newCompany.Branches.AddNew();
				newBranch.GB_Code = "~TB";
				newBranch.GB_RN_NKCountryCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, newCompany.GC_RN_NKCountryCode)).RL_RN_NKCountryCode;
				Factory.Save();

				var bizOf = new ReadOnlyBusinessObjectFactory();
				CreateTestStmServiceTask(Factory, serviceTaskCode, false, null, newBranch.PK);

				using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					// Act
					var success = new ServiceManagerQuerier(Mock.Of<IServiceHostsCache>(), bizOf)
						.TryGetServiceTaskBranchPK(serviceTaskCode, out var branchPK);

					// Assert
					Assert(success);
					AssertEquals("Expect correct result returned", newBranch.PK, branchPK);
				}
			}

			public void TestNullBranch()
			{
				// Arrange
				var bizOf = new ReadOnlyBusinessObjectFactory();
				CreateTestStmServiceTask(Factory, serviceTaskCode, false, null, new ZGuid(null));

				using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					// Act
					var success = new ServiceManagerQuerier(Mock.Of<IServiceHostsCache>(), bizOf)
						.TryGetServiceTaskBranchPK(serviceTaskCode, out var branchPK);

					// Assert
					Assert(!success);
					AssertEquals(Guid.Empty, branchPK);
				}
			}
		}

		public static void CreateTestStmServiceTask(BusinessObjectFactory factory, string serviceTaskCode, bool active = true, ZDateTimeOffset? nextRunTime = null, ZGuid? branch = null, string scheduleState = "")
		{
			var serviceTask = factory.NewWithValidTestData<StmServiceTask>();
			serviceTask.SST_ServiceTaskCode = serviceTaskCode;
			serviceTask.SST_Active = active;
			serviceTask.SST_GB_Branch = branch ?? serviceTask.SST_GB_Branch;
			serviceTask.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"15\" /></ScheduleConfig>";
			serviceTask.ConfigString = scheduleState;
			serviceTask.SST_NextRunTime = nextRunTime ?? serviceTask.SST_NextRunTime;

			factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();

			defaultScheduleMock = Mock.Of<IDefaultSchedule>(d =>
				d.RunEvery == "15minutes");

			hostedServiceAttributeMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.Code == serviceTaskCode &&
				a.Description == "some description" &&
				a.Category == "some category" &&
				a.ConfigControlTypeAssemblyName == "A" &&
				a.ConfigControlTypeName == "B" &&
				a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
				a.DefaultSchedule == defaultScheduleMock);

			attributeProviderMock = Mock.Of<IClientHostedServiceAttributeProvider>(p =>
				p.GetClientHostedServiceAttribute(It.IsAny<string>()) == hostedServiceAttributeMock);

			mockDisposable = ObjectFactory.Substitute(attributeProviderMock);
		}

		protected override void TearDown()
		{
			mockDisposable.Dispose();
			base.TearDown();
		}

		IDefaultSchedule defaultScheduleMock;
		IHostedServiceAttribute hostedServiceAttributeMock;
		IClientHostedServiceAttributeProvider attributeProviderMock;
		IDisposable mockDisposable;

		const string serviceTaskCode = "~TS";
	}
}
