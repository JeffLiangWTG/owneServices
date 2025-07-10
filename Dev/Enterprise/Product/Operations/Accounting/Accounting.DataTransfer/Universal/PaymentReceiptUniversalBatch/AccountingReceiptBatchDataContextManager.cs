using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	class AccountingReceiptBatchDataContextManager : AccountingReceiptPaymentBatchDataContextManager<Receipt>
	{
		public override DataContextType DataContextType => DataContextType.AccountingReceipt;

		public override ZString DataContextKey => string.Join(" ", ParentBO.AH_Ledger, ParentBO.AH_TransactionType, ParentBO.AH_TransactionNum);

		public override ITopLevelDataObjectWriter GetTransactionBatchDataObjectWriter(IDataWritingManager writeManager)
		{
			return new AccountingReceiptBatchDataWriter(writeManager);
		}
	}
}
