using System;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public abstract class GLOWInvoicingPostManagerGUIWrapper_PostingTransactionApprovalTest : PostManagerGUIWrapper_PostingTransactionApprovalTest
	{
		protected override PostManagerGUIWrapper GetNewGUIWrapperCore(params Job[] jobs)
		{
			return new GLOWInvoicingPostManagerGUIWrapper(jobs[0].PK.ToGuid(), JobInvoicingPostingOption.All, false, new DateTime(), new DateTime(), Factory);
		}

		protected override bool IsShowWarningsSupported
		{
			get { return false; }
		}
	}

	[TestedType(typeof(GLOWInvoicingPostManagerGUIWrapper))]
	public class ARCreditNoteGLOWInvoicingPostManagerGUIWrapper_PostingTransactionApprovalTest : GLOWInvoicingPostManagerGUIWrapper_PostingTransactionApprovalTest
	{
		protected override bool IsARCreditNoteTesting
		{
			get { return true; }
		}
	}

	[TestedType(typeof(GLOWInvoicingPostManagerGUIWrapper))]
	public class APInvoiceChargesGLOWInvoicingPostManagerGUIWrapper_PostingTransactionApprovalTest : GLOWInvoicingPostManagerGUIWrapper_PostingTransactionApprovalTest
	{
		protected override bool IsARCreditNoteTesting
		{
			get { return false; }
		}
	}
}
