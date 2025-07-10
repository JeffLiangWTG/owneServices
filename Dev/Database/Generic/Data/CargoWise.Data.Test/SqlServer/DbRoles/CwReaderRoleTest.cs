using CargoWise.Data.SqlServer.Testing;
using CargoWise.DataProtection;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class CwReaderRoleTest : TestCase
	{
		public void TestRoleName()
		{
			AssertEquals("cwReaderRole", cwReaderRole.Name);
		}

		public void TestGetRefreshSQLFullSQL()
		{
			var cwReaderRole = new CwReaderRole();

			cwReaderRole.GetRefreshRoleSQL(testConnection, Db.DatabaseName, Db.DatabaseName, true, out var cmdBuilder_Odyssey);
			cwReaderRole.GetRefreshRoleSQL(testConnection, Db.DatabaseName, testRefDb, false, out var cmdBuilder_RefDb);

			var sqlOdyssey = cmdBuilder_Odyssey.ToString();
			var sqlRefDb = cmdBuilder_RefDb.ToString();

			AssertContains("CREATE ROLE [cwReaderRole];", sqlOdyssey);
			AssertContains("ALTER ROLE [db_datareader] ADD MEMBER [cwReaderRole];", sqlOdyssey);
			AssertContains("ALTER AUTHORIZATION ON ROLE::[cwReaderRole] TO [dbo]", sqlOdyssey);
			AssertContains("GRANT VIEW DEFINITION TO [cwReaderRole];", sqlOdyssey);
			AssertContains("GRANT SHOWPLAN TO [cwReaderRole];", sqlOdyssey);
			AssertContains($"if exists (select 1 from sys.Database_Principals where name = '{dbReaderLoginName}') ALTER ROLE [cwReaderRole] ADD MEMBER [{dbReaderLoginName}]", sqlOdyssey);
			AssertContains("ALTER AUTHORIZATION ON [dbo].", sqlOdyssey);
			AssertContains("GRANT EXECUTE ON ", sqlOdyssey);
			AssertContains("GRANT EXEC ON TYPE::", sqlOdyssey);

			AssertContains("CREATE ROLE [cwReaderRole];", sqlRefDb);
			AssertContains("ALTER ROLE [db_datareader] ADD MEMBER [cwReaderRole];", sqlRefDb);
			AssertContains("ALTER AUTHORIZATION ON ROLE::[cwReaderRole] TO [dbo]", sqlRefDb);
			AssertContains("GRANT VIEW DEFINITION TO [cwReaderRole];", sqlRefDb);
			AssertContains("GRANT SHOWPLAN TO [cwReaderRole];", sqlRefDb);
			AssertContains($"if exists (select 1 from sys.Database_Principals where name = '{dbReaderLoginName}') ALTER ROLE [cwReaderRole] ADD MEMBER [{dbReaderLoginName}]", sqlRefDb);
			AssertNotContains("ALTER AUTHORIZATION ON [dbo].", sqlRefDb);
			AssertNotContains("GRANT EXECUTE ON ", sqlRefDb);
			AssertNotContains("GRANT EXEC ON TYPE::", sqlRefDb);
		}

		public void TestGetRefreshSQLRoleExists()
		{
			CreateRole(Db.DatabaseName, testRefDb);

			var cwReaderRole = new CwReaderRole();

			cwReaderRole.GetRefreshRoleSQL(testConnection, Db.DatabaseName, Db.DatabaseName, true, out var cmdBuilder_Odyssey);
			cwReaderRole.GetRefreshRoleSQL(testConnection, Db.DatabaseName, testRefDb, false, out var cmdBuilder_RefDb);

			var sqlOdyssey = cmdBuilder_Odyssey.ToString();
			var sqlRefDb = cmdBuilder_RefDb.ToString();

			AssertNotContains("CREATE ROLE [cwReaderRole];", sqlOdyssey);
			AssertContains("ALTER ROLE [db_datareader] ADD MEMBER [cwReaderRole];", sqlOdyssey);
			AssertContains("ALTER AUTHORIZATION ON ROLE::[cwReaderRole] TO [dbo]", sqlOdyssey);
			AssertContains("GRANT VIEW DEFINITION TO [cwReaderRole];", sqlOdyssey);
			AssertContains("GRANT SHOWPLAN TO [cwReaderRole];", sqlOdyssey);
			AssertContains($"if exists (select 1 from sys.Database_Principals where name = '{dbReaderLoginName}') ALTER ROLE [cwReaderRole] ADD MEMBER [{dbReaderLoginName}]", sqlOdyssey);
			AssertContains("ALTER AUTHORIZATION ON [dbo].", sqlOdyssey);
			AssertContains("GRANT EXECUTE ON ", sqlOdyssey);
			AssertContains("GRANT EXEC ON TYPE::", sqlOdyssey);

			AssertNotContains("CREATE ROLE [cwReaderRole];", sqlRefDb);
			AssertContains("ALTER ROLE [db_datareader] ADD MEMBER [cwReaderRole];", sqlRefDb);
			AssertContains("ALTER AUTHORIZATION ON ROLE::[cwReaderRole] TO [dbo]", sqlRefDb);
			AssertContains("GRANT VIEW DEFINITION TO [cwReaderRole];", sqlRefDb);
			AssertContains("GRANT SHOWPLAN TO [cwReaderRole];", sqlRefDb);
			AssertContains($"if exists (select 1 from sys.Database_Principals where name = '{dbReaderLoginName}') ALTER ROLE [cwReaderRole] ADD MEMBER [{dbReaderLoginName}]", sqlRefDb);
			AssertNotContains("ALTER AUTHORIZATION ON [dbo].", sqlRefDb);
			AssertNotContains("GRANT EXECUTE ON ", sqlRefDb);
			AssertNotContains("GRANT EXEC ON TYPE::", sqlRefDb);
		}

		public void TestGetRefreshSQLRoleExistsRoleIsMemeberOfDataReader()
		{
			CreateRole(Db.DatabaseName, testRefDb);

			var sqlText = string.Format(@"
				EXEC [{0}]..sp_executesql N'ALTER ROLE [db_datareader] ADD MEMBER [cwReaderRole]; ALTER AUTHORIZATION ON ROLE::[cwReaderRole] TO [dbo];';
				EXEC [{1}]..sp_executesql N'ALTER ROLE [db_datareader] ADD MEMBER [cwReaderRole]; ALTER AUTHORIZATION ON ROLE::[cwReaderRole] TO [dbo];';",
				Db.DatabaseName, testRefDb);
			testConnection.ExecuteNonQuery(sqlText);

			var cwReaderRole = new CwReaderRole();

			cwReaderRole.GetRefreshRoleSQL(testConnection, Db.DatabaseName, Db.DatabaseName, true, out var cmdBuilder_Odyssey);
			cwReaderRole.GetRefreshRoleSQL(testConnection, Db.DatabaseName, testRefDb, false, out var cmdBuilder_RefDb);

			var sqlOdyssey = cmdBuilder_Odyssey.ToString();
			var sqlRefDb = cmdBuilder_RefDb.ToString();

			AssertNotContains("CREATE ROLE [cwReaderRole];", sqlOdyssey);
			AssertNotContains("ALTER ROLE [db_datareader] ADD MEMBER [cwReaderRole];", sqlOdyssey);
			AssertNotContains("ALTER AUTHORIZATION ON ROLE::[cwReaderRole] TO [dbo]", sqlOdyssey);
			AssertContains("GRANT VIEW DEFINITION TO [cwReaderRole];", sqlOdyssey);
			AssertContains("GRANT SHOWPLAN TO [cwReaderRole];", sqlOdyssey);
			AssertContains($"if exists (select 1 from sys.Database_Principals where name = '{dbReaderLoginName}') ALTER ROLE [cwReaderRole] ADD MEMBER [{dbReaderLoginName}]", sqlOdyssey);
			AssertContains("ALTER AUTHORIZATION ON [dbo].", sqlOdyssey);
			AssertContains("GRANT EXECUTE ON ", sqlOdyssey);
			AssertContains("GRANT EXEC ON TYPE::", sqlOdyssey);

			AssertNotContains("CREATE ROLE [cwReaderRole];", sqlRefDb);
			AssertNotContains("ALTER ROLE [db_datareader] ADD MEMBER [cwReaderRole];", sqlRefDb);
			AssertNotContains("ALTER AUTHORIZATION ON ROLE::[cwReaderRole] TO [dbo]", sqlRefDb);
			AssertContains("GRANT VIEW DEFINITION TO [cwReaderRole];", sqlRefDb);
			AssertContains("GRANT SHOWPLAN TO [cwReaderRole];", sqlRefDb);
			AssertContains($"if exists (select 1 from sys.Database_Principals where name = '{dbReaderLoginName}') ALTER ROLE [cwReaderRole] ADD MEMBER [{dbReaderLoginName}]", sqlRefDb);
			AssertNotContains("ALTER AUTHORIZATION ON [dbo].", sqlRefDb);
			AssertNotContains("GRANT EXECUTE ON ", sqlRefDb);
			AssertNotContains("GRANT EXEC ON TYPE::", sqlRefDb);
		}

		public void TestGetRefreshSQLViewDefinitionGranted()
		{
			CreateRole(Db.DatabaseName, testRefDb);

			var sqlText = string.Format(@"
				EXEC [{0}]..sp_executesql N'GRANT VIEW DEFINITION TO [cwReaderRole];';
				EXEC [{1}]..sp_executesql N'GRANT VIEW DEFINITION TO [cwReaderRole];';",
				Db.DatabaseName, testRefDb);
			testConnection.ExecuteNonQuery(sqlText);

			var cwReaderRole = new CwReaderRole();

			cwReaderRole.GetRefreshRoleSQL(testConnection, Db.DatabaseName, Db.DatabaseName, true, out var cmdBuilder_Odyssey);
			cwReaderRole.GetRefreshRoleSQL(testConnection, Db.DatabaseName, testRefDb, false, out var cmdBuilder_RefDb);

			var sqlOdyssey = cmdBuilder_Odyssey.ToString();
			var sqlRefDb = cmdBuilder_RefDb.ToString();

			AssertNotContains("CREATE ROLE [cwReaderRole];", sqlOdyssey);
			AssertContains("ALTER ROLE [db_datareader] ADD MEMBER [cwReaderRole];", sqlOdyssey);
			AssertContains("ALTER AUTHORIZATION ON ROLE::[cwReaderRole] TO [dbo]", sqlOdyssey);
			AssertNotContains("GRANT VIEW DEFINITION TO [cwReaderRole];", sqlOdyssey);
			AssertContains("GRANT SHOWPLAN TO [cwReaderRole];", sqlOdyssey);
			AssertContains($"if exists (select 1 from sys.Database_Principals where name = '{dbReaderLoginName}') ALTER ROLE [cwReaderRole] ADD MEMBER [{dbReaderLoginName}]", sqlOdyssey);
			AssertContains("ALTER AUTHORIZATION ON [dbo].", sqlOdyssey);
			AssertContains("GRANT EXECUTE ON ", sqlOdyssey);
			AssertContains("GRANT EXEC ON TYPE::", sqlOdyssey);

			AssertNotContains("CREATE ROLE [cwReaderRole];", sqlRefDb);
			AssertContains("ALTER ROLE [db_datareader] ADD MEMBER [cwReaderRole];", sqlRefDb);
			AssertContains("ALTER AUTHORIZATION ON ROLE::[cwReaderRole] TO [dbo]", sqlRefDb);
			AssertNotContains("GRANT VIEW DEFINITION TO [cwReaderRole];", sqlRefDb);
			AssertContains("GRANT SHOWPLAN TO [cwReaderRole];", sqlRefDb);
			AssertContains($"if exists (select 1 from sys.Database_Principals where name = '{dbReaderLoginName}') ALTER ROLE [cwReaderRole] ADD MEMBER [{dbReaderLoginName}]", sqlRefDb);
			AssertNotContains("ALTER AUTHORIZATION ON [dbo].", sqlRefDb);
			AssertNotContains("GRANT EXECUTE ON ", sqlRefDb);
			AssertNotContains("GRANT EXEC ON TYPE::", sqlRefDb);
		}

		public void TestGetRefreshSQLShowPlanGranted()
		{
			CreateRole(Db.DatabaseName, testRefDb);

			var sqlText = string.Format(@"
				EXEC [{0}]..sp_executesql N'GRANT SHOWPLAN TO [cwReaderRole];';
				EXEC [{1}]..sp_executesql N'GRANT SHOWPLAN TO [cwReaderRole];';",
				Db.DatabaseName, testRefDb);
			testConnection.ExecuteNonQuery(sqlText);

			var cwReaderRole = new CwReaderRole();

			cwReaderRole.GetRefreshRoleSQL(testConnection, Db.DatabaseName, Db.DatabaseName, true, out var cmdBuilder_Odyssey);
			cwReaderRole.GetRefreshRoleSQL(testConnection, Db.DatabaseName, testRefDb, false, out var cmdBuilder_RefDb);

			var sqlOdyssey = cmdBuilder_Odyssey.ToString();
			var sqlRefDb = cmdBuilder_RefDb.ToString();

			AssertNotContains("CREATE ROLE [cwReaderRole];", sqlOdyssey);
			AssertContains("ALTER ROLE [db_datareader] ADD MEMBER [cwReaderRole];", sqlOdyssey);
			AssertContains("ALTER AUTHORIZATION ON ROLE::[cwReaderRole] TO [dbo]", sqlOdyssey);
			AssertContains("GRANT VIEW DEFINITION TO [cwReaderRole];", sqlOdyssey);
			AssertNotContains("GRANT SHOWPLAN TO [cwReaderRole];", sqlOdyssey);
			AssertContains($"if exists (select 1 from sys.Database_Principals where name = '{dbReaderLoginName}') ALTER ROLE [cwReaderRole] ADD MEMBER [{dbReaderLoginName}]", sqlOdyssey);
			AssertContains("ALTER AUTHORIZATION ON [dbo].", sqlOdyssey);
			AssertContains("GRANT EXECUTE ON ", sqlOdyssey);
			AssertContains("GRANT EXEC ON TYPE::", sqlOdyssey);

			AssertNotContains("CREATE ROLE [cwReaderRole];", sqlRefDb);
			AssertContains("ALTER ROLE [db_datareader] ADD MEMBER [cwReaderRole];", sqlRefDb);
			AssertContains("ALTER AUTHORIZATION ON ROLE::[cwReaderRole] TO [dbo]", sqlRefDb);
			AssertContains("GRANT VIEW DEFINITION TO [cwReaderRole];", sqlRefDb);
			AssertNotContains("GRANT SHOWPLAN TO [cwReaderRole];", sqlRefDb);
			AssertContains($"if exists (select 1 from sys.Database_Principals where name = '{dbReaderLoginName}') ALTER ROLE [cwReaderRole] ADD MEMBER [{dbReaderLoginName}]", sqlRefDb);
			AssertNotContains("ALTER AUTHORIZATION ON [dbo].", sqlRefDb);
			AssertNotContains("GRANT EXECUTE ON ", sqlRefDb);
			AssertNotContains("GRANT EXEC ON TYPE::", sqlRefDb);
		}

		void CreateRole(string odysseyDb, string testRefDb)
		{
			var sqlText = string.Format(@"
				EXEC [{0}]..sp_executesql N'CREATE ROLE [cwReaderRole]';
				EXEC [{1}]..sp_executesql N'CREATE ROLE [cwReaderRole]';",
				odysseyDb, testRefDb);
			testConnection.ExecuteNonQuery(sqlText);
		}

		protected override void SetUp()
		{
			base.SetUp();

			testConnection = Db.NewAdminConnection();

			DbSecurityTest.DropRoleOnDatabase(testConnection, Db.DatabaseName, "cwReaderRole");

			testRefDb = ((IPhysicalRefDbLocation)testConnection).GetReferenceDatabaseName(RefDbTypeEnum.Enterprise, "CA");
			DbSecurityTest.DropRoleOnDatabase(testConnection, testRefDb, "cwReaderRole");

			cwReaderRole = new CwReaderRole();

			dbReaderLoginName = CargoWiseReaderLoginCredentials.UserNameFor(Db.DatabaseName);
		}

		protected override void TearDown()
		{
			new DbSecurity().RefreshDbReaderRolePermissions(testConnection, logMessage => { });

			testConnection.Dispose();
			testConnection = null;

			base.TearDown();
		}

		AdminConnection testConnection;
		CwReaderRole cwReaderRole;

		string testRefDb;
		string dbReaderLoginName;
	}
}
