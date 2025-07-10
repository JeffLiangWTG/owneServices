using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class InvoiceChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			InvoiceCharge parent = Factory.New<InvoiceCharge>();
			AssertEquals(parent.Lookups.Parent, parent);
		}
	}
}
