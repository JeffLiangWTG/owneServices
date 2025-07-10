using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

public class T2LPOUSRequestAndReceptionPackagingWrapperTest : WrapperHelperTest<T2LPOUSRequestAndReceptionPackagingWrapper>
{
	public void TestMarks()
	{
		var wrapper = new T2LPOUSRequestAndReceptionPackagingWrapper("marks", ZString.Empty, ZInt.Zero, Factory);
		AssertEquals("Expected filled Marks", "marks", wrapper.Marks);
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

		var pack4 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo4 = Factory.New<Customs.Business.BasePackage>();
		packageInfo4.CW_PackType = "FR";
		packageInfo4.CW_MarksAndNos = "FR marks";
		pack4.CHC_CW = packageInfo4.PK;
		pack4.CHC_NumberOfPacks = 3;
		invoiceLine.PackagesPivot.Add(pack4);

		var packages = T2LPOUSRequestAndReceptionPackagingWrapper.GetPackagesList(entryLine);
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages", 4, packages.Count);

			AssertEquals("For first package expected filled TypeOfPackages", "CT", packages[0].TypeOfPackages);
			AssertEquals("For first package expected filled Marks", "marks", packages[0].Marks);
			AssertEquals("For first package expected filled NumberOfPackages", 9, packages[0].NumberOfPackages);
			AssertEquals("For first package expected filled NumberOfPackagesValueSpecified", true, packages[0].NumberOfPackagesValueSpecified);

			AssertEquals("For second package expected filled TypeOfPackages", "NE", packages[1].TypeOfPackages);
			AssertEquals("For second package expected filled Marks", "marks2", packages[1].Marks);
			AssertEquals("For second package expected filled NumberOfPackages", 4, packages[1].NumberOfPackages);
			AssertEquals("For second package expected filled NumberOfPackagesValueSpecified", true, packages[1].NumberOfPackagesValueSpecified);

			AssertEquals("For third package expected filled TypeOfPackages", "VG", packages[2].TypeOfPackages);
			AssertEquals("For third package expected filled Marks", "bulk gas marks", packages[2].Marks);
			AssertEquals("For third package expected empty NumberOfPackages", 0, packages[2].NumberOfPackages);
			AssertEquals("For third package expected filled NumberOfPackagesValueSpecified", false, packages[2].NumberOfPackagesValueSpecified);

			AssertEquals("For fourth package expected filled TypeOfPackages", "FR", packages[3].TypeOfPackages);
			AssertEquals("For fourth package expected filled Marks", "FR marks", packages[3].Marks);
			AssertEquals("For fourth package expected filled NumberOfPackages", 3, packages[3].NumberOfPackages);
			AssertEquals("For fourth package expected filled NumberOfPackagesValueSpecified", true, packages[3].NumberOfPackagesValueSpecified);
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
		var invLine3 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		var vehicle3 = invLine3.Vehicles.AddNew();
		vehicle3.CVH_VehicleIdentificationNumber = "VINCode2";
		vehicle3.CVH_BrandName = "Brand2";

		var packages = T2LPOUSRequestAndReceptionPackagingWrapper.GetPackagesList(entryLine);
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages for vehicles", 3, packages.Count);

			AssertEquals("For first package expected filled TypeOfPackages", "FR", packages[0].TypeOfPackages);
			AssertEquals("For first package expected filled Marks", "VINCode1:Brand1:Model1", packages[0].Marks);
			AssertEquals("For first package expected filled NumberOfPackages", 1, packages[0].NumberOfPackages);
			AssertEquals("For first package expected filled NumberOfPackagesValueSpecified", true, packages[0].NumberOfPackagesValueSpecified);

			AssertEquals("For second package expected filled TypeOfPackages", "FR", packages[1].TypeOfPackages);
			AssertEquals("For second package expected filled Marks", "VINCode2:Brand2", packages[1].Marks);
			AssertEquals("For second package expected filled NumberOfPackages", 1, packages[1].NumberOfPackages);
			AssertEquals("For second package expected filled NumberOfPackagesValueSpecified", true, packages[1].NumberOfPackagesValueSpecified);
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

		var packages = T2LPOUSRequestAndReceptionPackagingWrapper.GetPackagesList(entryLine);
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages for packages with the same type only 1", 1, packages.Count);
			AssertEquals("For only package expected filled TypeOfPackages", InternalPackage1.Type, packages[0].TypeOfPackages);
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

		var pack5 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo4 = Factory.New<Customs.Business.BasePackage>();
		packageInfo4.CW_PackType = "FR";
		packageInfo4.CW_MarksAndNos = "FR marks";
		pack5.CHC_CW = packageInfo4.PK;
		pack5.CHC_NumberOfPacks = 3;
		invoiceLine3.PackagesPivot.Add(pack5);

		var packages = T2LPOUSRequestAndReceptionPackagingWrapper.GetPackagesList(entryLine);
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages", 4, packages.Count);

			AssertEquals("For first package expected filled TypeOfPackages", "CT", packages[0].TypeOfPackages);
			AssertEquals("For first package expected filled Marks", "marks", packages[0].Marks);
			AssertEquals("For first package expected filled NumberOfPackages", 12, packages[0].NumberOfPackages);
			AssertEquals("For first package expected filled NumberOfPackagesValueSpecified", true, packages[0].NumberOfPackagesValueSpecified);

			AssertEquals("For second package expected filled TypeOfPackages", "VG", packages[1].TypeOfPackages);
			AssertEquals("For second package expected filled Marks", "bulk gas marks", packages[1].Marks);
			AssertEquals("For second package expected empty NumberOfPackages", 0, packages[1].NumberOfPackages);
			AssertEquals("For second package expected filled NumberOfPackagesValueSpecified", false, packages[1].NumberOfPackagesValueSpecified);

			AssertEquals("For third package expected filled TypeOfPackages", "NE", packages[2].TypeOfPackages);
			AssertEquals("For third package expected filled Marks", "marks2", packages[2].Marks);
			AssertEquals("For third package expected filled NumberOfPackages", 4, packages[2].NumberOfPackages);
			AssertEquals("For third package expected filled NumberOfPackagesValueSpecified", true, packages[2].NumberOfPackagesValueSpecified);

			AssertEquals("For fourth package expected filled TypeOfPackages", "FR", packages[3].TypeOfPackages);
			AssertEquals("For fourth package expected filled Marks", "FR marks", packages[3].Marks);
			AssertEquals("For fourth package expected filled NumberOfPackages", 3, packages[3].NumberOfPackages);
			AssertEquals("For fourth package expected filled NumberOfPackagesValueSpecified", true, packages[3].NumberOfPackagesValueSpecified);
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

		var pack4 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo4 = Factory.New<Customs.Business.BasePackage>();
		packageInfo4.CW_PackType = "FR";
		packageInfo4.CW_MarksAndNos = "FR marks";
		pack4.CHC_CW = packageInfo4.PK;
		pack4.CHC_NumberOfPacks = 3;
		invoiceLine4.PackagesPivot.Add(pack4);

		var packages = T2LPOUSRequestAndReceptionPackagingWrapper.GetPackagesList(entryLine);
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages", 6, packages.Count);

			AssertEquals("For first package (vehicle) expected filled TypeOfPackages", "FR", packages[0].TypeOfPackages);
			AssertEquals("For first package (vehicle) expected filled Marks", "VINCode1:Brand1:Model1", packages[0].Marks);
			AssertEquals("For first package (vehicle) expected filled NumberOfPackages", 1, packages[0].NumberOfPackages);
			AssertEquals("For first package (vehicle) expected filled NumberOfPackagesValueSpecified", true, packages[0].NumberOfPackagesValueSpecified);

			AssertEquals("For second package (vehicle) expected filled TypeOfPackages", "FR", packages[1].TypeOfPackages);
			AssertEquals("For second package (vehicle) expected filled Marks", "VINCode2:Brand2:Model2", packages[1].Marks);
			AssertEquals("For second package (vehicle) expected filled NumberOfPackages", 1, packages[1].NumberOfPackages);
			AssertEquals("For second package (vehicle) expected filled NumberOfPackagesValueSpecified", true, packages[1].NumberOfPackagesValueSpecified);

			AssertEquals("For third package (vehicle) expected filled TypeOfPackages", "FR", packages[2].TypeOfPackages);
			AssertEquals("For third package (vehicle) expected filled Marks", "VINCode3:Brand3", packages[2].Marks);
			AssertEquals("For third package (vehicle) expected filled NumberOfPackages", 1, packages[2].NumberOfPackages);
			AssertEquals("For third package (vehicle) expected filled NumberOfPackagesValueSpecified", true, packages[2].NumberOfPackagesValueSpecified);

			AssertEquals("For first package (non vehicle) expected filled TypeOfPackages", "CT", packages[3].TypeOfPackages);
			AssertEquals("For first package (non vehicle) expected filled Marks", "marks", packages[3].Marks);
			AssertEquals("For first package (non vehicle) expected filled NumberOfPackages", 9, packages[3].NumberOfPackages);
			AssertEquals("For first package (non vehicle) expected filled NumberOfPackagesValueSpecified", true, packages[3].NumberOfPackagesValueSpecified);

			AssertEquals("For second package (non vehicle) expected filled TypeOfPackages", "NE", packages[4].TypeOfPackages);
			AssertEquals("For second package (non vehicle) expected filled Marks", "marks2", packages[4].Marks);
			AssertEquals("For second package (non vehicle) expected filled NumberOfPackages", 4, packages[4].NumberOfPackages);
			AssertEquals("For second package (non vehicle) expected filled NumberOfPackagesValueSpecified", true, packages[4].NumberOfPackagesValueSpecified);

			AssertEquals("For third package (non vehicle) expected filled TypeOfPackages", "VG", packages[5].TypeOfPackages);
			AssertEquals("For third package (non vehicle) expected filled Marks", "bulk gas marks", packages[5].Marks);
			AssertEquals("For third package (non vehicle) expected empty NumberOfPackages", 0, packages[5].NumberOfPackages);
			AssertEquals("For third package (non vehicle) expected filled NumberOfPackagesValueSpecified", false, packages[5].NumberOfPackagesValueSpecified);
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

	protected override T2LPOUSRequestAndReceptionPackagingWrapper GetProvider() => new T2LPOUSRequestAndReceptionPackagingWrapper(InternalPackage1.Marks, InternalPackage1.Type, InternalPackage1.NumberOfPackages, Factory);
}
