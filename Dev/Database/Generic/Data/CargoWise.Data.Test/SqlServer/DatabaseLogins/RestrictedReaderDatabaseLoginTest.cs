using System.Linq;

namespace CargoWise.Data.Testing
{
	sealed class RestrictedReaderDatabaseLoginTest : DatabaseLoginTestCase
	{
		public void TestLoginName()
		{
			AssertEquals($"{TestAdminConnection.CurrentDatabase}_RestrictedReaderLogin", DatabaseLogin.LoginName);
		}

		public void TestLoginSuffix()
		{
			AssertEquals("RestrictedReaderLogin", DatabaseLogin.LoginSuffix);
		}

		public void TestDbLevelRoles()
		{
			var login = DatabaseLogin;

			AssertEquals(1, login.DbLevelRoles.Count());
			AssertType<CwRestrictedReaderRole>(login.DbLevelRoles.FirstOrDefault());
		}

		[UseSnapshotProtection]
		public void TestTableAccessibilies_NoSHOWPLANPermission()
		{
			var login = DatabaseLogin;
			using (var testConnection = Db.NewAdminConnection(Db.Connection.CurrentDatabase))
			{
				((IDbLoginRepair)testConnection).EnsureDbLoginsHaveRightsToCurrentDatabase();
				testConnection.ExecuteNonQuery($"REVOKE SHOWPLAN FROM cwRestrictedReaderRole");
			}

			Db.Connection.ExecuteNonQuery($"EXECUTE AS USER = '{login.LoginName}';");
			Db.Connection.ExecuteNonQuery("SET SHOWPLAN_TEXT ON;");

			AssertExceptionThrown("Select query of a dbo table should be blocked by SHOWPLAN permission", typeof(SqlException), $"SHOWPLAN permission denied in database '{Db.Connection.CurrentDatabase}'.", SelectDboTable, assertStartsWith: true);
			AssertExceptionThrown("Select query of a hrm table should be blocked by schema SELECT permission", typeof(SqlException), $"The SELECT permission was denied on the object 'GlbStaffRemuneration', database '{Db.Connection.CurrentDatabase}', schema 'hrm'.", SelectHRMTable, assertStartsWith: true);

			Db.Connection.ExecuteNonQuery("SET SHOWPLAN_TEXT OFF;");
			Db.Connection.ExecuteNonQuery("REVERT;");
		}

		[UseSnapshotProtection]
		public void TestTableAccessibilies_HasSHOWPLANPermission()
		{
			var login = DatabaseLogin;
			using (var testConnection = Db.NewAdminConnection(Db.Connection.CurrentDatabase))
			{
				((IDbLoginRepair)testConnection).EnsureDbLoginsHaveRightsToCurrentDatabase();
				testConnection.ExecuteNonQuery($"GRANT SHOWPLAN TO cwRestrictedReaderRole");
			}

			Db.Connection.ExecuteNonQuery($"EXECUTE AS USER = '{login.LoginName}';");
			Db.Connection.ExecuteNonQuery("SET SHOWPLAN_TEXT ON;");

			AssertNoExceptionThrown("Select query of a dbo table should pass", SelectDboTable);
			AssertExceptionThrown("Select query of a hrm table should be blocked by schema SELECT permission", typeof(SqlException), $"The SELECT permission was denied on the object 'GlbStaffRemuneration', database '{Db.Connection.CurrentDatabase}', schema 'hrm'.", SelectHRMTable, assertStartsWith: true);

			Db.Connection.ExecuteNonQuery("SET SHOWPLAN_TEXT OFF;");
			Db.Connection.ExecuteNonQuery("REVERT;");
		}

		void SelectDboTable() => Db.Connection.ExecuteNonQuery("SELECT COUNT(*) FROM [GlbBranch]");

		void SelectHRMTable() => Db.Connection.ExecuteNonQuery("SELECT COUNT(*) FROM [hrm].[GlbStaffRemuneration]");

		protected override DatabaseLogin DatabaseLogin => new RestrictedReaderDatabaseLogin(TestAdminConnection);
	}
}
