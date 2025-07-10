using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobRevenueJournalReOpenClosedJobDataProvider : IReOpenClosedJobDataProvider
	{
		public JobRevenueJournalReOpenClosedJobDataProvider(JobRevenueJournal jobRevenueJournal)
		{
			JobRevenueJournal = Argument.NotNull(jobRevenueJournal, nameof(jobRevenueJournal));
		}

		readonly JobRevenueJournal JobRevenueJournal;

		BusinessObjectFactory IFactoryProvider.Factory => JobRevenueJournal.Factory;

		string IReOpenClosedJobDataProvider.JobReopenLogText() => string.Format(CultureInfo.InvariantCulture, " - {0} {1}", JobRevenueJournal.AH_Ledger, JobRevenueJournal.AH_TransactionType);

		IReadOnlyCollection<Job> IReOpenClosedJobDataProvider.GetAllJobs()
		{
			var closedJobs = JobRevenueJournal.JournalLines.Cast<JobRevenueJournalLine>()
				.Select(x => (Job)x.Job)
				.Where(job => job != null)
				.ToList();
			return closedJobs;
		}
	}
}
