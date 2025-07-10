using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobRevenueJournalChargeCollection : NonPersistentBusinessObjectCollection<JobRevenueJournalCharge>
	{
		public JobRevenueJournalChargeCollection(JobRevenueJournal parentJournal)
			: base(parentJournal.Factory)
		{
			ParentJournal = parentJournal;
		}

		readonly JobRevenueJournal ParentJournal;

		public Job Job
		{
			get { return Job_cached; }
			set
			{
				Job_cached = value;

				foreach (JobRevenueJournalCharge journalCharge in this)
				{
					journalCharge.Job = Job;
				}
			}
		}
		Job Job_cached;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new JobRevenueJournalCharge(ParentJournal, Job);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			JobRevenueJournalCharge previousItem = GetSecondLastItemInCollection();
			JobRevenueJournalCharge newLine = child as JobRevenueJournalCharge;

			if (previousItem != null)
			{
				newLine.ExchangeRate = previousItem.ExchangeRate;
			}
		}

		JobRevenueJournalCharge GetSecondLastItemInCollection()
		{
			JobRevenueJournalCharge result = null;
			int secondLastItemIndex = Count - 1;

			if (secondLastItemIndex >= 0)
			{
				result = this[secondLastItemIndex];
			}

			return result;
		}

		internal void DetachCharges()
		{
			foreach (JobRevenueJournalCharge journalCharge in this)
			{
				journalCharge.Detach();
			}
			RemoveAndDeleteAll();
		}
	}
}