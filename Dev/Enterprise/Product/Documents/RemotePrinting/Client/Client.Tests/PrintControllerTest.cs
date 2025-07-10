using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Enterprise.RemotePrinting.Client.RemotePrintServer;
using Enterprise.RemotePrinting.Engine;
using Enterprise.RemotePrinting.Types;
using Microsoft.Win32;
using Moq;
using NUnit.Framework;
using static Enterprise.RemotePrinting.Client.Tests.ConnectionRegistryManagerTest;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class PrintControllerTest : TestCase
	{
		public void TestUploadQueueListToServerLogsWhenLoginUserIsCWSupportUser()
		{
			var log = new StringBuilder();
			var config = new WebClientConfiguration
			{
				WebServiceUser = "CWSupport-testinvalidtoken"
			};
			var client = new WebClient(new RemotePrintingServiceAdaptor(config));
			var testController = new ControllerForTesting("~TEST_SERVER", webClient: client);
			testController.ShowInformation += (sender, e) => { log.Append(e.Message); };
			var result = testController.UploadQueueListToServerAndReturnQueueCount();

			Assert("Should be CWSupport user", client.IsSupportUser);
			AssertEquals("Should no print queues", 0, result);
			AssertEquals("CWSupport user cannot send print client queue list to server", log.ToString());
		}

		public void TestRunSignalRShouldHandleRedirectResponse()
		{
			MethodDelegate retryAction = null;
			Exception outException = null;
			var responseProcessorMock = new Mock<IErrorResponseWebRequestProcessor>();
			responseProcessorMock.Setup(r => r.Process(It.IsAny<MethodDelegate>(), out retryAction, out outException, It.IsAny<bool>()))
				.Throws(new InvalidOperationException("Just for Jerry Test"))
				.Verifiable();

			var serviceAdaptorMock = new Mock<IRemotePrintingServiceAdaptor>();
			serviceAdaptorMock.Setup(i => i.ResponseProcessor).Returns(responseProcessorMock.Object);

			var webClient = new WebClientForTesting(serviceAdaptorMock.Object, true);
			var testController = new ControllerForTestingWithWebException("~TEST_SERVER", null, webClient);
			testController.Stop();

			AssertNoExceptionThrown(() => testController.Process_Exposed());
			responseProcessorMock.Verify();
		}

		public void TestHandleInnerWebException()
		{
			var exception = new Exception("", new WebException("test", System.Net.WebExceptionStatus.Timeout));
			var controller = new ControllerForTesting("", null, null);
			Assert(controller.HandleException(exception));
		}

		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		public void TestCheckUpdateShouldBeCalledWhenStartingRun()
		{
			var log = new StringBuilder();

			ErrorReporter.Instance = new Mock<ErrorReporter>().Object;
			var mock = new Mock<IRemotePrintingServiceAdaptor>();
			var webClient = new WebClientForTesting(mock.Object, true);
			var testController = new ControllerForTesting("~TEST_SERVER", null, webClient);
			testController.ShowInformation += (sender, e) => { log.Append(e.Message); };
			try
			{
				testController.Run();
				Assert(log.ToString().Contains("Checking for update ..."));
			}
			finally
			{
				ErrorReporter.Instance = null;
				DeleteRegistrySubKeySafe(Registry.CurrentUser, ConnectionRegistryManagerTest.TestEdiKeyName);
				AssertNull("Should have deleted folder", Registry.CurrentUser.OpenSubKey(ConnectionRegistryManagerTest.TestEdiKeyName));
			}
		}

		public void TestMemoryUsageExceedLimit()
		{
			var log = new StringBuilder();

			var configName1 = "webConfig1";
			ErrorReporter.Instance = new Mock<ErrorReporter>().Object;
			var mock = new Mock<IRemotePrintingServiceAdaptor>();
			var webClient = new WebClientForTesting(mock.Object, true);
			var testController = new ControllerForTesting("~TEST_SERVER", null, webClient);
			testController.ShowInformation += (sender, e) => { log.Append(e.Message); };
			try
			{
				var webConfig1 = new WebClientConfiguration("url1", "user1", "pwd1", 1, "machine1", false, "proxy1", 43, "proxyuser1", "proxypwd1", false, false, 30, false, 0, 0, 100, 100, false, false, 0, 0, 0, false, 3, 3, false, 0, new WebClientUpdateConfiguration(), true, 1);
				Registry.CurrentUser.CreateSubKey(ConnectionRegistryManagerTest.TestEdiKeyName);
				testController.ConfigName = configName1;
				testController.SaveConfig(configName1, webConfig1);

				testController.LogMemoryUsage_Exposed();

				Assert(log.ToString().Contains("Memory exceeded limit of 1 MB."));
			}
			finally
			{
				ErrorReporter.Instance = null;
				DeleteRegistrySubKeySafe(Registry.CurrentUser, ConnectionRegistryManagerTest.TestEdiKeyName);
				AssertNull("Should have deleted folder", Registry.CurrentUser.OpenSubKey(ConnectionRegistryManagerTest.TestEdiKeyName));
			}
		}

		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		public void TestLogSystemInformationOnProcessStarted()
		{
			var log = new StringBuilder();

			var mock = new Mock<IRemotePrintingServiceAdaptor>();
			var webClient = new WebClientForTesting(mock.Object, true);
			var testController = new ControllerForTesting("~TEST_SERVER", null, webClient);

			try
			{
				var expextedMessage = $"";
				testController.ProcessStarting += (s, e) => log.Append(e.Message);
				testController.Run();
				Assert(log.ToString().Contains("Process Started"));
			}
			finally
			{
				DeleteRegistrySubKeySafe(Registry.CurrentUser, ConnectionRegistryManagerTest.TestEdiKeyName);
				AssertNull("Should have deleted folder", Registry.CurrentUser.OpenSubKey(ConnectionRegistryManagerTest.TestEdiKeyName));
			}
		}

		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		public void TestCheckUpdateShouldBeCalledSeveralTimeIfFailedToStart()
		{
			ErrorReporter.Instance = new ErrorReporter();

			var configName1 = "webConfig1";
			var log = new StringBuilder();
			var mock = new Mock<IRemotePrintingServiceAdaptor>();
			var webClient = new WebClientForTesting(mock.Object, true);
			var testController = new ControllerForTestingWithWebException("~TEST_SERVER", null, webClient);
			testController.ShowInformation += (sender, e) => { log.Append(e.Message); };
			try
			{
				var webConfig1 = new WebClientConfiguration("url1", "user1", "pwd1", 1, "machine1", false, "proxy1", 43, "proxyuser1", "proxypwd1", false, false, 30, false, 0, 0, 100, 100, false, false, 0, 0, 0, false, 3, 3, false, 0, new WebClientUpdateConfiguration(), false, 100);
				Registry.CurrentUser.CreateSubKey(ConnectionRegistryManagerTest.TestEdiKeyName);
				testController.ConfigName = configName1;
				testController.SaveConfig(configName1, webConfig1);
				testController.ThrowException = true;
				AssertNoExceptionThrown(() => testController.Run());

				Assert(log.ToString().Contains("Update failed after 3 attempts"));
			}
			finally
			{
				testController.DeleteConfig(configName1);
				DeleteRegistrySubKeySafe(Registry.CurrentUser, ConnectionRegistryManagerTest.TestEdiKeyName);
				AssertNull("Should have deleted folder", Registry.CurrentUser.OpenSubKey(ConnectionRegistryManagerTest.TestEdiKeyName));
				ErrorReporter.Instance = null;
			}
		}

		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		public void TestCleanOldLogFile()
		{
			var logs = new List<string>();
			var controller = new ControllerForTesting("test", null, null);
			controller.ShowInformation += (_, e) => logs.Add(e.Message);
			controller.CleanOldLogFiles_Exposed();

			var logConfiguration = LogFileManager.Instance.GetLogConfiguration();
			var expectedMessage = $"Cleaning log file(s) older than {logConfiguration.DayToKeepOldLogFile} day(s).";

			AssertContains(expectedMessage, logs[0]);
		}

		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		public void TestRunStopRunCallsScanForNewPrinterEachTime()
		{
			var configName1 = "webConfig1";
			var registryManager = new ConnectionRegistryManagerForTest();
			registryManager.LoadFromRegistry(configName1);
			using (UpdateProcessor.OverrideConnectionRegistryManagerForTest(registryManager))
			{
				var log = new StringBuilder();
				var mock = new Mock<IRemotePrintingServiceAdaptor>();
				var webClient = new WebClientForTesting(mock.Object, true);
				var printJobManager = new PrintManagerForTesting();
				printJobManager.AddInstalledMockQueues("queue1", "queue2");
				var testController = new ControllerForTesting("~TEST_SERVER", printJobManager, webClient);

				ErrorReporter.Instance = new Mock<ErrorReporter>().Object;
				try
				{
					var webConfig1 = new WebClientConfiguration("url1", "user1", "pwd1", 1, "machine1", false, "proxy1", 43, "proxyuser1", "proxypwd1", false, false, 30, false, 0, 0, 100, 100, false, false, 0, 0, 0, false, 0, 0, false, 0, new WebClientUpdateConfiguration(), false, 100);
					Registry.CurrentUser.CreateSubKey(ConnectionRegistryManagerTest.TestEdiKeyName);
					testController.ConfigName = configName1;
					testController.SaveConfig(configName1, webConfig1);
					testController.ShowInformation += (sender, e) => { log.Append(e.Message + "\n"); };
					testController.Run();

					AssertContains("Should have scanned for printers after starting the loop", "Installed Printers:", log.ToString());

					log = new StringBuilder();
					testController.Run();

					AssertContains("Should have scanned for printers again after re-starting the loop", "Installed Printers:", log.ToString());
				}
				finally
				{
					ErrorReporter.Instance = null;
					testController.DeleteConfig(configName1);
					DeleteRegistrySubKeySafe(Registry.CurrentUser, ConnectionRegistryManagerTest.TestEdiKeyName);
					AssertNull("Should have deleted folder", Registry.CurrentUser.OpenSubKey(ConnectionRegistryManagerTest.TestEdiKeyName));
				}
			}
		}

		public void TestNudgeStopWaitHandle()
		{
			var log = new StringBuilder();
			var mock = new Mock<IRemotePrintingServiceAdaptor>();
			var webClient = new WebClientForTesting(mock.Object, true);

			var printJobManager = new PrintManagerForTesting();
			printJobManager.AddInstalledMockQueues("queue1", "queue2");

			var testController = new ControllerForTesting("~TEST_SERVER", printJobManager, webClient);
			testController.ShowInformation += (sender, e) => { log.Append(e.Message + "\n"); };

			var threadRunPrintController = new Thread(() =>
			{
				// Pause for 10 seconds unless interrupted by Nudge
				testController.PauseJobWhenRequestedExposed(0, 10);
			});

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			threadRunPrintController.Start();

			Thread.Sleep(100);
			testController.Nudge();

			threadRunPrintController.Join();

			stopwatch.Stop();

			Assert("PauseJobWhenRequested() should not run full 10 seconds", stopwatch.Elapsed.TotalSeconds < 2.0);
			AssertContains("Nudge", "Nudge received to check new print jobs", log.ToString());
		}

		public void TestUsesSettingToScanForNewPrintersPeriodically()
		{
			var configName1 = "webConfig1";
			var mock = new Mock<IRemotePrintingServiceAdaptor>();
			var webClient = new WebClientForTesting(mock.Object, true);
			var printJobManager = new PrintManagerForTesting();
			printJobManager.AddInstalledMockQueues("queue1", "queue2");
			var testController = new ControllerForTesting("~TEST_SERVER", printJobManager, webClient);
			try
			{
				var settingsForScanNewPrinters = 2;
				var webConfig1 = new WebClientConfiguration("url1", "user1", "pwd1", 1, "machine1", false, "proxy1", 43, "proxyuser1", "proxypwd1", false, false, settingsForScanNewPrinters, false, 0, 0, 100, 100, false, false, 0, 0, 0, false, 0, 0, false, 0, new WebClientUpdateConfiguration(), false, 100);
				testController.QueuesCount_Exposed = 2;
				Registry.CurrentUser.CreateSubKey(ConnectionRegistryManagerTest.TestEdiKeyName);
				testController.SaveConfig(configName1, webConfig1);
				Assert("Pre-Condition: Should be true the first time", testController.IsPrintQueuesRefreshRequired_Exposed());

				Thread.Sleep((settingsForScanNewPrinters - 1) * 1000);
				Assert("Should not require print queue refresh", !testController.IsPrintQueuesRefreshRequired_Exposed());

				Thread.Sleep(2000);
				Assert("Should now require print queue refresh", testController.IsPrintQueuesRefreshRequired_Exposed());
			}
			finally
			{
				testController.DeleteConfig(configName1);
				DeleteRegistrySubKeySafe(Registry.CurrentUser, ConnectionRegistryManagerTest.TestEdiKeyName);
				AssertNull("Should have deleted folder", Registry.CurrentUser.OpenSubKey(ConnectionRegistryManagerTest.TestEdiKeyName));
			}
		}

		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		public void TestScanForNewPrintersButtonClicks_ScansImmediately()
		{
			const string ConfigName = "webConfig1";
			var registryManager = new ConnectionRegistryManagerForTest();
			registryManager.LoadFromRegistry(ConfigName);
			using (UpdateProcessor.OverrideConnectionRegistryManagerForTest(registryManager))
			{
				var log = new StringBuilder();

				ErrorReporter.Instance = new Mock<ErrorReporter>().Object;

				var mock = new Mock<IRemotePrintingServiceAdaptor>();
				var webClient = new WebClientForTesting(mock.Object, true);
				var printJobManager = new PrintManagerForTesting();
				printJobManager.AddInstalledMockQueues("queue1", "queue2");
				var testController = new ControllerForTesting("~TEST_SERVER", printJobManager, webClient);
				try
				{
					Registry.CurrentUser.CreateSubKey(ConnectionRegistryManagerTest.TestEdiKeyName);
					testController.ConfigName = ConfigName;

					testController.ShowInformation += (sender, e) =>
					{
						lock (log)
						{
							log.AppendLine(e.Message);
						}
					};

					testController.ShouldLoopOver = true;

					using (var timer = new Timer(ScanForNewPrintersButtonClick, (testController, log), 100, Timeout.Infinite))
					{
						testController.Run();
					}

					var logText = log.ToString();
					AssertGreaterThanOrEqualTo("Should have scanned for printers at least twice, was", Regex.Matches(logText, "Installed Printers:").Count, 2);
					AssertContains("One of scans should be initiated by button", "Clicking button", logText);
				}
				finally
				{
					testController.DeleteConfig(ConfigName);
					ErrorReporter.Instance = null;
					DeleteRegistrySubKeySafe(Registry.CurrentUser, ConnectionRegistryManagerTest.TestEdiKeyName);
					AssertNull("Should have deleted folder", Registry.CurrentUser.OpenSubKey(ConnectionRegistryManagerTest.TestEdiKeyName));
				}
			}
		}

		static void ScanForNewPrintersButtonClick(object stateInfo)
		{
			var (controller, log) = (ValueTuple<ControllerForTesting, StringBuilder>)stateInfo;

			// Wait till main process in controller performs first printers scan
			do
			{
				string logText;
				lock (log)
				{
					logText = log.ToString();
				}
				if (logText.Contains("Installed Printers:"))
				{
					break;
				}

				Thread.Sleep(100);
			} while (true);

			lock (log)
			{
				log.AppendLine("Clicking button");
			}

			controller.UploadQueueListToServerAndReturnQueueCount();
			controller.Stop();
		}

		public void TestSyncJobStatus()
		{
			var mock = new Mock<IRemotePrintingServiceAdaptor>();
			var webClient = new WebClientForTesting(mock.Object, true);
			var testController = new ControllerForTesting("~TEST_SERVER", null, webClient) { LoopCount = 2 };
			mock.Setup(i => i.SetJobSuccess(It.IsAny<Guid[]>()))
				.Callback(new Action<Guid[]>(p =>
				{
					if (testController.LoopCount == 2)
					{
						throw new WebException();
					}
					webClient.SetJobSuccessMockCall(p);
				}));

			mock.Setup(i => i.SetJobFailure(It.IsAny<PrintJobFailed[]>()))
				.Callback(new Action<PrintJobFailed[]>(p =>
				{
					if (testController.LoopCount == 2)
					{
						throw new WebException();
					}
					webClient.SetJobFailureMockCall(p);
				}));

			var queueName1 = "Q1";
			var queueName2 = "Q2";

			testController.MockInstalledQueues.Clear();
			testController.AddInstalledMockQueues(queueName1, queueName2);

			testController.UploadQueueListToServerAndReturnQueueCount_Exposed();

			var job0 = GetNewPrintJob(queueName1, "Q1J1");
			var job1 = GetNewPrintJob(queueName1, "Q1J2");
			var job2 = GetNewPrintJob(queueName2, "Q2J1");
			var job3 = GetNewPrintJob(queueName2, "Q2J2");

			testController.MockJobsFromServer.Clear();
			testController.MockJobsFromServer.AddRange(new[] { job0, job1, job2, job3 });

			var numberOfJobsPrinted = testController.LoopDownloadJobsSendToPrinterAndReturnSuccessToServer();
			AssertEquals(4, numberOfJobsPrinted);
		}

		public void TestSyncJobStatusDoesNotBlockOtherPrintJobs()
		{
			var syncJobPK = Guid.NewGuid();

			var expectedContainingLogs = $@"Synchronizing previously processed jobs statuses...
Start updating successful print jobs status. (Jobs: {syncJobPK})
* * * Sync Job Status Failed * * *
Operation is not valid due to the current state of the object.
System.Net.WebException";
			AssertSyncJobStatusDoesNotBlockOtherPrintJobs(syncJobPK, new WebException(), expectedContainingLogs);

			expectedContainingLogs = $@"Synchronizing previously processed jobs statuses...
Start updating successful print jobs status. (Jobs: {syncJobPK})
Timeout while executing web operation: Test time out
Web Exception Status: Timeout
Timeout interval: 100s

Downloading job(s) from server";
			AssertSyncJobStatusDoesNotBlockOtherPrintJobs(syncJobPK, new WebException("Test time out", WebExceptionStatus.Timeout), expectedContainingLogs);
		}

		void AssertSyncJobStatusDoesNotBlockOtherPrintJobs(Guid syncJobPK, Exception ex, string expectedContainingLogs)
		{
			JobStatusFileWriter.Delete();
			var contents = new List<JobDetails>
			{
				new (syncJobPK, "", ProcessedStatus.Processed)
			};
			JobStatusFileWriter.Append(contents);

			var mock = new Mock<IRemotePrintingServiceAdaptor>();
			var webClient = new WebClientForTesting(mock.Object, true);
			var testController = new ControllerForTesting("~TEST_SERVER", null, webClient) { LoopCount = 1 };

			var log = new StringBuilder();
			testController.ShowInformation += (s, e) => log.AppendLine(e.Message);
			testController.ShowError += (s, e) => log.AppendLine(e.Message);
			mock.Setup(i => i.SetJobSuccess(It.IsAny<Guid[]>()))
				.Callback(new Action<Guid[]>(p =>
				{
					if (p.Contains(syncJobPK))
					{
						throw ex;
					}

					webClient.SetJobSuccessMockCall(p);
				}));

			var queueName = "Q1";

			testController.MockInstalledQueues.Clear();
			testController.AddInstalledMockQueues(queueName);

			testController.UploadQueueListToServerAndReturnQueueCount_Exposed();

			var job = GetNewPrintJob(queueName, "Q1J1");

			testController.MockJobsFromServer.Clear();
			testController.MockJobsFromServer.AddRange(new[] { job });

			var numberOfJobsPrinted = testController.LoopDownloadJobsSendToPrinterAndReturnSuccessToServer();
			AssertEquals(1, numberOfJobsPrinted);

			var logString = log.ToString();

			AssertContains("Synchronizing previously processed jobs statuses...", logString);
			AssertNotContains("Successfully synchronizing previously processed jobs statuses.", logString);
			AssertContains(expectedContainingLogs, logString);
		}

		public void TestFailingToPrintJobSendsBackToPrinter()
		{
			const string queueName = "Q1";
			var job = GetNewPrintJob(queueName, "Blah Blah");

			var config = Configurator.GetProxyDefaultSystemSettings();
			var webClient = new WebClientForTesting(new RemotePrintingServiceAdaptor(config));
			var printManager = new PrintManagerForTesting();
			printManager.JobsToThrow.Add(job.JobPk);

			var controller = new ControllerForTesting("~TEST_SERVER", printManager, webClient);

			controller.MockInstalledQueues.Clear();
			controller.AddInstalledMockQueues(queueName);

			controller.UploadQueueListToServerAndReturnQueueCount_Exposed();

			controller.MockJobsFromServer.Clear();
			controller.MockJobsFromServer.Add(job);
			controller.DownloadJobsSendToPrinterAndReturnSuccessToServer_Exposed();
			AssertEquals("The job should have been marked as a failure", job.JobPk, webClient.LastFailedJobs.First().JobPk);
		}

		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		public void TestSignalRDisabling()
		{
			ErrorReporter.Instance = new Mock<ErrorReporter>().Object;

			var configName1 = "webConfig1";
			var registryManager = new ConnectionRegistryManagerForTest();
			registryManager.LoadFromRegistry(configName1);
			using (UpdateProcessor.OverrideConnectionRegistryManagerForTest(registryManager))
			{
				var log = new StringBuilder();
				var mock = new Mock<IRemotePrintingServiceAdaptor>();
				var webClient = new WebClientForTesting(mock.Object, true);
				var printJobManager = new PrintManagerForTesting();
				printJobManager.AddInstalledMockQueues("queue1", "queue2");
				var testController = new ControllerForTesting("~TEST_SERVER", printJobManager, webClient);
				try
				{
					var webConfig1 = new WebClientConfiguration("url1", "user1", "pwd1", 1, "machine1", false, "proxy1", 43, "proxyuser1", "proxypwd1", false, false, 30, false, 0, 0, 100, 100, false, false, 0, 0, 0, false, 0, 0, false, 0, new WebClientUpdateConfiguration(), false, 100);
					Registry.CurrentUser.CreateSubKey(ConnectionRegistryManagerTest.TestEdiKeyName);
					testController.ConfigName = configName1;
					testController.SaveConfig(configName1, webConfig1);
					testController.ShowInformation += (sender, e) => { log.AppendLine(e.Message); };
					testController.OnHubClientControllerCreated = (client) => { log.AppendLine(client.ToString() + " created."); };

					testController.Run();

					AssertContains("When SignalR is disabled. We should notify the user", "SignalR is disabled. Skipping hub connection.", log.ToString());
					AssertNotContains("When SignalR is disabled. We should notify the user", "Connecting to hub.", log.ToString());
					AssertNotContains("When SignalR is disabled. HubClientController should not be created", "Enterprise.RemotePrinting.Client.HubClientController created.", log.ToString());

					log = new StringBuilder();
					webConfig1.EnableSignalR = true;
					testController.SaveConfig(configName1, webConfig1);
					testController.Run();

					CombineAssertions(() =>
					{
						AssertNotContains("When SignalR is enabled the disabled log shouldn't be present.", "SignalR is disabled. Skipping hub connection.", log.ToString());
						AssertContains("When SignalR is enabled. We should attempt to connect & notify the user", "SignalR.", log.ToString());
						AssertContains("When SignalR is enabled. HubClientController should be created", "Enterprise.RemotePrinting.Client.HubClientController created.", log.ToString());
					});
				}
				finally
				{
					testController.DeleteConfig(configName1);
					ErrorReporter.Instance = null;
					DeleteRegistrySubKeySafe(Registry.CurrentUser, ConnectionRegistryManagerTest.TestEdiKeyName);
					AssertNull("Should have deleted folder", Registry.CurrentUser.OpenSubKey(ConnectionRegistryManagerTest.TestEdiKeyName));
				}
			}
		}

		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		public void TestSignalRDoesNotBlowUpWithNoOnHubClientControllerCreatedAssigned()
		{
			var configName1 = "webConfig1";
			var registryManager = new ConnectionRegistryManagerForTest();
			registryManager.LoadFromRegistry(configName1);
			using (UpdateProcessor.OverrideConnectionRegistryManagerForTest(registryManager))
			{
				var log = new StringBuilder();
				var mockResponseProcessor = new Mock<IErrorResponseWebRequestProcessor>();

				MethodDelegate retryAction = null;
				Exception outException = null;
				mockResponseProcessor.Setup(r => r.Process(It.IsAny<MethodDelegate>(), out retryAction, out outException, It.IsAny<bool>()))
					.Returns<MethodDelegate, MethodDelegate, Exception, bool>((action, a2, a3, a4) =>
					{
						return action.Invoke();
					});

				var mock = new Mock<IRemotePrintingServiceAdaptor>();
				mock.Setup(m => m.ResponseProcessor).Returns(mockResponseProcessor.Object);
				var webClient = new WebClientForTesting(mock.Object, true);
				var printJobManager = new PrintManagerForTesting();
				printJobManager.AddInstalledMockQueues("queue1", "queue2");
				var testController = new ControllerForTesting("~TEST_SERVER", printJobManager, webClient);

				ErrorReporter.Instance = new Mock<ErrorReporter>().Object;

				try
				{
					var webConfig1 = new WebClientConfiguration("url1", "user1", "pwd1", 1, "machine1", false, "proxy1", 43, "proxyuser1", "proxypwd1", false, false, 30, false, 0, 0, 100, 100, true, false, 0, 0, 0, false, 0, 0, false, 0, new WebClientUpdateConfiguration(), false, 100);
					Registry.CurrentUser.CreateSubKey(ConnectionRegistryManagerTest.TestEdiKeyName);
					testController.ConfigName = configName1;
					testController.SaveConfig(configName1, webConfig1);
					testController.ShowInformation += (sender, e) => { log.AppendLine(e.Message); };
					testController.OnHubClientControllerCreated = null;
					testController.Run();

					CombineAssertions(() =>
					{
						AssertNotContains("When SignalR is enabled the disabled log shouldn't be present.", "SignalR is disabled. Skipping hub connection.", log.ToString());
						AssertNotContains("When OnHubClientControllerCreated Action is null, should not blow up with null ref exception",
							"System.NullReferenceException: Object reference not set to an instance of an object.", log.ToString());
						AssertContains("When SignalR is enabled. We should attempt to connect & notify the user", "SignalR.", log.ToString());
					});
				}
				finally
				{
					testController.DeleteConfig(configName1);
					ErrorReporter.Instance = null;
					DeleteRegistrySubKeySafe(Registry.CurrentUser, ConnectionRegistryManagerTest.TestEdiKeyName);
					AssertNull("Should have deleted folder", Registry.CurrentUser.OpenSubKey(ConnectionRegistryManagerTest.TestEdiKeyName));
				}
			}
		}

		public void TestThatJobStatusesAreSetCorrectly()
		{
			const string queueName = "Q1";
			var processedJob = GetNewPrintJob(queueName, "Good");
			var failedJob = GetNewPrintJob(queueName, "Bad");
			var unprocessedJob = GetNewPrintJob(queueName, "Nothing");

			var config = Configurator.GetProxyDefaultSystemSettings();
			var webClient = new WebClientForTesting(new RemotePrintingServiceAdaptor(config));
			var printManager = new PrintManagerForTesting();

			printManager.JobsToThrow.Add(failedJob.JobPk);

			var controller = new ControllerForTesting("~TEST_SERVER", printManager, webClient);

			controller.MockInstalledQueues.Clear();
			controller.AddInstalledMockQueues(queueName);

			controller.UploadQueueListToServerAndReturnQueueCount_Exposed();

			controller.MockJobsFromServer.Clear();
			controller.MockJobsFromServer.AddRange(new[] { processedJob, failedJob, unprocessedJob });
			controller.DownloadJobsSendToPrinterAndReturnSuccessToServer_Exposed();

			AssertEquals("The job that was processed should have been saved", "OK", processedJob.BlobType);
			AssertEquals("The job should have been marked as a failure", failedJob.JobPk, webClient.LastFailedJobs.Single().JobPk);

			// BlobType is set to OK if a job is processed
			AssertEquals("The last job should not have been reached", "", unprocessedJob.BlobType);
		}

		public void TestDownloadJobsSendToPrinterAndReturnSuccessToServer()
		{
			ControllerForTesting testController = new ControllerForTesting("~TEST_SERVER");

			string queueName1 = "Q1";
			string queueName2 = "Q2";

			testController.MockInstalledQueues.Clear();
			testController.AddInstalledMockQueues(queueName1, queueName2);

			testController.UploadQueueListToServerAndReturnQueueCount_Exposed();

			var job0 = GetNewPrintJob("QInexisting", "J0");
			var job1 = GetNewPrintJob(queueName1, "Q1J1");
			var job2 = GetNewPrintJob(queueName2, "Q2J1");
			var job3 = GetNewPrintJob(queueName2, "Q2J2");

			testController.MockJobsFromServer.Clear();
			testController.MockJobsFromServer.AddRange(new[] { job0, job1, job2, job3 });

			int numberOfJobsPrinted = testController.DownloadJobsSendToPrinterAndReturnSuccessToServer_Exposed();

			AssertEquals("Number of Jobs Printed", 3, numberOfJobsPrinted);

			AssertEquals("Job0 EmailSubjectLine", "J0", testController.MockJobsFromServer[0].EmailSubjectLine);
			AssertEquals("Job0 BlobType", "", testController.MockJobsFromServer[0].BlobType);
			AssertEquals("Job0 Copies", 0, testController.MockJobsFromServer[0].Copies);
			AssertEquals("Job0 Queue", "QInexisting", testController.MockJobsFromServer[0].QueueName);

			AssertEquals("Job1 EmailSubjectLine", "Q1J1", testController.MockJobsFromServer[1].EmailSubjectLine);
			AssertEquals("Job1 BlobType", "OK", testController.MockJobsFromServer[1].BlobType);
			AssertEquals("Job1 Copies", 1, testController.MockJobsFromServer[1].Copies);
			AssertEquals("Job1 Queue", queueName1, testController.MockJobsFromServer[1].QueueName);

			AssertEquals("Job2 EmailSubjectLine", "Q2J1", testController.MockJobsFromServer[2].EmailSubjectLine);
			AssertEquals("Job2 BlobType", "OK", testController.MockJobsFromServer[2].BlobType);
			AssertEquals("Job2 Copies", 1, testController.MockJobsFromServer[2].Copies);
			AssertEquals("Job2 Queue", queueName2, testController.MockJobsFromServer[2].QueueName);

			AssertEquals("Job3 EmailSubjectLine", "Q2J2", testController.MockJobsFromServer[3].EmailSubjectLine);
			AssertEquals("Job3 BlobType", "OK", testController.MockJobsFromServer[3].BlobType);
			AssertEquals("Job3 Copies", 1, testController.MockJobsFromServer[3].Copies);
			AssertEquals("Job3 Queue", queueName2, testController.MockJobsFromServer[3].QueueName);

			AssertEquals("Should have 1 failed Job.", 1, testController.LastFailedJobs.Length);

			var failedJob = testController.LastFailedJobs.Single();
			AssertEquals("Failed Job PK should match.", job0.JobPk, failedJob.JobPk);
			AssertEquals("Failure Reason should be correct.", "WHERE IS THE PRINTER?!", failedJob.FailureReason);

			job0.QueueName = queueName1;
			testController.ProcessedJobsPkCache_Exposed.Clear();
			numberOfJobsPrinted = testController.DownloadJobsSendToPrinterAndReturnSuccessToServer_Exposed();

			AssertEquals("Number of Jobs Printed", 4, numberOfJobsPrinted);

			AssertEquals("Job0 BlobType", "OK", testController.MockJobsFromServer[0].BlobType);
			AssertEquals("Job0 Copies", 1, testController.MockJobsFromServer[0].Copies);
			AssertEquals("Job0 Queue", queueName1, testController.MockJobsFromServer[0].QueueName);

			AssertEquals("Job1 Copies", 2, testController.MockJobsFromServer[1].Copies);
			AssertEquals("Job2 Copies", 2, testController.MockJobsFromServer[2].Copies);
			AssertEquals("Job3 Copies", 2, testController.MockJobsFromServer[3].Copies);
		}

		public void TestDownloadJobsSendToPrinterAndReturnSuccessToServer_WithFailingJob()
		{
			var printManager = new PrintManagerForTesting();
			var testController = new ControllerForTesting("~TEST_SERVER", printManager, null);
			testController.ShowInformation += Controller_OnShowInformation;

			var queueName1 = "Q1";
			var queueName2 = "Q2";
			var queueName3 = "Q3";

			testController.MockInstalledQueues.Clear();
			testController.AddInstalledMockQueues(queueName1, queueName2, queueName3);

			testController.UploadQueueListToServerAndReturnQueueCount_Exposed();

			var job0 = GetNewPrintJob("QInexisting", "J0");
			var job1 = GetNewPrintJob(queueName1, "Q1J1");
			var job2 = GetNewPrintJob(queueName2, "Q2J1");
			var job3 = GetNewPrintJob(queueName2, "Q2J2");
			var job4 = GetNewPrintJob(queueName3, "Q3J1");
			printManager.JobsToFailWithError.Add(job4.JobPk);

			testController.MockJobsFromServer.Clear();
			testController.MockJobsFromServer.AddRange(new[] { job0, job1, job2, job3, job4 });

			int numberOfJobsPrinted = testController.DownloadJobsSendToPrinterAndReturnSuccessToServer_Exposed();

			AssertEquals("Number of Jobs Printed", 3, numberOfJobsPrinted);

			// Due to multi-threaded printing - jobs can be printed in any order and simultaneously
			AssertContainsExactLinesInAnyOrder(@"Sending print client queue list to server
Installed Printers:
	Q1
	Q2
	Q3

Downloading job(s) from server
Got 5 print job(s), 0 bytes
Synchronising changed print queues
Printing job(s)
Begin printing job [J0]
Printing job [J0]
Printed job [J0]
End printing job [J0]
Begin printing job [Q1J1]
Printing job [Q1J1]
Printed job [Q1J1]
End printing job [Q1J1]
Begin printing job [Q2J1]
Printing job [Q2J1]
Printed job [Q2J1]
End printing job [Q2J1]
Begin printing job [Q2J2]
Printing job [Q2J2]
Printed job [Q2J2]
End printing job [Q2J2]
Begin printing job [Q3J1]
Printing job [Q3J1]
Printed job [Q3J1]
End printing job [Q3J1]
Returning success to server
Returning failed jobs to the server
The server will attempt to notify the sender of the documents that have failed three times.
The following printers are no longer accessible:
	QInexisting
	Q3
2 print jobs are still referencing these printers and will not be printed unless these printers are reinstalled.
The printers list will be refreshed.

", logs.ToString());
		}

		void Controller_OnShowInformation(object sender, LogEventArgs e)
		{
			lock (logs)
			{
				logs.AppendLine(e.Message);
			}
		}
		readonly StringBuilder logs = new StringBuilder();

		public void TestUploadQueueListToServerAndReturnQueueCount()
		{
			var testController = new ControllerForTesting("S1", null, null);

			var queue1 = new ServerPrintQueueForTest { Name = "Q1" };
			var queue2 = new ServerPrintQueueForTest { Name = "Q2" };
			var queue3 = new ServerPrintQueueForTest { Name = "Q3" };

			testController.MockServerSideQueues.Clear();
			testController.MockServerSideQueues.Add(queue1.Name, queue1);
			testController.MockServerSideQueues.Add(queue2.Name, queue2);
			testController.MockServerSideQueues.Add(queue3.Name, queue3);

			AssertEquals("[PRE-CONDITION] Test Queues Count", 3, testController.MockServerSideQueues.Count);

			testController.MockInstalledQueues.Clear();
			testController.AddInstalledMockQueues(queue1.Name);
			testController.AddInstalledMockQueues(queue3.Name);
			testController.AddInstalledMockQueues("QNew");

			testController.UploadQueueListToServerAndReturnQueueCount_Exposed();

			AssertEquals("Test Queues Count - After", 4, testController.MockServerSideQueues.Count);

			AssertEquals("Queue1 Name", queue1.Name, testController.MockServerSideQueues[queue1.Name].Name);
			AssertEquals("Queue1 DisplayName", "OLD[S1].[Q1]", testController.MockServerSideQueues[queue1.Name].DisplayName);

			AssertEquals("Queue2 Name", queue2.Name, testController.MockServerSideQueues[queue2.Name].Name);
			AssertEquals("Queue2 DisplayName", null, testController.MockServerSideQueues[queue2.Name].DisplayName);

			AssertEquals("Queue3 Name", queue3.Name, testController.MockServerSideQueues[queue3.Name].Name);
			AssertEquals("Queue3 DisplayName", "OLD[S1].[Q3]", testController.MockServerSideQueues[queue3.Name].DisplayName);

			AssertEquals("Queue4 Name", "QNew", testController.MockServerSideQueues["QNew"].Name);
			AssertEquals("Queue4 DisplayName", "NEW[S1].[QNew]", testController.MockServerSideQueues["QNew"].DisplayName);
		}

		public void TestUploadQueueListToServerUsesNewAPI()
		{
			var testController = new ControllerForTesting("S1", null, null);

			testController.MockInstalledQueues.Clear();
			testController.MockInstalledQueues.Add(new PrinterInfo("Queue1", true, false));
			testController.MockInstalledQueues.Add(new PrinterInfo("Queue2", true, true));
			testController.MockInstalledQueues.Add(new PrinterInfo("Queue3", false, false)); // Should not be added
			testController.MockInstalledQueues.Add(new PrinterInfo("Queue4", false, true)); // Should not be added

			testController.UploadQueueListToServerAndReturnQueueCount_Exposed();

			AssertEquals(2, testController.MockServerSideQueues.Count);

			AssertEquals("Queue1", testController.MockServerSideQueues["Queue1"].Name);
			AssertEquals(true, testController.MockServerSideQueues["Queue1"].AllowPrinting);

			AssertEquals("Queue2", testController.MockServerSideQueues["Queue2"].Name);
			AssertEquals(false, testController.MockServerSideQueues["Queue2"].AllowPrinting);
		}

		[ExpectNoExceptions()]
		public void TestDownloadAntSetWatermarkInfo()
		{
			var testController = new ControllerForTesting("~TEST_SERVER");

			testController.MockWatermarkFromServer.HorizontalAlignment = "TEST_HA";
			testController.MockWatermarkFromServer.VerticalAlignment = "TEST_VA";
			testController.MockWatermarkFromServer.TextWatermark = "MARCA D'AQUA";
			testController.DownloadAntSetWatermarkInfo_Exposed();
		}

		SerialisablePrintJob GetNewPrintJob(string queueName, string subjectLine)
		{
			return new SerialisablePrintJob
			{
				JobPk = Guid.NewGuid(),
				EmailSubjectLine = subjectLine,
				BlobType = "",
				Copies = 0,
				QueueName = queueName,
				QueueStateChangedStamp = Guid.NewGuid()
			};
		}

		public void TestClearOldFilesInError()
		{
			var testController = new ControllerForTesting("~TEST_SERVER");
			var fileName1 = CreateFile();
			var fileName2 = CreateFile();
			var fileName3 = CreateFile();
			var fileName4 = CreateFile();

			try
			{
				var fileInfo2 = new FileInfo(fileName2);
				fileInfo2.CreationTime = DateTime.Now - TimeSpan.FromDays(testController.NumberOfDaysToKeep_Exposed + 2);
				fileInfo2.Refresh();

				var fileInfo3 = new FileInfo(fileName3);
				fileInfo3.CreationTime = DateTime.Now - TimeSpan.FromDays(testController.NumberOfDaysToKeep_Exposed - 2);
				fileInfo3.Refresh();

				var fileInfo4 = new FileInfo(fileName4);
				fileInfo4.CreationTime = DateTime.Now - TimeSpan.FromDays(testController.NumberOfDaysToKeep_Exposed + 3);
				fileInfo4.Refresh();

				Assert(File.Exists(fileName1));
				Assert(File.Exists(fileName2));
				Assert(File.Exists(fileName3));
				Assert(File.Exists(fileName4));

				testController.ForceFlagForClearOldFilesInError();
				testController.ClearOldFilesInError_Exposed();

				Assert(File.Exists(fileName1));
				Assert(!File.Exists(fileName2));
				Assert(File.Exists(fileName3));
				Assert(!File.Exists(fileName4));
			}
			finally
			{
				DeleteIfExists(fileName1);
				DeleteIfExists(fileName2);
				DeleteIfExists(fileName3);
				DeleteIfExists(fileName4);
				testController.DeleteConfig("test");
			}
		}

		string CreateFile()
		{
			if (!Directory.Exists(Engine.Constants.ErrorDir))
			{
				Directory.CreateDirectory(Engine.Constants.ErrorDir);
			}
			var fileName = Path.Combine(Engine.Constants.ErrorDir, Guid.NewGuid().ToString("N") + ".tmp");
			using (File.Create(fileName))
			{ }
			return fileName;
		}

		#region Exception message

		public void TestExceptionMessageContainsStacktraceIncludingInnerExceptions()
		{
			var innerEx = GetThrownException("Inner exception message");
			var ex = GetThrownException("This is a message", innerEx);

			string message = ControllerForTesting.GetExceptionMessageAndStacktrace_Exposed(ex);

			Assert("Message should contain exception Message property", message.Contains("This is a message"));
			Assert("Message should contain inner exception Message property", message.Contains("Inner exception message"));

			int indexOfFirstStacktrace = message.IndexOf("GetThrownException");
			int indexOfSecondStacktrace = message.IndexOf("GetThrownException", indexOfFirstStacktrace + 1);

			Assert("Message should contain exception Message property", indexOfFirstStacktrace > 0);
			Assert("Message should contain inner exception Message property", indexOfSecondStacktrace > 0);
		}

		Exception GetThrownException(string message, Exception inner = null)
		{
			try
			{
				throw new Exception(message, inner);
			}
			catch (Exception ex)
			{
				return ex;
			}
		}

		#endregion

		public void TestProcessUnhandledError_CheckInnerException()
		{
			var testController = new ControllerForTesting("~TEST_SERVER", null, null);
			var ex = new WebException("Error happened", WebExceptionStatus.Timeout);
			var topEx = new ApplicationException("Something", ex);

			var args = new ErrorHandlingArgs(topEx, false);

			AssertEquals("Precondition", false, args.Handled);

			testController.ProcessUnhandledErrorExposed(null, args);

			AssertEquals("Should handled supported inner exception", true, args.Handled);
		}

		public void TestInitialiseAndResetErrorReporterRestartApplication()
		{
			var testReporter = new ErrorReporterForTest();
			ErrorReporter.Instance = testReporter;
			try
			{
				var errorMessage = new StringBuilder();
				var mock = new Mock<IRemotePrintingServiceAdaptor>();
				var webClient = new WebClientForTesting(mock.Object, true);
				var testController = new ControllerForTestingRestartApplication("~TEST_SERVER", null, webClient);
				testController.RestartApplication += (sender, s) => errorMessage.Append(s.Message);
				testController.InitialiseWebServiceClient_Exposed();

				testReporter.HandleOrReport(new CargoWise.PdfiumWrapper.CannotFoundPdfiumLibraryException("Canot found pdfium.ll for test", new DllNotFoundException("Canot found dll for test")), false);
				AssertEquals("RestartApplication has been called", "Canot found pdfium.ll for test", errorMessage.ToString());

				errorMessage.Clear();
				testController.ResetWebServiceClient_Exposed();
				testReporter.HandleOrReport(new CargoWise.PdfiumWrapper.CannotFoundPdfiumLibraryException("Canot found pdfium.ll for test", new DllNotFoundException("Canot found dll for test")), false);

				AssertEquals("Does not call RestartApplication", "", errorMessage.ToString());
			}
			finally
			{
				ErrorReporter.Instance = null;
			}
		}

		public void TestFailureToAutoUpdate_ThrowsException()
		{
			ErrorReporter.Instance = new ErrorReporterForTest();
			var logs = new List<string>();
			var mock = new Mock<IRemotePrintingServiceAdaptor>();
			var webClient = new WebClientForTesting(mock.Object, true);
			var testController = new ControllerForTestingWithWebException2("~TEST_SERVER", null, webClient);
			testController.ShowError += (_, e) => logs.Add(e.Message);
			var configName1 = "webConfig1";
			try
			{
				var webConfig1 = new WebClientConfiguration("url1", "user1", "pwd1", 1, "machine1", false, "proxy1", 43, "proxyuser1", "proxypwd1", false, false, 30, false, 0, 0, 100, 100, false, false, 0, 0, 0, false, 3, 3, false, 0, new WebClientUpdateConfiguration(), false, 100);
				testController.ConfigName = configName1;
				testController.ShouldLoopOver = true;
				testController.ThrowException = 1;
				testController.SaveConfig(configName1, webConfig1);
				testController.Process_Exposed();

				const string expectedMessage = "* * * Failed * * *\r\nNot Web Exception\r\nSystem.Exception";

				AssertContains(expectedMessage, logs[0]);
			}
			finally
			{
				testController.DeleteConfig(configName1);
				ErrorReporter.Instance = null;
			}
		}

		public void TestFailureToAutoUpdate_HandlesWebException()
		{
			ErrorReporter.Instance = new ErrorReporterForTest();
			var logs = new List<string>();
			var mock = new Mock<IRemotePrintingServiceAdaptor>();
			var webClient = new WebClientForTesting(mock.Object, true);
			var testController = new ControllerForTestingWithWebException2("~TEST_SERVER", null, webClient);
			testController.ShowInformation += (_, e) => logs.Add(e.Message);
			var configName1 = "webConfig1";
			try
			{
				var webConfig1 = new WebClientConfiguration("url1", "user1", "pwd1", 1, "machine1", false, "proxy1", 43, "proxyuser1", "proxypwd1", false, false, 30, false, 0, 0, 100, 100, false, false, 0, 0, 0, false, 3, 3, false, 0, new WebClientUpdateConfiguration(), false, 100);
				testController.ConfigName = configName1;
				testController.ShouldLoopOver = true;
				testController.ThrowException = 2;
				testController.SaveConfig(configName1, webConfig1);
				testController.Process_Exposed();

				const string expectedMessage = "Update failed: Web Exception";

				AssertCollectionContains(expectedMessage, logs);
			}
			finally
			{
				testController.DeleteConfig(configName1);
				ErrorReporter.Instance = null;
			}
		}

		public void TestProcessedJobsPkCache()
		{
			var mock = new Mock<IRemotePrintingServiceAdaptor>();
			var webClient = new WebClientForTesting(mock.Object, true);
			var testController = new ControllerForTesting("~TEST_SERVER", null, webClient) { LoopCount = 1 };
			mock.Setup(i => i.SetJobSuccess(It.IsAny<Guid[]>()))
				.Callback(new Action<Guid[]>(p =>
				{
					if (testController.LoopCount == 1)
					{
						throw new WebException();
					}
					webClient.SetJobSuccessMockCall(p);
				}));

			mock.Setup(i => i.SetJobFailure(It.IsAny<PrintJobFailed[]>()))
				.Callback(new Action<PrintJobFailed[]>(p =>
				{
					if (testController.LoopCount == 1)
					{
						throw new WebException();
					}
					webClient.SetJobFailureMockCall(p);
				}));

			var queueName1 = "Q1";
			var queueName2 = "Q2";

			testController.MockInstalledQueues.Clear();
			testController.AddInstalledMockQueues(queueName1, queueName2);

			testController.UploadQueueListToServerAndReturnQueueCount_Exposed();

			var job0 = GetNewPrintJob(queueName1, "Q1J1");
			var job1 = GetNewPrintJob(queueName1, "Q1J2");
			var job2 = GetNewPrintJob(queueName2, "Q2J1");
			var job3 = GetNewPrintJob(queueName2, "Q2J2");
			var job4 = GetNewPrintJob(queueName2, "Q2J3");

			testController.MockJobsFromServer.Clear();
			testController.MockJobsFromServer.AddRange(new[] { job0, job1, job2, job3, job4 });

			testController.ProcessedJobsPkCache_Exposed.Add(job0.JobPk);
			testController.ProcessedJobsPkCache_Exposed.Add(job3.JobPk);

			var numberOfJobsPrinted = testController.LoopDownloadJobsSendToPrinterAndReturnSuccessToServer();
			AssertEquals(3, numberOfJobsPrinted);
			AssertEquals(3, testController.ProcessedJobsPkCache_Exposed.Count);
		}

		protected override void TearDown()
		{
			JobStatusFileWriter.Delete();
			ConnectionRegistryManagerForTest.DeleteWebPrintConfig();
			base.TearDown();
		}

		void DeleteRegistrySubKeySafe(RegistryKey parentKey, string subKeyName)
		{
			if (parentKey != null && !string.IsNullOrEmpty(subKeyName) && parentKey.OpenSubKey(subKeyName) != null)
			{
				parentKey.DeleteSubKeyTree(subKeyName);
			}
		}
	}

	public class ControllerForTesting : PrintController
	{
		public ControllerForTesting(string localPrintServer, PrintManager printManager = null, WebClient webClient = null)
			: this(localPrintServer, printManager ?? new PrintManagerForTesting())
		{
			fWebServiceClient = webClient ?? new WebClientForTesting(new RemotePrintingServiceAdaptor(ConfigSetting));
		}

		ControllerForTesting(string localPrintServer, PrintManager printManager)
			: base(localPrintServer, printManager, null)
		{
			PrintManagerForTesting = printManager;
		}

		protected PrintManager PrintManagerForTesting { get; }

		public int LoopCount { get; set; }
		public int LoopDownloadJobsSendToPrinterAndReturnSuccessToServer()
		{
			int result = 0;

			while (LoopCount > 0)
			{
				result += DownloadJobsSendToPrinterAndReturnSuccessToServer(null, out var printJobWithUnhandledException);

				if (printJobWithUnhandledException.Length > 0)
				{
					return 0;
				}

				LoopCount--;
			}

			return result;
		}

		public bool HandleException(Exception ex)
		{
			return base.HandleServerException(ex, true, true);
		}

		public void ProcessUnhandledErrorExposed(object sender, ErrorHandlingArgs e)
		{
			ProcessUnhandledError(sender, e);
		}

		public void CleanOldLogFiles_Exposed() => CleanOldLogFiles();

		public int DownloadJobsSendToPrinterAndReturnSuccessToServer_Exposed()
		{
			return DownloadJobsSendToPrinterAndReturnSuccessToServer(null, out var printJobWithUnhandledException);
		}

		public int UploadQueueListToServerAndReturnQueueCount_Exposed()
		{
			return UploadQueueListToServerAndReturnQueueCount();
		}

		public Watermark DownloadAntSetWatermarkInfo_Exposed()
		{
			return WatermarkFactory.GetWatermark(WebServiceClient.GetWatermarkInfo());
		}

		public void LogMemoryUsage_Exposed() => base.LogMemoryUsage();

		public WebClientConfiguration ConfigSetting_Exposed => base.ConfigSetting;

		public int NumberOfDaysToKeep_Exposed
		{
			get { return ControllerForTesting.numberOfDaysToKeep; }
		}

		public void ForceFlagForClearOldFilesInError()
		{
			lastClearOldFilesInError = DateTime.MinValue;
		}

		public void ClearOldFilesInError_Exposed()
		{
			ClearOldFilesInError();
		}

		public PrintJobFailed[] LastFailedJobs => ((WebClientForTesting)WebServiceClient).LastFailedJobs;

		public List<SerialisablePrintJob> MockJobsFromServer
		{
			get => ((WebClientForTesting)WebServiceClient).MockJobsFromServer;
		}

		public SerialisableWatermark MockWatermarkFromServer
		{
			get => ((WebClientForTesting)WebServiceClient).MockWatermarkFromServer;
		}

		public Dictionary<string, ServerPrintQueueForTest> MockServerSideQueues
		{
			get => ((WebClientForTesting)WebServiceClient).MockServerSideQueues;
		}

		public virtual List<PrinterInfo> MockInstalledQueues
		{
			get => ((PrintManagerForTesting)PrintManagerForTesting).MockInstalledQueues;
		}

		public void AddInstalledMockQueues(params string[] queueNames)
		{
			((PrintManagerForTesting)PrintManagerForTesting).AddInstalledMockQueues(queueNames);
		}

		public bool IsPrintQueuesRefreshRequired_Exposed()
		{
			return IsPrintQueuesRefreshRequired();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		protected override void PauseJobWhenRequested(int numberOfJobs, int requestPauseInSeconds)
		{
			if (!ShouldLoopOver)
			{
				Stop();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public void PauseJobWhenRequestedExposed(int numberOfJobs, int requestPauseInSeconds) => base.PauseJobWhenRequested(numberOfJobs, requestPauseInSeconds);

		protected override ConnectionRegistryManager ConnectionRegistryManager => connectionRegistryManager ?? (connectionRegistryManager = new ConnectionRegistryManagerForTest());

		ConnectionRegistryManager connectionRegistryManager;

		public void SaveConfig(string configName, WebClientConfiguration config)
		{
			ConnectionRegistryManager.LoadFromRegistry(configName);
			ConnectionRegistryManager.SaveRemotePrintingRegistryValues(config);
			ConfigSetting = config;
		}

		public void DeleteConfig(string configName)
		{
			ConnectionRegistryManager.DeleteFromRegistry(configName);
		}

		public bool ShouldLoopOver;

		public int QueuesCount_Exposed
		{
			get => queuesCount;
			set => queuesCount = value;
		}

		protected override IClientUpdate GetClientUpdate()
		{
			// should get fixed properly in an Amnesty see http://crikey.wtg.zone/failures/testFailureHistory/04dfe7c4-47f5-491c-af23-e9893c980a0d
			var clientUpdate = new ClientUpdate { Link = "http://www.test.com", Version = "2.0.25" };
			return new ClientUpdateWrapper(clientUpdate);
		}
		public static string GetExceptionMessageAndStacktrace_Exposed(Exception ex) => GetExceptionMessageAndStacktrace(ex);

		public HashSet<Guid> ProcessedJobsPkCache_Exposed => ProcessedJobsPkCache;
	}

	class ControllerForTestingWithWebException : ControllerForTesting
	{
		public ControllerForTestingWithWebException(string localPrintServer, PrintManager printManager = null, WebClient webClient = null)
			: base(localPrintServer, printManager, webClient)
		{
		}

		public bool ThrowException
		{
			get => throwException;
			set => throwException = value;
		}
		bool throwException;

		public void Process_Exposed() => Process();

		public void InitialiseWebServiceClient_Exposed() => InitialiseWebServiceClient();

		protected override void Process()
		{
			if (throwException)
			{
				throw new WebException("Exception handled and not reported.", System.Net.WebExceptionStatus.Timeout);
			}
			else
			{
				base.Process();
			}
		}

		protected override IClientUpdate GetClientUpdate()
		{
			if (throwException)
			{
				throw new WebException("Exception handled and not reported.", System.Net.WebExceptionStatus.NameResolutionFailure);
			}
			return base.GetClientUpdate();
		}
	}

	class ControllerForTestingWithWebException2 : ControllerForTesting
	{
		public ControllerForTestingWithWebException2(string localPrintServer, PrintManager printManager = null, WebClient webClient = null)
			: base(localPrintServer, printManager, webClient)
		{
		}

		public int ThrowException { get; set; }

		public void Process_Exposed() => Process();

		public void InitialiseWebServiceClient_Exposed() => InitialiseWebServiceClient();

		protected override IClientUpdate GetClientUpdate()
		{
			if (ThrowException == 1)
			{
				ShouldLoopOver = false;
				throw new Exception("Not Web Exception");
			}
			if (ThrowException == 2)
			{
				ShouldLoopOver = false;
				var ex = new WebException("Web Exception");
				throw new Exception("Not Web Exception", ex);
			}
			return base.GetClientUpdate();
		}
	}

	class ControllerForTestingRestartApplication : ControllerForTesting
	{
		public ControllerForTestingRestartApplication(string localPrintServer, PrintManager printManager = null, WebClient webClient = null)
			: base(localPrintServer, printManager, webClient)
		{
		}

		protected override WebClientConfiguration GetNewConfigSetting(string configName) => new WebClientConfiguration();

		protected override bool IsMainController => false;

		public void InitialiseWebServiceClient_Exposed() => InitialiseWebServiceClient();

		public void ResetWebServiceClient_Exposed() => ResetWebServiceClient();
	}
}
