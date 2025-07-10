using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class GroupInvoiceChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			GroupInvoiceCharge parent = Factory.New<GroupInvoiceCharge>();
			AssertEquals(parent.Lookups.Parent, parent);
		}

		public void TestChargeDistributionBy()
		{
			var groupInvoiceCharge = Factory.New<GroupInvoiceCharge>();
			AssertSame(Factory.GetCachedValue<ChargeDistributeByList>(), groupInvoiceCharge.Lookups.ChargeDistributionBy);
		}
	}
}
