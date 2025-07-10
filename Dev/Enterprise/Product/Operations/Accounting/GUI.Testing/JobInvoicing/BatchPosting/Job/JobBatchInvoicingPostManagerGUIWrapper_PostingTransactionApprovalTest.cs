using System.Linq;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.BatchPosting.Testing
{
	abstract class JobBatchInvoicingPostManagerGUIWrapper_PostingTransactionApprovalTest : BaseBatchInvoicingPostManagerGUIWrapperTest_PostingTransactionApprovalTest
	{
		protected override BaseBatchInvoicingPostManagerGUIWrapper GetNewBatchGUIWrapper(params Job[] jobs)
		{
			return new JobBatchInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, jobs.Select(x => x.PlugInData).ToArray(), "Selected Posting Option Name", "Posting Object Name");
		}
	}

	[TestedType(typeof(JobBatchInvoicingPostManagerGUIWrapper))]
	class ARCreditNoteJobBatchInvoicingPostManagerGUIWrapper_PostingTransactionApprovalTest : JobBatchInvoicingPostManagerGUIWrapper_PostingTransactionApprovalTest
	{
		protected override bool IsARCreditNoteTesting
		{
			get { return true; }
		}
	}

	[TestedType(typeof(JobBatchInvoicingPostManagerGUIWrapper))]
	class APInvoiceChargesJobBatchInvoicingPostManagerGUIWrapper_PostingTransactionApprovalTest : JobBatchInvoicingPostManagerGUIWrapper_PostingTransactionApprovalTest
	{
		protected override bool IsARCreditNoteTesting
		{
			get { return false; }
		}
	}
}
