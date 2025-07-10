using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Web;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.Abstractions;
using CargoWiseOne.WebInfrastructure;
using Enterprise.Core;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Common.Test
{
	public class TopLevelWebExceptionHandlerTest : TestCase
	{
		public void TestServiceUnavailableOnDatabaseUpgradedExceptions()
		{
			WebTestingUtilities.DoWithMockHttpContext(() =>
			{
				var exception = new DatabaseUpgradedException();
				TopLevelWebExceptionHandler.HandleUnhandledException(exception);

				AssertEquals("Status Code", (int)HttpStatusCode.ServiceUnavailable, HttpContext.Current.Response.StatusCode);
				ErrorReporter.Clear();
			}, responseMessage =>
			{
				AssertEquals("The Web Application you attempted to access is currently being upgraded. Please try again shortly.", responseMessage);
			});
		}

		public void TestHandleUnhandledException_OrdinaryException_ReturnsInternalServerErrorOnResponseAndGetsReported()
		{
			WebTestingUtilities.DoWithMockHttpContext(() =>
			{
				var exception = new Exception("OrdinaryException");
				TopLevelWebExceptionHandler.HandleUnhandledException(exception);

				AssertEquals("Status Code", (int)HttpStatusCode.InternalServerError, HttpContext.Current.Response.StatusCode);
				AssertEquals(exception, ErrorReporter.LastExceptionReported);
				Assert("Does not show user notificatons on web", UnitTestUserNotification.Instance.LastMessage.WasNone);
				ErrorReporter.Clear();
			});
		}

		[UseSnapshotProtection]
		public void TestHandleUnhandledException_SqlException_ExceptionIsHandledAndGetsReported()
		{
			WebTestingUtilities.DoWithMockHttpContext(() =>
			{
				const int uniqueIndexViolationError = 2601;
				var error = SqlExceptionBuilder.CreateSqlError(uniqueIndexViolationError, 1, 1, "", "PK_UX__QER_PK", "", 1);
				var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
				var sqlException = SqlExceptionBuilder.CreateSqlException(errors);
				TopLevelWebExceptionHandler.HandleUnhandledException(sqlException);

				AssertEquals("Status Code", (int)HttpStatusCode.OK, HttpContext.Current.Response.StatusCode);
				Assert("Does not show user notificatons on web", UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertErrorReportCreated(nameof(SqlException), "PK_UX__QER_PK");
			});
		}

		public void TestHandleUnhandledException_DangerousRequestHttpException_DoesntGetReported()
		{
			WebTestingUtilities.DoWithMockHttpContext(() =>
			{
				var exception = new HttpException((int)HttpStatusCode.BadRequest, "DummyHttp400");
				TopLevelWebExceptionHandler.HandleUnhandledException(exception);
				Assert("Does not show user notificatons on web", UnitTestUserNotification.Instance.LastMessage.WasNone);
				ErrorReporter.Clear();
			});
		}

		public void TestReportInternalServerError_HttpExceptions()
		{
			AssertReportInternalServerError(new HttpException("A potentially dangerous Request.Path value was detected"),
				@"/login/login.aspx?returnurl=/default.aspx",
				"TopLevelWeb_HttpException_ApdR",
				"Internal server error: /login/login.aspx?returnurl=/default.aspx");

			AssertReportInternalServerError(new HttpException("Some unexpected error"),
				@"/RemotePrintingService.asmx/Nudge2",
				"TopLevelWeb_HttpException_Sue",
				"Internal server error: /RemotePrintingService.asmx/Nudge2");
		}

		public void TestReportInternalServerError_OtherExceptions()
		{
			AssertReportInternalServerError(new System.Data.ConstraintException("Column 'OH_PK' is constrained to be unique. Value 'efed528d-14ae-47e5-8033-93af530bd8fa' is already present."),
				@"/AutoLoginRequestHandler.axd",
				"TopLevelWeb_ConstraintException_COic",
				"Internal server error: /AutoLoginRequestHandler.axd");
		}

		public void TestReportInternalServerError_ExceptionMessageEmpty()
		{
			AssertReportInternalServerError(new InvalidOperationException(string.Empty),
				@"/test.axd",
				"TopLevelWeb_InvalidOperationException",
				"Internal server error: /test.axd");
		}

		public void TestReportInternalServerError_ExceptionMessageWithoutWords()
		{
			AssertReportInternalServerError(new InvalidOperationException("1234"),
				@"/test.axd",
				"TopLevelWeb_InvalidOperationException",
				"Internal server error: /test.axd");
		}

		public void TestReportInternalServerError_InnerExceptionTargetSite()
		{
			var a = "string";

			try
			{
				AssertEquals("Precondition", "string", a);
				a = null;
				a.ToLower();
			}
			catch (NullReferenceException e)
			{
				AssertReportInternalServerError(new InvalidOperationException("1234", e),
	@"/test.axd",
	"TopLevelWeb_NullReferenceException_TestReportInternalServerError_InnerExceptionTargetSite_Orns",
	"Internal server error: /test.axd");
			}
		}

		void AssertReportInternalServerError(Exception actualException, string requestPath, string expectedKey, string expectedMessage)
		{
			TopLevelWebExceptionHandler.ReportInternalServerError(actualException, requestPath);
			CombineAssertions(() =>
			{
				AssertEquals("report key", expectedKey, ErrorReporter.LastKeyReported);
				AssertEquals("report message", expectedMessage, ErrorReporter.LastMessageReported);
				AssertEquals(actualException, ErrorReporter.LastExceptionReported);
				ErrorReporter.Clear();
				Assert("Does not show user notificatons on web", UnitTestUserNotification.Instance.LastMessage.WasNone);
			});
		}

		public void TestHandle_HttpExceptions()
		{
			CombineAssertions(() =>
			{
				AssertIgnored(new HttpException(404, "Not found"));
				AssertIgnored(new HttpException(405, "Request format is unrecognized"));
				AssertIgnored(new HttpException(204, "Upload cancelled by user"));
				AssertIgnored(new HttpException(500, "Request timed out."));
				AssertIgnored(new HttpRequestValidationException("A potentially dangerous Request.QueryString value was detected from the client (t='... </SCRIPT><script src...')."));
				AssertIgnored(GetExceptionForHResult(unchecked((int)0x80070032)));
				AssertIgnored(GetExceptionForHResult(unchecked((int)0x800704CD)));
			});
		}

		static HttpException GetExceptionForHResult(int hresult)
		{
			var hrException = Marshal.GetExceptionForHR(hresult);
			return new HttpException(hrException.Message, hrException);
		}

		public void TestHandle_CriticalException()
		{
			AssertIgnored(new CriticalException());
		}

		[NonSerializedClass]
		class CriticalException : Exception, ICriticalException
		{
			public bool IsCriticalException => true;
		}

		void AssertIgnored(Exception actualException)
		{
			var initialErrorCount = ErrorReporter.TotalErrorCount;

			TopLevelWebExceptionHandler.HandleUnhandledException(actualException);

			AssertEquals(initialErrorCount, ErrorReporter.TotalErrorCount);
			Assert("Does not show user notificatons on web", UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		public void TestHandleUnhandledException_DoNotReportBadRequestExceptions()
		{
			var exception = new HttpException(
					httpCode: 400,
					hr: -2147467259,
					message: "The length of the URL for this request exceeds the configured maxUrlLength value."
					);
			var ishandled = TopLevelWebExceptionHandler.HandleUnhandledException(exception);

			Assert("HttpExceptions indicating a bad request, should be ignored.", exception.IsIgnorable());
			Assert("HttpExceptions indicating a bad request, should be considered handled and dont get reported.", ishandled);
		}

		public void TestHandleUnhandledException_DoNotReportIOExceptionExceptions()
		{
			var exception = new IOException();
			var ishandled = TopLevelWebExceptionHandler.HandleUnhandledException(exception);

			Assert("IOExceptions should be ignored.", exception.IsIgnorable());
			Assert("IOExceptions should be considered handled and dont get reported.", ishandled);
		}

		public void TestHandleUnhandledException_WhenDatabaseUpgradeExceptionHasBeenThrown()
		{
			try
			{
				// Arrange
				Db.EnableThreadSchemaVersionCheckPermanently_ForTest();
				var bumpedSchemaVersion = ObjectFactory.Get<IDatabaseAspectVersions>().SchemaVersion.Major + 10;
				var versionMock = new Mock<IDatabaseAspectVersions>();
				versionMock
					.Setup(x => x.SchemaVersion)
					.Returns(new VersionLabel(bumpedSchemaVersion, 0));

				var exceptionToHandle = new InvalidOperationException("");

				var errorReporterMock = new Mock<IErrorReporter>();
				// Setup any call to the error reporter so it will try and open the connection.
				// We want it to succeed and not throw DatabaseUpgradedException.
				errorReporterMock.Setup(x => x.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()))
					.Callback(() =>
					{
						((IDbReconnectionHandling)Db.Connection).CloseAndReopenConnection();
					});

				using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
				using (var dbEnv = new DbEnvironmentWithMockGuiPlugin())
				using (ObjectFactory.Substitute(versionMock.Object))
				{
					// Cause the first DatabaseUpgradedException.
					AssertExceptionThrown<DatabaseUpgradedException>(() => ((IDbReconnectionHandling)Db.Connection).CloseAndReopenConnection());

					// Act
					TopLevelWebExceptionHandler.HandleUnhandledException(exceptionToHandle);

					// Assert
					Mock.Get(dbEnv.ConnectionGuiPlugin).Verify(gp => gp.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradeException>()), Times.Never);
					Assert("Does not show user notificatons on web", UnitTestUserNotification.Instance.LastMessage.WasNone);
				}

				errorReporterMock.Verify(x => x.Report("TopLevelWeb_InvalidOperationException_AfterDbUpgrade", "Internal server error: ", exceptionToHandle), Times.Once);
				ErrorReporter.Clear();
			}
			finally
			{
				Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = false;
				Db.ResetDatabaseUpgraded_ForTest();
			}
		}

		public void TestDoesNotShowUserNotifications()
		{
			var ex = SqlExceptionBuilder.CreateSqlException(2, 0, 11, Db.ServerName, "Error Number 2 : A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)", "", 0);

			TopLevelWebExceptionHandler.HandleUnhandledException(ex);

			Assert("Does not show user notificatons on web", UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		[TestRequiresAdministrativePrivileges("Interacting with Event logs may require administrator privileges")]
		public void TestShowError_EventLog()
		{
			var topLevelWebExceptionHandler = new TopLevelWebExceptionHandler();
			var ex1 = new UnauthorizedAccessException("error_message");
			var dateTime = DateTime.Now;
			var isHandled = topLevelWebExceptionHandler.HandleSpecificExceptions(ex1);

			AssertEquals("UnauthorizedAccessException should be handled.", true, isHandled);

			var eventRecords = QueryEventLog(WebInfrastructureConstants.EventSourceName, string.Empty, dateTime);
			var record = eventRecords.Any(record =>
			{
				var level = record.LevelDisplayName;
				var desc = record.FormatDescription();
				return level.Equals("Error")
				&& desc.Contains("File operation failed. Details: error_message")
				&& desc.Contains("DatabaseName:" + Db.DatabaseName)
				&& desc.Contains("ServerName:" + Db.ServerName);
			});

			AssertEquals("The ShowError() should log Exception at the Error level and include the Exception message and site information.", true, record);

			var ex2 = new SecurityException();
			dateTime = DateTime.Now;
			isHandled = topLevelWebExceptionHandler.HandleSpecificExceptions(ex2);

			AssertEquals("SecurityException should be handled.", true, isHandled);

			eventRecords = QueryEventLog(WebInfrastructureConstants.EventSourceName, string.Empty, dateTime);
			record = eventRecords.Any(record =>
			{
				var level = record.LevelDisplayName;
				var desc = record.FormatDescription();
				return level.Equals("Error")
				&& desc.Contains("Caption: Access Denied")
				&& desc.Contains(TopLevelExceptionHandler.SecurityErrorText)
				&& desc.Contains("DatabaseName:" + Db.DatabaseName)
				&& desc.Contains("ServerName:" + Db.ServerName);
			});

			AssertEquals("The ShowError() should log Exception at the Error level and include the Caption, Exception message and site information.", true, record);
		}

		[TestRequiresAdministrativePrivileges("Interacting with Event logs may require administrator privileges")]
		public void TestShowWarning_EventLog()
		{
			var topLevelWebExceptionHandler = new TopLevelWebExceptionHandler();
			var ex = new TransactionException("warning_message");
			var dateTime = DateTime.Now;
			var isHandled = topLevelWebExceptionHandler.HandleSpecificExceptions(ex);

			AssertEquals("TransactionException should be handled.", true, isHandled);

			var eventRecords = QueryEventLog(WebInfrastructureConstants.EventSourceName, string.Empty, dateTime);
			var isLogged = eventRecords.Any(record =>
			{
				var level = record.LevelDisplayName;
				var desc = record.FormatDescription();
				return level.Equals("Warning")
				&& desc.Contains("warning_message")
				&& desc.Contains("DatabaseName:" + Db.DatabaseName)
				&& desc.Contains("ServerName:" + Db.ServerName);
			});

			AssertEquals("The ShowWarning() should log Exception at the Warning level and include the Exception message and site information.", true, isLogged);
		}

		[UseSnapshotProtection]
		public void TestShowErrorDialogIfNotSilentlyHandled_EventLog()
		{
			var topLevelWebExceptionHandler = new TopLevelWebExceptionHandler();
			var error = SqlExceptionBuilder.CreateSqlError(1205, 1, 1, "", "A deadlock error.", "", 1);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var ex = SqlExceptionBuilder.CreateSqlException(errors);
			var dateTime = DateTime.Now;
			var isHandled = topLevelWebExceptionHandler.HandleSpecificExceptions(ex);

			AssertEquals("SqlException should be handled.", true, isHandled);

			var eventRecords = QueryEventLog(WebInfrastructureConstants.EventSourceName, string.Empty, dateTime);
			var isLogged = eventRecords.Any(record =>
			{
				var level = record.LevelDisplayName;
				var desc = record.FormatDescription();
				return level.Equals("Error")
				&& desc.Contains("Server cancelled the operation due to deadlock with another operation. Please try again.")
				&& desc.Contains("DatabaseName:" + Db.DatabaseName)
				&& desc.Contains("ServerName:" + Db.ServerName);
			});

			AssertEquals("The ShowErrorDialogIfNotSilentlyHandled() should log Exception at the Error level and include the Exception message and site information.", true, isLogged);
			AssertErrorReportCreated(nameof(SqlException), "A deadlock error.");
		}

		static List<EventRecord> QueryEventLog(string source, string messageStartsWith, DateTime after)
		{
			var eventRecords = new List<EventRecord>();
			var eventsQuery = new EventLogQuery(
				"Application",
				PathType.LogName,
				$@"
*[
	System/Provider/@Name='{source}'
	and
	System/TimeCreated/@SystemTime >= '{after.ToUniversalTime():o}'
]");
			using (var eventlogReader = new EventLogReader(eventsQuery))
			{
				var eventRecord = eventlogReader.ReadEvent();
				while (eventRecord != null)
				{
					eventRecords.Add(eventRecord);
					eventRecord = eventlogReader.ReadEvent();
				}
			}

			return eventRecords;
		}

		void AssertErrorReportCreated(string expectedExceptionType, string expectedExceptionMessage)
		{
			var result = (string)Db.Connection.ExecuteScalar("SELECT QER_ReportXml FROM dbo.StmErrorReport ORDER BY QER_SystemCreateTimeUtc DESC");
			Assert(result.Contains(expectedExceptionType));
			Assert(result.Contains(expectedExceptionMessage));
		}

		class DbEnvironmentWithMockGuiPlugin : BaseDbEnvironment, IDisposable
		{
			public DbEnvironmentWithMockGuiPlugin()
			{
				existingEnv = DbEnv.Instance;
				DbEnv.SetDbEnvironment(this);
			}

			public override IDbConnectionGuiPlugin ConnectionGuiPlugin => connectionGuiPlugin;
			readonly IDbConnectionGuiPlugin connectionGuiPlugin = Mock.Of<IDbConnectionGuiPlugin>();

			public void Dispose() => DbEnv.SetDbEnvironment(existingEnv);
			readonly IDbEnvironment existingEnv;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var reporter = new BaseWebExceptionReporter(new TopLevelWebExceptionHandler());
			reporter.Enable();
		}

		protected override void TearDown()
		{
			Db.EnableSchemaVersionCheckPermanently_ForTest();
			Db.EnableThreadSchemaVersionCheckPermanently_ForTest();

			ExceptionReporter.DisableExposed();
			ExceptionReporter.Instance.Enable();

			base.TearDown();
		}
	}
}
