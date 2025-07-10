using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.GDM.Testing
{
	sealed class GuidedDecisionMakingBasicLookupsTest : TestCaseWithFactory
	{
		public void TestRegionOrTerritoryOfDestinationList()
		{
			var gDMBasic = GuidedDecisionMakingBasicTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory);
			var lookups = gDMBasic.Lookups;
			AssertType<FRDomesticOverseasTerritories>(lookups.RegionOrTerritoryOfDestinationList);
		}

		public void TestPreferenceList()
		{
			var gDMBasic = GuidedDecisionMakingBasicTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory);
			var lookups = gDMBasic.Lookups;

			AssertEquals(0, lookups.PreferenceList.Count);

			gDMBasic.CountryOfOrigin = Core.Constants.CountryCodes.FrenchGuyana;
			gDMBasic.RegionOrTerritoryOfDestination = FRDomesticOverseasTerritories.Codes.CONTI;
			AssertEquals(1, lookups.PreferenceList.Count);
			AssertEquals(Core.Constants.Customs.Universal.RefCusPreference.Codes._100, lookups.PreferenceList.CodesAsString);

			gDMBasic.CountryOfOrigin = Core.Constants.CountryCodes.France;
			AssertEquals(0, lookups.PreferenceList.Count);

			gDMBasic.CountryOfOrigin = Core.Constants.CountryCodes.FrenchGuyana;
			gDMBasic.RegionOrTerritoryOfDestination = FRDomesticOverseasTerritories.Codes.REUNI;
			AssertEquals(0, lookups.PreferenceList.Count);
		}
	}
}
