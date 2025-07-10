using Enterprise.Accounting.Business.AccountingPresentationProviders;
using Enterprise.Accounting.Business.JobInvoicing;

namespace Enterprise.Accounting.GUI
{
	public static class JobRevenueJournalFormFactory
	{
		public static JobRevenueJournalForm GetJobRevenueJournalForm(JobRevenueJournal jobRevenueJournal)
		{
			IReOpenClosedJobDataProvider reOpenClosedJobDataProvider = new JobRevenueJournalReOpenClosedJobDataProvider(jobRevenueJournal);
			IReopenClosedJobSecurityOverrideProvider reopenClosedJobSecurityOverrideProvider = new ReopenClosedJobSecurityOverrideProvider();

			IClosedJobReopener closedJobReopener = new ClosedJobReopener(reOpenClosedJobDataProvider, reopenClosedJobSecurityOverrideProvider);

			jobRevenueJournal.SetupDependencies(closedJobReopener);
			return new JobRevenueJournalForm(jobRevenueJournal, new JobRevenueJournalFormPresentationProvider(closedJobReopener));
		}
	}
}
