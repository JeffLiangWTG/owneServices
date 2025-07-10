using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ItineraryCountryValidationTest : BusinessObjectValidationTestCase
	{
		const string messageErrorOnlyOneCountry = "At least two countries must be entered.";
		const string messageErrorDestination = "The country must match the Destination country.";
		const string messageErrorDispatch = "The country must match the Dispatch country.";

		public void TestCheckCY_Code()
		{
			var declaration = Factory.New<Declaration.JobDeclaration>();
			declaration.JE_GoodsOrigin = "DE";
			declaration.JE_GoodsDestination = "AD";
			var itineraryCountry1 = declaration.ItineraryCountries.AddNew();
			itineraryCountry1.CY_Code = "DE";

			CombineAssertions(() =>
			{
				AssertHasMessageError("Only one country code is entered", itineraryCountry1.CY_CodeInfo, messageErrorOnlyOneCountry);
				var itineraryCountry2 = declaration.ItineraryCountries.AddNew();
				itineraryCountry1.Validation.ValidateCY_Code();
				AssertNoMessageError("More than one country code is entered", itineraryCountry1.CY_CodeInfo, messageErrorOnlyOneCountry);

				itineraryCountry2.CY_Code = "AD";
				AssertNoMessageError("The country code with the highest sequence number is the same from JE_GoodsDestination", itineraryCountry2.CY_CodeInfo, messageErrorDestination);
				itineraryCountry2.CY_Code = "IT";
				AssertHasMessageError("The country code with the highest sequence number is different from JE_GoodsDestination", itineraryCountry2.CY_CodeInfo, messageErrorDestination);

				AssertNoMessageError("The country code with sequence number 1 is the same from JE_GoodsOrigin", itineraryCountry1.CY_CodeInfo, messageErrorDispatch);
				itineraryCountry1.CY_Code = "ES";
				AssertHasMessageError("The country code with sequence number 1 is different from JE_GoodsOrigin", itineraryCountry1.CY_CodeInfo, messageErrorDispatch);
			});
		}
	}
}
