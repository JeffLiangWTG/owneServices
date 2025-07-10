using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Authentication;
using System.Security.Policy;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.ErrorManagement;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.Abstractions;
using CargoWise.DataProtection.TestFramework;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Core;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.ZArchitecture.Core.ZExceptionExtensions;
using DbConnection = CargoWise.Data.DbConnection;
using Globals = Enterprise.ZArchitecture.Environment.Globals;

namespace Enterprise.ZArchitecture.Core.Testing
{
	class TransactionedTest : TransactionedTestCase
	{
		BaseExceptionReporter ExistingExceptionReporter;
		int numErrorReportsWhenStartingTest;

		protected override void SetUp()
		{
			base.SetUp();

			if (ExceptionReporter.IsEnabled)
			{
				ExistingExceptionReporter = ExceptionReporter.Instance;
			}
			new BaseExceptionReporter().Enable();

			numErrorReportsWhenStartingTest = GetNumErrorReports();
		}

		protected override void TearDown()
		{
			if (ExistingExceptionReporter != null)
			{
				ExistingExceptionReporter.Enable();
			}
			else
			{
				ExceptionReporter.Disable();
			}
		}

		public void TestHandleCOMException()
		{
			var testExceptionReporter = new TestExceptionReporter();
			var innerException = new COMException("Logon failure: unknown user name or bad password.");
			var mainException = new AuthenticationException("Logon failure: unknown user name or bad password.", innerException);
			testExceptionReporter.HandleOrReport(mainException);
			AssertEquals("No Unhandled Exception expected", 0, testExceptionReporter.TotalReportCount);
		}

		public void TestDoHandleTypicallyHandledExceptionsFromDispose()
		{
			RaiseAndTest(() => new DisposeWithInnerIgnoredException().Run());
			RaiseAndTest(() => new DisposeWithOuterIgnoredException().Run());
		}

		void RaiseAndTest(Action action)
		{
			var testExceptionReporter = new TestExceptionReporter();

			try
			{
				action();
			}
			catch (Exception ex)
			{
				testExceptionReporter.ReportException("Key", ex);
			}

			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
		}

		class DisposeWithOuterIgnoredException : IDisposable
		{
			public void Run()
			{
				try
				{
					Dispose();
				}
				catch (Exception ex)
				{
					throw new IOException("Should Report", ex);
				}
			}

			public void Dispose()
			{
				throw new Exception("Wrapped Dispose exception");
			}
		}

		class DisposeWithInnerIgnoredException : IDisposable
		{
			public void Run()
			{
				try
				{
					Dispose();
				}
				catch (Exception ex)
				{
					throw new Exception("Should Report", ex);
				}
			}

			public void Dispose()
			{
				throw new IOException("Wrapped Dispose Exception");
			}
		}

