using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.Business.Testing;

public class DVDCommonPackageWrapperTest : WrapperHelperTest<DVDCommonPackageWrapper>
{
	public void TestNumberOfPackages()
	{
		var wrapper = new DVDCommonPackageWrapper(ZString.Empty, ZString.Empty, 9);
		AssertEquals("Expected filled NumberOfPackages", 9, wrapper.NumberOfPackages);
	}

	public void TestGetPackagesList_CanSendFRPackagesFalse_CanSendNEPackageQtyTrue()
	{
		AddPackages();

		var packages = DVDCommonPackageWrapper.GetPackagesList(entryLine).ToList();
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages", 3, packages.Count);

			AssertEquals("For first package expected filled PackageType", "CT", packages[0].PackageType);
			AssertEquals("For first package expected filled Marks", "marks", packages[0].Marks);
			AssertEquals("For first package expected filled NumberOfPackages", 9, packages[0].NumberOfPackages);

			AssertEquals("For second package expected filled PackageType", "NE", packages[1].PackageType);
			AssertEquals("For second package expected filled Marks", "marks2", packages[1].Marks);
			AssertEquals("For second package expected filled NumberOfPackages", 4, packages[1].NumberOfPackages);

			AssertEquals("For third package expected filled PackageType", "VG", packages[2].PackageType);
			AssertEquals("For third package expected filled Marks", "bulk gas marks", packages[2].Marks);
			AssertEquals("For third package expected empty NumberOfPackages", 0, packages[2].NumberOfPackages);
		});
	}

	public void TestGetPackagesList_CanSendFRPackagesFalse_CanSendNEPackageQtyFalse()
	{
		AddPackages();

		var packages = DVDCommonPackageWrapper.GetPackagesList(entryLine, canSendNEPackageQty: false).ToList();
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages", 3, packages.Count);

			AssertEquals("For first package expected filled PackageType", "CT", packages[0].PackageType);
			AssertEquals("For first package expected filled Marks", "marks", packages[0].Marks);
			AssertEquals("For first package expected filled NumberOfPackages", 9, packages[0].NumberOfPackages);

			AssertEquals("For second package expected filled PackageType", "NE", packages[1].PackageType);
			AssertEquals("For second package expected filled Marks", "marks2", packages[1].Marks);
			AssertEquals("For second package expected empty NumberOfPackages", 0, packages[1].NumberOfPackages);

			AssertEquals("For third package expected filled PackageType", "VG", packages[2].PackageType);
			AssertEquals("For third package expected filled Marks", "bulk gas marks", packages[2].Marks);
			AssertEquals("For third package expected empty NumberOfPackages", 0, packages[2].NumberOfPackages);
		});
	}

	public void TestGetPackagesList_CanSendFRPackagesTrue_CanSendNEPackageQtyFalse()
	{
		AddPackages();

		var packages = DVDCommonPackageWrapper.GetPackagesList(entryLine, canSendFRPackages: true, canSendNEPackageQty: false).ToList();
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages", 4, packages.Count);

			AssertEquals("For first package expected filled PackageType", "CT", packages[0].PackageType);
			AssertEquals("For first package expected filled Marks", "marks", packages[0].Marks);
			AssertEquals("For first package expected filled NumberOfPackages", 9, packages[0].NumberOfPackages);

			AssertEquals("For second package expected filled PackageType", "NE", packages[1].PackageType);
			AssertEquals("For second package expected filled Marks", "marks2", packages[1].Marks);
			AssertEquals("For second package expected empty NumberOfPackages", 0, packages[1].NumberOfPackages);

			AssertEquals("For third package expected filled PackageType", "FR", packages[2].PackageType);
			AssertEquals("For third package expected filled Marks", "marks frame", packages[2].Marks);
			AssertEquals("For third package expected filled NumberOfPackages", 3, packages[2].NumberOfPackages);

			AssertEquals("For fourth package expected filled PackageType", "VG", packages[3].PackageType);
			AssertEquals("For fourth package expected filled Marks", "bulk gas marks", packages[3].Marks);
			AssertEquals("For fourth package expected empty NumberOfPackages", 0, packages[3].NumberOfPackages);
		});
	}

	public void TestGetPackagesList_CanSendFRPackagesTrue_CanSendNEPackageQtyTrue()
	{
		AddPackages();

		var packages = DVDCommonPackageWrapper.GetPackagesList(entryLine, canSendFRPackages: true).ToList();
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages", 4, packages.Count);

			AssertEquals("For first package expected filled PackageType", "CT", packages[0].PackageType);
			AssertEquals("For first package expected filled Marks", "marks", packages[0].Marks);
			AssertEquals("For first package expected filled NumberOfPackages", 9, packages[0].NumberOfPackages);

			AssertEquals("For second package expected filled PackageType", "NE", packages[1].PackageType);
			AssertEquals("For second package expected filled Marks", "marks2", packages[1].Marks);
			AssertEquals("For second package expected filled NumberOfPackages", 4, packages[1].NumberOfPackages);

			AssertEquals("For third package expected filled PackageType", "FR", packages[2].PackageType);
			AssertEquals("For third package expected filled Marks", "marks frame", packages[2].Marks);
			AssertEquals("For third package expected filled NumberOfPackages", 3, packages[2].NumberOfPackages);

			AssertEquals("For fourth package expected filled PackageType", "VG", packages[3].PackageType);
			AssertEquals("For fourth package expected filled Marks", "bulk gas marks", packages[3].Marks);
			AssertEquals("For fourth package expected empty NumberOfPackages", 0, packages[3].NumberOfPackages);
		});
	}

	public void TestGetPackagesListWhenVehicles()
	{
		var vehicle = invoiceLine.Vehicles.AddNew();
		vehicle.CVH_VehicleIdentificationNumber = "VINCode1";
		vehicle.CVH_BrandName = "Brand1";
		vehicle.CVH_ModelName = "Model1";
		var invLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		var vehicle2 = invLine2.Vehicles.AddNew();

		vehicle2.CVH_VehicleIdentificationNumber = "VINCode2";
		vehicle2.CVH_BrandName = "Brand2";

		var pack = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo = Factory.New<Customs.Business.BasePackage>();
		packageInfo.CW_PackType = RefCusCodeList.PackageType.Frame;
		pack.CHC_CW = packageInfo.PK;
		invoiceLine.PackagesPivot.Add(pack);

		var packages = DVDCommonPackageWrapper.GetPackagesList(entryLine);
		AssertEquals("Expected filled packages for vehicles", 0, packages.Count);
	}

	public void TestGetPackagesListWhenSamePackageType()
	{
		var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
		packageInfo1.CW_PackType = "CT";
		pack1.CHC_CW = packageInfo1.PK;
		pack1.CHC_NumberOfPacks = 2;
		invoiceLine.PackagesPivot.Add(pack1);

		var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo2 = Factory.New<Customs.Business.BasePackage>();
		packageInfo2.CW_PackType = "CT";
		pack2.CHC_CW = packageInfo2.PK;
		pack2.CHC_NumberOfPacks = 3;
		invoiceLine.PackagesPivot.Add(pack2);

		var packages = DVDCommonPackageWrapper.GetPackagesList(entryLine).ToList();
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages for packages with the same type only 1", 1, packages.Count);
			AssertEquals("For only package expected filled PackageType", "CT", packages[0].PackageType);
			AssertEquals("For only package expected filled NumberOfPackages", 5, packages[0].NumberOfPackages);
		});
	}

	public void TestGetPackagesListMultipleMergedLines_CanSendFRPackagesFalse_CanSendNEPackageQtyTrue()
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

		var packages = DVDCommonPackageWrapper.GetPackagesList(entryLine).ToList();
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages", 3, packages.Count);

			AssertEquals("For first package expected filled PackageType", "CT", packages[0].PackageType);
			AssertEquals("For first package expected filled Marks", "marks", packages[0].Marks);
			AssertEquals("For first package expected filled NumberOfPackages", 12, packages[0].NumberOfPackages);

			AssertEquals("For second package expected filled PackageType", "VG", packages[1].PackageType);
			AssertEquals("For second package expected filled Marks", "bulk gas marks", packages[1].Marks);
			AssertEquals("For second package expected filled NumberOfPackages", 0, packages[1].NumberOfPackages);

			AssertEquals("For third package expected filled PackageType", "NE", packages[2].PackageType);
			AssertEquals("For third package expected filled Marks", "marks2", packages[2].Marks);
			AssertEquals("For third package expected empty NumberOfPackages", 4, packages[2].NumberOfPackages);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		Factory.SetBulkTypeHelper();

		var declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();

		AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

		entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
	}

	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;

	void AddPackages()
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
		packageInfo3.CW_PackType = RefCusCodeList.PackageType.Frame;
		packageInfo3.CW_MarksAndNos = "marks frame";
		pack3.CHC_CW = packageInfo3.PK;
		pack3.CHC_NumberOfPacks = 3;
		invoiceLine.PackagesPivot.Add(pack3);

		var pack4 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo4 = Factory.New<Customs.Business.BasePackage>();
		packageInfo4.CW_PackType = "VG";
		packageInfo4.CW_MarksAndNos = "bulk gas marks";
		pack4.CHC_CW = packageInfo4.PK;
		pack4.CHC_NumberOfPacks = 2;
		invoiceLine.PackagesPivot.Add(pack4);
	}

	protected override DVDCommonPackageWrapper GetProvider() => new DVDCommonPackageWrapper("CT", "marks", 9);
}
