using System;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusVehicle))]
class CusVehicleTest : Customs.Business.Testing.CusVehicleAbstractTest
{
	public void TestCVH_RegistrationNumber()
	{
		var vehicle = Factory.New<CusVehicle>();
		AssertEquals("CVH_RegistrationNumber.MaxLength", 9, vehicle.CVH_RegistrationNumberInfo.MaxLength);
	}

	public void TestCVH_ModelName()
	{
		var vehicle = Factory.New<CusVehicle>();
		AssertEquals("CVH_ModelName.MaxLength", 3, vehicle.CVH_ModelNameInfo.MaxLength);
	}

	public void TestCVH_VehicleIdentificationNumber()
	{
		var vehicle = Factory.New<CusVehicle>();
		AssertEquals("CVH_VehicleIdentificationNumber.MaxLength", 17, vehicle.CVH_VehicleIdentificationNumberInfo.MaxLength);
	}

	public void TestInvoiceLine()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var vehicle = invoiceLine.Vehicles.AddNew();

		AssertEquals("InvoiceLine.PK = Vehicle.InvoiceLine.PK", invoiceLine.PK, vehicle.InvoiceLine.PK);
	}

	protected override Type ExpectedLookupsType => typeof(CusVehicleLookups);
	protected override Type ExpectedValidationType => typeof(CusVehicleValidation);
}