		public void TestErrorReporterReportOnceForExceptionsThatAreAlwaysReportedAndHandled()
		{
			try
			{
				bool shouldReportAlwaysInReportOnce = false;
				var exception = new ExceptionImplementingReporterExtender(shouldReportAlwaysInReportOnce);
				ErrorReporter.ReportOnce("Hi Mum", exception);
				ErrorReporter.ReportOnce("Hi Mum", exception);
				AssertEquals(1, exception.HandleExceptionCallCount);

				shouldReportAlwaysInReportOnce = true;
				exception = new ExceptionImplementingReporterExtender(shouldReportAlwaysInReportOnce);
				ErrorReporter.ReportOnce("Hi Mum", exception);
				ErrorReporter.ReportOnce("Hi Mum", exception);
				AssertEquals(2, exception.HandleExceptionCallCount);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1064:Exceptions should be public", Justification = "For test only")]
		[Serializable]
		class ExceptionImplementingReporterExtender : Exception, IErrorReporterExtender, IExceptionReporterExtender
		{
			internal ExceptionImplementingReporterExtender(bool shouldReportAlwaysInReportOnce)
			{
				ShouldReportAlwaysInReportOnce = shouldReportAlwaysInReportOnce;
			}

#if NETFRAMEWORK
			protected ExceptionImplementingReporterExtender(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
			{
			}
#endif

			readonly bool ShouldReportAlwaysInReportOnce;

			bool IErrorReporterExtender.ShouldReportAlwaysInReportOnce => ShouldReportAlwaysInReportOnce;

			public bool HandleException()
			{
				HandleExceptionCallCount++;
				return true;
			}

			internal int HandleExceptionCallCount;
		}

		public void TestHandleEmailHasNoFromAddressException()
		{
			var testExceptionReporter = new TestExceptionReporter();
			var exception = new EmailHasNoFromAddressException("Whatever");
			testExceptionReporter.HandleOrReport(exception);
			AssertEquals("No Unhandled Exception expected", 0, testExceptionReporter.TotalReportCount);
		}

		public void TestCultureNotFoundException()
		{
			var testExceptionReporter = new TestExceptionReporter();
			var exception = new CultureNotFoundException(@" Culture is not supported.
Parameter name: culture
14345 (0x3809) is an invalid culture identifier.");
			testExceptionReporter.HandleOrReport(exception);
			AssertEquals("No Unhandled Exception expected", 0, testExceptionReporter.TotalReportCount);
		}

		public void TestHandleSystemRegistrationKeyException()
		{
			ExceptionReporter.Instance.ReportException("TestHandleSystemRegistrationKeyException", new SystemRegistrationKeyMissingInformationException("Some mock parameter."));
			Assert("SystemRegistrationKeyMissingInformationException not handled as expected. Actual notification message was:\r\n\r\n" + UnitTestUserNotification.Instance.LastMessage.Text + "\r\n",
				UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("Your System Registration Key is out of date."));
		}

		public void TestHandleDatabaseMissingException()
		{
			var testExceptionReporter = new TestExceptionReporter();
			var exception = new DatabaseMissingException("Country [CA] requires [Enterprise] reference database to be installed.");

			AssertEquals("DatabaseMissingException should handle as specific exception", true, testExceptionReporter.HandleSpecificExceptions_Exposed(exception));

			testExceptionReporter.HandleOrReport(exception);
			AssertEquals("No Unhandled Exception expected", 0, testExceptionReporter.TotalReportCount);
		}

		public void TestDatabaseTrustworthyIsHandled()
		{
			DataUtils.SetDbPropertyImmediately(Db.NewAdminConnection(), Db.DatabaseName, "1 = 1", "TRUSTWORTHY OFF"); // sql server property
			try
			{
				AssertEquals("Database is trustworthy", false, Db.Connection.ExecuteScalar($"Select is_trustworthy_on From sys.databases Where name = '{Db.DatabaseName}'"));

				try
				{
					Db.Connection.ExecuteScalar($"RAISERROR (15562, 18, 1, 'The module being executed is not trusted. Either the owner of the database of the module needs to be granted authenticate permission, or the module needs to be digitally signed.\r\nThe statement has been terminated.')");
				}
				catch (Exception ex)
				{
					ExceptionReporter.Instance.HandleOrReport(ex);
				}

				AssertEquals("Database is trustworthy", true, Db.Connection.ExecuteScalar($"Select is_trustworthy_on From sys.databases Where name = '{Db.DatabaseName}'"));
			}
			finally
			{
				DataUtils.SetTrustworthyOn(Db.NewAdminConnection(), Db.DatabaseName); // sql server property
			}
		}

		public void TestOwnerSIDDiffersFromMaster()
		{
			var testLogin = "testDbNameTempLogin";
			var testDbName = "testDbC4697877-3DCB-48BA-8533-96FA4B49CE36";
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				try
				{
					connection.ExecuteNonQuery($"CREATE LOGIN [{testLogin}] WITH PASSWORD=N'GVMFvvCGjCMRkS/vVngETKJa3msiOKUdZpUMWwqYAdA=', DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF");
					AdoTestUtils.CreateDbDropExisting(connection, testDbName);
					DataUtils.AlterDbAuthorisation(connection, testDbName, testLogin);
					AssertEquals("Database owner", testLogin, connection.ExecuteScalar($"Select name From sys.server_principals Where sid = (select owner_sid from sys.databases Where name = '{testDbName}')"));
					var errorMessage = $"The database owner SID recorded in the master database differs from the database owner SID recorded in database '{testDbName}'. You should correct this situation by resetting the owner of database using the ALTER AUTHORIZATION statement.";
					try
					{
						SqlError error = SqlExceptionBuilder.CreateSqlError(33009, byte.MaxValue, byte.MinValue, TestConnection.ServerName, errorMessage, null, 0);
						SqlErrorCollection errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
						throw (SqlExceptionBuilder.CreateSqlException(errors));
					}
					catch (Exception ex)
					{
						ExceptionReporter.Instance.HandleOrReport(ex);
					}
					var expectedAdminLogin = DataProtectionTestBed.Current.GetExpectedCredentials(TestEnvironmentWellKnownSecret.TestServerOdysseyAdmin);

					AssertEquals($"Database owner should be set to {expectedAdminLogin.UserName}", expectedAdminLogin.UserName, connection.ExecuteScalar($"Select name From sys.server_principals Where sid = (select owner_sid from sys.databases Where name = '{testDbName}')"));
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
					AdoTestUtils.DropDbLoginIfExists(connection, testLogin);
				}
			}
		}

#if !CODE_ANALYSIS

		public void TestHandleGDIObjectOverflow()
		{
			TestUIResources uiResources = new TestUIResources();
			UIResources oldUIResources = UIResources.Instance;
			UIResources.Instance = uiResources;
			try
			{
				ArgumentNullException ex = new ArgumentNullException();
				ex.Source = "System.Drawing";

				uiResources.SetGdiObjectsCount(9800);
				ExceptionReporter.Instance.ReportException("TestHandleGDIObjectOverflow", ex);
				Assertion.AssertEquals("No special reporting as GDI object count < 10000", null, UnitTestUserNotification.Instance.LastMessage.Text);
				ExceptionReporterTestListener.Instance.Clear();

				uiResources.SetGdiObjectsCount(9901);
				ExceptionReporter.Instance.ReportException("TestHandleGDIObjectOverflow", ex);
				Assertion.AssertEquals("User notified exception caused by GDI object overflow", "An error has occurred due to a large number of GDI objects currently in use. Consider closing one or more windows.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				UIResources.Instance = oldUIResources;
			}
		}

#endif
		public void TestUnobservedTaskCancelledExceptionIsHandled()
		{
			using (Globals.TemporaryOverrideForIsTest(false))
			{
				var count = (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmErrorReport");
				var exception = new AggregateException("A Task's exception(s) were not observed either by Waiting on the Task or accessing its Exception property. As a result, the unobserved exception was rethrown by the finalizer thread.",
					new AggregateException(new TaskCanceledException("A task was canceled.")));
				ExceptionReporter.Instance.HandleUnhandledException(null, new UnobservedTaskExceptionEventArgs(exception));
				AssertEquals(count, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmErrorReport"));
			}
		}

		public void TestHandleUserVisibleException()
		{
			ExceptionReporter.Instance.ReportException("TestHandleBatchProcessorExceptionReport", new UserVisibleException("Reason"));
			AssertEquals("The current action was unable to be completed.\r\n\r\nReason : Reason", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestHandleUserVisibleException_FromUnhandledException()
		{
			ExceptionReporter.Instance.HandleUnhandledException(null, new UnhandledExceptionEventArgs(new UserVisibleException("Reason"), false));
			AssertEquals("The current action was unable to be completed.\r\n\r\nReason : Reason", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestHandleUnhandledException_IgnoreCriticalExceptions()
		{
			ExceptionReporter.Instance.HandleUnhandledException(null, new UnhandledExceptionEventArgs(new AggregateException(new TestCriticalException()), false));
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestServiceTaskNonInfrastructureErrorSendReport()
		{
			var handler = new TestExceptionReporter()
			{
				AdditionalContextDetails = "TestData123321",
			};
			handler.TestingDoReportException.Value = true;

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var ex = new NullReferenceException("TestServiceTaskNonInfrastructureErrorSendReport");
				handler.ReportException("", ex);
				var result = (string)Db.Connection.ExecuteScalar("SELECT QER_ReportXml FROM dbo.StmErrorReport ORDER BY QER_SystemCreateTimeUtc DESC");
				Assert(result.Contains("NullReferenceException"));
				Assert(result.Contains("TestServiceTaskNonInfrastructureErrorSendReport"));
				Assert(result.Contains("TestData123321"));
			}
		}

		public void TestFileLoadExceptionShowingMessageIfUserInteractive()
		{
			var handler = new TestExceptionReporter()
			{
				AdditionalContextDetails = "TestData123321",
			};
			handler.TestingDoReportException.Value = true;

			using (Globals.SetIsUserInteractiveForTest(true))
			{
				var ex = new FileLoadException("TestFileLoadExceptionShowingMessageIfUserInteractive");
				handler.ReportException("", ex);

				AssertEquals($"The current version of {Constants.ProductName} could not determined.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestRequiresAdministrativePrivileges("Writes to the windows event logs")]
		public void TestFileLoadExceptionShowingMessageIfNotUserInteractive()
		{
			if (!EventLog.SourceExists(Constants.ProductName))
			{
				EventLog.CreateEventSource(Constants.ProductName, "Application");
			}

			var handler = new TestExceptionReporter()
			{
				AdditionalContextDetails = "TestData123321",
			};
			handler.TestingDoReportException.Value = true;

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var ex = new FileLoadException("TestFileLoadExceptionNotShowingMessageIfNotUserInteractive");
				handler.ReportException("", ex);

				Assert(UnitTestUserNotification.Instance.LastMessage.Text, UnitTestUserNotification.Instance.LastMessage.Text.StartsWith($"{Constants.ProductName} was unable to load a program file into memory."));
			}
		}

		[UseSnapshotProtection]
		public void TestServiceTaskInfrastructureErrorSendEmail()
		{
			var handler = ExceptionReporter.Instance;
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				Globals.SetIsUnitTestingProductionFunctionality(true);
				Db.Connection.ExecuteScalar("UPDATE dbo.GlbStaff SET GS_EmailAddress = 'test@example.com', GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate()");

				handler.AdditionalContextDetails = "will WiseTech hit $8 today?";
				var ex = CreateSqlException(952); //database offline
				AssertEquals(EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count, 0);
				handler.ReportException("", ex);
				AssertEquals("Should not send report", handler.TotalReportCount, 0);
				AssertEquals(EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count, 1);
				Assert(EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Body.Contains(string.Format(UnattendedErrorMessage, "Batch Processor", handler.AdditionalContextDetails)));

				Globals.SetIsUnitTestingProductionFunctionality(false);
			}
		}

		public void TestWebServiceNonInfrastructureErrorSendReport()
		{
			var handler = new TestExceptionReporter()
			{
				AdditionalContextDetails = "TestData123321Web",
			};
			handler.TestingDoReportException.Value = true;
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				using (Globals.SetIsWebForTest(true))
				{
					var ex = new NullReferenceException("TestWebServiceNonInfrastructureErrorSendReport");
					handler.ReportException("", ex);
					var result = (string)Db.Connection.ExecuteScalar("SELECT QER_ReportXml FROM dbo.StmErrorReport ORDER BY QER_SystemCreateTimeUtc DESC");
					Assert(result.Contains("NullReferenceException"));
					Assert(result.Contains("TestWebServiceNonInfrastructureErrorSendReport"));
					Assert(result.Contains("TestData123321Web"));
				}
			}
		}

		[UseSnapshotProtection]
		public void TestWebServiceInfrastructureErrorSendEmail()
		{
			var handler = ExceptionReporter.Instance;
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				Globals.SetIsUnitTestingProductionFunctionality(true);

				using (Globals.SetIsWebForTest(true))
				{
					Db.Connection.ExecuteScalar("UPDATE dbo.GlbStaff SET GS_EmailAddress = 'test@example.com', GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate()");

					handler.AdditionalContextDetails = "WiseTech hit $8";
					var ex = CreateSqlException(952); //database offline
					AssertEquals(EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count, 0);
					handler.ReportException("", ex);
					AssertEquals("Should not send report", handler.TotalReportCount, 0);
					AssertEquals(EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count, 1);
					Assert(EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Body.Contains(string.Format(UnattendedErrorMessage, "Web Service", handler.AdditionalContextDetails)));
				}

				Globals.SetIsUnitTestingProductionFunctionality(false);
			}
		}

		public void TestShowReportFormIsNotCalledfCanNotShowDialogs()
		{
			var handler = new TestExceptionReporter();

			handler.TestingDoReportException.Value = true;

			using (Globals.SetIsUserInteractiveForTest(true))
			using (Globals.SetIsConsoleSessionForTest(true))
			using (Globals.SetIsWebForTest(false))
			using (Globals.SetIsWinzorForTest(false))
			{
				var ex = new InvalidOperationException("Error");
				handler.ReportException("", ex);

				AssertEquals(nameof(handler.ShowReportFormWasCalled), false, handler.ShowReportFormWasCalled);
			}
		}

		public void TestHandleNotEnoughSpaceOnDisc()
		{
			ExceptionReporter.Instance.ReportException("TestHandleNotEnoughSpaceOnDisc", new IOException("There is not enough space on the disk.", ExceptionExtensions.HResultConstants.DiskFull));
			AssertEquals("There is not enough space on the disk or you have exceeded your quota. Please contact your system administrator.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestWPFError()
		{
			ExceptionReporter.Instance.ReportException("TestWPFError", new COMException("WPF Presentation Error", ExceptionExtensions.HResultConstants.WPFError));
			string errorMessage = "WPF Presentation Error";
			AssertEquals(string.Format(@"Oops. Something went wrong.

Error Message: {0}

Consider checking whether your video card drivers are up to date.

To prevent this from happening again, please visit this website for more details and steps on how to fix this: https://blogs.msdn.microsoft.com/dsui_team/2013/11/18/wpf-render-thread-failures/", errorMessage), UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestHandleCannotLoadFileOrAssembly()
		{
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
			ExceptionReporter.Instance.ReportException("TestHandleCannotLoadFileOrAssembly", new FileLoadException("Could not load file or assembly 'Enterprise.ZArchitecture.Schema, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350' or one of its dependencies. The paging file is too small for this operation to complete. (Exception from HRESULT: 0x800705AF)", "Enterprise.ZArchitecture.Schema.dll"));
			AssertEquals(@$"{Constants.ProductName} was unable to load a program file into memory.

Error Message: Could not load file or assembly 'Enterprise.ZArchitecture.Schema, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350' or one of its dependencies. The paging file is too small for this operation to complete. (Exception from HRESULT: 0x800705AF)
Full error details have been written to the Windows Event Log.

This may be due to one of the following:
- A problem with the local file system, physical hard drive, or available disk space
- Another program (such as an anti-virus) holding exclusive access to one or more {Constants.ProductName} files
- A problem with the installation of {Constants.ProductName}

Would you like to attempt to repair the {Constants.ProductName} installation?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
		}

		public void TestHandleDataBindingException()
		{
			ExceptionReporter.Instance.HandleUnhandledException(new InvalidOperationException("DataBinding cannot find a row in the list that is suitable for all bindings."));
			AssertEquals("'DataBinding cannot find a row in the list that is suitable for all bindings.'\r\nThere is a mismatch between the database schema and what the program expected.\r\nHelp > Recreate Database Synonyms is likely to resolve this problem.", UnitTestUserNotification.Instance.LastMessage.Text);
			ExceptionReporter.Instance.HandleUnhandledException(new TargetInvocationException("Exception has been thrown by the target of an invocation.", new InvalidOperationException("DataBinding cannot find a row in the list that is suitable for all bindings.")));
			AssertEquals("'DataBinding cannot find a row in the list that is suitable for all bindings.'\r\nThere is a mismatch between the database schema and what the program expected.\r\nHelp > Recreate Database Synonyms is likely to resolve this problem.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestHandleAppManagerNotRunningException_FromUnhandledException()
		{
			ExceptionReporter.Instance.HandleUnhandledException(null, new UnhandledExceptionEventArgs(new InvalidOperationException($"An error has occurred while attempting to communicate with the {Constants.ProductName} Application Manager service."), false));
			AssertEquals($"An error has occurred while attempting to communicate with the {Constants.ProductName} Application Manager service.", UnitTestUserNotification.Instance.LastMessage.Text);
			ExceptionReporter.Instance.HandleUnhandledException(null, new UnhandledExceptionEventArgs(new TargetInvocationException("Exception has been thrown by the target of an invocation.", new InvalidOperationException($"An error has occurred while attempting to communicate with the {Constants.ProductName} Application Manager service.")), false));
			AssertEquals($"An error has occurred while attempting to communicate with the {Constants.ProductName} Application Manager service.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestHandleUnhandledException_ShouldReportNullKey()
		{
			try
			{
				var ex = new Exception("blah");
				try
				{ throw ex; }
				catch { }

				ExceptionReporter.Instance.HandleUnhandledException(ex);
				AssertEquals(null, ErrorReporter.LastKeyReported);
				AssertEquals(ex, ErrorReporter.LastExceptionReported);
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestHandleSqlUnhandledException_ReportFullCallStack()
		{
			var error = SqlExceptionBuilder.CreateSqlError(1205, 1, 1, "MaiSrvr", "Errorz be happening", "sp_getErrorz", 1);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errors);

			bool registryBackup = DataRegistry.Instance.ReportAllNonCriticalSqlErrors;
			DataRegistry.Instance.ReportAllNonCriticalSqlErrors = true;
			try
			{
				try
				{ throw exception; }
				catch (SqlException sqlEx)
				{
					var outerException = new Exception("Outer Exception", sqlEx);
					throw outerException;
				}
			}
			catch (Exception outerEx)
			{
				ExceptionReporter.Instance.HandleUnhandledException(outerEx);
				AssertEquals("The outer exception is reported so that full stack track will be available.", outerEx, ExceptionReporter.Instance.ExceptionReportedSilentlyForTest);
			}
		}

		public void TestSqlLogUnavailableError_ShouldNotReport()
		{
			var error = SqlExceptionBuilder.CreateSqlError(9001, 1, 1, "", "Nothin to see here", "", 1);
			var message = "Sql Server failed to access one or more database files correctly.\r\nPlease contact your system administrator\r\n\r\nMessage: Nothin to see here";
			CreateSqlExceptionAndAssertNoErrorReportSent(message, error);
		}

		public void TestSqlServerUnavailableError_ShouldNotReport()
		{
			var error = SqlExceptionBuilder.CreateSqlError(53, 1, 1, "", "Nothin to see here", "", 1);
			var message = "SQL Server appears to be offline. Please contact your systems administrator.\r\nMessage: Nothin to see here";
			CreateSqlExceptionAndAssertNoErrorReportSent(message, error);
		}

		public void TestLoginFailedError_ShouldNotReport()
		{
			var error1 = SqlExceptionBuilder.CreateSqlError(4060, 1, 1, "", "Login totes failed", "", 1);
			var error2 = SqlExceptionBuilder.CreateSqlError(18456, 1, 1, "", "No really, login defo failed", "", 1);
			var message = "Database login failed - please check the server error log.\r\nIf the problem persists then please contact your system administrator.\r\n\r\nMessage: Login totes failed\r\nNo really, login defo failed\r\nServer Name: " + Db.ServerName + "\r\nDatabase: " + Db.DatabaseName;
			CreateSqlExceptionAndAssertNoErrorReportSent(message, error1, error2);
		}

		public void TestDeadlockError_ShouldNotReport()
		{
			var error = SqlExceptionBuilder.CreateSqlError(1205, 1, 1, "", "A deadlock error", "", 1);
			CreateSqlExceptionAndAssertNoErrorReportSent("Server cancelled the operation due to deadlock with another operation. Please try again.", error);
		}

		public void TestDeadlockError_BatchProcessor_ShouldReportBasedOffRegistrySetting()
		{
			var handler = ExceptionReporter.Instance;
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				Globals.SetIsUnitTestingProductionFunctionality(true);

				Db.Connection.ExecuteScalar("UPDATE dbo.GlbStaff SET GS_EmailAddress = 'test@example.com', GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate()");

				handler.AdditionalContextDetails = "will WiseTech hit $8 today?";
				var ex = CreateSqlException(1205); //deadlock

				AssertEquals(0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
				handler.ReportException("", ex);
				AssertEquals("Should not send report by default", 0, handler.TotalReportCount);
				AssertEquals(0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);

				DataRegistry.Instance.ReportAllNonCriticalSqlErrors = true;
				handler.ReportException("", ex);
				AssertEquals("Should have sent report when enabled in registry", 1, handler.TotalReportCount);
				AssertEquals(0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);

				Globals.SetIsUnitTestingProductionFunctionality(false);
			}
		}

		public void TestSqlOutOfMemory_ShouldNotReport()
		{
			CreateSqlExceptionAndAssertReportable(6533, false);
		}

		public void TestHardwareProblem_ShouldNotReport()
		{
			CreateSqlExceptionAndAssertReportable(823, false);
			CreateSqlExceptionAndAssertReportable(824, false);
		}

		public void TestAdminLoginFailedError_ShouldNotReport()
		{
			var error1 = SqlExceptionBuilder.CreateSqlError(4060, 1, 1, "", "Cannot open database", "", 1);
			var error2 = SqlExceptionBuilder.CreateSqlError(18456, 1, 1, "", "Failed for admin", "", 1);
			var message = "Database login failed - please check the server error log.\r\nIf the problem persists then please contact your system administrator.\r\n\r\nMessage: Cannot open database\r\nFailed for admin\r\nServer Name: " + Db.ServerName + "\r\nDatabase: " + Db.DatabaseName;

			var dbDatabaseNameField = typeof(Db).GetField("fDatabaseName", BindingFlags.NonPublic | BindingFlags.Static);
			if (dbDatabaseNameField != null)
			{
				string originalValue = (string)dbDatabaseNameField.GetValue(null);
				dbDatabaseNameField.SetValue(null, "ExceptionReporter.TestAdminLoginFailedError_ShouldNotReport");

				Exception e = null;
				try
				{
					CreateSqlExceptionAndAssertNoErrorReportSent(message, error1, error2);
				}
				catch (Exception ex)
				{
					e = ex;
				}
				finally
				{
					dbDatabaseNameField.SetValue(null, originalValue);
				}

				AssertNull("No SqlException should have occurred in the HandleSqlException method", e);
			}
			else
			{
				Fail("Could not obtain Db.Instance.DatabaseName backing field. Name may have changed");
			}
		}

		public void TestDatabaseOfflineError_ShouldNotReport()
		{
			var error = SqlExceptionBuilder.CreateSqlError(942, 1, 1, "", "Test SQL Exception should have been caught", "", 1);
			var message = "Sql Server failed to connect to a database.\r\nPlease contact your system administrator.\r\n\r\nMessage: Test SQL Exception should have been caught";
			CreateSqlExceptionAndAssertNoErrorReportSent(message, error);
		}

		public void TestUnableToAccessResolvingReplicaDbError_ShouldNotReport()
		{
			var error = SqlExceptionBuilder.CreateSqlError(983, 1, 1, "", "Unable to access database because its replica role is RESOLVING.", "", 1);
			var message = "Please contact your systems administrator to check the database for errors.\r\nMessage: Unable to access database because its replica role is RESOLVING.";
			CreateSqlExceptionAndAssertNoErrorReportSent(message, error);
		}

		public void TestDDLDisconnection_ShouldNotReport()
		{
			var error = SqlExceptionBuilder.CreateSqlError(1219, 1, 1, "", "Your session has been disconnected because of a high priority DDL operation.", "", 1);
			CreateSqlExceptionAndAssertNoErrorReportSent(null, error);
		}

		public void TestLoginIsNotAbleToAccessDatabaseUnderCurrentSecurityContext_ShouldNotReport()
		{
			var error = SqlExceptionBuilder.CreateSqlError(916, 1, 1, "", "Unable to access the database", "", 1);
			var message = "The server is not able to access the database under the current security context - please check the server error log.\r\nIf the problem persists then please contact your system administrator.\r\n\r\nMessage: Unable to access the database\r\nServer Name: " + Db.ServerName + "\r\nDatabase: " + Db.DatabaseName;
			CreateSqlExceptionAndAssertNoErrorReportSent(message, error);
		}

		void CreateSqlExceptionAndAssertNoErrorReportSent(string messageShownToUser, params SqlError[] errors)
		{
			var errorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(errors);
			var exception = SqlExceptionBuilder.CreateSqlException(errorCollection);

			Assert(ExceptionReporter.Instance.exceptionHandler.HandleSqlException(exception, new Exception()));

			AssertEquals(messageShownToUser, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
			AssertEquals(0, GetNumCreatedErrorReports());
		}

		public void TestReportTransportLevelErrorDeveloperException_ShouldNotReport()
		{
			var error = SqlExceptionBuilder.CreateSqlError(6, 1, 1, "", "Nothin to see here", "", 1);
			var errorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errorCollection);

			ExceptionReporter.Instance.ReportDeveloperExceptionOrHandleSilently("", exception);

			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
			AssertEquals(0, GetNumCreatedErrorReports());
		}

		public void TestHandleNotEnoughSpaceOnDisc_FromUnhandledException()
		{
			ExceptionReporter.Instance.HandleUnhandledException(null, new UnhandledExceptionEventArgs(new IOException("There is not enough space on the disk.", ExceptionExtensions.HResultConstants.DiskFull), false));
			AssertEquals("There is not enough space on the disk or you have exceeded your quota. Please contact your system administrator.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[ExceptionVisibility(ExceptionVisibility.User), Serializable]
		class UserVisibleException : Exception
		{
			public UserVisibleException(string message)
				: base(message)
			{
			}

#if NETFRAMEWORK
			protected UserVisibleException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		class ThrowExceptionDuringHandleHandler : TopLevelExceptionHandler
		{
			protected override bool HandleException(Exception exceptionToHandle, Exception outermostExceptionForErrorReport)
			{
				throw new Exception("Exception thrown during handle!!!! Oh noooo JKS JUST TESTING");
			}
		}

		class ThrowOoMExceptionDuringHandleHandler : TopLevelExceptionHandler
		{
			protected override bool HandleException(Exception exceptionToHandle, Exception outermostExceptionForErrorReport)
			{
				throw new OutOfMemoryException();
			}
		}

		public void TestExceptionDuringHandle()
		{
			var reporter = new BaseExceptionReporter(new ThrowExceptionDuringHandleHandler());
			reporter.ReportException("testexceptionduringhandlingexception", new Exception());
			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			Assert(ExceptionReporterTestListener.Instance[0].Message.StartsWith("Exception occured during HandleException (See Inner Stack Trace for original)"));
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestOoMExceptionDuringHandle()
		{
			var reporter = new BaseExceptionReporter(new ThrowOoMExceptionDuringHandleHandler());
			reporter.ReportException("testexceptionduringhandlingexception", new Exception());
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestOutOfMemoryExceptionWithNotEnoughPhysicalMemory()
		{
			ZSystemInformation oldInfo = ZSystemInformation.Instance;
			try
			{
				var sysInfoLessMem = new DummySysInfoWithLessThanMinimumRequirements();
				ZSystemInformation.SetInstanceForTesting(sysInfoLessMem);
				Globals.Message.ShowDeveloperException(new OutOfMemoryException("Out Of Memory Exception"));
				Assert("Dialogue should be shown", UnitTestUserNotification.Instance.LastMessage.Text.IndexOf("If problems persist, please contact your system administrator and show them this information:") > -1);
			}
			finally
			{
				ZSystemInformation.SetInstanceForTesting(oldInfo);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		[Serializable]
		class PageFileTooSmallException : FileLoadException, System.Runtime.Serialization.ISerializable
		{
			public PageFileTooSmallException()
				: base()
			{
				HResult = ExceptionExtensions.HResultConstants.PagingFileTooSmall;
			}

#if NETFRAMEWORK
			public PageFileTooSmallException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext ctxt)
				: base(info, ctxt)
			{
				info.AddValue("HResult", HResult);
			}
#endif
		}

		public void TestHandlePagingFileTooSmallException()
		{
			Globals.Message.ShowDeveloperException(new PageFileTooSmallException());
			AssertEquals("An error has occured because this program is running low on memory. Save all your work and restart this program. If problem persists contact your system administrator to have your page file size increased.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestOutOfMemoryExceptionWithSmallCommittableMemory()
		{
			ZSystemInformation oldInfo = ZSystemInformation.Instance;
			try
			{
				var sysInfoLessMem = new DummySysInfoWithLowPageFile();
				ZSystemInformation.SetInstanceForTesting(sysInfoLessMem);
				Globals.Message.ShowDeveloperException(new OutOfMemoryException("Out Of Memory Exception"));
				Assert("Dialog should be shown", UnitTestUserNotification.Instance.LastMessage.Text.Contains("If problems persist, please contact your system administrator and show them this information:"));
				Assert("Exception details should contain CommittableMemory", UnitTestUserNotification.Instance.LastMessage.Text.Contains(@"CommittableMemory
Available: 3 MB
Current Commit Limit: 5 MB"));
			}
			finally
			{
				ZSystemInformation.SetInstanceForTesting(oldInfo);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestOutOfMemoryExceptionWithNotEnoughVirtualMemory()
		{
			ZSystemInformation oldInfo = ZSystemInformation.Instance;
			try
			{
				var sysInfoLessMem = new DummySysInfoWithNotEnoughVirtualMemory();
				ZSystemInformation.SetInstanceForTesting(sysInfoLessMem);
				Globals.Message.ShowDeveloperException(new OutOfMemoryException("Out Of Memory Exception"));
				Assert("Dialogue should be shown", UnitTestUserNotification.Instance.LastMessage.Text.IndexOf("If problems persist, please contact your system administrator and show them this information:") > -1);
			}
			finally
			{
				ZSystemInformation.SetInstanceForTesting(oldInfo);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestOutOfMemoryExceptionWithNotEnoughVirtualMemoryInBatchProcessorContext()
		{
			ZSystemInformation oldInfo = ZSystemInformation.Instance;
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var sysInfoLessMem = new DummySysInfoWithNotEnoughVirtualMemory();
				ZSystemInformation.SetInstanceForTesting(sysInfoLessMem);
				AssertEquals("Precondition: Empty", null, UnitTestUserNotification.Instance.LastMessage.Text);
				Globals.Message.ShowDeveloperException(new OutOfMemoryException("Out Of Memory Exception"));
				Assert("Dialog shown", UnitTestUserNotification.Instance.LastMessage.Text.Contains("If problems persist, please contact your system administrator and show them this information:"));
				Assert("Dialog shown", UnitTestUserNotification.Instance.LastMessage.Text.Contains(@"Virtual
Available: 412 MB"));

				ZSystemInformation.SetInstanceForTesting(oldInfo);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestOutOfMemoryExceptionWithIsPageFileEnabled()
		{
			ZSystemInformation oldInfo = ZSystemInformation.Instance;
			try
			{
				bool registryAccessSucceeded = true;
				try
				{
					var pagingFiles = (string[])Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "PagingFiles", null);
				}
				catch
				{
					registryAccessSucceeded = false;
				}

				var sysInfoLessMem = new DummySysInfoWithLowPageFile();
				ZSystemInformation.SetInstanceForTesting(sysInfoLessMem);
				Globals.Message.ShowDeveloperException(new OutOfMemoryException("Out Of Memory Exception"));
				Assert("Dialog should be shown", UnitTestUserNotification.Instance.LastMessage.Text.Contains("If problems persist, please contact your system administrator and show them this information:"));

				if (registryAccessSucceeded)
				{
					Assert("Exception details should contain IsPageFileEnabled", UnitTestUserNotification.Instance.LastMessage.Text.Contains(@"IsPageFileEnabled: "));
				}
				else
				{
					Assert("Reading registry failed", UnitTestUserNotification.Instance.LastMessage.Text.Contains(@"Failed to get PagingFiles Registry:"));
				}
			}
			finally
			{
				ZSystemInformation.SetInstanceForTesting(oldInfo);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		[ExpectNoExceptions]
		public void TestExceptionNotThrownWhenExceptionReporterNotEnabled()
		{
			DisableExceptionReporterInstance();
			AssertEquals(typeof(BaseExceptionReporter), ExceptionReporter.Instance.GetType());
		}

		[ExpectNoExceptions]
		public void TestExceptionReporterInstanceIsNotThreadStatic()
		{
			BaseExceptionReporter reporter = null;

			Thread thread = new Thread(new ThreadStart(delegate
			{ reporter = ExceptionReporter.Instance; }));
			thread.Start();
			thread.Join();

			AssertNotNull("ExceptionReporter.Instance in other thread", reporter);
			AssertEquals("ExceptionReporter.Instance should be the same in different threads", ExceptionReporter.Instance, reporter);
		}

		public void TestEnableSpecialisedExceptionReporter()
		{
			AssertEquals(typeof(BaseExceptionReporter), ExceptionReporter.Instance.GetType());

			TestExceptionReporter testReporter = new TestExceptionReporter();
			testReporter.Enable();
			AssertEquals(testReporter, ExceptionReporter.Instance);
		}

		public void TestCheckForEnabledExceptionReporter()
		{
			AssertEquals(true, ExceptionReporter.IsEnabled);
			ExceptionReporter.Disable();
			AssertEquals(false, ExceptionReporter.IsEnabled);
		}

		public void TestExceptionReporterSaveRestoreInstance()
		{
			BaseExceptionReporter savedReporter = ExceptionReporter.Instance;

			TestExceptionReporter testReporter = new TestExceptionReporter();
			testReporter.Enable();

			AssertEquals(testReporter, ExceptionReporter.Instance);

			savedReporter.Enable();
			AssertEquals(savedReporter, ExceptionReporter.Instance);
		}

		public void TestExceptionPassedToDelegateExceptionReporter()
		{
			if (ExceptionReporter.IsEnabled)
			{
				BaseExceptionReporter savedReporter = ExceptionReporter.Instance;
			}
			TestExceptionReporter testReporter = new TestExceptionReporter();
			testReporter.Enable();

			AssertEquals(false, testReporter.ShowReportFormWasCalled);
			Exception expected = new ArgumentException("Error message");
			ExceptionReporter.Instance.ShowReportFormInternal(expected, "", "", "", false);
			AssertEquals(true, testReporter.ShowReportFormWasCalled);
		}

		public void TestWin32ExceptionOutOfMemoryWontShowTheForm()
		{
			var testReporter = new TestExceptionReporter();
			testReporter.Enable();

			AssertEquals(false, testReporter.ShowReportFormWasCalled);
			AssertEquals(false, testReporter.SendErrorReportWasCalled);
			ExceptionReporter.Instance.TestingDoReportException.Value = true;
			ExceptionReporter.Instance.ReportException("Some key", new Win32Exception(1406, "Error message"));
			ExceptionReporter.Instance.ReportException("Some key", new Win32Exception(8, "Error message"));
			AssertEquals(false, testReporter.ShowReportFormWasCalled);
			AssertEquals(true, testReporter.SendErrorReportWasCalled);
		}

		public void TestSuppressGuiWontShowTheForm()
		{
			TestExceptionReporter testReporter = new TestExceptionReporter();
			testReporter.Enable();

			AssertEquals(false, testReporter.ShowReportFormWasCalled);
			AssertEquals(false, testReporter.SendErrorReportWasCalled);
			ExceptionReporter.Instance.TestingDoReportException.Value = true;
			ExceptionReporter.SuppressGui();
			ExceptionReporter.Instance.ReportException("Some key", new ArgumentException("Error message"));
			AssertEquals(false, testReporter.ShowReportFormWasCalled);
			AssertEquals(true, testReporter.SendErrorReportWasCalled);
		}

		public void TestAutoGeneratedSubjectPrefixOverride()
		{
			TestExceptionReporter testReporter = new TestExceptionReporter();
			testReporter.Enable();

			AssertEquals(testReporter.AutoGeneratedSubjectPrefixString, ExceptionReporter.AutoGeneratedSubjectPrefix);
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestDisableErrorReportThrowsExceptionWhileNotTesting()
		{
			bool inTestCase = Globals.IsTest;
			BaseExceptionReporter savedReporter = ExceptionReporter.Instance;
			AssertEquals(savedReporter, ExceptionReporter.Instance);

			try
			{
				using (TestingState.SuspendIsRunningTests())
				{
					ExceptionReporter.Disable();
				}
			}
			finally
			{
				savedReporter.Enable();
				AssertEquals(true, Globals.IsTest);
				AssertEquals(savedReporter, ExceptionReporter.Instance);
			}
		}

		public void TestCaughtByTopLevelWarningException()
		{
			ErrorReporter.Clear();
			Globals.Message.ShowDeveloperException("Hi!", GetNewDBConcurrencyException());
			AssertEquals("Has error message for developers", true, ErrorReporter.ExceptionsThrown.Any(ex => ex.Contains("--- Save Aborted Due to Concurrency Check ---")));

			ErrorReporter.Clear();
			ErrorReporter.ReportOnce("Hey!", GetNewDBConcurrencyException());
			AssertEquals("Has error message for developers", true, ErrorReporter.ExceptionsThrown.Any(ex => ex.Contains("--- Save Aborted Due to Concurrency Check ---")));

			ErrorReporter.Clear();
			ExceptionReporter.Instance.ReportDeveloperException("Hello!", GetNewDBConcurrencyException());
			AssertEquals("Doesn't have error message for developers", false, ErrorReporter.ExceptionsThrown.Any(ex => ex.Contains("--- Save Aborted Due to Concurrency Check ---")));

			ExceptionReporterTestListener.Instance.Clear();
		}

		DBConcurrencyException GetNewDBConcurrencyException()
		{
			var e = new DBConcurrencyException("Bad")
			{
				Row = new DataTable().NewRow()
			};
			e.Row.Table.Columns.Add(JobShipmentSchema.Constants.PK, typeof(Guid));
			e.Row[0] = Guid.NewGuid();
			e.Row.Table.TableName = JobShipmentSchema.Constants.TableName;
			e.Row.Table.PrimaryKey = new DataColumn[] { e.Row.Table.Columns[0] };

			return e;
		}

		public void TestReportException()
		{
			try
			{
				Globals.Message.ShowDeveloperException(null);
				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
				ExceptionReporterTestListener.Instance.Clear();

				Exception expected = new ArgumentException("Error message");
				Globals.Message.ShowDeveloperException(expected);
				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
				AssertEquals(expected, ExceptionReporterTestListener.Instance[0]);
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		[ExpectNoExceptions]
		public void TestSendReport()
		{
			AssertEquals(0, ExceptionReporter.Instance.totalReportCount);

			var initialId = ExceptionReporter.Instance.sessionId;
			var args = new ExceptionReportArgs(new Exception("Message"), "Test Error ID", "Key", "Error Description");
			ExceptionReporter.Instance.SendReportInternal(null, args);
			AssertEquals(1, ExceptionReporter.Instance.totalReportCount);
			AssertEquals(initialId, args.SessionId);
			AssertEquals(0, args.Sequence);

			ExceptionReporter.Instance.SendReportInternal(null, args);
			AssertEquals(2, ExceptionReporter.Instance.totalReportCount);
			AssertEquals(initialId, args.SessionId);
			AssertEquals(1, args.Sequence);

			ExceptionReporter.Instance.SendReportInternal(null, args);
			AssertEquals(3, ExceptionReporter.Instance.totalReportCount);
			AssertEquals(initialId, args.SessionId);
			AssertEquals(2, args.Sequence);
		}

		public void TestSessionId()
		{
			AssertNotEquals(Guid.Empty, ExceptionReporter.Instance.sessionId);

			var reporter2 = new BaseExceptionReporter();
			AssertNotEquals("unique ID per reporter", ExceptionReporter.Instance, reporter2.sessionId);
		}

		[ExpectNoExceptions]
		public void TestWhetherErrorReportIDIsSetWhenSendReport()
		{
			var ex = new DummyHasErrorReportIDException("This error should have Error Report ID");
			Assert("No Error Report ID", string.IsNullOrEmpty(ex.ErrorReportID));

			ExceptionReporter.Instance.SendReportInternal(null, new ExceptionReportArgs(ex, "Test Error ID", "Key", "Error Description: " + ex.Message));
			Assert("Has Report ID", !string.IsNullOrEmpty(ex.ErrorReportID));
		}

		[Serializable]
		class DummyHasErrorReportIDException : Exception, IHasErrorReportID
		{
			public DummyHasErrorReportIDException(string message)
				: base(message)
			{
			}

			public string ErrorReportID { get; set; }

#if NETFRAMEWORK
			protected DummyHasErrorReportIDException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		[ExpectNoExceptions]
		public void TestSendReportBeforeUserLoggedIntoEnterprise()
		{
			Assert(EnvProxy.Instance is IWinFormsEnvironment);
			using (EnvProxy.Instance.SetTemporaryUserContext(null))
			{
				TestSendReport();
			}
		}

		public void TestReportDeveloperException()
		{
			try
			{
				ExceptionReporter.Instance.ReportDeveloperException("", "", null);
				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
				ExceptionReporterTestListener.Instance.Clear();

				Exception expected = new ArgumentException("Developer error message");
				ExceptionReporter.Instance.ReportDeveloperException("", expected.Message, expected);
				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
				AssertEquals(expected, ExceptionReporterTestListener.Instance[0]);
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestOnUnhandledException()
		{
			try
			{
				ExceptionReporter.Instance.HandleUnhandledException(null, new UnhandledExceptionEventArgs(null, false));
				AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

				Exception expected = new ArgumentException("Unhandled error message");
				ExceptionReporter.Instance.HandleUnhandledException(null, new UnhandledExceptionEventArgs(expected, false));
				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
				AssertEquals(new DeveloperNotificationException(expected.Message, expected).ToString(), ExceptionReporterTestListener.Instance[0].ToString());
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestExceptionTiming()
		{
			TestExceptionReporter reporter = new TestExceptionReporter
			{
				timeOfLastException = DateTime.MinValue
			};

			Assert("No previous exception should have been registered", !reporter.IsExceptionRepeated);

			reporter.timeOfLastException = LocalNow;
			Assert("Exception is repeated", reporter.IsExceptionRepeated);

			reporter.timeOfLastException = LocalNow.AddSeconds(-5);
			Assert("Previous exception has expired", !reporter.IsExceptionRepeated);
		}

		public void TestSequentialErrorCount()
		{
			TestExceptionReporter reporter = new TestExceptionReporter
			{
				timeOfLastException = LocalNow.AddSeconds(-5)
			};
			AssertEquals("Sequential error count", 1, reporter.sequentialErrorCount);
			Assert("Error count is not fatal", !reporter.ErrorCountIsFatal);

			reporter.UpdatePreviousException(true);
			AssertEquals("Sequential error count", 2, reporter.sequentialErrorCount);
			Assert("Error count is not fatal", !reporter.ErrorCountIsFatal);

			reporter.UpdatePreviousException(true);
			AssertEquals("Sequential error count", 3, reporter.sequentialErrorCount);
			Assert("Error count is not fatal", !reporter.ErrorCountIsFatal);

			reporter.UpdatePreviousException(true);
			AssertEquals("Sequential error count", 4, reporter.sequentialErrorCount);
			Assert("Error count is not fatal", !reporter.ErrorCountIsFatal);

			reporter.UpdatePreviousException(true);
			AssertEquals("Sequential error count", 5, reporter.sequentialErrorCount);
			Assert("Error count is now fatal", reporter.ErrorCountIsFatal);

			reporter.UpdatePreviousException(false);
			AssertEquals("Sequential error count", 1, reporter.sequentialErrorCount);
			Assert("Error count is not fatal", !reporter.ErrorCountIsFatal);
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestReportSilentlyQueuesReportsThatCantBeSentYet()
		{
			AssertEquals("Reports Sent So Far", 0, GetNumCreatedErrorReports());
			TestExceptionReporter reporter = new TestExceptionReporter();
			SetUpReporterToThrowExceptionWhenSendingReport(reporter, new InvalidOperationException("Intentional Exception For Test."));
			try
			{
				reporter.ReportSilently("Key1", "Message1", new Exception("Exception Text1"));
			}
			finally
			{
				AssertEquals(0, GetNumCreatedErrorReports());
				AssertEquals("Reports That Could Not Be Sent", 1, reporter.reportsThatCouldNotBeSent.Count);
			}
		}

		static void SetUpReporterToThrowExceptionWhenSendingReport(TestExceptionReporter reporter, Exception ex)
		{
			reporter.ThrowOnQueue = ex;
		}

		public void TestSuppressExceptionsThatOccurWhilstReportingDeveloperExceptions()
		{
			TestExceptionReporter reporter = new TestExceptionReporter
			{
				SuppressExceptionsThatOccurWhilstReportingDeveloperExceptions = true
			};
			SetUpReporterToThrowExceptionWhenSendingReport(reporter, new Exception("Blah"));
			reporter.ReportSilently("Key1", "Message1", new Exception("Exception Text1"));

			AssertEquals("Reports That Could Not Be Sent", 1, reporter.reportsThatCouldNotBeSent.Count);
			AssertEquals(0, GetNumCreatedErrorReports());
		}

		public void TestReportSilently_DoesntShowMessageBox()
		{
			TestExceptionReporter reporter = new TestExceptionReporter();
			reporter.ReportSilently("Key", "Message", new Exception("ExceptionMessage"));
			AssertEquals("No message shown to the user", null, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestReportsThatCouldNotBeSentGetSentGetSentOnNextAvailableException()
		{
			TestExceptionReporter reporter = new TestExceptionReporter();
			reporter.reportsThatCouldNotBeSent.Add(new ExceptionReportArgs(new Exception("Exception1"), "Test Error Report ID1", "Key1", "Description1"));
			reporter.reportsThatCouldNotBeSent.Add(new ExceptionReportArgs(new Exception("Exception2"), "Test Error Report ID2", "Key2", "Description2"));
			AssertEquals("Reports That Could Not Be Sent", 2, reporter.reportsThatCouldNotBeSent.Count);
			reporter.ReportSilently("Key3", "Message3", new Exception("Exception3"));
			AssertEquals(3, GetNumCreatedErrorReports());
			AssertEquals("Reports That Could Not Be Sent", 0, reporter.reportsThatCouldNotBeSent.Count);
		}

		public void TestIsSlientExceptionReportArgs()
		{
			ExceptionReporter.Instance.TestingDoReportException.Value = true;
			var mockFormManager = new Mock<IExceptionReportingFormManager>();
			ObjectFactory.Substitute(mockFormManager.Object);
			mockFormManager.Setup(o => o.ShowReportForm(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Action<ExceptionReportArgs>>()))
				.Callback<Exception, string, string, string, bool, Action<ExceptionReportArgs>>((ex, id, key, message, isFull, sendErrorReport) =>
				{
					sendErrorReport(new ExceptionReportArgs(ex, id, key, "Silently reported") { IsSlient = true });
				});
			ExceptionReporter.Instance.ReportDeveloperException(null, new Exception("FAIL"));
			AssertEquals("No message shown to the user", null, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSendAllUnsentDeveloperExceptions()
		{
			TestExceptionReporter reporter = new TestExceptionReporter();
			reporter.reportsThatCouldNotBeSent.Add(new ExceptionReportArgs(new Exception("Exception1"), "Test Error Report ID1", "Key1", "Description1"));
			reporter.reportsThatCouldNotBeSent.Add(new ExceptionReportArgs(new Exception("Exception2"), "Test Error Report ID2", "Key2", "Description2"));
			AssertEquals("Reports That Could Not Be Sent", 2, reporter.reportsThatCouldNotBeSent.Count);
			reporter.SendAllUnsentDeveloperExceptions();
			AssertEquals("Reports Sent So Far", 2, GetNumCreatedErrorReports());
			AssertEquals("Reports That Could Not Be Sent", 0, reporter.reportsThatCouldNotBeSent.Count);
		}

		public void TestSendAllUnsentDeveloperExceptionsDoesntThrowAnAnotherExceptionIfItCantSend()
		{
			TestExceptionReporter reporter = new TestExceptionReporter();
			SetUpReporterToThrowExceptionWhenSendingReport(reporter, new Exception("Blah"));
			reporter.reportsThatCouldNotBeSent.Add(new ExceptionReportArgs(new Exception("Exception1"), "Test Error Report ID1", "Key1", "Description1"));
			reporter.reportsThatCouldNotBeSent.Add(new ExceptionReportArgs(new Exception("Exception2"), "Test Error Report ID2", "Key2", "Description2"));
			AssertEquals("Reports That Could Not Be Sent", 2, reporter.reportsThatCouldNotBeSent.Count);

			reporter.SendAllUnsentDeveloperExceptions();

			AssertEquals("Reports Sent So Far", 0, GetNumCreatedErrorReports());
			AssertEquals("Reports That Could Not Be Sent", 2, reporter.reportsThatCouldNotBeSent.Count);
		}

		public void TestSecurityException()
		{
			var reporter = new TestExceptionReporter();
			reporter.ReportException("", new SecurityException());

			Assert("Should not show form", !reporter.ShowReportFormWasCalled);
			AssertEquals("Expected not to create an issue report", 0, GetNumCreatedErrorReports());
			AssertEquals("Should show error message text", SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContains("Access Denied", UnitTestUserNotification.Instance.LastMessage.Caption);
			Assert("Should show error message", UnitTestUserNotification.Instance.LastMessage.WasError);
		}

		public void TestPolicyException()
		{
			var reporter = new TestExceptionReporter();
			reporter.ReportException("", new PolicyException());

			Assert("Should not show form", !reporter.ShowReportFormWasCalled);
			AssertEquals("Expected not to create an issue report", 0, GetNumCreatedErrorReports());
			AssertEquals("Should show error message text", SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContains("Access Denied", UnitTestUserNotification.Instance.LastMessage.Caption);
			Assert("Should show error message", UnitTestUserNotification.Instance.LastMessage.WasError);
		}

		public void TestSecurityAccessDeniedException()
		{
			var accessDeniedMessage = "No soup for you.";
			var exception = new SecurityAccessDeniedException(accessDeniedMessage);

			var reporter = new TestExceptionReporter();
			reporter.ReportException("", exception);

			Assert("Should not show form", !reporter.ShowReportFormWasCalled);
			AssertEquals("Expected not to create an issue report", 0, GetNumCreatedErrorReports());
			AssertContains(accessDeniedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContains("Access Denied", UnitTestUserNotification.Instance.LastMessage.Caption);
			Assert("Expecting an error message", UnitTestUserNotification.Instance.LastMessage.WasError);
		}

		public void TestIssue01125053()
		{
			var exception = new TaskCanceledException("A task was canceled.");
			var exception1 = new Exception("Exception occured during HandleException (See Inner Stack Trace for original): System.ComponentModel.Win32Exception (0x80004005): Error creating window handle.", exception);
			var reporter = new TestExceptionReporter();
			reporter.ReportException("", exception1);
			Assert("Should not show form", !reporter.ShowReportFormWasCalled);
			AssertEquals("Expected not to create an issue report", 0, GetNumCreatedErrorReports());
		}

		public void TestGetNewExceptionReportBuilder()
		{
			var reporter = new TestExceptionReporter();
			var testReportArgs = new ExceptionReportArgs(new Exception(), "Test Error ID", "Key", "Test Exception");
			var reportBuilder = reporter.GetNewExceptionReportBuilderInternal(testReportArgs);
			AssertNotNull("Exception Report Builder should not be null", reportBuilder);
			AssertEquals("Incorrect Builder returned. Expected ExceptionReportBuilder", typeof(ExceptionReportBuilder), reportBuilder.GetType());
		}

		public void TestCannotContinueScanWithNoLockDueToDataMovement()
		{
			SqlError error = SqlExceptionBuilder.CreateSqlError(601, byte.MaxValue, byte.MinValue, Db.Connection.ServerName, "CannotContinueScanWithNoLockDueToDataMovement", "@@NoProceedure", 0);
			SqlErrorCollection errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			SqlException ex = SqlExceptionBuilder.CreateSqlException(errors);

			DbErrorHandler handler = new DbErrorHandler(ex, Db.Connection);
			AssertEquals("Incorrect SQL Exception type", handler.ExceptionType, DbErrorType.CannotContinueScanWithNoLockDueToDataMovement);

			ExceptionReporter.Instance.ReportException("TestCannotContinueScanWithNoLockDueToDataMovement", ex);
			AssertEquals("Should show CannotContinueScanWithNoLockDueToDataMovement friendly message", "CannotContinueScanWithNoLockDueToDataMovement", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestReportOnceInThread()
		{
			int totalErrorCount = 0;

			var thread = new Thread(() =>
			{
				try
				{
					var error = SqlExceptionBuilder.CreateSqlError(601, byte.MaxValue, byte.MinValue, "MyServer", "CannotContinueScanWithNoLockDueToDataMovement", "@@NoProceedure", 0);
					var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
					throw SqlExceptionBuilder.CreateSqlException(errors);
				}
				catch (SqlException sqlEx)
				{
					ErrorReporter.ReportOnce("Hi Mum", sqlEx);
					totalErrorCount = ErrorReporter.TotalErrorCount;
					ErrorReporter.Clear();
				}
			});

			thread.Start();
			thread.Join();

			AssertEquals("ErrorReporter.ReportOnce should be called only once.", 1, totalErrorCount);
		}

		public void TestReportSilentlySuspendsEventTracking()
		{
			TestExceptionReporter reporter = new TestExceptionReporter();
			Assert("Precondition", reporter.EventTrackingStatus);
			Assert("Precondition", ObjectFactory.Get<IUserEventTracker>().IsEnabled);
			reporter.ReportSilently("key", "message", new Exception());
			Assert("should be false during execution", !reporter.EventTrackingStatus);
			Assert("restored to true after execution", ObjectFactory.Get<IUserEventTracker>().IsEnabled);
		}

		public void TestReportAllNonCriticalSqlErrorsRegistry()
		{
			var exception = SqlExceptionBuilder.CreateSqlException(
				SqlExceptionBuilder.CreateSqlErrorCollection(
				SqlExceptionBuilder.CreateSqlError(-2, 0, 11, Db.ServerName, "Timeout expired.  The timeout period elapsed prior to completion of the operation or the server is not responding.", "", 0)));

			ExceptionReporter.Instance.ReportException("", exception);
			AssertEquals("Timeout errors not reported by default", 0, GetNumCreatedErrorReports());

			DataRegistry.Instance.ReportAllNonCriticalSqlErrors = true;
			ExceptionReporter.Instance.ReportException("", exception);
			AssertEquals("Timeout errors reported when registry is enabled", 1, GetNumCreatedErrorReports());
		}

		public void TestIsSqlException64Reportable()
		{
			int[] errorNumbers = { 64, 10054, 59, -1, 1236, 18461 };

			foreach (int i in errorNumbers)
			{
				var error = SqlExceptionBuilder.CreateSqlError(i, 1, 1, "", "", "", 1);
				var errorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(error);
				var exception = SqlExceptionBuilder.CreateSqlException(errorCollection);

				AssertEquals($"Error Number {i} should be handled.", false, IsSqlExceptionReportable(exception));
			}
		}

		public void TestIsSqlExceptionReportable()
		{
			CreateSqlExceptionAndAssertReportable(9001, false);
			CreateSqlExceptionAndAssertReportable(207, true);
		}

		public void TestIsInfrastructureDbError()
		{
			CreateSqlExceptionAndAssertIsInfrastructureDbError(515, false); //DbErrorType.CannotInsertNullIntoNonNullableColumn;
			CreateSqlExceptionAndAssertIsInfrastructureDbError(1222, false); //DbErrorType.LockTimeoutExpired
			CreateSqlExceptionAndAssertIsInfrastructureDbError(9005, false); //DbErrorType.InvalidParameterPassedToOpenRowset

			CreateSqlExceptionAndAssertIsInfrastructureDbError(4060, true); //DbErrorType.CannotOpenDbRequestedInLogin
			CreateSqlExceptionAndAssertIsInfrastructureDbError(904, true); //DatabaseCannotBeAutostartedDuringServerShutdownOrStartup
			CreateSqlExceptionAndAssertIsInfrastructureDbError(922, true); //DbErrorType.DatabaseIsBeingRecovered
			CreateSqlExceptionAndAssertIsInfrastructureDbError(927, true); //DbErrorType.DatabaseIsInTheMiddleOfRestore
			CreateSqlExceptionAndAssertIsInfrastructureDbError(952, true); //DbErrorType.DatabaseOffline
			CreateSqlExceptionAndAssertIsInfrastructureDbError(926, true); //DbErrorType.CannotOpenDatabaseMarkedAsSuspect
			CreateSqlExceptionAndAssertIsInfrastructureDbError(1105, true); //DbErrorType.DbFilegroupIsFull
			CreateSqlExceptionAndAssertIsInfrastructureDbError(5105, true); //DbErrorType.DeviceActivationError
			CreateSqlExceptionAndAssertIsInfrastructureDbError(6005, true); //DbErrorType.GeneralNetworkError
			CreateSqlExceptionAndAssertIsInfrastructureDbError(945, true); //DbErrorType.InaccessibleFiles
			CreateSqlExceptionAndAssertIsInfrastructureDbError(701, true); //DbErrorType.InsufficientSystemMemoryToRunQuery
			CreateSqlExceptionAndAssertIsInfrastructureDbError(18456, true); //DbErrorType.LoginFailedForUser
			CreateSqlExceptionAndAssertIsInfrastructureDbError(18486, true); //DbErrorType.LoginFailedBecauseItIsLockedOut
			CreateSqlExceptionAndAssertIsInfrastructureDbError(9002, true); //DbErrorType.LogIsFull
			CreateSqlExceptionAndAssertIsInfrastructureDbError(9001, true); //DbErrorType.LogUnavailable
			CreateSqlExceptionAndAssertIsInfrastructureDbError(11001, true); //DbErrorType.ServerDoesNotExist
			CreateSqlExceptionAndAssertIsInfrastructureDbError(6263, true); //DbErrorType.ClrDbOptionDisabled
			CreateSqlExceptionAndAssertIsInfrastructureDbError(15281, true); //DbErrorType.CmdShellDbOptionDisabled
			CreateSqlExceptionAndAssertIsInfrastructureDbError(18470, true); //DbErrorType.LoginDisabled
			CreateSqlExceptionAndAssertIsInfrastructureDbError(983, true); //DbErrorType.UnableToAccessResolvingReplicaDb
			CreateSqlExceptionAndAssertIsInfrastructureDbError(7139, true); //DbErrorType.LargeObjectSizeExceedsReplicationMaximum
			CreateSqlExceptionAndAssertIsInfrastructureDbError(1204, true); //DbErrorType.SqlServerCannotObtainALockResource
			CreateSqlExceptionAndAssertIsInfrastructureDbError(1219, true); //DbErrorType.SqlExceptionDdlDisconnection
			CreateSqlExceptionAndAssertIsInfrastructureDbError(6533, true); //DbErrorType.InsufficientSystemMemoryAccessingCriticalResource
			CreateSqlExceptionAndAssertIsInfrastructureDbError(33009, true); //DbErrorType.OwnerSIDDiffersFromMaster
			CreateSqlExceptionAndAssertIsInfrastructureDbError(18401, true); //DbErrorType.ServerIsInScriptMode
			CreateSqlExceptionAndAssertIsInfrastructureDbError(5149, true); // MODIFY FILE encountered operating system error
			CreateSqlExceptionAndAssertIsInfrastructureDbError(976, true); // The target database is participating in an availability group and is currently not accessible for queries.
			CreateSqlExceptionAndAssertIsInfrastructureDbError(596, true); // DbErrorType.SevereError - session is in the kill state
			CreateSqlExceptionAndAssertIsInfrastructureDbError(802, true); // There is insufficient memory available in the buffer pool.
			CreateSqlExceptionAndAssertIsInfrastructureDbError(21, true); //DbErrorType.FatalError
			CreateSqlExceptionAndAssertIsInfrastructureDbError(10060, true); //DbErrorType.TCPProviderConnectionAttemptFailed
			CreateSqlExceptionAndAssertIsInfrastructureDbError(8645, true); //DbErrorType.ExecuteQueryInResourcePoolTimeOut
		}

		SqlException CreateSqlException(int sqlErrorNumber)
		{
			var error = SqlExceptionBuilder.CreateSqlError(sqlErrorNumber, 1, 1, "", "Something to see here", "", 1);
			var errorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errorCollection);
			return exception;
		}

		void CreateSqlExceptionAndAssertReportable(int sqlErrorNumber, bool shouldBeReportable)
		{
			var exception = CreateSqlException(sqlErrorNumber);
			AssertEquals(shouldBeReportable, IsSqlExceptionReportable(exception));
		}

		void CreateSqlExceptionAndAssertIsInfrastructureDbError(int sqlErrorNumber, bool infrastructureDbError)
		{
			var exception = CreateSqlException(sqlErrorNumber);
			AssertEquals($"IsInfrastructureDbError for error #{sqlErrorNumber}", infrastructureDbError, IsInfrastructureDbError(exception));
		}

		public void TestCorruptedInstallationExceptionHandling()
		{
			AssertEquals(true, IsCorruptedInstallationException(new FileNotFoundException()));
			AssertEquals(true, IsCorruptedInstallationException(new FileNotFoundException("bla", Path.Combine(AssemblyLoader.GetBinPath(), "CargoWise.Stuff.dll"))));
			AssertEquals(true, IsCorruptedInstallationException(new FileNotFoundException(
				"Could not load file or assembly 'XmlDiffPatch, Version=1.0.8.28, Culture=neutral, PublicKeyToken=4f570df270576350' or one of its dependencies. The system cannot find the file specified.",
				"XmlDiffPatch, Version=1.0.8.28, Culture=neutral, PublicKeyToken=4f570df270576350")));
			AssertEquals(false, IsCorruptedInstallationException(new FileNotFoundException("bla", Path.Combine(Temp.TempPath, "SomeTempFile.txt"))));

			AssertEquals(true, IsCorruptedInstallationException(new FileLoadException()));
			AssertEquals(true, IsCorruptedInstallationException(new FileLoadException("bla", Path.Combine(AssemblyLoader.GetBinPath(), "CargoWise.Stuff.dll"))));
			AssertEquals(false, IsCorruptedInstallationException(new FileLoadException("bla", Path.Combine(Temp.TempPath, "SomeTempFile.txt"))));

			AssertEquals(true, IsCorruptedInstallationException(new BadImageFormatException()));
			AssertEquals(true, IsCorruptedInstallationException(new BadImageFormatException("bla", Path.Combine(AssemblyLoader.GetBinPath(), "CargoWise.Stuff.dll"))));
			AssertEquals(false, IsCorruptedInstallationException(new BadImageFormatException("bla", Path.Combine(Temp.TempPath, "SomeTempFile.txt"))));

			AssertEquals(true, IsCorruptedInstallationException(new Exception("Spring Error", new Exception("Inner Spring Error", new TypeLoadException()))));

			AssertEquals(true, IsCorruptedInstallationException(new DirectoryNotFoundException("Could not find a part of the path'" + AssemblyLoader.GetBinPath() + "'.")));
			AssertEquals(false, IsCorruptedInstallationException(new DirectoryNotFoundException("Could not find a part of the path'" + Temp.TempPath + "'.")));

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
			ExceptionReporter.Instance.ReportException(null, new FileNotFoundException("bla", Path.Combine(AssemblyLoader.GetBinPath(), "CargoWise.Stuff.dll")));
			var list = UnitTestUserNotification.Instance.PreviousMessages;
			AssertEquals(@$"{Constants.ProductName} was unable to load a program file into memory.

Error Message: bla
Full error details have been written to the Windows Event Log.

This may be due to one of the following:
- A problem with the local file system, physical hard drive, or available disk space
- Another program (such as an anti-virus) holding exclusive access to one or more {Constants.ProductName} files
- A problem with the installation of {Constants.ProductName}

Would you like to attempt to repair the {Constants.ProductName} installation?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			AssertEquals($"Please close and re-open {Constants.ProductName} before continuing.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestDatabaseUpgradeExceptionsAreHandled()
		{
			var actualDbEnv = DbEnv.Instance;
			try
			{
				bool exceptionHandled = false;
				var mockGuidPlugin = new Mock<IDbConnectionGuiPlugin>();
				var mockDbEnv = new Mock<BaseDbEnvironment>();
				mockDbEnv.Setup(m => m.ConnectionGuiPlugin)
					.Returns(mockGuidPlugin.Object);
				mockGuidPlugin.Setup(m => m.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradeException>()))
					.Callback(() => exceptionHandled = true);
				DbEnv.SetDbEnvironment(mockDbEnv.Object);

				var exceptionReporter = new TestExceptionReporter();
				exceptionReporter.ReportException(null, new DatabaseUpgradeInProgressException());
				Assert(exceptionHandled);
				exceptionHandled = false;
				exceptionReporter.ReportException(null, new DatabaseUpgradedException());
				Assert(exceptionHandled);

				mockDbEnv.VerifyAll();
				mockGuidPlugin.VerifyAll();
			}
			finally
			{
				DbEnv.SetDbEnvironment(actualDbEnv);
			}
		}

		public void TestTargetInvocationException_AreUnwrapped()
		{
			var actualDbEnv = DbEnv.Instance;
			try
			{
				bool exceptionHandled = false;
				var mockGuidPlugin = new Mock<IDbConnectionGuiPlugin>();
				var mockDbEnv = new Mock<BaseDbEnvironment>();
				mockDbEnv.Setup(m => m.ConnectionGuiPlugin)
					.Returns(mockGuidPlugin.Object);
				mockGuidPlugin.Setup(m => m.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradeException>()))
					.Callback(() => exceptionHandled = true);
				DbEnv.SetDbEnvironment(mockDbEnv.Object);

				try
				{
					var action = new Action(() => throw new DatabaseUpgradedException());
					action.DynamicInvoke();
				}
				catch (TargetInvocationException ex)
				{
					try
					{
						throw new AggregateException(ex);
					}
					catch (AggregateException hmm)
					{
						var exceptionReporter = new TestExceptionReporter();
						exceptionReporter.ReportException(null, hmm);
						Assert(exceptionHandled);
					}
				}

				mockDbEnv.VerifyAll();
				mockGuidPlugin.VerifyAll();
			}
			finally
			{
				DbEnv.SetDbEnvironment(actualDbEnv);
			}
		}

		public void TestConcurrencyExceptionAreReportedAndGivenToGuiPlugin()
		{
			var actualDbEnv = DbEnv.Instance;
			try
			{
				bool exceptionHandled = false;
				var mockGuidPlugin = new Mock<IDbConnectionGuiPlugin>();
				var mockDbEnv = new Mock<BaseDbEnvironment>();
				mockDbEnv.Setup(m => m.ConnectionGuiPlugin)
					.Returns(mockGuidPlugin.Object);
				mockGuidPlugin.Setup(m => m.HandleDbConcurrencyException(It.IsAny<Exception>()))
					.Callback(() => exceptionHandled = true);
				DbEnv.SetDbEnvironment(mockDbEnv.Object);

				var factory = new BusinessObjectFactory();
				DummyBusinessObject dummyBizo = factory.NewWithValidTestData<DummyBusinessObject>();
				dummyBizo.Z0_VarCharMax = "hello";
				factory.Save();

				var innerException = new Exception("exception message");
				var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)dummyBizo).Row, Db.Connection);
				var exceptionReporter = new TestExceptionReporter();
				exceptionReporter.ReportException(null, new ZSaveConcurrencyException(concurrencyException, factory));

				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
				Assert(exceptionHandled);
				mockDbEnv.VerifyAll();
				mockGuidPlugin.VerifyAll();
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
				DbEnv.SetDbEnvironment(actualDbEnv);
			}
		}

		public void TestExceptionsThatAreShownCanBeOverriden()
		{
			ShownExceptions.ForEach((ex) => AssertHandleShownException(ex: ex));
		}

		static List<Exception> ShownExceptions
		{
			get
			{
				return new List<Exception> {
						new OperationCanceledException(),
						new DatabaseMissingException(DummyExceptionMessage),
						new UseVfpOleDbProviderIn64BitPlatformException(),
						new EmailHasNoFromAddressException(DummyExceptionMessage),
						new IOException(),
						new SecurityException(),
						new PolicyException(),
						new SecurityAccessDeniedException(DummyExceptionMessage),
						new CommunicationException(),
						new ZBlobReadException(DummyExceptionMessage, null),
						new SqlStreamReaderRowNotFoundException(DummyExceptionMessage),
						new TransactionException(DummyExceptionMessage)
					};
			}
		}

		static string DummyExceptionMessage => "Bla";

		void AssertHandleShownException(Exception ex)
		{
			UnitTestUserNotification.Instance.ClearMessages();

			var exceptionReporter = new TestExceptionReporter();

			Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
			exceptionReporter.ReportException(null, ex);
			Assert(!UnitTestUserNotification.Instance.LastMessage.WasNone);
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
		}

		[ExpectNoExceptions]
		public void TestHandleSqlExceptionFailedToInitClrDueToMemPressure()
		{
			var error = SqlExceptionBuilder.CreateSqlError(6513, byte.MaxValue, byte.MinValue, Db.Connection.ServerName, "Failed to initialize the Common Language Runtime (CLR) v2.0.50727 due to memory pressure.", "", 0);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errors);

			var testExceptionReporter = new TestExceptionReporter();

			Assert(!IsSqlExceptionReportable(exception));
			Assert(testExceptionReporter.HandleSpecificExceptions_Exposed(exception));

			testExceptionReporter.HandleOrReport(exception);
			Assert(testExceptionReporter.TotalReportCount == 0);
		}

		[GuiTest]
		public void TestHandleUnhandledExceptionWithDatabaseUpgradeExceptionOccured()
		{
			var error = SqlExceptionBuilder.CreateSqlError(0, byte.MaxValue, byte.MinValue, Db.Connection.ServerName, "A severe error occurred on the current command. The results, if any, should be discarded.", "", 0);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errors);

			var handler = new HandlerThatThrowsUpgradeExceptions();

			Assert(!IsSqlExceptionReportable(exception));
			Assert(handler.HandleSpecificExceptions(exception));

			var testExceptionReporter = new BaseExceptionReporter(handler);
			testExceptionReporter.HandleOrReport(exception);
			Assert(testExceptionReporter.TotalReportCount == 0);
		}

		public void TestDoesNotCatchDatabaseUpgradeException()
		{
			// Arrange
			using (SetTemporaryExceptionReporter(new TestExceptionReporter { TestingDoReportException = { Value = true } }))
			using (MockSchemaVersion(major: int.MaxValue, minor: 0))
			using (var connection = Db.NewExtraConnectionToMainDb())
			using (SetTemporaryDbConnection(connection))
			{
				// Act
				// Assert
				AssertExceptionThrown<DatabaseUpgradeException>(() => ErrorReporter.ReportOnce("key1", "message1", new InvalidOperationException()));
			}
		}

		static IDisposable SetTemporaryDbConnection(DbConnection connection)
		{
			Db.ConnectionOverrideForTest = connection;
			return new DisposableAction(() =>
			{
				Db.ConnectionOverrideForTest = null;
			});
		}

		static IDisposable SetTemporaryExceptionReporter(BaseExceptionReporter reporter)
		{
			var originalExceptionReporter = ExceptionReporter.Instance;
			ExceptionReporter.SetInstance(reporter);
			return new DisposableAction(() => ExceptionReporter.SetInstance(originalExceptionReporter));
		}

		static IDisposable MockSchemaVersion(int major, int minor)
		{
			var mockServiceProvider = new Mock<IServiceProvider>();
			var serviceProvider = GlobalServiceProvider.Instance;
			mockServiceProvider.Setup(f => f.GetService(It.IsAny<Type>())).Returns<Type>(type =>
			{
				if (type == typeof(IDatabaseAspectVersions))
				{
					return Mock.Of<IDatabaseAspectVersions>(v => v.SchemaVersion == new VersionLabel(major, minor));
				}

				return serviceProvider.GetService(type);
			});

			return GlobalServiceProvider.Configure(mockServiceProvider.Object);
		}

		class HandlerThatThrowsUpgradeExceptions : TopLevelExceptionHandler
		{
		}

		public void TestDatabaseInSingleMode_RecoverGracefully()
		{
			TopLevelExceptionHandler.SetDatabaseIntoAccessMode(Db.DatabaseName, "MULTI_USER");
			AssertEquals("MULTI_USER", TopLevelExceptionHandler.GetDatabaseUserAccessMode(Db.DatabaseName));

			DbConnection conn1 = null;
			DbConnection conn2 = null;
			DbConnection conn3 = null;

			try
			{
				TopLevelExceptionHandler.SetDatabaseIntoAccessMode(Db.DatabaseName, "SINGLE_USER");
				AssertEquals("SINGLE_USER", TopLevelExceptionHandler.GetDatabaseUserAccessMode(Db.DatabaseName));
				try
				{
					conn1 = Db.NewExtraConnectionToMainDb();
					conn1.EnsureIsOpen();
					conn2 = Db.NewExtraConnectionToMainDb();
					conn2.EnsureIsOpen();
					Db.Connection.EnsureIsOpen();
					conn3 = Db.NewExtraConnectionToMainDb();
					conn3.EnsureIsOpen();
				}
				catch (SqlException e)
				{
					var testExceptionReporter = new TestExceptionReporter();
					testExceptionReporter.HandleOrReport(e);
					AssertEquals("MULTI_USER", TopLevelExceptionHandler.GetDatabaseUserAccessMode(Db.DatabaseName));
				}
			}
			finally
			{
				TopLevelExceptionHandler.SetDatabaseIntoAccessMode(Db.DatabaseName, "MULTI_USER");
				conn1?.Dispose();
				conn2?.Dispose();
				conn3?.Dispose();
			}
		}

		public void TestSqlExceptionHandleUnhandledWhenFriendlyMessageIsNullOrWhitespace()
		{
			var error = SqlExceptionBuilder.CreateSqlError(2, byte.MaxValue, byte.MinValue, Db.Connection.ServerName, "Test sqlException Message", "", 1);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errors);
			TopLevelExceptionHandler.SetDatabaseIntoAccessMode(Db.DatabaseName, "MULTI_USER");
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.EnsureIsOpen();
				bool isExceptionHandled = ExceptionReporter.Instance.exceptionHandler.HandleSpecificExceptions(exception);
				Assert("Sql Exception has been silenced", !isExceptionHandled);
			}
		}

		public void TestDontOverloadTheUserWithInformationWhenNotInBatchServer()
		{
			// Outside of ST the user will only get an error dialog instead of an email, and since they're interacting with it they usually have enough
			// context to the error to keep on trucking, so we dont need to blow up their screen with stack traces and what not.

			var handler = new TestExceptionReporter();

			try
			{
				throw new IOException("Blah blah blah");
			}
			catch (IOException ex)
			{
				Assert("PRE: Should be handled", handler.HandleSpecificExceptions_Exposed(ex));

				var message = UnitTestUserNotification.Instance.LastMessage;
				Assert("Expecting an error message", message.WasError);
				AssertEquals("We want to have a simple and to the point error message without too much developer information", "File operation failed. Details: Blah blah blah", message.Text);
			}
		}

		public void TestTaskCancellationExceptionIsNotReported()
		{
			ExceptionReporter.Instance.Enable();
			try
			{
				// Simulate a task cancellation exception
				throw new OperationCanceledException();
			}
			catch (Exception ex)
			{
				ExceptionReporter.Instance.HandleOrReport(ex);
			}

			AssertEquals("OperationCanceledException should not be reported", 0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestHandleGenericCOMException()
		{
			// Arrange
			const string exceptionMessage = "That's a test message";
			Exception innerExceptionToHandle = new COMException(exceptionMessage, -2147418113);
			Exception exceptionToHandle = new Exception("!!!", innerExceptionToHandle);
			// Act
			ExceptionReporter.Instance.HandleUnhandledException(exceptionToHandle);
			// Assert
			var v = UnitTestUserNotification.Instance;
			Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains(exceptionMessage));
			AssertEquals(0, GetNumCreatedErrorReports());
		}

		public void TestHandleUseVfpOleDbProviderIn64BitPlatformException()
		{
			ExceptionReporter.Instance.HandleUnhandledException(new UseVfpOleDbProviderIn64BitPlatformException());
			AssertEquals($"{Constants.ProductName} 64-bit does not support Visual Foxpro OLE DB Provider. Please use {Constants.ProductName} 32-bit to use Visual Foxpro OLE DB Provider.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestExceptionIdIsInExpectedFormat()
		{
			var actualErrorReportID = ExceptionReporter.Instance.GenerateErrorReportID();
			var productRegistrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var expectedPattern = string.Format(@"^E\d+-{0}-{1}$", productRegistrationKey.EnterpriseCode, productRegistrationKey.ServerCode);
			var regex = new Regex(expectedPattern);
			Assert("Exception ID should be in expected format: E<id>-<enterpriseCode>-<dbCode> but was " + actualErrorReportID, regex.IsMatch(actualErrorReportID));
		}

		public void TestErrorReportIdShouldBeSameAsInErrorReport()
		{
			var reporter = new TestExceptionReporter();
			reporter.ReportSilently("Key", "Message", new Exception("ExceptionMessage"));
			AssertContains("the error report id should be same", reporter.ErrorReportIdForTest, reporter.ErrorReportForTest);
		}

		public void TestGenerateErrorReportIDNotThrowAnyException()
		{
			var mockRegistry = new Mock<IProductRegistration>();
			for (int i = 0; i < 4; i++)
			{
				mockRegistry.Setup(m => m.Key)
					.Throws<CommunicationException>();
			}
			using (ObjectFactory.Substitute(mockRegistry.Object))
			{
				var errorReportId = ExceptionReporter.Instance.GenerateErrorReportID();
				AssertEquals("ReportIDFailed", errorReportId);
				mockRegistry.VerifyAll();
			}
		}

		public void TestAggregateExceptionWithMultipleInnerExceptionsReportedSeperatly()
		{
			var aggregate = new AggregateException("Failure", new Exception("One"), new Exception("Two"));
			ExceptionReporter.Instance.HandleOrReport(aggregate);
			AssertEquals(2, ExceptionReporterTestListener.Instance.Count);
			AssertEquals(aggregate.InnerExceptions[0], ExceptionReporterTestListener.Instance[0].InnerException.InnerException);
			AssertEquals(aggregate.InnerExceptions[1], ExceptionReporterTestListener.Instance[1].InnerException.InnerException);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestReportExceptionDuringDatabaseUpgrade()
		{
			var reporter = new TestExceptionReporter();
			using (SimulateDatabaseUpgradeProcess())
			{
				reporter.ReportSilently("Key", "Message", new Exception("ExceptionMessage"));
				AssertEquals("No reports sent during DB upgrade", 0, GetNumCreatedErrorReports());
			}

			reporter.ReportSilently("Key1", "Message1", new Exception("ExceptionMessage1"));
			AssertEquals("Reports will be sent all together after DB upgrade", 2, GetNumCreatedErrorReports());
		}

		#region Implementation

		DbConnection SimulateDatabaseUpgradeProcess()
		{
			var lockoutConnection = Db.NewAdminConnection();
			lockoutConnection.BeginTransaction();
			_ = DbLockout.AcquireTransactionLockout(lockoutConnection);
			return lockoutConnection;
		}

		protected int GetNumCreatedErrorReports()
		{
			return GetNumErrorReports() - numErrorReportsWhenStartingTest;
		}

		int GetNumErrorReports()
		{
			using (var cmd = Db.Connection.Command(string.Format("SELECT COUNT(*) FROM [{0}]", StmErrorReportSchema.Constants.TableName)))
			{
				return (int)cmd.ExecuteScalar();
			}
		}

		DateTime LocalNow
		{
			get { return DateTime.Now; } // Uses DateTime.Now internally
		}

		protected class TestExceptionReporter : BaseExceptionReporter
		{
			public bool ShowReportFormWasCalled;
			public bool SendErrorReportWasCalled;
			public Exception ThrowOnQueue;
			public string ErrorReportForTest;
			public string ErrorReportIdForTest;

			public bool IsShuttingDown
				=> ((NoShutdownHandler)exceptionHandler).IsShuttingdown;

			internal override ExceptionQueuer GetExceptionQueuer()
			{
				if (ThrowOnQueue != null)
				{
					return new ExplodingExceptionQueuer(ThrowOnQueue);
				}
				else
				{
					return new ExceptionQueuerForTest(this);
				}
			}

			class ExceptionQueuerForTest : ExceptionQueuer
			{
				readonly TestExceptionReporter reporter;
				public ExceptionQueuerForTest(TestExceptionReporter reporter)
				{
					this.reporter = reporter;
				}

				internal override void QueueExceptionReportForLaterSendingCore(string report, string initialTransmitStatus, bool useNewDbConnection)
				{
					reporter.ErrorReportForTest = report;
					base.QueueExceptionReportForLaterSendingCore(report, initialTransmitStatus, useNewDbConnection);
				}
			}

			class ExplodingExceptionQueuer : ExceptionQueuer
			{
				readonly Exception throwOnQueue;

				public ExplodingExceptionQueuer(Exception throwOnQueue)
				{
					this.throwOnQueue = throwOnQueue;
				}

				internal override void QueueExceptionReportForLaterSendingCore(string report, string initialTransmitStatus, bool useNewDbConnection)
				{
					throw throwOnQueue;
				}
			}

			class NoShutdownHandler : TopLevelExceptionHandler
			{
				public bool IsShuttingdown;

				public override void ShutdownEnterprise(string userMessage)
				{
					IsShuttingdown = true;
				}
			}

			public bool HandleSpecificExceptions_Exposed(Exception ex)
			{
				return exceptionHandler.HandleSpecificExceptions(ex);
			}

			public bool ErrorCountIsFatal
			{
				get { return IsErrorCountFatal(sequentialErrorCount); }
			}

			protected override void ShowReportForm(Exception ex, string errorReportId, string key, string message, bool isFullMode)
			{
				ShowReportFormWasCalled = true;
				ErrorReportIdForTest = errorReportId;
			}

			public readonly string AutoGeneratedSubjectPrefixString = "TestExceptionReporter AUTO-GENERATED TEST";

			public override string AutoGeneratedSubjectPrefix
			{
				get { return AutoGeneratedSubjectPrefixString; }
			}

			protected override ExceptionReportBuilder GetNewExceptionReportBuilder(ExceptionReportArgs reportArgs)
			{
				EventTrackingStatus = ObjectFactory.Get<IUserEventTracker>().IsEnabled;
				ErrorReportIdForTest = reportArgs.ErrorReportID;
				return base.GetNewExceptionReportBuilder(reportArgs);
			}

			internal ExceptionReportBuilder GetNewExceptionReportBuilderInternal(ExceptionReportArgs reportArgs) => GetNewExceptionReportBuilder(reportArgs);

			protected override void SendErrorReportHandler(ExceptionReportArgs args)
			{
				SendErrorReportWasCalled = true;
			}

			bool eventTrackingStatus = true;
			public bool EventTrackingStatus
			{
				get { return eventTrackingStatus; }
				set { eventTrackingStatus = value; }
			}
		}

		// This method will disable the current ExceptionReporter for TestCases
		void DisableExceptionReporterInstance()
		{
			ExceptionReporter.Disable();
		}

		#endregion
	}
}
