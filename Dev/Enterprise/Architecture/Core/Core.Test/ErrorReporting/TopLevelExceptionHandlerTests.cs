using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.Abstractions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DocumentScanning.Integration;
using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	[ExpectNoNewDbConnection]
	public class TopLevelExceptionHandlerTests : TestCase
	{
		[ExpectNoNewDbConnection]
		public class UpgradeExceptionsTreatedAsNotHandledTest : TestCase
		{
			readonly List<Type> upgradeExceptionTypesTreatedAsNotHandled = new()
			{
				typeof(UpgradeManagerException),
				typeof(OnlineDataTransformationException),
			};

			protected override void SetUp()
			{
				base.SetUp();
				topLevelExceptionHandler = new TopLevelExceptionHandler();
			}

			public void TestUpgradeExceptionIsNotHandled()
			{
				foreach (var type in upgradeExceptionTypesTreatedAsNotHandled)
				{
					// Arrange
					var exception = (Exception)Activator.CreateInstance(type);

					// Act
					var result = topLevelExceptionHandler.HandleSpecificExceptions(exception);

					// Assert
					AssertEquals(false, result);
				}
			}

			public void TestUpgradeManagerChildExceptionIsNotHandled()
			{
				// Arrange
				var exception = new UpgradeManagerChildException();

				// Act
				var result = topLevelExceptionHandler.HandleSpecificExceptions(exception);

				// Assert
				AssertEquals(false, result);
			}

			public void TestUpgradeExceptionWrappedInHandledExceptionIsNotHandled()
			{
				foreach (var type in upgradeExceptionTypesTreatedAsNotHandled)
				{
					// Arrange
					Assert("Precondition. Should be any handled exception.", topLevelExceptionHandler.HandleSpecificExceptions(new CriticalExceptionForTest()));
					var innerException = (Exception)Activator.CreateInstance(type);
					var exception = new CriticalExceptionForTest(string.Empty, innerException);

					// Act
					var result = topLevelExceptionHandler.HandleSpecificExceptions(exception);

					// Assert
					AssertEquals(false, result);
				}
			}

			public void TestUpgradeExceptionWrappingHandledExceptionIsNotHandled()
			{
				foreach (var type in upgradeExceptionTypesTreatedAsNotHandled)
				{
					// Arrange
					Assert("Precondition. Should be any handled exception.", topLevelExceptionHandler.HandleSpecificExceptions(new CriticalExceptionForTest()));
					var exception = (Exception)Activator.CreateInstance(type, new object[] { new CriticalExceptionForTest() });

					// Act
					var result = topLevelExceptionHandler.HandleSpecificExceptions(exception);

					// Assert
					AssertEquals(false, result);
				}
			}

			public void TestUpgradeExceptionInAggregateExceptionIsNotHandled()
			{
				foreach (var type in upgradeExceptionTypesTreatedAsNotHandled)
				{
					// Arrange
					var upgradeException = (Exception)Activator.CreateInstance(type);
					var exception = new AggregateException(upgradeException);

					// Act
					var result = topLevelExceptionHandler.HandleSpecificExceptions(exception);

					// Assert
					AssertEquals(false, result);
				}
			}

			public void TestHandledExceptionWrappingUpgradeExceptionInAggregateExceptionIsNotHandled()
			{
				foreach (var type in upgradeExceptionTypesTreatedAsNotHandled)
				{
					// Arrange
					var upgradeException = (Exception)Activator.CreateInstance(type);
					Assert("Precondition. Should be any handled exception.", topLevelExceptionHandler.HandleSpecificExceptions(new CriticalExceptionForTest()));
					var exception = new AggregateException(new CriticalExceptionForTest(string.Empty, upgradeException));

					// Act
					var result = topLevelExceptionHandler.HandleSpecificExceptions(exception);

					// Assert
					AssertEquals(false, result);
				}
			}

			public void TestUpgradeExceptionWithHandledExceptionInAggregateExceptionIsNotHandled()
			{
				foreach (var type in upgradeExceptionTypesTreatedAsNotHandled)
				{
					// Arrange
					var upgradeException = (Exception)Activator.CreateInstance(type);

					Assert("Precondition. Should be any handled exception.", topLevelExceptionHandler.HandleSpecificExceptions(new CriticalExceptionForTest()));
					var exception = new AggregateException(new CriticalExceptionForTest(), upgradeException);

					// Act
					var result = topLevelExceptionHandler.HandleSpecificExceptions(exception);

					// Assert
					AssertEquals(false, result);
				}
			}

			public void TestUpgradeExceptionWrappingHandledExceptionInAggregateExceptionIsNotHandled()
			{
				foreach (var type in upgradeExceptionTypesTreatedAsNotHandled)
				{
					// Arrange
					var upgradeException = (Exception)Activator.CreateInstance(type, new object[] { new CriticalExceptionForTest() });

					Assert("Precondition. Should be any handled exception.", topLevelExceptionHandler.HandleSpecificExceptions(new CriticalExceptionForTest()));
					var exception = new AggregateException(upgradeException);

					// Act
					var result = topLevelExceptionHandler.HandleSpecificExceptions(exception);

					// Assert
					AssertEquals(false, result);
				}
			}

			public void TestHandledExceptionWithUpgradeExceptionInAggregateExceptionIsNotHandled()
			{
				foreach (var type in upgradeExceptionTypesTreatedAsNotHandled)
				{
					// Arrange
					var upgradeException = (Exception)Activator.CreateInstance(type);

					Assert("Precondition. Should be any handled exception.", topLevelExceptionHandler.HandleSpecificExceptions(new CriticalExceptionForTest()));
					var exception = new AggregateException(upgradeException, new CriticalExceptionForTest());

					// Act
					var result = topLevelExceptionHandler.HandleSpecificExceptions(exception);

					// Assert
					AssertEquals(false, result);
				}
			}

			public void TestLoopInExceptionWithUpgradeExceptionIsBroken()
			{
				foreach (var type in upgradeExceptionTypesTreatedAsNotHandled)
				{
					// Arrange
					var exception1 = new Exception("error 1");
					var exception2 = new Exception("error 2", exception1);
					var exception3 = (Exception)Activator.CreateInstance(type, new object[] { exception2 });
					exception1.GetType()
						.GetField("_innerException", BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.NonPublic)
						.SetValue(exception1, exception3);

					// Act
					var task = Task.Run(() => _ = topLevelExceptionHandler.HandleSpecificExceptions(exception1));

					// Assert
					AssertEquals(true, task.Wait(TimeSpan.FromSeconds(5)));
				}
			}

			TopLevelExceptionHandler topLevelExceptionHandler;

			[Serializable]
			class UpgradeManagerChildException : UpgradeManagerException
			{
				public UpgradeManagerChildException()
				{
				}

				public UpgradeManagerChildException(string message) : base(message)
				{
				}

				public UpgradeManagerChildException(string message, Exception ex) : base(message, ex)
				{
				}

#if NETFRAMEWORK
				protected UpgradeManagerChildException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
				{
				}
#endif
			}
		}

		[ExpectNoNewDbConnection]
		public class MiscellaneousTest : TestCase
		{
			protected override void SetUp()
			{
				base.SetUp();
				topLevelExceptionHandler = new TopLevelExceptionHandler();
			}

			public void TestLoopInExceptionIsBroken()
			{
				// Arrange
				var exception1 = new Exception("error 1");
				var exception2 = new Exception("error 2", exception1);
				var exception3 = new Exception("error 3", exception2);
				exception1.GetType()
					.GetField("_innerException", BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.NonPublic)
					.SetValue(exception1, exception3);

				// Act
				var task = Task.Run(() => _ = topLevelExceptionHandler.HandleSpecificExceptions(exception1));

				// Assert
				AssertEquals(true, task.Wait(TimeSpan.FromSeconds(5)));
			}

			public void TestLoopInAggregateExceptionIsBroken()
			{
				// Arrange
				var exception1 = new Exception("error 1");
				var exception2 = new Exception("error 2", exception1);
				var exception3 = new Exception("error 3", exception2);
				exception1.GetType()
					.GetField("_innerException", BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.NonPublic)
					.SetValue(exception1, exception3);
				var aggregateException = new AggregateException(exception1, exception2, exception3);

				// Act
				var task = Task.Run(() => _ = topLevelExceptionHandler.HandleSpecificExceptions(aggregateException));

				// Assert
				AssertEquals(true, task.Wait(TimeSpan.FromSeconds(5)));
			}

			public void TestNullException()
			{
				// Arrange

				// Act
				topLevelExceptionHandler.HandleSpecificExceptions(null, out var result);

				// Assert
				AssertNull(result);
			}

			public void TestPopulatesExceptionsBufferInErrorReporter()
			{
				// Arrange
				var exception3 = new Exception("error 3");
				var exception2 = new Exception("error 2", exception3);
				var exception1 = new Exception("error 1", exception2);

				// Act
				topLevelExceptionHandler.HandleSpecificExceptions(exception1);

				// Assert
				CombineAssertions(
					() =>
					{
						AssertEquals(3, ErrorReporter.ExceptionsThrown.Count);
						AssertContains(exception1.Message, ErrorReporter.ExceptionsThrown.Dequeue());
						AssertContains(exception2.Message, ErrorReporter.ExceptionsThrown.Dequeue());
						AssertContains(exception3.Message, ErrorReporter.ExceptionsThrown.Dequeue());
					});
			}

			public void TestAggregateExceptionIsHandled()
			{
				// Arrange
				var handledException = new CriticalExceptionForTest();
				Assert("Precondition. Should be any handled exception.", topLevelExceptionHandler.HandleSpecificExceptions(handledException));
				var unhandledException = new Exception();
				Assert("Precondition. Should be any unhandled exception.", !topLevelExceptionHandler.HandleSpecificExceptions(unhandledException));

				CombineAssertions(() =>
				{
					Test(true, new AggregateException(handledException));
					Test(true, new AggregateException(handledException, handledException));
					Test(true, new AggregateException(new AggregateException(handledException)));
					Test(false, new AggregateException(unhandledException));
					Test(false, new AggregateException(handledException, unhandledException));
					Test(false, new AggregateException(unhandledException, handledException));
					Test(false, new AggregateException(new AggregateException(unhandledException)));
					Test(true, new AggregateException(handledException, new AggregateException(handledException)));
					Test(false, new AggregateException(unhandledException, new AggregateException(handledException)));
					Test(false, new AggregateException(handledException, new AggregateException(unhandledException)));
				});

				void Test(bool expected, Exception exception)
				{
					// Act
					var result = topLevelExceptionHandler.HandleSpecificExceptions(exception);

					// Assert
					AssertEquals(expected, result);
				}
			}

			public void TestExceptionDuringHandleIsReturned()
			{
				CombineAssertions(() =>
				{
					Test("Generic Exception", new Exception("Some other generic exception"));
					Test("Aggregate Exception", new AggregateException(new Exception("Some other generic exception")));
					Test("Another Generic Exception", new NullReferenceException("Some other generic exception"));
				});

				void Test(string exceptionMessage, Exception thrownException)
				{
					// Arrange
					var testException = new ExceptionThrowInHandle(exceptionMessage, thrownException);

					// Act
					var result = topLevelExceptionHandler.HandleSpecificExceptions(testException, out var exceptionDuringHandle);

					// Assert
					AssertEquals(false, result);
					AssertEquals(thrownException, exceptionDuringHandle);
				}
			}

			public void TestSEHExceptionIsHandled()
			{
				// Act
				var result = topLevelExceptionHandler.HandleSpecificExceptions(new SEHException());

				// Assert
				AssertEquals(true, result);
			}

			[Serializable]
			class ExceptionThrowInHandle : Exception, IExceptionReporterExtender
			{
				public ExceptionThrowInHandle(string message, Exception exceptionToThrow)
					: base(message)
				{
					this.exceptionToThrow = exceptionToThrow;
				}

#if NETFRAMEWORK
				protected ExceptionThrowInHandle(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
					: base(info, context)
				{
				}
#endif

				bool IExceptionReporterExtender.HandleException()
				{
					if (!exceptionThrown)
					{
						exceptionThrown = true;
						throw exceptionToThrow;
					}

					return true;
				}

				readonly Exception exceptionToThrow;
				bool exceptionThrown;
			}

			TopLevelExceptionHandler topLevelExceptionHandler;
		}

		[ExpectNoNewDbConnection]
		public class TopLevelExceptionHandlerServiceLogTests : TransactionedTestCase
		{
			protected override void SetUp()
			{
				base.SetUp();
				topLevelExceptionHandler = new TopLevelExceptionHandler();
			}

			[ExpectNoExceptions]
			[TestRequiresAdministrativePrivileges("Interacting with Event logs may require administrator privileges")]
			public void TestHandleSpecificExceptionFailureToLogToErrorReporterIsReportedInServiceLogs()
			{
				CombineAssertions(() =>
				{
					Test("Generic Exception", new Exception("Some other generic exception"));
					Test("Aggregate Exception", new AggregateException(new Exception("Some other generic exception")));
					Test("Another Generic Exception", new NullReferenceException("Some other generic exception"));
				});

				void Test(string exceptionMessage, Exception thrownException)
				{
					// Arrange
					var testException = new ExceptionThrowInHandle(exceptionMessage, thrownException);
					var dateTime = DateTime.Now;
					var expectedMessages = new[]
					{
						$"Exception: {testException.Message}",
						$"Exception: {thrownException.Message}",
					};

					// Act
					topLevelExceptionHandler.HandleSpecificExceptions(testException);

					// Assert
					var reportedExceptions = CheckEventLog(Constants.ProductName, string.Empty, dateTime);
					var reportedExceptionMessages = reportedExceptions.Select(ex => ex.exceptionMessage);
					AssertContainsExactElementsInAnyOrder(expectedMessages, reportedExceptionMessages);
				}
			}

			[ExpectNoExceptions]
			[TestRequiresAdministrativePrivileges("Interacting with Event logs may require administrator privileges")]
			public void TestHandleUnhandledExceptionFailureToLogToErrorReporterIsReportedInServiceLogs()
			{
				CombineAssertions(() =>
				{
					Test("Generic Exception", new Exception("Some other generic exception"));
					Test("Aggregate Exception", new AggregateException(new Exception("Some other generic exception")));
					Test("Another Generic Exception", new NullReferenceException("Some other generic exception"));
				});

				void Test(string exceptionMessage, Exception thrownException)
				{
					// Arrange
					var testException = new ExceptionThrowInHandle(exceptionMessage, thrownException);
					var dateTime = DateTime.Now;
					var expectedMessages = new[]
					{
						$"Exception: {testException.Message}",
						$"Exception: {thrownException.Message}",
					};

					// Act
					topLevelExceptionHandler.HandleUnhandledException(testException, (_, __) => { });

					// Assert
					var reportedExceptions = CheckEventLog(Constants.ProductName, string.Empty, dateTime);
					var reportedExceptionMessages = reportedExceptions.Select(ex => ex.exceptionMessage);
					AssertContainsExactElementsInAnyOrder(expectedMessages, reportedExceptionMessages);
				}
			}

			static IEnumerable<(string exceptionMessage, string stackTrace)> CheckEventLog(string source, string messageStartsWith, DateTime after)
			{
				var eventLogs = new List<string>();
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
						var message = eventRecord.FormatDescription();
						if (message.StartsWith(messageStartsWith, StringComparison.OrdinalIgnoreCase))
						{
							eventLogs.Add(eventRecord.FormatDescription());
						}

						eventRecord = eventlogReader.ReadEvent();
					}
				}

				return eventLogs.Select(log =>
				{
					var logSplit = log.Split(new[] { "\r\nStack trace:" }, StringSplitOptions.None);
					return (logSplit[0], logSplit[1]);
				});
			}

			[Serializable]
			class ExceptionThrowInHandle : Exception, IExceptionReporterExtender
			{
				public ExceptionThrowInHandle(string message, Exception exceptionToThrow)
					: base(message)
				{
					this.exceptionToThrow = exceptionToThrow;
				}

#if NETFRAMEWORK
				protected ExceptionThrowInHandle(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
					: base(info, context)
				{
				}
#endif

				bool IExceptionReporterExtender.HandleException()
				{
					if (!exceptionThrown)
					{
						exceptionThrown = true;
						throw exceptionToThrow;
					}

					return true;
				}

				readonly Exception exceptionToThrow;
				bool exceptionThrown;
			}

			TopLevelExceptionHandler topLevelExceptionHandler;
		}

		public void TestCannotLoadObjectTypeException_PagingFileIsTooSmall()
		{
			var handler = new TopLevelExceptionHandler();
			var factory = new BusinessObjectFactory();
			var exception = new CannotLoadObjectTypeException("a", new FileLoadException("a"));
			var fileLoadException = exception.InnerException;
			unchecked
			{
				typeof(FileLoadException).GetProperty("HResult", BindingFlags.Public | BindingFlags.Instance).GetSetMethod(true).Invoke(fileLoadException, new object[] { (int)0x800705AF });
			}
			var isExceptionHandled = handler.HandleSpecificExceptions(exception);
			AssertEquals(true, isExceptionHandled);
		}

		public void TestHandleExternalStorageException()
		{
			// Arrange
			UnitTestUserNotification.Instance.ClearMessages();

			var isReportExceptionForDeveloperCalled = false;
			var mock = new Mock<ExternalStorageException>("", "S3", null);
			mock.SetupGet(e => e.UnableToAccessStorageFriendlyMessage).Returns("Failed to access external storage");
			mock.Setup(e => e.ReportExceptionForDeveloper()).Callback(() => isReportExceptionForDeveloperCalled = true);

			// Act
			var handler = new TopLevelExceptionHandler();
			var handled = handler.HandleSpecificExceptions(mock.Object);

			// Assert
			AssertEquals("ExternalStorageException should be handled", true, handled);
			AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
			AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Correct exception message", "Failed to access external storage", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("ExternalStorageException should be called", true, isReportExceptionForDeveloperCalled);
		}

		public void TestHandleVirusDetectedException()
		{
			// Arrange
			UnitTestUserNotification.Instance.ClearMessages();

			var filename = "test.txt";
			var expectedFriendlyMessage = $"The file \"{filename}\" has been detected with virus and therefore cannot be saved or opened.";

			var mock = new Mock<VirusDetectedException>(filename);
			mock.SetupGet(e => e.VirusDetectedFriendlyMessage).Returns(expectedFriendlyMessage);

			// Act
			var handler = new TopLevelExceptionHandler();
			var handled = handler.HandleSpecificExceptions(mock.Object);

			// Assert
			AssertEquals("VirusDetectedException should be handled", true, handled);
			AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
			AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Correct exception message", expectedFriendlyMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[ExpectNoExceptions]
		public void TestHandleObjectDisposedException()
		{
			var mock = new Mock<ObjectDisposedException>("");
			mock.SetupGet(a => a.Message).Returns("Safe handle has been closed");
			mock.SetupGet(a => a.StackTrace).Returns(@"at System.Runtime.InteropServices.SafeHandle.DangerousAddRef(Boolean& success)
at System.StubHelpers.StubHelpers.SafeHandleAddRef(SafeHandle pHandle, Boolean& success)
at Microsoft.Win32.Win32Native.SetEvent(SafeWaitHandle handle)
at System.Threading.EventWaitHandle.Set()
at System.Windows.Input.PenThreadWorker.WorkerOperation.DoWork()
at System.Windows.Input.PenThreadWorker.ThreadProc()
at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)
at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)
at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state)
at System.Threading.ThreadHelper.ThreadStart()");

			var handler = new TopLevelExceptionHandler();
			var handled = handler.HandleSpecificExceptions(mock.Object);
			Assert("Should be handled.", handled);
		}

		[ExpectNoExceptions]
		public void TestHandleDbConcurrencyException_NotifyUserWithoutErrorReport()
		{
			var handler = new TopLevelExceptionHandler();
			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyBusinessObject>();
			var exception = new ZSaveConcurrencyException(new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), ((IBusinessObjectInternals)dummy).Row, Db.Connection), factory), true);
			var isExceptionHandled = handler.HandleSpecificExceptions(exception);
			AssertEquals(true, isExceptionHandled);
		}

		public void TestHandleDbConcurrencyException_NoDuplicatedErrorReport()
		{
			var handler = new TopLevelExceptionHandler();
			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyBusinessObject>();
			var exception = new ZSaveConcurrencyException(new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), ((IBusinessObjectInternals)dummy).Row, Db.Connection), factory));

			AssertEquals(true, handler.HandleSpecificExceptions(exception));
			AssertEquals("Developer error was reported", true, ErrorReporter.ExceptionsThrown.Any(ex => ex.Contains("--- Save Aborted Due to Concurrency Check ---")));
			AssertEquals("Duplicated developer Error was not reported", 0, ExceptionReporterTestListener.Instance.Count);
			ErrorReporter.Clear();
		}

		public void TestSizeCannotBeNegativeExceptionShouldBeHandled()
		{
			var handler = new TopLevelExceptionHandler();
			var exception = new ArgumentException("Width and Height must be non-negative.");

			AssertEquals(true, handler.HandleSpecificExceptions(exception));
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestOnBatchServerFileErrorWeGetAdditionalInformation()
		{
			ExceptionReporter.Instance.AdditionalContextDetails = "Service task: ASS";

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				try
				{
					throw new IOException("I've got to throw to get the stack trace");
				}
				catch (IOException ex)
				{
					var handler = new TopLevelExceptionHandler();
					Assert("PRE: Should be handled", handler.HandleSpecificExceptions(ex));

					var message = UnitTestUserNotification.Instance.LastMessage;

					Assert("Expecting an error message", message.WasError);

					var containsTaskName = message.Contains("ASS");
					var containsStacktrace = message.Contains("TestOnBatchServerFileErrorWeGetAdditionalInformation");

					Assert("We want to have the service name, file name and a stack trace in the message:\r\n\r\n" + message, containsTaskName && containsStacktrace);
				}
			}
		}

		public void TestGetDatabaseNameFromExceptionMessage()
		{
			var handler = new TopLevelExceptionHandler();

			var errorMessage = $"The database owner SID recorded in the master database differs from the database owner SID recorded in database 'Odyssey'. You should correct this situation by resetting the owner of database 'Odyssey' using the ALTER AUTHORIZATION statement.";
			AssertEquals("Database name from exception message: Odyssey", "Odyssey", handler.GetDatabaseNameFromOwnerSIDDiffersFromMasterExceptionMessage(errorMessage));

			errorMessage = $"The database owner SID recorded in the master database differs from the database owner SID recorded in database ''. You should correct this situation by resetting the owner of database using the ALTER AUTHORIZATION statement.";
			AssertEquals("Database name from exception message: (empty database name)", string.Empty, handler.GetDatabaseNameFromOwnerSIDDiffersFromMasterExceptionMessage(errorMessage));
			AssertEquals("(Is error handled? (empty database name)", false, handler.HandleOwnerSIDDiffersFromMaster(new Exception(errorMessage)));

			var nonExistingDbName = "ANonExisting(*_*)DatabaseName";
			errorMessage = $"The database owner SID recorded in the master database differs from the database owner SID recorded in database '{nonExistingDbName}'. You should correct this situation by resetting the owner of database using the ALTER AUTHORIZATION statement.";
			AssertEquals("Database name from exception message: (non-existing database)", nonExistingDbName, handler.GetDatabaseNameFromOwnerSIDDiffersFromMasterExceptionMessage(errorMessage));
			AssertEquals("(Is error handled? (non-existing database)", false, handler.HandleOwnerSIDDiffersFromMaster(new Exception(errorMessage)));
		}

		public void TestTempDbOutOfError()
		{
			var handler = new TopLevelExceptionHandler();
			var exception = SqlExceptionBuilder.CreateSqlException(3958, "Tempdb out of space");
			AssertNoExceptionThrown(() => handler.HandleUnhandledException(exception, (_, __) => throw new Exception("Failed to handle")));
			AssertContains("Tempdb out of space", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		class ThrowInnerDbUpgradeExceptionDuringShowErrorDialogIfNotSilentlyHandled : TopLevelExceptionHandler
		{
			protected override void ShowErrorDialogIfNotSilentlyHandled(DbErrorHandler dbErrorHandler, string friendlyMessage, Exception outermostExceptionForErrorReport)
			{
				throw new ApplicationException("Error while performing some action - Lets wrap the SQL exception:", new DatabaseUpgradeInProgressException());
			}
		}

		class ThrowInnerSqlExceptionDuringShowErrorDialogIfNotSilentlyHandled : TopLevelExceptionHandler
		{
			protected override void ShowErrorDialogIfNotSilentlyHandled(DbErrorHandler dbErrorHandler, string friendlyMessage, Exception outermostExceptionForErrorReport)
			{
				throw SqlExceptionBuilder.CreateSqlException(0, 0, 64, Db.ServerName, "A connection was successfully established with the server, but then an error occurred during the pre-login handshake. (provider: TCP Provider, error: 0 - The specified network name is no longer available.)", null, 0, new Win32Exception(0));
			}
		}

		public void TestInnerSQLExceptionDuringHandle()
		{
			var myReporter = new ThrowInnerSqlExceptionDuringShowErrorDialogIfNotSilentlyHandled();
			var ex = SqlExceptionBuilder.CreateSqlException(2, 0, 11, Db.ServerName, "Error Number 2 : A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)", "", 0);
			var handled = myReporter.HandleSqlException(ex, ex);
			AssertEquals(true, handled);
		}

		class ThrowDatabaseUpgradedExceptionDuringShowErrorDialogIfNotSilentlyHandled : TopLevelExceptionHandler
		{
			protected override void ShowErrorDialogIfNotSilentlyHandled(DbErrorHandler errorHandler, string friendlyMessage, Exception outermostExceptionForErrorReport)
			{
				throw new DatabaseUpgradedException();
			}
		}

		public void TestDatabaseUpgradedExceptionDuringHandleSqlException()
		{
			var actualDbEnv = DbEnv.Instance;
			try
			{
				bool databseUpgradedExceptionHandled = false;
				var mockGuidPlugin = new Mock<IDbConnectionGuiPlugin>();
				var mockDbEnv = new Mock<BaseDbEnvironment>();
				mockDbEnv.CallBase = true;
				mockDbEnv.Setup(m => m.ConnectionGuiPlugin)
						 .Returns(mockGuidPlugin.Object);
				mockGuidPlugin.Setup(m => m.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradeException>()))
							  .Callback(() => databseUpgradedExceptionHandled = true);
				DbEnv.SetDbEnvironment(mockDbEnv.Object);

				var exceptionReporter = new ThrowDatabaseUpgradedExceptionDuringShowErrorDialogIfNotSilentlyHandled();
				var ex = SqlExceptionBuilder.CreateSqlException(2, 0, 11, Db.ServerName, "Error Number 2 : A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)", "", 0);
				exceptionReporter.HandleUnhandledException(ex, (s, e) => databseUpgradedExceptionHandled = false);
				AssertEquals(true, databseUpgradedExceptionHandled);
				mockGuidPlugin.VerifyAll();
				mockDbEnv.VerifyAll();
			}
			finally
			{
				DbEnv.SetDbEnvironment(actualDbEnv);
			}
		}

		public void TestHandleSpecificExceptions_DatabaseUpgradedExceptionDuring()
		{
			var mockGuidPlugin = new Mock<IDbConnectionGuiPlugin>();
			mockGuidPlugin.Setup(x => x.HandleDbConcurrencyException(It.IsAny<Exception>()))
				.Callback(() => ((IDbReconnectionHandling)Db.Connection).CloseAndReopenConnection());

			var mockDbEnv = new Mock<BaseDbEnvironment>();
			mockDbEnv.CallBase = true;
			mockDbEnv.Setup(m => m.ConnectionGuiPlugin)
				.Returns(mockGuidPlugin.Object);

			var bumpedSchemaVersion = ObjectFactory.Get<IDatabaseAspectVersions>().SchemaVersion.Major + 10;
			var versionMock = new Mock<IDatabaseAspectVersions>();
			versionMock
				.Setup(x => x.SchemaVersion)
				.Returns(new VersionLabel(bumpedSchemaVersion, 0));

			using (ObjectFactory.Substitute(versionMock.Object))
			using (DbEnv.SetTemporaryDbEnvironment(mockDbEnv.Object))
			{
				var dummyBizO = new BusinessObjectFactory().New<DummyBusinessObject>();
				var sqlException = SqlExceptionBuilder.CreateSqlException(1, "SQL error");
				var concurrencyException = new ZDataConcurrencyException(sqlException, ((INeedRow)dummyBizO).Row, Db.Connection);

				var topLevelHandler = new TopLevelExceptionHandler();
				topLevelHandler.HandleSpecificExceptions(concurrencyException, out var exceptionDuringHandle);

				AssertNull("exceptionDuringHandle", exceptionDuringHandle);
				mockGuidPlugin.Verify(m => m.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradeException>()), Times.Once);
				AssertEquals("DatabaseUpgradedExceptionHasBeenThrown", false, Db.Connection.DatabaseUpgradedExceptionHasBeenThrown);
			}
		}

		public void TestHandleSqlException_DontShowErrorDiagolue()
		{
			var innerError = SqlExceptionBuilder.CreateSqlError(53, 1, 1, "", "Cannot connect to database", "", 1);
			var innerErrorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(innerError);
			var innerException = SqlExceptionBuilder.CreateSqlException(innerErrorCollection);

			var outerError = SqlExceptionBuilder.CreateSqlError(53, 1, 1, "", "Cannot open database", "", 1);
			var outerErrorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(outerError);
			var outerException = SqlExceptionBuilder.CreateSqlException(outerErrorCollection);

			Db.Connection.Dispose();

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var handler = new TopLevelExceptionHandler();
				AssertEquals(true, handler.HandleSqlException(outerException, new Exception()));
			}
		}

		public void TestHandleGenericIoException()
		{
			// Arrange
			const string exceptionMessage = "This is a test message";
			Exception exceptionToHandle = new IOException(exceptionMessage);
			var handler = new TopLevelExceptionHandler();
			// Act
			var isExceptionHandled = true;
			handler.HandleUnhandledException(exceptionToHandle, (s, e) => isExceptionHandled = false);
			// Assert
			Assert(isExceptionHandled);
			AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
			AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertStartsWith("Correct exception message", $"File operation failed. Details: {exceptionMessage}", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestFileNotFoundExceptionIsHandled()
		{
			// Arrange
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				Exception exceptionToHandle = new FileNotFoundException(
					"Could not load file or assembly 'XmlDiffPatch, Version=1.0.8.28, Culture=neutral, PublicKeyToken=4f570df270576350' or one of its dependencies. The system cannot find the file specified.",
					"XmlDiffPatch, Version=1.0.8.28, Culture=neutral, PublicKeyToken=4f570df270576350");
				var handler = new TopLevelExceptionHandler();

				// Act
				var result = handler.HandleSpecificExceptions(exceptionToHandle);

				// Assert
				AssertEquals(true, result);
			}
		}

		public void TestUpgradeManagerException_InfrastuctureDbError()
		{
			//doesn't have the inner Win32Exception but that should be irrelevant...
			var sqlException = SqlExceptionBuilder.CreateSqlException(10054,
				"A transport-level error has occurred when sending the request to the server. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.)"
				);
			var exception = new Exception(
				"Database View, Procedure, Function & Trigger Upgrade failed.\r\nA transport-level error has occurred when sending the request to the server. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.)", sqlException);
			var upgradeManagerException = new UpgradeManagerException("Upgrade manager exception.", exception);
			ErrorReporter.ReportOnce("Failed to upgrade database", upgradeManagerException);
			Assert(true); //test will fail if error report is sent
		}

		public void TestExceptionThrownShouldBeLogInErrorReporter()
		{
			try
			{
				using (EnsureCurrentUserIsDeveloperLogin())
				{
					Globals.SetIsUnitTestingProductionFunctionality(true);
					var handler = new TopLevelExceptionHandler();

					SetUpForLogExceptionsThrownMethod(handler, 10);

					ErrorReporter.ReportOnce("test1", "test");

					Assert(ErrorReporter.ExceptionsThrown.Count >= 10);
					AssertNotNullOrEmpty(ErrorReporter.LastMessageReported);

					Assert(ErrorReporter.ExceptionsThrown.Select(x => x.Contains("Type :System.Exception\r\nMessage :Something went wrong.\r\nStacktrace :\r\n")).Any());
				}
			}
			finally
			{
				ErrorReporter.Clear();
				Globals.SetIsUnitTestingProductionFunctionality(false);
			}
		}

		public void TestExceptionThrownShouldBeLogInErrorReporter_Stacktrace()
		{
			var handler = new TopLevelExceptionHandler();
			try
			{
				Globals.SetIsUnitTestingProductionFunctionality(true);
				throw new Exception("what came first the egg or the chicken");
			}
			catch (Exception exception)
			{
				using (EnsureCurrentUserIsDeveloperLogin())
				{
					var exceptionhandled = true;
					handler.HandleUnhandledException(exception, (s, e) => exceptionhandled = false);
					ErrorReporter.ReportOnce("test1", "test");

					Assert(!exceptionhandled);
					AssertNotNullOrEmpty(ErrorReporter.LastMessageReported);
					AssertContains("Stacktrace :   at Enterprise.ZArchitecture.Core.Testing.TopLevelExceptionHandlerTests.TestExceptionThrownShouldBeLogInErrorReporter_Stacktrace() in", ErrorReporter.ExceptionsThrown.First());
				}
			}
			finally
			{
				ErrorReporter.Clear();
				Globals.SetIsUnitTestingProductionFunctionality(false);
			}
		}

		public void TestExceptionThrownLoggedInErrorReporterDontExceedLimit()
		{
			try
			{
				using (EnsureCurrentUserIsDeveloperLogin())
				{
					Globals.SetIsUnitTestingProductionFunctionality(true);
					var handler = new TopLevelExceptionHandler();
					SetUpForLogExceptionsThrownMethod(handler, 20);

					ErrorReporter.ReportOnce("test2", "test");

					Assert(ErrorReporter.ExceptionsThrown.Count == 15);
					Assert(ErrorReporter.ExceptionsThrown.Select(x => x.Contains("Type :System.Exception\r\nMessage :Something went wrong.\r\nStacktrace :\r\n")).Any());
				}
			}
			finally
			{
				ErrorReporter.Clear();
				Globals.SetIsUnitTestingProductionFunctionality(false);
			}
		}

		void SetUpForLogExceptionsThrownMethod(TopLevelExceptionHandler handler, int exceptionNumber)
		{
			var exceptionsToThrwon = Enumerable.Range(0, exceptionNumber).Select(i => new Exception("Something went wrong.")).ToArray();

			var exceptionhandled = true;
			foreach (var exception in exceptionsToThrwon)
			{
				handler.HandleUnhandledException(exception, (s, e) => exceptionhandled = false);
			}
			Assert(!exceptionhandled);
		}

		public void TestExceptionThrownLoggedByDifferentThreads()
		{
			try
			{
				using (EnsureCurrentUserIsDeveloperLogin())
				{
					Globals.SetIsUnitTestingProductionFunctionality(true);
					var syncEvent = new ManualResetEvent(false);

					var thread = new Thread(() =>
					{
						var handler2 = new TopLevelExceptionHandler();
						SetUpForLogExceptionsThrownMethod(handler2, 10);
						syncEvent.Set();
					});

					var handler = new TopLevelExceptionHandler();
					handler.HandleUnhandledException(new InvalidOperationException("FirstExceptionThrew"), (s, e) => thread.Start());

					syncEvent.WaitOne();

					ErrorReporter.ReportOnce("test3", "test");

					Assert(ErrorReporter.ExceptionsThrown.Select(x => x.Contains("Type :System.Exception\r\nMessage :Something went wrong.\r\nStacktrace :\r\n")).Any());
					Assert(ErrorReporter.ExceptionsThrown.Select(x => x.Contains("Type: System.InvalidOperationException\r\nMessage: FirstExceptionThrew\r\nStacktrace:\r\n")).Any());
				}
			}
			finally
			{
				ErrorReporter.Clear();
				Globals.SetIsUnitTestingProductionFunctionality(false);
			}
		}

		IDisposable EnsureCurrentUserIsDeveloperLogin()
		{
			var isDeveloperLogin = EnvProxy.Instance.CurrentUser?.IsDeveloperLogin ?? false;
			if (!isDeveloperLogin)
			{
				var context = EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
				var mockLoginToken = new Mock<ILoginToken>();
				mockLoginToken.Setup(m => m.IsDeveloper)
					.Returns(true);
				EnvProxy.Instance.CurrentUser.LoginToken = mockLoginToken.Object;
				return context;
			}

			return DisposableAction.NoAction;
		}

		public void TestHandleUnauthorizedAccessException()
		{
			const string exceptionMessage = "This is a test message";
			var handler = new TopLevelExceptionHandler();
			var exceptionToHandle = new UnauthorizedAccessException(exceptionMessage);
			var isExceptionHandled = handler.HandleSpecificExceptions(exceptionToHandle);

			Assert(isExceptionHandled);
			AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
			AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertStartsWith("Correct exception message", string.Format("File operation failed. Details: {0}", exceptionMessage), UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestHandleSpecificCOMException()
		{
			const string exceptionMessage = "The user name or password is incorrect";

			var exception = new COMException(exceptionMessage);
			var handler = new TopLevelExceptionHandler();

			Assert("Exception should be handled", handler.HandleSpecificExceptions(exception));
			AssertEquals("The operation could not be completed because your log in credentials are not valid for this server", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestWin32ExceptionErrorCreatingWindowHandle()
		{
			var handler = new TopLevelExceptionHandler();
			ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest());
			AssertEquals("Code 1406 should not be handled", false, handler.HandleSpecificExceptions(new Win32Exception(1406, "Error creating window handle.")));
			AssertEquals("Code 1158 should be handled", true, handler.HandleSpecificExceptions(new Win32Exception(1158, "Error creating window handle.")));
			AssertEquals("Code 8 should not be handled", false, handler.HandleSpecificExceptions(new Win32Exception(8, "Error creating window handle.")));
		}

		public void TestWin32ExceptionErrorNotEnoughQuote()
		{
			var handler = new TopLevelExceptionHandler();
			ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest());

			var win32Exception = new Mock<Win32Exception>(1816, "Error creating window handle.");
			win32Exception.Setup(m => m.StackTrace)
				.Returns("MS.Internal.ShutDownListener.HandleShutDown");
			AssertEquals("Code 1816 should be handled when FlakyShutdownException", true, handler.HandleSpecificExceptions(win32Exception.Object));

			win32Exception = new Mock<Win32Exception>(1816, "Error creating window handle.");
			win32Exception.Setup(m => m.StackTrace)
				.Returns("MainForm.WndProc\r\nControl.WmShowWindow");
			AssertEquals("Code 1816 should be handled when IsMainFormFailedToShowException", true, handler.HandleSpecificExceptions(win32Exception.Object));

			win32Exception = new Mock<Win32Exception>(1816, "Error creating window handle.");
			win32Exception.Setup(m => m.StackTrace)
				.Returns("Timer.set_Enabled\r\nTimer.OnTick");
			AssertEquals("Code 1816 should be handled when IsTimerFailedToRestartException", true, handler.HandleSpecificExceptions(win32Exception.Object));

			win32Exception = new Mock<Win32Exception>(1816, "Error creating window handle.");
			win32Exception.Setup(m => m.StackTrace)
				.Returns("MainForm.SetVisibleCore");
			AssertEquals("Error creating handle when showing main form should be handled", true, handler.HandleSpecificExceptions(win32Exception.Object));

			win32Exception = new Mock<Win32Exception>(1816, "Error creating window handle.");
			win32Exception.Setup(m => m.StackTrace)
				.Returns("Test");
			AssertEquals("Code 1816 should be not handled when not (any of the above)", false, handler.HandleSpecificExceptions(win32Exception.Object));
		}

		public void TestWin32ExceptionErrorShowingMainForm()
		{
			var handler = new TopLevelExceptionHandler();
			ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest());

			var win32Exception = new Mock<Win32Exception>(0, "Error creating window handle.");
			win32Exception.Setup(m => m.StackTrace).Returns("MainForm.WndProc\r\nControl.WmShowWindow");
			win32Exception.Setup(m => m.Message).Returns("Test message");
			AssertEquals("Error creating handle when showing main form should be handled", true, handler.HandleSpecificExceptions(win32Exception.Object));
			AssertEquals("Error showing main form. Try closing and restarting programs and rebooting if problems persist. You can also try increasing the size of your paging file.\r\nTest message", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();

			win32Exception = new Mock<Win32Exception>(78, "Error creating window handle.");
			win32Exception.Setup(m => m.StackTrace).Returns("MainForm.SetVisibleCore");
			win32Exception.Setup(m => m.Message).Returns("Test message");
			AssertEquals("Error creating handle when showing main form should be handled", true, handler.HandleSpecificExceptions(win32Exception.Object));
			AssertEquals("Error showing main form. Try closing and restarting programs and rebooting if problems persist. You can also try increasing the size of your paging file.\r\nTest message", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();
		}

		public void TestWin32ExceptionErrorRefreshingHeartbeat()
		{
			var handler = new TopLevelExceptionHandler();
			ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest());

			var win32Exception = new Mock<Win32Exception>(0, "Error creating window handle.");
			win32Exception.Setup(m => m.StackTrace).Returns("Timer.set_Enabled\r\nTimer.OnTick");
			win32Exception.Setup(m => m.Message).Returns("Test message");
			AssertEquals("Error creating handle when showing main form should be handled", true, handler.HandleSpecificExceptions(win32Exception.Object));
			AssertEquals("Error initializing heartbeat timer. Try closing and restarting programs and rebooting if problems persist.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();

			win32Exception = new Mock<Win32Exception>(78, "Error creating window handle.");
			win32Exception.Setup(m => m.StackTrace).Returns("Timer.set_Enabled\r\nTimer.OnTick");
			win32Exception.Setup(m => m.Message).Returns("Test message");
			AssertEquals("Error creating handle when showing main form should be handled", true, handler.HandleSpecificExceptions(win32Exception.Object));
			AssertEquals("Error initializing heartbeat timer. Try closing and restarting programs and rebooting if problems persist.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();
		}

		public void TestWin32ExceptionWithTimeoutErrorCodeNotReported()
		{
			var handler = new TopLevelExceptionHandler();

			ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest());
			CombineAssertions(() =>
			{
				Assert(handler.HandleSpecificExceptions(new Win32Exception(8, "Not enough storage is available to process this command")));
				Assert(handler.HandleSpecificExceptions(new Win32Exception(258, "The wait operation timed out")));
				Assert(handler.HandleSpecificExceptions(new Win32Exception(121, "The semaphore timeout period has expired")));
				Assert(handler.HandleSpecificExceptions(new Win32Exception(1326, "The user name or password is incorrect")));
				Assert(handler.HandleSpecificExceptions(new Win32Exception(10054, "An existing connection was forcibly closed by the remote host")));
			});
		}

		public class TestsThatRequiresNewDbConnection : TestCase
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
			public void TestConnectionClosedException()
			{
				try
				{
					using (var connection = Db.NewExtraConnectionToMainDb())
					{
						connection.BeginTransaction();
						connection.CloseConnection();
						((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb);
						Fail("Expected for an InvalidOperationException.");
					}
				}
				catch (Exception ex)
				{
					CombineAssertions(() =>
					{
						AssertEquals("Exception type", typeof(InvalidOperationException), ex.GetType());
						Assert("Exception message is incorrect:\r\n" + ex.Message, ex.Message.Contains("The connection is closed."));

						var handler = new TopLevelExceptionHandler();
						Assert(handler.HandleSpecificExceptions(ex));
					});
				}
			}

			[UseSnapshotProtection]
			public void TestHandleSqlException_WhenFromRefDataRepo()
			{
				string errMessage = $@"The DELETE statement conflicted with the REFERENCE constraint ""AccChargeCode_AC_GC_FK2_GlbCompany_RRR_120N"". The conflict occurred in database ""{Db.DatabaseName}"", table ""dbo.AccChargeCode"", column 'AC_GC'.
The statement has been terminated.";
				var innerError = SqlExceptionBuilder.CreateSqlError(547, 1, 1, "", errMessage, "", 1);
				var innerErrorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(innerError);
				var innerException = SqlExceptionBuilder.CreateSqlException(innerErrorCollection);
				var outerExceptionForErrorReport = new RefDataException(innerException);

				Db.Connection.Dispose();

				using (Globals.SetIsUserInteractiveForTest(false))
				{
					var handler = new TopLevelExceptionHandler();
					AssertEquals(true, handler.HandleSqlException(innerException, outerExceptionForErrorReport));
					AssertEquals(outerExceptionForErrorReport, ExceptionReporter.Instance.ExceptionReportedSilentlyForTest);
				}
			}

			[GuiTest]
			public void TestInnerDbUpgradeExceptionDuringHandle()
			{
				var myReporter = new ThrowInnerDbUpgradeExceptionDuringShowErrorDialogIfNotSilentlyHandled();
				var ex = SqlExceptionBuilder.CreateSqlException(
						SqlExceptionBuilder.CreateSqlErrorCollection(
							SqlExceptionBuilder.CreateSqlError(2, 0, 11, Db.ServerName, "Error Number 2 : A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)", "", 0)));
				var handled = myReporter.HandleSqlException(ex, ex);
				AssertEquals(true, handled);
			}

			public void TestSQLHandleLoadingUntrustedAssemblyException()
			{
				const string message = "An error occurred in the Microsoft .NET Framework while trying to load assembly id 65564. The server may be running out of resources, or the assembly may not be trusted with PERMISSION_SET = EXTERNAL_ACCESS or UNSAFE. Run the query again, or check documentation to see how to solve the assembly trust issues.";
				var sqlException = SqlExceptionBuilder.CreateSqlException(10314, message);

				var handler = new TopLevelExceptionHandler();
				Assert(handler.HandleSpecificExceptions(sqlException));
			}

			public void TestSQLHandleInvalidColumnNameException_SDDatabase()
			{
				var sqlError1 = SqlExceptionBuilder.CreateSqlError(207, 1, 1, "", "Invalid column name 'SC_EncryptedDataKey'.", "", 1);
				var sqlError2 = SqlExceptionBuilder.CreateSqlError(207, 1, 1, "", "Invalid column name 'SC_SCK_MasterKey'.", "", 1);
				var sqlErrorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(sqlError1, sqlError2);
				var sqlException = SqlExceptionBuilder.CreateSqlException(sqlErrorCollection);

				var handler = new TopLevelExceptionHandler();
				AssertEquals("If missing columns are in the main database, exception should be handled", expected: true, handler.HandleSpecificExceptions(sqlException));
				AssertEquals("An error occurred while accessing the eDoc. This can be due to an outdated eDocs database schema. Please inform your System Administrator and have them ensure the eDocs database(s) are restored correctly.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();

				sqlError1 = SqlExceptionBuilder.CreateSqlError(207, 1, 1, "", "Invalid column name 'SC_InvalidColumn'.", "", 1);
				sqlError2 = SqlExceptionBuilder.CreateSqlError(207, 1, 1, "", "Invalid column name 'SC_SCK_MasterKey'.", "", 1);
				sqlErrorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(sqlError1, sqlError2);
				sqlException = SqlExceptionBuilder.CreateSqlException(sqlErrorCollection);

				AssertEquals("If missing columns are not in the main database, exception should not be handled", expected: false, handler.HandleSpecificExceptions(sqlException));
			}

			public void TestSQLHandleInvalidColumnNameException_NotSDDatabase()
			{
				var sqlError1 = SqlExceptionBuilder.CreateSqlError(207, 1, 1, "", "Invalid column name 'SM_EncryptedDataKey'.", "", 1);
				var sqlError2 = SqlExceptionBuilder.CreateSqlError(207, 1, 1, "", "Invalid column name 'SM_SCK_MasterKey'.", "", 1);
				var sqlErrorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(sqlError1, sqlError2);
				var sqlException = SqlExceptionBuilder.CreateSqlException(sqlErrorCollection);

				var handler = new TopLevelExceptionHandler();
				AssertEquals("If missing columns are not from StorageDocs table, the exception should not be handled", expected: false, handler.HandleSpecificExceptions(sqlException));
			}

			public void TestSQLInsufficientMemoryInBufferPoolExceptionIsHandled()
			{
				// Arrange
				var sqlException = SqlExceptionBuilder.CreateSqlException(802, "There is insufficient memory available in the buffer pool.");
				var handler = new TopLevelExceptionHandler();

				// Act
				var handled = handler.HandleSpecificExceptions(sqlException);

				// Assert
				Assert("Insufficient memory in buffer pool exception should be handled", handled);
			}

			[ExpectNoExceptions]
			public void TestFailureToAcquireSqlAppLockErrorAreNotReportedOnHandleOwnerSIDDiffersFromMaster()
			{
				var baseExceptionReporter = new BaseExceptionReporter();
				var errorReporterMock = new Mock<IErrorReporter>();

				// Arrange
				var errorMessage = $"The database owner SID recorded in the master database differs from the database owner SID recorded in database '{Db.DatabaseName}'. You should correct this situation by resetting the owner of database using the ALTER AUTHORIZATION statement.";
				var sqlException = SqlExceptionBuilder.CreateSqlException(33009, errorMessage);

				if (Db.Connection.TryGetLock("AlterDbAuthorisation_App_Lock", TimeSpan.FromSeconds(1), out var applock, Db.DatabaseName))
				{
					using (applock)
					{
						// Act
						using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
						{
							baseExceptionReporter.HandleUnhandledException(sqlException);
						}

						// Assert
						CombineAssertions(() =>
						{
							errorReporterMock.Verify(errorReporter => errorReporter.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never());
							errorReporterMock.Verify(errorReporter => errorReporter.ReportDeveloperExceptionOrHandleSilently(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never());
						});
					}
				}
			}

			public void TestHandleOwnerSIDDiffersFromMasterReturnsTrue()
			{
				var topLevelExceptionHandler = new TopLevelExceptionHandler();

				// Arrange
				var errorMessage = $"The database owner SID recorded in the master database differs from the database owner SID recorded in database '{Db.DatabaseName}'. You should correct this situation by resetting the owner of database using the ALTER AUTHORIZATION statement.";
				var sqlException = SqlExceptionBuilder.CreateSqlException(33009, errorMessage);

				if (Db.Connection.TryGetLock("AlterDbAuthorisation_App_Lock", TimeSpan.FromSeconds(1), out var applock, Db.DatabaseName))
				{
					using (applock)
					{
						// Act
						var handled = topLevelExceptionHandler.HandleSpecificExceptions(sqlException);

						// Assert
						AssertEquals(true, handled);
					}
				}
			}

			public void TestHandleDatabaseInEmergencyModeOrDamagedReturnsTrue()
			{
				var topLevelExceptionHandler = new TopLevelExceptionHandler();

				// Arrange
				var errorMessage = $"Could not run BEGIN TRANSACTION in database '{Db.DatabaseName}' because the database is in emergency mode or is damaged and must be restarted.";
				var sqlException = SqlExceptionBuilder.CreateSqlException(3908, errorMessage);

				if (Db.Connection.TryGetLock("AlterDbAuthorisation_App_Lock", TimeSpan.FromSeconds(1), out var applock, Db.DatabaseName))
				{
					using (applock)
					{
						// Act
						var handled = topLevelExceptionHandler.HandleSpecificExceptions(sqlException);

						// Assert
						AssertEquals(true, handled);
					}
				}
			}
		}

		#region TestExtraExceptionHandlers

		public void TestExtraExceptionHandlers()
		{
			var ex1 = new TestException("don't handle");
			var ex2 = new TestException("handle");

			var exceptionHandler = new TestExceptionHandler();

			var topLevelHandler = new TopLevelExceptionHandler();

			Assert("Should not handle yet", !topLevelHandler.HandleSpecificExceptions(ex1));
			Assert("Should not handle yet", !topLevelHandler.HandleSpecificExceptions(ex2));

			((ArrayList)ObjectFactory.Get(TopLevelExceptionHandler.ExtraExceptionHandlersListName)).Add(exceptionHandler);
			try
			{
				Assert("Should not handle", !topLevelHandler.HandleSpecificExceptions(ex1));
				Assert("Should handle supported exception", topLevelHandler.HandleSpecificExceptions(ex2));
			}
			finally
			{
				((ArrayList)ObjectFactory.Get(TopLevelExceptionHandler.ExtraExceptionHandlersListName)).Add(exceptionHandler);
			}
		}

		[Serializable]
		class TestException : Exception
		{
#if NETFRAMEWORK
			protected TestException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif

			public TestException(string errorMessage)
				: base(errorMessage)
			{
			}

			public TestException(string errorMessage, Exception innerException)
				: base(errorMessage, innerException)
			{
			}
		}

		class TestExceptionHandler : IExtraExceptionHandler
		{
			public bool HandleException(Exception exceptionToHandle)
			{
				return exceptionToHandle.Message.Equals("handle", StringComparison.Ordinal);
			}
		}

		#endregion

		[Serializable]
		class CriticalExceptionForTest : Exception, ICriticalException
		{
			public CriticalExceptionForTest()
			{
			}

			public CriticalExceptionForTest(string message, Exception innerException)
				: base(message, innerException)
			{
			}

#if NETFRAMEWORK
			protected CriticalExceptionForTest(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
			{
			}
#endif

			public bool IsCriticalException => true;
		}
	}
}
