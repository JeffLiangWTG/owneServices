namespace Enterprise.Accounting.DataTransfer.Invoices
{
	internal class ForwardingToGatewayTransactionHeaderBuilder : TransactionHeaderBuilder
	{
		public ForwardingToGatewayTransactionHeaderBuilder(NotificationManager notifier, TransactionBuilderConfig config) : base(notifier, config)
		{
		}

		protected override TransactionLineBuilder GetInvoiceLineBuilder(INotificationManager notificationManager, TransactionBuilderConfig config)
		{
			return new ForwardingToGatewayTransactionLineBuilder(notificationManager, config);
		}
	}
}
