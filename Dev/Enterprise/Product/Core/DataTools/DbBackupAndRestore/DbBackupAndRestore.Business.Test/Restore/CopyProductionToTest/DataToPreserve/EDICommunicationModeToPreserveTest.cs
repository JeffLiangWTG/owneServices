using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class EDICommunicationModeToPreserveTest : TestCase
	{
		public void TestSchema()
		{
			EDICommunicationModeToPreserveForTesting ediCommsToPreserve = new EDICommunicationModeToPreserveForTesting();
			Assert("EDICommunicationsMode exists", SchemaTestHelper.TableExists(ediCommsToPreserve.MainTableName_Exposed));
		}

		[UseSnapshotProtection]
		public void TestPreserveEK_DestinationLoginNameAndPassword()
		{
			// Arrange
			const string targetDbName = "TestDb-" + nameof(EDICommunicationModeToPreserve);
			const string dataVaultDbName = targetDbName + "-DataVault";

			using (AdoTestUtils.CreateDbDropExistingDisposable(targetDbName))
			using (AdoTestUtils.CreateDbDropExistingDisposable(dataVaultDbName))
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				PrepareTestDatabase(connection, targetDbName);
				PrepareTestDataVaultDatabase(connection, targetDbName, dataVaultDbName);

				// Act
				RunCopyProdToTestForEDICommsTable(connection, targetDbName, dataVaultDbName);

				// Assert 
				AssertRecordsAreCopied(connection, targetDbName);
			}
		}

		const string destination1 = "email123@yahoo.com";
		const string newDestination1 = "updatedEmail@yahoo.com";
		const string testDestination = "testSpecific@email.com";
		const string prodDestination = "prodSpecfic@email.com";

		const string loginName1 = "LoginName123";
		const string testLoginName = "testLogin";
		const string prodLoginName = "prodLogin";

		const string password1 = "Password123";
		const string testPassword = "testPassword";
		const string prodPassword = "prodPassword";

		void PrepareTestDatabase(AdminConnection connection, string dbName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				connection.ExecuteNonQuery(@"
				CREATE TABLE dbo.[EDICommunicationsMode]
				(
					[EK_PK] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
					[EK_Module] VARCHAR(3),
					[EK_Destination] VARCHAR(48),
					[EK_LoginName] VARCHAR(48),
					[EK_Password] VARCHAR(48),
				)
				");

				connection.ExecuteNonQuery($@"
					INSERT INTO [{dbName}].dbo.EDICommunicationsMode (EK_PK, EK_Module, EK_Destination, EK_LoginName, EK_Password) VALUES (newid(), 'TS1', '{destination1}', '{loginName1}', '{password1}')
					INSERT INTO [{dbName}].dbo.EDICommunicationsMode (EK_PK, EK_Module, EK_Destination, EK_LoginName, EK_Password) VALUES (newid(), 'TS2', '{testDestination}', '{testLoginName}', '{testPassword}')
					INSERT INTO [{dbName}].dbo.EDICommunicationsMode (EK_PK, EK_Module, EK_Destination, EK_LoginName, EK_Password) VALUES (newid(), 'TS3', '{prodDestination}', '{prodLoginName}', '{prodPassword}')
				");
			}
		}

		void PrepareTestDataVaultDatabase(AdminConnection connection, string dbName, string dataVaultDbName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(dataVaultDbName))
			{
				var sqlScript = $@"
				SELECT * INTO TempEDICommunicationsMode FROM [{dbName}].dbo.EDICommunicationsMode
				-- Duplicate PKs in test and prod --
				UPDATE TempEDICommunicationsMode SET EK_Destination = '{newDestination1}' WHERE EK_Destination = '{destination1}'
				-- Test specific data --
				DELETE FROM [{dbName}].dbo.EDICommunicationsMode WHERE EK_Destination = '{testDestination}'
				-- Prod specific data --
				DELETE FROM TempEDICommunicationsMode WHERE EK_Destination = '{prodDestination}'
				";
				connection.ExecuteNonQuery(sqlScript);
			}
		}

		void RunCopyProdToTestForEDICommsTable(AdminConnection connection, string targetDbName, string dataVaultDbName)
		{
			var ediCommsToPreserve = new EDICommunicationModeToPreserve();
			var sqlScript = ediCommsToPreserve.GetCopyTempDbDataToTestDbScript(dataVaultDbName, targetDbName);
			sqlScript += ediCommsToPreserve.GetClearDataToBeOverwrittenByTestDataScript(dataVaultDbName, targetDbName);

			connection.ExecuteNonQuery(sqlScript);
		}

		void AssertRecordsAreCopied(AdminConnection connection, string dbName)
		{
			var sqlScript = $"SELECT EK_Module, EK_Destination, EK_LoginName, EK_Password FROM [{dbName}].dbo.EDICommunicationsMode";
			using (var reader = connection.Command(sqlScript).ExecuteReader())
			{
				var recordNum = 0;
				while (reader.Read())
				{
					recordNum++;
					var module = reader[0].ToString();
					var destination = reader[1].ToString();
					var loginName = reader[2].ToString();
					var password = reader[3].ToString();

					switch (module)
					{
						case "TS1":
							AssertEquals(newDestination1, destination);
							AssertEquals(loginName1, loginName);
							AssertEquals(password1, password);
							break;
						case "TS2":
							AssertEquals(testDestination, destination);
							AssertEquals(testLoginName, loginName);
							AssertEquals(testPassword, password);
							break;
						case "TS3":
							AssertEquals(prodDestination, destination);
							AssertEquals(prodLoginName, loginName);
							AssertEquals(prodPassword, password);
							break;
					}
				}

				AssertEquals(3, recordNum);
			}
		}
	}
}
