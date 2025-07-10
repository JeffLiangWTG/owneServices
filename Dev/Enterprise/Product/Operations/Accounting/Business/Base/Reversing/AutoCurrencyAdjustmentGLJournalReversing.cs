using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public class AutoCurrencyAdjustmentGLJournalReversing : GLJournalReversing
	{
		public AutoCurrencyAdjustmentGLJournalReversing(IGeneralLedger journal)
			: base(journal)
		{
		}

		protected override void SetReverseJournalValues()
		{
			if (ReverseTransaction is GLJournal reverseJournal && OriginalTransaction is GLJournal originalJournal)
			{
				reverseJournal.AH_PostDate = originalJournal.AH_PostDate;
				reverseJournal.AH_DueDate = originalJournal.AH_DueDate;
				reverseJournal.PeriodPK = originalJournal.PeriodPK;
				reverseJournal.AH_Desc = Res.GetString("E231FFDB-E200-4726-9478-8FFE84732B28", "REVERSAL OF JOURNAL NUMBER {0} – {1}", originalJournal.AH_TransactionNum, reverseJournal.AH_Desc);
			}
		}
	}
}
