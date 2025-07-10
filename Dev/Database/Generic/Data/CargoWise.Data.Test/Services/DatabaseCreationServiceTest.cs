using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data.Services;
using CargoWise.Database.Abstractions;
using CargoWise.IO;
using Moq;
using NUnit.Framework;

namespace CargoWise.Data.Testing.Services
{
	sealed class DatabaseCreationServiceTest : TestCase
	{
		DatabaseCreationService Service { get; } = new DatabaseCreationService();

		public void TestCreateDatabase_NotCreated_WhenFailedToAcquireDatabaseLock()
		{
			var dbName = Guid.NewGuid().ToString("N");
			var sqlLockTimeout = TimeSpan.FromSeconds(0);

			using (AdoTestUtils.DropDbIfExistsDisposable(testConnection, dbName))
			using (var extraConnection = Db.NewAdminConnection())
			using (Service.AcquireSqlAppLock_Exposed(extraConnection, dbName, sqlLockTimeout))
			{
				AssertExceptionThrown<CreateDatabaseException>(() =>
				{
					Service.CreateDatabase(testConnection, dbName, sqlLockTimeout: sqlLockTimeout);
				});

				AssertEquals(false, testConnection.DatabaseExists(dbName));
			}
		}

		public void TestCreateDatabase_Created_WhenLockAcquiredOnDifferentDatabase()
		{
			var dbName = Guid.NewGuid().ToString("N");
			var dbName2 = Guid.NewGuid().ToString("N");
			var sqlLockTimeout = TimeSpan.FromSeconds(0);

			using (AdoTestUtils.DropDbIfExistsDisposable(testConnection, dbName))
			using (var extraConnection = Db.NewAdminConnection())
			using (Service.AcquireSqlAppLock_Exposed(extraConnection, dbName2, sqlLockTimeout))
			{
				AssertNoExceptionThrown(() => { Service.CreateDatabase(testConnection, dbName, sqlLockTimeout: sqlLockTimeout); });
				AssertEquals(true, testConnection.DatabaseExists(dbName));
			}
		}

		public void TestCreateDatabase()
		{
			AssertCreateDatabase((conn, dbName, dataPath, logPath) =>
				Service.CreateDatabase(conn, dbName, dataPath, logPath)
			);
		}

		public void TestCreateDatabase_ModifyDatabaseName_WithRetry()
		{
			using (AdoTestUtils.DropDbIfExistsDisposable(TestDbName))
			using (testConnection.PrepareRetryContextForTest(
				condition: (sqlText, executionCount) => sqlText.Contains(" MODIFY NAME "),
				action: (sqlText, executionCount) =>
				{
					if (executionCount < 2)
					{
						throw SqlExceptionBuilder.CreateSqlException(1222, "Lock request timeout exceeded");
					}
				}))
			{
				Service.CreateDatabase(testConnection, TestDbName);

				AssertEquals(2, testConnection.RetryContext_ForTest.ExecutionCount);
			}
		}

		public void TestCreateDatabaseSqlServerRaceConditionInModifyDatabaseNameWithRetry()
		{
			// Arrange
			string testDb = "DBUPG_NewTemplateDB_MockDbForDbConnectionTest";
			string tempTestDb = DatabaseCreationService.TempCreateDbNamePrefix + testDb;
			string exceptionMsg = "Lock request time out period exceeded.\r\nThe database name '" + testDb + "' has been set.\r\nThe statement has been terminated.";

			using (AdoTestUtils.DropDbIfExistsDisposable(testDb))
			using (AdoTestUtils.DropDbIfExistsDisposable(tempTestDb))
			using (testConnection.PrepareRetryContextForTest(condition: (sqlText, executionCount) => sqlText.Contains(" MODIFY NAME "),
					action: (sqlText, executionCount) =>
					{
						AdoTestUtils.DropDbIfExists(testConnection, tempTestDb);
						AdoTestUtils.CreateDbIfNotExists(testConnection, testDb, Db.DatabaseName);
						throw SqlExceptionBuilder.CreateSqlException(1222, exceptionMsg);
					}))
			{
				// Act & Assert
				AssertNoExceptionThrown(() => Service.CreateDatabase(testConnection, testDb));

				AssertEquals(1, testConnection.RetryContext_ForTest.ExecutionCount);
				Assert(!Db.Connection.DatabaseExists(tempTestDb));
				Assert(Db.Connection.DatabaseExists(testDb));
			}
		}

