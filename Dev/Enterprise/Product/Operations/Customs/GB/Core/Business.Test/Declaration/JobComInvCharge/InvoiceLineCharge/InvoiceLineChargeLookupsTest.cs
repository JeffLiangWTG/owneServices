using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class InvoiceLineChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			var parent = Factory.New<InvoiceLineCharge>();
			AssertEquals(parent.Lookups.Parent, parent);
		}
	}
}
