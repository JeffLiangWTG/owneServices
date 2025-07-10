using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.MX.Business.Testing
{
	class CusVehicleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCVH_SerialNumber()
		{
			var vehicle = Factory.New<CusVehicle>();
			vehicle.CVH_SerialNumber = ZString.Empty;
			AssertHasMessageErrorContaining(vehicle.CVH_SerialNumberInfo, MandatoryValidation.YouHaveNotEntered);

			vehicle.CVH_SerialNumber = "M123";
			AssertNoMessageErrorContaining("No Message Error when is not empty", vehicle.CVH_SerialNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCVH_Mileage()
		{
			var vehicle = Factory.New<CusVehicle>();
			vehicle.CVH_Mileage = 0;
			AssertHasMessageErrorContaining(vehicle.CVH_MileageInfo, MandatoryValidation.YouHaveNotEntered);

			vehicle.CVH_Mileage = 50;
			AssertNoMessageErrorContaining(vehicle.CVH_MileageInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
