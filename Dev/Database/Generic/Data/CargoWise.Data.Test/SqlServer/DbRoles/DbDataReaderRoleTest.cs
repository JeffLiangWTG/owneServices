using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DbDataReaderRoleTest : TestCase
	{
		public void TestRoleName()
		{
			AssertEquals("db_datareader", new DbDataReaderRole().Name);
		}
	}
}
