using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

public class CommonPackageWithSequenceAndPackNumWrapperTest : WrapperHelperTest<CommonPackageWithSequenceAndPackNumWrapper>
{
	public void TestSequenceNumber()
	{
		CombineAssertions(() =>
		{
			var wrapper = new CommonPackageWithSequenceAndPackNumWrapper(ZString.Empty, ZString.Empty, ZInt.Zero, 2, Factory);
			AssertEquals("Expected filled SequenceNumber with constructor with int quantity", "2", wrapper.SequenceNumber);

			wrapper = new CommonPackageWithSequenceAndPackNumWrapper(ZString.Empty, ZString.Empty, ZString.Empty, 5);
			AssertEquals("Expected filled SequenceNumber with constructor with string quantity", "5", wrapper.SequenceNumber);
		});
	}

	public void TestNumberOfPackages()
	{
		CombineAssertions(() =>
		{
			var wrapper = new CommonPackageWithSequenceAndPackNumWrapper(ZString.Empty, ZString.Empty, 9, ZShort.Zero, Factory);
			AssertEquals("Expected filled NumberOfPackages with constructor with int quantity", "9", wrapper.NumberOfPackages);

			wrapper = new CommonPackageWithSequenceAndPackNumWrapper(ZString.Empty, ZString.Empty, "5", ZShort.Zero);
			AssertEquals("Expected filled NumberOfPackages with constructor with string quantity", "5", wrapper.NumberOfPackages);
		});
	}

	public void TestNumberOfPackages0WithTypeBulk()
	{
		var wrapper = new CommonPackageWithSequenceAndPackNumWrapper("VG", ZString.Empty, 0, ZShort.Zero, Factory);
		AssertEquals("Expected filled NumberOfPackages with type bulk", string.Empty, wrapper.NumberOfPackages);
	}

