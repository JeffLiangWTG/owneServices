using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.BrandManager;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Testing
{
	sealed class DocManagerDbHelperMultipleDBsTransactionedTest : TransactionedTestCase
	{
		public void TestHandleSqlExceptionLogs()
		{
			var dbName = string.Empty;
			var log = new StringBuilder();
			var dbHelper = new DocManagerDBHelperTestClassForDbException();
			try
			{
				var dbNumber = dbHelper.LastWritableDatabaseWithFreeSpace();
				dbName = dbHelper.GetDatabaseName(dbNumber);

				dbHelper.LastWritableDatabaseWithFreeSpace(0, false, log);

				AssertStartsWith("Should log SqlException", "SqlException occurred while trying to access database", log.ToString());
			}
			finally
			{
				using (var dropDbConnection = Db.NewAdminConnection())
				{
					DropTestDbIfExists(dropDbConnection, dbName);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestCreateDocManagerDatabaseExceptionReportedWhenLogOrDataPathIsInvalidInRegistry()
		{
			var helper = new DocManagerDBHelperTestClass();
			var db1Name = helper.GetDatabaseName(1);
			var folderNotExist = Path.Combine(Temp.TempPathWithoutCreating, Guid.NewGuid().ToString());
			using (var adminConnection = Db.NewAdminConnection())
			{
				var db1Exist = adminConnection.DatabaseExists(db1Name);
				if (!db1Exist)
				{
					helper.CreateDatabase(1);
				}

				try
				{
					using (SystemDataRegistry.Instance.DocManagerDBLogFilePath.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, folderNotExist))
					{
						adminConnection.AlterDbWriteableStateForDocManager(db1Name, false);
						AssertExceptionThrown<CreateDocManagerDatabaseException>(() => helper.LastWritableDatabaseWithFreeSpace());
					}
				}
				finally
				{
					if (db1Exist)
					{
						adminConnection.AlterDbWriteableStateForDocManager(db1Name, true);
					}
					else
					{
						AdoTestUtils.DropDbIfExists(adminConnection, db1Name);
					}
				}
			}
		}

		public void TestLastCreationMessageShouldBeReportedIfCouldNotAcquireSqllock()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			{
				Assert(connection1.TryGetLock(MutexIDs.StorageDocsDBBeingCreated.Name, out var dbLock));
				Assert(dbLock.IsHoldingLock());

				using (var connection2 = Db.NewAdminConnection())
				{
					var helper = new DocManagerDBHelperTestClassUsingRegistry();
					var dbName = helper.GetDatabaseName(888);
					helper.CreateDatabase(888);
					AssertEquals($"Could not acquire lock for creating database {dbName}.", helper.GetLastCreationExceptionArray()[0]);

					DropTestDbIfExists(connection2, dbName);
				}

				dbLock.Dispose();
			}
		}

		[ExpectNoExceptions]
		public void TestDatabaseBackupDoesNotBlowUpIfNoRecipients()
		{
			string sqlText = GetSqlForPath(Db.DatabaseName, false);
			string existingLogPath = (string)Db.Connection.ExecuteScalar(sqlText); // Need to use db.connection; can't use factory to get any of this information

			Registry.Business.SystemDataRegistry.Instance.DocManagerDBDataFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Temp.TempPath);
			Env.Registry.BackupDirectoryPath = "";

			DocManagerDBHelperTestClass dBUtilitiesTestClass = new DocManagerDBHelperTestClass();

			using (var testConnection = Db.NewAdminConnection())
			{
				string testDbName = (new DocManagerDBHelper()).GetDatabaseName(10);
				string backupfile = string.Empty;
				try
				{
					DropTestDbIfExists(testConnection, testDbName);
					dBUtilitiesTestClass.CreateDatabase(10);
				}
				finally
				{
					if (!string.IsNullOrEmpty(backupfile))
					{
						TempFile.Delete(backupfile);
					}
					DropTestDbIfExists(testConnection, testDbName);
				}
			}
		}

		public void TestDatabaseBackupIsNotCreatedWhenFolderNotSpecified()
		{
			string sqlText = GetSqlForPath(Db.DatabaseName, false);
			string existingLogPath = (string)Db.Connection.ExecuteScalar(sqlText); // Need to use db.connection; can't use factory to get any of this information

			Registry.Business.SystemDataRegistry.Instance.DocManagerDBDataFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Temp.TempPath);
			Env.Registry.BackupDirectoryPath = "";
			GlbStaff staff = new BusinessObjectFactory().New<GlbStaff>();
			staff.GS_LoginName = "login";
			staff.GS_Code = "USR";
			staff.GS_EmailAddress = "notexistantemail@cargowise.com";
			GlbGroup pmg = staff.Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			staff.Groups.Add(pmg);
			staff.Factory.Save();

			DocManagerDBHelperTestClass dBUtilitiesTestClass = new DocManagerDBHelperTestClass();

			using (var testConnection = Db.NewAdminConnection())
			{
				string testDbName = (new DocManagerDBHelper()).GetDatabaseName(10);
				string backupfile = string.Empty;

				try
				{
					AssertEquals("pre condition", 0, Env.OutgoingMailManager.EmailsCreated.Count);
					DropTestDbIfExists(testConnection, testDbName);
					dBUtilitiesTestClass.CreateDatabase(10);

					string[] files = Directory.GetFiles(Temp.TempPath);
					bool exists = false;
					foreach (string file in files)
					{
						if (file.Contains(".bak"))
						{
							backupfile = file;
							exists = true;
						}
					}
					Assert("Backup should not have been created", !exists);
					AssertEquals("Error email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
					AssertEquals("Should contain correct message", "New database created " + testDbName + ". Please include it in your database maintenance plan, unless backups are performed by " + BrandingFactory.Instance.ProductName + " Service Tasks.", Env.OutgoingMailManager.EmailsCreated[0].Body);
				}
				finally
				{
					if (!string.IsNullOrEmpty(backupfile))
					{
						TempFile.Delete(backupfile);
					}
					DropTestDbIfExists(testConnection, testDbName);
				}
			}
		}

		public void TestDatabaseBackupIsCreated()
		{
			string sqlText = GetSqlForPath(Db.DatabaseName, false);
			string existingLogPath = (string)Db.Connection.ExecuteScalar(sqlText); // Need to use db.connection; can't use factory to get any of this information

			Registry.Business.SystemDataRegistry.Instance.DocManagerDBDataFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Temp.TempPath);
			Env.Registry.BackupDirectoryPath = Temp.TempPath;
			DocManagerDBHelperTestClass dBUtilitiesTestClass = new DocManagerDBHelperTestClass();

			using (var testConnection = Db.NewAdminConnection())
			{
				string testDbName = (new DocManagerDBHelper()).GetDatabaseName(10);
				string backupfile = string.Empty;
				try
				{
					DropTestDbIfExists(testConnection, testDbName);
					dBUtilitiesTestClass.CreateDatabase(10);

					string[] files = Directory.GetFiles(Temp.TempPath);
					bool exists = false;
					foreach (string file in files)
					{
						if (file.Contains(".bak"))
						{
							backupfile = file;
							exists = true;
						}
					}
					Assert("Backup should have been created", exists);
				}
				finally
				{
					if (!string.IsNullOrEmpty(backupfile))
					{
						TempFile.Delete(backupfile);
					}
					DropTestDbIfExists(testConnection, testDbName);
				}
			}
		}

		public void TestDatabaseSize()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				string testDbName = (new DocManagerDBHelper()).GetDatabaseName(10);
				var testClass = new DocManagerDBHelperTestClassUsingRegistry();

				int dbNumber = testClass.CreateDatabase(10);
				if (dbNumber != 10)
				{
					Assert(string.Format("failed to create database with error: {0}", string.Join("\r\n\r\n", testClass.GetLastCreationExceptionArray().ToArray())), false);// Check the reason and fix in new WI
				}
				var size = (int)testConnection.ExecuteScalar("select sum(size) * 8192 / 1024 / 1024 from " + testDbName + ".sys.database_files where data_space_id = 1");
				DropTestDbIfExists(testConnection, testDbName);
				AssertEquals("Db Size in megabytes", 1024, size);
			}
		}

		public void TestDatabaseLogFileSizeAndAutoGrowthSize()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				string testDbName = (new DocManagerDBHelper()).GetDatabaseName(10);
				var testClass = new DocManagerDBHelperTestClassUsingRegistry();

				int dbNumber = testClass.CreateDatabase(10);
				if (dbNumber != 10)
				{
					Assert(string.Format("failed to create database with error: {0}", string.Join("\r\n\r\n", testClass.GetLastCreationExceptionArray().ToArray())), false);// Check the reason and fix in new WI
				}
				var size = (int)testConnection.ExecuteScalar("select sum(size) * 8192 / 1024 / 1024 from " + testDbName + ".sys.database_files where data_space_id = 0");
				AssertEquals("Db Log File Size in megabytes", 500, size);

				size = (int)testConnection.ExecuteScalar("select sum(growth) * 8192 / 1024 / 1024 from " + testDbName + ".sys.database_files where data_space_id = 0");
				AssertEquals("Db Log File Size in megabytes", 200, size);

				DropTestDbIfExists(testConnection, testDbName);
			}
		}

		public void TestDatabaseAutoGrowthSize()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				string testDbName = (new DocManagerDBHelper()).GetDatabaseName(10);
				var testClass = new DocManagerDBHelperTestClassUsingRegistry();

				int dbNumber = testClass.CreateDatabase(10);
				if (dbNumber != 10)
				{
					Assert(string.Format("failed to create database with error: {0}", string.Join("\r\n\r\n", testClass.GetLastCreationExceptionArray().ToArray())), false);// Check the reason and fix in new WI
				}
				var size = (int)testConnection.ExecuteScalar("select sum(growth/128) from " + testDbName + ".sys.database_files where data_space_id = 1");
				DropTestDbIfExists(testConnection, testDbName);
				AssertEquals("Db autogrowth Size in megabytes", 500, size);
			}
		}

		public void TestDatabaseFilesCreatedInCorrectLocation()
		{
			string sqlText = GetSqlForPath(Db.DatabaseName, true);
			string mainDbDataFilePath = (string)Db.Connection.ExecuteScalar(sqlText); // Need to use db.connection; can't use factory to get any of this information

			sqlText = GetSqlForPath(Db.DatabaseName, false);
			string mainDbLogFilePath = (string)Db.Connection.ExecuteScalar(sqlText); // Need to use db.connection; can't use factory to get any of this information

			Registry.Business.SystemDataRegistry.Instance.DocManagerDBDataFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Temp.TempPath);
			DocManagerDBHelperTestClass dBUtilitiesTestClass = new DocManagerDBHelperTestClass();

			using (var testConnection = Db.NewAdminConnection())
			{
				var sd010 = (new DocManagerDBHelper()).GetDatabaseName(10);
				var sd011 = (new DocManagerDBHelper()).GetDatabaseName(11);
				var sd012 = (new DocManagerDBHelper()).GetDatabaseName(12);
				var newDataPath = Temp.TempPath + "DataDir\\";
				var newLogPath = Temp.TempPath + "LogLocation\\";

				try
				{
					DropTestDbIfExists(testConnection, sd010);
					dBUtilitiesTestClass.CreateDatabase(10);

					sqlText = GetSqlForPath(sd010, true);
					string sd010DataPath = (string)testConnection.ExecuteScalar(sqlText);
					sqlText = GetSqlForPath(sd010, false);
					string sd010LogPath = (string)testConnection.ExecuteScalar(sqlText);

					AssertEquals("Data file path is in the new location - Temp path", Temp.TempPath, sd010DataPath);
					AssertNotEquals("Log file is not in the old location same as the main DB", mainDbLogFilePath, sd010LogPath);

					Registry.Business.SystemDataRegistry.Instance.DocManagerDBDataFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
					Registry.Business.SystemDataRegistry.Instance.DocManagerDBLogFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

					DropTestDbIfExists(testConnection, sd011);
					dBUtilitiesTestClass.CreateDatabase(11);
					sqlText = GetSqlForPath(sd011, true);
					string sd011DataPath = (string)testConnection.ExecuteScalar(sqlText);
					sqlText = GetSqlForPath(sd011, false);
					string sd011LogPath = (string)testConnection.ExecuteScalar(sqlText);

					AssertEquals("Data file path for SD011 is the same as for SD010", sd010DataPath.ToUpper(), sd011DataPath.ToUpper());
					AssertEquals("Log file path for SD011 is the same as for SD010", sd010LogPath.ToUpper(), sd011LogPath.ToUpper());

					if (!Directory.Exists(newDataPath))
					{
						Directory.CreateDirectory(newDataPath);
					}

					if (!Directory.Exists(newLogPath))
					{
						Directory.CreateDirectory(newLogPath);
					}
					Registry.Business.SystemDataRegistry.Instance.DocManagerDBDataFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newDataPath);
					Registry.Business.SystemDataRegistry.Instance.DocManagerDBLogFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newLogPath);

					DropTestDbIfExists(testConnection, sd012);
					dBUtilitiesTestClass.CreateDatabase(12);
					sqlText = GetSqlForPath(sd012, true);
					string sd012DataPath = (string)testConnection.ExecuteScalar(sqlText);
					sqlText = GetSqlForPath(sd012, false);
					string sd012LogPath = (string)testConnection.ExecuteScalar(sqlText);

					AssertEquals("Data file path is in the new data location ", newDataPath.ToUpper(), sd012DataPath.ToUpper());
					AssertEquals("Log file path is in the new log location ", newLogPath.ToUpper(), sd012LogPath.ToUpper());
				}
				finally
				{
					DropTestDbIfExists(testConnection, sd010);
					DropTestDbIfExists(testConnection, sd011);
					DropTestDbIfExists(testConnection, sd012);

					if (Directory.Exists(newDataPath))
					{
						Directory.Delete(newDataPath);
					}

					if (Directory.Exists(newLogPath))
					{
						Directory.Delete(newLogPath);
					}
				}
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestCreateDatabaseAppliesUserRolesToRelevantUsers()
		{
			// LLZ: This test will become irrelevant for when UseModernSqlSecuritySystem flag is on
			// Once all of the self-healing is replaced with a nudge to DSA or removed
			// Hence, once UseModernSqlSecuritySystem is removed this test will have to be removed
			// This would mean that RefreshDbReaderRolePermissionsOnCreationOfDatabase function is removed as well
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
			var factory = new BusinessObjectFactory();
			var staff1 = factory.NewWithValidTestData<GlbStaff>();
			staff1.IsReadOnlyDBUser = true;
			factory.Save();

			var dBUtilitiesTestClass = new DocManagerDBHelperTestClass();
			dBUtilitiesTestClass.RefreshDbReaderRolePermissionsOnCreationOfDatabase();
			using (var testConnection = Db.NewAdminConnection())
			{
				string testDbName = (new DocManagerDBHelper()).GetDatabaseName(10);
				string backupfile = string.Empty;
				try
				{
					DropTestDbIfExists(testConnection, testDbName);
					dBUtilitiesTestClass.CreateDatabase(10);
					AssertEquals("Creation of Doc DB should update user roles", true, DbUserManagerForTesting.CheckUserHasRightsOnDb(testDbName, staff1.GS_LoginName, DbRoleTypes.CwRestrictedReaderRole, testConnection));
				}
				finally
				{
					if (!string.IsNullOrEmpty(backupfile))
					{
						TempFile.Delete(backupfile);
					}
					DropTestDbIfExists(testConnection, testDbName);
				}
			}
		}

		public void TestNoExceptionIfLogExists()
		{
			var dbUtilitiesTestClass = new DocManagerDBHelperTestClass();
			string testDbName = dbUtilitiesTestClass.GetDatabaseName(10);

			using (var tempoDirectory = new TempDirectory())
			using (var testConnection = Db.NewAdminConnection())
			using (SystemDataRegistry.Instance.DocManagerDBLogFilePath.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempoDirectory.DirectoryName))
			{
				string path = Path.Combine(tempoDirectory.DirectoryName, testDbName + "_Log.ldf");
				File.Create(path).Close();

				try
				{
					DropTestDbIfExists(testConnection, testDbName);
					int createDbResult = dbUtilitiesTestClass.CreateDatabase(10);
					AssertEquals("Create DB error should be handled, returning a negative result.", -1, createDbResult);
				}
				finally
				{
					DropTestDbIfExists(testConnection, testDbName);
				}
			}
		}

		public void TestThrowsExceptionIfCantCreateSDDatabase()
		{
			var dbUtilitiesTestClass = new DocManagerDBHelperTestClass();

			using (var testConnection = Db.NewAdminConnection())    //So SD001 doesn't get in the way
			{
				var name = dbUtilitiesTestClass.GetDatabaseName(DocManagerDBHelperTestClass.InitialStorageDocsDatabaseNumber);
				DropTestDbIfExists(testConnection, name);
			}
			var number = dbUtilitiesTestClass.GetStorageDocDbNumbersIncludingMainDb().Count();
			var testDbNames = new List<string> {
			dbUtilitiesTestClass.GetDatabaseName(number),
			dbUtilitiesTestClass.GetDatabaseName(number + 1),
			dbUtilitiesTestClass.GetDatabaseName(number + 2),
			dbUtilitiesTestClass.GetDatabaseName(number + 3),
			dbUtilitiesTestClass.GetDatabaseName(number + 4)
			};

			var factory = new BusinessObjectFactory();
			var documentFactory = new DbBackendDocumentFactory(factory);
			var docCountBefore = documentFactory.Load<StorageDocs>(new ZQuery()).Length;

			using (var tempoDirectory = new TempDirectory())
			using (var testConnection = Db.NewAdminConnection())
			using (SystemDataRegistry.Instance.DocManagerDBLogFilePath.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempoDirectory.DirectoryName))
			{
				var paths = testDbNames.Select((testDbName) => Path.Combine(tempoDirectory.DirectoryName, testDbName + "_Log.ldf")).ToList();
				paths.ForEach((path) => File.Create(path).Close());
				DateTime before = DateTime.Now;

				try
				{
					testDbNames.ForEach((testDbName) => DropTestDbIfExists(testConnection, testDbName));
					dbUtilitiesTestClass.TrimThresholdtoZero = true;
					var exception = AssertExceptionThrown<CanNotCreateSDDatabaseException>(() => { dbUtilitiesTestClass.LastWritableDatabaseWithFreeSpace(0, true); });
					DateTime after = DateTime.Now;
					Assert(after - before > new TimeSpan(0, 0, 2));
					AssertMultilineASCIIEquals("should show original message",
$@"Databases with Numbers = (1, 2, 3, 4, 5) cannot be created. Please try in few minutes or contact your system administrator.
Last Error(s):
Cannot create file '{paths.Last()}' because it already exists. Change the file path or the file name, and retry the operation.
CREATE DATABASE failed. Some file names listed could not be created. Check related errors.
Additional information:
'Count of document databases': {number - 1};
{dbUtilitiesTestClass.GetDatabaseName(1)} Exists: False
{dbUtilitiesTestClass.GetDatabaseName(2)} Exists: False
{dbUtilitiesTestClass.GetDatabaseName(3)} Exists: False
{dbUtilitiesTestClass.GetDatabaseName(4)} Exists: False
{dbUtilitiesTestClass.GetDatabaseName(5)} Exists: False
Create DocManager Database: ;
DB log File Path: {tempoDirectory.DirectoryName};
Db Data File Path: {Temp.TempPath};",
					 exception.Message);
				}
				finally
				{
					testDbNames.ForEach((testDbName) => DropTestDbIfExists(testConnection, testDbName));
				}
			}

			var docCountAfter = documentFactory.Load<StorageDocs>(new ZQuery()).Length;
			AssertEquals("Documents were not inserted into main database when failing to create SD database", docCountBefore, docCountAfter);
		}

		public void TestGetStorageDocDbNumbersIncludingMainDb()
		{
			DocManagerDBHelperTestClass dBUtilitiesTestClass = new DocManagerDBHelperTestClass();
			dBUtilitiesTestClass.CreateDatabase(11);
			dBUtilitiesTestClass.CreateDatabase(12);
			try
			{
				var existingDBNumbers = dBUtilitiesTestClass.GetStorageDocDbNumbersIncludingMainDb();
				AssertCollectionContains("Should contain database that exists", 11, existingDBNumbers);
				AssertCollectionContains("Should contain database that exists", 12, existingDBNumbers);
				AssertCollectionNotContains("Should not contain database that doesn't exist", 13, existingDBNumbers);
			}
			finally
			{
				dBUtilitiesTestClass.DropDatabase(dBUtilitiesTestClass.GetDatabaseName(11));
				dBUtilitiesTestClass.DropDatabase(dBUtilitiesTestClass.GetDatabaseName(12));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:Do Not Cast Factory Method", Justification = "There is no generic")]
		public void TestGetLastWritableDatabaseWithFreeSpace()
		{
			var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var helper = new DocManagerDbHelperWithLowDatabaseLimit();

			var db1Name = helper.GetDatabaseName(1);
			var db2Name = helper.GetDatabaseName(2);
			TestCaseHelper.ClearTable(db1Name + ".dbo." + StorageDocsSchema.Constants.TableName);
			Assert("Precondition: Db " + db2Name + " shouldn't exist", !helper.DatabaseExists(2));

			try
			{
				var factoryOne = masterFactory.GetFactory(1);
				var doc = (StorageDocs)factoryOne.NewWithParent(typeof(StorageDocs));
				doc.SC_ImageData = TestTiffBytes;
				doc.ParentMain.SM_DB = 1;
				masterFactory.Save();

				var nextDb = helper.LastWritableDatabaseWithFreeSpace();
				AssertEquals("NextDb should be number 1", 1, nextDb);

				helper.SetMinimumDatabaseFileSizeLimitInMb(1);
				nextDb = helper.LastWritableDatabaseWithFreeSpace();
				AssertEquals("NextDb should be number 1", 1, nextDb);

				using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.DB))
				{
					nextDb = helper.LastWritableDatabaseWithFreeSpace(10, true);
					AssertEquals("NextDb should be number 2 - db 1 does not have enough space more than 1Mb so should return new db", 2, nextDb);
					AssertEquals("Db " + db2Name + " exists?", true, helper.DatabaseExists(2));
					AssertEquals("Auto create statistics for DB = " + db2Name, false, TestConnection.IsDbAutoCreateStats(db2Name));
				}

				using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
				{
					nextDb = helper.LastWritableDatabaseWithFreeSpace(10, true);
					AssertEquals("NextDb should be number 1", 1, nextDb);
				}
			}
			finally
			{
				using (var dropDbConnection = Db.NewAdminConnection())
				{
					TestCaseHelper.ClearTable(db1Name + ".dbo." + StorageDocsSchema.Constants.TableName);
					DropTestDbIfExists(dropDbConnection, db2Name);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:Do Not Cast Factory Method", Justification = "There is no generic")]
		public void TestGetLastDatabaseWithFreeSpaceBySpecifyingRequiredSize()
		{
			var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var helper = new DocManagerDbHelperWithLowDatabaseLimit();

			var db1Name = helper.GetDatabaseName(1);
			var db2Name = helper.GetDatabaseName(2);

			TestCaseHelper.ClearTable(db1Name + ".dbo." + StorageDocsSchema.Constants.TableName);
			Assert("Precondition: Db " + db2Name + " shouldn't exist", !helper.DatabaseExists(2));

			try
			{
				var factoryOne = masterFactory.GetFactory(1);
				var doc = (StorageDocs)factoryOne.NewWithParent(typeof(StorageDocs));
				doc.SC_ImageData = TestTiffBytes;
				doc.ParentMain.SM_DB = 1;
				masterFactory.Save();

				var nextDb = helper.LastWritableDatabaseWithFreeSpace();
				AssertEquals("NextDb should be number 1", 1, nextDb);

				nextDb = helper.LastWritableDatabaseWithFreeSpace(10);
				AssertEquals("NextDb should be number 1", 1, nextDb);

				helper.SetMinimumDatabaseFileSizeLimitInMb(10);

				nextDb = helper.LastWritableDatabaseWithFreeSpace(5);
				AssertEquals("NextDb should be number 1", 1, nextDb);

				nextDb = helper.LastWritableDatabaseWithFreeSpace(10);
				AssertEquals("NextDb should be number 1 because we're not force creating", 1, nextDb);

				using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.DB))
				{
					nextDb = helper.LastWritableDatabaseWithFreeSpace(10, true);
					AssertEquals("NextDb should be number 2 when EDocsStorageProvider setting to DB", 2, nextDb);
					AssertEquals("Db " + db2Name + " exists?", true, helper.DatabaseExists(2));
					AssertEquals("Auto create statistics for DB = " + db2Name, false, TestConnection.IsDbAutoCreateStats(db2Name));
				}

				using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
				{
					nextDb = helper.LastWritableDatabaseWithFreeSpace(10, true);
					AssertEquals("NextDb should be number 1 when EDocsStorageProvider setting to S3", 1, nextDb);
				}
			}
			finally
			{
				using (var dropDbConnection = Db.NewAdminConnection())
				{
					TestCaseHelper.ClearTable(db1Name + ".dbo." + StorageDocsSchema.Constants.TableName);
					DropTestDbIfExists(dropDbConnection, db2Name);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestHandleDbInTransactionException()
		{
			string dbName = string.Empty;
			DocManagerDBHelperTestClassForDbException dbHelper = new DocManagerDBHelperTestClassForDbException();
			try
			{
				var dbNumber = dbHelper.LastWritableDatabaseWithFreeSpace();
				if (dbNumber == 0)
				{
					Assert(string.Format("failed to create database with error: {0}", UnitTestUserNotification.Instance.LastMessage.Text), false); // Check the reason and fix in new WI
				}

				dbName = dbHelper.GetDatabaseName(dbNumber);
			}
			finally
			{
				using (var dropDbConnection = Db.NewAdminConnection())
				{
					DropTestDbIfExists(dropDbConnection, dbName);
				}
			}
		}

		/// <summary>
		/// Gets the sql query for retrieving the database's data or log file.
		/// </summary>
		string GetSqlForPath(string dbName, bool isDataFile)
		{
			string fileType = isDataFile ? "0" : "1";

			string sqlText = string.Format(@"
				DECLARE @Path VARCHAR(max)
				SET @Path = (SELECT TOP 1 rtrim(physical_name) FROM {0}.sys.database_files WHERE [type] = {1})
				SET @Path = left(@Path, len(@Path) - charindex('\', reverse(@Path)) + 1)
				SELECT @Path",
				dbName, fileType);

			return sqlText;
		}

		void DropTestDbIfExists(AdminConnection dropDbConnection, string dbToDrop)
		{
			string sqlText = string.Format(
				"IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}')	DROP DATABASE [{0}]",
				dbToDrop);
			dropDbConnection.ExecuteNonQuery(sqlText);
		}

		class DocManagerDbHelperWithLowDatabaseLimit : DocManagerDBHelper
		{
			public DocManagerDbHelperWithLowDatabaseLimit()
			{
				SkipRefreshDbReaderRolePermissionsForTest = true;
			}

			public void SetMinimumDatabaseFileSizeLimitInMb(int limit)
			{
				fDatabaseFileSizeThresholdInMb = limit;
			}

			protected override int DatabaseFileSizeThresholdInMb
			{
				get { return fDatabaseFileSizeThresholdInMb; }
			}

			int fDatabaseFileSizeThresholdInMb = 1000;

			public void CreateDatabaseExposed(int newDBNumber)
			{
				CreateDatabase(newDBNumber);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			Registry.Business.SystemDataRegistry.Instance.DocManagerDBDataFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Temp.TempPath);
			Registry.Business.SystemDataRegistry.Instance.DocManagerDBLogFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Temp.TempPath);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		byte[] TestTiffBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Test.tif");

		protected override void MasterSetUp()
		{
			base.MasterSetUp();
			var helper = new DocManagerDBHelperTestClass();
			if (!helper.DatabaseExists(1))
			{
				helper.CreateDatabase(1);
			}
		}

		protected override void FinalTearDown()
		{
			base.FinalTearDown();
			var helper = new DocManagerDBHelperTestClass();
			if (helper.DatabaseExists(1))
			{
				helper.DropDatabase(helper.GetDatabaseName(1));
			}
		}
	}
}
