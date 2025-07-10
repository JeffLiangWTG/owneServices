using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	class CusEntryHeaderChargesLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRateOverrideReasonCodeList()
		{
			var rateOverrideReasonCodeList = Factory.GetCachedValue<ILRateOverrideReasonList>();
			CombineAssertions(() =>
			{
				AssertEquals("Should contain 3 items", 3, rateOverrideReasonCodeList.Count);
				AssertEquals("Values of the codes should be", "ADD, OVR, PRE", rateOverrideReasonCodeList.CodesAsString);
				var charge = Factory.New<CusEntryHeaderCharges>();
				AssertType<ILRateOverrideReasonList>(charge.Lookups.RateOverrideReasonCodeList);
				AssertSame("List should have been cached.", rateOverrideReasonCodeList, charge.Lookups.RateOverrideReasonCodeList);
			});
		}
	}
}
