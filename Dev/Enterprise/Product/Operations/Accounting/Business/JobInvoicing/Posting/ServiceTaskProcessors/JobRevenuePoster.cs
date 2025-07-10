using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobRevenuePoster : JobPostingWorkflowProcessorSupportingARCreditNoteLevelAuthorization
	{
		public JobRevenuePoster(IJobInvoicingPlugIn plugIn, IPostingJobTransactionsApprovalGUIProvider guiProviderForARCreditNoteLevelAuthorization, ZDateTime invoiceDate, ZDateTime postDate)
			: base(plugIn, guiProviderForARCreditNoteLevelAuthorization, invoiceDate, postDate)
		{
		}

		public JobRevenuePoster(IJobInvoicingPlugIn plugIn)
			: base(plugIn)
		{
		}

		protected override PostManagerValidation CreatePostManagerValidation(IEnumerable<Job> jobs)
		{
			return new PostManagerValidation(jobs, JobInvoicingPostingOption.Revenue, jobs);
		}

		protected override TransactionCreatorHashtable PerformPostCore(InvoicingPostManager postManager)
		{
			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);

			foreach (var invoice in transactions.GetAllARInvoicesAndCreditNotes())
			{
				invoice.OnComplianceSequenceFailedToAssign += invoice_OnComplianceSequenceFailedToAssign;
				invoice.OnDigitalSignatureFailedToSign += invoice_OnDigitalSignatureFailedToSign;
			}

			postManager.PerformTransactionDescriptionDefaulting(plugIn, transactions);
			return transactions;
		}

		protected override bool PerformBackDating(InvoicingPostManager postManager, TransactionCreatorHashtable transactions)
		{
			postManager.PerformTransactionBackDating(plugIn, (s, f, c) => new ChangeTransactionDatesBusinessObject(s, f, c),
				transactions, postManager.Job.Factory);

			return true;
		}

		protected override AccountingEmailDef GetEmailer(Job job)
		{
			return new JobRevenuePosterEmail(job, Notifications.ToMessageListString());
		}

		protected override void AttachEventHandlers(InvoicingPostManager postManager)
		{
			base.AttachEventHandlers(postManager);
			postManager.ChangeTransactionDates += postManager_ChangeTransactionDates;
		}

		protected override void AttachEventHandlersForJob(Job job)
		{
			base.AttachEventHandlersForJob(job);
			job.ShouldUseImmediateRevenueRecognisedDate += job_ShouldUseImmediateRevenueRecognisedDate;
		}

		void postManager_ChangeTransactionDates(object sender, BasePostManager.ChangeTransactionDatesEventArgs e)
		{
			e.Args = new QueryUserMsgBoxEventArgs(ZString.Empty, AccountingConfigurationRegistry.Instance.DefaultAllowUsersToBackDateInvoicesSetting.Value);
			e.TransactionDate = e.ChangeTransactionDatesBusinessObject.InvoiceDate;
			e.PostDate = e.ChangeTransactionDatesBusinessObject.PostDate;
		}

		void job_ShouldUseImmediateRevenueRecognisedDate(object sender, UserQueryEventArgs e)
		{
			e.Response = true;
		}

		void invoice_OnComplianceSequenceFailedToAssign(object sender, EventArgs e)
		{
			new ComplianceNumberAllocationFailureEmail(((InvoicingBase)sender)).Send();
		}

		void invoice_OnDigitalSignatureFailedToSign(object sender, UserMessageEventArgs e)
		{
			new DigitalSignatureSigningFailureEmail(((InvoicingBase)sender), e.Message).Send();
		}
	}
}
