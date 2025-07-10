using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class LocalSisterCompanyChargePoster : JobPostingWorkflowProcessorSupportingARCreditNoteLevelAuthorization
	{
		public LocalSisterCompanyChargePoster(IJobInvoicingPlugIn plugIn, IPostingJobTransactionsApprovalGUIProvider guiProviderForARCreditNoteLevelAuthorization, ZDateTime invoiceDate, ZDateTime postDate)
			: base(plugIn, guiProviderForARCreditNoteLevelAuthorization, invoiceDate, postDate)
		{
		}

		public LocalSisterCompanyChargePoster(IJobInvoicingPlugIn plugIn)
			: base(plugIn)
		{
		}

		protected override PostManagerValidation CreatePostManagerValidation(IEnumerable<Job> jobs)
		{
			return new PostManagerValidation(jobs, JobInvoicingPostingOption.LocalSisterCompanyChargesOnly, jobs);
		}

		protected override TransactionCreatorHashtable PerformPostCore(InvoicingPostManager postManager)
		{
			return postManager.CreateTransactions(JobInvoicingPostingOption.LocalSisterCompanyChargesOnly);
		}

		protected override AccountingEmailDef GetEmailer(Job job)
		{
			return new LocalSisterCompanyChargePosterEmail(job, Notifications.ToMessageListString());
		}
	}
}