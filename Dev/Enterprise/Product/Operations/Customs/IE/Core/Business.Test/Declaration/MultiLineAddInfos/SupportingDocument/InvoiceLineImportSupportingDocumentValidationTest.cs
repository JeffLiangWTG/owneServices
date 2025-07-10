using CargoWise.Types;
using Enterprise.Customs.Common.IE;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineImportSupportingDocumentValidation))]
	sealed class InvoiceLineImportSupportingDocumentValidationTest : SupportingDocumentValidationAbstractTest
	{
		public void TestCheckCSI_UnitOfQuantity()
		{
			var targetInfo = supportingDocument.CSI_UnitOfQuantityInfo;
			supportingDocument.CSI_Quantity = 0;
			supportingDocument.CSI_UnitOfQuantity = ZString.Empty;
			AssertNoMessageErrors(targetInfo);

			supportingDocument.CSI_Quantity = 123;
			supportingDocument.Validation.ValidateCSI_UnitOfQuantity();
			AssertHasMessageError(targetInfo, "When Quantity is provided at Invoice line's Supporting Document, then Measurement Unit & Qualifier is mandatory");

			supportingDocument.CSI_Quantity = 0;
			supportingDocument.CSI_UnitOfQuantity = "UQB";
			AssertHasMessageError(targetInfo, "Supporting Documents' Measurement Unit is required at Invoice lines only if Quantity is provided.");

			supportingDocument.CSI_Quantity = 123;
			supportingDocument.CSI_UnitOfQuantity = "UQB";
			AssertNoMessageErrors(targetInfo);
		}

		public void TestCheckCSI_RX_NKCurrency()
		{
			var targetInfo = supportingDocument.CSI_RX_NKCurrencyInfo;
			supportingDocument.CSI_Value = 0;
			supportingDocument.CSI_RX_NKCurrency = ZString.Empty;
			AssertNoMessageErrors(targetInfo);

			supportingDocument.CSI_Value = 123;
			supportingDocument.Validation.ValidateCSI_RX_NKCurrency();
			AssertHasMessageError(targetInfo, "When Amount is provided at Invoice line's Supporting Document, then Currency is mandatory");

			supportingDocument.CSI_Value = 0;
			supportingDocument.CSI_RX_NKCurrency = "AUD";
			AssertHasMessageError(targetInfo, "Supporting Documents' Currency is required at Invoice lines only if Amount is provided.");

			supportingDocument.CSI_Value = 123;
			supportingDocument.CSI_RX_NKCurrency = "AUD";
			AssertNoMessageErrors(targetInfo);
		}

		public void TestCheckCSI_Code()
		{
			var targetInfo = supportingDocument.CSI_CodeInfo;
			var message = "[BR2030] Please enter Supporting Document '1A05' at Entry Instruction or Invoice Header level.";
			declaration.JE_ApplicationCode = "V1";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			instruction.CEI_Style = "H1";
			supportingDocument.CSI_Code = "1A05";
			validation.ValidateCSI_Code();
			AssertHasMessageError(targetInfo, message);

			declaration.JE_ApplicationCode = "V2";
			validation.ValidateCSI_Code();
			AssertNoMessageError(targetInfo, message);

			declaration.JE_ApplicationCode = "V1";
			instruction.CEI_Style = "I1";
			validation.ValidateCSI_Code();
			AssertNoMessageError(targetInfo, message);

			instruction.CEI_Style = "H2";
			validation.ValidateCSI_Code();
			AssertHasMessageError(targetInfo, message);

			supportingDocument.CSI_Code = "1A06";
			validation.ValidateCSI_Code();
			AssertNoMessageError(targetInfo, message);
		}

		protected override string MessageType => IEJobMessageTypeList.Codes.Import;
		protected override SupportingDocument SetupSupportingDocument()
		{
			inovice = declaration.Invoices.AddNew();
			invoiceLine = inovice.JobComInvoiceLines.AddNew();
			return invoiceLine.SupportingDocuments.AddNew();
		}
		JobComInvoiceHeader inovice;
		JobComInvoiceLine invoiceLine;
	}
}
