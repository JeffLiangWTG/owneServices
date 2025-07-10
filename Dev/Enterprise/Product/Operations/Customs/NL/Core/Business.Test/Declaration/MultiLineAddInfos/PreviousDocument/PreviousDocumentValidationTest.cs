using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

sealed class PreviousDocumentValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckRule_UC0849()
	{
		var errorMessage = "[C0849] If the 'Type' is C651, C658 or N705, then 'Line No.' is required.";

		var previousDocument_InvoiceHeader = invoiceHeader.PreviousDocuments.AddNew();
		previousDocument_InvoiceHeader.CSI_Code = "C651";
		AssertNoRowMessageError("PreviousDocument under InvoiceHeader", preDoc, errorMessage);

		preDoc.CSI_Code = "C651";
		AssertHasRowMessageError("CSI_Code is C651", preDoc, errorMessage);
		preDoc.CSI_Code = "C666";
		AssertNoRowMessageError("CSI_Code is not C651, C658 or N705", preDoc, errorMessage);
		decl.JE_MessageType = "IMP";
		preDoc.CSI_Code = "C651";
		AssertNoRowMessageError("CSI_Code is C651 and declaration is Import", preDoc, errorMessage);
	}

	public void TestCheckCSI_Code()
	{
		var messageError = "[NR9015] Previous document type N705 can only be used at 'Item/Line' level.";
		preDoc.CSI_Code = "N705";
		AssertNoMessageError("parent is InvoiceLine, No MessageError", preDoc.CSI_CodeInfo, messageError);

		var instruction = decl.CustomsEntryInstructions.AddNew();
		var preDoc_Instruction = instruction.PreviousDocuments.AddNew();
		preDoc_Instruction.CSI_Code = "N705";
		AssertHasMessageError("parent is CusEntryInstruction, Has MessageError", preDoc_Instruction.CSI_CodeInfo, messageError);
	}

	public void TestCheckRuleBG9006()
	{
		var errorMessage = "[G9006] If previous document type = N705 then only one occurrence is allowed for previous document";
		var dec = Factory.New<JobDeclaration>();
		var invoiceLine = dec.InvoiceLines.AddNew();

		var preDoc1 = invoiceLine.PreviousDocuments.AddNew();
		preDoc1.CSI_Code = "N705";
		preDoc1.CSI_ReferenceNumber = "123";

		var preDoc2 = invoiceLine.PreviousDocuments.AddNew();
		preDoc2.CSI_Code = "N705";
		preDoc2.CSI_ReferenceNumber = "456";

		CombineAssertions(() =>
		{
			AssertHasRowMessageError("Two Types under Previous Document cannot be N705", preDoc2, errorMessage);

			preDoc2.CSI_Code = "C651";
			AssertHasRowMessageError("Two Types under Previous Document are different with one N705", preDoc2, errorMessage);

			preDoc1.CSI_Code = "C658";
			AssertNoRowMessageError("Type under Previous Document is not N705", preDoc1, errorMessage);
		});
	}

	public void TestIsSubTypeMandatoryIsFalse()
	{
		var previousDocumentValidationForTest = new PreviousDocumentValidationForTest(preDoc);

		AssertEquals("IsSubTypeMandatory should be false", expected: false, previousDocumentValidationForTest.IsSubTypeMandatoryFlag);
	}

	protected override void SetUp()
	{
		base.SetUp();
		decl = Factory.New<JobDeclaration>();
		decl.JE_MessageType = MessageTypeList.Codes.Export;
		invoiceHeader = decl.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		preDoc = invoiceLine.PreviousDocuments.AddNew();
	}
	JobDeclaration decl;
	JobComInvoiceHeader invoiceHeader;
	PreviousDocument preDoc;

	sealed class PreviousDocumentValidationForTest : PreviousDocumentValidation
	{
		public PreviousDocumentValidationForTest(PreviousDocument parent) : base(parent)
		{
		}

		public bool IsSubTypeMandatoryFlag => IsSubTypeMandatory;
	}
}

