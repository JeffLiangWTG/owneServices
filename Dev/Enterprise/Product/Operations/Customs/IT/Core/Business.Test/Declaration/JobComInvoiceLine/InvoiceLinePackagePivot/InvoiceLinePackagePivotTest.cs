using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CustomsUniversalConstants = Enterprise.Customs.Business.UniversalReferenceConstants;
using EUConstants = Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(InvoiceLinePackagePivot))]
sealed class InvoiceLinePackagePivotTest : ZArchitecture.Business.Testing.EnterpriseBusinessObjectTestCase
{
	public void TestGetNewValidation()
	{
		var validation = (Factory.New<InvoiceLinePackagePivotForTest>()).GetNewValidation_Exposed();
		AssertType<InvoiceLinePackagePivotValidation>(validation);
	}

	public void TestPreviousPackages()
	{
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "1";

		var invoiceLine1 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff 1", "invoiceLine1");
		var packagePivot1 = GetNewPackagePivot(invoiceLine1, packageCT, 1);

		LineMerger merger = new LineMerger(declaration);
		merger.DoMerge();

		CombineAssertions("Expected previous packages", () =>
		{
			var previousPackages = GetPreviousPackagesAsArray(packagePivot1);

			AssertEquals("previous packages number with just one line", 0, previousPackages.Length);
		});

		var invoiceLine2 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff 2", "InvoiceLine2");
		var packagePivot2 = GetNewPackagePivot(invoiceLine2, packageCT, 0);

		merger.DoMerge();
		CombineAssertions("Expected previous packages", () =>
		{
			var previousPackages = GetPreviousPackagesAsArray(packagePivot2);

			AssertEquals("previous packages number with two lines", 1, previousPackages.Length);
			AssertEquals("Second package", 1, previousPackages[0].UnitCount);
		});

		var invoiceLine3 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff 3", "InvoiceLine3");
		var packagePivot3 = GetNewPackagePivot(invoiceLine3, packageCT, 0);

		merger.DoMerge();
		CombineAssertions("Expected previous packages", () =>
		{
			var previousPackages = GetPreviousPackagesAsArray(packagePivot3);

			AssertEquals("previous packages number", 2, previousPackages.Length);
			AssertEquals("First package", 0, previousPackages[0].UnitCount);
			AssertEquals("Second package", 1, previousPackages[1].UnitCount);
		});
	}

	public void TestPreviousPackagesWithMoreInvoiceLinesInOneEntryLine()
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "1";

		var invoiceLine1 = MergeTestHelper.GetNewInvoiceLine(invoice, "111", "invoiceLine1");
		var packagePivot1 = GetNewPackagePivot(invoiceLine1, packageCT, 1);

		var invoiceLine2 = MergeTestHelper.GetNewInvoiceLine(invoice, "111", "invoiceLine2");
		var packagePivot2 = GetNewPackagePivot(invoiceLine2, packageCT, 0);

		var invoiceLine3 = MergeTestHelper.GetNewInvoiceLine(invoice, "222", "invoiceLine3");
		var packagePivot3 = GetNewPackagePivot(invoiceLine3, packageCT, 0);

		var invoiceLine4 = MergeTestHelper.GetNewInvoiceLine(invoice, "333", "invoiceLine4");
		var packagePivot4 = GetNewPackagePivot(invoiceLine4, packageCT, 0);

		var invoiceLine5 = MergeTestHelper.GetNewInvoiceLine(invoice, "333", "invoiceLine5");
		var packagePivot5 = GetNewPackagePivot(invoiceLine5, packageCT, 0);

		var invoiceLine6 = MergeTestHelper.GetNewInvoiceLine(invoice, "444", "invoiceLine6");
		var packagePivot6 = GetNewPackagePivot(invoiceLine6, packageCT, 0);

		LineMerger merger = new LineMerger(declaration);
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
		merger.DoMerge();

		CombineAssertions("Expected previous packages", () =>
		{
			AssertEquals("previous packages number for line 1 is 0", 0, GetPreviousPackagesLength(packagePivot1));
			AssertEquals("previous packages number for line 2 is 1", 1, GetPreviousPackagesLength(packagePivot2));
			AssertEquals("previous packages number for line 3 is 2", 2, GetPreviousPackagesLength(packagePivot3));
			AssertEquals("previous packages number for line 4 is 3", 3, GetPreviousPackagesLength(packagePivot4));
			AssertEquals("previous packages number for line 5 is 4", 4, GetPreviousPackagesLength(packagePivot5));
			AssertEquals("previous packages number for line 6 is 5", 5, GetPreviousPackagesLength(packagePivot6));
		});

		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		merger.DoMerge();

