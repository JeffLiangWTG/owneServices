using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUTravelDocAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZA_DocumentNo()
		{
			var travelDoc = Factory.New<TravelDocument>();
			travelDoc.AddInfoValidation.ValidateZA_DocumentNo();
			AssertHasMessageError(travelDoc.ZA_DocumentNoInfo, AUTravelDocAddInfoValidation.DocumentNoRequired);

			travelDoc.ZA_DocumentNo = "12456";
			AssertNoMessageError(travelDoc.ZA_DocumentNoInfo, AUTravelDocAddInfoValidation.DocumentNoRequired);
		}

		public void TestCheckZA_Country()
		{
			var travelDoc = Factory.New<TravelDocument>();
			travelDoc.ZA_Country = "!";
			AssertHasMessageError(travelDoc.ZA_CountryInfo, ListValidation.InvalidCodeMessageError);

			travelDoc.ZA_Country = CMRICAOCountryCodes.Codes.YEMENDEMOCRATIC;
			AssertNoMessageError(travelDoc.ZA_CountryInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
