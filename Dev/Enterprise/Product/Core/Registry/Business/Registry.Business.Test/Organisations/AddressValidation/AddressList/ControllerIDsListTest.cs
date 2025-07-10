using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class ControllerIDsListTest : TransactionedTestCase
	{
		public void TestContainsAll()
		{
			var controllerIDsList = new ControllerIDsList();
			AssertEquals(true, controllerIDsList.ContainsCode("All"));
		}
	}
}
