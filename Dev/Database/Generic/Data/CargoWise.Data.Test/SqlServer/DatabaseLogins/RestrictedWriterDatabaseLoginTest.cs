using System.Linq;

namespace CargoWise.Data.Testing
{
	sealed class RestrictedWriterDatabaseLoginTest : DatabaseLoginTestCase
	{
		public void TestLoginName()
		{
			AssertEquals($"{TestAdminConnection.CurrentDatabase}_RestrictedWriterLogin", DatabaseLogin.LoginName);
		}

		public void TestLoginSuffix()
		{
			AssertEquals("RestrictedWriterLogin", DatabaseLogin.LoginSuffix);
		}

		public void TestDbLevelRoles()
		{
			var login = DatabaseLogin;

			AssertEquals(1, login.DbLevelRoles.Count());
			AssertType<CwRestrictedWriterRole>(login.DbLevelRoles.FirstOrDefault());
		}

		protected override DatabaseLogin DatabaseLogin => new RestrictedWriterDatabaseLogin(TestAdminConnection);
	}
}
