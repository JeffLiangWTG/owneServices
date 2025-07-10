namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	class InvoiceChargeLookupsTest : EU.Business.Declaration.Testing.InvoiceChargeLookupsTest
	{
		public new void TestParent()
		{
			var parent = Factory.New<InvoiceCharge>();
			AssertEquals(parent.Lookups.Parent, parent);
		}
	}
}
