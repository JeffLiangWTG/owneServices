using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	class CcsukCusAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckC4_CommunityHandlingCode()
		{
			var basic = Factory.New<CusMAWB>();
			var chc = basic.CommunityHandlingCodes.AddNew().Data;
			chc.C4_CommunityHandlingCode = "XXX";
			AssertHasMessageErrorContaining(chc.C4_CommunityHandlingCodeInfo, "not in the list");
			chc.C4_CommunityHandlingCode = chc.Lookups.SpecialHandlingCodes[0].Code;
			AssertNoMessageErrorContaining(chc.C4_CommunityHandlingCodeInfo, "not in the list");
		}

		public void TestCheckC4_SplitReferenceToWhichThisPertains()
		{
			var basic = Factory.New<CusMAWB>();
			var chc = basic.CommunityHandlingCodes.AddNew().Data;
			chc.C4_CommunityHandlingCode = chc.Lookups.SpecialHandlingCodes[0].Code;
			AssertNoWarningContaining(chc.C4_SplitReferenceToWhichThisPertainsInfo, "individual split");
			var split = basic.Splits.AddNew();
			split.SplitReference = "01";
			chc = basic.CommunityHandlingCodes.AddNew().Data;
			AssertNoWarningContaining(chc.C4_SplitReferenceToWhichThisPertainsInfo, "individual split");
			chc.C4_CommunityHandlingCode = chc.Lookups.SpecialHandlingCodes[0].Code;
			AssertHasWarningContaining(chc.C4_SplitReferenceToWhichThisPertainsInfo, "individual split");
			chc.C4_SplitReferenceToWhichThisPertains = "02";
			AssertHasMessageErrorContaining(chc.C4_SplitReferenceToWhichThisPertainsInfo, "not in the list");
			AssertNoWarningContaining(chc.C4_SplitReferenceToWhichThisPertainsInfo, "individual split");
			chc.C4_SplitReferenceToWhichThisPertains = "01";
			AssertNoMessageErrorContaining(chc.C4_SplitReferenceToWhichThisPertainsInfo, "not in the list");
		}
	}
}
