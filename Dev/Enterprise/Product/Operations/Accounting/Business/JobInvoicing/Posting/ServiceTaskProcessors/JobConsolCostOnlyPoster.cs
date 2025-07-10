using System.Collections.Generic;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	class JobConsolCostOnlyPoster : ConsolPostingWorkflowProcessor
	{
		public JobConsolCostOnlyPoster(IJobInvoicingPlugIn plugIn)
			: base(plugIn)
		{
		}

		protected override ConsolPostManagerValidation CreatePostManagerValidation(IEnumerable<Job> jobs)
		{
			return new ConsolPostManagerValidation(jobs, Consol, JobInvoicingPostingOption.ConsolCosts, jobs);
		}

		protected override TransactionCreatorHashtable CreateTransacions(ConsolInvoicingPostManager postManager)
		{
			return postManager.CreateTransactions(JobInvoicingPostingOption.ConsolCosts);
		}

		protected override AccountingEmailDef GetEmailer(IJobCostingPlugIn consol, string errors)
		{
			return new JobConsolCostOnlyPosterEmail(consol, errors);
		}
	}
}