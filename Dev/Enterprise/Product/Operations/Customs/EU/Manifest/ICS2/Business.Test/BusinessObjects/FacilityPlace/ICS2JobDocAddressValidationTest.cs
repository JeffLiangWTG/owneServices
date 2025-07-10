using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class ICS2JobDocAddressValidationTest : TestCaseWithFactory
	{
		public void TestFacilityFieldsRequirement()
		{
			var facilityPlace = Factory.New<ICS2JobDocAddress>();

			facilityPlace.E2_City = "Paris";
			Assert(facilityPlace.E2_RN_NKCountryCodeInfo.HasMessageError("You have not entered an Unspecified: Facility Place Country."));
			Assert(facilityPlace.E2_PostcodeInfo.HasMessageError("You have not entered an Unspecified: Facility Place Postcode."));

			facilityPlace.E2_City = "";
			Assert(!facilityPlace.E2_RN_NKCountryCodeInfo.HasMessageErrors());
			Assert(!facilityPlace.E2_PostcodeInfo.HasMessageErrors());

			facilityPlace.E2_Postcode = "021322";
			Assert(facilityPlace.E2_CityInfo.HasMessageError("You have not entered an Unspecified: Facility Place City."));
			Assert(facilityPlace.E2_RN_NKCountryCodeInfo.HasMessageError("You have not entered an Unspecified: Facility Place Country."));

			facilityPlace.E2_Postcode = "";
			Assert(!facilityPlace.E2_CityInfo.HasMessageErrors());
			Assert(!facilityPlace.E2_RN_NKCountryCodeInfo.HasMessageErrors());

			facilityPlace.E2_RN_NKCountryCode = "FR";
			Assert(facilityPlace.E2_CityInfo.HasMessageError("You have not entered an Unspecified: Facility Place City."));
			Assert(facilityPlace.E2_PostcodeInfo.HasMessageError("You have not entered an Unspecified: Facility Place Postcode."));

			facilityPlace.E2_RN_NKCountryCode = "";
			Assert(!facilityPlace.E2_CityInfo.HasMessageErrors());
			Assert(!facilityPlace.E2_PostcodeInfo.HasMessageErrors());
		}
	}
}
