using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.GDM.Testing
{
	sealed class GuidedDecisionMakingBasicValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckRegionorTerritoryOfDestination()
		{
			var gDMBasic = GuidedDecisionMakingBasicTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory);
			gDMBasic.RegionOrTerritoryOfDestination = "";
			gDMBasic.Validation.ValidateRegionOrTerritoryOfDestination();
			AssertHasError("Error if Region is empty", gDMBasic.RegionOrTerritoryOfDestinationInfo, "Please enter a Region.");

			gDMBasic.RegionOrTerritoryOfDestination = "XXX";
			gDMBasic.Validation.ValidateRegionOrTerritoryOfDestination();
			AssertHasError("Error if Region is not in list", gDMBasic.RegionOrTerritoryOfDestinationInfo, "Enter a valid Region.");

			gDMBasic.RegionOrTerritoryOfDestination = "CONTI";
			gDMBasic.Validation.ValidateRegionOrTerritoryOfDestination();
			AssertNoErrors("No error shown if correct region is entered", gDMBasic.RegionOrTerritoryOfDestinationInfo);
		}
	}
}
