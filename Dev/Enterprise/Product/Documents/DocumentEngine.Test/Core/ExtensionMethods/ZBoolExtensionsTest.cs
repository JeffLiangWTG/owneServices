using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class ZBoolExtensionsTest : TestCaseWithFactory
	{
		public void TestToYN()
		{
			AssertEquals("Y", ZBool.True.ToYN());
			AssertEquals("N", ZBool.False.ToYN());
		}
	}
}
