using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public class PeriodApportionmentGLJournalReversing : GLJournalReversing
	{
		public PeriodApportionmentGLJournalReversing(IGeneralLedger journal, InvoicingBase masterInvoice, BusinessObjectFactory factory, ZBool isReversingPrePaymentTransaction)
			: base(journal)
		{
			this.masterInvoice = masterInvoice;
			this.factory = factory;
			IsReversingPrePaymentTransaction = isReversingPrePaymentTransaction;
		}

		readonly InvoicingBase masterInvoice;
		readonly BusinessObjectFactory factory;

		public ZBool IsReversingPrePaymentTransaction
		{
			get;
		}

		protected override void GenerateReverseTransactions()
		{
			base.GenerateReverseTransactions();
			AdjustPostDateForReversalJournal();
			ModifyDescriptions();
			LinkJournalsToMasterInvoice();
		}

		AccountingPeriodCalculator PeriodCalculator => calc ?? (calc = new AccountingPeriodCalculator(factory));
		AccountingPeriodCalculator calc;

		void LinkJournalsToMasterInvoice()
		{
			if (ReverseTransaction is GLJournal glJournal)
			{
				glJournal.AH_TransactionBelongsToGroup = masterInvoice.PK;
			}
		}

		void ModifyDescriptions()
		{
			ZString prependReverse(ZString desc) => Res.GetString("49658D4D-AD4D-4DC1-9F1C-0158D7792BD2", "(REVERSE) {0}", desc);

			var reverseTransaction = ReverseTransaction;
			if (reverseTransaction is GLJournal reverseJournal)
			{
				reverseJournal.AH_Desc = prependReverse(reverseJournal.AH_Desc);
				foreach (GLJournalLine line in reverseJournal.Lines)
				{
					line.AL_Desc = prependReverse(line.AL_Desc);
				}
			}
		}

		public void AdjustPostDateForReversalJournal()
		{
			if (ReverseTransaction is GLJournal reverseJournal && OriginalTransaction is GLJournal originalJournal)
			{
				int masterInvoiceReversalPeriod = PeriodCalculator.GetPeriodFromDate(masterInvoice.AH_PostDate);

				var originalJournalPeriod = PeriodCalculator.GetPeriodFromDate(originalJournal.AH_PostDate);

				if (IsReversingPrePaymentTransaction)
				{
					reverseJournal.AH_PostDate = originalJournalPeriod < masterInvoiceReversalPeriod
						? PeriodCalculator.GetLastDayForPeriod(masterInvoiceReversalPeriod)
						: originalJournal.AH_PostDate;
				}
				else
				{
					if (PeriodCalculator.IsPeriodGLClosed(originalJournalPeriod))
					{
						var nextOpeningPeriod = PeriodCalculator.GetNextGeneralLedgerOpenPeriodManagementFromDate(originalJournal.AH_PostDate, GlbCompany.CurrentCompany.PK);
						reverseJournal.AH_PostDate = nextOpeningPeriod != null ? nextOpeningPeriod.AM_EndDate : ZDateTime.Today;
					}
					else
					{
						reverseJournal.AH_PostDate = PeriodCalculator.GetLastDayForPeriod(originalJournalPeriod);
					}
				}
			}
		}

		protected override bool IsClosePeriodCheckRequired() => IsReversingPrePaymentTransaction;

		protected override void SetCancellationFlagOnTransactionsToReverse()
		{
		}

		protected override void SetTransactionBelongsToGroupOnTransactionsToReverse()
		{
		}
	}
}
