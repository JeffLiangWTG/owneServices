using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class DeltaIEPreviousDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestIfDuplicateTypeAndReferenceInInvoiceAndDeclarationLevel()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var invoiceHeader = declaration.Invoices.AddNew();

			var miscPreviousDocument = declaration.PreviousDocuments.AddNew();
			miscPreviousDocument.CSI_Code = "TST1";
			miscPreviousDocument.CSI_ReferenceNumber = "REF1";

			var invoiceHeaderPreviousDocument = invoiceHeader.PreviousDocuments.AddNew();
			invoiceHeaderPreviousDocument.CSI_Code = "TST1";
			invoiceHeaderPreviousDocument.CSI_ReferenceNumber = "REF1";

			invoiceHeaderPreviousDocument.Validation.ValidateCSI_Code();
			invoiceHeaderPreviousDocument.Validation.ValidateCSI_ReferenceNumber();

			AssertHasMessageError(invoiceHeaderPreviousDocument.CSI_CodeInfo, "The type and reference number of this record is duplicate with records under Misc tab.");
			AssertHasMessageError(invoiceHeaderPreviousDocument.CSI_ReferenceNumberInfo, "The type and reference number of this record is duplicate with records under Misc tab.");

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			invoiceHeaderPreviousDocument.Validation.ValidateCSI_Code();
			invoiceHeaderPreviousDocument.Validation.ValidateCSI_ReferenceNumber();

			AssertNoMessageError(invoiceHeaderPreviousDocument.CSI_CodeInfo, "The type and reference number of this record is duplicate with records under Misc tab.");
			AssertNoMessageError(invoiceHeaderPreviousDocument.CSI_ReferenceNumberInfo, "The type and reference number of this record is duplicate with records under Misc tab.");

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			invoiceHeaderPreviousDocument.CSI_Code = "TST2";
			invoiceHeaderPreviousDocument.Validation.ValidateCSI_Code();
			invoiceHeaderPreviousDocument.Validation.ValidateCSI_ReferenceNumber();

			AssertNoMessageError(invoiceHeaderPreviousDocument.CSI_CodeInfo, "The type and reference number of this record is duplicate with records under Misc tab.");
			AssertNoMessageError(invoiceHeaderPreviousDocument.CSI_ReferenceNumberInfo, "The type and reference number of this record is duplicate with records under Misc tab.");

			var invoiceHeader2 = Factory.New<JobComInvoiceHeader>();
			var invoiceHeader2PreviousDocument = invoiceHeader2.PreviousDocuments.AddNew();
			invoiceHeader2PreviousDocument.CSI_Code = "TST1";
			invoiceHeader2PreviousDocument.CSI_ReferenceNumber = "REF1";

			AssertNoExceptionThrown(invoiceHeader2PreviousDocument.Validation.ValidateCSI_Code);
			AssertNoExceptionThrown(invoiceHeader2PreviousDocument.Validation.ValidateCSI_ReferenceNumber);
		}
	}
}
