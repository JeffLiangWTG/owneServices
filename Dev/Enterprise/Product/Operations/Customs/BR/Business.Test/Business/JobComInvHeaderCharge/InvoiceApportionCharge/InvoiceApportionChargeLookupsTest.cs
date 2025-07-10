using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class InvoiceApportionChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			InvoiceApportionCharge parent = Factory.New<InvoiceApportionCharge>();
			AssertEquals(parent.Lookups.Parent, parent);
		}

		public void TestChargeDistributionBy()
		{
			var invoiceApportionCharge = Factory.New<InvoiceApportionCharge>();
			AssertSame(Factory.GetCachedValue<ChargeDistributeByList>(), invoiceApportionCharge.Lookups.ChargeDistributionBy);
		}
	}
}
