using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class CusVehicleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCVH_VehicleIdentificationNumber()
		{
			const string warningMsg = "The VIN captured is invalid.";
			CombineAssertions(() =>
			{
				var vehicle = Factory.New<CusVehicle>();
				vehicle.CVH_VehicleIdentificationNumber = "12";
				AssertHasWarning("vin has no 17 digits", vehicle.CVH_VehicleIdentificationNumberInfo, warningMsg);
				vehicle.CVH_VehicleIdentificationNumber = "123456789ABCDEFGI";
				AssertHasWarning("vin has 17 digits but contain invalid char 'I'", vehicle.CVH_VehicleIdentificationNumberInfo, warningMsg);
				vehicle.CVH_VehicleIdentificationNumber = "123456789ABCDEFGO";
				AssertHasWarning("vin has 17 digits but contain invalid char 'O'", vehicle.CVH_VehicleIdentificationNumberInfo, warningMsg);
				vehicle.CVH_VehicleIdentificationNumber = "123456789ABCDEFGQ";
				AssertHasWarning("vin has 17 digits but contain invalid char 'Q'", vehicle.CVH_VehicleIdentificationNumberInfo, warningMsg);
				vehicle.CVH_VehicleIdentificationNumber = "123456789ABCDEFG$";
				AssertHasWarning("vin has 17 digits but contain invalid char '$'", vehicle.CVH_VehicleIdentificationNumberInfo, warningMsg);
				vehicle.CVH_VehicleIdentificationNumber = "0123456789ABCDEFG";
				AssertHasWarning("vin has 17 digits and no invalid char but failed through the ninth digit check'", vehicle.CVH_VehicleIdentificationNumberInfo, warningMsg);
				vehicle.CVH_VehicleIdentificationNumber = "UU6JA69691D713820";
				AssertNoWarnings("valid vin", vehicle.CVH_VehicleIdentificationNumberInfo);
				vehicle.CVH_VehicleIdentificationNumber = "";
				AssertNoWarnings("vin is empty", vehicle.CVH_VehicleIdentificationNumberInfo);
			});
		}
	}
}
