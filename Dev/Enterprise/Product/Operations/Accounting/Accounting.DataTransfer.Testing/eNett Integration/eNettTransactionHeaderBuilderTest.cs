using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Accounting.DataTransfer.Invoices.Testing;

namespace Enterprise.Accounting.DataTransfer.eNett_Integration.Testing
{
	sealed class eNettTransactionHeaderBuilderTest : TransactionHeaderBuilderTest
	{
		protected override TransactionHeaderBuilder GetTransactionHeaderBuilderForTest()
		{
			return new eNettTransactionHeaderBuilder(Notifier, new TransactionBuilderConfig());
		}

		protected override bool InvoiceWithPaymentOrReceiptShouldBeCreated
		{
			get
			{
				return false;
			}
		}

		protected override void AssertCashReceiptPaymentDetails(Invoice invoice, bool isCashInvoice, string paymentType, ZGuid bankAccount, ZGuid chequeBook, string chequeReference, string chequeDrawer, string chequeDrawerBank, string chequeDrawerBankBranch, bool shouldHaveHotCheque)
		{
			// the eNett Builder should only create invoices, not payments or receipts
			AssertEquals("Is Cash Invoice", false, invoice.IsInvoiceReceiptPayment);
		}
	}
}