		void AssertCreateDatabase(Action<AdminConnection, string, string, string> createDbAction)
		{
			string testPath = TempForTest.TempPath;

			AdoTestUtils.DropDbIfExists(testConnection, TestDbName);
			createDbAction(testConnection, TestDbName, "", testPath);
			AssertDbPhysicalFilePathAndCollation(testConnection, TestDbName, GetExpectedPath(testConnection, DbConnection.DatabaseFileTypes.Data), testPath);

			AdoTestUtils.DropDbIfExists(testConnection, TestDbName);
			createDbAction(testConnection, TestDbName, testPath, "");
			AssertDbPhysicalFilePathAndCollation(testConnection, TestDbName, testPath, GetExpectedPath(testConnection, DbConnection.DatabaseFileTypes.Log));

			// Handles database already exists error
			AssertNoExceptionThrown(() => createDbAction(testConnection, TestDbName, null, null));
			// Paths are as when the database was actually created
			AssertDbPhysicalFilePathAndCollation(testConnection, TestDbName, testPath, GetExpectedPath(testConnection, DbConnection.DatabaseFileTypes.Log));
		}

		public void TestCreateDatabaseWithDbSpecificLogins()
		{
			string testPath = TempForTest.TempPath;

			((IDbLoginRepair)testConnection).EnsureRestrictedWriterDbLogin();

			AdoTestUtils.DropDbIfExists(testConnection, TestDbName);
			Service.CreateDatabase(testConnection, TestDbName, "", testPath);

			DatabaseLoginTest.AssertUserIsMemberOfRole(testConnection, TestDbName, ((IDbLoginRepair)testConnection).RestrictedWriterDbLoginName, "cwRestrictedWriterRole");
			AdminConnectionNonTransactionalTest.AssertAllLoginsAreMappedToDatabase(testConnection, TestDbName);
		}

		public void TestCreateDatabaseWithInitialSizeAndGrowth()
		{
			string testPath = TempForTest.TempPath;

			int modelDataFilePages = GetModelDbDataFileSize(testConnection);
			int firstTestInitialSizeMb = (modelDataFilePages / 128) + 1;

			// Create database defining the initial size and growth
			AdoTestUtils.DropDbIfExists(testConnection, TestDbName);
			Service.CreateDatabase(testConnection, TestDbName, testPath, testPath, firstTestInitialSizeMb, dataGrowthMb: 123);
			AssertDbDataFileSizeAndGrowth(testConnection, TestDbName, (firstTestInitialSizeMb * 128), (123 * 128));
			AssertDbPhysicalFilePathAndCollation(testConnection, TestDbName, testPath, testPath);

			// Create database using the model DB size and specifying the file growth
			AdoTestUtils.DropDbIfExists(testConnection, TestDbName);
			Service.CreateDatabase(testConnection, TestDbName, testPath, testPath, dataGrowthMb: 75);
			AssertDbDataFileSizeAndGrowth(testConnection, TestDbName, modelDataFilePages, (75 * 128));

			// Create database defining the initial size
			AdoTestUtils.DropDbIfExists(testConnection, TestDbName);
			int secondTestInitialSizeMb = firstTestInitialSizeMb + 2;
			Service.CreateDatabase(testConnection, TestDbName, testPath, testPath, secondTestInitialSizeMb);
			AssertDbDataFileSizeAndGrowth(testConnection, TestDbName, (secondTestInitialSizeMb * 128), null);

			// Create database using default DB size and growth
			AdoTestUtils.DropDbIfExists(testConnection, TestDbName);
			Service.CreateDatabase(testConnection, TestDbName, testPath, testPath);
			AssertDbDataFileSizeAndGrowth(testConnection, TestDbName, modelDataFilePages, null);

			if (modelDataFilePages > 128)
			{
				// Create database specifying a size smaller than the model DB, should handle the SQL exception
				AdoTestUtils.DropDbIfExists(testConnection, TestDbName);
				Service.CreateDatabase(testConnection, TestDbName, testPath, testPath, 1);
				AssertDbDataFileSizeAndGrowth(testConnection, TestDbName, modelDataFilePages, null);
			}

			// Create database defining the initial size and growth for the log file
			AdoTestUtils.DropDbIfExists(testConnection, TestDbName);
			Service.CreateDatabase(testConnection, TestDbName, testPath, testPath, logInitialSizeMb: 4, logGrowthMb: 4);
			AssertDbDataFileSizeAndGrowth(testConnection, TestDbName, (4 * 128), (4 * 128), 1);
		}

