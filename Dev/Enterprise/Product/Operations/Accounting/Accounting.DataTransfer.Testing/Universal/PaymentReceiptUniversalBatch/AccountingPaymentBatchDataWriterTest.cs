using Enterprise.Accounting.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Accounting.DataTransfer.Testing.Universal.PaymentReceiptUniversalBatch
{
	class AccountingPaymentBatchDataWriterTest : AccountingReceiptPaymentBatchDataWriterTest
	{
		protected override AccountingReceiptPaymentBatchDataWriter GetBatchDataWriter(IDataWritingManager manager)
		{
			return new AccountingPaymentBatchDataWriter(manager);
		}
	}
}