		CombineAssertions("Expected previous packages", () =>
		{
			AssertEquals("previous packages number for line 1 is 0", 0, GetPreviousPackagesLength(packagePivot1));
			AssertEquals("previous packages number for line 2 is 0", 0, GetPreviousPackagesLength(packagePivot2));
			AssertEquals("previous packages number for line 3 is 2", 2, GetPreviousPackagesLength(packagePivot3));
			AssertEquals("previous packages number for line 4 is 3", 3, GetPreviousPackagesLength(packagePivot4));
			AssertEquals("previous packages number for line 5 is 3", 3, GetPreviousPackagesLength(packagePivot5));
			AssertEquals("previous packages number for line 6 is 5", 5, GetPreviousPackagesLength(packagePivot6));
		});
	}

	public void TestIsEmptyPackTypeAllowed()
	{
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "1";
		var invoiceLine1 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff 1", "invoiceLine1");
		var packagePivot1 = MergeTestHelper.GetNewPackagePivot(invoiceLine1, packageCT, 0);

		AssertIsEmptyPackTypeAllowed(packagePivot1, EUConstants.RefCusCodeBulkPackageUnitType.BulkLiquidGas, true);
		AssertIsEmptyPackTypeAllowed(packagePivot1, EUConstants.RefCusCodeBulkPackageUnitType.BulkGas, true);
		AssertIsEmptyPackTypeAllowed(packagePivot1, EUConstants.RefCusCodeBulkPackageUnitType.BulkLiquid, true);
		AssertIsEmptyPackTypeAllowed(packagePivot1, EUConstants.RefCusCodeBulkPackageUnitType.BulkPowders, true);
		AssertIsEmptyPackTypeAllowed(packagePivot1, EUConstants.RefCusCodeBulkPackageUnitType.BulkGrains, true);
		AssertIsEmptyPackTypeAllowed(packagePivot1, EUConstants.RefCusCodeBulkPackageUnitType.BulkNodules, true);
		AssertIsEmptyPackTypeAllowed(packagePivot1, EUConstants.RefCusCodeUnPackedPackageUnitType.Unpacked, true);
		AssertIsEmptyPackTypeAllowed(packagePivot1, "A1", false);
	}

	void AssertIsEmptyPackTypeAllowed(InvoiceLinePackagePivot packagePivot, string type, bool expectedValue)
	{
		packagePivot.Package.CW_PackType = type;
		AssertEquals(ZString.Format("for type{0}", type), expectedValue, packagePivot.Package.IsEmptyPackTypeAllowed);
	}

	public void TestUnitType()
	{
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "1";

		var invoiceLine1 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff 1", "invoiceLine1");
		var packagePivot1 = MergeTestHelper.GetNewPackagePivot(invoiceLine1, packageCT, 1);

		AssertEquals("When CW_PackType is CT, Package_UnitType is CT", "CT", ((IRN22CheckablePackage)packagePivot1).UnitType);
	}

	public void TestMarksAndNumbers()
	{
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "1";

		var invoiceLine1 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff 1", "invoiceLine1");
		var packagePivot1 = MergeTestHelper.GetNewPackagePivot(invoiceLine1, packageCT, 1);

		AssertEquals("When CW_MarksAndNos is IND, Package_MarksAndNumbers is IND", "IND", ((IRN22CheckablePackage)packagePivot1).MarksAndNumbers);
	}

	public void TestUnitCount()
	{
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "1";

		var invoiceLine1 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff 1", "invoiceLine1");
		var packagePivot1 = MergeTestHelper.GetNewPackagePivot(invoiceLine1, packageCT, 1);
		AssertEquals("When CW_PackQty is 1, Package_UnitCount is 1", 1, ((IRN22CheckablePackage)packagePivot1).UnitCount);

		var invoiceLine2 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff 2", "invoiceLine2");
		var packagePivot2 = MergeTestHelper.GetNewPackagePivot(invoiceLine2, packageCT, 0);
		AssertEquals("When CW_PackQty is 0, Package_UnitCount is 0", 0, ((IRN22CheckablePackage)packagePivot2).UnitCount);
	}

	public void TestUnitCountInfo()
	{
		var invoice = declaration.Invoices.AddNew();

		var invoiceLine = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff 1", "invoiceLine1");
		var packagePivot1 = MergeTestHelper.GetNewPackagePivot(invoiceLine, packageCT, 1);

		AssertEquals("Package_UnitCountInfo of the interface must be CHC_NumberOfPacksInfo", packagePivot1.CHC_NumberOfPacksInfo, ((IRN22CheckablePackage)packagePivot1).UnitCountInfo);
	}

	public void TestCHC_NumberOfPacksHumanReadableName()
	{
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = MergeTestHelper.GetNewInvoiceLine(invoice, "8001.10.00", "invoiceLine1");

		var packagePivot = MergeTestHelper.GetNewPackagePivot(invoiceLine, packageCT, 1);
		AssertEquals("HumanReadableName", "Invoice Line Pack Quantity", packagePivot.CHC_NumberOfPacksInfo.HumanReadableName);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = MergeTestHelper.GetNewInvoiceLine(invoice, "8001.10.00", "invoiceLine1");

		var packagePivot = MergeTestHelper.GetNewPackagePivot(invoiceLine, packageCT, 1);
		return packagePivot;
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		var declarationBill = declaration.Bills.AddNew();
		declarationBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
		declarationBill.CU_BillNum = "999";

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = MergeTestHelper.GetNewInvoiceLine(invoice, "55", "invoice line");
		var package = declaration.Packages.AddNew();
		package.CW_PackQty = 1;
		package.CW_HouseBill = declarationBill.CU_BillUniqueCode;

		var packagePivot = MergeTestHelper.GetNewPackagePivot(invoiceLine, package, 1);
		packagePivot.CHC_JE = declaration.PK;
		return packagePivot;
	}

	IRN22CheckablePackage GetNewPackagePivot(JobComInvoiceLine invoiceLine, BasePackage package, ZInt numberOfPacks) => MergeTestHelper.GetNewPackagePivot(invoiceLine, package, numberOfPacks);

	IRN22CheckablePackage[] GetPreviousPackagesAsArray(IRN22CheckablePackage packagePivot) => packagePivot.GetRelatedEntryPreviousPackages().ToArray();

	ZInt GetPreviousPackagesLength(IRN22CheckablePackage packagePivot) => GetPreviousPackagesAsArray(packagePivot).Length;

	protected override void SetUp()
	{
		base.SetUp();

		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VQ", "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, CustomsUniversalConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, CustomsUniversalConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VL", "VL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, CustomsUniversalConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VY", "VY", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, CustomsUniversalConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VR", "VR", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, CustomsUniversalConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VO", "VO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, CustomsUniversalConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, CustomsUniversalConstants.PackageUnitAttributes.BreakBulk, "");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
		Factory.Save();

		declaration = Factory.New<JobDeclaration>();

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var container = declaration.CusContainers.AddNew();
		container.CO_ContainerNumber = "OOCL0000006";

		var declarationBill = declaration.Bills.AddNew();
		declarationBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
		declarationBill.CU_BillNum = "999";

		packageCT = declaration.Packages.AddNew();
		packageCT.CW_PackQty = 1;
		packageCT.CW_PackType = "CT";
		packageCT.CW_MarksAndNos = "IND";
		packageCT.CW_HouseBill = declarationBill.CU_BillUniqueCode;

		packagePK = declaration.Packages.AddNew();
		packagePK.CW_PackQty = 3;
		packagePK.CW_PackType = "PK";
		packagePK.CW_MarksAndNos = "IND";
		packagePK.CW_HouseBill = declarationBill.CU_BillUniqueCode;

		packageVG = declaration.Packages.AddNew();
		packageVG.CW_PackQty = 1;
		packageVG.CW_PackType = "VG";
		packageVG.CW_MarksAndNos = "IND";
		packageVG.CW_HouseBill = declarationBill.CU_BillUniqueCode;

		packageNE = declaration.Packages.AddNew();
		packageNE.CW_PackQty = 1;
		packageNE.CW_PackType = "NE";
		packageNE.CW_MarksAndNos = "IND";
		packageNE.CW_HouseBill = declarationBill.CU_BillUniqueCode;
	}

	JobDeclaration declaration;

	BasePackage packageNE;
	BasePackage packageVG;
	BasePackage packagePK;
	BasePackage packageCT;
}

class InvoiceLinePackagePivotForTest : InvoiceLinePackagePivot
{
	public InvoiceLinePackagePivotForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public CusHouseContPackInvoiceLinePivotValidation GetNewValidation_Exposed() => base.GetNewValidation();
}