		public void TestCreateDatabaseAdjustsRecoveryModel()
		{
			// Arrange
			var testPath = TempForTest.TempPath;

			var mockServiceProvider = new Mock<IServiceProvider>();
			var originalServiceProvider = GlobalServiceProvider.Instance;
			var mockDbRecoveryModelManager = new Mock<IDbRecoveryModelManagerInternals>();
			mockDbRecoveryModelManager
				.Setup(m => m.AdjustDatabase(It.IsAny<AdminConnection>(), It.IsAny<string>(), TestDbName, It.IsAny<string>()))
				.Callback(() =>
				{
					AssertEquals("AdjustDatabase should be called before database is renamed to the actual name", false, Db.Connection.DatabaseExists(TestDbName));
				})
				.Verifiable();

			mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns<Type>(t => originalServiceProvider.GetService(t));
			mockServiceProvider.Setup(x => x.GetService(typeof(IDbRecoveryModelManager))).Returns(mockDbRecoveryModelManager.Object);

			using (GlobalServiceProvider.Configure(mockServiceProvider.Object))
			{
				AdoTestUtils.DropDbIfExists(testConnection, TestDbName);

				// Act
				Service.CreateDatabase(testConnection, TestDbName, testPath, testPath);
			}

			// Assert
			mockDbRecoveryModelManager.VerifyAll();
			AssertEquals("DatabaseExists", true, Db.Connection.DatabaseExists(TestDbName));
		}

		public void TestCreatesNonExistingMainDatabase()
		{
			// Arrange
			TestingState.IsRunningTests = false;
			var initialDatabaseName = Db.DatabaseName;
			var fDatabaseName =
				typeof(Db).GetField("fDatabaseName", BindingFlags.Static | BindingFlags.NonPublic)
				?? throw new Exception("Cannot find Db.fDatabaseName field");

			AdoTestUtils.DropDbIfExists(TestDbName);
			var testPath = TempForTest.TempPath;

			fDatabaseName.SetValue(null, TestDbName);
			using (new DisposableAction(() => fDatabaseName.SetValue(null, initialDatabaseName)))
			using (new DisposableAction(() => TestingState.IsRunningTests = true))
			{
				// Act
				Service.CreateDatabase(testConnection, TestDbName, testPath, testPath);

				// Assert
				AssertEquals("DatabaseExists", true, Db.Connection.DatabaseExists(TestDbName));
			}
		}

		public void TestCreateDatabaseAfterFailedCreate()
		{
			Service.CreateDatabase(testConnection, DatabaseCreationService.TempCreateDbNamePrefix + TestDbName);
			Service.CreateDatabase(testConnection, TestDbName);

			Assert(!testConnection.DatabaseExists(DatabaseCreationService.TempCreateDbNamePrefix + TestDbName));
			Assert(testConnection.DatabaseExists(TestDbName));
		}

		public void TestConnectionDroppedDuringAction_DbIsSuccessfullyDropped()
		{
			AssertExceptionThrown(typeof(Utils.SqlLockLostException), () =>
			{
				Service.CreateDatabase(testConnection, TestDbName, actionToPerformAfterCreatingDb: (db) =>
				{
					testConnection.CloseConnection();
				});
			});

			Assert(!testConnection.DatabaseExists(DatabaseCreationService.TempCreateDbNamePrefix + TestDbName));
			Assert(!testConnection.DatabaseExists(TestDbName));
		}

