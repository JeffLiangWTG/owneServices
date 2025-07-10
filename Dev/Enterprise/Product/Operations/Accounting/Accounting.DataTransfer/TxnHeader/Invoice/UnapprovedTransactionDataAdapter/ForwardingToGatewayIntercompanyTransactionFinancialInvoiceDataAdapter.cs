namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public class ForwardingToGatewayIntercompanyTransactionFinancialInvoiceDataAdapter : IntercompanyTransactionFinancialInvoiceDataAdapter
	{
		public ForwardingToGatewayIntercompanyTransactionFinancialInvoiceDataAdapter() : base()
		{
		}

		protected override TransactionHeaderBuilder GetTransactionHeaderBuilder(bool isCrossLedgerImport)
		{
			return new ForwardingToGatewayTransactionHeaderBuilder(Notifier, GetTransactionBuilderConfig(isCrossLedgerImport));
		}
	}
}
