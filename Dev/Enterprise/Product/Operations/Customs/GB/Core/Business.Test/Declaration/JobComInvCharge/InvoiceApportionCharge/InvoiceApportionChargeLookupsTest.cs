namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class InvoiceApportionChargeLookupsTest : EU.Business.Declaration.Testing.InvoiceApportionChargeLookupsTest
	{
		public new void TestParent()
		{
			var parent = Factory.New<InvoiceApportionCharge>();
			AssertEquals(parent.Lookups.Parent, parent);
		}
	}
}
