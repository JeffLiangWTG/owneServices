using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHMasterJobDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckE2_OA_Address()
		{
			CAAddressValidatorTest.AssertAddressCityNameValidationIfNotEntered(Master.Consolidator);
			CAAddressValidatorTest.AssertAddressCityNameValidationIfNotEntered(Master.PlaceOfConsolidation);
		}

		public void TestCheckE2_City()
		{
			Master.Consolidator.E2_AddressOverride = true;
			Master.Consolidator.Validation.ValidateAll();
			AssertHasMessageErrorContaining(Master.Consolidator.E2_CityInfo, "You have not entered a Consolidator Address: City");
			Master.Consolidator.E2_City = "City";
			Master.Consolidator.Validation.ValidateAll();
			AssertNoMessageErrorContaining(Master.Consolidator.E2_CityInfo, "You have not entered a Consolidator Address: City");

			Master.PlaceOfConsolidation.E2_AddressOverride = true;
			Master.PlaceOfConsolidation.Validation.ValidateAll();
			AssertHasMessageErrorContaining(Master.PlaceOfConsolidation.E2_CityInfo, "You have not entered a Place of Consolidation: City");
			Master.PlaceOfConsolidation.E2_City = "City";
			Master.PlaceOfConsolidation.Validation.ValidateAll();
			AssertNoMessageErrorContaining(Master.PlaceOfConsolidation.E2_CityInfo, "You have not entered a Place of Consolidation: City");
		}

		CusCAeMHMaster Master
		{
			get { return master ?? (master = Factory.New<CusCAeMHMaster>()); }
		}
		CusCAeMHMaster master;
	}
}
