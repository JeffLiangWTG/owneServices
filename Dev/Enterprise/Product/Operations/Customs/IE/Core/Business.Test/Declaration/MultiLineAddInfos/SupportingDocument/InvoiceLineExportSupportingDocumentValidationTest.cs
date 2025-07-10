using CargoWise.Types;
using Enterprise.Customs.Common.IE;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineExportSupportingDocumentValidation))]
	sealed class InvoiceLineExportSupportingDocumentValidationTest : SupportingDocumentValidationAbstractTest
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

		protected override string MessageType => IEJobMessageTypeList.Codes.Export;
		protected override SupportingDocument SetupSupportingDocument() => declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().SupportingDocuments.AddNew();
	}
}
