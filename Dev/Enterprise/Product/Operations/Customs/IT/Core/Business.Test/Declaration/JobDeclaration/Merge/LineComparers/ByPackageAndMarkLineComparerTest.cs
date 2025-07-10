using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class ByPackageAndMarkLineComparerTest : TestCaseWithFactory
{
	public void TestCompareWithSamePackAndMark()
	{
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var entryLine1 = entryHeader.MergedLines.AddNew();
		var invoiceLine1 = MergeTestHelper.GetNewInvoiceLine(invoice1, "tariff 1", "Invoice1 - Line1");
		invoiceLine1.JI_CL = entryLine1.PK;
		var packagePivot1 = MergeTestHelper.GetNewPackagePivot(invoiceLine1, packageCT, 0);

		var entryLine2 = entryHeader.MergedLines.AddNew();
		var invoiceLine2 = MergeTestHelper.GetNewInvoiceLine(invoice1, "tariff 2", "Invoice1 - Line2");
		invoiceLine2.JI_CL = entryLine2.PK;
		var packagePivot2 = MergeTestHelper.GetNewPackagePivot(invoiceLine2, packageCT, 2);

		var comparer = new ByPackageAndMarkLineComparer();
		AssertEquals("When line 2 has #packages greater than zero, it comes first", 1, comparer.Compare(invoiceLine1, invoiceLine2));

		invoiceLine1.JI_LineNo = 2;
		invoiceLine2.JI_LineNo = 1;
		AssertEquals("#packages wins on line number in the sort", 1, comparer.Compare(invoiceLine1, invoiceLine2));

		packagePivot1.CHC_NumberOfPacks = 8;
		AssertEquals("When lines are not in the same package, line number comes first", 1, comparer.Compare(invoiceLine1, invoiceLine2));
	}

	public void TestCompareWithDifferentPackAndMark()
	{
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var entryLine1 = entryHeader.MergedLines.AddNew();
		var invoiceLine1 = MergeTestHelper.GetNewInvoiceLine(invoice1, "tariff 1", "Invoice1 - Line1");
		invoiceLine1.JI_CL = entryLine1.PK;
		var packagePivot1 = MergeTestHelper.GetNewPackagePivot(invoiceLine1, packagePK, 0);

		var entryLine2 = entryHeader.MergedLines.AddNew();
		var invoiceLine2 = MergeTestHelper.GetNewInvoiceLine(invoice1, "tariff 2", "Invoice1 - Line2");
		invoiceLine2.JI_CL = entryLine2.PK;
		var packagePivot2 = MergeTestHelper.GetNewPackagePivot(invoiceLine2, packageCT, 2);

		var comparer = new ByPackageAndMarkLineComparer();
		AssertEquals("For lines not belonging to the same package, normal sort is used in the sort", -1, comparer.Compare(invoiceLine1, invoiceLine2));

		invoiceLine1.PackagesPivot.RemoveAll();
		invoiceLine2.PackagesPivot.RemoveAll();

		packagePivot1 = invoiceLine1.PackagesPivot.AddNew();
		packagePivot1.CHC_CW = packageCT.PK;
		packagePivot1.CHC_NumberOfPacks = 0;

		packagePivot2 = invoiceLine2.PackagesPivot.AddNew();
		packagePivot2.CHC_CW = packagePK.PK;
		packagePivot2.CHC_NumberOfPacks = 2;

		AssertEquals("invoice line 2 comes first", -1, comparer.Compare(invoiceLine1, invoiceLine2));
	}

	public void TestCompareWhenChangingPackagesPivot()
	{
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var entryLine1 = entryHeader.MergedLines.AddNew();
		var invoiceLine1 = MergeTestHelper.GetNewInvoiceLine(invoice1, "tariff 1", "Invoice1 - Line1");
		invoiceLine1.JI_CL = entryLine1.PK;
		var packagePivot1 = MergeTestHelper.GetNewPackagePivot(invoiceLine1, packagePK, 0);

		var entryLine2 = entryHeader.MergedLines.AddNew();
		var invoiceLine2 = MergeTestHelper.GetNewInvoiceLine(invoice1, "tariff 2", "Invoice1 - Line2");
		invoiceLine2.JI_CL = entryLine2.PK;
		var packagePivot2 = MergeTestHelper.GetNewPackagePivot(invoiceLine2, packageCT, 2);

		var comparer = new ByPackageAndMarkLineComparer();
		AssertEquals("When lines are not in the same package, line number comes first", -1, comparer.Compare(invoiceLine1, invoiceLine2));

		invoiceLine1.PackagesPivot.RemoveAll();
		invoiceLine2.PackagesPivot.RemoveAll();

		packagePivot1 = invoiceLine1.PackagesPivot.AddNew();
		packagePivot1.CHC_CW = packageCT.PK;
		packagePivot1.CHC_NumberOfPacks = 2;

		packagePivot2 = invoiceLine2.PackagesPivot.AddNew();
		packagePivot2.CHC_CW = packagePK.PK;
		packagePivot2.CHC_NumberOfPacks = 0;

		AssertEquals("Even changing the packages, when lines are not in the same package, line number comes first", -1, comparer.Compare(invoiceLine1, invoiceLine2));
	}

	public void TestCompareWithoutPacksAndMarks()
	{
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var entryLine1 = entryHeader.MergedLines.AddNew();
		var invoiceLine1 = MergeTestHelper.GetNewInvoiceLine(invoice1, "tariff 1", "Invoice1 - Line1");
		invoiceLine1.JI_CL = entryLine1.PK;

		var entryLine2 = entryHeader.MergedLines.AddNew();
		var invoiceLine2 = MergeTestHelper.GetNewInvoiceLine(invoice1, "tariff 2", "Invoice1 - Line2");
		invoiceLine2.JI_CL = entryLine2.PK;

		var comparer = new ByPackageAndMarkLineComparer();
		AssertEquals("When no package information is available, normal sort is used", -1, comparer.Compare(invoiceLine1, invoiceLine2));
	}

	public void TestCompareWithOnlyOnePack()
	{
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var entryLine1 = entryHeader.MergedLines.AddNew();
		var invoiceLine1 = MergeTestHelper.GetNewInvoiceLine(invoice1, "tariff 1", "Invoice1 - Line1");
		invoiceLine1.JI_CL = entryLine1.PK;
		var packagePivot1 = MergeTestHelper.GetNewPackagePivot(invoiceLine1, packagePK, 1);

		var entryLine2 = entryHeader.MergedLines.AddNew();
		var invoiceLine2 = MergeTestHelper.GetNewInvoiceLine(invoice1, "tariff 2", "Invoice1 - Line2");

		var comparer = new ByPackageAndMarkLineComparer();
		AssertNoExceptionThrown("No exception if second line has no package", () => comparer.Compare(invoiceLine1, invoiceLine2));
		AssertEquals("No packs for invoice line 2 - invoice line order is respected", -1, comparer.Compare(invoiceLine1, invoiceLine2));
		invoiceLine1.PackagesPivot.RemoveAll();

		var packagePivot2 = invoiceLine2.PackagesPivot.AddNew();
		packagePivot2.CHC_CW = packageCT.PK;
		packagePivot2.CHC_NumberOfPacks = 2;

		AssertNoExceptionThrown("No exception if first line has no package", () => comparer.Compare(invoiceLine1, invoiceLine2));
		AssertEquals("No packs for invoice line 1 - invoice line order is respected", -1, comparer.Compare(invoiceLine1, invoiceLine2));
	}

	protected override void SetUp()
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		declaration.Packages.RemoveAndDeleteAll();

		declarationBill = declaration.Bills.AddNew();
		billPackingGroup = declarationBill.PackingGroups.AddNew();
		packageCT = declaration.Packages.AddNew();
		packageCT.CW_CR_HouseContainer = billPackingGroup.PK;
		packageCT.CW_PackQty = 1;
		packageCT.CW_PackType = "CT";
		packageCT.CW_MarksAndNos = "IND";

		packagePK = declaration.Packages.AddNew();
		packagePK.CW_CR_HouseContainer = billPackingGroup.PK;
		packagePK.CW_PackQty = 3;
		packagePK.CW_PackType = "PK";
		packagePK.CW_MarksAndNos = "IND";

		invoice1 = declaration.Invoices.AddNew();
		invoice1.JZ_InvoiceNumber = "1";
		invoice2 = declaration.Invoices.AddNew();
		invoice2.JZ_InvoiceNumber = "2";
	}

	JobDeclaration declaration;
	Bill declarationBill;
	BasePackingGroup billPackingGroup;

	JobComInvoiceHeader invoice1;
	JobComInvoiceHeader invoice2;

	BasePackage packageCT;
	BasePackage packagePK;
}
