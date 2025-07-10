namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public class UnapprovedTransactionFinancialInvoiceDataAdapter : FinancialInvoiceDataAdapter
	{
		public UnapprovedTransactionFinancialInvoiceDataAdapter()
			: base(false)
		{ }

		protected override TransactionBuilderConfig GetTransactionBuilderConfig(bool isCrossLedgerImport)
		{
			TransactionBuilderConfig config = base.GetTransactionBuilderConfig(isCrossLedgerImport);
			config.AllowResetChargeCodeAndJobOnError = false;
			config.SetLineDescription = false;
			return config;
		}
	}
}
