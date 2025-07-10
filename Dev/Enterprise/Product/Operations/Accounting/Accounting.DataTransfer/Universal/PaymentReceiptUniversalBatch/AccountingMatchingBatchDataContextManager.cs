using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	class AccountingMatchingBatchDataContextManager : AccountingReceiptPaymentBatchDataContextManager<MatchingBase>
	{
		public override DataContextType DataContextType => DataContextType.AccountingMatching;

		public override ZString DataContextKey => string.Empty;

		public override ITopLevelDataObjectWriter GetTransactionBatchDataObjectWriter(IDataWritingManager writeManager)
		{
			return new AccountingMatchingBatchDataWriter(writeManager);
		}
	}
}
