using System.Linq;

namespace CargoWise.Data.Testing
{
	sealed class ReaderDatabaseLoginTest : DatabaseLoginTestCase
	{
		public void TestLoginName()
		{
			AssertEquals($"{TestAdminConnection.CurrentDatabase}_CargoWiseReaderLogin", DatabaseLogin.LoginName);
		}

		public void TestLoginSuffix()
		{
			AssertEquals("CargoWiseReaderLogin", DatabaseLogin.LoginSuffix);
		}

		public void TestDbLevelRoles()
		{
			var login = DatabaseLogin;

			AssertEquals(2, login.DbLevelRoles.Count());
			AssertType<CwReaderRole>(login.DbLevelRoles.ElementAt(0));
			AssertType<DbDataReaderRole>(login.DbLevelRoles.ElementAt(1));
		}

		protected override DatabaseLogin DatabaseLogin => new ReaderDatabaseLogin(TestAdminConnection);
	}
}
