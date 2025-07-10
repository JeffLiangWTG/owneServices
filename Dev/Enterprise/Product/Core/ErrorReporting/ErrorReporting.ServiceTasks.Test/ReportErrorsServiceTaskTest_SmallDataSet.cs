using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data;
using Enterprise.ErrorReporting.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.ErrorReporting;

namespace Enterprise.ErrorReporting.ServiceTasks.Test
{
	[TestedType(typeof(ReportErrorsServiceTask))]
	sealed class ReportErrorsServiceTaskTest_SmallDataSet : ReportErrorsServiceTaskTestBase
	{
		//
		// Check the test data sql to see which records these GUIDs correspond to.
		//

		public void TestDoesNotSendIfThereAreNoErrorReports()
		{
			using (var cmd = Db.Connection.Command(string.Format("DELETE [{0}]", StmErrorReportSchema.Constants.TableName)))
			{
				cmd.ExecuteNonQuery();
			}
			AssertHasNumErrorReports(0);

			var logger = InitialiseAndRunTaskSchedule(ServiceTask);
			Factory.ReloadAll<StmErrorReport>();

			AssertHasNumErrorReports(0);
			MockErrorReportingClientProvider.Verify(x => x.CreateClient(It.IsAny<Uri>(), It.IsAny<TimeSpan?>()), Times.Never);
		}

		public void TestDeletesSentRecordsOlderThan7Days()
		{
			using (ServiceTask.TemporarySetDeleteBatchSize_ForTest(3))
			{
				var logger = InitialiseAndRunTaskSchedule(ServiceTask);
				Factory.ReloadAll<StmErrorReport>();

				AssertHasNumErrorReports(5);
				AssertErrorReportDoesNotExistWithPK(new Guid("89FDF4A7-D9A9-47A0-AF5E-61C659A8FDAF"));
				AssertErrorReportDoesNotExistWithPK(new Guid("AB08D02F-D037-4490-A173-4C292B7C5FD1"));
				AssertErrorReportDoesNotExistWithPK(new Guid("44F0E8B6-61EB-4384-9002-76CB582E0BC9"));
				AssertErrorReportDoesNotExistWithPK(new Guid("25FE5DF2-8982-49A4-B8DA-4B8E3B25E961"));

				AssertErrorReportExistsWithPK(new Guid("FD429F7D-54A8-4CAF-BCFA-853C36ADE13A"));

				AssertEquals(4, logger.Count);
				AssertEquals("Information|Sent 5 error reports", logger[0]);
				AssertEquals("Debug|Marked 5 error reports as 'SNT'", logger[1]);
				AssertEquals("Debug|Deleted 3 stale error reports", logger[2]);
				AssertEquals("Debug|Deleted 1 stale error reports", logger[3]);
			}
		}

		public void TestSendsErrorReportsToSydneyProductionSystem()
		{
			InitialiseAndRunTaskSchedule(ServiceTask);

			MockErrorReportingClientProvider.Verify(x => x.CreateClient(new Uri(WellKnownServiceUris.Production), It.IsAny<TimeSpan?>()));
			MockErrorReportingClient.Verify(x => x.PostCrashReportAsync(It.IsAny<IOpaqueErrorReport>(), It.IsAny<CancellationToken>()));
			Assert("This test is not empty", true);
		}

		public void TestMarksSentErrorsAsSent()
		{
			var logger = InitialiseAndRunTaskSchedule(ServiceTask);
			Factory.ReloadAll<StmErrorReport>();

			AssertErrorReportTransmitStatus(new Guid("AA49A965-6DF2-4356-908B-8E12EB256C3C"), StmErrorReportTransmitStatus.Codes.Sent);
			AssertErrorReportTransmitStatus(new Guid("3F0F71CB-2B67-490F-880A-CBDFCB9146BF"), StmErrorReportTransmitStatus.Codes.Sent);
			AssertErrorReportTransmitStatus(new Guid("469A28CE-B612-4D04-A981-BBE4434B4C3E"), StmErrorReportTransmitStatus.Codes.Sent);

			// The others are > 7 days old and get deleted instantly
			AssertEquals(3, logger.Count);
			AssertEquals("Information|Sent 5 error reports", logger[0]);
			AssertEquals("Debug|Marked 5 error reports as 'SNT'", logger[1]);
			AssertEquals("Debug|Marked 5 error reports as 'SNT'", logger[1]);
			AssertEquals("Debug|Deleted 4 stale error reports", logger[2]);
		}

