using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public partial class GLJournalReversing : ReversingBase
	{
		public GLJournalReversing(IGeneralLedger journal)
			: base(journal)
		{
		}

		protected override bool CanTransactionBeReversed()
		{
			return base.CanTransactionBeReversed() && string.IsNullOrWhiteSpace(GenerateCantReverseErrorMessage());
		}

		protected virtual bool IsClosePeriodCheckRequired() => true;

		protected override ZString GenerateCantReverseErrorMessage()
		{
			var message = base.GenerateCantReverseErrorMessage();
			if (string.IsNullOrWhiteSpace(message) && ReverseTransaction is GLJournal reverseJournal && IsClosePeriodCheckRequired())
			{
				var periodCalculator = new AccountingPeriodCalculator(reverseJournal.Factory);
				var period = periodCalculator.GetPeriodFromDate(reverseJournal.AH_PostDate);
				if (periodCalculator.IsPeriodGLClosed(period))
				{
					message = Res.GetString("F5FB9AE7-A80E-4EC7-AE66-16071BB9A7A1", "{0} GL period required for posting is closed.", period);
				}
			}

			return message;
		}

		protected override void GenerateReverseTransactions()
		{
			base.GenerateReverseTransactions();
			SetReverseJournalValues();
		}

		protected virtual void SetReverseJournalValues()
		{
			if (ReverseTransaction is GLJournal reverseJournal && OriginalTransaction is GLJournal originalJournal)
			{
				var periodCalculator = new AccountingPeriodCalculator(originalJournal.Factory);
				var postPeriod = originalJournal.PostPeriod;
				var reverseOrEndingPeriod = originalJournal.AgePeriod;

				if (postPeriod != AccountingPeriodCalculator.InvalidPeriod)
				{
					reverseJournal.AH_PostDate = periodCalculator.IsPeriodGLClosed(postPeriod) ? ZDateTime.Today : originalJournal.AH_PostDate;
				}

				if (reverseOrEndingPeriod != AccountingPeriodCalculator.InvalidPeriod)
				{
					reverseJournal.AH_DueDate = periodCalculator.IsPeriodGLClosed(reverseOrEndingPeriod) ? ZDateTime.Empty : originalJournal.AH_DueDate;
				}
			}
		}

		protected override void SetTransactionBelongsToGroupOnTransactionsToReverse()
		{
			if (ReverseTransaction is GLJournal reverseJournal && OriginalTransaction is GLJournal originalJournal)
			{
				if (originalJournal.AH_TransactionBelongsToGroup.IsEmpty)
				{
					originalJournal.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
				}

				reverseJournal.AH_TransactionBelongsToGroup = originalJournal.AH_TransactionBelongsToGroup;
			}
		}

		protected override void SetReversingDescriptionOnTransactions()
		{
		}

		protected override void OnFactorySaved(bool savedSuccessfully)
		{
		}
	}
}
