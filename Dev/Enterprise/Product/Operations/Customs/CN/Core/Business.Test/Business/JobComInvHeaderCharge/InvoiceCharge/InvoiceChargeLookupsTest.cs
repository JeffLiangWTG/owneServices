using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class InvoiceChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			var parent = Factory.New<InvoiceCharge>();
			AssertEquals(parent.Lookups.Parent, parent);
		}
	}
}
