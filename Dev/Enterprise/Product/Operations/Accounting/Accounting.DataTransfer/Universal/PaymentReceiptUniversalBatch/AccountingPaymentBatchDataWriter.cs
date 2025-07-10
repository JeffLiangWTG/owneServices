using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	class AccountingPaymentBatchDataWriter : AccountingReceiptPaymentBatchDataWriter
	{
		public AccountingPaymentBatchDataWriter(IDataWritingManager manager) : base(manager) { }

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.AccountingPayment;
		}
	}
}
