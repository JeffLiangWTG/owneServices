using System.Linq;

namespace CargoWise.Data.Testing
{
	sealed class UnrestrictedWriterDatabaseLoginTest : DatabaseLoginTestCase
	{
		public void TestLoginName()
		{
			AssertEquals($"{TestAdminConnection.CurrentDatabase}_UnrestrictedWriterLogin", DatabaseLogin.LoginName);
		}

		public void TestLoginSuffix()
		{
			AssertEquals("UnrestrictedWriterLogin", DatabaseLogin.LoginSuffix);
		}

		public void TestDbLevelRoles()
		{
			var login = DatabaseLogin;

			AssertEquals(1, login.DbLevelRoles.Count());
			AssertType<CwUnrestrictedWriterRole>(login.DbLevelRoles.FirstOrDefault());
		}

		public override void TestIsImpersonateEnterpriseDbUser()
		{
			Assert(DatabaseLogin.IsImpersonateEnterpriseDbUser);
		}

		protected override DatabaseLogin DatabaseLogin => new UnrestrictedWriterDatabaseLogin(TestAdminConnection);
	}
}
