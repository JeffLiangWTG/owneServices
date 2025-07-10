using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class DeltaIEAdditionalInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestIfDuplicateTypeAndReferenceInInvoiceAndDeclarationLevel()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var invoiceHeader = declaration.Invoices.AddNew();

			var miscAdditionalInfo = declaration.AdditionalInfos.AddNew();
			miscAdditionalInfo.CSI_Code = "TST1";
			miscAdditionalInfo.CSI_ReferenceNumber = "REF1";

			var invoiceHeaderAdditionalInfo = invoiceHeader.AdditionalInfos.AddNew();
			invoiceHeaderAdditionalInfo.CSI_Code = "TST1";
			invoiceHeaderAdditionalInfo.CSI_ReferenceNumber = "REF1";

			invoiceHeaderAdditionalInfo.Validation.ValidateCSI_Code();
			invoiceHeaderAdditionalInfo.Validation.ValidateCSI_ReferenceNumber();

			AssertHasMessageError(invoiceHeaderAdditionalInfo.CSI_CodeInfo, "The type and reference number of this record is duplicate with records under Misc tab.");
			AssertHasMessageError(invoiceHeaderAdditionalInfo.CSI_ReferenceNumberInfo, "The type and reference number of this record is duplicate with records under Misc tab.");

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			invoiceHeaderAdditionalInfo.Validation.ValidateCSI_Code();
			invoiceHeaderAdditionalInfo.Validation.ValidateCSI_ReferenceNumber();

			AssertNoMessageError(invoiceHeaderAdditionalInfo.CSI_CodeInfo, "The type and reference number of this record is duplicate with records under Misc tab.");
			AssertNoMessageError(invoiceHeaderAdditionalInfo.CSI_ReferenceNumberInfo, "The type and reference number of this record is duplicate with records under Misc tab.");

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			invoiceHeaderAdditionalInfo.CSI_Code = "TST2";
			invoiceHeaderAdditionalInfo.Validation.ValidateCSI_Code();
			invoiceHeaderAdditionalInfo.Validation.ValidateCSI_ReferenceNumber();

			AssertNoMessageError(invoiceHeaderAdditionalInfo.CSI_CodeInfo, "The type and reference number of this record is duplicate with records under Misc tab.");
			AssertNoMessageError(invoiceHeaderAdditionalInfo.CSI_ReferenceNumberInfo, "The type and reference number of this record is duplicate with records under Misc tab.");

			var invoiceHeader2 = Factory.New<JobComInvoiceHeader>();
			var invoiceHeader2AdditionalInfo = invoiceHeader2.AdditionalInfos.AddNew();
			invoiceHeader2AdditionalInfo.CSI_Code = "TST1";
			invoiceHeader2AdditionalInfo.CSI_ReferenceNumber = "REF1";

			AssertNoExceptionThrown(invoiceHeader2AdditionalInfo.Validation.ValidateCSI_Code);
			AssertNoExceptionThrown(invoiceHeader2AdditionalInfo.Validation.ValidateCSI_ReferenceNumber);
		}
	}
}
