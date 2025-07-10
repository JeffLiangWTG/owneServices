using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Manifest.Business.Testing
{
	public class AsycudaTaxLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTaxCodeList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var asycudaTax = header.Bills.AddNew().AsycudaTaxes.AddNew();
			var taxCodeList = asycudaTax.Lookups.ChargeTypeList;
			AssertEquals(17, taxCodeList.Count);
		}

		public void TestMethodOfPaymentList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var asycudaTax = header.Bills.AddNew().AsycudaTaxes.AddNew();
			var methodofpaymentList = asycudaTax.Lookups.MethodOfPaymentList;
			AssertEquals(2, methodofpaymentList.Count);
		}
	}
}
