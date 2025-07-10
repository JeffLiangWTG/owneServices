using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobCostQueuePoster : JobCostPoster
	{
		public JobCostQueuePoster(IJobInvoicingPlugIn plugIn, IEnumerable<Charge> jobCharges, IEnumerable<IJobChargePostingQueue> queueEntries)
			: base(plugIn)
		{
			this.JobCharges = jobCharges;
			this.QueueEntries = queueEntries;
		}

		protected readonly IEnumerable<Charge> JobCharges;
		protected readonly IEnumerable<IJobChargePostingQueue> QueueEntries;

		protected override InvoicingPostManager CreatePostManager(Job job)
		{
			Predicate<Charge> isEligibleForPosting = x => JobCharges.Select(c => c.PK).Contains(x.PK);
			return new InvoicingPostManager(job, isEligibleForPosting, isEligibleForPosting);
		}

		protected override PostManagerValidation CreatePostManagerValidation(IEnumerable<Job> jobs)
		{
			var jobsWithSelectedCharges = new[] { (jobs.First(), JobCharges) };
			return new JobChargeQueuePostManagerValidation(jobs, JobInvoicingPostingOption.Costs, jobsWithSelectedCharges, QueueEntries);
		}
	}
}
