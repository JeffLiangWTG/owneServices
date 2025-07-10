using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CommonInvoiceHeaderChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestChargeTypeList()
		{
			var parent = Factory.New<GroupInvoiceCharge>();
			AssertEquals("ChargeTypeList type", typeof(CAChargeTypeList), parent.Lookups.ChargeTypeList.GetType());
		}
	}
}
