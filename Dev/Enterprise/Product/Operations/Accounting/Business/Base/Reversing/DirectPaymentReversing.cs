using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Reversing
{
#if DEBUG
	internal
	#endif
	partial class DirectPaymentReversing : CashBookReversing
	{
		public DirectPaymentReversing(IDirectPayment directPayment)
			: base(directPayment)
		{
		}

		protected override void GenerateReverseTransactions()
		{
			base.GenerateReverseTransactions();
			DirectPayment originalDirectPayment = OriginalTransaction as DirectPayment;
			if (originalDirectPayment != null)
			{
				ReversingDDRBatchHeader = DirectDebitBatchHeader.ReverseDDRBatch(originalDirectPayment);
			}
		}

		DirectDebitBatchHeader ReversingDDRBatchHeader;

		protected override void SetOtherNumberFountainFields(BusinessObjectFactory factory)
		{
			base.SetOtherNumberFountainFields(factory);
			if (ReversingDDRBatchHeader != null)
			{
				ZString nextDDRBatchNo = AccountingNumberFountainWrapperFactory.Instance.DDRBatchNo.GetNext(ReversingDDRBatchHeader.Factory);
				DirectPayment revPayment = (DirectPayment)ReverseTransaction;
				revPayment.AH_ReceiptBatchNo = nextDDRBatchNo;
				ReversingDDRBatchHeader.AH_TransactionNum = nextDDRBatchNo;
				ReversingDDRBatchHeader.AH_ReceiptBatchNo = nextDDRBatchNo;
			}
		}

		protected override bool CanTransactionBeReversed()
		{
			return base.CanTransactionBeReversed() && !IsBankTransferTransaction;
		}

		protected override ZString GenerateCantReverseErrorMessage()
		{
			string result = base.GenerateCantReverseErrorMessage();
			if (IsBankTransferTransaction)
			{
				result = CreatedByTRFMessage;
			}
			return result;
		}

		bool IsBankTransferTransaction
		{
			get
			{
				TransactionHeader transaction = OriginalTransaction as TransactionHeader;
				if (transaction != null && transaction.AH_TransactionBelongsToGroup != ZGuid.Empty && transaction.AH_TransactionCount == 3)
				{
					ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, transaction.AH_TransactionBelongsToGroup);
					filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCount, (byte)1);
					filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, transaction.AH_GC);
					BankTransferFromRow bankTransferFromRow = transaction.Factory.LoadTop1<BankTransferFromRow>(filter);

					return bankTransferFromRow != null;
				}
				return false;
			}
		}

		string CreatedByTRFMessage
		{
			get { return Res.GetString("3310a21b-dda4-4c4e-a99a-857756f504c8", "The transactions cannot be reversed for the following reasons: - It cannot be reversed individually as it relates to Bank Transfer. It will be automatically reversed through canceling of the Bank Transfer."); }
		}
	}
}
