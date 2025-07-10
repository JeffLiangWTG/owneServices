using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public partial class CashBookReversing : ReversingBase
	{
		public CashBookReversing(ICashBook cashBookTransaction)
			: base(cashBookTransaction)
		{
		}

		protected ICashBook CashbookOriginalTransaction
		{
			get { return OriginalTransaction as ICashBook; }
		}

		protected ICashBook ReversingCashbookTransaction
		{
			get { return ReverseTransaction as ICashBook; }
		}

		protected override bool CanTransactionBeReversed()
		{
			return !CashbookOriginalTransaction.IsClearedInCashbook
				&& base.CanTransactionBeReversed()
				&& string.IsNullOrEmpty(GetCannotReverseBankTransferCashbookExchangeDiffErrorMessage())
				&& string.IsNullOrEmpty(GetCancelDDRBatchError(OriginalTransaction as DirectDebitBatchHeader));
		}

		protected override ZString GenerateCantReverseErrorMessage()
		{
			if (CashbookOriginalTransaction.IsClearedInCashbook)
			{
				return ClearedInCashBookErrorMessage;
			}

			var result = base.GenerateCantReverseErrorMessage();
			if (!string.IsNullOrEmpty(result))
			{
				return result;
			}

			result = GetCannotReverseBankTransferCashbookExchangeDiffErrorMessage();
			if (!string.IsNullOrEmpty(result))
			{
				return result;
			}

			return GetCancelDDRBatchError(OriginalTransaction as DirectDebitBatchHeader);
		}

		protected override ZString AlreadyReversedErrorMessage
		{
			get
			{
				if (OriginalTransaction is BankTransfer)
				{
					return AlreadyCancelledMessage;
				}
				else
				{
					return base.AlreadyReversedErrorMessage;
				}
			}
		}

		string AlreadyCancelledMessage
		{
			get { return Res.GetString("4f3eadd7-72ff-4b07-a5ea-f17b28c37336", "This transaction has already been canceled."); }
		}

		string GetCannotReverseBankTransferCashbookExchangeDiffErrorMessage()
		{
			if (CashbookOriginalTransaction is CashbookExchangeDiff cashbookExchangeDiff
				&& cashbookExchangeDiff.AH_TransactionBelongsToGroup.IsValid
				&& cashbookExchangeDiff.AH_TransactionCategory == Core.Constants.TransactionCategory.Codes.RealizedExchangeGainLoss)
			{
				var bankTransferFilter = AccountingUtils.GetTransactionFilter(cashbookExchangeDiff.AH_TransactionBelongsToGroup, cashbookExchangeDiff.AH_GC, AccTransactionHeader.TransactionCountConstants.BankTransferToRow);
				var bankTransfer = cashbookExchangeDiff.Factory.LoadTop1<AccTransactionHeader>(bankTransferFilter);
				if (bankTransfer != null)
				{
					return Res.GetString("B6141912-2BD6-4C93-B68D-6549B8334E99", "This EXX Transaction was created by Bank Transfer {0}, and can only be reversed by reversing the Bank Transfer.", bankTransfer.AH_TransactionNum);
				}
			}

			return string.Empty;
		}

		#region DirectDebitBatchHeader Errors

		ZString GetCancelDDRBatchError(DirectDebitBatchHeader batch)
		{
			ZString result = "";
			if (batch != null)
			{
				if (AnyIndividualPaymentCleared(batch))
				{
					result = Res.GetString("d3f0d601-61d5-427f-a157-63bfd3d84f3f", "You cannot cancel DDR Batch because some of the Payments / Direct Payments included in the batch is already cleared in cashbook.");
				}
				else if (AnyIndividualPaymentsCancelled(batch))
				{
					result = Res.GetString("aacd034f-d4d0-4b93-9db3-6a9b26f50f4d", "You cannot cancel this DDR batch because one or more payments in the batch are canceled.");
				}
			}
			return result;
		}

		bool AnyIndividualPaymentCleared(DirectDebitBatchHeader batch)
		{
			ZQuery query = GetPaymentInDDRBatchQuery(batch);
			query.AddToFilter(AccTransactionHeaderSchema.AH_DateClearedInCashbook, SQLComparisonOperator.NotEqual, ZDateTime.Empty);

			return batch.Factory.LoadTop1<AccTransactionHeader>(query) != null;
		}

		bool AnyIndividualPaymentsCancelled(DirectDebitBatchHeader batch)
		{
			ZQuery query = GetPaymentInDDRBatchQuery(batch);
			query.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, SQLComparisonOperator.NotEqual, ZBool.False);

			return batch.Factory.LoadTop1<AccTransactionHeader>(query) != null;
		}

		ZQuery GetPaymentInDDRBatchQuery(DirectDebitBatchHeader batch)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			query.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptBatchNo, batch.AH_TransactionNum);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

			ZQuery transactionTypeInclude = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment);
			transactionTypeInclude.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, TransactionTypes.DirectPayment);

			query.AddToFilter(transactionTypeInclude);

			return query;
		}

		#endregion
	}
}
