using System;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class WarehouseAdjustmentJobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationTest
	{
		public void TestCheckJZ_RX_NKInvoice_Currency()
		{
			invoiceHeader.Validation.ValidateJZ_RX_NKInvoice_Currency();
			AssertNoMessageErrors(invoiceHeader.JZ_RX_NKInvoice_CurrencyInfo);
		}

		public void TestCheckJZ_Incoterm()
		{
			invoiceHeader.Validation.ValidateJZ_IncoTerm();
			AssertNoMessageErrors(invoiceHeader.JZ_IncoTermInfo);
		}

		protected override Type GetTypeForTest()
		{
			return typeof(WarehouseAdjustmentJobComInvoiceHeaderValidation);
		}

		protected override string MessageType => Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;
	}
}
