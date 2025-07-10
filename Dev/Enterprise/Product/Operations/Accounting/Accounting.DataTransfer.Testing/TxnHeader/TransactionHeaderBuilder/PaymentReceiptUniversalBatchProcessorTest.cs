using Enterprise.Accounting.DataTransfer.Universal;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	public class PaymentReceiptUniversalBatchProcessorTest : TxnHeaderProcessorBaseTest
	{
		protected override TxnHeaderProcessorBase GetTestProcessor()
		{
			return new PaymentReceiptUniversalBatchProcessor(Factory, new NotificationManager(new NotificationBuffer()));
		}
	}
}
