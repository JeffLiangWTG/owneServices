using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class InvoiceLineChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			var parent = Factory.New<InvoiceLineCharge>();
			AssertEquals(parent.Lookups.Parent, parent);
		}
	}
}
