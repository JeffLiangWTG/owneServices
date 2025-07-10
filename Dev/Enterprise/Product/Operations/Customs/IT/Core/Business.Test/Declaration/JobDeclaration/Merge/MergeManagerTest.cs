using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class MergeManagerTest : EU.Business.Declaration.Testing.MergeManagerTest
{
	public void TestPreviousDocumentPackageQuantityAffectsRequiresMerge()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_ApplicationCode = "BLT";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		var previousDocument = declaration.PreviousDocuments.AddNew();
		previousDocument.CSI_PackQty = 100;
		declaration.DoMerge();
		Factory.Save();
		Assert("PRE-Condition", !declaration.MergeManager.RequiresMerge);
		previousDocument.CSI_PackQty = 200;
		Assert(declaration.MergeManager.RequiresMerge);
	}

	public void TestMergedLinesCustomSortOrdersByLineNumbersNoGap()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_ApplicationCode = "BLT";
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_Tariff = "2";
		invoiceLine1.JI_LineNo = 2;

		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_Tariff = "1";
		invoiceLine2.JI_LineNo = 1;

		var invoiceLine3 = invoice.InvoiceLines.AddNew();
		invoiceLine3.JI_Tariff = "4";
		invoiceLine3.JI_LineNo = 4;

		declaration.DoMerge();
		Factory.Save();
		AssertEquals("CustomsEntryHeaders Count", 1, declaration.CustomsEntryHeaders.Count);
		var entryHeader = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().Single();
		var mergedLines = entryHeader.MergedLines.Cast<CusEntryLine>();

		AssertEquals("MergedLines Count", 3, entryHeader.MergedLines.Count);
		CombineAssertions("Assert MergedLines are ordered by LineNumber and with no gap in the numeration", () =>
		 {
			 var mergedLine1 = mergedLines.ElementAt(0);
			 AssertEquals("[Entry Line at position: 0] CL_LineNumber", (ZShort)1, mergedLine1.CL_LineNumber);
			 AssertEquals("[Entry Line at position: 0] Tariff", "1", mergedLine1.Tariff);

			 var mergedLine2 = mergedLines.ElementAt(1);
			 AssertEquals("[Entry Line at position: 1] CL_LineNumber", (ZShort)2, mergedLine2.CL_LineNumber);
			 AssertEquals("[Entry Line at position: 1] Tariff", "2", mergedLine2.Tariff);

			 var mergedLine3 = mergedLines.ElementAt(2);
			 AssertEquals("[Entry Line at position: 2] CL_LineNumber", (ZShort)3, mergedLine3.CL_LineNumber);
			 AssertEquals("[Entry Line at position: 2] Tariff", "4", mergedLine3.Tariff);
		 });

		invoiceLine2.Delete();

		declaration.DoMerge();
		Factory.Save();

		mergedLines = entryHeader.MergedLines.Cast<CusEntryLine>();
		AssertEquals("MergedLines Count", 2, entryHeader.MergedLines.Count);
		CombineAssertions("Assert MergedLines are ordered by LineNumber and with no gap in the numeration", () =>
		 {
			 var mergedLine1 = mergedLines.ElementAt(0);
			 AssertEquals("[Entry Line at position: 0] CL_LineNumber", (ZShort)1, mergedLine1.CL_LineNumber);
			 AssertEquals("[Entry Line at position: 0] Tariff", "2", mergedLine1.Tariff);

			 var mergedLine2 = mergedLines.ElementAt(1);
			 AssertEquals("[Entry Line at position: 1] CL_LineNumber", (ZShort)2, mergedLine2.CL_LineNumber);
			 AssertEquals("[Entry Line at position: 1] Tariff", "4", mergedLine2.Tariff);
		 });
	}

	[ExpectNoExceptions]
	public void TestEntryHeaderCheckLinesCountFiredOnMerged()
	{
		var declaration = Factory.New<JobDeclaration>();
		var mockEntryHeader = Factory.NewMoq<CusEntryHeader>();
		declaration.CustomsEntryHeaders.Add(mockEntryHeader.Object);
		var mockEntryHeaderValidation = new Mock<CusEntryHeaderValidation>(mockEntryHeader.Object);
		mockEntryHeader.Protected().Setup<Customs.Business.CusEntryHeaderValidation>("GetNewValidation").Returns(mockEntryHeaderValidation.Object);
		mockEntryHeaderValidation.Protected().Setup("CheckLinesCountCore");
		var mergeManager = new MergeManagerForTest(declaration);
		mergeManager.OnMergedExposed();
		mockEntryHeaderValidation.VerifyAll();
	}

	public void TestApplyEntryLineDefaultLogicForC100SupportingDocumentsFiredOnMerged()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";

		var supplierWithRex = Factory.New<OrgHeader>();
		var rexNumber = supplierWithRex.CustomsCodes.AddNew();
		rexNumber.OK_CodeType = "REX";
		rexNumber.OK_CustomsRegNo = "IEREX12345AB";
		declaration.JE_OH_Supplier = supplierWithRex.PK;

		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_PrimaryPreference = "100";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_PrimaryPreference = "220";

		declaration.DoMerge();

		AssertEquals("[PRE-CONDITION] An Entry Header is expected", 1, declaration.CustomsEntryHeaders.Count);
		var entryHeader = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().Single();
		AssertEquals("[PRE-CONDITION] An Entry Line is expected", 1, entryHeader.MergedLines.Count);
		var entryLine = entryHeader.MergedLines.Cast<CusEntryLine>().Single();

		var supportingDocuments = entryLine.SupportingDocuments.Cast<SupportingDocument>();
		AssertEquals("C100 Supporting Documents is expected", true, supportingDocuments.Any(x => x.CSI_Code == "C100"));
		var supC100 = supportingDocuments.SingleOrDefault(x => x.CSI_Code == "C100");

		CombineAssertions("Check Sup C100 properties", () =>
		{
			AssertEquals("CSI_Code", "C100", supC100.CSI_Code);
			AssertEquals("CSI_ReferenceNumber", "IEREX12345AB", supC100.CSI_ReferenceNumber);
		});
	}

	public void TestEntryLineFeesSortFiredOnMerged()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_ApplicationCode = "BLT";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		foreach (var rateCode in new[] { "407", "406", "405", "927", "911", "201", "165", "116", "A35", "A30", "A20", "A10", "A00" })
		{
			entryLine.Fees.AddOrUpdate(rateCode, 0m);
		}

		declaration.DoMerge();

		AssertArrayEqualsByElements("Fees ordered after merge", new ZString[] { "A00", "A10", "A20", "A30", "A35", "116", "165", "201", "911", "927", "405", "406", "407" }, entryLine.Fees.Cast<CusEntryLineFee>().Select(x => x.CF_ChargeType).ToArray());
	}

	public void TestBeforeApplyingEntryLineDefaultLogicForC100SupportingDocumentsFiredOnMerged()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var supplierWithRex = Factory.New<OrgHeader>();
		supplierWithRex.CustomsCodes.AddNew("REX", "REX1234");
		declaration.JE_OH_Supplier = supplierWithRex.PK;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_PrimaryPreference = "200";
		invoiceLine.JI_CL = entryLine.PK;

		var mergeManager = new MergeManagerForTest(declaration);

		entryHeader.MergedLines[0].InvoiceLines.Load();

		var beforeApplyingEntryLineDefaultLogicForC100SupportingDocumentsHasBeenFired = false;
		mergeManager.BeforeApplyingEntryLineDefaultLogicForC100SupportingDocuments += (s, e) => beforeApplyingEntryLineDefaultLogicForC100SupportingDocumentsHasBeenFired = true;
		mergeManager.OnMergedExposed();

		AssertEquals("Has BeforeApplyingEntryLineDefaultLogicForC100SupportingDocuments been fired?", true, beforeApplyingEntryLineDefaultLogicForC100SupportingDocumentsHasBeenFired);
	}

	protected override Type GetLineMergerType() => typeof(LineMerger);

	protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();
}

