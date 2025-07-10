using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZPropertyInfoDateTest : TestCaseWithDummy
	{
		public void TestCanSetEmpty()
		{
			Dummy.Z0_Date = ZDate.Empty;
			AssertEquals(Dummy.Z0_Date, ZDate.Empty);
		}
	}
}
