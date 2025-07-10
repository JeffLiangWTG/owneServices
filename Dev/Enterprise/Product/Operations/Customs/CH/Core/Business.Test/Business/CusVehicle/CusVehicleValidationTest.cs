using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

class CusVehicleValidationTest : BusinessObjectValidationTestCase
{
	public void TestCVH_VehicleIdentificationNumberCheckDigit()
	{
		vehicle.Validation.ValidateCVH_VehicleIdentificationNumber();
		AssertHasMessageErrorContaining("VIN is empty", vehicle.CVH_VehicleIdentificationNumberInfo, MandatoryValidation.YouHaveNotEntered);
		vehicle.CVH_VehicleIdentificationNumber = "0123456";
		AssertHasMessageErrorContaining("VIN must be 17 digits", vehicle.CVH_VehicleIdentificationNumberInfo, "must be 17 characters");
		vehicle.CVH_VehicleIdentificationNumber = "UU6JA69691D713820";
		AssertNoMessageErrorContaining("Valid VIN", vehicle.CVH_VehicleIdentificationNumberInfo, "must be 17 characters");
	}

	public void TestCVH_RegistrationNumber()
	{
		vehicle.CVH_RegistrationNumber = "0123456";
		AssertHasMessageErrorContaining("Registration Number must be 9 digits", vehicle.CVH_RegistrationNumberInfo, "must be 9 digits");
		vehicle.CVH_RegistrationNumber = "672141219";
		AssertHasMessageErrorContaining("Registration Number is not valid", vehicle.CVH_RegistrationNumberInfo, "Registration number is invalid.");
		vehicle.CVH_RegistrationNumber = "672141217";
		AssertNoMessageErrorContaining("Valid Registration Number", vehicle.CVH_RegistrationNumberInfo, "Registration number is invalid.");
	}

	public void TestCVH_ModelName()
	{
		RefCusCodeTestHelper.CreateVehicleModelNameCodeList(Factory);

		CombineAssertions("CVH_ModelName", () =>
		{
			vehicle.Validation.ValidateCVH_ModelName();
			AssertHasMessageErrorContaining(vehicle.CVH_ModelNameInfo, MandatoryValidation.YouHaveNotEntered);

			vehicle.CVH_ModelName = RefCusCodeTestHelper.InvalidVehicleModelNameCode;
			AssertHasMessageErrorContaining(vehicle.CVH_ModelNameInfo, ListValidation.InvalidCodeMessageError);

			vehicle.CVH_ModelName = RefCusCodeTestHelper.ValidVehicleModelNameCode;
			AssertNoMessageErrorContaining(vehicle.CVH_ModelNameInfo, ListValidation.InvalidCodeMessageError);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		JobDeclaration jobDeclaration = Factory.New<JobDeclaration>();
		var invoiceLine = jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
		vehicle = invoiceLine.Vehicles.AddNew();
	}
	CusVehicle vehicle;
}
