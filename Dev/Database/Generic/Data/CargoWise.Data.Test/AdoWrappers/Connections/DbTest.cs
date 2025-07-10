using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DbTest : TestCase
	{
		public void TestDbUpgradeIsInProgress()
		{
			using (((IDbUpgradeSupport)Db.Instance).SetUpgradeWorkingInProgress())
			{
				Assert("Database upgrade is in progress", Db.IsUpgradeWorkingInProgress);
			}

			Assert("Database upgrade process has completed", !Db.IsUpgradeWorkingInProgress);
		}

		public void TestNewExtraConnectionWithMainDbCredentials()
		{
			using (var cnx = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, Db.SqlMasterDb))
			using (var cmd = cnx.Command("SELECT SUSER_NAME()"))
			{
				AssertEquals("Connection current database", Db.SqlMasterDb, cnx.CurrentDatabase);
				AssertEquals(
					"Should use main DB specific login",
					RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName).ToUpperInvariant(), cmd.ExecuteScalar().ToString().ToUpperInvariant());
			}
		}

		public void TestNewExtraConnectionWithTargetDatabaseSpecificCredentials()
		{
			using (var cnx = Db.NewExtraConnectionWithTargetDatabaseSpecificCredentials(Db.ServerName, Db.DatabaseName))
			using (var cmd = cnx.Command("SELECT SUSER_NAME()"))
			{
				AssertEquals("Connection current database", Db.DatabaseName, cnx.CurrentDatabase);
				AssertEquals(
					"Should use connection initial DB specific login",
					RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName).ToUpperInvariant(), cmd.ExecuteScalar().ToString().ToUpperInvariant());
			}
		}

		[ExpectNoExceptions]
		public void TestDbConnectionReportsNoDisposableActionErrorOnceForSameCallStack()
		{
			// Arrange
			var errorRepoterMock = new Mock<IErrorReporter>();
			using (ErrorReporter.SetTemporaryInstanceForTest(errorRepoterMock.Object))
			{
				Task
					.Run(DbPoker.PokeDbConnection)
					.Wait();

				// Act
				Task
					.Run(DbPoker.PokeDbConnection)
					.Wait();
			}

			// Assert
			errorRepoterMock.Verify(x => x.ReportDeveloperExceptionOrHandleSilently(
				It.IsAny<string>(),
				It.Is<string>(z => z == ThreadStaticConnectionFactory.AttemptToUseConnectionWithoutDisposableAction),
				It.Is<InvalidOperationException>(u => u.Message == ThreadStaticConnectionFactory.AttemptToUseConnectionWithoutDisposableAction)), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestDbConnectionReportsNoDisposableActionErrorForDifferentCallStacks()
		{
			// Arrange
			var errorRepoterMock = new Mock<IErrorReporter>();
			using (ErrorReporter.SetTemporaryInstanceForTest(errorRepoterMock.Object))
			{
				var t = new Thread(DbPoker.PokeDbConnection);
				t.Start();
				t.Join();

				// Act
				Task
					.Run(() => DbPoker.PokeDbConnection())
					.Wait();
			}

			// Assert
			errorRepoterMock.Verify(x => x.ReportDeveloperExceptionOrHandleSilently(
				It.IsAny<string>(),
				It.Is<string>(z => z == ThreadStaticConnectionFactory.AttemptToUseConnectionWithoutDisposableAction),
				It.Is<InvalidOperationException>(u => u.Message == ThreadStaticConnectionFactory.AttemptToUseConnectionWithoutDisposableAction)), Times.Exactly(2));
		}

		public void TestNewAdminConnection_SelectsRequestedDb()
		{
			// Arrange
			const string databaseName = "tempdb";
			using (var adminConnection = Db.NewAdminConnection(databaseName))
			{
				// Act
				var result = adminConnection.ExecuteScalar<string>("SELECT DB_NAME()");

				// Assert
				AssertEquals(databaseName, result);
			}
		}

		public void TestErrorReportInGetDisposableExtraConnectionCannotCauseDeadlock()
		{
			var timeoutSpan = TimeSpan.FromSeconds(30);
			var lockObj = new object();
			using (var firstLockEvent = new AutoResetEvent(false))
			using (var secondLockEvent = new AutoResetEvent(false))
			using (ErrorReporter.SetTemporaryInstanceForTest(new DeadlockTestErrorReporter(lockObj, firstLockEvent, secondLockEvent, timeoutSpan)))
			{
				var taskThatDoesNotCleanupExtraDbConnection = new Task(() =>
				{
					Db.ResetAlreadyReported_ForTest();
					var connection = Db.Connection;
				});
				var taskThatLocksFirst = new Task(() =>
				{
					lock (lockObj)
					{
						secondLockEvent.Set();
						Assert("First lock event did not fire", firstLockEvent.WaitOne(timeoutSpan));
						using (Db.DisposableActionForDbConnection())
						{
							var connection = Db.Connection;
						}
					}
				});

				taskThatDoesNotCleanupExtraDbConnection.Start();
				taskThatLocksFirst.Start();

				Assert("TaskThatDoesNotCleanupExtraDbConnection did not complete, perhaps a deadlock occured?", taskThatDoesNotCleanupExtraDbConnection.Wait(timeoutSpan));
				Assert("TaskThatLocksFirst did not complete, perhaps a deadlock occured?", taskThatLocksFirst.Wait(timeoutSpan));
			}
		}

		public void TestDisposableActionForDbConnection_ReportThreadSentryOwnerError_DoesNotProduceDeadLock()
		{
			// Arrange
			var threadSentryIsLockedEvent = new ManualResetEvent(false);
			var dbLockObjIsLockedEvent = new ManualResetEvent(false);
			var errorReporterMock = new Mock<IErrorReporter>();

			errorReporterMock
				.Setup(reporter => reporter.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()))
				.Callback(() =>
				{
					threadSentryIsLockedEvent.Set();
					using (Db.DisposableActionForDbConnection()) // deadlock point - wait for the Db.lockObj to be released by thread[1]
					{
						// if the other thread does NOT have to wait for the threadSentry lock
						// acquired by ThreadSentry.EnsureCurrentThreadIsOwner() call,
						// it would finish it's work and release the lockObj in no time,
						// and then the Db.DisposableActionForDbConnection() wouldn't have to wait forever.
					}
				});

			// Act
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				var factory = new ThreadStaticConnectionFactory<DbConnection>(new ConnectionFactoryAction<DbConnection>(() => Db.NewExtraConnectionToMainDb()));

				DbConnection extraDbConnection = null;
				var threadTakingLockToThreadSentry = new Thread(() =>
				{
					dbLockObjIsLockedEvent.WaitOne(); // lockObj was acquired by the other thread
					AssertNotNull(extraDbConnection);

					((ThreadSentry)extraDbConnection.ThreadSentry).EnsureCurrentThreadIsOwner(); // thread 0 has now acquired the lock to the thread sentry
				});

				var threadTakingLockToErrorReporter = new Thread(() =>
				{
					using (factory.BeginThreadScopeAndGiveDisposer(out var disposer))
					{
						AssertNotNull(disposer);

						disposer.OnDispose += OnDispose;
						disposer.DbConnection.ExecuteNonQuery("SELECT 1 WHERE 1 = 1;");
					}

					void OnDispose(DbConnection dbConnection)
					{
						AssertNotNull(dbConnection); // thread 1 acquired the lock to the Db.lockObj from the call DisposableActionForDbConnection
						Assert(ReferenceEquals(dbConnection, factory.ProvideConnection()));

						extraDbConnection = dbConnection;
						extraDbConnection.ThreadSentry.RelinquishThreadOwnership();

						dbLockObjIsLockedEvent.Set();
						threadSentryIsLockedEvent.WaitOne();

						// deadlock point - ThreadSentry Dispose will wait for thread sentry lock to be released by thread[0]
						// using (((ThreadSentry)dbConnection.ThreadSentry).ForcefullyBorrowThreadOwnership())
					}
				});

				var threads = new[] { threadTakingLockToThreadSentry, threadTakingLockToErrorReporter };
				threads.ForEach(t => t.Start());
				threads.ForEach(t => t.Join());

				// Assert
				Assert("All threads have exited with no deadlock", threads.All(t => !t.IsAlive));
			}
		}

		class DeadlockTestErrorReporter : IErrorReporter
		{
			public DeadlockTestErrorReporter(object lockObj, AutoResetEvent firstLockEvent, AutoResetEvent secondLockEvent, TimeSpan timeoutSpan)
			{
				this.lockObj = lockObj;
				this.firstLockEvent = firstLockEvent;
				this.secondLockEvent = secondLockEvent;
				this.timeoutSpan = timeoutSpan;
			}

			public void Clear()
			{
			}

			public void Report(string key, string message, Exception exception)
			{
			}

			public void ReportDeveloperExceptionOrHandleSilently(string key, string message, Exception ex)
			{
				firstLockEvent.Set();
				Assert("Second lock event did not fire", secondLockEvent.WaitOne(timeoutSpan));
				lock (lockObj)
				{
				}
			}

			readonly object lockObj;
			readonly AutoResetEvent firstLockEvent;
			readonly AutoResetEvent secondLockEvent;
			readonly TimeSpan timeoutSpan;
		}

		public void TestGetAllLoginNames()
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			var random = new Random();
			var dbName = new string(Enumerable.Repeat(chars, 35).Select(s => s[random.Next(s.Length)]).ToArray());

			var loginNames = Db.GetAllLoginNames(dbName);

			AssertEquals(loginNames.Count(), 5);

			AssertEquals(loginNames.ElementAt(0), dbName + "_CargoWiseReaderLogin");
			AssertEquals(loginNames.ElementAt(1), dbName + "_CargoWiseWriterLogin");
			AssertEquals(loginNames.ElementAt(2), dbName + "_RestrictedReaderLogin");
			AssertEquals(loginNames.ElementAt(3), dbName + "_RestrictedWriterLogin");
			AssertEquals(loginNames.ElementAt(4), dbName + "_UnrestrictedWriterLogin");
		}

		public void TestDisableSchemaVersionCheck()
		{
			Db.EnableThreadSchemaVersionCheckPermanently_ForTest();
			Db.EnableSchemaVersionCheckPermanently_ForTest();

			AssertEquals("PRE", false, Db.IsSchemaVersionCheckDisabled);
			AssertEquals("PRE", false, Db.IsGlobalSchemaVersionCheckDisabled);
			AssertEquals("PRE", false, Db.IsThreadSchemaVersionCheckDisabled);

			using (Db.DisableSchemaVersionCheck())
			{
				AssertEquals(true, Db.IsSchemaVersionCheckDisabled);
				AssertEquals(true, Db.IsGlobalSchemaVersionCheckDisabled);
				AssertEquals(false, Db.IsThreadSchemaVersionCheckDisabled);
			}

			AssertEquals(false, Db.IsSchemaVersionCheckDisabled);
			AssertEquals(false, Db.IsGlobalSchemaVersionCheckDisabled);
			AssertEquals(false, Db.IsThreadSchemaVersionCheckDisabled);
		}

		public void TestDisableThreadSchemaVersionCheckPermanently()
		{
			Db.EnableThreadSchemaVersionCheckPermanently_ForTest();
			Db.EnableSchemaVersionCheckPermanently_ForTest();

			AssertEquals("PRE", false, Db.IsSchemaVersionCheckDisabled);
			AssertEquals("PRE", false, Db.IsGlobalSchemaVersionCheckDisabled);
			AssertEquals("PRE", false, Db.IsThreadSchemaVersionCheckDisabled);

			try
			{
				Db.DisableThreadSchemaVersionCheckPermanently();

				AssertEquals(true, Db.IsSchemaVersionCheckDisabled);
				AssertEquals(false, Db.IsGlobalSchemaVersionCheckDisabled);
				AssertEquals(true, Db.IsThreadSchemaVersionCheckDisabled);
			}
			finally
			{
				Db.EnableThreadSchemaVersionCheckPermanently_ForTest();
			}
		}

		public void TestNewExtraConnectionToMainDbWithReaderCredentialsReturnsInstanceTypeOfMainDbRestrictedReaderLoginExtraConnection()
		{
			// Arrange
			// Act
			using (var connection = Db.NewExtraConnectionToMainDbWithReaderCredentials("balabala"))
			{
				// Assert
				AssertType<RestrictedReaderConnection>(connection);
				AssertEquals("CargoWiseOnebalabala", connection.ExecuteScalar("Select APP_NAME()"));
				AssertEquals(Db.ServerName, connection.ServerName);
				AssertEquals(Db.DatabaseName, connection.CurrentDatabase);
				AssertEquals(RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName), connection.UserLogin);
			}
		}

		public void TestNewExtraRestrictedReaderConnectionWithRestrictedReaderLoginCredentials()
		{
			// Arrange
			// Act
			using (var connection = Db.NewExtraRestrictedReaderConnection(Db.ServerName, Db.DatabaseName, "balabala"))
			{
				// Assert
				AssertType<RestrictedReaderConnection>(connection);
				AssertEquals("CargoWiseOnebalabala", connection.ExecuteScalar("Select APP_NAME()"));
				AssertEquals(RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName), connection.UserLogin);
			}
		}

		public void TestNewExtraRestrictedWriterConnectionWithRestrictedWriterLoginCredentials()
		{
			// Arrange
			// Act
			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, Db.DatabaseName, "balabala"))
			{
				// Assert
				AssertType<RestrictedWriterConnection>(connection);
				AssertEquals("CargoWiseOnebalabala", connection.ExecuteScalar("Select APP_NAME()"));
				AssertEquals(RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), connection.UserLogin);
			}
		}

		public void TestNewExtraUnrestrictedWriterConnectionWithUnrestrictedWriterLoginCredentials()
		{
			// Arrange
			// Act
			using (var connection = Db.NewExtraUnrestrictedWriterConnection(Db.ServerName, Db.DatabaseName, "balabala"))
			{
				// Assert
				AssertType<UnrestrictedWriterConnection>(connection);
				AssertEquals("CargoWiseOnebalabala", connection.ExecuteScalar("Select APP_NAME()"));
				AssertEquals(UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), connection.UserLogin);
			}
		}

		public void TestInitializeDatabaseDetails()
		{
			TestDefault();
			TestParameterized(ApplicationType.Default, "LocalFileKnownGoodProtectedDataCache");
			TestParameterized(ApplicationType.Web, "WebKnownGoodProtectedDataCache");

			void TestDefault()
			{
				AssertType("LocalFileKnownGoodProtectedDataCache");
			}

			void TestParameterized(ApplicationType applicationType, string expectedTypeName)
			{
				// Arrange
				var serverName = Db.ServerName;
				var databaseName = Db.DatabaseName;

				using (Db.ClearServerDetailsTemporarily())
				{
					// Act
					Db.InitializeDatabaseDetails(serverName, databaseName, applicationType);

					// Assert
					AssertType(expectedTypeName);
				}
			}

			void AssertType(string typeName)
			{
				AssertEquals(
					ProtectedDataService.GlobalServiceProvider.GetRequiredService<IKnownGoodProtectedDataCache>().GetType().Name,
					typeName);
			}
		}

		public void TestDisposerCannotBeMissingFromMainThread()
		{
			// Arrange
			using (var disposer = Db.DisposableActionForDbConnection())
			{
				AssertNull("Disposer is null", disposer);
			}

			// Act
			var disposerIsMissing = Db.IsDbConnectionDisposerMissing();

			// Assert
			AssertEquals("Disposer can't be missing in main thread", false, disposerIsMissing);
		}

		[UseSnapshotProtection]
		public void TestIsDatabaseUpgradedIsResetWhenBinaryVersionAgreesWithSchemaVersion()
		{
			try
			{
				// Arrange
				using (Db.DisposableUpgrade_ForTest(acquireLockOut: false, resetDatabaseUpgradedForTest: false))
				{
					AssertExceptionThrown<DatabaseUpgradedException>(() => ((IDbReconnectionHandling)Db.Connection).CloseAndReopenConnection());
					Assert("Db.IsDatabaseUpgraded is true", Db.IsDatabaseUpgraded);

					var task1 = Task.Run((() =>
					{
						Assert("Db.IsDatabaseUpgraded is true", Db.IsDatabaseUpgraded);
					}));
					task1.Wait();
				}

				// Act
				((IDbReconnectionHandling)Db.Connection).CloseAndReopenConnection();

				// Assert
				Assert("Db.IsDatabaseUpgraded is reset after db upgrade has completed", !Db.IsDatabaseUpgraded);

				var task2 = Task.Run((() =>
				{
					Assert("Db.IsDatabaseUpgraded is false", !Db.IsDatabaseUpgraded);
				}));
				task2.Wait();
			}
			finally
			{
				Db.Connection.ResetDatabaseUpgradedExceptionHasBeenThrown();
				Db.ResetDatabaseUpgraded_ForTest();
			}
		}

		[UseSnapshotProtection]
		public void TestIsDatabaseUpgradedIsTrueWhenDatabaseUpgradedExceptionThrowFirstTimeInMultipleThreads()
		{
			try
			{
				// Arrange
				using (Db.DisposableUpgrade_ForTest(acquireLockOut: false, resetDatabaseUpgradedForTest: true))
				{
					// Act
					AssertExceptionThrown<DatabaseUpgradedException>(() => ((IDbReconnectionHandling)Db.Connection).CloseAndReopenConnection());

					// Assert
					Assert("Db.IsDatabaseUpgraded is true", Db.IsDatabaseUpgraded);

					var task1 = Task.Run((() =>
					{
						Assert("Db.IsDatabaseUpgraded is true", Db.IsDatabaseUpgraded);
					}));
					task1.Wait();
				}

				// Assert because resetDatabaseUpgradedForTest: true from IDisposable as above
				Assert("Db.IsDatabaseUpgraded is false", !Db.IsDatabaseUpgraded);
			}
			finally
			{
				Db.Connection.ResetDatabaseUpgradedExceptionHasBeenThrown();
				Db.ResetDatabaseUpgraded_ForTest();
			}
		}

		[UseSnapshotProtection]
		public void TestIsDatabaseUpgradedIsFalseWhenDatabaseUpgradeInProgressExceptionBeenThrown()
		{
			// Arrange
			using (Db.DisposableUpgrade_ForTest(acquireLockOut: true, resetDatabaseUpgradedForTest: true))
			{
				// Act
				AssertExceptionThrown<DatabaseUpgradeInProgressException>(() => ((IDbReconnectionHandling)Db.Connection).CloseAndReopenConnection());

				// Assert
				Assert("Db.IsDatabaseUpgraded is false", !Db.IsDatabaseUpgraded);
			}
		}

		[UseSnapshotProtection]
		public void TestDisableSchemaVersionCheckOnCurrentThread()
		{
			// Arrange
			using (Db.DisposableActionForDbConnection())
			{
				var schemaVersion = DbRegistry.DatabaseMajorSchemaVersion.LoadValue(Db.Connection);
				using (new DisposableAction(() => DbRegistry.DatabaseMajorSchemaVersion.SaveValue(schemaVersion, Db.Connection)))
				{
					DbRegistry.DatabaseMajorSchemaVersion.SaveValue(schemaVersion + 1, Db.Connection);
					var noSchemaVersionCheckTask = Task.Run(() =>
					{
						using (Db.DisposableActionForDbConnection())
						using (Db.DisableSchemaVersionCheckOnCurrentThread())
						{
							Db.Connection.EnsureIsOpen();
						}
					});

					// Act
					// Assert
					AssertNoExceptionThrown(() => noSchemaVersionCheckTask.Wait());
				}
			}
		}

		static class DbPoker
		{
			public static void PokeDbConnection()
			{
				Db.Connection.ExecuteNonQuery(@"SELECT @@SPID");
				Db.ResetAlreadyReported_ForTest();
				Db.DisposeThreadConnection();
			}
		}
	}
}
