using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	class AccountingPaymentBatchDataContextManager : AccountingReceiptPaymentBatchDataContextManager<Payment>
	{
		public override DataContextType DataContextType => DataContextType.AccountingPayment;

		public override ZString DataContextKey => string.Join(" ", ParentBO.AH_Ledger, ParentBO.AH_TransactionType, ParentBO.AH_TransactionNum);

		public override ITopLevelDataObjectWriter GetTransactionBatchDataObjectWriter(IDataWritingManager writeManager)
		{
			return new AccountingPaymentBatchDataWriter(writeManager);
		}
	}
}
