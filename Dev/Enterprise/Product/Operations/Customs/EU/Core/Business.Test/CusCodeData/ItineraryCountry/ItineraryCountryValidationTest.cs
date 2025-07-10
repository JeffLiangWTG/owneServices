using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class ItineraryCountryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Code()
		{
			var itineraryCountry = Factory.New<ItineraryCountry>();
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(itineraryCountry.CY_CodeInfo, "~A", "BE");
		}
	}
}
