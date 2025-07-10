using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment.Testing;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.CW;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	class ServiceTaskScheduleStatusProviderTest : TestCase
	{
		class InvokeActionOnTaskTest : TestCase
		{
			[ExpectNoExceptions]
			public void TestInvokeActionOnTask_RequestTaskConfigurationReload_DoesNothingIfTaskCodeIsEmpty()
			{
				TestInvokeActionOnTask((statusProvider, taskCode) => statusProvider.RequestTaskConfigurationReload(taskCode));
			}

			[ExpectNoExceptions]
			public void TestInvokeActionOnTask_SetServiceTaskNextRuntime_DoesNothingIfTaskCodeIsEmpty()
			{
				var currentTime = DateTime.UtcNow;
				TestInvokeActionOnTask((statusProvider, taskCode) => statusProvider.SetServiceTaskNextRuntime(taskCode, currentTime, revertingAfterFailedRun: true));
			}

			void TestInvokeActionOnTask(Action<ServiceTaskScheduleStatusProvider, string> action)
			{
				var hostMock1 = new Mock<IServiceHostClient>();
				var hostMock2 = new Mock<IServiceHostClient>();

				hostMock1.Setup(x => x.InvokeActionOnTasks(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string[]>())).Verifiable();
				hostMock2.Setup(x => x.InvokeActionOnTasks(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string[]>())).Verifiable();

				var serviceHostsCacheMock = new Mock<IServiceHostsCache>();
				var serviceHosts = new List<IServiceHostClient> { hostMock1.Object, hostMock2.Object };
				serviceHostsCacheMock.Setup(x => x.AvailableServiceHosts).Returns(serviceHosts);

				using (var statusProvider = new ServiceTaskScheduleStatusProvider(serviceHostsCacheMock.Object))
				{
					foreach (var taskCode in new[] { null, string.Empty, System.Environment.NewLine, " ", "\t" })
					{
						action(statusProvider, taskCode);
					}
				}

				hostMock1.Verify(x => x.InvokeActionOnTasks(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string[]>()), Times.Never);
				hostMock2.Verify(x => x.InvokeActionOnTasks(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string[]>()), Times.Never);
			}
		}

		[ExpectNoExceptions]
		public void TestSetServiceTaskNextRuntime()
		{
			var currentTime = DateTime.UtcNow;
			var taskResult = new TasksActionResultDTO(new Dictionary<TaskCodeDTO, TaskActionResultDTO>() { { new TaskCodeDTO("Code5"), new TaskActionResultDTO(TaskActionOutcomeDTO.Succeeded) } });
			var hostClient2 = new Mock<IServiceHostClient>();
			hostClient2.Setup(hc => hc.InvokeActionOnTasks("setnextruntime", It.Is<IEnumerable<string>>(t => t.Count() == 1 && t.First() == "Code5"), It.Is<string[]>(o => o.Contains("reverting=True")))).Returns(taskResult);
			var hostClient3 = new Mock<IServiceHostClient>();
			hostClient3.Setup(hc => hc.InvokeActionOnTasks("setnextruntime", It.Is<IEnumerable<string>>(t => t.Count() == 1 && t.First() == "Code5"), It.Is<string[]>(o => o.Contains("reverting=True")))).Returns(taskResult);

			var serviceHostsCache = new Mock<IServiceHostsCache>();
			var serviceHosts = new List<IServiceHostClient> { hostClient2.Object, hostClient3.Object };
			serviceHostsCache.Setup(sc => sc.AvailableServiceHosts).Returns(serviceHosts);

			using (var statusProvider = new ServiceTaskScheduleStatusProvider(serviceHostsCache.Object))
			{
				statusProvider.SetServiceTaskNextRuntime("Code5", currentTime, revertingAfterFailedRun: true);
			}

			hostClient2.VerifyAll();
			hostClient3.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestSetServiceTaskNextRuntimeEmptyDateTime()
		{
			var taskResult = new TasksActionResultDTO(new Dictionary<TaskCodeDTO, TaskActionResultDTO>() { { new TaskCodeDTO("Code5"), new TaskActionResultDTO(TaskActionOutcomeDTO.Succeeded) } });
			var hostClient2 = new Mock<IServiceHostClient>();
			hostClient2.Setup(hc => hc.InvokeActionOnTasks("setnextruntime", It.Is<IEnumerable<string>>(t => t.Count() == 1 && t.First() == "Code5"), It.Is<string[]>(o => o.Contains("reverting=True")))).Returns(taskResult);
			var hostClient3 = new Mock<IServiceHostClient>();
			hostClient3.Setup(hc => hc.InvokeActionOnTasks("setnextruntime", It.Is<IEnumerable<string>>(t => t.Count() == 1 && t.First() == "Code5"), It.Is<string[]>(o => o.Contains("reverting=True")))).Returns(taskResult);

			var serviceHostsCache = new Mock<IServiceHostsCache>();
			var serviceHosts = new List<IServiceHostClient> { hostClient2.Object, hostClient3.Object };
			serviceHostsCache.Setup(sc => sc.AvailableServiceHosts).Returns(serviceHosts);

			using (var statusProvider = new ServiceTaskScheduleStatusProvider(serviceHostsCache.Object))
			{
				statusProvider.SetServiceTaskNextRuntime("Code5", null, revertingAfterFailedRun: true);
			}

			hostClient2.VerifyAll();
			hostClient3.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestRequestTaskConfigurationReload()
		{
			var currentTime = ZDateTime.UtcNow;
			var taskResult = new TasksActionResultDTO(new Dictionary<TaskCodeDTO, TaskActionResultDTO>() { { new TaskCodeDTO("Code5"), new TaskActionResultDTO(TaskActionOutcomeDTO.Succeeded) } });
			var hostClient2 = new Mock<IServiceHostClient>();
			hostClient2.Setup(hc => hc.InvokeActionOnTasks("requestconfigreload", It.IsAny<IEnumerable<string>>(), It.IsAny<string[]>())).Returns(taskResult);
			var hostClient3 = new Mock<IServiceHostClient>();
			hostClient3.Setup(hc => hc.InvokeActionOnTasks("requestconfigreload", It.IsAny<IEnumerable<string>>(), It.IsAny<string[]>())).Returns(taskResult);

			var serviceHostsCache = new Mock<IServiceHostsCache>();
			var serviceHosts = new List<IServiceHostClient> { hostClient2.Object, hostClient3.Object };
			serviceHostsCache.Setup(sc => sc.AvailableServiceHosts).Returns(serviceHosts);

			using (var statusProvider = new ServiceTaskScheduleStatusProvider(serviceHostsCache.Object))
			{
				statusProvider.RequestTaskConfigurationReload("Code5");
			}

			hostClient2.VerifyAll();
			hostClient3.VerifyAll();
		}

		public void TestGetServiceStatus()
		{
			// Arrange
			var hostClient1 = new Mock<IServiceHostClient>();
			hostClient1
				.Setup(hc => hc.HostName)
				.Returns(new ServiceHostName("Host1"));
			hostClient1
				.Setup(hc => hc.GetTaskStatusList())
				.Returns(new TaskStatusListDTO(new List<TaskStatusDTO> { taskStatusDTO1, taskStatusDTO2, taskStatusDTO3, taskStatusDTO4 }));
			var hostClient2 = new Mock<IServiceHostClient>();
			hostClient2
				.Setup(hc => hc.HostName)
				.Returns(new ServiceHostName("Host2"));
			hostClient2
				.Setup(hc => hc.GetTaskStatusList())
				.Returns(new TaskStatusListDTO(new List<TaskStatusDTO> { taskStatusDTO3, taskStatusDTO4, taskStatusDTO5Host2 }));
			var hostClient3 = new Mock<IServiceHostClient>();
			hostClient3
				.Setup(hc => hc.HostName)
				.Returns(new ServiceHostName("Host3"));
			hostClient3
				.Setup(hc => hc.GetTaskStatusList())
				.Returns(new TaskStatusListDTO(new List<TaskStatusDTO> { taskStatusDTO1, taskStatusDTO2Host3, taskStatusDTO5, taskStatusDTO6 }));

			var serviceHostsCache = new Mock<IServiceHostsCache>();
			var serviceHosts = new List<IServiceHostClient> { hostClient1.Object, hostClient2.Object, hostClient3.Object };
			serviceHostsCache.Setup(sc => sc.AvailableServiceHosts).Returns(serviceHosts);
			using var statusProvider = new ServiceTaskScheduleStatusProvider(serviceHostsCache.Object);

			// Act
			var webServiceStatus = statusProvider.GetServiceStatus();

			// Assert
			AssertEquals(6, webServiceStatus.Count);
			AssertCode1Status(webServiceStatus["oms"]);
			AssertCode2Status(webServiceStatus["EhO"]);
			AssertCode5Status(webServiceStatus["Code5"]);
			AssertCode6Status(webServiceStatus["ADS"]);
		}

		public void TestGetServiceStatus_ParallelEnabled()
		{
			AssertGetServiceStatusParallelization(enableParallelRequests: true);
		}

		public void TestGetServiceStatus_ParallelDisabled()
		{
			AssertGetServiceStatusParallelization(enableParallelRequests: false);
		}

		void AssertGetServiceStatusParallelization(bool enableParallelRequests)
		{
			// Arrange
			var shortTimeout = TimeSpan.FromSeconds(10);
			var longTimeout = TimeSpan.FromMinutes(1);
			var enableParallelInitialValue = SystemDataRegistryForTest.Get().ServiceTaskParallelWebRequestsEnabled;
			try
			{
				SystemDataRegistryForTest.Get().ServiceTaskParallelWebRequestsEnabled = enableParallelRequests;
				using (var completionEvent = enableParallelRequests ? (EventWaitHandle)new ManualResetEvent(false) : new AutoResetEvent(false))
				{
					var getCount = 0;
					var whenCalledAction = new Action(() =>
					{
						Interlocked.Increment(ref getCount);
						Assert(completionEvent.WaitOne(longTimeout));
					});

					var hostClient1 = new Mock<IServiceHostClient>();
					hostClient1.Setup(hc => hc.HostName).Returns(new ServiceHostName("Host1"));
					hostClient1.Setup(hc => hc.GetTaskStatusList()).Callback(() => whenCalledAction()).Returns(new TaskStatusListDTO(new List<TaskStatusDTO> { taskStatusDTO1, taskStatusDTO2, taskStatusDTO3, taskStatusDTO4 }));
					var hostClient2 = new Mock<IServiceHostClient>();
					hostClient2.Setup(hc => hc.HostName).Returns(new ServiceHostName("Host2"));
					hostClient2.Setup(hc => hc.GetTaskStatusList()).Callback(() => whenCalledAction()).Returns(new TaskStatusListDTO(new List<TaskStatusDTO> { taskStatusDTO3, taskStatusDTO4, taskStatusDTO5Host2 }));
					var hostClient3 = new Mock<IServiceHostClient>();
					hostClient3.Setup(hc => hc.HostName).Returns(new ServiceHostName("Host3"));
					hostClient3.Setup(hc => hc.GetTaskStatusList()).Callback(() => whenCalledAction()).Returns(new TaskStatusListDTO(new List<TaskStatusDTO> { taskStatusDTO1, taskStatusDTO2Host3, taskStatusDTO5, taskStatusDTO6 }));

					var serviceHostsCache = new Mock<IServiceHostsCache>();
					var serviceHosts = new List<IServiceHostClient> { hostClient1.Object, hostClient2.Object, hostClient3.Object };
					serviceHostsCache.Setup(sc => sc.AvailableServiceHosts).Returns(serviceHosts);
					var hostClientFactory = new Mock<IServiceHostClientFactory>();
					using var statusProvider = new ServiceTaskScheduleStatusProvider(serviceHostsCache.Object);

					var task = Task.Run(() =>
					{
						if (enableParallelRequests)
						{
							try
							{
								Assert(WaitUntil(() => getCount == 3, shortTimeout));
							}
							finally
							{
								completionEvent.Set();
							}
						}
						else
						{
							var successCount = 0;
							for (var i = 1; i <= 3; ++i)
							{
								WaitUntil(() => getCount >= i, shortTimeout);
								Thread.Sleep(TimeSpan.FromMilliseconds(250));
								successCount = (getCount == i) ? ++successCount : successCount;
								completionEvent.Set();
							}
							AssertEquals(3, successCount);
						}
					});

					// Act
					var webServiceStatus = statusProvider.GetServiceStatus();

					// Assert
					Assert(task.Wait(longTimeout));
					AssertEquals(6, webServiceStatus.Count);
					AssertCode1Status(webServiceStatus["OmS"]);
					AssertCode2Status(webServiceStatus["eho"]);
					AssertCode5Status(webServiceStatus["Code5"]);
					AssertCode6Status(webServiceStatus["ADS"]);
				}
			}
			finally
			{
				SystemDataRegistryForTest.Get().ServiceTaskParallelWebRequestsEnabled = enableParallelInitialValue;
			}
		}

		bool WaitUntil(Func<bool> outcome, TimeSpan timeout)
		{
			var elapsed = Stopwatch.StartNew();
			while (elapsed.Elapsed < timeout && !outcome())
			{
				Thread.Sleep(TimeSpan.FromMilliseconds(50));
			}

			return outcome();
		}

		public void TestGetServiceTaskStatus_BypassCache()
		{
			var hostClient2 = new Mock<IServiceHostClient>();
			hostClient2.Setup(hc => hc.HostName).Returns(new ServiceHostName("Host2"));
			hostClient2.Setup(hc => hc.GetTaskStatus("Code5")).Returns(taskStatusDTO5Host2);
			var hostClient3 = new Mock<IServiceHostClient>();
			hostClient3.Setup(hc => hc.HostName).Returns(new ServiceHostName("Host3"));
			hostClient3.Setup(hc => hc.GetTaskStatus("Code5")).Returns(taskStatusDTO5);

			var serviceHostsCache = new Mock<IServiceHostsCache>();
			var serviceHosts = new List<IServiceHostClient> { hostClient2.Object, hostClient3.Object };
			serviceHostsCache.Setup(sc => sc.AvailableServiceHosts).Returns(serviceHosts);
			using var statusProvider = new ServiceTaskScheduleStatusProvider(serviceHostsCache.Object);

			var taskCode5Status = statusProvider.GetServiceTaskStatus("Code5");
			AssertCode5Status(taskCode5Status);
		}

		public void TestGetServiceTaskStatus_SortsNumericallyAscending()
		{
			//Arrange
			var taskStatusDTO = new TaskStatusDTO("CD1", "BNE", new List<string> { "B1" }, "description", "category", true, 1, new List<int> { 70 }, 1, new DateTime(2020, 3, 8), TimeSpan.FromSeconds(1), 1, TimeSpan.FromSeconds(900), TimeSpan.FromSeconds(101), new DateTime(2020, 3, 8), new DateTime(2020, 3, 8), 12);
			var taskStatusDTO2 = new TaskStatusDTO("CD1", "BNE", new List<string> { "B1" }, "description", "category", true, 1, new List<int> { 75, 50 }, 1, new DateTime(2020, 3, 8), TimeSpan.FromSeconds(1), 10, TimeSpan.FromSeconds(25), TimeSpan.FromSeconds(102), new DateTime(2020, 3, 8), new DateTime(2020, 3, 8), 12);
			var taskStatusDTO3 = new TaskStatusDTO("CD1", "BNE", new List<string> { "B1" }, "description", "category", true, 1, new List<int> { 80 }, 1, new DateTime(2020, 3, 8), TimeSpan.FromSeconds(1), 2, TimeSpan.FromSeconds(650), TimeSpan.FromSeconds(103), new DateTime(2020, 3, 8), new DateTime(2020, 3, 8), 12);

			var hostClient1 = new Mock<IServiceHostClient>();
			hostClient1.Setup(hc => hc.HostName).Returns(new ServiceHostName("Host1"));
			hostClient1.Setup(hc => hc.GetTaskStatus("CD1")).Returns(taskStatusDTO);
			var hostClient2 = new Mock<IServiceHostClient>();
			hostClient2.Setup(hc => hc.HostName).Returns(new ServiceHostName("Host2"));
			hostClient2.Setup(hc => hc.GetTaskStatus("CD1")).Returns(taskStatusDTO2);
			var hostClient3 = new Mock<IServiceHostClient>();
			hostClient3.Setup(hc => hc.HostName).Returns(new ServiceHostName("Host3"));
			hostClient3.Setup(hc => hc.GetTaskStatus("CD1")).Returns(taskStatusDTO3);

			var serviceHostsCache = new Mock<IServiceHostsCache>();
			var serviceHosts = new List<IServiceHostClient> { hostClient1.Object, hostClient2.Object, hostClient3.Object };
			serviceHostsCache.Setup(sc => sc.AvailableServiceHosts).Returns(serviceHosts);
			using var statusProvider = new ServiceTaskScheduleStatusProvider(serviceHostsCache.Object);

			//Act
			var status = statusProvider.GetServiceTaskStatus("CD1");

			//Assert
			CombineAssertions(() =>
			{
				AssertEquals("[Host2: 25];[Host3: 650];[Host1: 900]".Replace('\u00A0', ' '), status.SecondsInQueueString);
				AssertEquals("[Host1: 1];[Host3: 2];[Host2: 10]".Replace('\u00A0', ' '), status.PlaceInQueueString);
				AssertEquals("[Host2: 75,50];[Host1: 70];[Host3: 80]".Replace('\u00A0', ' '), status.ProcessIDsString);
				AssertEquals("[Host1: 101];[Host2: 102];[Host3: 103]".Replace('\u00A0', ' '), status.SecondsRunningString);
			});
		}

		public void TestWaitForPendingTasksWaitsForAllTasksToFinish()
		{
			// Arrange
			const int amount = 5;
			var counter = 0;
			var taskResult = new TasksActionResultDTO(new Dictionary<TaskCodeDTO, TaskActionResultDTO>() { { new TaskCodeDTO("Code5"), new TaskActionResultDTO(TaskActionOutcomeDTO.Succeeded) } });
			var serviceHostsCacheMock = new Mock<IServiceHostsCache>();
			var serviceHostClientMock = new Mock<IServiceHostClient>();
			using var semaphore = new SemaphoreSlim(amount);
			serviceHostsCacheMock
				.Setup(cache => cache.AvailableServiceHosts)
				.Returns(new[] { serviceHostClientMock.Object });
			serviceHostClientMock
				.Setup(client => client.InvokeActionOnTasks(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string[]>()))
				.Returns(() =>
				{
					semaphore.WaitAsync().Wait();
					Task.Delay(TimeSpan.FromSeconds(5)).Wait();
					Interlocked.Increment(ref counter);
					return taskResult;
				});

			var asd = ZDateTime.Empty.ToString(ServiceManagerConstants.JsonDateTimeFormat, CultureInfo.InvariantCulture);
			using var statusProvider = new ServiceTaskScheduleStatusProvider(serviceHostsCacheMock.Object);
			for (var i = 0; i < amount; i++)
			{
				statusProvider.SetServiceTaskNextRuntime($"Code{i}", DateTime.UtcNow);
			}

			while (semaphore.CurrentCount > 0)
			{
				Thread.Sleep(100);
			}

			// Act
			statusProvider.WaitForPendingTasks(TimeSpan.FromSeconds(15));

			// Assert
			CombineAssertions(() =>
			{
				serviceHostClientMock.Verify(client => client.InvokeActionOnTasks(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string[]>()), Times.Exactly(amount));
				AssertEquals(counter, amount);
			});
		}

		public void TestDisposeWaitsForAllTasksToFinish()
		{
			// Arrange
			const int amount = 5;
			var counter = 0;
			var taskResult = new TasksActionResultDTO(new Dictionary<TaskCodeDTO, TaskActionResultDTO>() { { new TaskCodeDTO("Code5"), new TaskActionResultDTO(TaskActionOutcomeDTO.Succeeded) } });
			var serviceHostsCacheMock = new Mock<IServiceHostsCache>();
			var serviceHostClientMock = new Mock<IServiceHostClient>();
			using var semaphore = new SemaphoreSlim(amount);
			serviceHostsCacheMock
				.Setup(cache => cache.AvailableServiceHosts)
				.Returns(new[] { serviceHostClientMock.Object });
			serviceHostClientMock
				.Setup(client => client.InvokeActionOnTasks(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string[]>()))
				.Returns(() =>
				{
					semaphore.WaitAsync().Wait();
					Task.Delay(TimeSpan.FromSeconds(5)).Wait();
					Interlocked.Increment(ref counter);
					return taskResult;
				});

			var statusProvider = new ServiceTaskScheduleStatusProvider(serviceHostsCacheMock.Object);
			for (var i = 0; i < amount; i++)
			{
				statusProvider.SetServiceTaskNextRuntime($"Code{i}", new DateTime());
			}

			while (semaphore.CurrentCount > 0)
			{
				Thread.Sleep(100);
			}

			// Act
			statusProvider.Dispose();

			// Assert
			CombineAssertions(() =>
			{
				serviceHostClientMock.Verify(client => client.InvokeActionOnTasks(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string[]>()), Times.Exactly(amount));
				AssertEquals(counter, amount);
			});
		}

		void AssertCode1Status(TaskInstanceStatus taskCodeStatus)
		{
			AssertEquals(taskCodeStatus.BindingCount, 2);
			AssertEquals(taskCodeStatus.BindingTypes, "MailDBItems");
			AssertMatchingHosts(taskCodeStatus.PlaceInQueueString, "[Host1: 1];[Host3: 1]");
			AssertMatchingHosts(taskCodeStatus.ProcessIDsString, "[Host1: 8080,8081];[Host3: 8080,8081]");
			AssertMatchingHosts(taskCodeStatus.RegisteredOnHosts, "Host1;Host3");
			AssertEquals(taskCodeStatus.RunningCount, 2);
			AssertMatchingHosts(taskCodeStatus.SecondsInQueueString, "[Host1: 0];[Host3: 0]");
			AssertMatchingHosts(taskCodeStatus.SecondsRunningString, "[Host1: 0];[Host3: 0]");
			AssertEquals(taskCodeStatus.StatusString, "");
			AssertEquals(taskCodeStatus.NextRunTime, new DateTime(2017, 6, 1));
			AssertEquals(taskCodeStatus.LastRunTime, new DateTime(2014, 10, 5));
			AssertEquals(taskCodeStatus.LastErrorTime, new DateTime(2012, 2, 23));
			AssertEquals(taskCodeStatus.ErrorCountLast24Hours, 114);
			AssertEquals(taskCodeStatus.ErrorCountLast24Hours, taskStatusDTO1.ErrorCountLast24Hours * 2);
		}

		void AssertCode2Status(TaskInstanceStatus taskCodeStatus)
		{
			AssertEquals(taskCodeStatus.BindingCount, 1);
			AssertEquals(taskCodeStatus.BindingTypes, "EDIInterchange");
			AssertMatchingHosts(taskCodeStatus.PlaceInQueueString, "[Host1: 2];[Host3: 2]");
			AssertMatchingHosts(taskCodeStatus.ProcessIDsString, "[Host1: 1234];[Host3: 4321]");
			AssertMatchingHosts(taskCodeStatus.RegisteredOnHosts, "Host1;Host3");
			AssertEquals(taskCodeStatus.RunningCount, 2);
			AssertMatchingHosts(taskCodeStatus.SecondsInQueueString, "[Host1: 0];[Host3: 0]");
			AssertMatchingHosts(taskCodeStatus.SecondsRunningString, "[Host1: 0];[Host3: 0]");
			AssertEquals(taskCodeStatus.StatusString, "");
			AssertEquals(taskCodeStatus.NextRunTime, new DateTime(1990, 6, 2));
			AssertEquals(taskCodeStatus.LastRunTime, new DateTime(1990, 3, 2));
			AssertEquals(taskCodeStatus.LastErrorTime, new DateTime(1990, 2, 2));
			AssertEquals(taskCodeStatus.ErrorCountLast24Hours, 22);
			AssertEquals(taskCodeStatus.ErrorCountLast24Hours, taskStatusDTO2.ErrorCountLast24Hours + taskStatusDTO2Host3.ErrorCountLast24Hours);
		}

		void AssertCode5Status(TaskInstanceStatus taskCodeStatus)
		{
			AssertEquals(taskCodeStatus.BindingCount, 0);
			AssertEquals(taskCodeStatus.BindingTypes, string.Empty);
			AssertMatchingHosts(taskCodeStatus.PlaceInQueueString, "[Host2: 5];[Host3: 5]");
			AssertMatchingHosts(taskCodeStatus.ProcessIDsString, "[Host2: 80,81];[Host3: 80,81]");
			AssertMatchingHosts(taskCodeStatus.RegisteredOnHosts, "Host2;Host3");
			AssertEquals(taskCodeStatus.RunningCount, 2);
			AssertMatchingHosts(taskCodeStatus.SecondsInQueueString, "[Host2: 0];[Host3: 0]");
			AssertMatchingHosts(taskCodeStatus.SecondsRunningString, "[Host2: 0];[Host3: 0]");
			AssertEquals(taskCodeStatus.StatusString, "");
			AssertEquals(taskCodeStatus.NextRunTime, new DateTime(2020, 3, 8));
			AssertEquals(taskCodeStatus.LastRunTime, new DateTime(2020, 3, 8));
			AssertEquals(taskCodeStatus.LastErrorTime, new DateTime(2020, 3, 8));
			AssertEquals(taskCodeStatus.ErrorCountLast24Hours, 101);
			AssertEquals(taskCodeStatus.ErrorCountLast24Hours, taskStatusDTO5.ErrorCountLast24Hours + taskStatusDTO5Host2.ErrorCountLast24Hours);
		}

		void AssertMatchingHosts(string actualHosts, string expectedHosts)
		{
			var actualHostsSplit = actualHosts.Split(';');
			var expectedHostsSplit = expectedHosts.Split(';');
			AssertEquals("Wrong number of hosts reported", expectedHostsSplit.Length, actualHostsSplit.Length);
			Assert("Wrong hosts", expectedHostsSplit.All((s) => actualHostsSplit.Contains(s)));
		}

		void AssertCode6Status(TaskInstanceStatus taskCodeStatus)
		{
			AssertEquals(taskCodeStatus.BindingCount, 4);
			AssertEquals(taskCodeStatus.BindingTypes, "GlbStaff,GlbGroup");
			AssertMatchingHosts(taskCodeStatus.PlaceInQueueString, "[Host3: 5]");
			AssertMatchingHosts(taskCodeStatus.ProcessIDsString, "[Host3: 80]");
			AssertMatchingHosts(taskCodeStatus.RegisteredOnHosts, "Host3");
			AssertEquals(taskCodeStatus.RunningCount, 1);
			AssertMatchingHosts(taskCodeStatus.SecondsInQueueString, "[Host3: 0]");
			AssertMatchingHosts(taskCodeStatus.SecondsRunningString, "[Host3: 0]");
			AssertEquals(taskCodeStatus.StatusString, "");
			Assert(!taskCodeStatus.NextRunTime.HasValue);
			Assert(!taskCodeStatus.LastRunTime.HasValue);
			Assert(!taskCodeStatus.LastErrorTime.HasValue);
			AssertEquals(taskCodeStatus.ErrorCountLast24Hours, 0);
			AssertEquals(taskCodeStatus.ErrorCountLast24Hours, taskStatusDTO6.ErrorCountLast24Hours);
		}

		readonly TaskStatusDTO taskStatusDTO1 = new TaskStatusDTO("OMS", "BNE", new List<string> { "B1", "B2" }, "description", "category", true, 1, new List<int> { 8080, 8081 }, 1, new DateTime(2017, 6, 1), new TimeSpan(100), 1, new TimeSpan(950), new TimeSpan(230), new DateTime(2014, 10, 5), new DateTime(2012, 2, 23), 57);
		readonly TaskStatusDTO taskStatusDTO2 = new TaskStatusDTO("EHO", "BNE", new List<string> { "B1", "C1" }, "description", "category", true, 1, new List<int> { 1234 }, 1, new DateTime(1990, 6, 2), new TimeSpan(300), 2, new TimeSpan(850), new TimeSpan(330), new DateTime(1990, 3, 2), new DateTime(1990, 2, 2), 22);
		readonly TaskStatusDTO taskStatusDTO2Host3 = new TaskStatusDTO("EHO", "BNE", new List<string> { "B1", "C1" }, "description", "category", true, 1, new List<int> { 4321 }, 1, new DateTime?(), new TimeSpan(300), 2, new TimeSpan(850), new TimeSpan(330), new DateTime?(), new DateTime?(), 0);
		readonly TaskStatusDTO taskStatusDTO3 = new TaskStatusDTO("Code3", "BNE", new List<string> { "C1", "C2" }, "description", "category", true, 1, new List<int> { 123, 234 }, 1, new DateTime(2018, 7, 1), new TimeSpan(500), 3, new TimeSpan(750), new TimeSpan(430), new DateTime(2018, 7, 1), new DateTime(2018, 7, 1), 345);
		readonly TaskStatusDTO taskStatusDTO4 = new TaskStatusDTO("Code4", "BNE", new List<string> { "C2", "D1" }, "description", "category", true, 1, new List<int> { 996, 997 }, 1, new DateTime(2011, 3, 2), new TimeSpan(800), 4, new TimeSpan(650), new TimeSpan(530), new DateTime(2011, 3, 2), new DateTime(2011, 3, 2), 33);
		readonly TaskStatusDTO taskStatusDTO5 = new TaskStatusDTO("Code5", "BNE", new List<string> { "D1", "B1" }, "description", "category", true, 1, new List<int> { 80, 81 }, 1, new DateTime(2020, 3, 8), new TimeSpan(1000), 5, new TimeSpan(550), new TimeSpan(630), new DateTime(2020, 3, 8), new DateTime(2020, 3, 8), 12);
		readonly TaskStatusDTO taskStatusDTO5Host2 = new TaskStatusDTO("Code5", "BNE", new List<string> { "D1", "B1" }, "description", "category", false, 1, new List<int> { 80, 81 }, 1, new DateTime(2010, 3, 8), new TimeSpan(1000), 5, new TimeSpan(550), new TimeSpan(630), new DateTime(2010, 3, 8), new DateTime(2010, 3, 8), 89);
		readonly TaskStatusDTO taskStatusDTO6 = new TaskStatusDTO("ADS", "BNE", new List<string> { "D1", "B1" }, "description", "category", true, 1, new List<int> { 80 }, 1, new DateTime?(), new TimeSpan(1000), 5, new TimeSpan(550), new TimeSpan(630), new DateTime?(), new DateTime?(), 0);
	}
}
