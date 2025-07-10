using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business.Testing;

class CusVehicleValidationTest : BusinessObjectValidationTestCase
{
	public void TestValidateMaxVehicleCombinedFieldsLength()
	{
		var expectedMessageError = "In provisional period, VIN+Brand+Model length could not be greater than 40.";

		CombineAssertions(() =>
		{
			var vehicle = invoiceLine.Vehicles.AddNew();
			vehicle.CVH_VehicleIdentificationNumber = "123456789-1234567";
			vehicle.CVH_BrandName = "123456789-123";
			vehicle.CVH_ModelName = "1234567890-";
			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				vehicle.Validation.ValidateAll();
				AssertHasRowMessageErrorContaining(vehicle, expectedMessageError);

				vehicle.CVH_ModelName = "1234567890";
				vehicle.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(vehicle, expectedMessageError);
			}

			vehicle.CVH_ModelName = "1234567890-";
			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
			{
				vehicle.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(vehicle, expectedMessageError);
			}
		});
	}

	public void TestCheckCVH_VehicleIdentificationNumber()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = MessageTypeList.Codes.Import;
		var invoiceLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_Tariff = "870210";
		var vehicle = invoiceLine.Vehicles.AddNew();

		vehicle.CVH_VehicleIdentificationNumber = "vin_number";
		vehicle.Validation.ValidateAll();
		AssertNoWarningContaining(vehicle.CVH_VehicleIdentificationNumberInfo, "The VIN may not be required for Tariff:");

		invoiceLine.JI_Tariff = "1110";
		vehicle.CVH_VehicleIdentificationNumber = "vin_number";
		vehicle.Validation.ValidateAll();
		AssertHasWarningContaining(vehicle.CVH_VehicleIdentificationNumberInfo, "The VIN may not be required for Tariff:");

		invoiceLine.JI_Tariff = "870310";
		vehicle.CVH_VehicleIdentificationNumber = "vin_number";
		vehicle.Validation.ValidateAll();
		AssertNoWarningContaining(vehicle.CVH_VehicleIdentificationNumberInfo, "The VIN may not be required for Tariff:");
	}

	public void TestCheckCVH_BrandName()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		var vehicle = invoiceLine.Vehicles.AddNew();

		CombineAssertions(() =>
		{
			vehicle.CVH_VehicleIdentificationNumber = "vin_number";
			vehicle.CVH_BrandName = "";
			vehicle.Validation.ValidateAll();
			AssertHasMessageErrorContaining(vehicle.CVH_BrandNameInfo, "When declaring VIN, Brand must be declared");

			vehicle.CVH_BrandName = "brand";
			vehicle.Validation.ValidateAll();
			AssertNoMessageErrorContaining(vehicle.CVH_BrandNameInfo, "When declaring VIN, Brand must be declared");
		});
	}

	public void TestCheckCVH_ModelName()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		var vehicle = invoiceLine.Vehicles.AddNew();

		CombineAssertions(() =>
		{
			vehicle.CVH_VehicleIdentificationNumber = "vin_number";
			vehicle.CVH_ModelName = "";
			vehicle.Validation.ValidateAll();
			AssertHasMessageErrorContaining(vehicle.CVH_ModelNameInfo, "When declaring VIN, Model must be declared");

			vehicle.CVH_ModelName = "model";
			vehicle.Validation.ValidateAll();
			AssertNoMessageErrorContaining(vehicle.CVH_ModelNameInfo, "When declaring VIN, Model must be declared");
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		JobDeclaration jobDeclaration = Factory.New<JobDeclaration>();
		invoiceLine = jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
	}
	JobComInvoiceLine invoiceLine;
}
