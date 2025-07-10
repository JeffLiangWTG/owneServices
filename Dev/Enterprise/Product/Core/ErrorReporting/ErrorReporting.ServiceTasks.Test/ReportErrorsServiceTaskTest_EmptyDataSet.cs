using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ErrorReporting.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using WTG.ErrorReporting;

namespace Enterprise.ErrorReporting.ServiceTasks.Test
{
	[TestedType(typeof(ReportErrorsServiceTask))]
	sealed class ReportErrorsServiceTaskTest_EmptyDataSet : ReportErrorsServiceTaskTestBase
	{
		public void TestStmErrorReportType()
		{
			var errorReportXml = "<?placeholder LazyLoading=\"Yes\"?>";
			var errorReport = Factory.NewWithValidTestData<StmErrorReport>();
			errorReport.QER_ReportXml = errorReportXml;
			errorReport.QER_TransmitStatus = "QUE";
			errorReport.QER_ReportType = 1234;
			Factory.Save();

			var serviceTask = new ReportErrorsServiceTask();

			var mockErrorReportingClientProvider = new Mock<IErrorReportingClientProvider>();
			serviceTask.ErrorReportingClientProvider = mockErrorReportingClientProvider.Object;

			var mockErrorReportingClient = new Mock<IErrorReportingClient>();

			var sentErrorReport = default(IOpaqueErrorReport);
			mockErrorReportingClient
				.Setup(x => x.PostCrashReportAsync(It.IsAny<IOpaqueErrorReport>(), It.IsAny<CancellationToken>()))
				.Callback((IOpaqueErrorReport report, CancellationToken cancellationToken) =>
				{
					sentErrorReport = report;
				})
				.Returns(Task.CompletedTask)
				.Verifiable();

			mockErrorReportingClientProvider.Setup(x => x.CreateClient(It.IsAny<Uri>(), It.IsAny<TimeSpan>()))
				.Returns(mockErrorReportingClient.Object);

			InitialiseAndRunTaskSchedule(serviceTask);

			mockErrorReportingClient.Verify(x => x.PostCrashReportAsync(It.IsAny<IOpaqueErrorReport>(), It.IsAny<CancellationToken>()), Times.Once);
			AssertEquals((ErrorReportType)(int)errorReport.QER_ReportType, sentErrorReport.ErrorReportType);
		}

		public void TestStmErrorReportType_IsSetToEnterpriseWhenReportTypeIsInvalid()
		{
			var errorReportXml = "<?placeholder LazyLoading=\"Yes\"?>";
			var errorReport = Factory.NewWithValidTestData<StmErrorReport>();
			errorReport.QER_ReportXml = errorReportXml;
			errorReport.QER_TransmitStatus = "QUE";
			errorReport.QER_ReportType = (int)ErrorReportType.Invalid;
			Factory.Save();

			var serviceTask = new ReportErrorsServiceTask();

			var mockErrorReportingClientProvider = new Mock<IErrorReportingClientProvider>();
			serviceTask.ErrorReportingClientProvider = mockErrorReportingClientProvider.Object;

			var mockErrorReportingClient = new Mock<IErrorReportingClient>();

			var sentErrorReport = default(IOpaqueErrorReport);
			mockErrorReportingClient
				.Setup(x => x.PostCrashReportAsync(It.IsAny<IOpaqueErrorReport>(), It.IsAny<CancellationToken>()))
				.Callback((IOpaqueErrorReport report, CancellationToken cancellationToken) =>
				{
					sentErrorReport = report;
				})
				.Returns(Task.CompletedTask)
				.Verifiable();

			mockErrorReportingClientProvider.Setup(x => x.CreateClient(It.IsAny<Uri>(), It.IsAny<TimeSpan>()))
				.Returns(mockErrorReportingClient.Object);

			InitialiseAndRunTaskSchedule(serviceTask);

			mockErrorReportingClient.Verify(x => x.PostCrashReportAsync(It.IsAny<IOpaqueErrorReport>(), It.IsAny<CancellationToken>()), Times.Once);
			AssertEquals(ErrorReportType.EnterpriseXml, sentErrorReport.ErrorReportType);
		}

		protected override string GetTestDataSqlFileName() => "Enterprise.ErrorReporting.ServiceTasks.Test.DeleteTestData.sql";
		protected override int GetNumTestErrorReports() => 0;
	}
}
