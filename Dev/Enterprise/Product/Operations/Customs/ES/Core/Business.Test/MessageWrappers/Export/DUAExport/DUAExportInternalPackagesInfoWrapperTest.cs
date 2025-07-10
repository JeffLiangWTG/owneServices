using System;
using System.Linq;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing;

class DUAExportInternalPackagesInfoWrapperTest : WrapperHelperTest<DUAExportInternalPackagesInfoWrapper>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null Entry Line", () => new DUAExportInternalPackagesInfoWrapper(null));
			AssertExceptionThrown<ArgumentOutOfRangeException>("No InvoiceLines", () => new DUAExportInternalPackagesInfoWrapper(Factory.New<CusEntryLine>()));
		});
	}

	public void TestPackages()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty Packages list", false, wrapper.Packages.Any());

			var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
			var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
			packageInfo1.CW_PackType = InternalPackage1.Type;
			pack1.CHC_CW = packageInfo1.PK;
			invoiceLine.PackagesPivot.Add(pack1);

			var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
			var packageInfo2 = Factory.New<Customs.Business.BasePackage>();
			packageInfo2.CW_PackType = InternalPackage2.Type;
			pack2.CHC_CW = packageInfo2.PK;
			invoiceLine.PackagesPivot.Add(pack2);

			wrapper = new DUAExportInternalPackagesInfoWrapper(cusEntryLine);
			var packages = wrapper.Packages;

			AssertEquals("Expected filled Packages list", 2, packages.Count);
			AssertSame("Cached Packages", wrapper.Packages, packages);

			var vehicle = invoiceLine.Vehicles.AddNew();
			vehicle.CVH_VehicleIdentificationNumber = InternalPackage3.Vin;
			wrapper = new DUAExportInternalPackagesInfoWrapper(cusEntryLine);
			packages = wrapper.Packages;

			AssertEquals("Expected filled Packages list with vehicles and packages", 3, packages.Count);
			AssertSame("Cached Packages", wrapper.Packages, packages);
		});
	}

	public void TestPackagesWithoutBasePackage()
	{
		var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		invoiceLine.PackagesPivot.Add(pack1);

		var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		invoiceLine.PackagesPivot.Add(pack2);

		wrapper = new DUAExportInternalPackagesInfoWrapper(cusEntryLine);
		AssertExceptionThrown<NullReferenceException>("No BasePackage", () => wrapper.Packages.ToString());
	}

	public void TestPackages_PackageMarksAndNumbers()
	{
		var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
		pack1.CHC_CW = packageInfo1.PK;
		invoiceLine.PackagesPivot.Add(pack1);
		packageInfo1.CW_MarksAndNos = InternalPackage1.Marks;
		packageInfo1.CW_PackType = InternalPackage1.Type;
		pack1.CHC_NumberOfPacks = InternalPackage1.NumberOfElements;
		var packageMarks = wrapper.Packages.Single().Tag;
		AssertEquals("Expected filled Tag package1", InternalPackage1.Marks, packageMarks);
	}

	public void TestPackages_LineHasVIN()
	{
		var vehicle = invoiceLine.Vehicles.AddNew();
		vehicle.CVH_VehicleIdentificationNumber = InternalPackage3.Vin;
		var packageMarks = wrapper.Packages.Single().Tag;
		AssertEquals("Expected filled Tag vehicle package", InternalPackage3.Marks, packageMarks);
	}

	public void TestPackagesMultipleMergedLines()
	{
		var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
		packageInfo1.CW_PackType = InternalPackage1.Type;
		pack1.CHC_CW = packageInfo1.PK;
		invoiceLine.PackagesPivot.Add(pack1);
		var invoiceLine2 = (JobComInvoiceLine)cusEntryLine.InvoiceLines.AddNew();
		var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		pack2.CHC_CW = packageInfo1.PK;
		invoiceLine2.PackagesPivot.Add(pack2);

		var pack3 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo2 = Factory.New<Customs.Business.BasePackage>();
		packageInfo2.CW_PackType = InternalPackage2.Type;
		pack3.CHC_CW = packageInfo2.PK;
		var invoiceLine3 = (JobComInvoiceLine)cusEntryLine.InvoiceLines.AddNew();
		invoiceLine3.PackagesPivot.Add(pack3);

		var packages = wrapper.Packages;

		CombineAssertions(() =>
		{
			AssertEquals("Expected filled Packages list", 2, packages.Count);
			AssertSame("Cached Packages", wrapper.Packages, packages);
		});
	}

	public void TestPackagesMultipleMergedLinesWhenVehicles()
	{
		var vehicle = invoiceLine.Vehicles.AddNew();
		vehicle.CVH_VehicleIdentificationNumber = "VINCODE";
		var vehicle1 = invoiceLine.Vehicles.AddNew();
		vehicle1.CVH_VehicleIdentificationNumber = "VINCODE1";
		var invoiceLine2 = (JobComInvoiceLine)cusEntryLine.InvoiceLines.AddNew();
		var vehicle2 = invoiceLine2.Vehicles.AddNew();
		vehicle2.CVH_VehicleIdentificationNumber = "VINCODE";
		var invoiceLine3 = (JobComInvoiceLine)cusEntryLine.InvoiceLines.AddNew();
		var vehicle3 = invoiceLine3.Vehicles.AddNew();
		vehicle3.CVH_VehicleIdentificationNumber = "VINCODE2";

		var packages = wrapper.Packages;
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled Packages list for vehicles 1", 1, packages.Count);
			AssertEquals("Expected filled vehicles amount", 4, packages.Single().NumberOfElements);
			AssertSame("Cached Packages", wrapper.Packages, packages);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		cusEntryLine = Factory.New<CusEntryLine>();
		invoiceLine = (JobComInvoiceLine)cusEntryLine.InvoiceLines.AddNew();
		wrapper = new DUAExportInternalPackagesInfoWrapper(cusEntryLine);
	}

	CusEntryLine cusEntryLine;
	JobComInvoiceLine invoiceLine;
	DUAExportInternalPackagesInfoWrapper wrapper;

	protected override DUAExportInternalPackagesInfoWrapper GetProvider() => wrapper;
}
