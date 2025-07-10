using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class CusEntryLineFeeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryLineFee()
		{
			var parent = Factory.New<CusEntryLineFee>();
			AssertEquals(parent.Lookups.EntryLineFee, parent);
		}

		public void TestChargeTypeList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Brazil, "TT1", "Test Description 1");
			helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Brazil, "TT2", "Test Description 2");
			ReferenceTestDataHelper.CreateReferenceDataForILFFeeTypeList(Factory);
			Factory.Save();

			var fee = Factory.New<CusEntryLineFee>();
			var cusFeeLookups1 = new CusEntryLineFeeLookups(fee);
			AssertContainsExactElementsInAnyOrder(new string[] { "TT1", "TT2", "SUF", "FMM", "F1ND", "F1D5", "ICM", "EIC", "FCP" }, cusFeeLookups1.ChargeTypeList.GetAllCodes());

			AssertEquals("SISCOMEX Usage Entry Fee", cusFeeLookups1.ChargeTypeList.GetDescriptionFromCode(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee));
			AssertEquals("Freight Surcharge for Renewal of the Merchant Marine (AFRMM)", cusFeeLookups1.ChargeTypeList.GetDescriptionFromCode(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax));
			AssertEquals("Tax on the Movement of Goods and Services", cusFeeLookups1.ChargeTypeList.GetDescriptionFromCode(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.ICMSTax));
			AssertEquals("Tax on the Fund to Combat Poverty", cusFeeLookups1.ChargeTypeList.GetDescriptionFromCode(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.ICMSFCPTax));
			var cusFeeLookups2 = new CusEntryLineFeeLookups(fee);
			AssertSame("The collection should be cached", cusFeeLookups1.ChargeTypeList, cusFeeLookups2.ChargeTypeList);
		}
	}
}