		public void TestActionIsRunDuringCreateDatabase()
		{
			Service.CreateDatabase(testConnection, TestDbName, actionToPerformAfterCreatingDb: (db) =>
			{
				Assert(testConnection.DatabaseExists(DatabaseCreationService.TempCreateDbNamePrefix + TestDbName));
				Assert(!testConnection.DatabaseExists(TestDbName));
			});

			Assert(!testConnection.DatabaseExists(DatabaseCreationService.TempCreateDbNamePrefix + TestDbName));
			Assert(testConnection.DatabaseExists(TestDbName));
		}

		public void TestErrorDuringAfterCreateActionRemovesTempDatabase()
		{
			try
			{
				Service.CreateDatabase(testConnection, TestDbName, actionToPerformAfterCreatingDb: _ => throw new Exception());
				Fail("Should have thrown an exception");
			}
			catch (Exception)
			{
				Assert(!testConnection.DatabaseExists(DatabaseCreationService.TempCreateDbNamePrefix + TestDbName));
				Assert(!testConnection.DatabaseExists(TestDbName));
			}
		}

		public void TestCreateDBWithCannotGetExclusiveLockException()
		{
			using (var newConnection = Db.NewAdminConnection())
			{
				AssertNoExceptionThrown(() =>
				{
					Service.CreateDatabase(testConnection, TestDbName, mapDbLogins: false, actionToPerformAfterCreatingDb: _ =>
					{
						((ICurrentDbControl)newConnection).UseDatabase(DatabaseCreationService.TempCreateDbNamePrefix + TestDbName);
					});
				});
			}
		}

		[SnailTest]
		public void TestCreateDatabaseCannotObtainExclusiveLockOnModelWithException()
		{
			TestCreateDatabaseCannotObtainExclusiveLockOnModel(-1, () =>
			{
				// Arrange
				var stopwatch = new Stopwatch();
				stopwatch.Start();

				// Act
				var sqlException = AssertExceptionThrown<SqlException>(() => Service.CreateDatabase(testConnection, TestDbName));

				// Assert
				stopwatch.Stop();
				Assert(new DbErrorMatch(sqlException).ExceptionType == DbErrorType.CouldNotObtainExclusiveLock);
				AssertGreaterThanOrEqualTo(stopwatch.Elapsed, TimeSpan.FromMinutes(1.5));
			});
		}

		[SnailTest]
		public void TestCreateDatabaseCannotObtainExclusiveLockOnModelWithoutException()
		{
			TestCreateDatabaseCannotObtainExclusiveLockOnModel(3000, () =>
			{
				AssertNoExceptionThrown(() => Service.CreateDatabase(testConnection, TestDbName));
			});
		}

		void TestCreateDatabaseCannotObtainExclusiveLockOnModel(int timeout, Action assertAction)
		{
			var taskStartEvent = new AutoResetEvent(false);
			var dbCreatedEvent = new AutoResetEvent(false);
			var action = new Action<object>(t =>
			{
				using (var newConnection = Db.NewAdminConnection())
				{
					using (((ICurrentDbControl)newConnection).UseDatabase("model"))
					{
						taskStartEvent.Set();
						dbCreatedEvent.WaitOne((int)t);
					}
				}
			});

			new Task(action, timeout).Start();
			taskStartEvent.WaitOne();
			try
			{
				assertAction();
			}
			finally
			{
				dbCreatedEvent.Set();
			}
		}