		public void TestDoesNotMarkErrorsAsSentWhenSendFails()
		{
			SetUpErrorReportingClientForTask(TaskFromException(new Exception("Blargh")));

			var logger = InitialiseAndRunTaskSchedule(ServiceTask);
			Factory.ReloadAll<StmErrorReport>();

			AssertErrorReportTransmitStatus(new Guid("AA49A965-6DF2-4356-908B-8E12EB256C3C"), StmErrorReportTransmitStatus.Codes.Queued);
			AssertErrorReportTransmitStatus(new Guid("3F0F71CB-2B67-490F-880A-CBDFCB9146BF"), StmErrorReportTransmitStatus.Codes.Queued);
			AssertErrorReportTransmitStatus(new Guid("469A28CE-B612-4D04-A981-BBE4434B4C3E"), StmErrorReportTransmitStatus.Codes.Queued);
			AssertErrorReportTransmitStatus(new Guid("89FDF4A7-D9A9-47A0-AF5E-61C659A8FDAF"), StmErrorReportTransmitStatus.Codes.Queued);
			AssertErrorReportTransmitStatus(new Guid("44F0E8B6-61EB-4384-9002-76CB582E0BC9"), StmErrorReportTransmitStatus.Codes.Queued);

			AssertEquals("Error|Failed to upload error report.|System.Exception: Blargh", logger[0].Split(new[] { '\r', '\n' }).First());
		}

		public void TestStillDeletesOldReportsWhenSendFails()
		{
			SetUpErrorReportingClientForTask(TaskFromException(new Exception("Blargh")));

			var logger = InitialiseAndRunTaskSchedule(ServiceTask);
			Factory.ReloadAll<StmErrorReport>();

			AssertErrorReportTransmitStatus(new Guid("AA49A965-6DF2-4356-908B-8E12EB256C3C"), StmErrorReportTransmitStatus.Codes.Queued);
			AssertErrorReportTransmitStatus(new Guid("3F0F71CB-2B67-490F-880A-CBDFCB9146BF"), StmErrorReportTransmitStatus.Codes.Queued);
			AssertErrorReportTransmitStatus(new Guid("469A28CE-B612-4D04-A981-BBE4434B4C3E"), StmErrorReportTransmitStatus.Codes.Queued);
			AssertErrorReportTransmitStatus(new Guid("89FDF4A7-D9A9-47A0-AF5E-61C659A8FDAF"), StmErrorReportTransmitStatus.Codes.Queued);
			AssertErrorReportTransmitStatus(new Guid("44F0E8B6-61EB-4384-9002-76CB582E0BC9"), StmErrorReportTransmitStatus.Codes.Queued);

			AssertErrorReportTransmitStatus(new Guid("EFC3F319-109C-4D65-B48E-7C783D4FB1B3"), StmErrorReportTransmitStatus.Codes.Sent);

			AssertEquals("Error|Failed to upload error report.|System.Exception: Blargh", logger[0].Split(new[] { '\r', '\n' }).First());
		}

		public void TestLogsExceptionWhenSendFails()
		{
			SetUpErrorReportingClientForTask(TaskFromException(new Exception("Blargh")));

			var logger = InitialiseAndRunTaskSchedule(ServiceTask);
			AssertEquals("Error|Failed to upload error report.|System.Exception: Blargh", logger[0].Split(new[] { '\r', '\n' }).First());
		}

