using Enterprise.Accounting.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Accounting.DataTransfer.Testing.Universal.PaymentReceiptUniversalBatch
{
	class AccountingReceiptBatchDataWriterTest : AccountingReceiptPaymentBatchDataWriterTest
	{
		protected override AccountingReceiptPaymentBatchDataWriter GetBatchDataWriter(IDataWritingManager manager)
		{
			return new AccountingReceiptBatchDataWriter(manager);
		}
	}
}
