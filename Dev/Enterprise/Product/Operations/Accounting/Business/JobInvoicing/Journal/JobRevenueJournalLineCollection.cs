using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobRevenueJournalLineCollection : DependentTransactionLineCollection
	{
		public JobRevenueJournalLineCollection(JobRevenueJournal journal, ZQuery query)
			: base(journal, query)
		{
		}

		public JobRevenueJournalLineCollection(JobRevenueJournal journal)
			: this(journal, new ZQuery())
		{
		}

		public new JobRevenueJournalLine this[int index]
		{
			get { return (JobRevenueJournalLine)Elements[index]; }
		}

		public new JobRevenueJournalLine AddNew()
		{
			return (JobRevenueJournalLine)base.AddNew();
		}

		protected override void SetDefaultsForNewChildCore(BusinessObject child)
		{
			base.SetDefaultsForNewChildCore(child);
			JobRevenueJournalLine previousItem = ((IBusinessObjectCollectionInternals)this).IsListChangedSuspended ? null : GetSecondLastItemInCollection();
			JobRevenueJournalLine newLine = child as JobRevenueJournalLine;

			if (previousItem != null)
			{
				ZDecimal signedJournalBalance = Journal.AH_LocalExTaxAmount;
				newLine.CopyValuesFrom(previousItem);
				newLine.LocalUnsignedLineAmount = Math.Abs(signedJournalBalance);
				newLine.DebitCreditSign = signedJournalBalance >= 0 ? DebitCreditDataEntry.DR : DebitCreditDataEntry.CR;
				if (!Journal.IsHeaderAmountsUpdateSuspended)
				{
					Journal.UpdateAH_LocalExTaxAmount(newLine.AL_LocalExTaxAmount);
				}
			}
		}

		protected JobRevenueJournal Journal
		{
			get { return (JobRevenueJournal)Master; }
		}

		protected JobRevenueJournalLine GetSecondLastItemInCollection()
		{
			JobRevenueJournalLine result = null;
			int secondLastItemIndex = Count - 1;

			if (secondLastItemIndex >= 0)
			{
				result = this[secondLastItemIndex];
			}

			return result;
		}
	}
}
