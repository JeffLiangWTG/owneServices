using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobOverseasAgentChargesPoster : JobPostingWorkflowProcessorSupportingARCreditNoteLevelAuthorization
	{
		public JobOverseasAgentChargesPoster(IJobInvoicingPlugIn plugIn, IPostingJobTransactionsApprovalGUIProvider guiProviderForARCreditNoteLevelAuthorization, ZDateTime invoiceDate, ZDateTime postDate)
			: base(plugIn, guiProviderForARCreditNoteLevelAuthorization, invoiceDate, postDate)
		{
		}

		public JobOverseasAgentChargesPoster(IJobInvoicingPlugIn plugIn)
			: base(plugIn)
		{
		}

		protected override PostManagerValidation CreatePostManagerValidation(IEnumerable<Job> jobs)
		{
			return new PostManagerValidation(jobs, JobInvoicingPostingOption.Agent, jobs);
		}

		protected override TransactionCreatorHashtable PerformPostCore(InvoicingPostManager postManager)
		{
			return postManager.CreateTransactions(JobInvoicingPostingOption.Agent);
		}

		protected override AccountingEmailDef GetEmailer(Job job)
		{
			return new JobOverseasAgentChargesPosterEmail(job, Notifications.ToMessageListString());
		}
	}
}
