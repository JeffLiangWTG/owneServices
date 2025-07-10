using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class AddressTypeListTest : TransactionedTestCase
	{
		public void TestContainsAll()
		{
			var addressTypeList = new AddressTypeList();
			AssertEquals(true, addressTypeList.ContainsCode("All"));
		}
	}
}