		public void TestLogsExceptionWhenSendFailsForTimeout()
		{
			var serviceTask = new ReportErrorsServiceTask();
			var task = TaskFromException(new TaskCanceledException());
			var mockErrorReportingClientProvider = new Mock<IErrorReportingClientProvider>();
			serviceTask.ErrorReportingClientProvider = mockErrorReportingClientProvider.Object;
			var mockErrorReportingClient = new Mock<IErrorReportingClient>();

			mockErrorReportingClient
				.Setup(x => x.PostCrashReportAsync(It.IsAny<IOpaqueErrorReport>(), It.IsAny<CancellationToken>()))
				.Returns(task);

			mockErrorReportingClientProvider.Setup(x => x.CreateClient(It.IsAny<Uri>(), It.IsAny<TimeSpan?>()))
				.Returns(mockErrorReportingClient.Object);

			var logger = InitialiseAndRunTaskSchedule(serviceTask);
			AssertEquals("Error|There was a timeout error for each report. The system will retry to process these items on the next run. Please check your network performance. If error persists, contact WiseTech Global support about increasing timeout limits, or manually edit the timeout value through: System -> Database -> User Options -> RET Service Task HTTP Request Timeout", logger[0].Split(new[] { '\r', '\n' }).First());
			Factory.ReloadAll<StmErrorReport>();
			AssertErrorReportTransmitStatus(new Guid("AA49A965-6DF2-4356-908B-8E12EB256C3C"), StmErrorReportTransmitStatus.Codes.Queued);
			AssertErrorReportTransmitStatus(new Guid("3F0F71CB-2B67-490F-880A-CBDFCB9146BF"), StmErrorReportTransmitStatus.Codes.Queued);
			AssertErrorReportTransmitStatus(new Guid("469A28CE-B612-4D04-A981-BBE4434B4C3E"), StmErrorReportTransmitStatus.Codes.Queued);
			// The others are > 7 days old and get deleted instantly
		}

		public void TestLogsExceptionWhenSendOneByOneAllFailsForTimeout()
		{
			var serviceTask = new ReportErrorsServiceTask();
			var task = TaskFromException(new TaskCanceledException());
			var mockErrorReportingClientProvider = new Mock<IErrorReportingClientProvider>();
			serviceTask.ErrorReportingClientProvider = mockErrorReportingClientProvider.Object;
			var mockErrorReportingClient = new Mock<IErrorReportingClient>();

			mockErrorReportingClient
				.Setup(x => x.PostCrashReportAsync(It.IsAny<IOpaqueErrorReport>(), It.IsAny<CancellationToken>()))
				.Returns(task);

			mockErrorReportingClientProvider.Setup(x => x.CreateClient(It.IsAny<Uri>(), It.IsAny<TimeSpan>()))
				.Returns(mockErrorReportingClient.Object);

			var logger = InitialiseAndRunTaskSchedule(serviceTask);
			AssertEquals("Error|There was a timeout error for each report. The system will retry to process these items on the next run. Please check your network performance. If error persists, contact WiseTech Global support about increasing timeout limits, or manually edit the timeout value through: System -> Database -> User Options -> RET Service Task HTTP Request Timeout", logger[0].Split(new[] { '\r', '\n' }).First());
			Factory.ReloadAll<StmErrorReport>();
			AssertErrorReportTransmitStatus(new Guid("AA49A965-6DF2-4356-908B-8E12EB256C3C"), StmErrorReportTransmitStatus.Codes.Queued);
			AssertErrorReportTransmitStatus(new Guid("3F0F71CB-2B67-490F-880A-CBDFCB9146BF"), StmErrorReportTransmitStatus.Codes.Queued);
			AssertErrorReportTransmitStatus(new Guid("469A28CE-B612-4D04-A981-BBE4434B4C3E"), StmErrorReportTransmitStatus.Codes.Queued);
			// The others are > 7 days old and get deleted instantly
		}

		public void TestStmErrorReportWriteToAsync()
		{
			var errorReport = Factory.NewWithValidTestData<StmErrorReport>();
			errorReport.QER_ReportXml = "<?placeholder LazyLoading=\"Yes\"?>";
			errorReport.QER_TransmitStatus = "QUE";
			Factory.Save();

			var serviceTask = new ReportErrorsServiceTask();

			var mockErrorReportingClientProvider = new Mock<IErrorReportingClientProvider>();
			serviceTask.ErrorReportingClientProvider = mockErrorReportingClientProvider.Object;

			var mockErrorReportingClient = new Mock<IErrorReportingClient>();

			Task MockPostCrashReportAsync(IOpaqueErrorReport report, CancellationToken cancellationToken)
			{
				return Task.Run(() =>
				{
					using (var stream = new MemoryStream())
					{
						return report.WriteToAsync(stream);
					}
				});
			}

			mockErrorReportingClient
				.Setup(x => x.PostCrashReportAsync(It.IsAny<IOpaqueErrorReport>(), It.IsAny<CancellationToken>()))
				.Returns((Func<IOpaqueErrorReport, CancellationToken, Task>)MockPostCrashReportAsync);

			mockErrorReportingClientProvider.Setup(x => x.CreateClient(It.IsAny<Uri>(), It.IsAny<TimeSpan>()))
				.Returns(mockErrorReportingClient.Object);

			InitialiseAndRunTaskSchedule(serviceTask);
		}

