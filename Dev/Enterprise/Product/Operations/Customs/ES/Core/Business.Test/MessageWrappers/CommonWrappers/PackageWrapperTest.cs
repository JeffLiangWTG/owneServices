using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

public class PackageWrapperTest : WrapperHelperTest<PackageWrapper>
{
	public void TestPackagesQty()
	{
		var wrapper = new PackageWrapper(ZString.Empty, ZString.Empty, InternalPackage1.NumberOfPackages, ZInt.Zero);
		AssertEquals("Expected filled PackagesQty", InternalPackage1.NumberOfPackages, wrapper.PackagesQty);
	}

	public void TestPiecesQty()
	{
		var wrapper = new PackageWrapper(ZString.Empty, ZString.Empty, ZInt.Zero, InternalPackage2.NumberOfPieces);
		AssertEquals("Expected filled PiecesQty", InternalPackage2.NumberOfPieces, wrapper.PiecesQty);
	}

	public void TestGetPackagesList()
	{
		var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
		packageInfo1.CW_PackType = InternalPackage1.Type;
		packageInfo1.CW_MarksAndNos = InternalPackage1.Marks;
		pack1.CHC_CW = packageInfo1.PK;
		pack1.CHC_NumberOfPacks = InternalPackage1.NumberOfElements;
		invoiceLine.PackagesPivot.Add(pack1);

		var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo2 = Factory.New<Customs.Business.BasePackage>();
		packageInfo2.CW_PackType = InternalPackage2.Type;
		packageInfo2.CW_MarksAndNos = InternalPackage2.Marks;
		pack2.CHC_CW = packageInfo2.PK;
		pack2.CHC_NumberOfPacks = InternalPackage2.NumberOfElements;
		invoiceLine.PackagesPivot.Add(pack2);

		var packages = PackageWrapper.GetPackagesList(entryLine).ToList();
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages", 2, packages.Count);

			AssertEquals("For first package expected filled PackageType", InternalPackage1.Type, packages[0].PackageType);
			AssertEquals("For first package expected filled Marks", InternalPackage1.Marks, packages[0].Marks);
			AssertEquals("For first package expected filled PackagesQty", InternalPackage1.NumberOfPackages, packages[0].PackagesQty);
			AssertEquals("For first package expected empty PiecesQty", InternalPackage1.NumberOfPieces, packages[0].PiecesQty);

			AssertEquals("For second package expected filled PackageType", InternalPackage2.Type, packages[1].PackageType);
			AssertEquals("For second package expected filled Marks", InternalPackage2.Marks, packages[1].Marks);
			AssertEquals("For second package expected empty PackagesQty", InternalPackage2.NumberOfPackages, packages[1].PackagesQty);
			AssertEquals("For second package expected filled PiecesQty", InternalPackage2.NumberOfPieces, packages[1].PiecesQty);
		});
	}

	public void TestGetPackagesListWhenVehicles()
	{
		var vehicle = invoiceLine.Vehicles.AddNew();
		vehicle.CVH_VehicleIdentificationNumber = "VINCode1";
		var vehicle1 = invoiceLine.Vehicles.AddNew();
		vehicle1.CVH_VehicleIdentificationNumber = "VINCode11";
		var invLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		var vehicle2 = invLine2.Vehicles.AddNew();
		vehicle2.CVH_VehicleIdentificationNumber = "VINCode1";
		var invLine3 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		var vehicle3 = invLine3.Vehicles.AddNew();
		vehicle3.CVH_VehicleIdentificationNumber = "VINCode2";

		var packages = PackageWrapper.GetPackagesList(entryLine).ToList();
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages for vehicles only 1", 1, packages.Count);

			AssertEquals("For only package expected filled PackageType", InternalPackage3.Type, packages[0].PackageType);
			AssertEquals("For only package expected filled Marks", InternalPackage3.Marks, packages[0].Marks);
			AssertEquals("For only package expected filled PackagesQty with 4 (the amount of vehicles declared)", 4, packages[0].PackagesQty);
			AssertEquals("For only package expected empty PiecesQty", 0, packages[0].PiecesQty);
		});
	}

	public void TestGetPackagesListWhenSamePackageType()
	{
		var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
		packageInfo1.CW_PackType = InternalPackage1.Type;
		pack1.CHC_CW = packageInfo1.PK;
		invoiceLine.PackagesPivot.Add(pack1);

		var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo2 = Factory.New<Customs.Business.BasePackage>();
		packageInfo2.CW_PackType = InternalPackage1.Type;
		pack2.CHC_CW = packageInfo2.PK;
		invoiceLine.PackagesPivot.Add(pack2);

		var packages = PackageWrapper.GetPackagesList(entryLine).ToList();
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages for packages with the same type only 1", 1, packages.Count);
			AssertEquals("For only package expected filled PackageType", InternalPackage1.Type, packages[0].PackageType);
		});
	}

	public void TestGetPackagesListMultipleMergedLines()
	{
		var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
		packageInfo1.CW_PackType = "CT";
		packageInfo1.CW_MarksAndNos = "marks";

		var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		pack1.CHC_CW = packageInfo1.PK;
		pack1.CHC_NumberOfPacks = 9;
		invoiceLine.PackagesPivot.Add(pack1);

		var invoiceLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		pack2.CHC_CW = packageInfo1.PK;
		pack2.CHC_NumberOfPacks = 3;
		invoiceLine2.PackagesPivot.Add(pack2);

		var packageInfo2 = Factory.New<Customs.Business.BasePackage>();
		packageInfo2.CW_PackType = "NE";
		packageInfo2.CW_MarksAndNos = "marks2";
		var invoiceLine3 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		var pack3 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		pack3.CHC_CW = packageInfo2.PK;
		pack3.CHC_NumberOfPacks = 4;
		invoiceLine3.PackagesPivot.Add(pack3);

		var pack4 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo3 = Factory.New<Customs.Business.BasePackage>();
		packageInfo3.CW_PackType = "VG";
		packageInfo3.CW_MarksAndNos = "bulk gas marks";
		pack4.CHC_CW = packageInfo3.PK;
		pack4.CHC_NumberOfPacks = 2;
		invoiceLine.PackagesPivot.Add(pack4);

		var packages = PackageWrapper.GetPackagesList(entryLine).ToList();
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages", 3, packages.Count);

			AssertEquals("For first package expected filled PackageType", "CT", packages[0].PackageType);
			AssertEquals("For first package expected filled Marks", "marks", packages[0].Marks);
			AssertEquals("For first package expected filled PackagesQty", 12, packages[0].PackagesQty);
			AssertEquals("For first package expected empty PiecesQty", 0, packages[0].PiecesQty);

			AssertEquals("For second package expected filled PackageType", "VG", packages[1].PackageType);
			AssertEquals("For second package expected filled Marks", "bulk gas marks", packages[1].Marks);
			AssertEquals("For second package expected empty PackagesQty", 2, packages[1].PackagesQty);
			AssertEquals("For second package expected filled PiecesQty", 0, packages[1].PiecesQty);

			AssertEquals("For third package expected filled PackageType", "NE", packages[2].PackageType);
			AssertEquals("For third package expected filled Marks", "marks2", packages[2].Marks);
			AssertEquals("For third package expected filled PackagesQty", 0, packages[2].PackagesQty);
			AssertEquals("For third package expected empty PiecesQty", 4, packages[2].PiecesQty);
		});
	}

	public void TestGetPackagesListMultipleMergedLinesWhenPackagesAndVehicles()
	{
		var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
		packageInfo1.CW_PackType = "CT";
		packageInfo1.CW_MarksAndNos = "marks";
		var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		pack1.CHC_CW = packageInfo1.PK;
		pack1.CHC_NumberOfPacks = 9;
		invoiceLine.PackagesPivot.Add(pack1);

		var vehicle = invoiceLine.Vehicles.AddNew();
		vehicle.CVH_VehicleIdentificationNumber = "VINCode1";
		vehicle.CVH_BrandName = "Brand1";
		vehicle.CVH_ModelName = "Model1";

		var packageInfo2 = Factory.New<Customs.Business.BasePackage>();
		packageInfo2.CW_PackType = "NE";
		packageInfo2.CW_MarksAndNos = "marks2";
		var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		pack2.CHC_CW = packageInfo2.PK;
		pack2.CHC_NumberOfPacks = 4;
		var invoiceLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		var vehicle2 = invoiceLine2.Vehicles.AddNew();
		invoiceLine2.PackagesPivot.Add(pack2);
		vehicle2.CVH_VehicleIdentificationNumber = "VINCode2";
		vehicle2.CVH_BrandName = "Brand2";
		vehicle2.CVH_ModelName = "Model2";

		var pack3 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo3 = Factory.New<Customs.Business.BasePackage>();
		packageInfo3.CW_PackType = "VG";
		packageInfo3.CW_MarksAndNos = "bulk gas marks";
		pack3.CHC_CW = packageInfo3.PK;
		pack3.CHC_NumberOfPacks = 2;
		var invoiceLine3 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine3.PackagesPivot.Add(pack3);

		var invoiceLine4 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		var vehicle4 = invoiceLine4.Vehicles.AddNew();
		vehicle4.CVH_VehicleIdentificationNumber = "VINCode3";
		vehicle4.CVH_BrandName = "Brand3";

		var packages = PackageWrapper.GetPackagesList(entryLine).ToList();
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages", 4, packages.Count);

			AssertEquals("For first package (vehicle) expected filled PackageType", "FR", packages[0].PackageType);
			AssertEquals("For first package (vehicle) expected filled Marks", "BASTIDORES", packages[0].Marks);
			AssertEquals("For first package (vehicle) expected filled PackagesQty", 3, packages[0].PackagesQty);
			AssertEquals("For first package (vehicle) expected empty PiecesQty", 0, packages[0].PiecesQty);

			AssertEquals("For first package (non vehicle) expected filled PackageType", "CT", packages[1].PackageType);
			AssertEquals("For first package (non vehicle) expected filled Marks", "marks", packages[1].Marks);
			AssertEquals("For first package (non vehicle) expected filled PackagesQty", 9, packages[1].PackagesQty);
			AssertEquals("For first package (non vehicle) expected empty PiecesQty", 0, packages[1].PiecesQty);

			AssertEquals("For second package (non vehicle) expected filled PackageType", "NE", packages[2].PackageType);
			AssertEquals("For second package (non vehicle) expected filled Marks", "marks2", packages[2].Marks);
			AssertEquals("For second package (non vehicle) expected empty PackagesQty", 0, packages[2].PackagesQty);
			AssertEquals("For second package (non vehicle) expected filled PiecesQty", 4, packages[2].PiecesQty);

			AssertEquals("For third package (non vehicle) expected filled PackageType", "VG", packages[3].PackageType);
			AssertEquals("For third package (non vehicle) expected filled Marks", "bulk gas marks", packages[3].Marks);
			AssertEquals("For third package (non vehicle) expected empty PackagesQty", 2, packages[3].PackagesQty);
			AssertEquals("For third package (non vehicle) expected empty PiecesQty", 0, packages[3].PiecesQty);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();

		AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

		entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
	}

	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;

	protected override PackageWrapper GetProvider() => new PackageWrapper(InternalPackage1.Type, InternalPackage1.Marks, InternalPackage1.NumberOfPackages, ZInt.Zero);
}
