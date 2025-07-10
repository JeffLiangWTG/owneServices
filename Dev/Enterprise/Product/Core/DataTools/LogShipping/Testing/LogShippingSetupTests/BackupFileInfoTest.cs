using System;
using System.IO;
using CargoWise.Data;
using Enterprise.LogShipping.Setup;
using NUnit.Framework;

namespace Enterprise.LogShipping.Testing
{
	sealed class BackupFileInfoTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetBackupFileInfoArrayWhenSecondaryDatabaseDoesNotExist()
		{
			using (var dbTestHelper = new DbTestHelper())
			{
				var info = new LogShippingInfo();
				info.BackupSourceDirectory = "Z:\\SourceFolder";
				info.MainDatabase = GetTestMainDbInfo(info, dbTestHelper.GetTestBackupFile(Db.Connection.ServerVersionNumber));

				var sqlMasterDbDataFolder = dbTestHelper.GetDatabasePhysicalPath(Db.SqlMasterDb);
				var sqlMasterDbLogFolder = dbTestHelper.GetDatabasePhysicalPath(Db.SqlMasterDb);
				AssertBackupFileArray(dbTestHelper, info, sqlMasterDbDataFolder, sqlMasterDbLogFolder);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetBackupFileInfoArrayWithFolderOverride()
		{
			using (var dbTestHelper = new DbTestHelper())
			{
				var info = new LogShippingInfo();
				info.BackupSourceDirectory = "Z:\\SourceFolder";
				info.MainDatabase = GetTestMainDbInfo(info, dbTestHelper.GetTestBackupFile(Db.Connection.ServerVersionNumber));

				var sqlMasterDbDataFolder = dbTestHelper.GetDatabasePhysicalPath(Db.SqlMasterDb);
				var sqlMasterDbLogFolder = dbTestHelper.GetDatabasePhysicalPath(Db.SqlMasterDb);
				AssertBackupFileArray(dbTestHelper, info, sqlMasterDbDataFolder, sqlMasterDbLogFolder);

				info.RestoreLogDirectoryOverride = "Z:\\" + Guid.NewGuid().ToString();
				AssertBackupFileArray(dbTestHelper, info, sqlMasterDbDataFolder, info.RestoreLogDirectoryOverride);

				info.RestoreDataDirectoryOverride = "Z:\\" + Guid.NewGuid().ToString();
				AssertBackupFileArray(dbTestHelper, info, info.RestoreDataDirectoryOverride, info.RestoreLogDirectoryOverride);
			}
		}

		void AssertBackupFileArray(DbTestHelper dbTestHelper, LogShippingInfo info, string expectedDataFolder, string expectedLogFolder)
		{
			var sqlTestConnection = ((IDbConnectionInternals)dbTestHelper.TestConnection).ADOConnection;
			var backupFiles = BackupFileInfo.GetBackupFileInfoArray(sqlTestConnection, info.MainDatabase);
			AssertEquals("Count of database files", 2, backupFiles.Length);
			AssertEquals("First file (data): File Type", "D", backupFiles[0].Type);
			AssertEquals("First file (data): LogicalName", "TestDatabase_Data", backupFiles[0].LogicalName);
			AssertEquals("First file (data): PhysicalName", Path.Combine(expectedDataFolder, info.MainDatabase.SecondaryDatabaseName + "_Data.mdf"), backupFiles[0].PhysicalName);
			AssertEquals("Second file (log): File Type", "L", backupFiles[1].Type);
			AssertEquals("Second file (log): LogicalName", "TestDatabase_Log", backupFiles[1].LogicalName);
			AssertEquals("Second file (log): PhysicalName", Path.Combine(expectedLogFolder, info.MainDatabase.SecondaryDatabaseName + "_Log.ldf"), backupFiles[1].PhysicalName);
		}

		MainDatabaseInfoForTesting GetTestMainDbInfo(LogShippingInfo info, string testBackupFile)
		{
			var databaseInfo = new MainDatabaseInfoForTesting(info, "PrimaryDb");
			databaseInfo.BackupFileName = Path.Combine(BaseSourcePath, testBackupFile);
			databaseInfo.SecondaryDatabaseExistValue = false;
			return databaseInfo;
		}
	}
}
