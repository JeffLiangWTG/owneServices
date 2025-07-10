using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class InvoiceLineChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestChargeDistributionBy()
		{
			var invoiceLineCharge = Factory.New<InvoiceLineCharge>();
			AssertSame(Factory.GetCachedValue<ChargeDistributeByList>(), invoiceLineCharge.Lookups.ChargeDistributionBy);
		}
	}
}