		public void TestCreateDatabaseMainDbExists()
		{
			var mainDbName = "DBUPG_OdysseyTest";
			using (var tempDir = new TempDirectory())
			using (var adminConnection = AdminConnection.New(Db.ServerName, Db.SqlMasterDb))
			{
				try
				{
					Service.CreateDatabase(adminConnection, mainDbName, dataPath: tempDir.DirectoryName, logPath: tempDir.DirectoryName);
					using (Service.OverrideMainDatabaseNameTemporarily(mainDbName))
					{
						Service.CreateDatabase(adminConnection, TestDbName);

						Assert("TestDb does not exist.", adminConnection.DatabaseExists(TestDbName));

						AssertDbPhysicalFilePathAndCollation(adminConnection, TestDbName, tempDir.DirectoryName, tempDir.DirectoryName);
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(adminConnection, TestDbName);
					AdoTestUtils.DropDbIfExists(adminConnection, mainDbName);
				}
			}
		}

		public void TestCreateDatabaseMainDbDoesNotExist()
		{
			using (var adminConnection = AdminConnection.New(Db.ServerName, Db.SqlMasterDb))
			{
				using (Service.OverrideMainDatabaseNameTemporarily("DBUPG_OdysseyTest"))
				{
					Service.CreateDatabase(adminConnection, TestDbName);

					Assert("TestDb does not exist.", adminConnection.DatabaseExists(TestDbName));

					var expectedDataFilePath = adminConnection.ExecuteScalar("SELECT SERVERPROPERTY ('InstanceDefaultDataPath')").ToString();
					var expectedLogFilePath = adminConnection.ExecuteScalar("SELECT SERVERPROPERTY ('InstanceDefaultLogPath')").ToString();

					AssertDbPhysicalFilePathAndCollation(adminConnection, TestDbName, expectedDataFilePath, expectedLogFilePath);
				}
			}
		}

		public void TestCreateDatabaseMainDbIsOffline()
		{
			var mainDbName = "DBUPG_OdysseyTest";
			using (var tempDir = new TempDirectory())
			using (var adminConnection = AdminConnection.New(Db.ServerName, Db.SqlMasterDb))
			{
				try
				{
					Service.CreateDatabase(adminConnection, mainDbName, dataPath: tempDir.DirectoryName, logPath: tempDir.DirectoryName);
					adminConnection.ExecuteNonQuery($"ALTER DATABASE [{mainDbName}] SET OFFLINE");
					using (Service.OverrideMainDatabaseNameTemporarily(mainDbName))
					{
						var sqlText = $"SELECT state_desc FROM sys.databases WHERE name = '{mainDbName}'";
						Assert("DB should be offline.", string.Equals(adminConnection.ExecuteScalar(sqlText).ToString(), "OFFLINE", StringComparison.OrdinalIgnoreCase));

						Service.CreateDatabase(adminConnection, TestDbName);

						Assert("TestDb does not exist.", adminConnection.DatabaseExists(TestDbName));

						AssertDbPhysicalFilePathAndCollation(adminConnection, TestDbName, tempDir.DirectoryName, tempDir.DirectoryName);
					}
				}
				finally
				{
					adminConnection.ExecuteNonQuery($"ALTER DATABASE [{mainDbName}] SET ONLINE");
					AdoTestUtils.DropDbIfExists(adminConnection, TestDbName);
					AdoTestUtils.DropDbIfExists(adminConnection, mainDbName);
				}
			}
		}

		public void TestCreateDatabaseMainDbDoesNotExistConnectionToSystemDatabase()
		{
			using (var adminConnection = AdminConnection.New(Db.ServerName, Db.SqlMasterDb))
			{
				using (Service.OverrideMainDatabaseNameTemporarily("DBUPG_OdysseyTest"))
				{
					Service.CreateDatabase(adminConnection, TestDbName);

					Assert("TestDb does not exist.", adminConnection.DatabaseExists(TestDbName));

					var sqlText = "SELECT SERVERPROPERTY('InstanceDefaultDataPath') AS DataFilePath, SERVERPROPERTY('InstanceDefaultLogPath') AS LogFilePath";

					string expectedDataFilePath = null;
					string expectedLogFilePath = null;

					using (var cmd = adminConnection.Command(sqlText))
					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							expectedDataFilePath = reader["DataFilePath"].ToString();
							expectedLogFilePath = reader["LogFilePath"].ToString();
						}
						else
						{
							Fail("Failed to get InstanceDefaultDataPath and InstanceDefaultLogPath for server.");
						}
					}

					AssertDbPhysicalFilePathAndCollation(adminConnection, TestDbName, expectedDataFilePath, expectedLogFilePath);
				}
			}
		}

		public void TestLogins()
		{
			using (var adminConnection = AdminConnection.New(Db.ServerName, Db.SqlMasterDb))
			{
				AssertContainsExactElementsInAnyOrder(
					"All login types are in the Login list.",
					new[] {
						typeof(RestrictedReaderDatabaseLogin),
						typeof(RestrictedWriterDatabaseLogin),
						typeof(UnrestrictedWriterDatabaseLogin),
						typeof(ReaderDatabaseLogin),
						typeof(CargoWiseWriterLogin)
					},
					adminConnection.Logins.Select(l => l.GetType()));
			}
		}

