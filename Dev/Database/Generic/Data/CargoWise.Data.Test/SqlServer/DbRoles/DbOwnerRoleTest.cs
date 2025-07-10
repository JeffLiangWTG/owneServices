using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DbOwnerRoleTest : TestCase
	{
		public void TestRoleName()
		{
			AssertEquals("db_owner", new DbOwnerRole().Name);
		}
	}
}
