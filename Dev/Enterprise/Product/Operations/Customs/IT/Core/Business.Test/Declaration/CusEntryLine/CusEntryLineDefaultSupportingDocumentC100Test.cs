using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusEntryLineDefaultSupportingDocumentC100Test : TestCaseWithFactory
{
	public void TestIsDefaultLogicForSupportingDocumentC100Applicable()
	{
		invoiceLine1A.JI_PrimaryPreference = "200";
		invoiceLine2A.JI_PrimaryPreference = "200";
		invoiceLine1B.JI_PrimaryPreference = "200";

		declaration.JE_OH_Supplier = supplierWithRex.PK;

		(var entryLine, var entryHeader) = GetEntry();

		declaration.JE_MessageType = "";
		AssertEquals("When JE_MessageType is empty, IsDefaultLogicForSupportingDocumentC100Applicable", false, entryLine.IsDefaultLogicForSupportingDocumentC100Applicable);

		declaration.JE_MessageType = "IMP";
		AssertEquals("IsDefaultLogicForSupportingDocumentC100Applicable", true, entryLine.IsDefaultLogicForSupportingDocumentC100Applicable);

		declaration.JE_MessageType = "EXP";
		AssertEquals("When JE_MessageType is EXP, IsDefaultLogicForSupportingDocumentC100Applicable", false, entryLine.IsDefaultLogicForSupportingDocumentC100Applicable);

		invoiceLine1A.JI_PrimaryPreference = "100";
		AssertEquals("When Preference does not start with 2, IsDefaultLogicForSupportingDocumentC100Applicable", false, entryLine.IsDefaultLogicForSupportingDocumentC100Applicable);

		invoiceLine1A.JI_PrimaryPreference = "200";
		entryHeader.CH_Status = "ACO";
		entryHeader.CH_EntryStatus = "REG";
		AssertEquals("When parent Entry Header is a final status, IsDefaultLogicForSupportingDocumentC100Applicable", false, entryLine.IsDefaultLogicForSupportingDocumentC100Applicable);

		entryHeader.CH_Status = "";
		entryHeader.CH_EntryStatus = "";
		declaration.JE_OH_Supplier = ZGuid.Empty;
		AssertEquals("When Supplier is empty, IsDefaultLogicForSupportingDocumentC100Applicable", false, entryLine.IsDefaultLogicForSupportingDocumentC100Applicable);

		var supplierWithNoRex = Factory.New<OrgHeader>();
		declaration.JE_OH_Supplier = supplierWithNoRex.PK;
		AssertEquals("When Supplier has not the REX number, IsDefaultLogicForSupportingDocumentC100Applicable", false, entryLine.IsDefaultLogicForSupportingDocumentC100Applicable);
	}

	public void TestApplyDefaultLogicForSupportingDocumentsC100IfApplicableWhenAllConditionMatches()
	{
		invoiceLine1A.JI_PrimaryPreference = "200";
		invoiceLine2A.JI_PrimaryPreference = "200";
		invoiceLine1B.JI_PrimaryPreference = "200";
		declaration.JE_OH_Supplier = supplierWithRex.PK;
		declaration.ShouldOverrideC100SupportingDocumentRexCode = false;

		(var entryLine, var entryHeader) = GetEntry();

		AssertPreConditionEntryLineNotContainsSupC100(entryLine);

		entryLine.ApplyDefaultLogicForSupportingDocumentsC100IfApplicable();

		AssertEquals("Supporting Document C100 Supporting Document is expected", 1, CountC100SupportingDocuments(entryLine.SupportingDocuments));
		AssertEquals("Supporting Document in first invoice line of first invoice (ordered by InvoiceNumber and InvoiceLine) is expected", 1, invoiceLine1A.SupportingDocuments.Cast<SupportingDocument>().Count(x => x.CSI_Code == SupportingDocumentCodeC100));

		var supC100 = entryLine.SupportingDocuments.Single();
		CombineAssertions("Check Supporting Document C100 properties", () =>
		{
			AssertEquals("Code", "C100", supC100.CSI_Code);
			AssertEquals("Reference", "IEREX12345AB", supC100.CSI_ReferenceNumber);
		});
	}

	public void TestApplyDefaultLogicForSupportingDocumentsC100IfApplicableWhenSupplierHasNotREX()
	{
		var supplierWithNoRex = Factory.New<OrgHeader>();

		declaration.JE_OH_Supplier = supplierWithNoRex.PK;
		invoiceLine1A.JI_PrimaryPreference = "200";

		(var entryLine, var _) = GetEntry();

		AssertPreConditionEntryLineNotContainsSupC100(entryLine);

		entryLine.ApplyDefaultLogicForSupportingDocumentsC100IfApplicable();

		AssertNotContainsC100SupportingDocument(entryLine);
	}

	public void TestApplyDefaultLogicForSupportingDocumentsC100IfApplicableWhenDeclarationHasNoSupplier()
	{
		declaration.JE_OH_Supplier = ZGuid.Empty;
		invoiceLine1A.JI_PrimaryPreference = "200";

		(var entryLine, var _) = GetEntry();

		AssertPreConditionEntryLineNotContainsSupC100(entryLine);

		entryLine.ApplyDefaultLogicForSupportingDocumentsC100IfApplicable();

		AssertNotContainsC100SupportingDocument(entryLine);
	}

	public void TestApplyDefaultLogicForSupportingDocumentsC100IfApplicableWhenEntryLineHasNoDeclaration()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		var entryLine = entryHeader.AllEntryLines.AddNew();

		entryLine.ApplyDefaultLogicForSupportingDocumentsC100IfApplicable();

		AssertEquals("No Supporting Document C100 in first invoice line of first invoice (ordered by InvoiceNumber and InvoiceLine) is expected", false, HasAnyC100SupportingDocuments(invoiceLine1A.SupportingDocuments));
	}

	public void TestApplyDefaultLogicForSupportingDocumentsC100IfApplicableWhenPreferenceDoesNotStartWith2()
	{
		declaration.JE_OH_Supplier = supplierWithRex.PK;
		invoiceLine1A.JI_PrimaryPreference = "100";

		(var entryLine, var _) = GetEntry();
		AssertPreConditionEntryLineNotContainsSupC100(entryLine);

		entryLine.ApplyDefaultLogicForSupportingDocumentsC100IfApplicable();

		AssertNotContainsC100SupportingDocument(entryLine);
	}

	public void TestApplyDefaultLogicForSupportingDocumentsC100IfApplicableWhenEntryIsInFinalStatus()
	{
		invoiceLine1A.JI_PrimaryPreference = "200";
		declaration.JE_OH_Supplier = supplierWithRex.PK;

		(var entryLine, var entryHeader) = GetEntry();
		entryHeader.CH_Status = "ACO";
		entryHeader.CH_EntryStatus = "REG";

		AssertPreConditionEntryLineNotContainsSupC100(entryLine);

		entryLine.ApplyDefaultLogicForSupportingDocumentsC100IfApplicable();

		AssertNotContainsC100SupportingDocument(entryLine);
	}

	public void TestApplyDefaultLogicForSupportingDocumentsC100IfApplicableAddDocumentToFirstInvoiveLine()
	{
		invoiceLine1A.JI_PrimaryPreference = "200";
		invoiceLine2A.JI_PrimaryPreference = "200";
		invoiceLine1B.JI_PrimaryPreference = "200";

		declaration.JE_OH_Supplier = supplierWithRex.PK;

		(var entryLine, var entryHeader) = GetEntry();

		AssertPreConditionEntryLineNotContainsSupC100(entryLine);

		entryLine.ApplyDefaultLogicForSupportingDocumentsC100IfApplicable();

		CombineAssertions("Assert SUP C100 has been inserted only in the first invoice line of first invoice", () =>
		{
			AssertEquals("SUP C100 is expected in Invoice Line 1A", 1, CountC100SupportingDocuments(invoiceLine1A.SupportingDocuments));
			AssertEquals("SUP C100 is not expected in Invoice Line 2A", 0, CountC100SupportingDocuments(invoiceLine2A.SupportingDocuments));
			AssertEquals("SUP C100 is not expected in Invoice Line 1B", 0, CountC100SupportingDocuments(invoiceLine1B.SupportingDocuments));
		});
	}

	public void TestApplyDefaultLogicForSupportingDocumentsC100IfApplicableDoesNotDuplicateC100Sup()
	{
		invoiceLine1A.JI_PrimaryPreference = "200";
		declaration.JE_OH_Supplier = supplierWithRex.PK;

		var supC100WithDiffRex = invoiceLine1A.SupportingDocuments.AddNew();
		supC100WithDiffRex.CSI_Code = "C100";
		supC100WithDiffRex.CSI_ReferenceNumber = "IEREX12345AB";

		(var entryLine, var _) = GetEntry();

		entryLine.ApplyDefaultLogicForSupportingDocumentsC100IfApplicable();

		AssertEquals("Only 1 C100 Sup is expected", 1, CountC100SupportingDocuments(entryLine.SupportingDocuments));
		AssertEquals("Only 1 SUP C100 is expected in Invoice Line 1A", 1, CountC100SupportingDocuments(invoiceLine1A.SupportingDocuments));

		invoiceLine1A.SupportingDocuments.RemoveAndDeleteAll();

		var invoice = invoiceLine1A.InvoiceHeader;
		supC100WithDiffRex = invoice.SupportingDocuments.AddNew();
		supC100WithDiffRex.CSI_Code = "C100";
		supC100WithDiffRex.CSI_ReferenceNumber = "IEREX12345AB";

		entryLine.ApplyDefaultLogicForSupportingDocumentsC100IfApplicable();

		CombineAssertions("Check C100 sup has not been duplicated", () =>
		 {
			 AssertEquals("Only 1 C100 Sup is expected", 1, CountC100SupportingDocuments(entryLine.SupportingDocuments));
			 AssertEquals("0 SUP C100 is expected in Invoice Line 1A because it is already entered in Declaration", 0, CountC100SupportingDocuments(invoiceLine1A.SupportingDocuments));
			 AssertEquals("Only 1 SUP C100 is expected in Declaration", 1, CountC100SupportingDocuments(invoice.SupportingDocuments));
		 });
	}

	public void TestApplyDefaultLogicForSupportingDocumentsC100IfApplicableWhenEntryHasSupC100WithDifferentRexNumber()
	{
		invoiceLine1A.JI_PrimaryPreference = "200";
		invoiceLine2A.JI_PrimaryPreference = "200";
		invoiceLine1B.JI_PrimaryPreference = "200";

		declaration.JE_OH_Supplier = supplierWithRex.PK;

		var supC100WithDiffRex = invoiceLine1A.SupportingDocuments.AddNew();
		supC100WithDiffRex.CSI_Code = "C100";
		supC100WithDiffRex.CSI_ReferenceNumber = "IEREXAAAAAA";
		declaration.ShouldOverrideC100SupportingDocumentRexCode = false;

		(var entryLine, var entryHeader) = GetEntry();

		entryLine.ApplyDefaultLogicForSupportingDocumentsC100IfApplicable();

		AssertEquals("Supporting Document C100 Supporting Document is expected", 1, CountC100SupportingDocuments(entryLine.SupportingDocuments));

		var supC100 = entryLine.SupportingDocuments.Single();
		CombineAssertions("When ShouldOverrideC100SupportingDocumentRexNumber is false, Check Supporting Document C100 properties", () =>
		{
			AssertEquals("Code", "C100", supC100.CSI_Code);
			AssertEquals("Reference", "IEREXAAAAAA", supC100.CSI_ReferenceNumber);
		});

		declaration.ShouldOverrideC100SupportingDocumentRexCode = true;

		entryLine.ApplyDefaultLogicForSupportingDocumentsC100IfApplicable();

		AssertEquals("Supporting Document C100 Supporting Document is expected", 1, CountC100SupportingDocuments(entryLine.SupportingDocuments));

		supC100 = entryLine.SupportingDocuments.Single();
		CombineAssertions("When ShouldOverrideC100SupportingDocumentRexNumber is true, Check Supporting Document C100 properties", () =>
		{
			AssertEquals("Code", "C100", supC100.CSI_Code);
			AssertEquals("Reference", "IEREX12345AB", supC100.CSI_ReferenceNumber);
			AssertEquals("ShouldOverrideC100SupportingDocumentRexCode should be reset", false, declaration.ShouldOverrideC100SupportingDocumentRexCode);
		});
	}

	void AssertPreConditionEntryLineNotContainsSupC100(CusEntryLine entryLine) => AssertEquals("[PRE-CONDITION] No C100 Supporting Document", false, HasAnyC100SupportingDocuments(entryLine.SupportingDocuments));

	void AssertNotContainsC100SupportingDocument(CusEntryLine entryLine)
	{
		CombineAssertions("Assert No Supporting Document C100 has been added", () =>
		{
			AssertEquals("No C100 Supporting Document expected", false, HasAnyC100SupportingDocuments(entryLine.SupportingDocuments));
			AssertEquals("No Supporting Document C100 in first invoice line of first invoice (ordered by InvoiceNumber and InvoiceLine) is expected", false, HasAnyC100SupportingDocuments(invoiceLine1A.SupportingDocuments));
		});
	}

	bool HasAnyC100SupportingDocuments(IEnumerable<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument> supportingDocuments) => supportingDocuments.Any(x => x.CSI_Code == SupportingDocumentCodeC100);
	bool HasAnyC100SupportingDocuments(SupportingDocumentCollection supportingDocuments) => HasAnyC100SupportingDocuments(supportingDocuments.Cast<SupportingDocument>());

	int CountC100SupportingDocuments(IEnumerable<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument> supportingDocuments) => supportingDocuments.Count(x => x.CSI_Code == SupportingDocumentCodeC100);
	int CountC100SupportingDocuments(SupportingDocumentCollection supportingDocuments) => CountC100SupportingDocuments(supportingDocuments.Cast<SupportingDocument>());

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		supplierWithRex = Factory.New<OrgHeader>();
		var rexNumber = supplierWithRex.CustomsCodes.AddNew();
		rexNumber.OK_CodeType = "REX";
		rexNumber.OK_CustomsRegNo = "IEREX12345AB";

		var invoiceA = declaration.Invoices.AddNew();
		invoiceA.JZ_InvoiceNumber = "A";
		invoiceLine2A = invoiceA.InvoiceLines.AddNew();
		invoiceLine2A.JI_LineNo = 2;
		invoiceLine1A = invoiceA.InvoiceLines.AddNew();
		invoiceLine1A.JI_LineNo = 1;
		var invoiceB = declaration.Invoices.AddNew();
		invoiceB.JZ_InvoiceNumber = "B";
		invoiceLine1B = invoiceB.InvoiceLines.AddNew();
		invoiceLine1B.JI_LineNo = 1;
	}

	(CusEntryLine entryLine, CusEntryHeader entryHeader) GetEntry()
	{
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();

		invoiceLine1A.JI_CL = entryLine.PK;
		invoiceLine2A.JI_CL = entryLine.PK;
		invoiceLine1B.JI_CL = entryLine.PK;

		return (entryLine, entryHeader);
	}

	JobDeclaration declaration;
	OrgHeader supplierWithRex;
	JobComInvoiceLine invoiceLine1A;
	JobComInvoiceLine invoiceLine2A;
	JobComInvoiceLine invoiceLine1B;

	const string SupportingDocumentCodeC100 = "C100";
}
