using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing;

[TestedType(typeof(CusVehicle))]
public class CusVehicleTest : EU.Business.Testing.CusVehicleAbstractTest
{
	protected override Type ExpectedLookupsType => typeof(CusVehicleLookups);

	protected override Type ExpectedValidationType => typeof(CusVehicleValidation);

	public void TestModelMaxLength()
	{
		AssertEquals(35, vehicle.CVH_ModelNameInfo.MaxLength);
	}

	public void TestBrandMaxLength()
	{
		AssertEquals(35, vehicle.CVH_BrandNameInfo.MaxLength);
	}

	public void TestCVH_VehicleIdentificationNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("", vehicle.CVH_VehicleIdentificationNumber);
			vehicle.CVH_VehicleIdentificationNumber = "VIN1234";
			AssertEquals("Invoice line vehicle VIN", "VIN1234", vehicle.CVH_VehicleIdentificationNumber);
		});
	}

	public void TestCVH_ModelName()
	{
		CombineAssertions(() =>
		{
			AssertEquals("", vehicle.CVH_ModelName);
			vehicle.CVH_ModelName = "MODEL1234";
			AssertEquals("Invoice line vehicle MODEL", "MODEL1234", vehicle.CVH_ModelName);
		});
	}

	public void TestCVH_BrandName()
	{
		CombineAssertions(() =>
		{
			AssertEquals("", vehicle.CVH_BrandName);
			vehicle.CVH_BrandName = "BRAND1234";
			AssertEquals("Invoice line vehicle BRAND", "BRAND1234", vehicle.CVH_BrandName);
		});
	}

	public void TestInvoiceLine()
	{
		CombineAssertions(() =>
		{
			var vehicle = Factory.New<CusVehicle>();
			AssertNull(vehicle.InvoiceLine);

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			vehicle.CVH_ParentTableCode = invoiceLine.TablePrefix;
			vehicle.CVH_ParentID = invoiceLine.PK;
			AssertSame(invoiceLine, vehicle.InvoiceLine);
		});
	}

	public void TestValidation()
	{
		AssertType<CusVehicleValidation>("Validation Type", vehicle.Validation);
	}

	protected override void SetUp()
	{
		base.SetUp();
		dec = Factory.New<JobDeclaration>();
		var invoiceHeader = dec.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		vehicle = invoiceLine.Vehicles.AddNew();
	}

	JobDeclaration dec;
	JobComInvoiceLine invoiceLine;
	CusVehicle vehicle;
}