		static void AssertDbPhysicalFilePathAndCollation(DbConnection conn, string dbName, string expectedDataPath, string expectedLogPath)
		{
			string sqlTextRaw = "SELECT physical_name FROM [{0}].sys.database_files WHERE type = {1};";

			string actualDataPath = conn.ExecuteScalar(string.Format(sqlTextRaw, dbName, "0")).ToString();
			AssertEquals("DATA FILE FULL PATH - " + dbName, Path.Combine(expectedDataPath, dbName + "_Data.mdf").ToLowerInvariant(), actualDataPath.ToLowerInvariant());

			string actualLogPath = conn.ExecuteScalar(string.Format(sqlTextRaw, dbName, "1")).ToString();
			AssertEquals("LOG FILE FULL PATH - " + dbName, Path.Combine(expectedLogPath, dbName + "_Log.ldf").ToLowerInvariant(), actualLogPath.ToLowerInvariant());

			string actualCollation = conn.ExecuteScalar(string.Format("SELECT databasepropertyex('{0}', 'Collation')", dbName)).ToString();
			AssertEquals("COLLATION - " + dbName, Db.DatabaseCollation, actualCollation);
		}

		string GetExpectedPath(AdminConnection conn, DbConnection.DatabaseFileTypes fileType)
		{
			var sqlText = FormattableString.Invariant($"SELECT TOP 1 physical_name FROM sys.database_files WHERE type = {(int)fileType} ORDER BY file_id;");
			var folderPath = Path.GetDirectoryName(conn.ExecuteScalar(sqlText).ToString());
			folderPath = Service.GetFilePathFromMasterIfRamDrive(conn, conn.CurrentDatabase, folderPath, fileType);
			return folderPath;
		}

		static int GetModelDbDataFileSize(DbConnection conn)
		{
			string sqlText = "SELECT size FROM model.sys.database_files WHERE type = 0";
			return Convert.ToInt32(conn.ExecuteScalar(sqlText));
		}

		/// <summary>
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="dbName"></param>
		/// <param name="expectedSizeInPages"></param>
		/// <param name="expectedGrowthInPagesOrPercent"></param>
		/// <param name="dbType">null/0 : data    1 : log</param>
		static void AssertDbDataFileSizeAndGrowth(DbConnection conn, string dbName, int expectedSizeInPages, int? expectedGrowthInPagesOrPercent, int? dbType = null)
		{
			string sqlText = string.Format("SELECT size, growth FROM [{0}].sys.database_files WHERE type = {1}", dbName, dbType == null ? 0 : dbType); //null/0-data 1-log

			using (var cmd = conn.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				reader.Read();

				Assert(
					string.Format("DATA FILE SIZE {0} < {1} (expected min size)", reader.ToString(), expectedSizeInPages.ToString()),
					reader.GetInt32(0) >= expectedSizeInPages);

				if (expectedGrowthInPagesOrPercent != null)
				{
					AssertEquals("DATA FILE GROWTH", expectedGrowthInPagesOrPercent, reader.GetInt32(1));
				}
			}
		}

		#region Setup / Teardown

		protected override void SetUp()
		{
			base.SetUp();
			testConnection = Db.NewAdminConnection();
			AdoTestUtils.DropDbIfExists(testConnection, TestDbName);
			AdoTestUtils.DropDbIfExists(testConnection, DatabaseCreationService.TempCreateDbNamePrefix + TestDbName);
		}

		protected override void TearDown()
		{
			AdoTestUtils.DropDbIfExists(testConnection, TestDbName);
			AdoTestUtils.DropDbIfExists(testConnection, DatabaseCreationService.TempCreateDbNamePrefix + TestDbName);
			if (dbToDropOnTearDown != null)
			{
				AdoTestUtils.DropDbIfExists(testConnection, dbToDropOnTearDown);
			}
			testConnection?.Dispose();
			base.TearDown();
		}

		AdminConnection testConnection;
		readonly string dbToDropOnTearDown;

		#endregion

		const string TestDbName = "AdminConnectionNonTransactionalTest-TestCreateDatabase-B84BEEB75E2243DD9EBA0D55BBD033E2";
	}
}
