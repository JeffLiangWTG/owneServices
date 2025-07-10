using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class AdminConnectionTest : TransactionedTestCase
	{
		public void TestEnableApplicationDbLogins()
		{
			AdoTestUtils.DropDbLoginIfExists(TestAdminConnection, ((IDbLoginRepair)TestAdminConnection).ReaderDbLoginName);
			AdoTestUtils.DropDbLoginIfExists(TestAdminConnection, ((IDbLoginRepair)TestAdminConnection).RestrictedReaderDbLoginName);
			AdoTestUtils.DropDbLoginIfExists(TestAdminConnection, ((IDbLoginRepair)TestAdminConnection).UnrestrictedWriterDbLoginName);

			((IDbLoginRepair)TestAdminConnection).EnableApplicationDbLogins();

			DatabaseLoginTest.AssertLoginIsEnabled(TestAdminConnection, ((IDbLoginRepair)TestAdminConnection).ReaderDbLoginName, expected: true);
			DatabaseLoginTest.AssertLoginIsEnabled(TestAdminConnection, ((IDbLoginRepair)TestAdminConnection).RestrictedReaderDbLoginName, expected: true);
			DatabaseLoginTest.AssertLoginIsEnabled(TestAdminConnection, ((IDbLoginRepair)TestAdminConnection).RestrictedWriterDbLoginName, expected: true);
			DatabaseLoginTest.AssertLoginIsEnabled(TestAdminConnection, ((IDbLoginRepair)TestAdminConnection).UnrestrictedWriterDbLoginName, expected: true);

			DatabaseLoginTest.AssertUserIsMemberOfRole(TestAdminConnection, TestAdminConnection.CurrentDatabase, ((IDbLoginRepair)TestAdminConnection).ReaderDbLoginName, "cwReaderRole");
			DatabaseLoginTest.AssertUserIsMemberOfRole(TestAdminConnection, TestAdminConnection.CurrentDatabase, ((IDbLoginRepair)TestAdminConnection).ReaderDbLoginName, "db_datareader");
			DatabaseLoginTest.AssertUserIsMemberOfRole(TestAdminConnection, TestAdminConnection.CurrentDatabase, ((IDbLoginRepair)TestAdminConnection).RestrictedReaderDbLoginName, "cwRestrictedReaderRole");
			DatabaseLoginTest.AssertUserIsMemberOfRole(TestAdminConnection, TestAdminConnection.CurrentDatabase, ((IDbLoginRepair)TestAdminConnection).RestrictedWriterDbLoginName, "cwRestrictedWriterRole");
			DatabaseLoginTest.AssertUserIsMemberOfRole(TestAdminConnection, TestAdminConnection.CurrentDatabase, ((IDbLoginRepair)TestAdminConnection).UnrestrictedWriterDbLoginName, "cwUnrestrictedWriterRole");
		}

		public void TestDisableApplicationDbLogins()
		{
			AdoTestUtils.DropDbLoginIfExists(TestAdminConnection, ((IDbLoginRepair)TestAdminConnection).ReaderDbLoginName);
			AdoTestUtils.DropDbLoginIfExists(TestAdminConnection, ((IDbLoginRepair)TestAdminConnection).RestrictedReaderDbLoginName);
			AdoTestUtils.DropDbLoginIfExists(TestAdminConnection, ((IDbLoginRepair)TestAdminConnection).UnrestrictedWriterDbLoginName);

			((IDbLoginRepair)TestAdminConnection).EnableApplicationDbLogins();
			((IDbLockout)TestAdminConnection).DisableApplicationDbLogins();

			DatabaseLoginTest.AssertLoginIsExist(TestAdminConnection, ((IDbLoginRepair)TestAdminConnection).ReaderDbLoginName, expected: true);
			DatabaseLoginTest.AssertLoginIsExist(TestAdminConnection, ((IDbLoginRepair)TestAdminConnection).RestrictedReaderDbLoginName, expected: true);
			DatabaseLoginTest.AssertLoginIsExist(TestAdminConnection, ((IDbLoginRepair)TestAdminConnection).RestrictedWriterDbLoginName, expected: true);
			DatabaseLoginTest.AssertLoginIsExist(TestAdminConnection, ((IDbLoginRepair)TestAdminConnection).UnrestrictedWriterDbLoginName, expected: true);

			DatabaseLoginTest.AssertLoginIsEnabled(TestAdminConnection, ((IDbLoginRepair)TestAdminConnection).ReaderDbLoginName, expected: false);
			DatabaseLoginTest.AssertLoginIsEnabled(TestAdminConnection, ((IDbLoginRepair)TestAdminConnection).RestrictedReaderDbLoginName, expected: false);
			DatabaseLoginTest.AssertLoginIsEnabled(TestAdminConnection, ((IDbLoginRepair)TestAdminConnection).RestrictedWriterDbLoginName, expected: false);
			DatabaseLoginTest.AssertLoginIsEnabled(TestAdminConnection, ((IDbLoginRepair)TestAdminConnection).UnrestrictedWriterDbLoginName, expected: false);

			DatabaseLoginTest.AssertUserIsMemberOfRole(TestAdminConnection, TestAdminConnection.CurrentDatabase, ((IDbLoginRepair)TestAdminConnection).ReaderDbLoginName, "cwReaderRole");
			DatabaseLoginTest.AssertUserIsMemberOfRole(TestAdminConnection, TestAdminConnection.CurrentDatabase, ((IDbLoginRepair)TestAdminConnection).ReaderDbLoginName, "db_datareader");
			DatabaseLoginTest.AssertUserIsMemberOfRole(TestAdminConnection, TestAdminConnection.CurrentDatabase, ((IDbLoginRepair)TestAdminConnection).RestrictedReaderDbLoginName, "cwRestrictedReaderRole");
			DatabaseLoginTest.AssertUserIsMemberOfRole(TestAdminConnection, TestAdminConnection.CurrentDatabase, ((IDbLoginRepair)TestAdminConnection).RestrictedWriterDbLoginName, "cwRestrictedWriterRole");
			DatabaseLoginTest.AssertUserIsMemberOfRole(TestAdminConnection, TestAdminConnection.CurrentDatabase, ((IDbLoginRepair)TestAdminConnection).UnrestrictedWriterDbLoginName, "cwUnrestrictedWriterRole");
		}

		public void TestDisableMainUserLoginWithOwnedServiceBrokerObjects()
		{
			AdoTestUtils.DropDbLoginIfExists(TestAdminConnection, ((IDbLoginRepair)TestAdminConnection).ReaderDbLoginName);
			AdoTestUtils.DropDbLoginIfExists(TestAdminConnection, ((IDbLoginRepair)TestAdminConnection).RestrictedReaderDbLoginName);
			AdoTestUtils.DropDbLoginIfExists(TestAdminConnection, ((IDbLoginRepair)TestAdminConnection).UnrestrictedWriterDbLoginName);

			((IDbLoginRepair)TestAdminConnection).EnableApplicationDbLogins();

			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				CREATE CONTRACT [{0}_contract] AUTHORIZATION [{1}] ([DEFAULT] SENT BY ANY);
				CREATE QUEUE [{0}_queue];
				ALTER AUTHORIZATION ON OBJECT::[{0}_queue] TO [{1}];
				CREATE SERVICE [{0}_service] AUTHORIZATION [{1}] ON QUEUE [{0}_queue] ([DEFAULT]);",
				"Dummy30DD4A3B26544F2590EDC0AA9FF65CB6", TestLoginRepairConnection.RestrictedWriterDbLoginName);
			TestAdminConnection.ExecuteNonQuery(sqlText);

			((IDbLockout)TestAdminConnection).DisableApplicationDbLogins();

			DatabaseLoginTest.AssertLoginIsEnabled(TestAdminConnection, TestLoginRepairConnection.ReaderDbLoginName, expected: false);
			DatabaseLoginTest.AssertLoginIsEnabled(TestAdminConnection, TestLoginRepairConnection.RestrictedReaderDbLoginName, expected: false);
			DatabaseLoginTest.AssertLoginIsEnabled(TestAdminConnection, TestLoginRepairConnection.RestrictedWriterDbLoginName, expected: false);
			DatabaseLoginTest.AssertLoginIsEnabled(TestAdminConnection, TestLoginRepairConnection.UnrestrictedWriterDbLoginName, expected: false);
		}

		public void TestEnsureRestrictedReaderDbLogin()
		{
			var allDatabases = TestAdminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW & ~DatabaseType.SingleSharedRef);

			var baseDatabase = TestAdminConnection.CurrentDatabase;

			AdoTestUtils.DropDbLoginIfExists(TestAdminConnection, $"{baseDatabase}_RestrictedReaderLogin");

			((IDbLoginRepair)TestAdminConnection).EnsureRestrictedReaderDbLogin();

			foreach (var dbName in allDatabases)
			{
				DatabaseLoginTest.AssertLoginIsMappedToDatabase(TestAdminConnection, dbName, $"{baseDatabase}_RestrictedReaderLogin", expected: true);
				DatabaseLoginTest.AssertUserIsMemberOfRole(TestAdminConnection, dbName, $"{baseDatabase}_RestrictedReaderLogin", "cwRestrictedReaderRole");
			}
		}

		public void TestRestrictedReaderDbLoginName()
		{
			AssertEquals($"{TestAdminConnection.CurrentDatabase}_RestrictedReaderLogin", ((IDbLoginRepair)TestAdminConnection).RestrictedReaderDbLoginName);

			((IDbLoginRepair)TestAdminConnection).EnsureRestrictedReaderDbLogin();

			var logInName = ((IDbLoginRepair)TestAdminConnection).RestrictedReaderDbLoginName;
			var logsInAndUsers = new List<Tuple<string, string>>();

			TestAdminConnection.ExecuteNonQuery($"EXECUTE AS LOGIN = '{logInName}'");
			TestAdminConnection.ExecuteReader("SELECT SUSER_NAME() AS LogINName, USER_NAME() AS UserName", reader => logsInAndUsers.Add(new Tuple<string, string>(reader.GetString(0), reader.GetString(1))));

			AssertEquals(1, logsInAndUsers.Count);
			AssertEquals($"{TestAdminConnection.CurrentDatabase}_RestrictedReaderLogin", logsInAndUsers[0].Item1);
			AssertEquals($"{TestAdminConnection.CurrentDatabase}_RestrictedReaderLogin", logsInAndUsers[0].Item2);
		}

		void AssertErrorMessageContains<T>(string message, string expectedErrorMessage, AnonymousMethod codeToRun) where T : Exception
		{
			var exceptionThrown = false;
			Type expectedTypeOfException = typeof(T);
			try
			{
				codeToRun();
			}
			catch (Exception actualException)
			{
				if (!expectedTypeOfException.IsInstanceOfType(actualException))
				{
					Fail($"Expected exception of type {expectedTypeOfException.FullName}, but was:\n{actualException}");
				}

				exceptionThrown = true;
				Assert(message, actualException.Message.Contains(expectedErrorMessage));
			}
			if (!exceptionThrown)
			{
				Fail($"Expected exception of type {expectedTypeOfException.FullName}, but none occured");
			}
		}

		public void SetupRestrictedReaderTests()
		{
			((IDbLoginRepair)TestAdminConnection).EnsureRestrictedReaderDbLogin();

			var logInName = ((IDbLoginRepair)TestAdminConnection).RestrictedReaderDbLoginName;
			var logsInAndUsers = new List<Tuple<string, string>>();

			TestAdminConnection.ExecuteNonQuery($"EXECUTE AS LOGIN = '{logInName}'");
			TestAdminConnection.ExecuteReader("SELECT SUSER_NAME() AS LogINName, USER_NAME() AS UserName", reader => logsInAndUsers.Add(new Tuple<string, string>(reader.GetString(0), reader.GetString(1))));
		}

		public void TestRestrictedReaderPermissionsOnDboSchema()
		{
			SetupRestrictedReaderTests();
			var sdPk = Guid.NewGuid().ToString();

			CombineAssertions("Restricted reader should have SELECT permissions only, but had:\n", () =>
			{
				//SELECT dbo
				AssertNoExceptionThrown("Restricted reader should SELECT dbo tables",
					() => _ = TestAdminConnection.ExecuteScalar("SELECT COUNT(*) FROM [dbo].[StmData]"));
				//INSERT dbo
				AssertErrorMessageContains<SqlException>("Restricted reader should not INSERT dbo tables",
					$"The INSERT permission was denied on the object 'StmData', database '{TestAdminConnection.CurrentDatabase}', schema 'dbo'.",
					() => TestAdminConnection.ExecuteNonQuery($"INSERT INTO [dbo].[StmData] ([SD_PK], [SD_Name]) VALUES ('{sdPk}', 'aaaa')"));
				//UPDATE dbo
				AssertErrorMessageContains<SqlException>("Restricted reader should not UPDATE dbo tables",
					$"The UPDATE permission was denied on the object 'StmData', database '{TestAdminConnection.CurrentDatabase}', schema 'dbo'.",
					() => TestAdminConnection.ExecuteNonQuery($"UPDATE [dbo].[StmData] SET [SD_Name] = 'bbbb' WHERE [SD_PK] = '{sdPk}'"));
				//DELETE dbo
				AssertErrorMessageContains<SqlException>("Restricted reader should not DELETE dbo tables",
					$"The DELETE permission was denied on the object 'StmData', database '{TestAdminConnection.CurrentDatabase}', schema 'dbo'.",
					() => TestAdminConnection.ExecuteNonQuery($"DELETE FROM [dbo].[StmData] WHERE [SD_PK] = '{sdPk}'"));
				//EXECUTE dbo
				AssertNoExceptionThrown("Restricted reader should EXECUTE SELECT dbo tables",
					() => TestAdminConnection.ExecuteNonQuery("EXECUTE ('SELECT COUNT(*) FROM [dbo].[StmData]')"));
			});
		}

		public void TestRestrictedReaderPermissionsOnHrmSchema()
		{
			SetupRestrictedReaderTests();
			var gsrPk = Guid.NewGuid().ToString();
			var gsPK = TestAdminConnection.ExecuteScalar("SELECT TOP 1 GS_PK FROM [dbo].[GlbStaff]");

			CombineAssertions("Restricted reader should have no permissions on HRM schema, but had:\n", () =>
			{
				//SELECT hrm
				AssertExceptionThrown<SqlException>("Restricted reader should not SELECT hrm tables",
					$"The SELECT permission was denied on the object 'GlbStaffRemuneration', database '{TestAdminConnection.CurrentDatabase}', schema 'hrm'.",
					() => _ = TestAdminConnection.ExecuteScalar("SELECT COUNT(*) FROM [hrm].[GlbStaffRemuneration]"));
				//INSERT hrm
				AssertErrorMessageContains<SqlException>("Restricted reader should not INSERT hrm tables",
					$"The INSERT permission was denied on the object 'GlbStaffRemuneration', database '{TestAdminConnection.CurrentDatabase}', schema 'hrm'.",
					() => TestAdminConnection.ExecuteNonQuery($"INSERT INTO [hrm].[GlbStaffRemuneration]([GSR_PK], [GSR_GS_Staff], [GSR_RN_NKCountry], [GSR_RX_NKCurrency], [GSR_EffectiveDate], [GSR_SystemCreateTimeUtc], [GSR_SystemLastEditTimeUtc], [GSR_SystemCreateUser], [GSR_SystemLastEditUser]) VALUES ('{gsrPk}', '{gsPK}', 'AU', 'TTT', '2020-2-1', '2020-1-1', '2020-2-1', 'AKE', 'BNC')"));
				//UPDATE hrm
				AssertErrorMessageContains<SqlException>("Restricted reader should not UPDATE hrm tables",
					$"The UPDATE permission was denied on the object 'GlbStaffRemuneration', database '{TestAdminConnection.CurrentDatabase}', schema 'hrm'.",
					() => TestAdminConnection.ExecuteNonQuery($"UPDATE [hrm].[GlbStaffRemuneration] SET [GSR_EffectiveDate] = '1990-02-01' WHERE [GSR_PK] = '{gsrPk}'"));
				//DELETE hrm
				AssertErrorMessageContains<SqlException>("Restricted reader should not DELETE hrm tables",
					$"The DELETE permission was denied on the object 'GlbStaffRemuneration', database '{TestAdminConnection.CurrentDatabase}', schema 'hrm'.",
					() => TestAdminConnection.ExecuteNonQuery($"DELETE FROM [hrm].[GlbStaffRemuneration] WHERE [GSR_PK] = '{gsrPk}'"));
				//EXECUTE hrm
				AssertErrorMessageContains<SqlException>("Restricted reader should not EXECUTE SELECT hrm tables",
					$"The SELECT permission was denied on the object 'GlbStaffRemuneration', database '{TestAdminConnection.CurrentDatabase}', schema 'hrm'.",
					() => TestAdminConnection.ExecuteNonQuery("EXECUTE ('SELECT COUNT(*) FROM [hrm].[GlbStaffRemuneration]')"));
			});
		}

		public void TestRestrictedreaderPermissionsOnStoredProc()
		{
			SetupRestrictedReaderTests();

			//EXECUTE stored procedure
			AssertNoExceptionThrown("Restricted reader should EXECUTE stored procedure",
				() => TestAdminConnection.ExecuteNonQuery("EXECUTE [dbo].[ep_SpaceUsed]"));
		}

		public void TestEnsureRestrictedWriterDbLogin()
		{
			var allDatabases = TestAdminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW & ~DatabaseType.SingleSharedRef);

			var baseDatabase = TestAdminConnection.CurrentDatabase;

			new RestrictedWriterDatabaseLogin(TestAdminConnection).DisableLogin();

			((IDbLoginRepair)TestAdminConnection).EnsureRestrictedWriterDbLogin();

			foreach (var dbName in allDatabases)
			{
				DatabaseLoginTest.AssertLoginIsMappedToDatabase(TestAdminConnection, dbName, $"{baseDatabase}_RestrictedWriterLogin", expected: true);
				DatabaseLoginTest.AssertUserIsMemberOfRole(TestAdminConnection, dbName, $"{baseDatabase}_RestrictedWriterLogin", "cwRestrictedWriterRole");
			}
		}

		public void TestRestrictedWriterDbLoginName()
		{
			AssertEquals($"{TestAdminConnection.CurrentDatabase}_RestrictedWriterLogin", ((IDbLoginRepair)TestAdminConnection).RestrictedWriterDbLoginName);

			((IDbLoginRepair)TestAdminConnection).EnsureRestrictedWriterDbLogin();

			var logInName = ((IDbLoginRepair)TestAdminConnection).RestrictedWriterDbLoginName;
			var logsInAndUsers = new List<Tuple<string, string>>();

			TestAdminConnection.ExecuteNonQuery($"EXECUTE AS LOGIN = '{logInName}'");
			TestAdminConnection.ExecuteReader("SELECT SUSER_NAME() AS LogINName, USER_NAME() AS UserName", reader => logsInAndUsers.Add(new Tuple<string, string>(reader.GetString(0), reader.GetString(1))));

			AssertEquals(1, logsInAndUsers.Count);
			AssertEquals($"{TestAdminConnection.CurrentDatabase}_RestrictedWriterLogin", logsInAndUsers[0].Item1);
			AssertEquals($"{TestAdminConnection.CurrentDatabase}_RestrictedWriterLogin", logsInAndUsers[0].Item2);
		}

		public void SetupRestrictedWriterTests()
		{
			((IDbLoginRepair)TestAdminConnection).EnsureRestrictedWriterDbLogin();

			var logInName = ((IDbLoginRepair)TestAdminConnection).RestrictedWriterDbLoginName;
			var logsInAndUsers = new List<Tuple<string, string>>();

			TestAdminConnection.ExecuteNonQuery($"EXECUTE AS LOGIN = '{logInName}'");
			TestAdminConnection.ExecuteReader("SELECT SUSER_NAME() AS LogINName, USER_NAME() AS UserName", reader => logsInAndUsers.Add(new Tuple<string, string>(reader.GetString(0), reader.GetString(1))));
		}

		public void TestRestrictedWriterPermissionsOnDboSchema()
		{
			SetupRestrictedWriterTests();
			var sdPk = Guid.NewGuid().ToString();

			CombineAssertions("Restricted writer should have all permissions on DBO schema, permission exceptions thrown:\n", () =>
			{
				//SELECT dbo
				AssertNoExceptionThrown("Restricted writer should SELECT dbo tables",
					() => _ = TestAdminConnection.ExecuteScalar("SELECT COUNT(*) FROM [dbo].[StmData]"));
				//INSERT dbo
				AssertNoExceptionThrown("Restricted writer should INSERT dbo tables",
					() => TestAdminConnection.ExecuteNonQuery($"INSERT INTO [dbo].[StmData] ([SD_PK], [SD_Name]) VALUES ('{sdPk}', 'aaaa')"));
				//UPDATE dbo
				AssertNoExceptionThrown("Restricted writer should UPDATE dbo tables",
					() => TestAdminConnection.ExecuteNonQuery($"UPDATE [dbo].[StmData] SET [SD_Name] = 'bbbb' WHERE [SD_PK] = '{sdPk}'"));
				//DELETE dbo
				AssertNoExceptionThrown("Restricted writer should DELETE dbo tables",
					() => TestAdminConnection.ExecuteNonQuery($"DELETE FROM [dbo].[StmData] WHERE [SD_PK] = '{sdPk}'"));
				//EXECUTE dbo
				AssertNoExceptionThrown("Restricted writer should EXECUTE SELECT dbo tables",
					() => TestAdminConnection.ExecuteNonQuery("EXECUTE ('SELECT COUNT(*) FROM [dbo].[StmData]')"));
			});
		}

		public void TestRestrictedWriterPermissionsOnHrmSchema()
		{
			SetupRestrictedWriterTests();
			var gsrPk = Guid.NewGuid().ToString();
			var gsPK = TestAdminConnection.ExecuteScalar("SELECT TOP 1 GS_PK FROM [dbo].[GlbStaff]");

			CombineAssertions("Restricted writer should have no permissions on HRM schema:\n", () =>
			{
				//SELECT hrm			
				AssertErrorMessageContains<SqlException>("Restricted writer should not SELECT hrm tables",
					$"The SELECT permission was denied on the object 'GlbStaffRemuneration', database '{TestAdminConnection.CurrentDatabase}', schema 'hrm'.",
					() => _ = TestAdminConnection.ExecuteScalar("SELECT COUNT(*) FROM [hrm].[GlbStaffRemuneration]"));
				//INSERT hrm
				AssertErrorMessageContains<SqlException>("Restricted writer should not INSERT hrm tables",
					$"The INSERT permission was denied on the object 'GlbStaffRemuneration', database '{TestAdminConnection.CurrentDatabase}', schema 'hrm'.",
					() => TestAdminConnection.ExecuteNonQuery($"INSERT INTO [hrm].[GlbStaffRemuneration]([GSR_PK], [GSR_GS_Staff], [GSR_RN_NKCountry], [GSR_RX_NKCurrency], [GSR_EffectiveDate], [GSR_SystemCreateTimeUtc], [GSR_SystemLastEditTimeUtc], [GSR_SystemCreateUser], [GSR_SystemLastEditUser]) VALUES ('{gsrPk}', '{gsPK}', 'AU', 'TTT', '2020-2-1', '2020-1-1', '2020-2-1', 'AKE', 'BNC')"));
				//UPDATE hrm
				AssertErrorMessageContains<SqlException>("Restricted writer should not UPDATE hrm tables",
					$"The UPDATE permission was denied on the object 'GlbStaffRemuneration', database '{TestAdminConnection.CurrentDatabase}', schema 'hrm'.",
					() => TestAdminConnection.ExecuteNonQuery($"UPDATE [hrm].[GlbStaffRemuneration] SET [GSR_EffectiveDate] = '1990-02-01' WHERE [GSR_PK] = '{gsrPk}'"));
				//DELETE hrm
				AssertErrorMessageContains<SqlException>("Restricted writer should not DELETE hrm tables",
					$"The DELETE permission was denied on the object 'GlbStaffRemuneration', database '{TestAdminConnection.CurrentDatabase}', schema 'hrm'.",
					() => TestAdminConnection.ExecuteNonQuery($"DELETE FROM [hrm].[GlbStaffRemuneration] WHERE [GSR_PK] = '{gsrPk}'"));
				//EXECUTE hrm
				AssertErrorMessageContains<SqlException>("Restricted writer should not EXECUTE SELECT hrm tables",
					$"The SELECT permission was denied on the object 'GlbStaffRemuneration', database '{TestAdminConnection.CurrentDatabase}', schema 'hrm'.",
					() => TestAdminConnection.ExecuteNonQuery("EXECUTE ('SELECT COUNT(*) FROM [hrm].[GlbStaffRemuneration]')"));
			});
		}

		public void TestRestrictedWriterPermissionsStoredProc()
		{
			SetupRestrictedWriterTests();

			//EXECUTE stored procedure
			AssertNoExceptionThrown("Restricted writer should EXECUTE stored procedure",
				() => TestAdminConnection.ExecuteNonQuery("EXECUTE [dbo].[ep_SpaceUsed]"));
		}

		public void TestEnsureUnrestrictedWriterDbLogin()
		{
			var allDatabases = TestAdminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW & ~DatabaseType.SingleSharedRef);
			var baseDatabase = TestAdminConnection.CurrentDatabase;

			AdoTestUtils.DropDbLoginIfExists(TestAdminConnection, $"{baseDatabase}_UnrestrictedWriterLogin");

			((IDbLoginRepair)TestAdminConnection).EnsureUnrestrictedWriterDbLogin();

			foreach (var dbName in allDatabases)
			{
				DatabaseLoginTest.AssertLoginIsMappedToDatabase(TestAdminConnection, dbName, $"{baseDatabase}_UnrestrictedWriterLogin", expected: true);
				DatabaseLoginTest.AssertUserIsMemberOfRole(TestAdminConnection, dbName, $"{baseDatabase}_UnrestrictedWriterLogin", "cwUnrestrictedWriterRole");
			}
		}

		public void TestUnrestrictedWriterDbLoginName()
		{
			AssertEquals($"{TestAdminConnection.CurrentDatabase}_UnrestrictedWriterLogin", ((IDbLoginRepair)TestAdminConnection).UnrestrictedWriterDbLoginName);
		}

		public void TestUnrestrictedWriterPermissions()
		{
			((IDbLoginRepair)TestAdminConnection).EnsureRestrictedWriterDbLogin();

			var logInName = ((IDbLoginRepair)TestAdminConnection).UnrestrictedWriterDbLoginName;
			var logsInAndUsers = new List<Tuple<string, string>>();
			var sdPk = Guid.NewGuid().ToString();
			var gsrPk = Guid.NewGuid().ToString();
			var gsPK = TestAdminConnection.ExecuteScalar("SELECT TOP 1 GS_PK FROM [dbo].[GlbStaff]");

			TestAdminConnection.ExecuteNonQuery($"EXECUTE AS LOGIN = '{logInName}'");

			TestAdminConnection.ExecuteReader("SELECT SUSER_NAME() AS LogINName, USER_NAME() AS UserName", reader => logsInAndUsers.Add(new Tuple<string, string>(reader.GetString(0), reader.GetString(1))));

			AssertEquals(1, logsInAndUsers.Count);
			AssertEquals($"{TestAdminConnection.CurrentDatabase}_UnrestrictedWriterLogin", logsInAndUsers[0].Item1);
			AssertEquals($"{TestAdminConnection.CurrentDatabase}_UnrestrictedWriterLogin", logsInAndUsers[0].Item2);

			//SELECT dbo
			AssertNoExceptionThrown("Unrestricted writer can SELECT dbo tables",
				() => _ = TestAdminConnection.ExecuteScalar("SELECT COUNT(*) FROM [dbo].[StmData]"));
			//INSERT dbo
			AssertNoExceptionThrown("Unrestricted writer can INSERT dbo tables",
				() => TestAdminConnection.ExecuteNonQuery($"INSERT INTO [dbo].[StmData] ([SD_PK], [SD_Name]) VALUES ('{sdPk}', 'aaaa')"));
			//UPDATE dbo
			AssertNoExceptionThrown("Unrestricted writer can UPDATE dbo tables",
				() => TestAdminConnection.ExecuteNonQuery($"UPDATE [dbo].[StmData] SET [SD_Name] = 'bbbb' WHERE [SD_PK] = '{sdPk}'"));
			//DELETE dbo
			AssertNoExceptionThrown("Unrestricted writer can DELETE dbo tables",
				() => TestAdminConnection.ExecuteNonQuery($"DELETE FROM [dbo].[StmData] WHERE [SD_PK] = '{sdPk}'"));
			//EXECUTE dbo
			AssertNoExceptionThrown("Unrestricted writer can EXECUTE SELECT dbo tables",
				() => TestAdminConnection.ExecuteNonQuery("EXECUTE ('SELECT COUNT(*) FROM [dbo].[StmData]')"));

			//SELECT hrm			
			AssertNoExceptionThrown("Unrestricted writer can SELECT hrm tables",
				() => _ = TestAdminConnection.ExecuteScalar("SELECT COUNT(*) FROM [hrm].[GlbStaffRemuneration]"));
			//INSERT hrm
			AssertNoExceptionThrown("Unrestricted writer can INSERT hrm tables",
				() => TestAdminConnection.ExecuteNonQuery($"INSERT INTO [hrm].[GlbStaffRemuneration]([GSR_PK], [GSR_GS_Staff], [GSR_RN_NKCountry], [GSR_RX_NKCurrency], [GSR_EffectiveDate], [GSR_SystemCreateTimeUtc], [GSR_SystemLastEditTimeUtc], [GSR_SystemCreateUser], [GSR_SystemLastEditUser]) VALUES ('{gsrPk}', '{gsPK}', 'AU', 'TTT', '2020-2-1', '2020-1-1', '2020-2-1', 'AKE', 'BNC')"));
			//UPDATE hrm
			AssertNoExceptionThrown("Unrestricted writer can UPDATE hrm tables",
				() => TestAdminConnection.ExecuteNonQuery($"UPDATE [hrm].[GlbStaffRemuneration] SET [GSR_EffectiveDate] = '1990-02-01' WHERE [GSR_PK] = '{gsrPk}'"));
			//DELETE hrm
			AssertNoExceptionThrown("Unrestricted writer can DELETE hrm tables",
				() => TestAdminConnection.ExecuteNonQuery($"DELETE FROM [hrm].[GlbStaffRemuneration] WHERE [GSR_PK] = '{gsrPk}'"));
			//EXECUTE hrm
			AssertNoExceptionThrown("Unrestricted writer can EXECUTE SELECT hrm tables",
				() => TestAdminConnection.ExecuteNonQuery("EXECUTE ('SELECT COUNT(*) FROM [hrm].[GlbStaffRemuneration]')"));

			//EXECUTE stored procedure
			AssertNoExceptionThrown("Unrestricted writer can EXECUTE stored procedure",
				() => TestAdminConnection.ExecuteNonQuery("EXECUTE [dbo].[ep_SpaceUsed]"));

			var definitionViewableSchemas = GetDefinitionViewableSchemas(TestAdminConnection);
			AssertCollectionContains("Unrestricted writer should be able to view definition on dbo objects.", "dbo", definitionViewableSchemas);
			AssertCollectionContains("Unrestricted writer should be able to view definition on hrm objects.", "hrm", definitionViewableSchemas);
		}

		#region Implementation

		IDbLoginRepair TestLoginRepairConnection
		{
			get { return (IDbLoginRepair)TestConnection; }
		}

		AdminConnection TestAdminConnection
		{
			get { return (AdminConnection)TestConnection; }
		}

		protected override DbConnection TestConnection
		{
			get
			{
				if (adminConnection == null)
				{
					adminConnection = Db.NewAdminConnection();
				}
				return adminConnection;
			}
		}
		AdminConnection adminConnection;

		static string[] GetDefinitionViewableSchemas(AdminConnection connection)
		{
			// The definition column of sys.check_constraints will return null if you don't have the VIEW DEFINITION
			// right for the object in question.
			const string sql =
@"select SCHEMA_NAME(schema_id)
from sys.check_constraints
where
	[definition] is not null
	and
	schema_id not in
	(
		select schema_id
		from sys.check_constraints
		where [definition] is null
	)
group by schema_id
";
			var result = new List<string>();
			connection.ExecuteReader(sql, reader => result.Add(reader.GetString(0)));
			return result.ToArray();
		}

		#endregion
	}
}
