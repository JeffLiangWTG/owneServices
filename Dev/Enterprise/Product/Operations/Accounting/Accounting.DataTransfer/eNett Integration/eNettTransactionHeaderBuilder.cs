using Enterprise.Accounting.DataTransfer.Invoices;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.eNett_Integration
{
	internal class eNettTransactionHeaderBuilder : TransactionHeaderBuilder
	{
		public eNettTransactionHeaderBuilder(INotificationManager notifier, TransactionBuilderConfig config)
			: base(notifier, config)
		{
		}

		protected override TransactionLineBuilder GetInvoiceLineBuilder(INotificationManager notificationManager, TransactionBuilderConfig config)
		{
			return new eNettTransactionLineBuilder(notificationManager, config);
		}

		protected override ApportionmentBuilder GetApportionmentBuilder(INotificationManager notificationManager, TransactionBuilderConfig config)
		{
			return new eNettApportionmentBuilder(notificationManager, config);
		}

		protected override bool ShouldPopulateCashReceiptOrPaymentDetails(Xsd.TxnHeader xmlInvoiceHeader)
		{
			return false; // the eNett Builder should only create invoices, not payments or receipts
		}
	}
}
