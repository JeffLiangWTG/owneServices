using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class GroupInvoiceChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			GroupInvoiceCharge parent = Factory.New<GroupInvoiceCharge>();
			AssertEquals(parent.Lookups.Parent, parent);
		}
	}
}
