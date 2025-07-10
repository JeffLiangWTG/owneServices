using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	public class PaymentReceiptRemittanceFileProcessorTest : TxnHeaderProcessorBaseTest
	{
		protected override TxnHeaderProcessorBase GetTestProcessor()
		{
			return new PaymentReceiptRemittanceFileProcessor(Factory, new NotificationBuffer());
		}
	}
}
