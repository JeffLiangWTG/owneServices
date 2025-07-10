using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data;
using Enterprise.ErrorReporting.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.ErrorReporting;

namespace Enterprise.ErrorReporting.ServiceTasks.Test
{
	[TestDate]
	public abstract class ReportErrorsServiceTaskTestBase : ServiceTaskTestCase<ReportErrorsServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						StmErrorReportSchema.Constants.TableName,
						"Queued Errors",
						StmErrorReportSchema.Constants.QER_TransmitStatus + "=" + StmErrorReportTransmitStatus.Codes.Queued),
				};
			}
		}

		protected Task TaskFromException(Exception ex)
		{
			var tcs = new TaskCompletionSource<object>();
			tcs.SetException(ex);
			return tcs.Task;
		}

		protected void AssertErrorReportTransmitStatus(Guid pk, string transmitStatus)
		{
			var report = Factory.Load<StmErrorReport>(pk);
			AssertEquals(transmitStatus, report.QER_TransmitStatus);
		}

		protected void AssertErrorReportDoesNotExistWithPK(Guid pk)
		{
			AssertNull(Factory.Load<StmErrorReport>(pk));
		}

		protected void AssertHasNumErrorReports(int num)
		{
			int actualNum;
			using (var cmd = Db.Connection.Command(string.Format("SELECT COUNT(*) FROM [{0}]", StmErrorReportSchema.Constants.TableName)))
			{
				actualNum = (int)cmd.ExecuteScalar();
			}

			AssertEquals(string.Format("Should have {0} error reports", num), num, actualNum);
		}

		protected void AssertErrorReportExistsWithPK(Guid pk)
		{
			AssertNotNull(string.Format("Error report with PK '{0}' should exist.", pk), Factory.Load<StmErrorReport>(pk));
		}

		protected void AssertNoErrorReportExistsWithPK(Guid pk)
		{
			AssertNull(string.Format("Error report with PK '{0}' should not exist.", pk), Factory.Load<StmErrorReport>(pk));
		}

		protected ReportErrorsServiceTask ServiceTask
		{
			get { return serviceTask; }
			set { serviceTask = value; }
		}
		ReportErrorsServiceTask serviceTask;

		protected Mock<IErrorReportingClientProvider> MockErrorReportingClientProvider
		{
			get { return mockErrorReportingClientProvider; }
			set { mockErrorReportingClientProvider = value; }
		}
		Mock<IErrorReportingClientProvider> mockErrorReportingClientProvider;

		protected Mock<IErrorReportingClient> MockErrorReportingClient
		{
			get { return mockErrorReportingClient; }
			set { mockErrorReportingClient = value; }
		}
		Mock<IErrorReportingClient> mockErrorReportingClient;

		protected abstract string GetTestDataSqlFileName();
		protected abstract int GetNumTestErrorReports();

		protected override void SetUpCore()
		{
			base.SetUpCore();
			using (var cmd = Db.Connection.Command(GetCreateTestDataSql()))
			{
				cmd.ExecuteNonQuery();
			}

			serviceTask = new ReportErrorsServiceTask();
			SetUpErrorReportingClientForTask(Task.FromResult(true));

			AssertHasNumErrorReports(GetNumTestErrorReports());

			TestDateAttribute.Date = new DateTime(2014, 08, 28, 12, 51, 34, DateTimeKind.Utc).ToLocalTime();
		}

		protected void SetUpErrorReportingClientForTask(Task task)
		{
			mockErrorReportingClientProvider = new Mock<IErrorReportingClientProvider>();
			serviceTask.ErrorReportingClientProvider = mockErrorReportingClientProvider.Object;

			mockErrorReportingClient = new Mock<IErrorReportingClient>();

			mockErrorReportingClient
				.Setup(x => x.PostCrashReportAsync(It.IsAny<IOpaqueErrorReport>(), It.IsAny<CancellationToken>()))
				.Returns(task);

			mockErrorReportingClientProvider
				.Setup(x => x.CreateClient(It.IsAny<Uri>(), It.IsAny<TimeSpan>()))
				.Returns(mockErrorReportingClient.Object);
		}

		string GetCreateTestDataSql()
		{
			var assembly = GetType().Assembly;
			using (var resourceStream = assembly.GetManifestResourceStream(GetTestDataSqlFileName()))
			using (var reader = new StreamReader(resourceStream))
			{
				return reader.ReadToEnd();
			}
		}
	}
}
