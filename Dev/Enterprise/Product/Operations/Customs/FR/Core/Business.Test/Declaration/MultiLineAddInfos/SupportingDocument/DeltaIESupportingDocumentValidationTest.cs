using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class DeltaIESupportingDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestIfDuplicateTypeAndReferenceInInvoiceAndDeclarationLevel()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var invoiceHeader = declaration.Invoices.AddNew();

			var miscSupportingDocument = declaration.SupportingDocuments.AddNew();
			miscSupportingDocument.CSI_Code = "TST1";
			miscSupportingDocument.CSI_ReferenceNumber = "REF1";

			var invoiceHeaderSupportingDocument = invoiceHeader.SupportingDocuments.AddNew();
			invoiceHeaderSupportingDocument.CSI_Code = "TST1";
			invoiceHeaderSupportingDocument.CSI_ReferenceNumber = "REF1";

			invoiceHeaderSupportingDocument.Validation.ValidateCSI_Code();
			invoiceHeaderSupportingDocument.Validation.ValidateCSI_ReferenceNumber();

			AssertHasMessageError(invoiceHeaderSupportingDocument.CSI_CodeInfo, "The type and reference number of this record is duplicate with records under Misc tab.");
			AssertHasMessageError(invoiceHeaderSupportingDocument.CSI_ReferenceNumberInfo, "The type and reference number of this record is duplicate with records under Misc tab.");

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			invoiceHeaderSupportingDocument.Validation.ValidateCSI_Code();
			invoiceHeaderSupportingDocument.Validation.ValidateCSI_ReferenceNumber();

			AssertNoMessageError(invoiceHeaderSupportingDocument.CSI_CodeInfo, "The type and reference number of this record is duplicate with records under Misc tab.");
			AssertNoMessageError(invoiceHeaderSupportingDocument.CSI_ReferenceNumberInfo, "The type and reference number of this record is duplicate with records under Misc tab.");

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			invoiceHeaderSupportingDocument.CSI_Code = "TST2";
			invoiceHeaderSupportingDocument.Validation.ValidateCSI_Code();
			invoiceHeaderSupportingDocument.Validation.ValidateCSI_ReferenceNumber();

			AssertNoMessageError(invoiceHeaderSupportingDocument.CSI_CodeInfo, "The type and reference number of this record is duplicate with records under Misc tab.");
			AssertNoMessageError(invoiceHeaderSupportingDocument.CSI_ReferenceNumberInfo, "The type and reference number of this record is duplicate with records under Misc tab.");

			var invoiceHeader2 = Factory.New<JobComInvoiceHeader>();
			var invoiceHeader2SupportingDocument = invoiceHeader2.SupportingDocuments.AddNew();
			invoiceHeader2SupportingDocument.CSI_Code = "TST1";
			invoiceHeader2SupportingDocument.CSI_ReferenceNumber = "REF1";

			AssertNoExceptionThrown(invoiceHeader2SupportingDocument.Validation.ValidateCSI_Code);
			AssertNoExceptionThrown(invoiceHeader2SupportingDocument.Validation.ValidateCSI_ReferenceNumber);
		}
	}
}
