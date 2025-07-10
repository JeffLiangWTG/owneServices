using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.DataProtection;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	[UseSnapshotProtection]
	class DbErrorMatchNonTransactionalTest : TestCase
	{
		/// <summary>
		/// Msg 15151, Level 16, State 1, Line 1
		/// Cannot drop the login '%s', because it does not exist or you do not have permission.
		/// </summary>
		public void TestServerPrincipalDoesNotExists()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				ExecuteAndAssert("DROP LOGIN [A login which does not exist]", DbErrorType.ServerPrincipalDoesNotExist, adminConnection);
			}
		}

		/// <summary>
		/// Msg 15025, Level 16, State 1, Line 1
		/// The server principal '%s' already exists.
		/// </summary>
		public void TestServerPrincipalAlreadyExists()
		{
			var applicationLogin = UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName);

			using (var adminConnection = Db.NewAdminConnection())
			{
				ExecuteAndAssert($"CREATE LOGIN {applicationLogin} WITH PASSWORD = 'q@CbK4eJ.~udy#9/J7KU'", DbErrorType.ServerPrincipalAlreadyExists, adminConnection);
			}
		}

		/// <summary>
		/// Msg 1801, Level 16, State 3
		/// Database '%.*ls' already exists. Choose a different database name.
		/// </summary>
		public void TestDatabaseAlreadyExists()
		{
			string sqlText = string.Format("CREATE DATABASE [{0}]", Db.DatabaseName);

			using (var adminConnection = Db.NewAdminConnection())
			{
				ExecuteAndAssert(sqlText, DbErrorType.DatabaseAlreadyExists, adminConnection);
			}
		}

		/// <summary>
		/// Msg 1802, Level 16, State 1, Line 1
		/// CREATE DATABASE failed. Some file names listed could not be created. Check related errors.
		/// </summary>
		public void TestCannotCreateDbFileErrorType()
		{
			var db1 = "TestDb01_B08F26D7_F09A_47E2_8AEA_56261B860001";
			var db2 = "TestDb02_B08F26D7_F09A_47E2_8AEA_56261B860002";
			string tempDir = TempForTest.TempPath;
			AdoTestUtils.DropDbIfExists(db1);

			try
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					string createDbScript = @"
						CREATE DATABASE {0}
							ON (NAME = db_data, FILENAME = '{1}\Db.mdf', SIZE = {2}GB )
							LOG ON (NAME = db_log, FILENAME = '{1}\Db_log.ldf')";

					adminConnection.ExecuteNonQuery(string.Format(createDbScript, db1, tempDir, 1)); // DbCommand in Data solution
					AssertEquals($"DB [{db1}] exists?", true, Db.Connection.DatabaseExists(db1));

					try
					{
						adminConnection.ExecuteNonQuery(string.Format(createDbScript, db2, tempDir, 1)); // DbCommand in Data solution
						AssertEquals($"DB [{db2}] exists?", false, Db.Connection.DatabaseExists(db1));
					}
					catch (SqlException ex)
					{
						AssertEquals("Number of errors:", 2, ex.Errors.Count);
						AssertEquals("First error number:", 5170, ex.Errors[0].Number);
						AssertEquals("Second error number:", 1802, ex.Errors[1].Number);
						var dbErrorMatch = new DbErrorMatch(ex);
						var errorType = dbErrorMatch.ExceptionType;
						AssertEquals("Wrong Exception thrown - " + ex.Message, DbErrorType.CannotCreateFileBecauseItAlreadyExists, errorType);
						Assert("Errors 5170 and 1802 are infrastructure errors", dbErrorMatch.IsInfrastructureDbError);
					}

					var sqlText = $"Select CONVERT(INT, available_bytes/ 1024 / 1024 / 1024) AS FreeSpaceGB From sys.dm_os_volume_stats(DB_ID('{db1}'), 1) freeSpaceGb";
					var freeDiskSpaceInGB = (int)adminConnection.ExecuteScalar(sqlText);

					try
					{
						sqlText = $"ALTER DATABASE[{db1}] MODIFY FILE(NAME = N'db_data', SIZE = {freeDiskSpaceInGB + 10}GB)";
						adminConnection.ExecuteNonQuery(sqlText); // DbCommand in Data solution
						Assert($"Alter DB should have failed", false);
					}
					catch (SqlException ex)
					{
						AssertEquals("Number of errors:", 1, ex.Errors.Count);
						AssertEquals("Error number:", 5149, ex.Errors[0].Number);
						var dbErrorMatch = new DbErrorMatch(ex);
						var errorType = dbErrorMatch.ExceptionType;
						AssertEquals("Wrong Exception thrown - " + ex.Message, DbErrorType.ModifyFileEncounteredOperatingSystemError, errorType);
						Assert("Error 5149 is an infrastructure error", dbErrorMatch.IsInfrastructureDbError);
					}

					AdoTestUtils.DropDbIfExists(db1);
					try
					{
						adminConnection.ExecuteNonQuery(string.Format(createDbScript, db1, tempDir, freeDiskSpaceInGB + 10)); // DbCommand in Data solution
						AssertEquals($"DB [{db1}] exists?", false, Db.Connection.DatabaseExists(db1));
					}
					catch (SqlException ex)
					{
						AssertEquals("Number of errors:", 2, ex.Errors.Count);
						AssertEquals("First error number:", 5149, ex.Errors[0].Number);
						AssertEquals("Second error number:", 1802, ex.Errors[1].Number);
						var dbErrorMatch = new DbErrorMatch(ex);
						var errorType = dbErrorMatch.ExceptionType;
						AssertEquals("Create DB with file size > free disk space. Wrong Exception thrown - " + ex.Message, DbErrorType.ModifyFileEncounteredOperatingSystemError, errorType);
						Assert("Errors 5148 and 1802 are infrastructure errors", dbErrorMatch.IsInfrastructureDbError);
					}
				}
			}
			finally
			{
				AdoTestUtils.DropDbIfExists(db1);
				AdoTestUtils.DropDbIfExists(db2);
			}
		}

		/// <summary>
		/// Msg 1803, Level 16, State 1, Line 1
		/// The CREATE DATABASE statement failed. The primary file must be at least %MB to accommodate a copy of the model database.
		/// </summary>
		public void TestCreateDbFailedSizeCannotAccommodateCopyOfModelDb()
		{
			string sqlText = @"
				if exists (select * from sys.databases where name = 'TestDb_596F7D8AAC474F71BE37D7A1DD1417EE')
					DROP DATABASE TestDb_596F7D8AAC474F71BE37D7A1DD1417EE
				CREATE DATABASE [TestDb_596F7D8AAC474F71BE37D7A1DD1417EE] ON (
					NAME = [TestDb_Data], FILENAME = 'C:\TestDb_596F7D8AAC474F71BE37D7A1DD1417EE.mdf', SIZE = 512KB)"; // The path just needs to be valid, beyond that it is irrelevant what it is

			if (!Db.Connection.ServerVersionNumber.IsMinimumRequiredVersionOrAbove)
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					ExecuteAndAssert(sqlText, DbErrorType.CreateDbFailedSizeCannotAccommodateCopyOfModelDb, adminConnection);
				}
			}
			else
			{
				Assert("Does not fail in SQL 2016. Instead the file size is adjusted to accommodate the model database size.", true);
			}
		}

		/// <summary>
		/// Cannot convert a char value to money. The char value has incorrect syntax.
		/// </summary>
		public void TestCannotConvertNonnumericCharValueToMoney()
		{
			string sqlText = "SELECT convert(money, '1Z345')";
			ExecuteAndAssert(sqlText, DbErrorType.CannotConvertNonnumericCharValueToMoney);
		}

		public void TestCouldNotFindStoredProcedure()
		{
			string sqlText = "exec DefinitellyMissingStoredProcedure";
			ExecuteAndAssert(sqlText, DbErrorType.CouldNotFindStoredProcedure);
		}

		public void TestDefinitionOfObjectHasChangedSinceCompilation()
		{
			var ex = AdoTestUtils.GetSqlException(2801, "The definition of object 'StoredProc' has changed since it was compiled.", Db.Connection);
			var handler = new DbErrorMatch(ex);
			AssertEquals("Incorrect SQL Exception type", handler.ExceptionType, DbErrorType.DefinitionOfObjectHasChangedSinceCompilation);
		}

		/// <summary>
		/// Could not find stored procedure 'DefinitellyMissingStoredProcedure'.
		/// </summary>
		public void TestCannotFindDataType()
		{
			var ex = AdoTestUtils.GetSqlException(351, "CannotFindDataType", Db.Connection);
			var handler = new DbErrorMatch(ex);
			AssertEquals("Incorrect SQL Exception type", handler.ExceptionType, DbErrorType.CannotFindDataType);

			ex = AdoTestUtils.GetSqlException(2715, "CannotFindDataType", Db.Connection);
			handler = new DbErrorMatch(ex);
			AssertEquals("Incorrect SQL Exception type", handler.ExceptionType, DbErrorType.CannotFindDataType);
		}

		/// <summary>
		/// Procedure or function '%s' expects parameter '%s', which was not supplied.
		/// </summary>
		public void TestExpectedParameterNotSupplied()
		{
			var ex = AdoTestUtils.GetSqlException(201, "ExpectedParameterNotSupplied", Db.Connection);
			var handler = new DbErrorMatch(ex);
			AssertEquals("Incorrect SQL Exception type", handler.ExceptionType, DbErrorType.ExpectedParameterNotSupplied);
		}

		/// <summary>
		/// Conversion failed when converting from a character string to uniqueidentifier.
		/// </summary>
		public void TestFailedToConvertCharToUniqueidentifier()
		{
			string sqlText = "SELECT convert(uniqueidentifier, 'X')";
			ExecuteAndAssert(sqlText, DbErrorType.FailedToConvertCharToUniqueidentifier);
		}

		/// <summary>
		/// The conversion of a %ls data type to a %ls data type resulted in an out-of-range value.
		/// </summary>
		public void TestDataTypeConversionOverflow()
		{
			string sqlText = "SELECT convert(datetime, '1000-01-01')";
			ExecuteAndAssert(sqlText, DbErrorType.DataTypeConversionOverflow);
		}

		/// <summary>
		/// The conversion from datetime data type to smalldatetime data type resulted in a smalldatetime overflow error.
		/// </summary>
		public void TestDatetimeToSmalldatetimeConversionOverflow()
		{
			string sqlText = "SELECT convert(smalldatetime, convert(datetime, '1800-01-01'))";
			ExecuteAndAssert(sqlText, DbErrorType.DataTypeConversionOverflow);
		}

		/// <summary>
		/// Arithmetic overflow error for data type %ls, value = %ld.
		/// </summary>
		public void TestArithmeticOverflowForDataType()
		{
			string sqlText = "SELECT convert(smallint, 32768)";
			ExecuteAndAssert(sqlText, DbErrorType.ArithmeticOverflowForNumericType);

			sqlText = "SELECT convert(int, -2147483649)";
			ExecuteAndAssert(sqlText, DbErrorType.ArithmeticOverflowConvertingToDataType);
		}

		public void TestDatabaseDoesNotExist()
		{
			string dbName = "This_Got_To_Be_A_Database_Name_That_Does_Not_Exist_13228645DC8A4C18A059DD2B9C945581";
			string sqlText = string.Format("ALTER DATABASE [{0}] SET AUTO_SHRINK OFF", dbName);

			try
			{
				Db.Connection.ExecuteNonQuery(sqlText); // DbCommand in Data solution
				Fail("Should throw exception");
			}
			catch (SqlException ex)
			{
				DbErrorMatch dbError = new DbErrorMatch(ex);
				AssertEquals("Wrong Exception caught - " + ex.Message, DbErrorType.DatabaseDoesNotExist, dbError.ExceptionType);
			}
		}

		public void TestCouldNotLocateDbInSysdatabases()
		{
			string dbName = "This_Got_To_Be_A_Database_Name_That_Does_Not_Exist_13228645DC8A4C18A059DD2B9C945581";
			string sqlText = string.Format("BACKUP DATABASE {0} TO DISK = 'Anypath\\Anyname.bak'", dbName);

			try
			{
				Db.Connection.ExecuteNonQuery(sqlText); // DbCommand in Data solution
				Fail("Should throw exception");
			}
			catch (SqlException ex)
			{
				DbErrorMatch dbError = new DbErrorMatch(ex);
				AssertEquals("Wrong Exception caught - " + ex.Message, DbErrorType.CouldNotLocateDbInSysdatabases, dbError.ExceptionType);
			}
		}

		/// <summary>
		/// Msg 22901:
		///   The database 'xxx' is not enabled for Change Data Capture.
		///   Ensure that the correct database context is set and retry the operation.
		///   To report on the databases enabled for Change Data Capture, query the is_cdc_enabled column in the sys.databases catalog view.
		/// Msg 22910:
		///   The cleanup request for database 'master' failed.  The database is not enabled for Change Data Capture.
		/// </summary>
		public void TestDatabaseNotEnabledForChangeDataCapture()
		{
			using (var conn = Db.NewAdminConnection())
			{
				// 22901
				ExecuteAndAssert("EXEC master.sys.sp_cdc_scan", DbErrorType.DatabaseNotEnabledForChangeDataCapture, conn);
				ExecuteAndAssert("EXEC master.sys.sp_cdc_help_jobs", DbErrorType.DatabaseNotEnabledForChangeDataCapture, conn);
				ExecuteAndAssert("EXEC master.sys.sp_cdc_change_job", DbErrorType.DatabaseNotEnabledForChangeDataCapture, conn);
				// 22910
				ExecuteAndAssert("EXEC master.sys.sp_MScdc_cleanup_job", DbErrorType.DatabaseNotEnabledForChangeDataCapture, conn);
				ExecuteAndAssert("EXEC master.sys.sp_cdc_cleanup_change_table @capture_instance = '', @low_water_mark = null", DbErrorType.DatabaseNotEnabledForChangeDataCapture, conn);
			}
		}

		/// <summary>
		/// Msg 1223, Level 16, State 1, Procedure xp_userlock, Line 39
		/// Cannot release the application lock (Database Principal: 'public', Resource: 'x') because it is not currently held.
		/// </summary>
		public void TestCannotReleaseAppLockBecauseItIsNotCurrentlyHeld()
		{
			ExecuteAndAssert(
				"EXEC sp_releaseapplock @Resource = 'x', @LockOwner = 'Session'",
				DbErrorType.CannotReleaseAppLockBecauseItIsNotCurrentlyHeld);
		}

		/// <summary>
		/// Msg 4928, Level 16, State 1, Procedure sp_rename, Line 612
		/// Cannot alter column 'JE_WarehouseReleaseStatus' because it is 'REPLICATED'.
		/// </summary>
		public void TestCannotAlterColumnBecauseItIsReplicated()
		{
			string testRefDb = ((IPhysicalRefDbLocation)Db.Connection).GetReferenceDatabaseName(RefDbTypeEnum.Tariff, "NZ");
			const string testTable = "CannotAlterColumnBecauseItIsReplicated$Table";

			using (var conn = Db.NewAdminConnection(testRefDb))
			{
				try
				{
					conn.BeginTransaction();
					conn.ExecuteNonQuery("EXEC sys.sp_cdc_enable_db");
					conn.ExecuteNonQuery(string.Format("CREATE TABLE [{0}] (Col1 int PRIMARY KEY, Col2 int)", testTable));
					conn.ExecuteNonQuery(string.Format("EXEC sys.sp_cdc_enable_table @source_schema='dbo', @source_name='{0}', @role_name=null", testTable));
					ExecuteAndAssert(string.Format("EXEC sp_rename '{0}.Col2', 'Col2n', 'COLUMN'", testTable), DbErrorType.CannotAlterColumnBecauseItIsReplicated, conn);
				}
				finally
				{
					conn.RollbackTransaction();
				}
			}
		}

		/// <summary>
		/// The SELECT permission was denied on the object 'XXX', database 'YYY', schema 'ZZZ'.
		/// </summary>
		public void TestPermissionDeniedOnObject()
		{
			string testDb = Db.DatabaseName + "~TestPermissionDeniedOnObject-DB~";

			using (var testAdminConn = Db.NewAdminConnection())
			{
				try
				{
					AdoTestUtils.DropDbIfExists(testAdminConn, testDb);

					string sqlText = string.Format(CultureInfo.InvariantCulture, "CREATE DATABASE [{0}]", testDb);
					testAdminConn.ExecuteNonQuery(sqlText);

					sqlText = string.Format(CultureInfo.InvariantCulture,
						"IF EXISTS(SELECT 1 FROM [{0}].sys.database_principals WHERE name = '{1}') EXEC [{0}]..sp_executesql N'DROP USER [{1}]';",
						testDb,
						RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName));
					testAdminConn.ExecuteNonQuery(sqlText);

					sqlText = string.Format(
						CultureInfo.InvariantCulture,
						"EXEC [{0}]..sp_executesql N'CREATE TABLE T1 (Col1 bit)'",
						testDb);
					testAdminConn.ExecuteNonQuery(sqlText);

					sqlText = string.Format(
						CultureInfo.InvariantCulture,
						"EXEC [{0}]..sp_executesql N'CREATE USER [{1}] FOR LOGIN [{1}]'",
						testDb,
						RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName));
					testAdminConn.ExecuteNonQuery(sqlText);

					sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}]..T1", testDb);
					var errorMatch = ExecuteAndAssert(sqlText, DbErrorType.PermissionDeniedOnObject);
					string errorDbName = errorMatch.GetDatabaseFromSecurityError();
					AssertEquals("Database from error message", testDb, errorDbName);
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(testAdminConn, testDb);
				}
			}
		}

		[SnailTest()]
		public void TestCannotChangeDbStateWhenInSingleUserMode()
		{
			try
			{
				AdoTestUtils.DropDbIfExists(TestDb01);
				AdoTestUtils.CreateDbIfNotExists(TestDb01);

				using (var singleUserConnection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, TestDb01))
				{
					string setSingleUserSqlText = "ALTER DATABASE " + TestDb01 + " SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
					singleUserConnection.ExecuteNonQuery(setSingleUserSqlText);

					using (DbConnection anotherDbConnection = Db.NewAdminConnection())
					{
						try
						{
							anotherDbConnection.ExecuteNonQuery("ALTER DATABASE " + TestDb01 + " SET AUTO_CLOSE OFF");
							Fail("Should throw exception");
						}
						catch (SqlException e)
						{
							AssertEquals("Wrong Exception caught - " + e.Message, DbErrorType.CannotAlterDbStateWhileInSingleUserMode, new DbErrorMatch(e).ExceptionType);
						}
					}
				}
			}
			finally
			{
				AdoTestUtils.DropDbIfExists(TestDb01);
			}
		}

		[SnailTest()]
		public void TestCannotBackupLogWithNoCurrentDbBackup()
		{
			string testBackupPath = TempForTest.TempPath;

			try
			{
				AdoTestUtils.DropDbIfExists(TestDb01);
				AdoTestUtils.CreateDbIfNotExists(TestDb01);

				string sqlText = string.Format("ALTER DATABASE [{0}] SET RECOVERY FULL", TestDb01);

				using (var connection = Db.NewAdminConnection())
				{
					connection.ExecuteNonQuery(sqlText); // DbCommand in Data solution

					try
					{
						sqlText = string.Format("BACKUP LOG {0} TO DISK = '{1}\\{0}.trn' WITH INIT", TestDb01, testBackupPath);
						connection.ExecuteNonQuery(sqlText); // DbCommand in Data solution
						Fail("Should throw exception");
					}
					catch (SqlException ex)
					{
						DbErrorType errorType = new DbErrorMatch(ex).ExceptionType;
						AssertEquals("Wrong Exception caught - " + ex.Message, DbErrorType.CannotBackupLogWithNoCurrentDbBackup, errorType);
					}
				}
			}
			finally
			{
				AdoTestUtils.DropDbIfExists(TestDb01);
			}
		}

		public void TestCouldNotObtainExclusiveLock()
		{
			var error = SqlExceptionBuilder.CreateSqlError(5030, 1, 1, Db.Connection.ServerName, "The database could not be exclusively locked to perform the operation", "", 1);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errors);

			var errorHandler = new DbErrorHandler(exception, Db.Connection);
			Assert("Did not correctly match the  5030 error.", errorHandler.ExceptionType == DbErrorType.CouldNotObtainExclusiveLock);

			error = SqlExceptionBuilder.CreateSqlError(1807, 1, 1, Db.Connection.ServerName, "Could not obtain exclusive lock on database '%.*ls'. Retry the operation later.", "", 1);
			errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			exception = SqlExceptionBuilder.CreateSqlException(errors);

			errorHandler = new DbErrorHandler(exception, Db.Connection);
			Assert("Did not correctly match the  1807 error.", errorHandler.ExceptionType == DbErrorType.CouldNotObtainExclusiveLock);
		}

		public void TestIndexOperationAlreadyInProgress()
		{
			var error = SqlExceptionBuilder.CreateSqlError(1912, 1, 1, Db.Connection.ServerName, "Could not proceed with index DDL operation on %S_MSG '%.*ls' because it conflicts with another concurrent operation that is already in progress on the object. The concurrent operation could be an online index operation on the same object or another concurrent operation that moves index pages like DBCC SHRINKFILE.", "", 1);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errors);

			var errorHandler = new DbErrorHandler(exception, Db.Connection);
			Assert("Did not correctly match the 1912 error.", errorHandler.ExceptionType == DbErrorType.IndexOperationAlreadyInProgress);
		}

		[SnailTest()]
		public void TestLogBackupIsTooEarlyToApplyToTheDatabase()
		{
			string testBackupPath = TempForTest.TempPath;

			try
			{
				AdoTestUtils.DropDbIfExists(TestDb01);
				AdoTestUtils.CreateDbIfNotExists(TestDb01);

				string sqlText = string.Format("ALTER DATABASE [{0}] SET RECOVERY FULL", TestDb01);

				using (var adminConnection = Db.NewAdminConnection())
				{
					adminConnection.ExecuteNonQuery(sqlText); // DbCommand in Data solution

					try
					{
						sqlText = string.Format(@"BACKUP DATABASE {0} TO DISK = '{1}\\{0}.bak' WITH INIT
					BACKUP LOG {0} TO DISK = '{1}\\{0}_1.trn'
					BACKUP DATABASE {0} TO DISK = '{1}\\{0}.bak' WITH INIT", TestDb01, testBackupPath);
						adminConnection.ExecuteNonQuery(sqlText); // DbCommand in Data solution

						AdoTestUtils.DropDbIfExists(TestDb01);

						sqlText = string.Format(@"RESTORE DATABASE {0} FROM DISK = '{1}\\{0}.bak' WITH NORECOVERY
					RESTORE LOG {0} FROM DISK = '{1}\\{0}_1.trn'", TestDb01, testBackupPath);
						adminConnection.ExecuteNonQuery(sqlText); // DbCommand in Data solution					}

						Fail("Should throw exception");
					}
					catch (SqlException ex)
					{
						DbErrorType errorType = new DbErrorMatch(ex).ExceptionType;
						AssertEquals("Wrong Exception caught - " + ex.Message, DbErrorType.LogBackupIsTooEarlyToApplyToTheDatabase, errorType);
					}
				}
			}
			finally
			{
				AdoTestUtils.DropDbIfExists(TestDb01);
				File.Delete(Path.Combine(testBackupPath, TestDb01 + ".bak"));
				File.Delete(Path.Combine(testBackupPath, TestDb01 + "_1.trn"));
			}
		}

		[SnailTest()]
		public void TestFileCannotBeRestoredToPath()
		{
			string tempDir = TempForTest.TempPath;
			string backupFilePath = Path.Combine(tempDir, TestDb01 + ".bak");

			File.Delete(backupFilePath);
			AdoTestUtils.DropDbIfExists(TestDb01);

			try
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					string sqlText = string.Format(@"
						CREATE DATABASE {0}
							ON (NAME = {0}_data, FILENAME = '{1}\{0}.mdf')
							LOG ON (NAME = {0}_log, FILENAME = '{1}\{0}_log.ldf')",
						TestDb01, tempDir);
					adminConnection.ExecuteNonQuery(sqlText); // DbCommand in Data solution

					sqlText = string.Format(@"BACKUP DATABASE {0} TO DISK = '{1}' WITH INIT",
						TestDb01, backupFilePath);
					adminConnection.ExecuteNonQuery(sqlText); // DbCommand in Data solution

					try
					{
						sqlText = string.Format(@"
						RESTORE DATABASE {0} FROM disk = '{1}'
							WITH REPLACE,
							MOVE '{0}_data' to '{1}.mdf',
							MOVE '{0}_log'  to '{1}'",
							TestDb01, backupFilePath);
						adminConnection.ExecuteNonQuery(sqlText); // DbCommand in Data solution
					}
					catch (SqlException ex)
					{
						DbErrorType errorType = new DbErrorMatch(ex).ExceptionType;

						if (errorType != DbErrorType.FileCannotBeRestoredToPath && ex.Errors.Count > 1)
						{
							SqlError error = ex.Errors[1];
							SqlErrorCollection errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
							SqlException secondSqlEx = SqlExceptionBuilder.CreateSqlException(errors);
							errorType = new DbErrorMatch(secondSqlEx).ExceptionType;
						}

						AssertEquals("Wrong Exception thrown - " + ex.Message, DbErrorType.FileCannotBeRestoredToPath, errorType);
					}
				}
			}
			finally
			{
				AdoTestUtils.DropDbIfExists(TestDb01);
				File.Delete(backupFilePath);
			}
		}

		[SnailTest()]
		public void TestDatabaseInSingleUserModeAndAlreadyOpen()
		{
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestDb01, Db.DatabaseName))
			{
				using (var singleUserConnection = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, TestDb01))
				{
					string setSingleUserSqlText = "ALTER DATABASE " + TestDb01 + " SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
					singleUserConnection.ExecuteNonQuery(setSingleUserSqlText);

					using (DbConnection anotherDbConnection = Db.NewExtraConnectionToMainDb())
					{
						try
						{
							anotherDbConnection.EnsureIsOpen();
							anotherDbConnection.ExecuteNonQuery(string.Format("SELECT TOP 1 name FROM {0}.sys.objects", TestDb01));
							Fail("Should throw exception");
						}
						catch (SqlException e)
						{
							AssertEquals("Wrong Exception caught - " + e.Message, DbErrorType.DatabaseInSingleUserModeAndAlreadyOpen, new DbErrorMatch(e).ExceptionType);
						}
					}
				}
			}
		}

		public void TestNotAnActiveProcessId()
		{
			var sql = "SELECT ISNULL(MAX(session_id), 50) FROM sys.dm_exec_sessions";

			using (var adminConnection = Db.NewAdminConnection())
			using (var cmd = adminConnection.Command(sql))
			{
				var inexistingProcess = Convert.ToInt32(cmd.ExecuteScalar()) + 1;

				try
				{
					adminConnection.ExecuteNonQuery("KILL " + inexistingProcess.ToString());
					Fail("Should throw exception");
				}
				catch (SqlException e)
				{
					AssertEquals("Wrong Exception caught - " + e.Message, DbErrorType.NotAnActiveProcessId, new DbErrorMatch(e).ExceptionType);
				}
			}
		}

		public void TestMetadataHasBeenChanged()
		{
			using (var anotherConnection = Db.NewExtraConnectionToMainDb())
			{
				Db.Connection.ExecuteNonQuery("SET TRANSACTION ISOLATION LEVEL SNAPSHOT; BEGIN TRANSACTION");
				string selectSql = "SELECT * FROM dbo.DummyBizo";
				Db.Connection.ExecuteNonQuery(selectSql);
				anotherConnection.ExecuteNonQuery("ALTER TABLE dbo.DummyBizo REBUILD WITH (ONLINE = ON);");
				try
				{
					Db.Connection.ExecuteNonQuery(selectSql);
					Fail("Should throw exception");
				}
				catch (SqlException e)
				{
					AssertEquals("DbErrorType", DbErrorType.MetadataHasBeenChanged, new DbErrorMatch(e).ExceptionType);
				}
			}
		}

		[SnailTest()]
		public void TestCouldNotBeginTransactionAsDbIsReadOnly()
		{
			try
			{
				AdoTestUtils.DropDbIfExists(TestDb01);
				AdoTestUtils.CreateDbIfNotExists(TestDb01);

				string sqlText = string.Format("ALTER DATABASE {0} SET READ_ONLY WITH ROLLBACK IMMEDIATE", TestDb01);
				using var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, TestDb01);
				connection.ExecuteNonQuery(sqlText); // DbCommand in Data solution

				try
				{
					sqlText = string.Format("CREATE TABLE {0}.dbo.TestCouldNotBeginTransactionAsDbIsReadOnlyTable (Col1 int)", TestDb01);
					connection.ExecuteNonQuery(sqlText); // DbCommand in Data solution
					Fail("Should throw exception");
				}
				catch (SqlException e)
				{
					AssertEquals("Wrong Exception caught - " + e.Message, DbErrorType.CouldNotBeginTransactionAsDbIsReadOnly, new DbErrorMatch(e).ExceptionType);
				}
			}
			finally
			{
				AdoTestUtils.DropDbIfExists(TestDb01);
			}
		}

		[SnailTest()]
		public void TestDatabaseInEmergencyModeOrDamaged()
		{
			try
			{
				AdoTestUtils.DropDbIfExists(TestDb01);
				AdoTestUtils.CreateDbIfNotExists(TestDb01);

				var sqlText = string.Format("ALTER DATABASE {0} SET EMERGENCY", TestDb01);
				using var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, TestDb01);
				connection.ExecuteNonQuery(sqlText);
				try
				{
					sqlText = string.Format("CREATE TABLE {0}.dbo.TestDatabaseInEmergencyModeOrDamaged (Col1 int)", TestDb01);
					connection.ExecuteNonQuery(sqlText);
					Fail("Should throw exception");
				}
				catch (SqlException e)
				{
					AssertEquals("Wrong Exception caught - " + e.Message, DbErrorType.DatabaseInEmergencyModeOrDamaged, new DbErrorMatch(e).ExceptionType);
				}
			}
			finally
			{
				AdoTestUtils.DropDbIfExists(TestDb01);
			}
		}

		///// <summary>
		///// Creates a new database with the minimum data size with no autogrowth
		///// and fills the data until SQL Server raises a filegroup is full error.
		///// </summary>
		[SnailTest()]
		public void TestDbFileGroupIsFull()
		{
			string tempDir = TempForTest.TempPath;
			string testDbName = "TestFileGroupIsFull_DE6790C9A7D143DC8AF3AE7FECEC7D7B";
			{
				using (AdminConnection auxConn = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					AdoTestUtils.DropDbIfExists(auxConn, testDbName);

					try
					{
						CreateTestDbAndFillItsFileGroup(tempDir, testDbName, auxConn);
						Fail("Should throw exception");
					}
					catch (SqlException e)
					{
						AssertEquals(
							"Wrong Exception caught: " + e.Number.ToString() + " - " + e.Message,
							DbErrorType.DbFilegroupIsFull, new DbErrorMatch(e).ExceptionType);
					}
					finally
					{
						AdoTestUtils.DropDbIfExists(auxConn, testDbName);
					}
				}
			}
		}

		void CreateTestDbAndFillItsFileGroup(string tempDir, string testDbName, DbConnection auxConn)
		{
			string sqlText = string.Format(@"
					CREATE DATABASE {0}
						ON (NAME = {0}_Data, FILENAME = '{1}\{0}_Data.mdf', FILEGROWTH = 0%)
						LOG ON (NAME = {0}_Log, FILENAME = '{1}\{0}_Log.ldf')", testDbName, tempDir);

			using (DbCommand cmd = auxConn.Command(sqlText))
			{
				cmd.ExecuteNonQuery();
				cmd.CommandText = string.Format("ALTER DATABASE {0} SET RECOVERY SIMPLE", testDbName);
				cmd.ExecuteNonQuery();

				cmd.CommandText = string.Format("EXEC [{0}]..sp_executesql N'CREATE TABLE Test (col1 char(8000))';", testDbName);
				cmd.ExecuteNonQuery();

				cmd.CommandText = string.Format(@"
				DECLARE @i int
				SET @i = 0
				WHILE (@i < {0})
				BEGIN
					INSERT {1}..Test VALUES (@i)
					SET @i = @i + 1
				END", 10000, testDbName);
				cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Creates a new database with the minimum log size with no autogrowth
		/// and fills the log until SQL Server raises a Log is full error.
		/// Note: This test may be considerably slow!
		/// </summary>
		[SnailTest()]
		protected void NotToRunEveryTime_TooSlow_TestLogIsFull()
		{
			using (DbConnection auxConn = Db.NewExtraConnectionToMainDb())
			{
				string sqlText = "IF EXISTS (SELECT name FROM sys.databases WHERE name = 'TestLogFull') DROP DATABASE TestLogFull";
				using (DbCommand cmd = auxConn.Command(sqlText))
				{
					cmd.ExecuteNonQuery();

					try
					{
						cmd.CommandText = "CREATE DATABASE TestLogFull";
						cmd.ExecuteNonQuery();
						cmd.CommandText = "ALTER DATABASE TestLogFull MODIFY FILE (NAME = 'TestLogFull_Log', FILEGROWTH = 0%)";
						cmd.ExecuteNonQuery();

						cmd.CommandText = "EXEC TestLogFull..sp_executesql N'CREATE TABLE Test (col1 char(8000))';";
						cmd.ExecuteNonQuery();

						cmd.CommandText = @"
					DECLARE @i int
					SET @i = 0
					WHILE (@i < 25)
					BEGIN
						INSERT TestLogFull..Test VALUES ('')
						SET @i = @i + 1
					END";
						cmd.ExecuteNonQuery();
						cmd.CommandText = "INSERT TestLogFull..Test VALUES ('')";
						cmd.ExecuteNonQuery();

						Fail("Should throw exception");
					}
					catch (SqlException e)
					{
						AssertEquals("Wrong Exception caught - " + e.Message, DbErrorType.LogIsFull, new DbErrorMatch(e).ExceptionType);
					}
					finally
					{
						cmd.CommandText = @"
						IF EXISTS (SELECT name FROM sys.databases WHERE name = 'TestLogFull')
							DROP DATABASE TestLogFull";
						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		#region TestDbIsInTheMiddleOfRestore

		//        [SnailTest()]
		//        public void TestDbIsInTheMiddleOfRestore()
		//        {
		//            string testDbName = "TestDB_23F2B10BB967448880C1DBA5BEB4A07E";
		//            string sqlText;
		//            using (TempDirectory tempDirectory = new TempDirectory())
		//            {
		//                TempDbHelper.Instance.DropTestDb(testDbName, TestConnection);
		//                TempDbHelper.Instance.BackupNewDB(testDbName, tempDirectory);

		//                using (BackgroundWorker backgroundWorker = new BackgroundWorker())
		//                {
		//                    backgroundWorker.DoWork += delegate(object sender, DoWorkEventArgs e)
		//                    {
		//                        sqlText = string.Format(@"RESTORE DATABASE {0} FROM DISK = '{1}\{0}.bak'
		//						WITH REPLACE,
		//						MOVE '{0}_Data' to '{1}\{0}.mdf',
		//						MOVE '{0}_Log'  to '{1}\{0}.ldf'",
		//                        testDbName, tempDirectory.DirectoryName);
		//                        TestConnection.ExecuteNonQuery(sqlText);
		//                    };
		//                    backgroundWorker.RunWorkerAsync();

		//                    while (!backgroundWorker.IsBusy) ;
		//                    System.Threading.Thread.Sleep(1000);

		//                    using (DbConnection auxConn = Db.NewExtraConnectionToMainDb())
		//                    {
		//                        try
		//                        {
		//                            sqlText = string.Format(@"SELECT TOP 1 * FROM [{0}].dbo.Test", testDbName);
		//                            auxConn.ExecuteScalar(sqlText);
		//                            Fail("Should have thrown an exception");
		//                        }
		//                        catch (SqlException ex)
		//                        {
		//                            DbErrorType errorType = new DbErrorMatch(ex, auxConn).ExceptionType;
		//                            AssertEquals("Wrong exception thrown - " + ex.Message, DbErrorType.DatabaseIsInTheMiddleOfRestore, errorType);
		//                        }
		//                    }
		//                    TempDbHelper.Instance.DropTestDb(testDbName, TestConnection);
		//                }
		//            }
		//        }

		//    public class TempDbHelper
		//    {
		//      private TempDbHelper()
		//      {
		//      }

		//      static public TempDbHelper Instance
		//      {
		//        get { return instance ?? (instance = new TempDbHelper()); }
		//      }
		//      static TempDbHelper instance;

		//      public void BackupNewDB(string TestDbName, TempDirectory TempDirectory)
		//      {
		//        BackupNewDB(TestDbName, TempDirectory, 2000);
		//      }

		//      public void BackupNewDB(string TestDbName, TempDirectory TempDirectory, int NumberOfRows)
		//      {
		//        CreateTestDbAndFillItsFileGroup(TempDirectory, TestDbName, Db.Connection, NumberOfRows, false, false);
		//        string sqlText = string.Format(@"BACKUP DATABASE {0} TO DISK = '{1}\{0}.bak' WITH INIT",
		//          TestDbName, TempDirectory.DirectoryName);
		//        Db.Connection.ExecuteNonQuery(sqlText);

		//        DropTestDb(TestDbName, Db.Connection);
		//      }

		//      public void CreateTestDbAndFillItsFileGroup(TempDirectory tempDirectory, string testDbName, DbConnection auxConn, int numberOfRows, bool setFilegrowth0, bool setRecoverySimple)
		//      {
		//        string sqlText = string.Format(@"
		//					CREATE DATABASE {0}
		//						ON (NAME = {0}_Data, FILENAME = '{1}\{0}_Data.mdf'{2})
		//						LOG ON (NAME = {0}_Log, FILENAME = '{1}\{0}_Log.ldf')", testDbName, tempDirectory.DirectoryName, setFilegrowth0 ? ", FILEGROWTH = 0%" : "");

		//        DbCommand cmd = auxConn.Command(sqlText);
		//        cmd.ExecuteNonQuery();

		//        if (setRecoverySimple)
		//        {
		//          cmd.CommandText = string.Format("ALTER DATABASE {0} SET RECOVERY SIMPLE", testDbName);
		//          cmd.ExecuteNonQuery();
		//        }

		//        auxConn.CurrentDatabase = testDbName;
		//        cmd.CommandText = "CREATE TABLE Test (col1 char(8000))";
		//        cmd.ExecuteNonQuery();

		//        cmd.CommandText = string.Format(@"
		//							DECLARE @i int
		//							SET @i = 0
		//							WHILE (@i < {0})
		//							BEGIN
		//								INSERT Test VALUES (@i)
		//								SET @i = @i + 1
		//							END", numberOfRows);
		//        cmd.ExecuteNonQuery();
		//      }

		//      public void DropTestDb(string testDbName, DbConnection auxConn)
		//      {
		//        auxConn.CurrentDatabase = Db.DatabaseName;
		//        DbCommand cmd = auxConn.Command(string.Format("IF EXISTS (SELECT name FROM sys.databases WHERE name = '{0}') DROP DATABASE {0}", testDbName));
		//        cmd.ExecuteNonQuery();
		//      }
		//    }

		#endregion

		SqlException TryConnectWithLogin(string loginName, string password)
		{
			try
			{
				using (var connection = new ExtraConnectionWithTinyTimeoutForTest(Db.ServerName, Db.DatabaseName, loginName, password))
				{
					connection.EnsureIsOpen();
					return null;
				}
			}
			catch (SqlException sqlEx)
			{
				return sqlEx;
			}
		}

		[SnailTest]
		public void TestLoginFailedBecauseItIsLockedOut()
		{
			using (var login = new DbLoginForTest("~_Temp_TestLoginFailedBecauseItIsLockedOut_~", "~Some*Strong*Pwd~", checkPolicy: true))
			{
				SqlException sqlEx = null;
				for (int i = 0; i < 10 && sqlEx == null; i++)
				{
					sqlEx = TryConnectWithLogin(login.LoginName, "~The*Wrong*Password~");
				}
				AssertNotNull("PRE: Should be locked out - check this machines DB lockout rules (should be locked out after 3 failed attempts)", sqlEx);

				var expectionExceptionType = DbErrorType.LoginFailedForUser;
				AssertEquals(string.Format("Wrong Exception caught - {0} Exception Number {1}", sqlEx, sqlEx.Number), expectionExceptionType, new DbErrorMatch(sqlEx).ExceptionType);
			}
		}

		public void TestCannotDropLoginWhoOwnsDatabase()
		{
			const string testSuffix = "-Test-78A42E68-45C7-4403-A6E3-221C710955B4";

			string createSql = string.Format(CultureInfo.InvariantCulture, @"
				CREATE LOGIN [Login{0}] WITH PASSWORD = '', CHECK_POLICY = OFF;
				CREATE DATABASE [Db{0}];
				ALTER AUTHORIZATION ON DATABASE::[Db{0}] TO [Login{0}];",
				testSuffix);

			string dropSql = string.Format(CultureInfo.InvariantCulture,
				"IF EXISTS (SELECT null FROM sys.server_principals WHERE name = 'Login{0}') DROP LOGIN [Login{0}]",
				testSuffix);

			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(adminConnection, "Db" + testSuffix);
				adminConnection.ExecuteNonQuery(dropSql);

				try
				{
					adminConnection.ExecuteNonQuery(createSql);
					ExecuteAndAssert(dropSql, DbErrorType.CannotDropLoginWhoOwnsDatabase, adminConnection);
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(adminConnection, "Db" + testSuffix);
					adminConnection.ExecuteNonQuery(dropSql);
				}
			}
		}

		public void TestDeadlockError()
		{
			var pk1 = Guid.NewGuid();
			var pk2 = Guid.NewGuid();

			using (var victimUserEvent = new AutoResetEvent(initialState: false))
			using (var winnerUserEvent = new AutoResetEvent(initialState: false))
			{
				using (var victimConnection = Db.NewExtraConnectionToMainDb())
				using (victimConnection.TemporarySetDeadlockPriority(DeadlockPriority.Low))
				using (victimConnection.BeginTransactionWithManager())
				{
					var task = Task.Run(() =>
					{
						using (Db.DisposableActionForDbConnection())
						using (var winnerConnection = Db.NewExtraConnectionToMainDb())
						using (winnerConnection.TemporarySetDeadlockPriority(DeadlockPriority.High))
						using (winnerConnection.BeginTransactionWithManager())
						{
							winnerConnection.ExecuteNonQuery($"INSERT dbo.DummyBizo(Z0_PK) VALUES('{pk1}')");
							winnerUserEvent.Set();
							victimUserEvent.WaitOne();
							winnerConnection.ExecuteNonQuery($"UPDATE dbo.DummyBizo SET Z0_Guid = newid() WHERE Z0_PK = '{pk2}'");
						}
					});

					victimConnection.ExecuteNonQuery($"INSERT dbo.DummyBizo(Z0_PK) VALUES('{pk2}')");
					victimUserEvent.Set();
					winnerUserEvent.WaitOne();
					ExecuteAndAssert($"UPDATE dbo.DummyBizo SET Z0_Guid = newid() WHERE Z0_PK = '{pk1}'", DbErrorType.DeadlockError, victimConnection);
					task.Wait(TimeSpan.FromSeconds(10));
				}
			}
		}

		DbErrorMatch ExecuteAndAssert(string sqlText, DbErrorType expectedErrorType, DbConnection testConnection = null)
		{
			DbErrorMatch result = null;

			if (testConnection == null)
			{
				testConnection = Db.Connection;
			}

			try
			{
				testConnection.ExecuteNonQuery(sqlText);
				Fail("Should throw exception");
			}
			catch (SqlException ex)
			{
				result = new DbErrorMatch(ex);
				AssertEquals("Wrong Exception caught - " + ex.Message, expectedErrorType, result.ExceptionType);
			}

			AssertNotNull(result);
			return result;
		}

		const string TestDb01 = "DbErrorMatchNonTransactionalTestDb235068B513DD4CACA27D4C6D2C35C557";
	}
}