	public void TestNumberOfPackages0WithoutTypeBulk()
	{
		var wrapper = new CommonPackageWithSequenceAndPackNumWrapper("CT", ZString.Empty, 0, ZShort.Zero, Factory);
		AssertEquals("Expected filled NumberOfPackages without type bulk", "0", wrapper.NumberOfPackages);
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

		var packages = CommonPackageWithSequenceAndPackNumWrapper.GetPackagesList(entryLine).ToList();
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages", 4, packages.Count);

			AssertEquals("For first package expected filled PackageType", "CT", packages[0].PackageType);
			AssertEquals("For first package expected filled Marks", "marks", packages[0].Marks);
			AssertEquals("For first package expected filled NumberOfPackages", "9", packages[0].NumberOfPackages);
			AssertEquals("For first package expected filled SequenceNumber", "1", packages[0].SequenceNumber);

			AssertEquals("For second package expected filled PackageType", "NE", packages[1].PackageType);
			AssertEquals("For second package expected filled Marks", "marks2", packages[1].Marks);
			AssertEquals("For second package expected empty NumberOfPackages", "4", packages[1].NumberOfPackages);
			AssertEquals("For second package expected filled SequenceNumber", "2", packages[1].SequenceNumber);

			AssertEquals("For third package expected filled PackageType", "VG", packages[2].PackageType);
			AssertEquals("For third package expected filled Marks", "bulk gas marks", packages[2].Marks);
			AssertEquals("For third package expected empty NumberOfPackages", ZString.Empty, packages[2].NumberOfPackages);
			AssertEquals("For third package expected filled SequenceNumber", "3", packages[2].SequenceNumber);

			AssertEquals("For fourth package expected filled PackageType", "FR", packages[3].PackageType);
			AssertEquals("For fourth package expected filled Marks", "FR marks", packages[3].Marks);
			AssertEquals("For fourth package expected empty NumberOfPackages", "3", packages[3].NumberOfPackages);
			AssertEquals("For fourth package expected filled SequenceNumber", "4", packages[3].SequenceNumber);
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

		var packages = CommonPackageWithSequenceAndPackNumWrapper.GetPackagesList(entryLine).ToList();
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages for vehicles", 3, packages.Count);

			AssertEquals("For first package expected filled PackageType", "FR", packages[0].PackageType);
			AssertEquals("For first package expected filled Marks", "VINCode1:Brand1:Model1", packages[0].Marks);
			AssertEquals("For first package expected filled NumberOfPackages", "1", packages[0].NumberOfPackages);
			AssertEquals("For first package expected filled SequenceNumber", "1", packages[0].SequenceNumber);

			AssertEquals("For second package expected filled PackageType", "FR", packages[1].PackageType);
			AssertEquals("For second package expected filled Marks", "VINCode2:Brand2", packages[1].Marks);
			AssertEquals("For second package expected empty NumberOfPackages", "1", packages[1].NumberOfPackages);
			AssertEquals("For second package expected filled SequenceNumber", "2", packages[1].SequenceNumber);

			AssertEquals("For third package expected filled PackageType", "FR", packages[2].PackageType);
			AssertEquals("For third package expected filled Marks", "VINCode2:Brand2", packages[2].Marks);
			AssertEquals("For third package expected empty NumberOfPackages", "1", packages[2].NumberOfPackages);
			AssertEquals("For third package expected filled SequenceNumber", "3", packages[2].SequenceNumber);
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

		var packages = CommonPackageWithSequenceAndPackNumWrapper.GetPackagesList(entryLine).ToList();
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

		var pack5 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
		var packageInfo4 = Factory.New<Customs.Business.BasePackage>();
		packageInfo4.CW_PackType = "FR";
		packageInfo4.CW_MarksAndNos = "FR marks";
		pack5.CHC_CW = packageInfo4.PK;
		pack5.CHC_NumberOfPacks = 3;
		invoiceLine3.PackagesPivot.Add(pack5);

		var packages = CommonPackageWithSequenceAndPackNumWrapper.GetPackagesList(entryLine).ToList();
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages", 4, packages.Count);

			AssertEquals("For first package expected filled PackageType", "CT", packages[0].PackageType);
			AssertEquals("For first package expected filled Marks", "marks", packages[0].Marks);
			AssertEquals("For first package expected filled NumberOfPackages", "12", packages[0].NumberOfPackages);
			AssertEquals("For first package expected filled SequenceNumber", "1", packages[0].SequenceNumber);

			AssertEquals("For second package expected filled PackageType", "VG", packages[1].PackageType);
			AssertEquals("For second package expected filled Marks", "bulk gas marks", packages[1].Marks);
			AssertEquals("For second package expected empty NumberOfPackages", ZString.Empty, packages[1].NumberOfPackages);
			AssertEquals("For second package expected filled SequenceNumber", "2", packages[1].SequenceNumber);

			AssertEquals("For third package expected filled PackageType", "NE", packages[2].PackageType);
			AssertEquals("For third package expected filled Marks", "marks2", packages[2].Marks);
			AssertEquals("For third package expected empty NumberOfPackages", "4", packages[2].NumberOfPackages);
			AssertEquals("For third package expected filled SequenceNumber", "3", packages[2].SequenceNumber);

			AssertEquals("For fourth package expected filled PackageType", "FR", packages[3].PackageType);
			AssertEquals("For fourth package expected filled Marks", "FR marks", packages[3].Marks);
			AssertEquals("For fourth package expected empty NumberOfPackages", "3", packages[3].NumberOfPackages);
			AssertEquals("For fourth package expected filled SequenceNumber", "4", packages[3].SequenceNumber);
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

		var packages = CommonPackageWithSequenceAndPackNumWrapper.GetPackagesList(entryLine).ToList();
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled packages", 6, packages.Count);

			AssertEquals("For first package (vehicle) expected filled PackageType", "FR", packages[0].PackageType);
			AssertEquals("For first package (vehicle) expected filled Marks", "VINCode1:Brand1:Model1", packages[0].Marks);
			AssertEquals("For first package (vehicle) expected filled NumberOfPackages", "1", packages[0].NumberOfPackages);
			AssertEquals("For first package (vehicle) expected filled SequenceNumber", "1", packages[0].SequenceNumber);

			AssertEquals("For second package (vehicle) expected filled PackageType", "FR", packages[1].PackageType);
			AssertEquals("For second package (vehicle) expected filled Marks", "VINCode2:Brand2:Model2", packages[1].Marks);
			AssertEquals("For second package (vehicle) expected filled NumberOfPackages", "1", packages[1].NumberOfPackages);
			AssertEquals("For second package (vehicle) expected filled SequenceNumber", "2", packages[1].SequenceNumber);

			AssertEquals("For third package (vehicle) expected filled PackageType", "FR", packages[2].PackageType);
			AssertEquals("For third package (vehicle) expected filled Marks", "VINCode3:Brand3", packages[2].Marks);
			AssertEquals("For third package (vehicle) expected filled NumberOfPackages", "1", packages[2].NumberOfPackages);
			AssertEquals("For third package (vehicle) expected filled SequenceNumber", "3", packages[2].SequenceNumber);

			AssertEquals("For first package (non vehicle) expected filled PackageType", "CT", packages[3].PackageType);
			AssertEquals("For first package (non vehicle) expected filled Marks", "marks", packages[3].Marks);
			AssertEquals("For first package (non vehicle) expected filled NumberOfPackages", "9", packages[3].NumberOfPackages);
			AssertEquals("For first package (non vehicle) expected filled SequenceNumber", "4", packages[3].SequenceNumber);

			AssertEquals("For second package (non vehicle) expected filled PackageType", "NE", packages[4].PackageType);
			AssertEquals("For second package (non vehicle) expected filled Marks", "marks2", packages[4].Marks);
			AssertEquals("For second package (non vehicle) expected empty NumberOfPackages", "4", packages[4].NumberOfPackages);
			AssertEquals("For second package (non vehicle) expected filled SequenceNumber", "5", packages[4].SequenceNumber);

			AssertEquals("For third package (non vehicle) expected filled PackageType", "VG", packages[5].PackageType);
			AssertEquals("For third package (non vehicle) expected filled Marks", "bulk gas marks", packages[5].Marks);
			AssertEquals("For third package (non vehicle) expected empty NumberOfPackages", ZString.Empty, packages[5].NumberOfPackages);
			AssertEquals("For third package (non vehicle) expected filled SequenceNumber", "6", packages[5].SequenceNumber);
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

	protected override CommonPackageWithSequenceAndPackNumWrapper GetProvider() => new CommonPackageWithSequenceAndPackNumWrapper(InternalPackage1.Type, InternalPackage1.Marks, InternalPackage1.NumberOfPackages, 1, Factory);
}
