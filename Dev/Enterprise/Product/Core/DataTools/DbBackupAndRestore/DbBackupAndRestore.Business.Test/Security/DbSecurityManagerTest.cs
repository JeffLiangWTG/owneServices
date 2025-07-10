using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;
using WTG.Foundation.Cryptography.UserSecrets;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class DbSecurityManagerTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestResetCW1Sysadmin()
		{
			var testManager = new DbSecurityManager();
			testManager.OnTaskFailed += OnTestTaskFailed;

			var newPassword = "abcD3f9";

			testManager.ResetCW1Sysadmin(Db.ServerName, Db.DatabaseName, newPassword);
			AssertNullOrEmpty("Task should success", taskFailedInfo);

			using (var connection = Db.NewAdminConnection(Db.ServerName, Db.DatabaseName))
			{
				var sql = "SELECT GS_PasswordHash, GS_PasswordSalt, GS_PasswordHashIterations, GS_IsActive FROM dbo.GlbStaff WHERE GS_LoginName = 'sysadmin'";
				using (var reader = connection.Command(sql).ExecuteReader())
				{
					if (reader.Read())
					{
						var passwordHash = (byte[])reader[0];
						var passwordSalt = (byte[])reader[1];
						var passwordIterations = (int)reader[2];

						var component = new UserSecretsRawHashComponents(UserSecretHashAlgorithmExtensions.PreferredAlgorithm, passwordHash, passwordSalt, passwordIterations);
						AssertEquals("Password", true, UserSecretsContext.DefaultContext.IsMatchingSecret(newPassword, component.GetAdapter()));

						AssertEquals("GS_IsActive", "True", reader[3].ToString());
					}
					else
					{
						Assert("sysadmin is missing!!", false);
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestResetAllStaffLocalPassword()
		{
			CreateTestStaff();

			var testManager = new DbSecurityManager();
			testManager.OnTaskFailed += OnTestTaskFailed;

			testManager.ResetAllStaffLocalPassword(Db.ServerName, Db.DatabaseName);
			AssertNullOrEmpty("Task should success", taskFailedInfo);

			using (var connection = Db.NewAdminConnection(Db.ServerName, Db.DatabaseName))
			{
				var sql = @"SELECT GS_IsSystemAccount, GS_LoginName, GS_PasswordHash, GS_PasswordSalt, GS_PasswordHashIterations FROM dbo.GlbStaff WHERE GS_LoginName IN ('test1', 'test2', 'test3', 'test4')";
				using (var reader = connection.Command(sql).ExecuteReader())
				{
					while (reader.Read())
					{
						var isSystemAccount = (bool)reader[0];
						var loginName = reader[1];
						var passwordHash = Convert.ToString(reader[2]);
						var passwordSalt = Convert.ToString(reader[3]);
						var pwdIterations = (int)reader[4];
						if (isSystemAccount)
						{
							AssertNotNullOrEmpty($@"System account {loginName} GS_PasswordHash", passwordHash);
							AssertNotNullOrEmpty($@"System account {loginName} GS_PasswordSalt", passwordSalt);
							AssertEquals($@"System account {loginName} GS_PasswordHashIterations", 200000, pwdIterations);
						}
						else
						{
							AssertNullOrEmpty($@"Non-system account {loginName} GS_PasswordHash", passwordHash);
							AssertNullOrEmpty($@"Non-system account {loginName} GS_PasswordSalt", passwordSalt);
							AssertEquals($@"Non-system account {loginName} GS_PasswordHashIterations", 0, pwdIterations);
						}
					}
				}
			}
		}

		void CreateTestStaff()
		{
			var sqlScript = $@"
INSERT INTO dbo.GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_PasswordHash, GS_PasswordSalt, GS_PasswordHashIterations, GS_IsSystemAccount, GS_IsActive, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (newid(), 'test1', 'TS1', 0xA4A29ECF959AA1E4B9D50524473CAE2453734750, 0x18EE04A1AAA61445C01B180A362F4A4F, 200000, 0, 1, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_PasswordHash, GS_PasswordSalt, GS_PasswordHashIterations, GS_IsSystemAccount, GS_IsActive, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (newid(), 'test2', 'TS2', 0xA4A29ECF959AA1E4B9D50524473CAE2453734750, 0x18EE04A1AAA61445C01B180A362F4A4F, 200000, 0, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_PasswordHash, GS_PasswordSalt, GS_PasswordHashIterations, GS_IsSystemAccount, GS_IsActive, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (newid(), 'test3', 'TS3', 0xA4A29ECF959AA1E4B9D50524473CAE2453734750, 0x18EE04A1AAA61445C01B180A362F4A4F, 200000, 1, 1, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_PasswordHash, GS_PasswordSalt, GS_PasswordHashIterations, GS_IsSystemAccount, GS_IsActive, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (newid(), 'test4', 'TS4', 0xA4A29ECF959AA1E4B9D50524473CAE2453734750, 0x18EE04A1AAA61445C01B180A362F4A4F, 200000, 1, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
";
			Db.Connection.Command(sqlScript).ExecuteNonQuery();
		}

		[UseSnapshotProtection]
		public void TestGetServerMainDbList()
		{
			var testManager = new DbSecurityManager();

			using (var conn = Db.NewAdminConnection())
			{
				var sqlText = @"
IF EXISTS (SELECT NULL FROM dbo.StmData WHERE SD_Name = 'BiDataWarehouseServer')
	UPDATE dbo.StmData
	SET SD_BinaryValue = CONVERT(VARBINARY(MAX), N'DataWarehouseServer')
	WHERE SD_Name = 'BiDataWarehouseServer'
ELSE
	INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_BinaryValue)
	VALUES(NEWID(), 'BiDataWarehouseServer', CONVERT(VARBINARY(MAX), N'DataWarehouseServer'))";
				conn.ExecuteNonQuery(sqlText);
			}

			var dbEntryList = testManager.GetServerMainDbList(Db.ServerName, null);
			AssertEquals("DataWarehouseServer value?", "DataWarehouseServer", dbEntryList.GetDataWarehouseServer(Db.DatabaseName));
		}

		#region DatabaseEntryCollectionTests

		public void TestDatabaseEntryCollection_GetDataWarehouseServer()
		{
			var dbEntryCollection = new DbSecurityManager.DatabaseEntryCollection();
			dbEntryCollection.Add("testDbName1", "testServerName1");
			dbEntryCollection.Add("testDbName2", "testServerName2");

			AssertEquals("testServerName1", dbEntryCollection.GetDataWarehouseServer("testDbName1"));
			AssertEquals(null, dbEntryCollection.GetDataWarehouseServer("TeStDbNaMe1"));
			AssertEquals(null, dbEntryCollection.GetDataWarehouseServer("testDbName3"));
		}

		public void TestDatabaseEntryCollection_GetDatabaseList()
		{
			var dbEntryCollection = new DbSecurityManager.DatabaseEntryCollection();
			dbEntryCollection.Add("testDbName1", "testServerName1");
			dbEntryCollection.Add("testDbName2", "testServerName2");

			AssertArrayEqualsByElements(new[] { "testDbName1", "testDbName2" }, dbEntryCollection.GetDatabaseList());
		}

		public void TestDatabaseEntryCollection_Count()
		{
			var dbEntryCollection = new DbSecurityManager.DatabaseEntryCollection();
			AssertEquals(0, dbEntryCollection.Count);

			dbEntryCollection.Add("testDbName1", "testServerName1");
			AssertEquals(1, dbEntryCollection.Count);

			dbEntryCollection.Add("testDbName2", "testServerName2");
			AssertEquals(2, dbEntryCollection.Count);
		}

		#endregion

		void OnTestTaskFailed(string message)
		{
			taskFailedInfo = message;
		}
		string taskFailedInfo;
	}
}
