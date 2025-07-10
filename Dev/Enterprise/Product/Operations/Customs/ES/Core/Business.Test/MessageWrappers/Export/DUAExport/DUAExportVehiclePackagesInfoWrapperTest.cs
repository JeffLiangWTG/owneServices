using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing;

class DUAExportVehiclePackagesInfoWrapperTest : Customs.Business.Testing.DataProviderTestCase<DUAExportVehiclePackagesInfoWrapper>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null Entry Line", () => new DUAExportVehiclePackagesInfoWrapper(null));
			AssertExceptionThrown<ArgumentOutOfRangeException>("No InvoiceLines", () => new DUAExportVehiclePackagesInfoWrapper(Factory.New<CusEntryLine>()));
		});
	}

	public void TestPackages()
	{
		CombineAssertions(() =>
		{
			var packages = wrapper.Packages;
			AssertEquals("Expected empty Packages list", 0, packages.Count);
			AssertSame("Cached", packages, wrapper.Packages);
		});
	}

	public void TestPackagesWithVin()
	{
		var vehicle = invoiceLine.Vehicles.AddNew();
		vehicle.CVH_VehicleIdentificationNumber = "VINCODE";
		AssertEquals(1, wrapper.Packages.Count);
	}

	public void TestPackagesWithBrand()
	{
		var vehicle = invoiceLine.Vehicles.AddNew();
		vehicle.CVH_BrandName = "Brand";
		AssertEquals(1, wrapper.Packages.Count);
	}

	public void TestPackagesWithModel()
	{
		var vehicle = invoiceLine.Vehicles.AddNew();
		vehicle.CVH_ModelName = "Model";
		AssertEquals(1, wrapper.Packages.Count);
	}

	public void TestPackagesMultipleMergedLines()
	{
		var vehicle = invoiceLine.Vehicles.AddNew();
		vehicle.CVH_VehicleIdentificationNumber = "VINCODE";
		var vehicleX = invoiceLine.Vehicles.AddNew();
		vehicleX.CVH_VehicleIdentificationNumber = "VINCODEX";
		var invLine2 = (JobComInvoiceLine)cusEntryLine.InvoiceLines.AddNew();
		var vehicle2 = invLine2.Vehicles.AddNew();
		vehicle2.CVH_VehicleIdentificationNumber = "VINCODE";
		var invLine3 = (JobComInvoiceLine)cusEntryLine.InvoiceLines.AddNew();
		var vehicle3 = invLine3.Vehicles.AddNew();
		vehicle3.CVH_VehicleIdentificationNumber = "VINCODE2";

		CombineAssertions(() =>
		{
			var packages = wrapper.Packages;
			AssertEquals("Expected filled Packages list", 4, packages.Count);
			AssertContainsExactElementsInAnyOrder("Expected Packages with correct Chassis", new ZString[] { "VINCODE", "VINCODEX", "VINCODE", "VINCODE2" }, packages.Select(x => x.Chassis).ToArray());
			AssertSame("Cached", packages, wrapper.Packages);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		cusEntryLine = Factory.New<CusEntryLine>();
		invoiceLine = (JobComInvoiceLine)cusEntryLine.InvoiceLines.AddNew();
		wrapper = new DUAExportVehiclePackagesInfoWrapper(cusEntryLine);
	}
	CusEntryLine cusEntryLine;
	JobComInvoiceLine invoiceLine;
	DUAExportVehiclePackagesInfoWrapper wrapper;

	protected override DUAExportVehiclePackagesInfoWrapper GetProvider() => wrapper;
}