		public void TestLogsExceptionWhenSendOneByOneSomeFailsForTimeout()
		{
			var serviceTask = new ReportErrorsServiceTask();
			var mockErrorReportingClientProvider = new Mock<IErrorReportingClientProvider>();
			serviceTask.ErrorReportingClientProvider = mockErrorReportingClientProvider.Object;
			var mockErrorReportingClient = new Mock<IErrorReportingClient>();

			var failedReportsXmlArray = new[] { "<EDI_Exception_Report><ErrorReportID>TEST1</ErrorReportID></EDI_Exception_Report>", "<EDI_Exception_Report><ErrorReportID>TEST2</ErrorReportID></EDI_Exception_Report>" };

			Func<IOpaqueErrorReport, CancellationToken, Task> function = async (IOpaqueErrorReport report, CancellationToken cancellationToken) =>
			{
				var reportText = await report.ToStringAsync(Encoding.UTF8);
				var isFailed = failedReportsXmlArray.Any(p => p.Equals(reportText, StringComparison.Ordinal));
				if (isFailed)
				{
					throw new TaskCanceledException();
				}
			};

			mockErrorReportingClient
				.Setup(x => x.PostCrashReportAsync(It.IsAny<IOpaqueErrorReport>(), It.IsAny<CancellationToken>()))
				.Returns(function);

			mockErrorReportingClientProvider.Setup(x => x.CreateClient(It.IsAny<Uri>(), It.IsAny<TimeSpan>()))
				.Returns(mockErrorReportingClient.Object);

			var logger = InitialiseAndRunTaskSchedule(serviceTask);
			AssertEquals("Debug|Marked 2 error reports as 'FAL'", logger[0]);
			AssertEquals("Error|Some error reports could not be delivered due to timeout errors and were marked as failed.", logger[1].Split(new[] { '\r', '\n' }).First());
			AssertEquals("Debug|Timeout: 100000 milliseconds", logger[2]);
			AssertEquals("Information|Sent 3 error reports", logger[3]);
			AssertEquals("Debug|Marked 3 error reports as 'SNT'", logger[4]);

			Factory.ReloadAll<StmErrorReport>();
			AssertErrorReportTransmitStatus(new Guid("AA49A965-6DF2-4356-908B-8E12EB256C3C"), StmErrorReportTransmitStatus.Codes.Failed);
			AssertErrorReportTransmitStatus(new Guid("3F0F71CB-2B67-490F-880A-CBDFCB9146BF"), StmErrorReportTransmitStatus.Codes.Failed);
			AssertErrorReportTransmitStatus(new Guid("469A28CE-B612-4D04-A981-BBE4434B4C3E"), StmErrorReportTransmitStatus.Codes.Sent);
			// The others are > 7 days old and get deleted instantly
		}

		public void TestSendsToProductionSystemWhenRegistryOverrideNotConfigured()
		{
			InitialiseAndRunTaskSchedule(ServiceTask);
			MockErrorReportingClientProvider.Verify(x => x.CreateClient(new Uri(WellKnownServiceUris.Production), It.IsAny<TimeSpan?>()), Times.AtLeastOnce());
		}

		public void TestSendsToOverrideSystemWhenRegistryOverrideConfigured()
		{
			using (SystemDataRegistry.Instance.ErrorReportingServiceUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://error-reporting.example/"))
			{
				InitialiseAndRunTaskSchedule(ServiceTask);
				MockErrorReportingClientProvider.Verify(x => x.CreateClient(new Uri("https://error-reporting.example/"), It.IsAny<TimeSpan?>()), Times.AtLeastOnce());
			}
		}

		protected override string GetTestDataSqlFileName()
		{
			return "Enterprise.ErrorReporting.ServiceTasks.Test.CreateTestData.sql";
		}

		protected override int GetNumTestErrorReports()
		{
			return 9;
		}
	}
}
