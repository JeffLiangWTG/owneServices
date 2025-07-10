using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data.SqlServer.Testing;
using CargoWise.DataProtection;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	[UseSnapshotProtection]
	public class DatabaseLoginTest : DatabaseLoginTestCase
	{
		public void TestCalculateSid()
		{
			AssertEquals("0x5359444f455300590000000000000000", DatabaseLogin.CalculateSid("Odyssey"));
			AssertEquals("0x5359444f455300590000000000000000", DatabaseLogin.CalculateSid("Odyssey   ")); // 10 chars
			AssertEquals("0x5359444f455300590000000000000000", DatabaseLogin.CalculateSid("Odyssey     ")); // 12 chars
			AssertEquals("0x5359444f455300590000000000000000", DatabaseLogin.CalculateSid("Odyssey       ")); // 14 chars
			AssertEquals("0x5359444f455300590000000000000000", DatabaseLogin.CalculateSid("Odyssey         ")); // 16 chars
			AssertEquals("0x5359444f4553415955544f524547454e", DatabaseLogin.CalculateSid("OdysseyAutoRegen"));
			AssertEquals("0x34333231363538373930313233343536", DatabaseLogin.CalculateSid("1234567890123456"));
			AssertEquals("0x9cab9b948aa7abab92889b954f574953", DatabaseLogin.CalculateSid("Odyssey_CargoWiseWriterLogin"));
			AssertEquals("0x9cab9b948aa7abab92889b954f574953", DatabaseLogin.CalculateSid("ODYSSEY_CARGOWiseWRITERLogin"));
			AssertEquals("0x949e96948a97abab92889b954f574953", DatabaseLogin.CalculateSid("Odyssey_CargoWiseReaderLogin"));
			AssertEquals("0x949e96948a97abab92889b954f574953", DatabaseLogin.CalculateSid("OdYSsEy_CargoWiseReaderLogin"));
			AssertEquals("0x3948272a12212d44e9e2ded5efe1e2f2", DatabaseLogin.CalculateSid("DbNameWithMaximumNumberOfCharacters_CargoWiseWriterLogin"));
		}

		public void TestEnableLogin()
		{
			var dbLogin = DatabaseLogin;

			dbLogin.EnableLogin(msg => { });

			AssertLoginIsEnabled(TestAdminConnection, dbLogin.LoginName, expected: true);
			AssertLoginDefaultLanguage(TestAdminConnection, dbLogin.LoginName, "us_english");
		}

		public void TestDisableLogin()
		{
			var dbLogin = DatabaseLogin;

			dbLogin.EnableLogin(msg => { });
			dbLogin.DisableLogin();

			AssertLoginIsExist(TestAdminConnection, dbLogin.LoginName, expected: true);
			AssertLoginIsEnabled(TestAdminConnection, dbLogin.LoginName, expected: false);
		}

		public void TestEnableLogin_FixPasswordChanged()
		{
			var dbLogin = DatabaseLogin;
			dbLogin.EnableLogin(msg => { });

			var tempPassword = "abcde*fg$hi123'45";

			AdoTestUtils.ModifyLoginPasswordIfExists(TestAdminConnection, dbLogin.LoginName, tempPassword);
			AssertLoginPassword(TestAdminConnection, dbLogin.LoginName, tempPassword);

			dbLogin.EnableLogin(msg => { });

			AssertLoginPassword(TestAdminConnection, dbLogin.LoginName, "123abc");
		}

		public void TestEnsureLogin()
		{
			var allDatabases = TestAdminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW & ~DatabaseType.SingleSharedRef);
			var dbLogin = DatabaseLogin;

			dbLogin.EnsureLogin();

			AssertLoginIsEnabled(TestAdminConnection, dbLogin.LoginName, expected: true);
			AssertLoginDefaultLanguage(TestAdminConnection, dbLogin.LoginName, "us_english");
			AssertLoginIsMappedToDatabase(TestAdminConnection, TestAdminConnection.CurrentDatabase, dbLogin.LoginName, expected: true);
			AssertUserIsMemberOfRole(TestAdminConnection, TestAdminConnection.CurrentDatabase, dbLogin.LoginName, "DatabaseTestRole");
			AssertLoginIsMappedToAllDatabases(allDatabases, dbLogin.LoginName, expected: true);
		}

		public void TestCheckAndFixSidIfRequired()
		{
			var dbLogin = DatabaseLogin;

			var sqlText = String.Format(@"CREATE LOGIN [{0}] WITH PASSWORD = '', CHECK_POLICY = OFF;", dbLogin.LoginName);
			TestAdminConnection.ExecuteNonQuery(sqlText);
			AssertLoginIsMappedToDatabase(TestAdminConnection, Db.DatabaseName, dbLogin.LoginName, expected: false);

			dbLogin.CheckAndFixSidIfRequired();
			AssertLoginIsEnabled(TestAdminConnection, dbLogin.LoginName, expected: true);
			AssertLoginIsMappedToDatabase(TestAdminConnection, Db.DatabaseName, dbLogin.LoginName, expected: true);
			AssertLoginIsMappedToAllDatabases(TestAdminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW & ~DatabaseType.SingleSharedRef), dbLogin.LoginName, expected: true);
		}

		public void TestDisableLoginWithOwnedObjects()
		{
			const string guid = "Dummy30DD4A3B26544F2590EDC0AA9FF65CB6";
			var dbLogin = DatabaseLogin;
			dbLogin.EnableLogin(msg => { });

			AssertLoginIsEnabled(TestAdminConnection, dbLogin.LoginName, expected: true);

			var sqlText_DeleteUserAndObjects = $@"
IF EXISTS (SELECT null FROM sys.services WHERE name = '{guid}_service')
BEGIN
	DROP SERVICE [{guid}_service]
END

IF EXISTS (SELECT null FROM sys.service_queues WHERE name = '{guid}_queue')
BEGIN
	DROP QUEUE [{guid}_queue]
END

IF EXISTS (SELECT null FROM sys.service_contracts WHERE name = '{guid}_contract')
BEGIN
	DROP CONTRACT [{guid}_contract]
END

IF EXISTS (SELECT null FROM sys.database_principals WHERE name = {dbLogin.LoginName.QuoteName('\'')})
BEGIN
	DROP USER [{dbLogin.LoginName}]
END
";
			TestAdminConnection.ExecuteNonQuery(sqlText_DeleteUserAndObjects);

			var sqlText_CreateUserAndObjects = $@"
CREATE USER [{dbLogin.LoginName}] FOR LOGIN [{dbLogin.LoginName}]
CREATE CONTRACT [{guid}_contract] AUTHORIZATION [{dbLogin.LoginName}] ([DEFAULT] SENT BY ANY);
CREATE QUEUE [{guid}_queue];
ALTER AUTHORIZATION ON OBJECT::[{guid}_queue] TO [{dbLogin.LoginName}];
CREATE SERVICE [{guid}_service] AUTHORIZATION [{dbLogin.LoginName}] ON QUEUE [{guid}_queue] ([DEFAULT]);
";
			TestAdminConnection.ExecuteNonQuery(sqlText_CreateUserAndObjects);

			dbLogin.DisableLogin();

			AssertLoginIsEnabled(TestAdminConnection, dbLogin.LoginName, expected: false);
		}

		public void TestEnsureLoginCorrectlyMappedToAllDatabases_NoLogin()
		{
			var allDatabases = TestAdminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW & ~DatabaseType.SingleSharedRef);
			var dbLogin = DatabaseLogin;

			// Login doesn't exist => nothing should happen
			var builder1 = new StringBuilder();
			dbLogin.EnsureLoginCorrectlyMappedToAllDatabases(msg => builder1.AppendLine(msg));

			AssertEquals(string.Empty, builder1.ToString());
			AssertLoginIsMappedToDatabase(TestAdminConnection, TestAdminConnection.CurrentDatabase, dbLogin.LoginName, expected: false);
			AssertLoginIsMappedToAllDatabases(allDatabases, dbLogin.LoginName, expected: false);
		}

		public void TestEnsureLoginCorrectlyMappedToAllDatabases_LoginExists()
		{
			var allDatabases = TestAdminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW & ~DatabaseType.SingleSharedRef);

			AssertEquals("[PRE-CONDITION] The main database and at least one dependent database exist?", true, allDatabases.Count() > 1);

			var dbLogin = DatabaseLogin;

			// Create login
			dbLogin.EnableLogin(msg => { });

			var builder1 = new StringBuilder();
			dbLogin.EnsureLoginCorrectlyMappedToAllDatabases(msg => builder1.AppendLine(msg));

			var expectedLogEntries = allDatabases.SelectMany(database => new string[] {
				$"Db Role(s) [{string.Join(", ", dbLogin.DbLevelRoles.Select(r => r.Name))}] created and permissions granted on database [{database}].",
				$"Db User [{dbLogin.LoginName}] recreated on database [{database}]."
			}).ToArray();

			DbSecurityTest.AssertLogEntries(builder1.ToString(), expectedLogEntries);

			AssertLoginIsMappedToDatabase(TestAdminConnection, TestAdminConnection.CurrentDatabase, dbLogin.LoginName, expected: true);
			AssertUserIsMemberOfRole(TestAdminConnection, TestAdminConnection.CurrentDatabase, dbLogin.LoginName, "DatabaseTestRole");
			AssertLoginIsMappedToAllDatabases(allDatabases, dbLogin.LoginName, expected: true);
		}

		public void TestEnsureLoginCorrectlyMappedToAllDatabases_LanguageChanged()
		{
			var allDatabases = TestAdminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW & ~DatabaseType.SingleSharedRef);

			var dbLogin = DatabaseLogin;
			dbLogin.EnableLogin(msg => { });

			// language doesn't change mapping
			ChangeLoginDefaultLanguage(TestAdminConnection, dbLogin.LoginName, "norsk");

			var builder1 = new StringBuilder();
			dbLogin.EnsureLoginCorrectlyMappedToAllDatabases(msg => builder1.AppendLine(msg));

			var expectedLogEntries = allDatabases.SelectMany(database => new string[] {
				$"Db Role(s) [{string.Join(", ", dbLogin.DbLevelRoles.Select(r => r.Name))}] created and permissions granted on database [{database}].",
				$"Db User [{dbLogin.LoginName}] recreated on database [{database}]."
			}).ToArray();

			DbSecurityTest.AssertLogEntries(builder1.ToString(), expectedLogEntries);

			AssertLoginDefaultLanguage(TestAdminConnection, dbLogin.LoginName, "norsk");
			AssertLoginIsMappedToDatabase(TestAdminConnection, TestAdminConnection.CurrentDatabase, dbLogin.LoginName, expected: true);
			AssertUserIsMemberOfRole(TestAdminConnection, TestAdminConnection.CurrentDatabase, dbLogin.LoginName, "DatabaseTestRole");
			AssertLoginIsMappedToAllDatabases(allDatabases, dbLogin.LoginName, expected: true);
		}

		public void TestEnsureLoginCorrectlyMappedToAllDatabases_UserDropped()
		{
			var allDatabases = TestAdminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW & ~DatabaseType.SingleSharedRef);

			var dbLogin = DatabaseLogin;
			dbLogin.EnableLogin(msg => { });

			dbLogin.DropUserFromDatabase(TestAdminConnection.CurrentDatabase);

			var builder1 = new StringBuilder();
			dbLogin.EnsureLoginCorrectlyMappedToAllDatabases(msg => builder1.AppendLine(msg));

			var expectedLogEntries = allDatabases.SelectMany(database => new string[] {
				$"Db User [{dbLogin.LoginName}] created and permissions granted on database [{database}].",
				$"Db User [{dbLogin.LoginName}] recreated on database [{database}]."
			}).ToArray();

			AssertLoginIsMappedToDatabase(TestAdminConnection, TestAdminConnection.CurrentDatabase, dbLogin.LoginName, expected: true);
			AssertUserIsMemberOfRole(TestAdminConnection, TestAdminConnection.CurrentDatabase, dbLogin.LoginName, "DatabaseTestRole");
			AssertLoginIsMappedToAllDatabases(allDatabases, dbLogin.LoginName, expected: true);
		}

		public void TestEnsureLoginCorrectlyMappedToAllDatabases_DoNotImpersonateEnterpriseDbUser()
		{
			var allDatabases = TestAdminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW & ~DatabaseType.SingleSharedRef);
			AssertEquals("[PRE-CONDITION] The main database and at least one dependent database exist?", true, allDatabases.Count() > 1);

			foreach (var dbName in allDatabases)
			{
				CreateEnterpriseDbUser(TestAdminConnection, dbName, "testuser1");
			}

			var dbLogin = DatabaseLogin;

			// Create login
			dbLogin.EnableLogin(msg => { });

			var builder1 = new StringBuilder();
			dbLogin.EnsureLoginCorrectlyMappedToAllDatabases(msg => builder1.AppendLine(msg));

			var expectedLogEntries = allDatabases.SelectMany(database => new string[] {
				$"Db Role(s) [{string.Join(", ", dbLogin.DbLevelRoles.Select(r => r.Name))}] created and permissions granted on database [{database}].",
				$"Db User [{dbLogin.LoginName}] recreated on database [{database}]."
			}).ToArray();

			DbSecurityTest.AssertLogEntries(builder1.ToString(), expectedLogEntries);

			foreach (var dbName in allDatabases)
			{
				AssertLoginHasImpersonatePermissions(TestAdminConnection, dbName, dbLogin.LoginName, $"EnterpriseDbUser_{dbName}_testuser1", expected: false);
			}
		}

		public void TestEnsureLoginCorrectlyMappedToAllDatabases_ImpersonateEnterpriseDbUser()
		{
			var allDatabases = TestAdminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW & ~DatabaseType.SingleSharedRef);
			AssertEquals("[PRE-CONDITION] The main database and at least one dependent database exist?", true, allDatabases.Count() > 1);

			foreach (var dbName in allDatabases)
			{
				CreateEnterpriseDbUser(TestAdminConnection, dbName, "testuser1");
			}

			var dbLogin = new DatabaseLoginForTesting(TestAdminConnection, true);

			// Create login
			dbLogin.EnableLogin(msg => { });

			var builder1 = new StringBuilder();
			dbLogin.EnsureLoginCorrectlyMappedToAllDatabases(msg => builder1.AppendLine(msg));

			var expectedLogEntries = allDatabases.SelectMany(database => new string[] {
				$"Db Role(s) [{string.Join(", ", dbLogin.DbLevelRoles.Select(r => r.Name))}] created and permissions granted on database [{database}].",
				$"Db User [{dbLogin.LoginName}] recreated on database [{database}].",
				$"Enterprise Db User Impersonate permissions created for [{dbLogin.LoginName}]"
			}).ToArray();

			DbSecurityTest.AssertLogEntries(builder1.ToString(), expectedLogEntries);

			foreach (var dbName in allDatabases)
			{
				AssertLoginHasImpersonatePermissions(TestAdminConnection, dbName, dbLogin.LoginName, $"EnterpriseDbUser_{Db.DatabaseName}_testuser1", expected: TestAdminConnection.IsDbWriteable(dbName));
			}
		}

		static void ChangeLoginDefaultLanguage(DbConnection connection, string loginName, string language) => connection.ExecuteNonQuery($"ALTER LOGIN [{loginName}] WITH DEFAULT_LANGUAGE = {language}");

		void AssertLoginIsMappedToAllDatabases(IEnumerable<string> allDatabases, string loginName, bool expected)
		{
			foreach (var dbName in allDatabases)
			{
				AssertLoginIsMappedToDatabase(TestAdminConnection, dbName, loginName, expected);
			}
		}

		[ExpectNoExceptions]
		public void TestEnsureLoginHasRightsToCurrentDatabase_DatabaseTestRoleDoesNotExistOnOtherDatabase()
		{
			var dbLogin = DatabaseLogin;
			var dbs = TestAdminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW & ~DatabaseType.SingleSharedRef).Where(name => !name.Equals(TestAdminConnection.CurrentDatabase, StringComparison.OrdinalIgnoreCase));

			var otherDb = dbs.First();
			using (((ICurrentDbControl)TestAdminConnection).UseDatabase(otherDb))
			{
				var sql = $@"DECLARE @RoleName sysname
set @RoleName = N'DatabaseTestRole'

IF @RoleName <> N'public' and (select is_fixed_role from sys.database_principals where name = @RoleName) = 0
BEGIN
    DECLARE @RoleMemberName sysname
    DECLARE Member_Cursor CURSOR FOR
    select [name]
    from sys.database_principals 
    where principal_id in ( 
        select member_principal_id
        from sys.database_role_members
        where role_principal_id in (
            select principal_id
            FROM sys.database_principals where [name] = @RoleName AND type = 'R'))

    OPEN Member_Cursor;

    FETCH NEXT FROM Member_Cursor
    into @RoleMemberName

    DECLARE @SQL NVARCHAR(4000)

    WHILE @@FETCH_STATUS = 0
    BEGIN

        SET @SQL = 'ALTER ROLE '+ QUOTENAME(@RoleName,'[') +' DROP MEMBER '+ QUOTENAME(@RoleMemberName,'[')
        EXEC(@SQL)

        FETCH NEXT FROM Member_Cursor
        into @RoleMemberName
    END;

    CLOSE Member_Cursor;
    DEALLOCATE Member_Cursor;
END
DROP ROLE IF EXISTS [DatabaseTestRole]
";
				TestAdminConnection.ExecuteNonQuery(sql);
			}
			AdoTestUtils.DropDbLoginIfExists(TestAdminConnection, dbLogin.LoginName);

			dbLogin.EnsureLoginCorrectlyMappedToAllDatabases(msg => { });
		}

		public void TestEnsureLoginHasRightsToCurrentDatabase_NoLogin()
		{
			var dbLogin = DatabaseLogin;

			// Login doesn't exist => nothing should happen
			var builder1 = new StringBuilder();
			dbLogin.EnsureLoginCorrectlyMappedToAllDatabases(msg => builder1.AppendLine(msg));

			AssertEquals(string.Empty, builder1.ToString());
			AssertLoginIsMappedToDatabase(TestAdminConnection, TestAdminConnection.CurrentDatabase, dbLogin.LoginName, expected: false);
		}

		public void TestEnsureLoginHasRightsToCurrentDatabase_LoginExists()
		{
			var dbLogin = DatabaseLogin;
			dbLogin.EnableLogin(msg => { });

			dbLogin.EnsureLoginHasRightsToCurrentDatabase();

			AssertLoginDefaultLanguage(TestAdminConnection, dbLogin.LoginName, "us_english");
			AssertLoginIsMappedToDatabase(TestAdminConnection, TestAdminConnection.CurrentDatabase, dbLogin.LoginName, expected: true);
			AssertUserIsMemberOfRole(TestAdminConnection, TestAdminConnection.CurrentDatabase, dbLogin.LoginName, "DatabaseTestRole");
		}

		public void TestEnsureLoginHasRightsToCurrentDatabase_RenameUser()
		{
			var dbLogin = DatabaseLogin;
			dbLogin.EnableLogin(msg => { });
			dbLogin.EnsureLoginHasRightsToCurrentDatabase();

			// Rename user => should handle unmatching login <-> user names
			var query = String.Format(CultureInfo.InvariantCulture, "ALTER USER [{0}] WITH NAME = [{0}*]; CREATE USER [{0}] WITHOUT LOGIN;", dbLogin.LoginName);
			TestAdminConnection.ExecuteNonQuery(query);

			dbLogin.EnsureLoginHasRightsToCurrentDatabase();

			AssertLoginIsMappedToDatabase(TestAdminConnection, TestAdminConnection.CurrentDatabase, dbLogin.LoginName, expected: true);
			AssertUserIsMemberOfRole(TestAdminConnection, TestAdminConnection.CurrentDatabase, dbLogin.LoginName, "DatabaseTestRole");
		}

		public void TestEnsureLoginHasRightsToCurrentDatabaseWithOwnedRoles()
		{
			var dbLogin = DatabaseLogin;
			dbLogin.EnableLogin(msg => { });
			dbLogin.EnsureLoginHasRightsToCurrentDatabase();

			//Add a few roles
			CreateRoleWithSpecificOwner(TestAdminConnection, dbLogin.LoginName, "DatabaseLoginTest$Role1");
			CreateRoleWithSpecificOwner(TestAdminConnection, dbLogin.LoginName, "DatabaseLoginTest$Role2");

			dbLogin.EnsureLoginHasRightsToCurrentDatabase();

			AssertLoginIsMappedToDatabase(TestAdminConnection, TestAdminConnection.CurrentDatabase, dbLogin.LoginName, expected: true);
		}

		public void TestEnsureLoginHasRightsToCurrentDatabase_DoNotImpersonateDbUsers()
		{
			var dbLogin = DatabaseLogin;
			dbLogin.EnableLogin(msg => { });
			dbLogin.EnsureLoginHasRightsToCurrentDatabase();

			CreateEnterpriseDbUser(TestAdminConnection, Db.DatabaseName, "testuser1");
			CreateEnterpriseDbUser(TestAdminConnection, Db.DatabaseName, "testuser2");
			CreateEnterpriseDbUser(TestAdminConnection, Db.DatabaseName, "testuser3");

			dbLogin.EnsureLoginHasRightsToCurrentDatabase();

			AssertLoginIsMappedToDatabase(TestAdminConnection, TestAdminConnection.CurrentDatabase, dbLogin.LoginName, expected: true);

			AssertLoginHasImpersonatePermissions(TestAdminConnection, Db.DatabaseName, dbLogin.LoginName, $"EnterpriseDbUser_{Db.DatabaseName}_testuser1", expected: false);
			AssertLoginHasImpersonatePermissions(TestAdminConnection, Db.DatabaseName, dbLogin.LoginName, $"EnterpriseDbUser_{Db.DatabaseName}_testuser2", expected: false);
			AssertLoginHasImpersonatePermissions(TestAdminConnection, Db.DatabaseName, dbLogin.LoginName, $"EnterpriseDbUser_{Db.DatabaseName}_testuser3", expected: false);
		}

		public void TestEnsureLoginHasRightsToCurrentDatabase_ImpersonateDbUsers()
		{
			var dbLogin = new DatabaseLoginForTesting(TestAdminConnection, true);
			dbLogin.EnableLogin(msg => { });
			dbLogin.EnsureLoginHasRightsToCurrentDatabase();

			CreateEnterpriseDbUser(TestAdminConnection, Db.DatabaseName, "testuser1");
			CreateEnterpriseDbUser(TestAdminConnection, Db.DatabaseName, "testuser2");
			CreateEnterpriseDbUser(TestAdminConnection, Db.DatabaseName, "testuser3");

			//Add a few roles
			CreateRoleWithSpecificOwner(TestAdminConnection, dbLogin.LoginName, "DatabaseLoginTest$Role1");
			CreateRoleWithSpecificOwner(TestAdminConnection, dbLogin.LoginName, "DatabaseLoginTest$Role2");

			dbLogin.EnsureLoginHasRightsToCurrentDatabase();

			AssertLoginIsMappedToDatabase(TestAdminConnection, TestAdminConnection.CurrentDatabase, dbLogin.LoginName, expected: true);

			AssertLoginHasImpersonatePermissions(TestAdminConnection, Db.DatabaseName, dbLogin.LoginName, $"EnterpriseDbUser_{Db.DatabaseName}_testuser1", expected: true);
			AssertLoginHasImpersonatePermissions(TestAdminConnection, Db.DatabaseName, dbLogin.LoginName, $"EnterpriseDbUser_{Db.DatabaseName}_testuser2", expected: true);
			AssertLoginHasImpersonatePermissions(TestAdminConnection, Db.DatabaseName, dbLogin.LoginName, $"EnterpriseDbUser_{Db.DatabaseName}_testuser3", expected: true);
		}

		public void TestEnsureLoginHasRightsToRelevantDatabase()
		{
			var testDb = ((IPhysicalRefDbLocation)TestAdminConnection).GetReferenceDatabaseName(RefDbTypeEnum.Customs, "AU");

			AssertEnsureLoginHasRightsToRelevantDatabase(testDb);
			AssertEnsureLoginHasRightsToRelevantDatabase(testDb, true);
		}

		public void TestEnsureLoginHasRightsToRelevantDatabase_BiDb()
		{
			AssertEnsureLoginHasRightsToRelevantDatabase(Db.AuditDatabaseName);
			AssertEnsureLoginHasRightsToRelevantDatabase(Db.EdwDatabaseName);
			AssertEnsureLoginHasRightsToRelevantDatabase(Db.AuditDatabaseName, true);
			AssertEnsureLoginHasRightsToRelevantDatabase(Db.EdwDatabaseName, true);
		}

		void AssertEnsureLoginHasRightsToRelevantDatabase(string testDb, bool isImpersonateEnterpriseDbUser = false)
		{
			var dbLogin = new DatabaseLoginForTesting(TestAdminConnection, isImpersonateEnterpriseDbUser);
			dbLogin.DropUserFromDatabase(testDb);
			AdoTestUtils.DropDbLoginIfExists(TestAdminConnection, dbLogin.LoginName);

			// Login doesn't exist => nothing should happen
			var wasPermissionRestored = dbLogin.EnsureLoginHasRightsToRelevantDatabase(testDb);
			AssertEquals("Permission Restored?", false, wasPermissionRestored);
			AssertLoginIsMappedToDatabase(TestAdminConnection, testDb, dbLogin.LoginName, expected: false);
			AssertUserIsMemberOfRole(TestAdminConnection, testDb, dbLogin.LoginName, "DatabaseTestRole", expected: false);

			// Create login
			dbLogin.EnableLogin(msg => { });
			AssertLoginIsMappedToDatabase(TestAdminConnection, testDb, dbLogin.LoginName, expected: false);
			AssertUserIsMemberOfRole(TestAdminConnection, testDb, dbLogin.LoginName, "DatabaseTestRole", expected: false);

			// Login exists => should create a user for the login in the database
			wasPermissionRestored = dbLogin.EnsureLoginHasRightsToRelevantDatabase(testDb);
			AssertEquals("Permission Restored?", true, wasPermissionRestored);
			AssertLoginIsMappedToDatabase(TestAdminConnection, testDb, dbLogin.LoginName, expected: true);
			AssertUserIsMemberOfRole(TestAdminConnection, testDb, dbLogin.LoginName, "DatabaseTestRole");

			//changed mapped role - it should be changed back
			TestAdminConnection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "USE [{0}]; ALTER ROLE [DatabaseTestRole] DROP MEMBER {1};", testDb, dbLogin.LoginName));
			TestAdminConnection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "USE [{0}]; ALTER ROLE [db_datareader] ADD MEMBER {1};", testDb, dbLogin.LoginName));
			AssertLoginIsMappedToDatabase(TestAdminConnection, testDb, dbLogin.LoginName, expected: true);
			AssertUserIsMemberOfRole(TestAdminConnection, testDb, dbLogin.LoginName, "DatabaseTestRole", expected: false);
			AssertEquals(1, dbLogin.GetDatabasesWithIncorrectlyMappedUser(new string[] { testDb }, true).Count);
			wasPermissionRestored = dbLogin.EnsureLoginHasRightsToRelevantDatabase(testDb);
			AssertEquals(0, dbLogin.GetDatabasesWithIncorrectlyMappedUser(new string[] { testDb }, true).Count);
			AssertEquals("Permission Restored?", true, wasPermissionRestored);
			AssertLoginIsMappedToDatabase(TestAdminConnection, testDb, dbLogin.LoginName, expected: true);
			AssertUserIsMemberOfRole(TestAdminConnection, testDb, dbLogin.LoginName, "DatabaseTestRole");

			// Rename user => should handle unmatching login <-> user names
			var sqlText = String.Format(
				CultureInfo.InvariantCulture,
				"EXEC [{0}]..sp_executesql N'ALTER USER [{1}] WITH NAME = [{1}*]; CREATE USER [{1}] WITHOUT LOGIN;'",
				testDb,
				dbLogin.LoginName);
			TestAdminConnection.ExecuteNonQuery(sqlText);
			AssertLoginIsMappedToDatabase(TestAdminConnection, testDb, dbLogin.LoginName, expected: false);
			AssertUserIsMemberOfRole(TestAdminConnection, testDb, dbLogin.LoginName, "DatabaseTestRole", expected: false);
			AssertEquals(1, dbLogin.GetDatabasesWithIncorrectlyMappedUser(new string[] { testDb }, false).Count);
			wasPermissionRestored = dbLogin.EnsureLoginHasRightsToRelevantDatabase(testDb);
			AssertEquals(0, dbLogin.GetDatabasesWithIncorrectlyMappedUser(new string[] { testDb }, false).Count);
			AssertEquals("Permission Restored?", true, wasPermissionRestored);
			AssertLoginIsMappedToDatabase(TestAdminConnection, testDb, dbLogin.LoginName, expected: true);
			AssertUserIsMemberOfRole(TestAdminConnection, testDb, dbLogin.LoginName, "DatabaseTestRole");

			//Impersonate enterprise db users
			CreateEnterpriseDbUser(TestAdminConnection, testDb, "testuser1");
			CreateEnterpriseDbUser(TestAdminConnection, testDb, "testuser2");
			CreateEnterpriseDbUser(TestAdminConnection, testDb, "testuser3");

			dbLogin.EnsureLoginHasRightsToRelevantDatabase(testDb);

			AssertLoginHasImpersonatePermissions(TestAdminConnection, testDb, dbLogin.LoginName, $"EnterpriseDbUser_{Db.DatabaseName}_testuser1", expected: isImpersonateEnterpriseDbUser && TestAdminConnection.IsDbWriteable(testDb));
			AssertLoginHasImpersonatePermissions(TestAdminConnection, testDb, dbLogin.LoginName, $"EnterpriseDbUser_{Db.DatabaseName}_testuser2", expected: isImpersonateEnterpriseDbUser && TestAdminConnection.IsDbWriteable(testDb));
			AssertLoginHasImpersonatePermissions(TestAdminConnection, testDb, dbLogin.LoginName, $"EnterpriseDbUser_{Db.DatabaseName}_testuser3", expected: isImpersonateEnterpriseDbUser && TestAdminConnection.IsDbWriteable(testDb));
		}

		public void TestEnsureLoginHasRightsToRelevantDatabase_AlienDb()
		{
			var testDb = Db.SqlMasterDb;

			var dbLogin = new DatabaseLoginForTesting(TestAdminConnection, true);
			dbLogin.DropUserFromDatabase(testDb);
			AdoTestUtils.DropDbLoginIfExists(TestAdminConnection, dbLogin.LoginName);

			dbLogin.EnableLogin(msg => { });
			var sqlText = String.Format(
				CultureInfo.InvariantCulture,
				"EXEC [{0}]..sp_executesql N'CREATE USER [{1}] FOR LOGIN [{1}]'",
				testDb,
				dbLogin.LoginName
				);
			TestAdminConnection.ExecuteNonQuery(sqlText);
			AssertLoginIsMappedToDatabase(TestAdminConnection, testDb, dbLogin.LoginName, expected: true);

			CreateEnterpriseDbUser(TestAdminConnection, testDb, "testuser1");
			CreateEnterpriseDbUser(TestAdminConnection, testDb, "testuser2");
			CreateEnterpriseDbUser(TestAdminConnection, testDb, "testuser3");

			// Database does not belong to the system's set of database = not relevant => nothing should happen
			var wasPermissionRestored = dbLogin.EnsureLoginHasRightsToRelevantDatabase(testDb);
			AssertEquals("Permission Restored?", false, wasPermissionRestored);
			AssertUserIsMemberOfRole(TestAdminConnection, testDb, dbLogin.LoginName, "DatabaseTestRole", expected: false);
			AssertLoginIsMappedToDatabase(TestAdminConnection, testDb, dbLogin.LoginName, expected: true);

			AssertLoginHasImpersonatePermissions(TestAdminConnection, testDb, dbLogin.LoginName, $"EnterpriseDbUser_{testDb}_testuser1", expected: false);
			AssertLoginHasImpersonatePermissions(TestAdminConnection, testDb, dbLogin.LoginName, $"EnterpriseDbUser_{testDb}_testuser2", expected: false);
			AssertLoginHasImpersonatePermissions(TestAdminConnection, testDb, dbLogin.LoginName, $"EnterpriseDbUser_{testDb}_testuser3", expected: false);
		}

		public void TestDropUserFromDatabase()
		{
			var dbLogin = DatabaseLogin;
			dbLogin.EnableLogin(msg => { });
			dbLogin.EnsureLoginHasRightsToCurrentDatabase();
			AssertLoginIsMappedToDatabase(TestAdminConnection, TestAdminConnection.CurrentDatabase, dbLogin.LoginName, expected: true);

			dbLogin.DropUserFromDatabase(TestAdminConnection.CurrentDatabase);
			AssertLoginIsMappedToDatabase(TestAdminConnection, TestAdminConnection.CurrentDatabase, dbLogin.LoginName, expected: false);
		}

		[ExpectNoExceptions]
		public void TestEnableLoginWithEmptyPassword()
		{
			var dbLogin = DatabaseLogin;
			dbLogin.EnableLogin(msg => { });
			dbLogin.DisableLogin();
			TestAdminConnection.ExecuteNonQuery(string.Format("ALTER LOGIN [{0}] WITH PASSWORD = ''", dbLogin.LoginName));

			dbLogin.EnableLogin(msg => { });
		}

		public void TestIsSchemaPermissionExpected()
		{
			var restricedReaderLogin = RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName);
			var restricedWriterLogin = RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName);

			AssertEquals(true, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedReaderLogin, "SELECT", "dbo"));
			AssertEquals(false, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedReaderLogin, "INSERT", "dbo"));
			AssertEquals(false, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedReaderLogin, "UPDATE", "dbo"));
			AssertEquals(false, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedReaderLogin, "DELETE", "dbo"));
			AssertEquals(true, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedReaderLogin, "EXECUTE", "dbo"));
			AssertEquals(false, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedReaderLogin, "ALTER", "dbo"));
			AssertEquals(false, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedReaderLogin, "VIEW CHANGE TRACKING", "dbo"));

			AssertEquals(false, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedReaderLogin, "SELECT", DbSecurity.SqlHrmSchema));
			AssertEquals(false, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedReaderLogin, "INSERT", DbSecurity.SqlHrmSchema));
			AssertEquals(false, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedReaderLogin, "UPDATE", DbSecurity.SqlHrmSchema));
			AssertEquals(false, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedReaderLogin, "DELETE", DbSecurity.SqlHrmSchema));
			AssertEquals(false, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedReaderLogin, "EXECUTE", DbSecurity.SqlHrmSchema));
			AssertEquals(false, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedReaderLogin, "ALTER", DbSecurity.SqlHrmSchema));
			AssertEquals(false, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedReaderLogin, "VIEW CHANGE TRACKING", DbSecurity.SqlHrmSchema));

			AssertEquals(true, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedWriterLogin, "SELECT", "dbo"));
			AssertEquals(true, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedWriterLogin, "INSERT", "dbo"));
			AssertEquals(true, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedWriterLogin, "UPDATE", "dbo"));
			AssertEquals(true, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedWriterLogin, "DELETE", "dbo"));
			AssertEquals(true, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedWriterLogin, "EXECUTE", "dbo"));
			AssertEquals(true, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedWriterLogin, "ALTER", "dbo"));
			AssertEquals(true, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedWriterLogin, "CREATE SEQUENCE", "dbo"));
			AssertEquals(false, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedWriterLogin, "VIEW CHANGE TRACKING", "dbo"));

			AssertEquals(false, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedWriterLogin, "SELECT", DbSecurity.SqlHrmSchema));
			AssertEquals(false, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedWriterLogin, "INSERT", DbSecurity.SqlHrmSchema));
			AssertEquals(false, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedWriterLogin, "UPDATE", DbSecurity.SqlHrmSchema));
			AssertEquals(false, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedWriterLogin, "DELETE", DbSecurity.SqlHrmSchema));
			AssertEquals(false, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedWriterLogin, "EXECUTE", DbSecurity.SqlHrmSchema));
			AssertEquals(false, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedWriterLogin, "ALTER", DbSecurity.SqlHrmSchema));
			AssertEquals(false, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedWriterLogin, "CREATE SEQUENCE", DbSecurity.SqlHrmSchema));
			AssertEquals(false, DatabaseLogin.IsSchemaPermissionExpected(TestAdminConnection, Db.ServerName, Db.DatabaseName, restricedWriterLogin, "VIEW CHANGE TRACKING", DbSecurity.SqlHrmSchema));
		}

		public void TestIsApplicationLogin()
		{
			var loginNames = TestAdminConnection.Logins.Select(x => x.LoginName).ToArray();

			foreach (var loginName in loginNames)
			{
				Assert($"{loginName} is an application login", DatabaseLogin.IsApplicationLogin(loginName));
			}

			Assert("Enterprise db user is not an application login", !DatabaseLogin.IsApplicationLogin($"EnterpriseDbUser_{Db.DatabaseName}_BO7.Temp"));
		}

		public static void AssertLoginIsMappedToDatabase(AdminConnection connection, string dbName, string loginName, bool expected)
		{
			var checkUserSql = String.Format(CultureInfo.InvariantCulture,
				"SELECT count(*) FROM sys.server_principals l INNER JOIN [{0}].sys.database_principals u ON u.sid = l.sid WHERE l.name = '{1}' AND u.name = '{1}'",
				dbName, loginName);
			AssertEquals(string.Format("Does [{0}] login exist and is it mapped to database [{1}]?", loginName, dbName), expected, (int)connection.ExecuteScalar(checkUserSql) == 1);
		}

		public static void AssertUserIsMemberOfRole(DbConnection conn, string dbName, string loginName, string roleName, bool expected = true)
		{
			var sqlText = string.Format(@"
				IF exists (
					SELECT null
					FROM [{0}].sys.database_principals du
						INNER JOIN [{0}].sys.database_role_members drm ON drm.member_principal_id = du.principal_id
						INNER JOIN [{0}].sys.database_principals dr ON drm.role_principal_id = dr.principal_id
					WHERE
						du.name = '{1}'
						AND dr.name = '{2}')
					SELECT 1
					ELSE SELECT 0",
				dbName, loginName, roleName);

			AssertEquals(
				String.Format(CultureInfo.InvariantCulture, "Is [{0}] user a member of role [{1}] on database [{2}]?", loginName, roleName, dbName),
				expected,
				Convert.ToBoolean(conn.ExecuteScalar(sqlText)));
		}

		public static void AssertLoginHasImpersonatePermissions(AdminConnection connection, string dbName, string loginName, string enterpriseDbUser, bool expected)
		{
			var checkImpersonateSql = $@"IF exists (SELECT null FROM  [{dbName}].sys.database_permissions AS pe JOIN  [{dbName}].sys.database_principals AS pr1 ON pe.grantee_principal_id = pr1.principal_id 
JOIN  [{dbName}].sys.database_principals AS pr2 ON pe.grantor_principal_id = pr2.principal_id 
WHERE permission_name = 'IMPERSONATE' AND state = 'G' AND pr1.name = '{loginName}' AND pr2.name = '{enterpriseDbUser}')
SELECT 1
ELSE SELECT 0";

			AssertEquals(string.Format("Does [{0}] login exist and is granted impersonate to enterprise db user [{1}] on database [{2}]?", loginName, enterpriseDbUser, dbName), expected, Convert.ToBoolean(connection.ExecuteScalar(checkImpersonateSql)));
		}

		public static void AssertLoginIsExist(AdminConnection connection, string loginName, bool expected)
		{
			var sqlText = string.Format("SELECT COUNT(name) FROM sys.server_principals WHERE name = '{0}'", loginName);
			var result = connection.ExecuteScalar(sqlText);
			AssertEquals(string.Format("Is [{0}] login {1} exists?", loginName, (expected ? "" : "not")), expected, Convert.ToBoolean(result));
		}

		public static void AssertLoginIsEnabled(AdminConnection connection, string loginName, bool expected)
		{
			var sqlText = string.Format("IF exists (SELECT null FROM sys.server_principals WHERE name = '{0}' AND is_disabled = {1}) SELECT 1 ELSE SELECT 0", loginName, (expected ? "0" : "1"));
			AssertEquals(string.Format("Is [{0}] login {1}abled?", loginName, (expected ? "en" : "dis")), true, Convert.ToBoolean(connection.ExecuteScalar(sqlText)));
		}

		public static void AssertLoginDefaultLanguage(AdminConnection connection, string loginName, string expectedLanguage)
		{
			var sqlText = string.Format("SELECT default_language_name FROM sys.server_principals WHERE name = '{0}'", loginName);
			var objResult = connection.ExecuteScalar(sqlText);
			AssertEquals(string.Format("Login [{0}] default language", loginName), expectedLanguage, (objResult == null || objResult == DBNull.Value) ? null : objResult.ToString());
		}

		public static void AssertLoginPassword(AdminConnection connection, string loginName, string expectedPassword)
		{
			var sqlText = string.Format("IF EXISTS (SELECT NULL FROM sys.sql_logins WHERE name = '{0}' AND PWDCOMPARE('{1}', password_hash) = 1) SELECT 1 ELSE SELECT 0", loginName, expectedPassword.QuoteEscapedName('\''));
			AssertEquals(true, Convert.ToBoolean(connection.ExecuteScalar(sqlText)));
		}

		protected override void SetUp()
		{
			base.SetUp();
			AdoTestUtils.DropDbLoginIfExists(TestAdminConnection, DatabaseLogin.LoginName);
			var allDatabases = TestAdminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW & ~DatabaseType.SingleSharedRef).ToArray();
			var testDbRoleName = new DatabaseTestRole().Name;
			Array.ForEach(allDatabases, x => AdoTestUtils.DropDbRoleIfExists(TestAdminConnection, x, testDbRoleName));
		}

		protected override void TearDown()
		{
			base.TearDown();
			AdoTestUtils.DropDbLoginIfExists(TestAdminConnection, DatabaseLogin.LoginName);
			var allDatabases = TestAdminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW & ~DatabaseType.SingleSharedRef).ToArray();
			var testDbRoleName = new DatabaseTestRole().Name;
			Array.ForEach(allDatabases, x => AdoTestUtils.DropDbRoleIfExists(TestAdminConnection, x, testDbRoleName));
		}

		static void CreateRoleWithSpecificOwner(AdminConnection adminConnection, string loginName, string roleName) => adminConnection.ExecuteNonQuery($"CREATE ROLE [{roleName}] AUTHORIZATION [{loginName}]");

		static void CreateEnterpriseDbUser(AdminConnection adminConnection, string dbName, string userName)
		{
			using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
			{
				var enterpriseDbUserName = $"EnterpriseDbUser_{Db.DatabaseName}_{userName}";
				adminConnection.ExecuteNonQuery($@"IF NOT EXISTS(SELECT name FROM sys.database_principals WHERE name = '{enterpriseDbUserName}')
BEGIN
	CREATE USER {enterpriseDbUserName.QuoteName()} WITHOUT LOGIN;                                                     
END");
			}
		}

		public override void TestIsImpersonateEnterpriseDbUser()
		{
			var databaseLogin1 = new DatabaseLoginForTesting(TestAdminConnection);
			var databaseLogin2 = new DatabaseLoginForTesting(TestAdminConnection, true);

			Assert(!databaseLogin1.IsImpersonateEnterpriseDbUser);
			Assert(databaseLogin2.IsImpersonateEnterpriseDbUser);
		}

		protected override DatabaseLogin DatabaseLogin => new DatabaseLoginForTesting(TestAdminConnection);
	}
}
