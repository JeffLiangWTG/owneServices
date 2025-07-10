using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	#region Job Invoicing Payment Approval Matcher

	public class JobInvoicingPaymentApprovalMatcher : BaseTransactionCreator
	{
		public JobInvoicingPaymentApprovalMatcher(Job job, ZDateTime postingTime)
			: base(job, null, false)
		{
			this.PostingTime = postingTime;
		}

		protected override bool IsChargeApplicable(Charge charge)
		{
			return charge.Cost != null && !charge.Cost.IsInDatabase;
		}

		protected override bool CreateTransactionsCore(TransactionCreatorHashtable transactions, bool isMultiJobOperationInProgress)
		{
			bool result = false;

			foreach (Charge jobCharge in Charges)
			{
				PaymentApprovalBase paymentApproval = RetrievePaymentApproval(transactions, jobCharge);
				APInvoice invoice = RetrieveInvoice(transactions, jobCharge);

				if (paymentApproval != null && invoice != null)
				{
					invoice.GetPaymentApprovalItem(paymentApproval);
					result = true;
				}
			}

			return result;
		}

		protected APInvoice RetrieveInvoice(TransactionCreatorHashtable transactions, Charge charge)
		{
			string clientCode = charge.CostAccount != null ? charge.CostAccount.OH_Code : ZString.Empty;
			string aPInvoiceNumber = charge.JR_APInvoiceNum;

			return transactions.RetrieveAPInvoice(clientCode, aPInvoiceNumber);
		}

		protected PaymentApprovalBase RetrievePaymentApproval(TransactionCreatorHashtable transactions, Charge charge)
		{
			string clientCode = charge.CostAccount != null ? charge.CostAccount.OH_Code : ZString.Empty;
			string bankAccountCode = charge.BankAccount != null ? charge.BankAccount.AB_Code : ZString.Empty;
			string receiptType = charge.JR_PaymentType;
			string chequeOrReferences = (charge.JR_ChequeNo.IsEmpty && charge.ChequeBook != null) ? charge.ChequeBook.AK_Code : charge.JR_ChequeNo;
			string jobNumber = !charge.JR_IsApportioned ? Job.JH_JobNum : ZString.Empty;

			return transactions.RetrieveAPPaymentApproval(clientCode, bankAccountCode, receiptType, chequeOrReferences, jobNumber);
		}
	}

	#endregion

	#region Consol Payment Approval Matcher

	public class ConsolInvoicingPaymentApprovalMatcher
	{
		public ConsolInvoicingPaymentApprovalMatcher(BusinessObjectFactory factory, IEnumerable<Job> jobs, ZDateTime postingTime)
		{
			this.Factory = factory;
			this.Jobs = jobs;
			this.PostingTime = postingTime;
		}
		protected BusinessObjectFactory Factory;
		protected IEnumerable<Job> Jobs;
		protected ZDateTime PostingTime;

		public void Match(TransactionCreatorHashtable transactions)
		{
			foreach (Job job in Jobs)
			{
				if (!job.IsWorkOnHold)
				{
					JobInvoicingPaymentApprovalMatcher approvalMatcher = new JobInvoicingPaymentApprovalMatcher(job, PostingTime);
					approvalMatcher.CreateTransactions(transactions);
				}
			}
		}
	}

	#endregion
}
