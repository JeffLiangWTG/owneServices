using Enterprise.Customs.Common;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class InvoiceChargeLookupsTest : EU.Business.Declaration.Testing.InvoiceChargeLookupsTest
	{
		public new void TestParent()
		{
			var parent = Factory.New<InvoiceCharge>();
			AssertEquals(parent.Lookups.Parent, parent);
		}

		public void TestChargeDistributeByList()
		{
			var parent = Factory.New<InvoiceCharge>();
			AssertEquals("Only two options", 2, parent.Lookups.ChargeDistributionBy.Count);
			Assert(parent.Lookups.ChargeDistributionBy.ContainsCode(ChargeDistributeByList.Codes.Value));
			Assert(parent.Lookups.ChargeDistributionBy.ContainsCode(ChargeDistributeByList.Codes.Weight));
		}
	}
}
