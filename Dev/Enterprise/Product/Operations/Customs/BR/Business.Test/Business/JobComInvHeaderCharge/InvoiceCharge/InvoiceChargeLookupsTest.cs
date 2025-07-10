using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class InvoiceChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			InvoiceCharge parent = Factory.New<InvoiceCharge>();
			AssertEquals(parent.Lookups.Parent, parent);
		}

		public void TestChargeDistributionBy()
		{
			var invoiceCharge = Factory.New<InvoiceCharge>();
			AssertSame(Factory.GetCachedValue<ChargeDistributeByList>(), invoiceCharge.Lookups.ChargeDistributionBy);
		}
	}
}
