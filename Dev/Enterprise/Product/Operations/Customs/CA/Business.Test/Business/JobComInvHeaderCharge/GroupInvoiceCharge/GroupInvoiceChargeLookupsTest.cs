using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class GroupInvoiceChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			var parent = Factory.New<GroupInvoiceCharge>();
			AssertEquals(parent.Lookups.Parent, parent);
		}
	}
}
