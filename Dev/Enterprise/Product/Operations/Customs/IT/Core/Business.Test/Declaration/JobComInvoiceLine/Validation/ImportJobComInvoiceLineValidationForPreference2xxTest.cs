using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class ImportJobComInvoiceLineValidationForPreference2xxTest : BusinessObjectValidationTestCase
{
	public void TestRequiredDocumentsValidationForC100OrU166SupportingDocuments()
	{
		var expectedMessageError = "Preference 200 requires a Supporting Document of type 'C100' or 'U166'. Consider adding the REX code to the Supplier>Config>Registration Numbers in order to add automatically C100 document.";

		invoiceLine.Validation.ValidateAll();
		AssertHasRowMessageError("Message error is expected when related entry line does not have C100 or U166 Supporting Documents", invoiceLine, expectedMessageError);

		declaration.JE_MessageType = "EXP";
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("No message error when Declaration is not IMP", invoiceLine, expectedMessageError);

		var c100Sup = invoiceLine.SupportingDocuments.AddNew();
		c100Sup.CSI_Code = "C100";
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("No message error when related entry line has C100 supporting document", invoiceLine, expectedMessageError);

		invoiceLine.SupportingDocuments.RemoveAndDeleteAll();
		var u166Sup = invoice.SupportingDocuments.AddNew();
		u166Sup.CSI_Code = "U166";
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("No message error when related entry line has U166 supporting document", invoiceLine, expectedMessageError);

		invoice.SupportingDocuments.RemoveAndDeleteAll();
		declaration.JE_MessageType = "IMP";
		AssertRequiredDocumentsValidationRunWhenMergeIsDone(expectedMessageError);
		AssertRequiredDocumentsValidationRunWhenPrimaryPreferenceStartsWith2xx(expectedMessageError);
	}

	public void TestRequiredDocumentsValidationForC100OrU166SupportingDocumentsRunOnlyOnFirstInvoiceLine()
	{
		var expectedMessageError = "Preference 200 requires a Supporting Document of type 'C100' or 'U166'. Consider adding the REX code to the Supplier>Config>Registration Numbers in order to add automatically C100 document.";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";

		var invoiceB = declaration.Invoices.AddNew();
		invoiceB.JZ_InvoiceNumber = "B";
		var invoiceLineB1 = invoiceB.InvoiceLines.AddNew();
		invoiceLineB1.JI_LineNo = 1;
		invoiceLineB1.JI_PrimaryPreference = "200";

		var invoiceA = declaration.Invoices.AddNew();
		invoiceA.JZ_InvoiceNumber = "A";
		var invoiceLineA2 = invoiceA.InvoiceLines.AddNew();
		invoiceLineA2.JI_LineNo = 2;
		invoiceLineA2.JI_PrimaryPreference = "200";
		var invoiceLineA1 = invoiceA.InvoiceLines.AddNew();
		invoiceLineA1.JI_LineNo = 1;
		invoiceLineA1.JI_PrimaryPreference = "200";

		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader1.MergedLines.AddNew();
		invoiceLineB1.JI_CL = entryLine1.PK;
		invoiceLineA2.JI_CL = entryLine1.PK;
		invoiceLineA1.JI_CL = entryLine1.PK;

		invoiceLineA1.Validation.ValidateAll();
		invoiceLineA2.Validation.ValidateAll();
		invoiceLineB1.Validation.ValidateAll();

		CombineAssertions("Assert validation should be execute only for the first Invoice Line", () =>
		{
			AssertHasRowMessageError("Message error is expected only in the first invoice line of the Entry", invoiceLineA1, expectedMessageError);
			AssertNoRowMessageError("No message error expected", invoiceLineA2, expectedMessageError);
			AssertNoRowMessageError("No message error expected", invoiceLineB1, expectedMessageError);
		});
	}

	public void TestRequiredDocumentsValidationWhenC100andU166AreBothEntered()
	{
		var expectedMessageError = "U166 and C100 cannot be used together: use U166 if the supplier is not a registered one and the total value of the imported goods is less than 6000€";

		var c100Sup = invoiceLine.SupportingDocuments.AddNew();
		c100Sup.CSI_Code = "C100";
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("No message error when related entry line has C100 supporting document", invoiceLine, expectedMessageError);

		var u166Sup = invoice.SupportingDocuments.AddNew();
		u166Sup.CSI_Code = "U166";
		invoiceLine.Validation.ValidateAll();
		AssertHasRowMessageError("Message error is expected when related entry line has both C100 and U166 Supporting Documents", invoiceLine, expectedMessageError);

		AssertRequiredDocumentsValidationRunWhenMergeIsDone(expectedMessageError);
		AssertRequiredDocumentsValidationRunWhenPrimaryPreferenceStartsWith2xx(expectedMessageError);
	}

	public void TestRequiredDocumentsValidationWhenU164andU165AreBothEntered()
	{
		var expectedMessageError = "One and only one Certificate of Origin is required: use document U164 when Customs value is less than 6000€, U165 otherwise.";

		var u164Sup = invoiceLine.SupportingDocuments.AddNew();
		u164Sup.CSI_Code = "U164";
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("No message error when related entry line has U164 supporting document", invoiceLine, expectedMessageError);

		var u165Sup = invoice.SupportingDocuments.AddNew();
		u165Sup.CSI_Code = "U165";
		invoiceLine.Validation.ValidateAll();
		AssertHasRowMessageError("Message error is expected when related entry line has both U164 and U165 Supporting Documents", invoiceLine, expectedMessageError);
	}

	public void TestRequiredDocumentsValidationWhenVfdLessThan6000AndU164orU165AreNotEntered()
	{
		var expectedMessageError = "Preference 200 requires a Supporting Document of type U164 when Customs value is less than 6000€.";

		entryLine.CL_CustomsValue = 5000m;

		invoiceLine.Validation.ValidateAll();
		AssertHasRowMessageError("Message error is expected when related entry line has Customs Value < 6000€ and neither U164 or U165 Supporting Documents have been entered", invoiceLine, expectedMessageError);

		var u164Sup = invoiceLine.SupportingDocuments.AddNew();
		u164Sup.CSI_Code = "U164";
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("No message error when related entry line with Customs Value < 6000€ and has U164 supporting document", invoiceLine, expectedMessageError);

		invoiceLine.SupportingDocuments.RemoveAndDeleteAll();
		var u165Sup = invoice.SupportingDocuments.AddNew();
		u165Sup.CSI_Code = "U165";
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("No message error when related entry line with Customs Value < 6000€ and has U165 supporting document", invoiceLine, expectedMessageError);
	}

	public void TestRequiredDocumentsValidationWhenCustomsValueIsGreaterOrEqualThan6000AndU164orU165AreNotEntered()
	{
		var expectedMessageError = "Preference 200 requires a Supporting Document of type U165 when Customs value exceeds 6000€.";

		entryLine.CL_CustomsValue = 6000m;
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("No message error when related entry line with Customs Value = 6000€ and has U164 supporting document", invoiceLine, expectedMessageError);

		var u164Sup = invoiceLine.SupportingDocuments.AddNew();
		u164Sup.CSI_Code = "U164";
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("No message error when related entry line with Customs Value > 6000€ and has U164 supporting document", invoiceLine, expectedMessageError);

		invoiceLine.SupportingDocuments.RemoveAndDeleteAll();
		var u165Sup = invoice.SupportingDocuments.AddNew();
		u165Sup.CSI_Code = "U165";
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("No message error when related entry line with Customs Value < 6000€ and has U165 supporting document", invoiceLine, expectedMessageError);
	}

	public void TestRequiredU164U165DocumentsValidationRunOnlyForCorrectInvoiceLines()
	{
		var expectedU164MessageError = "Preference 200 requires a Supporting Document of type U164 when Customs value is less than 6000€.";
		var expectedU165MessageError = "Preference 200 requires a Supporting Document of type U165 when Customs value exceeds 6000€.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";

		var invoiceB = declaration.Invoices.AddNew();
		invoiceB.JZ_InvoiceNumber = "B";
		invoiceB.JZ_RX_NKInvoice_Currency = "EUR";
		var invoiceLineB1 = invoiceB.InvoiceLines.AddNew();
		invoiceLineB1.JI_LineNo = 1;
		invoiceLineB1.JI_PrimaryPreference = "200";

		var invoiceA = declaration.Invoices.AddNew();
		invoiceA.JZ_InvoiceNumber = "A";
		invoiceA.JZ_RX_NKInvoice_Currency = "EUR";
		var invoiceLineA2 = invoiceA.InvoiceLines.AddNew();
		invoiceLineA2.JI_LineNo = 2;
		invoiceLineA2.JI_PrimaryPreference = "200";
		var invoiceLineA1 = invoiceA.InvoiceLines.AddNew();
		invoiceLineA1.JI_LineNo = 1;
		invoiceLineA1.JI_PrimaryPreference = "200";

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLineA1.JI_CL = entryLine.PK;
		invoiceLineA2.JI_CL = entryLine.PK;
		invoiceLineB1.JI_CL = entryLine.PK;

		entryLine.CL_CustomsValue = 5000m;

		invoiceLineA1.Validation.ValidateAll();
		invoiceLineA2.Validation.ValidateAll();
		invoiceLineB1.Validation.ValidateAll();

		AssertHasRowMessageError("Message error is expected when related entry line has VFD < 6000€ and neither U164 or U165 Supporting Documents have been entered", invoiceLineA1, expectedU164MessageError);
		AssertNoRowMessageError("No message error when related entry line with VFD < 6000€ has U164 supporting document", invoiceLineA2, expectedU164MessageError);
		AssertNoRowMessageError("No message error when related entry line with VFD < 6000€ has U164 supporting document", invoiceLineB1, expectedU164MessageError);

		entryLine.CL_CustomsValue = 8000m;

		invoiceLineA1.Validation.ValidateAll();
		invoiceLineA2.Validation.ValidateAll();
		invoiceLineB1.Validation.ValidateAll();

		AssertHasRowMessageError("Message error is expected when related entry line has VFD > 6000€ and neither U164 or U165 Supporting Documents have been entered", invoiceLineA1, expectedU165MessageError);
		AssertHasRowMessageError("Message error is expected when related entry line has VFD > 6000€ and neither U164 or U165 Supporting Documents have been entered", invoiceLineA2, expectedU165MessageError);
		AssertHasRowMessageError("Message error is expected when related entry line has VFD > 6000€ and neither U164 or U165 Supporting Documents have been entered", invoiceLineB1, expectedU165MessageError);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = "EUR";
		invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_PrimaryPreference = "200";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryLine = entryHeader.MergedLines.AddNew();

		invoiceLine.JI_CL = entryLine.PK;
	}
	JobDeclaration declaration;
	JobComInvoiceHeader invoice;
	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;

	void AssertRequiredDocumentsValidationRunWhenMergeIsDone(string expectedMessageError)
	{
		invoiceLine.JI_PrimaryPreference = "200";

		invoiceLine.JI_CL = ZGuid.Empty;
		entryLine.InvoiceLines.Reload(true);
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("No message error when Merge is not done", invoiceLine, expectedMessageError);

		invoiceLine.JI_CL = entryLine.PK;
		entryLine.InvoiceLines.Reload(true);
		invoiceLine.Validation.ValidateAll();
		AssertHasRowMessageError("Message error expected when merge is done", invoiceLine, expectedMessageError);
	}

	void AssertRequiredDocumentsValidationRunWhenPrimaryPreferenceStartsWith2xx(string expectedMessageError)
	{
		invoiceLine.JI_PrimaryPreference = "400";
		invoiceLine.Validation.ValidateAll();

		AssertNoRowMessageError("No message error when Merge is not done", invoiceLine, expectedMessageError);

		invoiceLine.JI_PrimaryPreference = "200";
		invoiceLine.Validation.ValidateAll();

		AssertHasRowMessageError("Message error expected when merge is done", invoiceLine, expectedMessageError);
	}
}
