using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CK.Manifest.Business.Testing
{
	sealed class AsycudaManifestHeaderValidationTest : TestCaseWithFactory
	{
		public void TestCheckAMA_RN_NKConveyanceNationality()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			header.AMA_ManifestType = "ASY";
			header.AMA_RN_NKConveyanceNationality = "XX";
			AssertNoMessageError(header.AMA_RN_NKConveyanceNationalityInfo, "You have not entered a Conveyance Country/Region.");

			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.CookIslands;
			header.AMA_ManifestType = "ASY";
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_RN_NKConveyanceNationality = ZString.Empty;
			AssertHasMessageError("Validation for Cook Islands manifest", header.AMA_RN_NKConveyanceNationalityInfo, "You have not entered a Conveyance Country/Region.");

			header.AMA_RN_NKConveyanceNationality = Core.Constants.CountryCodes.Singapore;
			AssertNoMessageError(header.AMA_RN_NKConveyanceNationalityInfo, "You have not entered a Conveyance Country/Region.");

			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.CookIslands;
			header.AMA_ManifestType = "ASY";
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertHasMessageError("Validation applies for both Air & Sea", header.AMA_RN_NKConveyanceNationalityInfo, "You have not entered a Conveyance Country/Region.");

			header.AMA_RN_NKConveyanceNationality = Core.Constants.CountryCodes.Taiwan;
			AssertNoMessageError(header.AMA_RN_NKConveyanceNationalityInfo, "You have not entered a Conveyance Country/Region.");
		}
	}
}
