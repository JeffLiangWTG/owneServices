
using CargoWise.Types;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	public class AgentInvoiceCreationPostingChargeCollection : IReceivablesPostingChargeCollection
	{
		#region InvoicePostingBranch

		public override ZGuid InvoicePostingBranch
		{
			get { return fInvoicePostingBranch; }
		}

		public void SetInvoicePostingBranch(ZGuid branch)
		{
			fInvoicePostingBranch = branch;
		}

		#endregion

		#region InvoicePostingDepartment

		public override ZGuid InvoicePostingDepartment
		{
			get { return fInvoicePostingDepartment; }
		}

		public void SetInvoicePostingDepartment(ZGuid postingDept)
		{
			fInvoicePostingDepartment = postingDept;
		}

		#endregion

		#region JobPK

		public override ZGuid JobPK
		{
			get { return fJobPK; }
		}

		public void SetJobPK(ZGuid jobPK)
		{
			fJobPK = jobPK;
		}

		#endregion

		#region Debtor

		public override ZGuid Debtor
		{
			get { return fDebtor; }
		}

		public void SetDebtor(ZGuid debtor)
		{
			fDebtor = debtor;
		}

		public override ZGuid DebtorAddress
		{
			get { return ZGuid.Empty; }
		}

		public override ZGuid DebtorContact
		{
			get { return ZGuid.Empty; }
		}

		#endregion

		#region JobNumber

		public override ZString JobNumber
		{
			get { return fJobNumber; }
		}

		public void SetJobNumber(ZString jobNumber)
		{
			fJobNumber = jobNumber;
		}

		#endregion

		#region IsBillInLocalCurrency

		public override ZBool IsBillInLocalCurrency
		{
			get { return fIsBillInLocalCurrency; }
		}

		public void SetIsBillInLocalCurrency(ZBool billInLocal)
		{
			fIsBillInLocalCurrency = billInLocal;
		}

		#endregion

		#region PostingJob

		public override IPostingJob Job
		{
			get { return fJob; }
		}

		public void SetPostingJob(IPostingJob job)
		{
			fJob = job;
		}

		#endregion

		#region Implementation

		ZGuid fDebtor;
		ZGuid fInvoicePostingDepartment;
		ZGuid fInvoicePostingBranch;
		ZGuid fJobPK;
		ZString fJobNumber;
		ZBool fIsBillInLocalCurrency;
		IPostingJob fJob;

		#endregion

	}
}
