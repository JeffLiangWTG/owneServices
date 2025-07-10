namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class InvoiceLineChargeLookupsTest : EU.Business.Declaration.Testing.InvoiceLineChargeLookupsTest
	{
		public new void TestParent()
		{
			var parent = Factory.New<InvoiceLineCharge>();
			AssertEquals(parent.Lookups.Parent, parent);
		}
	}
}
