using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.ErrorReporting;

namespace Enterprise.ErrorReporting.ServiceTasks.Test
{
	[UseSnapshotProtection]
	public class ReportErrorsServiceTaskCommandTimeoutTest : TestCase
	{
		public void TestSetNonDefaultTimeoutForDeleteReport()
		{
			// Arrange
			SetupTestEnvironment();

			using (Db.Connection.TemporarySetLockTimeout(DbConnection.LockTimeout.Infinite))
			using (Db.Connection.TemporarySetDefaultCommandTimeOut(1))
			using (var blockerStarted = new AutoResetEvent(false))
			{
				// Lock the DB record for 5 secs
				Task task = new Task(() =>
				{
					using (Db.DisposableActionForDbConnection())
					using (Db.Connection.BeginTransactionWithManager())
					{
						var sqlStatement = @"
								SELECT * FROM dbo.StmErrorReport WITH(UPDLOCK)
								WHERE 1=1
								AND QER_PK = '25FE5DF2-8982-49A4-B8DA-4B8E3B25E961';
							";
						_ = Db.Connection.ExecuteNonQuery(sqlStatement);

						blockerStarted.Set();

						Thread.Sleep(5000);
					}
				});

				task.Start();
				blockerStarted.WaitOne();

				AssertHasNumErrorReports(1);

				try
				{
					// Act & Assert
					AssertNoExceptionThrown(() => serviceTask.RunTask(CancellationToken.None));
				}
				finally
				{
					task.Wait();
				}

				AssertHasNumErrorReports(0);
			}
		}

		protected void SetupTestEnvironment()
		{
			serviceTask = new ReportErrorsServiceTask();
			SetUpErrorReportingClientForTask(Task.CompletedTask);

			var sqlStatement = @"
								DELETE [StmErrorReport];
								
								INSERT INTO [StmErrorReport]
								(
									[QER_PK],
									[QER_ReportXml],
									[QER_TransmitStatus],
									[QER_SystemCreateTimeUtc],
									[QER_SystemCreateUser],
									[QER_SystemLastEditTimeUtc],
									[QER_SystemLastEditUser]
								)
								VALUES
								(
									'25FE5DF2-8982-49A4-B8DA-4B8E3B25E961',
									'<EDI_Exception_Report><ErrorReportID>TEST9</ErrorReportID></EDI_Exception_Report>',
									'SNT',
									DATEADD(DAY, -28, SYSUTCDATETIME()),
									'E',
									DATEADD(DAY, -28, SYSUTCDATETIME()),
									'E'
								);
							";
			using (var cmd = Db.Connection.Command(sqlStatement))
			{
				_ = cmd.ExecuteNonQuery();
			}

			AssertHasNumErrorReports(1);
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

			serviceTask.ServiceLogger = new TestServiceLogger();
		}

		protected void AssertHasNumErrorReports(int num)
		{
			int actualNum;
			using (var cmd = Db.Connection.Command($"SELECT COUNT(*) FROM [{StmErrorReportSchema.Constants.TableName}]"))
			{
				actualNum = (int)cmd.ExecuteScalar();
			}

			AssertEquals($"Should have {num} error reports", num, actualNum);
		}

		ReportErrorsServiceTask serviceTask;

		Mock<IErrorReportingClientProvider> mockErrorReportingClientProvider;

		Mock<IErrorReportingClient> mockErrorReportingClient;
	}
}
