using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class InvoiceLineApportionChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			var parent = Factory.New<InvoiceLineApportionCharge>();
			AssertEquals(parent.Lookups.Parent, parent);
		}
	}
}
