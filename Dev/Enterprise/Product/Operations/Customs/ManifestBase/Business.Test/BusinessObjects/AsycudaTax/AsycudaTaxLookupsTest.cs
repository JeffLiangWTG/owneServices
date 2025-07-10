using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ManifestBase.Testing
{
	class AsycudaTaxLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeLists()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var bill = header.Bills.AddNew();
			var tax = Factory.New<AsycudaTax>();
			tax.AET_ABL = bill.PK;
			var rateOverrideReasonCodeList = tax.Lookups.RateOverrideReasonCodeList;
			AssertNotNull(rateOverrideReasonCodeList);
			Assert(rateOverrideReasonCodeList.Count == 2);
			Assert(rateOverrideReasonCodeList.ContainsCode("ADD"));
			Assert(rateOverrideReasonCodeList.ContainsCode("OVR"));

			var methodOfPaymentList = tax.Lookups.MethodOfPaymentList;
			AssertNotNull(methodOfPaymentList);

			var chargeTypeList = tax.Lookups.ChargeTypeList;
			AssertNotNull(chargeTypeList);
		}
	}
}
