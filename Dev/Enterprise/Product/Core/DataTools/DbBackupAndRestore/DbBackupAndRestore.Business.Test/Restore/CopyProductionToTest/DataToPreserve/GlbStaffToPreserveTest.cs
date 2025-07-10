using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;
using WTG.Foundation.Cryptography.UserSecrets;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class GlbStaffToPreserveTest : TestCase
	{
		public void TestSchema()
		{
			GlbStaffToPreserveForTesting glbStaffToPreserve = new GlbStaffToPreserveForTesting();
			Assert("GlbStaff exists", SchemaTestHelper.TableExists(glbStaffToPreserve.MainTableName_Exposed));
		}

		[UseSnapshotProtection]
		public void TestPreserveGS_PasswordHashAndCo()
		{
			CreateTestStaff();
			var targetDbName = "TestDbF6602D816A074FA3BD2A596AB569C528";
			PrepareTestDatabase(targetDbName);
			var dataVaultDbName = targetDbName + "-DataVault";
			PrepareTestDataVaultDatabase(dataVaultDbName);

			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				try
				{
					RunCopyProdToTestForStmScheduleTaskTable(connection, targetDbName, dataVaultDbName);
					AssertRecordsAreCopied(connection, targetDbName, "Password preserved");
				}

				finally
				{
					AdoTestUtils.DropDbIfExists(connection, targetDbName);
					AdoTestUtils.DropDbIfExists(connection, dataVaultDbName);
				}
			}
		}

		const string oldPassword = "Changeme1234";
		const string oldPasswordHash = "0xA4A29ECF959AA1E4B9D50524473CAE2453734750";
		const string oldPasswordHashBase64 = "pKKez5WaoeS51QUkRzyuJFNzR1A=";
		const string oldPasswordSalt = "0x18EE04A1AAA61445C01B180A362F4A4F";
		const string oldPasswordSaltBase64 = "GO4EoaqmFEXAGxgKNi9KTw==";
		const int oldPasswordHashIterations = 200000;

		const string newPassword = "Password123";
		const string newPasswordHash = "0x666905A066E0B94CD66F7995116329D5B4F294E9";
		const string newPasswordHashBase64 = "ZmkFoGbguUzWb3mVEWMp1bTylOk=";
		const string newPasswordSalt = "0x66F6ED06172FA62DBB65B5FDC815C213";
		const string newPasswordSaltBase64 = "ZvbtBhcvpi27ZbX9yBXCEw==";
		const int newPasswordHashIterations = 1000;

		void CreateTestStaff()
		{
			var sqlScript = $@"
			INSERT INTO dbo.GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_PasswordHash, GS_PasswordSalt, GS_PasswordHashIterations, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (newid(), 'test1', 'TS1', {oldPasswordHash}, {oldPasswordSalt}, {oldPasswordHashIterations}, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
			INSERT INTO dbo.GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_PasswordHash, GS_PasswordSalt, GS_PasswordHashIterations, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (newid(), 'test2', 'TS2', {oldPasswordHash}, {oldPasswordSalt}, {oldPasswordHashIterations}, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
			INSERT INTO dbo.GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_PasswordHash, GS_PasswordSalt, GS_PasswordHashIterations, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (newid(), 'test3', 'TS3', {oldPasswordHash}, {oldPasswordSalt}, {oldPasswordHashIterations}, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
			INSERT INTO dbo.GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_PasswordHash, GS_PasswordSalt, GS_PasswordHashIterations, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (newid(), 'test4', 'TS4', {oldPasswordHash}, {oldPasswordSalt}, {oldPasswordHashIterations}, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
			INSERT INTO dbo.GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_PasswordHash, GS_PasswordSalt, GS_PasswordHashIterations, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (newid(), 'test5', 'TS5', {oldPasswordHash}, {oldPasswordSalt}, {oldPasswordHashIterations}, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
			";
			Db.Connection.ExecuteNonQuery(sqlScript);
		}

		void PrepareTestDatabase(string dbName)
		{
			AdoTestUtils.CreateDbDropExisting(dbName);
			using (var connection = Db.NewAdminConnection(dbName))
			{
				var sqlScript = $@"
SELECT * INTO GlbStaff FROM [{Db.DatabaseName}].dbo.GlbStaff
ALTER TABLE dbo.GlbStaff ADD CONSTRAINT [PK_UX__GS_PK] PRIMARY KEY NONCLUSTERED (GS_PK) ON [PRIMARY]
UPDATE dbo.GlbStaff SET GS_PasswordHash = {newPasswordHash}, GS_PasswordSalt = {newPasswordSalt}, GS_PasswordHashIterations = {newPasswordHashIterations}
UPDATE dbo.GlbStaff SET GS_LoginName = 'TestY' WHERE GS_LoginName = 'test3'
UPDATE dbo.GlbStaff SET GS_PK = newid() WHERE GS_LoginName = 'test5'
";
				connection.ExecuteNonQuery(sqlScript);
			}
		}

		void PrepareTestDataVaultDatabase(string dbName)
		{
			AdoTestUtils.CreateDbDropExisting(dbName);
			using (var connection = Db.NewAdminConnection(dbName))
			{
				var sqlScript = $@"
SELECT * INTO TempGlbStaff FROM [{Db.DatabaseName}].dbo.GlbStaff
UPDATE TempGlbStaff SET GS_LoginName = 'TestX' WHERE GS_LoginName = 'test2'
UPDATE TempGlbStaff SET GS_LoginName = 'TestY' WHERE GS_LoginName = 'test3'
UPDATE TempGlbStaff SET GS_SystemCreateTimeUtc = NULL, GS_SystemLastEditTimeUtc = NULL WHERE GS_LoginName = 'test3';
DELETE FROM TempGlbStaff  WHERE GS_LoginName = 'test4';
";
				connection.ExecuteNonQuery(sqlScript);
			}
		}

		void RunCopyProdToTestForStmScheduleTaskTable(AdminConnection connection, string targetDbName, string dataVaultDbName)
		{
			var glbStaffToPreserve = new GlbStaffToPreserve();
			var sqlScript = glbStaffToPreserve.GetCopyTempDbDataToTestDbScript(dataVaultDbName, targetDbName);
			sqlScript += glbStaffToPreserve.GetClearDataToBeOverwrittenByTestDataScript(dataVaultDbName, targetDbName);

			connection.ExecuteNonQuery(sqlScript);
		}

		void AssertRecordsAreCopied(AdminConnection connection, string dbName, string messageToDisplay)
		{
			var sqlScript = $"SELECT GS_LoginName, GS_PasswordHash, GS_PasswordSalt, GS_PasswordHashIterations, GS_SystemCreateTimeUtc, GS_SystemLastEditTimeUtc FROM [{dbName}].dbo.GlbStaff WHERE GS_LoginName LIKE 'test%'";
			using (var reader = connection.Command(sqlScript).ExecuteReader())
			{
				var recordNum = 0;
				while (reader.Read())
				{
					recordNum++;
					var loginName = reader[0].ToString();
					var passwordHash = (byte[])reader[1];
					var passwordSalt = (byte[])reader[2];
					var passwordIterations = (int)reader[3];
					var component = new UserSecretsRawHashComponents(UserSecretHashAlgorithmExtensions.PreferredAlgorithm, passwordHash, passwordSalt, passwordIterations);

					if ((loginName == "test1") // test1 match in both db
						|| (loginName == "TestY")) //testY was renamed from test3 in both db
					{
						AssertEquals($"{loginName} Password IsMatchingSecrect", true, UserSecretsContext.DefaultContext.IsMatchingSecret(oldPassword, component.GetAdapter()));
						AssertEquals($"{loginName} GS_PasswordHash should be preserved", oldPasswordHashBase64, Convert.ToBase64String(passwordHash));
						AssertEquals($"{loginName} GS_PasswordSalt should be preserved", oldPasswordSaltBase64, Convert.ToBase64String(passwordSalt));
						AssertEquals($"{loginName} GS_PasswordHashIterations should be preserved", oldPasswordHashIterations, passwordIterations);
					}
					else // test2, test4 are not match so should not preserve password. test5's PK is not match and should not preserved too
					{
						AssertEquals($"{loginName} Password IsMatchingSecrect", true, UserSecretsContext.DefaultContext.IsMatchingSecret(newPassword, component.GetAdapter()));
						AssertEquals($"Unmatch user {loginName} GS_PasswordHash should be preserved", newPasswordHashBase64, Convert.ToBase64String(passwordHash));
						AssertEquals($"Unmatch user {loginName} GS_PasswordSalt should be preserved", newPasswordSaltBase64, Convert.ToBase64String(passwordSalt));
						AssertEquals($"Unmatch user {loginName} GS_PasswordHashIterations should be preserved", newPasswordHashIterations, passwordIterations);
					}

					AssertEquals($"{loginName} GS_SystemCreateTimeUtc should not be null", false, reader.IsDBNull(4));
					AssertEquals($"{loginName} GS_SystemLastEditTimeUtc should not be null", false, reader.IsDBNull(5));
				}

				AssertGreaterThan("Record should be more than 0", recordNum, 0);
			}
		}
	}
}
