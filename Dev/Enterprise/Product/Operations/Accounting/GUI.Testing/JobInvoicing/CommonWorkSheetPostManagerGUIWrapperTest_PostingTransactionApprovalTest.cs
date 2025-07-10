using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing.JobInvoicing
{
	public abstract class CommonWorkSheetPostManagerGUIWrapperTest_PostingTransactionApprovalTest : ConsolInvoicingPostManagerGUIWrapper_PostingTransactionApprovalTest
	{
		protected override PostManagerGUIWrapper GetNewGUIWrapperCore(params Job[] jobs)
		{
			var consol = GetConsol(jobs[0]);
			ApportionmentListing apportionments;
			if (!apportionmentsDictionary.TryGetValue(consol.PK, out apportionments))
			{
				apportionments = new ApportionmentListing(Factory, consol);
				apportionmentsDictionary.Add(consol.PK, apportionments);
			}
			return new CommonWorkSheetPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, jobs, consol, null, apportionments);
		}
	}

	[TestedType(typeof(CommonWorkSheetPostManagerGUIWrapper))]
	public class ARCreditNoteCommonWorkSheetPostManagerGUIWrapper_PostingTransactionApprovalTest : CommonWorkSheetPostManagerGUIWrapperTest_PostingTransactionApprovalTest
	{
		protected override bool IsARCreditNoteTesting
		{
			get { return true; }
		}
	}

	[TestedType(typeof(CommonWorkSheetPostManagerGUIWrapper))]
	public class APInvoiceChargesCommonWorkSheetPostManagerGUIWrapper_PostingTransactionApprovalTest : CommonWorkSheetPostManagerGUIWrapperTest_PostingTransactionApprovalTest
	{
		protected override bool IsARCreditNoteTesting
		{
			get { return false; }
		}
	}
}