class MergeManagerForTest : MergeManager
{
	public MergeManagerForTest(EU.Business.Declaration.JobDeclaration jobDec) : base(jobDec)
	{
	}

	public void OnMergedExposed() => OnMerged();
}

sealed class EntryLineDefaultLogicForC100SupportingDocumentsEventArgsTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Should be exception when param entryLinesWhereDefaultLogicForSupportingDocumentC100IsApplicable is null", () => new EntryLineDefaultLogicForC100SupportingDocumentsEventArgs(null));

		var entryLineCollection = new CusEntryLine[] { Factory.New<CusEntryLine>() };
		EntryLineDefaultLogicForC100SupportingDocumentsEventArgs entryLinesWhereDefaultLogicForSupportingDocumentC100IsApplicable = null;
		AssertNoExceptionThrown("No exception expected", () => entryLinesWhereDefaultLogicForSupportingDocumentC100IsApplicable = new EntryLineDefaultLogicForC100SupportingDocumentsEventArgs(entryLineCollection));

		AssertEquals("EntryLinesWhereDefaultLogicForSupportingDocumentC100IsApplicable", entryLineCollection, entryLinesWhereDefaultLogicForSupportingDocumentC100IsApplicable.EntryLinesWhereDefaultLogicForSupportingDocumentC100IsApplicable);
	}
}
