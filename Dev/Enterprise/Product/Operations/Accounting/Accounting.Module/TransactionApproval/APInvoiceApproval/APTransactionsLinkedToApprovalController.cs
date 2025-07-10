using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Module.Transaction;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	public abstract class APTransactionsLinkedToApprovalController : APIncompleteTransactionsController
	{
		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.APInvoiceApproval_Cancel; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.APInvoiceApproval; }
		}

		protected override SecurityCheckpoint CheckPointForCancel
		{
			get { return Env.Security.APInvoiceApproval_Cancel; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.APInvoiceApproval; }
		}

		protected override SecurityCheckpoint GetEditCheckPointForDirectEnteredTransaction(InvoicingBase invoice) => Env.Security.APInvoiceApproval_Edit_DirectEntered;

		protected override SecurityCheckpoint GetEditCheckPointForUniveralXMLImportedTransaction(InvoicingBase invoice) => Env.Security.APInvoiceApproval_Edit_ImportSourced;

		protected override SecurityCheckpoint GetEditHeaderCheckPointForDirectEnteredTransaction(InvoicingBase invoice) => Env.Security.APInvoiceApproval_Edit_DirectEntered_EditInvoiceHeader;

		protected override SecurityCheckpoint GetEditHeaderCheckPointForUniveralXMLImportedTransaction(InvoicingBase invoice) => Env.Security.APInvoiceApproval_Edit_ImportSourced_EditInvoiceHeader;
	}
}
