using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class InvoiceLineApportionChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			var parent = Factory.New<InvoiceLineApportionCharge>();
			AssertEquals(parent.Lookups.Parent, parent);
		}
	}
}
