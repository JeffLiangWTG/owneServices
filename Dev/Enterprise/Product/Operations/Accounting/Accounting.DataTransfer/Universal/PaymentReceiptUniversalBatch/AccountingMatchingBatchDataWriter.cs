using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	class AccountingMatchingBatchDataWriter : AccountingReceiptPaymentBatchDataWriter
	{
		public AccountingMatchingBatchDataWriter(IDataWritingManager manager) : base(manager) { }

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.AccountingMatching;
		}
	}
}
