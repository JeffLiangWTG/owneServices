using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

public class EXSPackageWrapperTest : WrapperHelperTest<EXSPackageWrapper>
{
	public void TestPackagesQty()
	{
		var wrapper = new EXSPackageWrapper(ZString.Empty, ZString.Empty, InternalPackage1.NumberOfPackages, false);
		AssertEquals("Expected filled PackagesQty", InternalPackage1.NumberOfPackages, wrapper.PackagesQty);
	}

	public void TestGetPackagesList()
	{
		var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
		packageInfo1.CW_PackType = "CT";
		packageInfo1.CW_MarksAndNos = "marks";
		pack1.CHC_CW = packageInfo1.PK;
		pack1.CHC_NumberOfPacks = 9;
		invoiceLine.PackagesPivot.Add(pack1);

		var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo2 = Factory.New<Customs.Business.BasePackage>();
		packageInfo2.CW_PackType = "NE";
		packageInfo2.CW_MarksAndNos = "marks2";
		pack2.CHC_CW = packageInfo2.PK;
		pack2.CHC_NumberOfPacks = 4;
		invoiceLine.PackagesPivot.Add(pack2);

		var pack3 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo3 = Factory.New<Customs.Business.BasePackage>();
		packageInfo3.CW_PackType = "VG";
		packageInfo3.CW_MarksAndNos = "bulk gas marks";
		pack3.CHC_CW = packageInfo3.PK;
		pack3.CHC_NumberOfPacks = 2;
		invoiceLine.PackagesPivot.Add(pack3);

		var packages = EXSPackageWrapper.GetPackagesList(entryLine).ToList();
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages", 3, packages.Count);

			AssertEquals("For first package expected filled PackageType", "CT", packages[0].PackageType);
			AssertEquals("For first package expected filled Marks", "marks", packages[0].Marks);
			AssertEquals("For first package expected filled NumberOfPackages", 9, packages[0].PackagesQty);

			AssertEquals("For second package expected filled PackageType", "NE", packages[1].PackageType);
			AssertEquals("For second package expected filled Marks", "marks2", packages[1].Marks);
			AssertEquals("For second package expected empty NumberOfPackages", 4, packages[1].PackagesQty);

			AssertEquals("For third package expected filled PackageType", "VG", packages[2].PackageType);
			AssertEquals("For third package expected filled Marks", "bulk gas marks", packages[2].Marks);
			AssertEquals("For third package expected empty NumberOfPackages", 0, packages[2].PackagesQty);
		});
	}

	public void TestGetPackagesListWhenVehicles()
	{
		var vehicle = invoiceLine.Vehicles.AddNew();
		vehicle.CVH_VehicleIdentificationNumber = "VINCODE";
		var vehicle1 = invoiceLine.Vehicles.AddNew();
		vehicle1.CVH_VehicleIdentificationNumber = "VINCODE1";
		var invLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		var vehicle2 = invLine2.Vehicles.AddNew();
		vehicle2.CVH_VehicleIdentificationNumber = "VINCODE";
		var invLine3 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		var vehicle3 = invLine3.Vehicles.AddNew();
		vehicle3.CVH_VehicleIdentificationNumber = "VINCODE2";

		var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
		packageInfo1.CW_PackType = "CT";
		packageInfo1.CW_MarksAndNos = "marks";
		pack1.CHC_CW = packageInfo1.PK;
		pack1.CHC_NumberOfPacks = 9;
		invoiceLine.PackagesPivot.Add(pack1);

		var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo2 = Factory.New<Customs.Business.BasePackage>();
		packageInfo2.CW_PackType = "NE";
		packageInfo2.CW_MarksAndNos = "marks2";
		pack2.CHC_CW = packageInfo2.PK;
		pack2.CHC_NumberOfPacks = 4;
		invoiceLine.PackagesPivot.Add(pack2);

		var packages = EXSPackageWrapper.GetPackagesList(entryLine).ToList();
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages with vehicles and packages", 3, packages.Count);

			AssertEquals("For first package expected filled PackageType", InternalPackage3.Type, packages[0].PackageType);
			AssertEquals("For first package expected filled Marks", InternalPackage3.Marks, packages[0].Marks);
			AssertEquals("For first package expected filled PackagesQty with 4", 4, packages[0].PackagesQty);

			AssertEquals("For second package expected filled PackageType", "CT", packages[1].PackageType);
			AssertEquals("For second package expected filled Marks", "marks", packages[1].Marks);
			AssertEquals("For second package expected empty NumberOfPackages", 9, packages[1].PackagesQty);

			AssertEquals("For third package expected filled PackageType", "NE", packages[2].PackageType);
			AssertEquals("For third package expected filled Marks", "marks2", packages[2].Marks);
			AssertEquals("For third package expected empty NumberOfPackages", 4, packages[2].PackagesQty);
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

		var packages = EXSPackageWrapper.GetPackagesList(entryLine).ToList();
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages for packages with the same type only 1", 1, packages.Count);
			AssertEquals("For only package expected filled PackageType", InternalPackage1.Type, packages[0].PackageType);
		});
	}

	public void TestGetPackagesListMultipleMergedLines()
	{
		var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
		packageInfo1.CW_PackType = "CT";
		packageInfo1.CW_MarksAndNos = "marks";
		pack1.CHC_CW = packageInfo1.PK;
		pack1.CHC_NumberOfPacks = 9;
		invoiceLine.PackagesPivot.Add(pack1);

		var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo2 = Factory.New<Customs.Business.BasePackage>();
		packageInfo2.CW_PackType = "NE";
		packageInfo2.CW_MarksAndNos = "marks2";
		pack2.CHC_CW = packageInfo2.PK;
		pack2.CHC_NumberOfPacks = 4;
		invoiceLine.PackagesPivot.Add(pack2);

		var pack3 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo3 = Factory.New<Customs.Business.BasePackage>();
		packageInfo3.CW_PackType = "VG";
		packageInfo3.CW_MarksAndNos = "bulk gas marks";
		pack3.CHC_CW = packageInfo3.PK;
		pack3.CHC_NumberOfPacks = 2;
		invoiceLine.PackagesPivot.Add(pack3);

		var packages = EXSPackageWrapper.GetPackagesList(entryLine).ToList();
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages", 3, packages.Count);

			AssertEquals("For first package expected filled PackageType", "CT", packages[0].PackageType);
			AssertEquals("For first package expected filled Marks", "marks", packages[0].Marks);
			AssertEquals("For first package expected filled NumberOfPackages", 9, packages[0].PackagesQty);

			AssertEquals("For second package expected filled PackageType", "NE", packages[1].PackageType);
			AssertEquals("For second package expected filled Marks", "marks2", packages[1].Marks);
			AssertEquals("For second package expected empty NumberOfPackages", 4, packages[1].PackagesQty);

			AssertEquals("For third package expected filled PackageType", "VG", packages[2].PackageType);
			AssertEquals("For third package expected filled Marks", "bulk gas marks", packages[2].Marks);
			AssertEquals("For third package expected empty NumberOfPackages", 0, packages[2].PackagesQty);
		});
	}

	public void TestIsPackTypeBulk()
	{
		var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
		packageInfo1.CW_PackType = "CT";
		pack1.CHC_CW = packageInfo1.PK;
		invoiceLine.PackagesPivot.Add(pack1);

		var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo2 = Factory.New<Customs.Business.BasePackage>();
		packageInfo2.CW_PackType = "VG";
		pack2.CHC_CW = packageInfo2.PK;
		invoiceLine.PackagesPivot.Add(pack2);

		var packages = EXSPackageWrapper.GetPackagesList(entryLine).ToList();
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages", 2, packages.Count);

			AssertEquals("For first package expected false IsPackTypeBulk", false, packages[0].IsPackTypeBulk);
			AssertEquals("For second package expected true IsPackTypeBulk", true, packages[1].IsPackTypeBulk);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		Factory.SetBulkTypeHelper();

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

	protected override EXSPackageWrapper GetProvider() => new EXSPackageWrapper(InternalPackage1.Type, InternalPackage1.Marks, InternalPackage1.NumberOfPackages, false);
}
