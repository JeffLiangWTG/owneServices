using Enterprise.Accounting.DataTransfer.Invoices.Testing;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	sealed class ForwardingToGatewayTransactionHeaderBuilderTest : TransactionHeaderBuilderTest
	{
		protected override TransactionHeaderBuilder GetTransactionHeaderBuilderForTest() => new ForwardingToGatewayTransactionHeaderBuilder(Notifier, new TransactionBuilderConfig());
	}
}
