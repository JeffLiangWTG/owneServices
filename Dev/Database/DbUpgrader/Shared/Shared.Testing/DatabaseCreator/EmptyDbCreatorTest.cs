namespace Enterprise.DbUpgrader.Shared
{
	using System;
	using System.Globalization;
	using System.IO;
	using CargoWise.Data;
	using CargoWise.Data.Testing;
	using NUnit.Framework;

	public class EmptyDbCreatorTest : TestCase
	{
		public void TestCreateDb()
		{
			AssertCreateDb(mappingLogins: false);
		}

		public void TestCreateDbWithLoginMapping()
		{
			AssertCreateDb(mappingLogins: true);
		}

		void AssertCreateDb(bool mappingLogins)
		{
			string testDbName = "SimpleDbCreatorTestTestCreateDb";
			IDbCreator testDbCreator = new EmptyDbCreator(testDbName, dataPath: null, mapDbLogins: mappingLogins);
			testDbCreator.Drop(testConn);

			AssertEquals("Database should NOT exist", false, Db.Connection.DatabaseExists(testDbName));

			try
			{
				testDbCreator.CreateIfNotExists(testConn);
				AssertEquals("Database should exist", true, Db.Connection.DatabaseExists(testDbName));
				AssertEquals("Database collation", Db.DatabaseCollation, GetDbCollation(testDbName));
				//We do not care if DB user is mapped to server login or not if we have mapDbLogins = false
				if (mappingLogins)
				{
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(testConn, testDbName, ((IDbLoginRepair)testConn).RestrictedWriterDbLoginName, expected: mappingLogins);
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(testConn, testDbName, ((IDbLoginRepair)testConn).ReaderDbLoginName, expected: mappingLogins);
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(testConn, testDbName, ((IDbLoginRepair)testConn).RestrictedReaderDbLoginName, expected: mappingLogins);
					DatabaseLoginTest.AssertLoginIsMappedToDatabase(testConn, testDbName, ((IDbLoginRepair)testConn).UnrestrictedWriterDbLoginName, expected: mappingLogins);
				}
			}
			finally
			{
				testDbCreator.Drop(testConn);
			}
		}

		public void TestCreateDbHandlesExistingFileConflict()
		{
			AssertCreateDbHandlesExistingFileConflict(testConn.CurrentDatabase);
		}

		public void TestCreateDbRunningOnMasterHandlesExistingFileConflict()
		{
			AssertCreateDbHandlesExistingFileConflict(Db.SqlMasterDb);
		}

		void AssertCreateDbHandlesExistingFileConflict(string dbNameForCreationContext)
		{
			string testDbName = "TestEmptyDbCreator750740288C944719A34BB5E3EACABD3C";
			DbCreatorTestUtils.CreateConflictFiles(testConn, testDbName);

			string currentDbFileFolder = DbCreatorTestUtils.GetCurrentDbFileFolder();
			IDbCreator dbCreator = new EmptyDbCreatorForTesting(testDbName, currentDbFileFolder);

			try
			{
				using (((ICurrentDbControl)testConn).UseDatabase(dbNameForCreationContext))
				{
					dbCreator.CreateDropExisting(testConn);
				}
				AssertEquals(testDbName + "DB created?" + testDbName, true, DbObjectCreator.DatabaseExists(testConn, testDbName));
			}
			finally
			{
				dbCreator.Drop(testConn);
				DbCreatorTestUtils.DropConflictFiles(testConn, testDbName);
			}
		}

		public void TestCreateDbHandlesExistingFileConflict_WithTwoLogFiles()
		{
			string testDbName = "TestEmptyDbCreatorWith2LogFiles750740288C944719A34BB5E3EACABD3C";
			DbCreatorTestUtils.CreateConflictFiles(testConn, testDbName);

			string currentDbFileFolder = DbCreatorTestUtils.GetCurrentDbFileFolder();
			IDbCreator dbCreator = new EmptyDbCreatorWithTwoLogFilesForTest(testDbName, currentDbFileFolder);

			try
			{
				dbCreator.CreateDropExisting(testConn);
				AssertEquals(testDbName + "DB created?" + testDbName, true, DbObjectCreator.DatabaseExists(testConn, testDbName));
			}
			finally
			{
				dbCreator.Drop(testConn);
				DbCreatorTestUtils.DropConflictFiles(testConn, testDbName);
			}
		}

		[TestRequiresAdministrativePrivileges("Lock mdf file under program files")]
		public void TestCreateDbHandlesExistingFileConflict_FileLocked()
		{
			var testDbName = "TestEmptyDbCreatorFileLocked750740288C944719A34BB5E3EACABD3C";
			var files = DbCreatorTestUtils.CreateConflictFiles(testConn, testDbName);

			try
			{
				using (File.Open(files[0], FileMode.Open, FileAccess.ReadWrite, FileShare.None))
				{
					var currentDbFileFolder = DbCreatorTestUtils.GetCurrentDbFileFolder();
					IDbCreator dbCreator = new EmptyDbCreatorForTesting(testDbName, currentDbFileFolder);
					var ex = AssertExceptionThrown<AggregateException>(() => dbCreator.CreateDropExisting(testConn));
					AssertEquals(DbErrorType.CannotCreateFileBecauseItAlreadyExists, new DbErrorMatch((SqlException)ex.InnerException).ExceptionType);
				}
			}
			finally
			{
				DbCreatorTestUtils.DropConflictFiles(testConn, testDbName);
			}
		}

		public void TestCreateDbDeleteFileProcedureDoesNotExist()
		{
			string testDbName = "TestEmptyDbCreatorWith2LogFiles750740288C944719A34BB5E3EACABD3C";
			var dbFileDirectory = DbCreatorTestUtils.GetCurrentDbFileFolder();
			IDbCreator dbCreator = new EmptyDbCreatorWithInvalidDeleteProcNameForTesting(testDbName, dbFileDirectory);

			try
			{
				DbCreatorTestUtils.CreateConflictFiles(testConn, testDbName);

				var ex = AssertExceptionThrown<AggregateException>(() => dbCreator.CreateIfNotExists(testConn));
				AssertEquals(DbErrorType.CannotCreateFileBecauseItAlreadyExists, new DbErrorMatch((SqlException)ex.InnerException).ExceptionType);
			}
			finally
			{
				dbCreator.Drop(testConn);
				DbCreatorTestUtils.DropConflictFiles(testConn, testDbName);
			}
		}

		protected string GetDbCollation(string dbName)
		{
			string sqlText = String.Format("SELECT databasepropertyex('{0}', 'Collation')", dbName);
			return Db.Connection.ExecuteScalar(sqlText).ToString();
		}

		readonly AdminConnection testConn = Db.NewAdminConnection();

		protected override void TearDown()
		{
			((IDisposable)testConn).Dispose();
		}
	}

	public class EmptyDbCreatorForTesting : EmptyDbCreator
	{
		public EmptyDbCreatorForTesting(string dbName, string dbFileFolder)
			: base(dbName)
		{
			this.dbFileFolder = dbFileFolder;
		}

		protected override void CreateDbCore(AdminConnection conn)
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture, @"
				CREATE DATABASE {0} 
					ON
					(NAME = {0}, FILENAME = '{1}\{0}_Data.mdf')
					LOG ON 
					(NAME = {0}Log1, FILENAME = '{1}\{0}_Log1.ldf')",
				dbName, dbFileFolder);

			conn.ExecuteNonQuery(sqlText);
		}

		protected readonly string dbFileFolder;
	}

	public class EmptyDbCreatorWithTwoLogFilesForTest : EmptyDbCreatorForTesting
	{
		public EmptyDbCreatorWithTwoLogFilesForTest(string dbName, string dbFileFolder)
			: base(dbName, dbFileFolder)
		{
		}

		protected override void CreateDbCore(AdminConnection conn)
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture, @"
				CREATE DATABASE {0} 
					ON
					(NAME = {0}, FILENAME = '{1}\{0}_Data.mdf')
					LOG ON 
					(NAME = {0}Log1, FILENAME = '{1}\{0}_Log1.ldf'),
					(NAME = {0}Log2, FILENAME = '{1}\{0}_Log2.ldf')",
				dbName, dbFileFolder);

			conn.ExecuteNonQuery(sqlText);
		}
	}

	class EmptyDbCreatorWithInvalidDeleteProcNameForTesting : EmptyDbCreatorForTesting
	{
		public EmptyDbCreatorWithInvalidDeleteProcNameForTesting(string dbName, string dbFileFolder)
			: base(dbName, dbFileFolder)
		{
		}

		protected override string DeleteFileProcName
		{
			get { return "NonExisistingDelteProc9F68D860048D4AEFAB3419518CDF31B2"; }
		}
	}

	public static class DbCreatorTestUtils
	{
		public static string GetCurrentDbFileFolder()
		{
			string sqlText = "SELECT TOP 1 physical_name FROM sys.database_files";
			string currentDbFileFullName = Db.Connection.ExecuteScalar(sqlText)?.ToString().Trim() ?? string.Empty;

			return Path.GetDirectoryName(currentDbFileFullName);
		}

		public static string[] CreateConflictFiles(DbConnection connection, string testDbName)
		{
			string dbFileFolder = GetCurrentDbFileFolder();

			var tempFilePath = Path.Combine(NUnit.Framework.TempForTest.TempPath, NUnit.Framework.TempForTest.GetTempFileName());
			try
			{
				using (File.Create(tempFilePath))
				{ }

				string testDataFileName = String.Format(@"{0}\{1}_Data.mdf", dbFileFolder, testDbName);
				string sqlText = String.Format(@"EXEC CLRCopyFile '{0}', '{1}'", tempFilePath, testDataFileName);
				connection.ExecuteNonQuery(sqlText);

				string testLog1FileName = String.Format(@"{0}\{1}_Log1.ldf", dbFileFolder, testDbName);
				sqlText = String.Format(@"EXEC CLRCopyFile '{0}', '{1}'", tempFilePath, testLog1FileName);
				connection.ExecuteNonQuery(sqlText);

				string testLog2FileName = String.Format(@"{0}\{1}_Log2.ldf", dbFileFolder, testDbName);
				sqlText = String.Format(@"EXEC CLRCopyFile '{0}', '{1}'", tempFilePath, testLog2FileName);
				connection.ExecuteNonQuery(sqlText);

				return new[] { testDataFileName, testLog1FileName, testLog2FileName };
			}
			finally
			{
				File.Delete(tempFilePath);
			}
		}

		public static void DropConflictFiles(DbConnection connection, string testDbName)
		{
			string dbFileFolder = GetCurrentDbFileFolder();

			string testDataFileName = String.Format(@"{0}\{1}_Data.mdf", dbFileFolder, testDbName);
			string sqlText = String.Format(@"EXEC CLRDeleteFile '{0}', '1'", testDataFileName);
			connection.ExecuteNonQuery(sqlText);

			string testLog1FileName = String.Format(@"{0}\{1}_Log1.ldf", dbFileFolder, testDbName);
			sqlText = String.Format(@"EXEC CLRDeleteFile '{0}', '1'", testLog1FileName);
			connection.ExecuteNonQuery(sqlText);

			string testLog2FileName = String.Format(@"{0}\{1}_Log2.ldf", dbFileFolder, testDbName);
			sqlText = String.Format(@"EXEC CLRDeleteFile '{0}', '1'", testLog2FileName);
			connection.ExecuteNonQuery(sqlText);
		}
	}
}
