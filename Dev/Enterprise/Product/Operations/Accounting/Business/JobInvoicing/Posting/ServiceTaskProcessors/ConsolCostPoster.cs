using System.Collections.Generic;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ConsolCostPoster : ConsolPostingWorkflowProcessor
	{
		public ConsolCostPoster(IJobInvoicingPlugIn plugIn)
			: base(plugIn)
		{
		}

		protected override ConsolPostManagerValidation CreatePostManagerValidation(IEnumerable<Job> jobs)
		{
			return new ConsolPostManagerValidation(jobs, Consol, JobInvoicingPostingOption.Costs, jobs);
		}

		protected override TransactionCreatorHashtable CreateTransacions(ConsolInvoicingPostManager postManager)
		{
			return postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
		}

		protected override AccountingEmailDef GetEmailer(IJobCostingPlugIn consol, string errors)
		{
			return new ConsolCostPosterEmail(consol, errors);
		}
	}
}