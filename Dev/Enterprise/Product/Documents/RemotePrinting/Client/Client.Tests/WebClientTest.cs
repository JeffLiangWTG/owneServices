using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web.Services.Protocols;
using System.Xml;
using Enterprise.RemotePrinting.Client.RemotePrintServer;
using Enterprise.RemotePrinting.Engine;
using Enterprise.RemotePrinting.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class WebClientTest : TestCase
	{
		public void TestCheckClientUpdate()
		{
			using (UpdateProcessor.OverrideInstalledVersionForTest("2.151.1"))
			{
				var log = new StringBuilder();
				var emailLog = new StringBuilder();

				var clientUpdate = new ClientUpdate
				{
					Version = "99.99.99",
					Link = "http://www.test.com"
				};

				var mock = new Mock<IRemotePrintingServiceAdaptor>();
				mock.Setup(i => i.CheckClientUpdate2()).Throws(new TargetInvocationException(new SoapException("Server did not recognize the value of HTTP Header SOAPAction: http://www.cargowise.com/CheckClientUpdate2.", new XmlQualifiedName("abc"))));
				mock.Setup(m => m.CheckClientUpdate()).Returns(clientUpdate);
				mock.Setup(i => i.SendNotificationEmail(It.IsAny<string>(), It.IsAny<string>()))
					.Callback(new Action<string, string>((subject, body) =>
					{
						emailLog.AppendLine(subject).AppendLine(body);
					}));

				var config = new WebClientConfiguration
				{
					LocalMachineName = "MachineName-Jerry",
					UpdateConfiguration = new WebClientUpdateConfiguration
					{
						UpdateWithNotMatchingLastNotificationDate = DateTime.Now.AddDays(-1)
					}
				};

				var client = new WebClient(mock.Object);
				client.SetWebServiceUrlAndCredentials(config);

				var registryManager = new ConnectionRegistryManagerTest.ConnectionRegistryManagerForTest();
				var configName = "configForTest3";

				try
				{
					registryManager.LoadFromRegistry(configName);

					using (UpdateProcessor.OverrideConnectionRegistryManagerForTest(registryManager))
					{
						var update = client.CheckClientUpdate((message) => log.Append(message));
						var expectedEmailMessage = @"Remote Printing Client Update
There is a newer version of the WebPrint Client that is installed on the MachineName-Jerry.
The version of the WebPrint Client that is installed is 2.151.1
The version of the WebPrint Client that is available on the Remote server is 99.99.99

Please follow these steps to update the WebPrint Client to the version on the Remote server:
1. Uninstall the current version of the WebPrint Client on the MachineName-Jerry.
2. Download the latest WebPrint Client from http://www.test.com to the CargoWiseOneWebPrintClientIntSetup.msi.
3. Install the WebPrint Client on the MachineName-Jerry.
4. Start the WebPrint Client.

";

						AssertEquals("Should get client update from CheckClientUpdate", clientUpdate.Version, update.Version);
						AssertEquals("WebPrint Client appears to have newer version than WebPrint Server.", log.ToString());
						AssertEquals(expectedEmailMessage, emailLog.ToString());
					}
				}
				finally
				{
					registryManager.DeleteFromRegistry(configName);
				}
			}
		}

		public void TestGetChangedQueues()
		{
			var mock = new Mock<IRemotePrintingServiceAdaptor>();
			var serverPrintQueues = new[]
			{
				new ServerPrintQueue { IsRollPaper = true }
			};
			mock.Setup(m => m.GetChangedQueues(It.IsAny<string>(), It.IsAny<string[]>())).Returns(serverPrintQueues);

			var client = new WebClient(mock.Object);
			var serializablePrintQueue = client.GetChangedQueues(null, null);
			AssertEquals(1, serializablePrintQueue.Length);
			Assert(serializablePrintQueue[0].IsRollPaper);
		}

		public void TestJobStatusShouldBeSavedWhenInternetConnectionDrops()
		{
			var mock = new Mock<IRemotePrintingServiceAdaptor>();
			mock.Setup(m => m.SetJobSuccess(It.IsAny<Guid[]>())).Throws(new WebException());
			mock.Setup(m => m.SetJobFailure(It.IsAny<PrintJobFailed[]>())).Throws(new WebException());

			var client = new WebClient(mock.Object);
			var pk1Success = Guid.NewGuid();
			var pk2Success = Guid.NewGuid();
			var pk1Fail = Guid.NewGuid();
			var pk2Fail = Guid.NewGuid();

			var pksSuccess = new List<Guid> { pk1Success, pk2Success };

			var failedJobList = new List<PrintJobFailed>();
			var failedJob = new PrintJobFailed
			{
				JobPk = pk1Fail,
				FailureReason = "Test Failure"
			};
			var failedJob2 = new PrintJobFailed
			{
				JobPk = pk2Fail,
				FailureReason = "Test Failure 2"
			};

			failedJobList.Add(failedJob);
			failedJobList.Add(failedJob2);

			AssertExceptionThrown<WebException>(() => client.SetJobSuccess(pksSuccess.ToArray()));
			AssertExceptionThrown<WebException>(() => client.SetJobFailure(failedJobList.ToArray()));
			mock.VerifyAll();

			var jobs = JobStatusFileWriter.ReadAll();
			AssertEquals(4, jobs.Count());
			Assert(jobs.Contains(new JobDetails(pk1Success, "Success", ProcessedStatus.Processed)));
			Assert(jobs.Contains(new JobDetails(pk2Success, "Success", ProcessedStatus.Processed)));
			Assert(jobs.Contains(new JobDetails(pk1Fail, "Test Failure", ProcessedStatus.Failed)));
			Assert(jobs.Contains(new JobDetails(pk2Fail, "Test Failure 2", ProcessedStatus.Failed)));
		}

		public void TestGetJobsCompressed()
		{
			var serverPrintJobs = new[]
			{
				new ServerPrintJobEx {
					JobPk = Guid.NewGuid(),
					Contents = Array.Empty<byte>(),
					JobType  = "PRN",
					EmailSubjectLine = "test",
					EscapeSequence = Array.Empty<byte>(),
					QueueName = "Queue1" }
			};

			var mock = new Mock<IRemotePrintingServiceAdaptor>();
			mock.Setup(m => m.GetJobsCompressed(It.IsAny<string>())).Returns(serverPrintJobs);

			var client = new WebClient(mock.Object);
			var serializablePrintJobs = client.GetJobsCompressed(null);

			AssertEquals(1, serializablePrintJobs.Length);
			AssertEquals("test", serializablePrintJobs[0].EmailSubjectLine);
		}

		public void TestGetJobsCompressed2()
		{
			var serverPrintJobs = new[]
			{
				new ServerPrintJobEx {
					JobPk = Guid.NewGuid(),
					Contents = Array.Empty<byte>(),
					JobType  = "PRN",
					EmailSubjectLine = "test",
					EscapeSequence = Array.Empty<byte>(),
					QueueName = "Queue1" }
			};

			var mock = new Mock<IRemotePrintingServiceAdaptor>();
			mock.Setup(m => m.GetJobsCompressed2(It.IsAny<string>())).Returns(serverPrintJobs);

			var client = new WebClient(mock.Object);
			var serializablePrintJobs = client.GetJobsCompressed2(null);

			AssertEquals(1, serializablePrintJobs.Length);
			AssertEquals("test", serializablePrintJobs[0].EmailSubjectLine);
		}

		public void TestSetQueuesExFallback()
		{
			string fallbackServerName = null;
			string[] fallbackPrintQueueNames = null;

			var mock = new Mock<IRemotePrintingServiceAdaptor>();
			mock.Setup(i => i.SetQueuesEx(It.IsAny<string>(), It.IsAny<PrintQueueInfo[]>())).Throws(new SoapException("Unknown method 'http://www.cargowise.com/SetQueuesEx'", XmlQualifiedName.Empty));
			mock.Setup(i => i.SetQueues(It.IsAny<string>(), It.IsAny<string[]>())).Callback<string, string[]>((serverName, printQueueNames) =>
			{
				fallbackServerName = serverName;
				fallbackPrintQueueNames = printQueueNames;
			});

			var queues = new[]
			{
				new PrinterInfo("P1", true, false), new PrinterInfo("P2", true, true)
			};

			var client = new WebClient(mock.Object);
			client.SetQueuesEx("Server1", queues);

			AssertEquals("Server1", fallbackServerName);

			AssertNotNull(fallbackPrintQueueNames);
			AssertEquals(2, fallbackPrintQueueNames.Length);
			AssertEquals("P1", fallbackPrintQueueNames[0]);
			AssertEquals("P2", fallbackPrintQueueNames[1]);
		}

		public void TestSyncJobStatusSafe_WithRetry()
		{
			var guid1 = Guid.NewGuid();
			var guid2 = Guid.NewGuid();
			var guid3 = Guid.NewGuid();
			var exceptionGuid = Guid.NewGuid();

			var mock = new Mock<IRemotePrintingServiceAdaptor>();
			mock.Setup(m => m.SetJobFailure(It.IsAny<PrintJobFailed[]>())).Callback<PrintJobFailed[]>(failedPrintJobs =>
			{
				if (failedPrintJobs?.Any(j => j.JobPk.Equals(exceptionGuid)) ?? false)
				{
					throw new SoapException("Boom!", XmlQualifiedName.Empty);
				}
			});
			mock.Setup(m => m.SetJobSuccess(It.IsAny<Guid[]>())).Callback<Guid[]>(processedPrintJobs =>
			{
				if (processedPrintJobs?.Any(jobPk => jobPk.Equals(exceptionGuid)) ?? false)
				{
					throw new SoapException("Boom!", XmlQualifiedName.Empty);
				}
			});

			var jobStatuses = new List<JobDetails>
			{
				new(guid1, "Failure 1", ProcessedStatus.Failed),
				new(exceptionGuid, "Failure 2", ProcessedStatus.Failed),
				new(guid2, "Failure 3", ProcessedStatus.Failed),
				new(guid3, "Success", ProcessedStatus.Processed),
				new(exceptionGuid, "Success", ProcessedStatus.Processed)
			};

			var logs = new List<string>();
			var exceptionHandlerCalled = false;

			JobStatusFileWriter.Append(jobStatuses);
			try
			{
				var client = new WebClient(mock.Object);
				client.SyncJobStatusSafe(log => logs.Add(log), (ex, a, b, c) => exceptionHandlerCalled = true);

				AssertEquals("Exception handler should not be called", false, exceptionHandlerCalled);

				var expectedLogs =
$@"Reading 5 processed jobs statuses from local file.
Synchronizing previously processed jobs statuses...
Start updating successful print jobs status. (Jobs: {guid3},{exceptionGuid})
Error updating multiple print jobs statuses. Retrying with individual print jobs.
Failed to update status of print job {exceptionGuid}
Boom!
End updating successful print jobs status.
Start updating failed print jobs status. (Failed jobs: {guid1},{exceptionGuid},{guid2})
Error updating multiple print jobs statuses. Retrying with individual print jobs.
Failed to update status of print job {exceptionGuid}
Boom!
End updating failed print jobs status.
Finished synchronized previously processed jobs statuses.";

				AssertMultilineASCIIEquals(expectedLogs, string.Join(System.Environment.NewLine, logs));

				AssertNull("All job statuses should be cleared", JobStatusFileWriter.ReadAll());
			}
			finally
			{
				JobStatusFileWriter.Delete();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			originalSecurityProtocolType = ServicePointManager.SecurityProtocol;
		}

		protected override void TearDown()
		{
			ServicePointManager.SecurityProtocol = originalSecurityProtocolType;
			JobStatusFileWriter.Delete();
			base.TearDown();
		}

		SecurityProtocolType originalSecurityProtocolType;
	}

	public class WebClientForTesting : WebClient
	{
		public WebClientForTesting(IRemotePrintingServiceAdaptor remotePrintingService, bool isMock = false) : base(remotePrintingService)
		{
			this.isMock = isMock;
		}

		readonly bool isMock;

		public override SerialisablePrintJob[] GetJobsCompressed(string printServer)
		{
			if (isMock)
			{
				return MockJobsFromServer.Where(j => j.BlobType != "OK").ToArray();
			}
			return MockJobsFromServer.ToArray();
		}

		public override SerialisablePrintQueue[] GetChangedQueues(string printServer, string[] changedPrintQueueNames)
		{
			return Array.Empty<SerialisablePrintQueue>();
		}

		public override void SetJobSuccess(Guid[] printJobPks, Action<string> log = null)
		{
			if (isMock)
			{
				try
				{
					base.SetJobSuccess(printJobPks, log);
				}
				catch (WebException)
				{
				}
			}
			else
			{
				SetJobSuccessMockCall(printJobPks);
			}
		}

		public override ICNSWClientApplicationSetting GetCNSWClientApplicationSetting(string machineName)
		{
			return null;
		}

		public void SetJobSuccessMockCall(Guid[] printJobPks)
		{
			foreach (var job in MockJobsFromServer)
			{
				foreach (var jobPk in printJobPks)
				{
					if (job.JobPk == jobPk && MockServerSideQueues.ContainsKey(job.QueueName))
					{
						job.BlobType = "OK";
					}
				}
			}
		}

		public PrintJobFailed[] LastFailedJobs;
		public override void SetJobFailure(PrintJobFailed[] printJobs, Action<string> log = null)
		{
			if (isMock)
			{
				try
				{
					base.SetJobFailure(printJobs, log);
				}
				catch (WebException)
				{
				}
			}
			else
			{
				SetJobFailureMockCall(printJobs);
			}
		}

		public void SetJobFailureMockCall(PrintJobFailed[] jobs)
		{
			LastFailedJobs = jobs;
		}

		public override SerialisableWatermark GetWatermarkInfo()
		{
			return MockWatermarkFromServer;
		}

		public override void SetQueues(string printServer, string[] printQueueNames)
		{
			SetQueuesEx(printServer, printQueueNames.Select(name => new PrintQueueInfo { Name = name, IsSuspectedSurrogate = false }).ToArray());
		}

		public override void SetQueuesEx(string printServer, PrintQueueInfo[] printQueues)
		{
			SetQueuesExWasCalled = true;

			foreach (var queue in printQueues)
			{
				if (MockServerSideQueues.ContainsKey(queue.Name))
				{
					MockServerSideQueues[queue.Name].DisplayName = "OLD[" + printServer + "].[" + queue.Name + "]";
				}
				else
				{
					var newQueue = new ServerPrintQueueForTest();
					newQueue.Name = queue.Name;
					newQueue.DisplayName = "NEW[" + printServer + "].[" + queue.Name + "]";
					newQueue.AllowPrinting = !queue.IsSuspectedSurrogate;
					MockServerSideQueues.Add(queue.Name, newQueue);
				}
			}
		}

		public override void UpdateClientLogs(string recipientEmail, string fileName, byte[] fileData, string comments)
		{
			LastUploadedLogFileName = fileName;
			LastUploadedLogFile = fileData;
			LastUploadedLogComments = comments;
		}

		public string LastUploadedLogFileName { get; set; }

		public byte[] LastUploadedLogFile { get; set; }

		public string LastUploadedLogComments { get; set; }

		public bool SetQueuesExWasCalled { get; set; }

		public List<SerialisablePrintJob> MockJobsFromServer = new List<SerialisablePrintJob>();
		public SerialisableWatermark MockWatermarkFromServer = new SerialisableWatermark();
		public Dictionary<string, ServerPrintQueueForTest> MockServerSideQueues = new Dictionary<string, ServerPrintQueueForTest>();
	}

	public class ServerPrintQueueForTest : ServerPrintQueue
	{
		public bool AllowPrinting { get; set; } = true;
	}
}
