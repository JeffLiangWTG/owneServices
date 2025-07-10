using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public class AutoCurrencyAdjustmentGLJournalReversingChina : AutoCurrencyAdjustmentGLJournalReversing
	{
		public AutoCurrencyAdjustmentGLJournalReversingChina(IGeneralLedger journal)
			: base(journal)
		{
		}

		protected override void GenerateReverseTransactions()
		{
			base.GenerateReverseTransactions();
			GenerateReverseTransactionsForPairJournal();
		}

		void GenerateReverseTransactionsForPairJournal()
		{
			if (OriginalTransaction is GLJournal originalJournal)
			{
				ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, originalJournal.AH_TransactionBelongsToGroup);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, originalJournal.AH_TransactionType);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, false);
				filter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, originalJournal.PK);
				var pairJournal = originalJournal.Factory.LoadTop1<GLJournal>(filter);

				if (pairJournal != null)
				{
					originalJournal.PairTransaction = pairJournal;

					var autoCurrencyAdjustmentReversingPair = new ReversingFactory().NewReversing(pairJournal);
					autoCurrencyAdjustmentReversingPair.Reverse();
				}
			}
		}
	}
}
