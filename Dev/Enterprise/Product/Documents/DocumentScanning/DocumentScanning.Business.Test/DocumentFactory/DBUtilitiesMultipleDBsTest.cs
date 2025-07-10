using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.IO;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Testing
{
	public class DBUtilitiesMultipleDBsTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestGetLastDatabaseWithFreeSpace_SD001IsCreatedWhenS3IsEnabled()
		{
			var dBUtilitiesTestClass = new DocManagerDBHelperTestClass();
			var initialDatabaseName = dBUtilitiesTestClass.GetDatabaseName(1);
			var secondDatabaseName = dBUtilitiesTestClass.GetDatabaseName(2);

			using (AdminConnection auxConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(auxConnection, initialDatabaseName);

				if (!dBUtilitiesTestClass.DatabaseExists(2))
				{
					dBUtilitiesTestClass.CreateDatabase(2);
				}

				AssertEquals("Precondition: Db " + initialDatabaseName + " shouldn't exist", false, dBUtilitiesTestClass.DatabaseExists(1));
				AssertEquals("Precondition: Db " + secondDatabaseName + " should exist", true, dBUtilitiesTestClass.DatabaseExists(2));

				using (SystemDataRegistry.Instance.DocManagerDBDataFilePath.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
				using (SystemDataRegistry.Instance.DocManagerDBLogFilePath.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
				{
					try
					{
						using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
						{
							dBUtilitiesTestClass.LastWritableDatabaseWithFreeSpace(0, true);

							AssertEquals("Db " + initialDatabaseName + " should exist", true,
								dBUtilitiesTestClass.DatabaseExists(1));
						}
					}
					finally
					{
						if (!dBUtilitiesTestClass.DatabaseExists(1))
						{
							dBUtilitiesTestClass.CreateDatabase(1);
						}

						AdoTestUtils.DropDbIfExists(auxConnection, secondDatabaseName);
					}
				}
			}
		}

		[ExpectNoExceptions()]
		[UseSnapshotProtection]
		public void TestDocumentDatabasesExistingIgnoresCase()
		{
			var dBUtilitiesTestClass = new DocManagerDBHelperTestClass();
			int originalDBCount = (dBUtilitiesTestClass.GetStorageDocDbNumbersIncludingMainDb()).Count();

			try
			{
				// need to create DB with lowercase name
				string sqlText = "create database " + Db.DatabaseName + "_sd123";
				using (DbConnection adminConnection = Db.NewAdminConnection())
				{
					adminConnection.ExecuteNonQuery(sqlText); // Need to use db.connection; can't use factory to create a db...
				}
				AssertEquals("Should be one more DB in the count of existing DBs", originalDBCount + 1, dBUtilitiesTestClass.GetStorageDocDbNumbersIncludingMainDb().Count());
			}
			finally
			{
				string sqlText = "drop database " + Db.DatabaseName + "_sd123";
				using (DbConnection adminConnection = Db.NewAdminConnection())
				{
					adminConnection.ExecuteNonQuery(sqlText); // Need to use db.connection; can't use factory to drop a db...
				}
			}
		}

		[UseSnapshotProtection]
		public void TestDatabasesCanBeCreatedAndDeleted()
		{
			DocManagerDBHelperTestClass dBUtilitiesTestClass = new DocManagerDBHelperTestClass();
			int originalDBCount = (dBUtilitiesTestClass.GetStorageDocDbNumbersIncludingMainDb()).Count();
			dBUtilitiesTestClass.CreateDatabase(originalDBCount + 1);
			try
			{
				AssertEquals(originalDBCount + 1, dBUtilitiesTestClass.GetStorageDocDbNumbersIncludingMainDb().Count());
			}
			finally
			{
				dBUtilitiesTestClass.DropDatabase((new DocManagerDBHelper()).GetDatabaseName(originalDBCount + 1));
			}
			AssertEquals(originalDBCount, dBUtilitiesTestClass.GetStorageDocDbNumbersIncludingMainDb().Count());
		}

		[UseSnapshotProtection]
		public void TestGetLastDatabaseWithFreeSpace_SD001_SD003_MakesSD002()
		{
			var dBUtilitiesTestClass = new DocManagerDBHelperTestClass();
			var helper = new DocManagerDBHelper();
			using (AdminConnection auxConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbIfNotExists(auxConnection, helper.GetDatabaseName(1));
				AdoTestUtils.DropDbIfExists(auxConnection, helper.GetDatabaseName(2));
				AdoTestUtils.DropDbIfExists(auxConnection, helper.GetDatabaseName(3));
			}
			dBUtilitiesTestClass.CreateDatabase(3);
			AssertEquals(3, dBUtilitiesTestClass.GetStorageDocDbNumbersIncludingMainDb().Count());
			AssertCollectionContains(0, dBUtilitiesTestClass.GetStorageDocDbNumbersIncludingMainDb());
			AssertCollectionContains(1, dBUtilitiesTestClass.GetStorageDocDbNumbersIncludingMainDb());
			AssertCollectionContains(3, dBUtilitiesTestClass.GetStorageDocDbNumbersIncludingMainDb());

			using (AdminConnection auxConnection = Db.NewAdminConnection())
			{
				try
				{
					auxConnection.AlterDbWriteableStateForDocManager(helper.GetDatabaseName(1), false);
					auxConnection.AlterDbWriteableStateForDocManager(helper.GetDatabaseName(3), false);

					int nextDb = helper.LastWritableDatabaseWithFreeSpace();
					AssertEquals("NextDb should be number 2", 2, nextDb);
					AssertEquals("DB2 writeable state", DbWriteableState.Writeable, helper.GetDbWriteableState(2));
				}
				finally
				{
					auxConnection.AlterDbWriteableStateForDocManager(helper.GetDatabaseName(1), true);
					AdoTestUtils.DropDbIfExists(auxConnection, helper.GetDatabaseName(2));
					AdoTestUtils.DropDbIfExists(auxConnection, helper.GetDatabaseName(3));
				}
			}
		}

		[UseSnapshotProtection]
		public void TestGetLastDatabaseWithFreeSpaceIgnoresReadOnlyDbs()
		{
			DocManagerDBHelper helper = new DocManagerDBHelper();

			string db1Name = helper.GetDatabaseName(1);
			string db2Name = helper.GetDatabaseName(2);

			using (AdminConnection auxConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbIfNotExists(auxConnection, db1Name);
				AdoTestUtils.DropDbIfExists(auxConnection, db2Name);
				AdoTestUtils.DropDbIfExists(auxConnection, helper.GetDatabaseName(3));

				int nextDb = helper.LastWritableDatabaseWithFreeSpace();
				AssertEquals("NextDb should be number 1", 1, nextDb);
				AssertEquals("DB1 writeable state", DbWriteableState.Writeable, helper.GetDbWriteableState(1));

				try
				{
					// Make DB1 read-only
					auxConnection.AlterDbWriteableStateForDocManager(db1Name, false);
					nextDb = helper.LastWritableDatabaseWithFreeSpace();
					if (nextDb == 0)
					{
						Assert(string.Format("failed to create database with error: {0}", UnitTestUserNotification.Instance.LastMessage.Text), false);// Check the reason and fix in new WI
					}
					var expectedNextDb = SystemDataRegistry.Instance.EDocsStorageProvider.Value == Core.Constants.EDocsStorageProviders.Code.DB ? 2 : 1;
					AssertEquals("Next DB should be #2. DB1 is read-only => new db is created and returned", expectedNextDb, nextDb);
					AssertEquals("DB1 writeable state", DbWriteableState.ReadOnly, helper.GetDbWriteableState(1));
					AssertEquals("DB2 writeable state", DbWriteableState.Writeable, helper.GetDbWriteableState(2));
				}
				finally
				{
					// Make DB1 writeable
					auxConnection.AlterDbWriteableStateForDocManager(db1Name, true);
					AdoTestUtils.DropDbIfExists(auxConnection, db2Name);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestStaffLoginRightsGrantedOnNewlyCreatedDatabase()
		{
			string testDbName = (new DocManagerDBHelper()).GetDatabaseName(237);
			string staffLogin1 = "TestStaffLogin9991";
			string staffLogin2 = "TestStaffLogin9992";
			var personPK = Guid.NewGuid();
			var testDbUserManager = new DbUserManagerForTesting();

			using (var auxConnection = Db.NewAdminConnection())
			{
				try
				{
					AdoTestUtils.DropDbIfExists(auxConnection, testDbName);

					var staffLogin1PK = Guid.NewGuid();
					var staffLogin2PK = Guid.NewGuid();
					var passwordHash = "0x02009b4291a6ae9f607f69ebb6a7559fa48044632166e4bb3d3463c6ad6fc62cf3fa414f37167674d16c6c8cf74f3c4c34d95177c3090406b489e2c3a6ea7ca5d1ab4528df0e";
					string sqlText = string.Format(@"
						insert dbo.GlbPerson (PER_PK, PER_FullName) values ('{0}', 'name')
						INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SqlLoginPasswordHash, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{3}', '!T1', '{1}', {5}, '{0}', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
						INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SqlLoginPasswordHash, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{4}', '!T2', '{2}', {5}, '{0}', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
						INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (newid(), '208068b6-3383-44bf-8e0d-dbd827f9d675', '{3}');
						INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (newid(), '6f0eb310-fc5c-4696-9594-f8ce156542c6', '{4}');
						INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (newid(), '208068b6-3383-44bf-8e0d-dbd827f9d675', '{4}');
						",
						personPK, staffLogin1, staffLogin2, staffLogin1PK, staffLogin2PK, passwordHash);
					auxConnection.ExecuteNonQuery(sqlText);

					testDbUserManager.CreateDbLogin_Exposed(auxConnection, staffLogin1, new HashSet<string>(), () => "", passwordHash);
					testDbUserManager.CreateDbLogin_Exposed(auxConnection, staffLogin2, new HashSet<string>(), () => "", passwordHash);

					new DocManagerDBHelperTestClass().CreateDatabase(237);

					AssertEquals("TestStaffLogin9991 has [cwRestrictedReaderRole] rights on new DB?", false,
						DbUserManagerForTesting.CheckUserHasRightsOnDb(testDbName, staffLogin1, DbRoleTypes.CwRestrictedReaderRole, auxConnection));
					AssertEquals("TestStaffLogin9991 has [backupoperator] rights on new DB?", true,
						DbUserManagerForTesting.CheckUserHasRightsOnDb(testDbName, staffLogin1, "db_backupoperator", auxConnection));
					AssertEquals("TestStaffLogin9992 has [cwRestrictedReaderRole] rights on new DB?", true,
						DbUserManagerForTesting.CheckUserHasRightsOnDb(testDbName, staffLogin2, DbRoleTypes.CwRestrictedReaderRole, auxConnection));
					AssertEquals("TestStaffLogin9992 has [backupoperator] rights on new DB?", true,
						DbUserManagerForTesting.CheckUserHasRightsOnDb(testDbName, staffLogin2, "db_backupoperator", auxConnection));
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(auxConnection, testDbName);
					testDbUserManager.DropDbLogin_Exposed(auxConnection, staffLogin1, () => "");
					testDbUserManager.DropDbLogin_Exposed(auxConnection, staffLogin2, () => "");
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			Registry.Business.SystemDataRegistry.Instance.DocManagerDBDataFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Temp.TempPath);
			Registry.Business.SystemDataRegistry.Instance.DocManagerDBLogFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Temp.TempPath);
		}
	}
}
