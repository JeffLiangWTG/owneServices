using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	class AccountingReceiptBatchDataWriter : AccountingReceiptPaymentBatchDataWriter
	{
		public AccountingReceiptBatchDataWriter(IDataWritingManager manager) : base(manager) { }

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.AccountingReceipt;
		}
	}
}
