using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class LineMergerWithSortTest : EU.Business.Testing.LineMergerTest
{
	public void TestGetLineNumberAssigner()
	{
		var lineNumberAssigner = new LineMergerForTest(declaration).GetLineNumberAssigner_Exposed(Factory.New<CusEntryHeader>());
		AssertType<LineNumberAssigner>(lineNumberAssigner);
	}

	public void TestNoLineToMove()
	{
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "1";

		var invoiceLine1 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff1", "invoiceLine1");
		var packagePivot1 = MergeTestHelper.GetNewPackagePivot(invoiceLine1, packageCT, 1);

		var invoiceLine2 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff2", "invoiceLine2");
		var packagePivot2 = MergeTestHelper.GetNewPackagePivot(invoiceLine2, packagePK, 2);

		var invoiceLine3 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff3", "invoiceLine3");
		var packagePivot3 = MergeTestHelper.GetNewPackagePivot(invoiceLine3, packageCT, 1);

		LineMerger merger = new LineMerger(declaration);
		merger.DoMerge();

		var entryHeader = declaration.CustomsEntryHeaders.Single();
		AssertNotNull("entryHeader must not be null", entryHeader);

		CombineAssertions("Expected invoice lines order", () =>
		{
			AssertEquals("For EntryLine 1", "invoiceLine1", entryHeader.MergedLines[0].Description);
			AssertEquals("For EntryLine 2", "invoiceLine2", entryHeader.MergedLines[1].Description);
			AssertEquals("For EntryLine 3", "invoiceLine3", entryHeader.MergedLines[2].Description);
		});
	}

	public void TestZeroPackLineBubblingUp()
	{
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "1";

		var invoiceLine1 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff1", "invoiceLine1");
		var packagePivot1 = MergeTestHelper.GetNewPackagePivot(invoiceLine1, packageCT, 1);

		var invoiceLine2 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff2", "invoiceLine2");
		var packagePivot2 = MergeTestHelper.GetNewPackagePivot(invoiceLine2, packagePK, 2);

		var invoiceLine3 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff3", "invoiceLine3");
		var packagePivot3 = MergeTestHelper.GetNewPackagePivot(invoiceLine3, packageCT, 0);

		LineMerger merger = new LineMerger(declaration);
		merger.DoMerge();

		var entryHeader = declaration.CustomsEntryHeaders.Single();
		AssertNotNull("entryHeader must not be null", entryHeader);

		CombineAssertions("Expected invoice lines order", () =>
		{
			AssertEquals("For EntryLine 1", "invoiceLine1", entryHeader.MergedLines[0].Description);
			AssertEquals("For EntryLine 2", "invoiceLine3", entryHeader.MergedLines[1].Description);
			AssertEquals("For EntryLine 3", "invoiceLine2", entryHeader.MergedLines[2].Description);
		});
	}

	public void TestZeroPackLineSinkingDown()
	{
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "1";

		var invoiceLine1 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff1", "invoiceLine1");
		var packagePivot1 = MergeTestHelper.GetNewPackagePivot(invoiceLine1, packageCT, 0);

		var invoiceLine2 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff2", "invoiceLine2");
		var packagePivot2 = MergeTestHelper.GetNewPackagePivot(invoiceLine2, packagePK, 2);

		var invoiceLine3 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff3", "invoiceLine3");
		var packagePivot3 = MergeTestHelper.GetNewPackagePivot(invoiceLine3, packageCT, 1);

		LineMerger merger = new LineMerger(declaration);
		merger.DoMerge();

		var entryHeader = declaration.CustomsEntryHeaders.Single();
		AssertNotNull("entryHeader must not be null", entryHeader);

		CombineAssertions("Expected invoice lines order", () =>
		{
			AssertEquals("For EntryLine 1", "invoiceLine2", entryHeader.MergedLines[0].Description);
			AssertEquals("For EntryLine 2", "invoiceLine3", entryHeader.MergedLines[1].Description);
			AssertEquals("For EntryLine 3", "invoiceLine1", entryHeader.MergedLines[2].Description);
		});
	}

	public void TestZeroPackSiblingsMantainOriginalOrder()
	{
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "1";

		var invoiceLine1 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff1", "invoiceLine1");
		var packagePivot1 = MergeTestHelper.GetNewPackagePivot(invoiceLine1, packageCT, 0);

		var invoiceLine2 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff2", "invoiceLine2");
		var packagePivot2 = MergeTestHelper.GetNewPackagePivot(invoiceLine2, packageCT, 0);

		var invoiceLine3 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff3", "invoiceLine3");
		var packagePivot3 = MergeTestHelper.GetNewPackagePivot(invoiceLine3, packageCT, 1);

		LineMerger merger = new LineMerger(declaration);
		merger.DoMerge();

		var entryHeader = declaration.CustomsEntryHeaders.Single();
		AssertNotNull("entryHeader must not be null", entryHeader);

		CombineAssertions("Expected invoice lines order", () =>
		{
			AssertEquals("For EntryLine 1", "invoiceLine3", entryHeader.MergedLines[0].Description);
			AssertEquals("For EntryLine 2", "invoiceLine1", entryHeader.MergedLines[1].Description);
			AssertEquals("For EntryLine 3", "invoiceLine2", entryHeader.MergedLines[2].Description);
		});
	}

	public void TestSortWithMoreInvoicesAndSiblings()
	{
		var invoice1 = declaration.Invoices.AddNew();
		invoice1.JZ_InvoiceNumber = "1";
		var invoice2 = declaration.Invoices.AddNew();
		invoice2.JZ_InvoiceNumber = "2";

		var invoiceLine1 = MergeTestHelper.GetNewInvoiceLine(invoice1, "8001.10.00", "Invoice1 - Line1");
		var packagePivot1 = MergeTestHelper.GetNewPackagePivot(invoiceLine1, packageCT, 0);

		var invoiceLine2 = MergeTestHelper.GetNewInvoiceLine(invoice1, "8007.00.10", "Invoice1 - Line2");
		var packagePivot2 = MergeTestHelper.GetNewPackagePivot(invoiceLine2, packagePK, 2);

		var invoiceLine3 = MergeTestHelper.GetNewInvoiceLine(invoice1, "8507.90.80", "Invoice1 - Line3");
		var packagePivot3 = MergeTestHelper.GetNewPackagePivot(invoiceLine3, packageCT, 1);

		var invoiceLine4 = MergeTestHelper.GetNewInvoiceLine(invoice1, "8007.00.80", "Invoice1 - Line4");
		var packagePivot4 = MergeTestHelper.GetNewPackagePivot(invoiceLine4, packageDM, 1);

		var invoiceLine5 = MergeTestHelper.GetNewInvoiceLine(invoice1, "8001.20.00", "Invoice1 - Line5");
		var packagePivot5 = MergeTestHelper.GetNewPackagePivot(invoiceLine5, packageCT, 0);

		var invoiceLine2_1 = MergeTestHelper.GetNewInvoiceLine(invoice1, "9006.53.80", "Invoice2 - Line1");
		var packagePivot6 = MergeTestHelper.GetNewPackagePivot(invoiceLine2_1, packagePK, 1);

		var invoiceLine2_2 = MergeTestHelper.GetNewInvoiceLine(invoice1, "9006.61.00", "Invoice2 - Line2");
		var packagePivot7 = MergeTestHelper.GetNewPackagePivot(invoiceLine2_2, packageA2, 2);

		var invoiceLine2_3 = MergeTestHelper.GetNewInvoiceLine(invoice1, "9007.91.00", "Invoice2 - Line3");
		var packagePivot8 = MergeTestHelper.GetNewPackagePivot(invoiceLine2_3, packageDM, 0);

		var invoiceLine2_4 = MergeTestHelper.GetNewInvoiceLine(invoice1, "9006.53.10", "Invoice2 - Line4");
		var packagePivot9 = MergeTestHelper.GetNewPackagePivot(invoiceLine2_4, packageDM, 0);

		LineMerger merger = new LineMerger(declaration);
		merger.DoMerge();

		var entryHeader = declaration.CustomsEntryHeaders.Single();
		AssertNotNull("entryHeader must not be null", entryHeader);

		CombineAssertions("Expected invoice lines order", () =>
		{
			AssertEquals("For EntryLine 1", "Invoice1 - Line2", entryHeader.MergedLines[0].InvoiceLines[0].JI_Description);
			AssertEquals("For EntryLine 2", "Invoice1 - Line3", entryHeader.MergedLines[1].InvoiceLines[0].JI_Description);
			AssertEquals("For EntryLine 3", "Invoice1 - Line1", entryHeader.MergedLines[2].InvoiceLines[0].JI_Description);
			AssertEquals("For EntryLine 4", "Invoice1 - Line5", entryHeader.MergedLines[3].InvoiceLines[0].JI_Description);
			AssertEquals("For EntryLine 5", "Invoice1 - Line4", entryHeader.MergedLines[4].InvoiceLines[0].JI_Description);
			AssertEquals("For EntryLine 6", "Invoice2 - Line3", entryHeader.MergedLines[5].InvoiceLines[0].JI_Description);
			AssertEquals("For EntryLine 7", "Invoice2 - Line4", entryHeader.MergedLines[6].InvoiceLines[0].JI_Description);
			AssertEquals("For EntryLine 8", "Invoice2 - Line1", entryHeader.MergedLines[7].InvoiceLines[0].JI_Description);
			AssertEquals("For EntryLine 9", "Invoice2 - Line2", entryHeader.MergedLines[8].InvoiceLines[0].JI_Description);
		});
	}

	protected override void CustomizeSupportingDocumentForLocalCountry(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument supportingDocument)
	{
		var supDocument = (SupportingDocument)supportingDocument;

		supDocument.CSI_YearOfIssue = ZDate.Today.Year.ToString();
	}

	protected override Type[] ExpectedEntryCreationStrategiesType => new[] { typeof(EntryCreationStrategy) };

	protected override Customs.Business.LineMerger GetNewLineMerger(BaseJobDeclaration declaration) => new LineMerger((JobDeclaration)declaration);

	protected override Type GetSupportingDocumentType() => typeof(SupportingDocument);

	protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

	protected override Type ExpectedDutyCalculatorStrategyType => typeof(DutyCalculatorStrategy);

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		declaration.Packages.RemoveAndDeleteAll();

		var declarationBill = declaration.Bills.AddNew();
		var billPackingGroup = declarationBill.PackingGroups.AddNew();
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

		packageDM = declaration.Packages.AddNew();
		packageDM.CW_CR_HouseContainer = billPackingGroup.PK;
		packageDM.CW_PackQty = 1;
		packageDM.CW_PackType = "DM";
		packageDM.CW_MarksAndNos = "IND";

		packageA2 = declaration.Packages.AddNew();
		packageA2.CW_CR_HouseContainer = billPackingGroup.PK;
		packageA2.CW_PackQty = 2;
		packageA2.CW_PackType = "A2";
		packageA2.CW_MarksAndNos = "IND";

		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;

	BasePackage packageA2;
	BasePackage packageDM;
	BasePackage packagePK;
	BasePackage packageCT;

	class LineMergerForTest : LineMerger
	{
		public LineMergerForTest(EU.Business.Declaration.JobDeclaration declaration)
			: base(declaration)
		{
		}

		public ILineNumberAssigner GetLineNumberAssigner_Exposed(Customs.Business.CusEntryHeader entryHeader) => base.GetLineNumberAssigner(entryHeader);
	}
}
