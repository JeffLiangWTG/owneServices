using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class InvoiceLineApportionChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestChargeDistributionBy()
		{
			var invoiceLineApportionCharge = Factory.New<InvoiceLineApportionCharge>();
			AssertSame(Factory.GetCachedValue<ChargeDistributeByList>(), invoiceLineApportionCharge.Lookups.ChargeDistributionBy);
		}
	}
}


