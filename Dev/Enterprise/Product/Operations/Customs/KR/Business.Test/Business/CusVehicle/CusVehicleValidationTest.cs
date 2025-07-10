using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	class CusVehicleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCVH_RN_NKCountryOfManufacture()
		{
			var vehicle = Factory.New<CusVehicle>();
			vehicle.CVH_RN_NKCountryOfManufacture = "";
			AssertNoMessageErrors(vehicle.CVH_RN_NKCountryOfManufactureInfo);

			vehicle.CVH_RN_NKCountryOfManufacture = "XX";
			AssertHasMessageErrorContaining(vehicle.CVH_RN_NKCountryOfManufactureInfo, ListValidation.InvalidCodeMessageError);

			vehicle.CVH_RN_NKCountryOfManufacture = Core.Constants.CountryCodes.KoreaSouth;
			AssertNoMessageErrors(vehicle.CVH_RN_NKCountryOfManufactureInfo);
		}
	}
}
