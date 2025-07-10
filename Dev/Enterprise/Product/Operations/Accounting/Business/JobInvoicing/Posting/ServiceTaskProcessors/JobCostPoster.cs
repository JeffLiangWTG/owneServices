using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobCostPoster : JobPostingWorkflowProcessor
	{
		public JobCostPoster(IJobInvoicingPlugIn plugIn)
			: base(plugIn)
		{
		}

		protected override PostManagerValidation CreatePostManagerValidation(IEnumerable<Job> jobs)
		{
			return new PostManagerValidation(jobs, JobInvoicingPostingOption.Costs, jobs);
		}

		protected override bool PerformPost(InvoicingPostManager postManager)
		{
			bool result = false;
			postManager.CreateTransactions(JobInvoicingPostingOption.Costs);

			if (!postManager.CancelPosting)
			{
				result = true;
			}
			return result;
		}

		protected override AccountingEmailDef GetEmailer(Job job)
		{
			return new JobCostPosterEmail(job, Notifications.ToMessageListString());
		}
	}
}
