using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.InvoicingApproval
{
	public class BaseInvoicingFormApprovalGUIProvider :
		TransactionFormApprovalGUIProvider<InvoicingBase, APInvoiceChargesApprovalBulk, APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails>,
		IPostingJobAndTransactionApprovalGUIProvider
	{
		public BaseInvoicingFormApprovalGUIProvider(ZForm parentForm)
		{
			this.parentForm = parentForm;
		}

		public ISecurityOverrideProvider GetNewSecurityOverrideProviderForPosting(InvoicingBase transaction)
		{
			return new InvoicingSecurityOverrideProvider(transaction);
		}

		public ContinueWithSave IsPostingWithInvalidPostingGroupsAllowed(InvoicingBase invoice)
		{
			string message = Res.GetString("0d0123b5-d9ec-4f08-b933-7ea7731917b8", "You have prepared charges using a mix of Tax ID Posting Groups. Are you sure you want to post these charges?");

			if (Globals.Message.Show(message, Res.GetString("ba85116a-4b06-49a3-a59b-7ab5c751270a", "Do you want to continue?"), MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.Yes)
			{
				return ContinueWithSave.No;
			}

			return ContinueWithSave.Yes;
		}

		public void PrintAPInvoiceAndCreditNote(InvoicingBase postedTransaction)
		{
			var printer = postedTransaction is APInvoice ? new InvoicePrinter() : new CreditNotePrinter();
			printer.PrintTransaction(postedTransaction, parentForm, InvoicePrintContext.DontCare, true);
		}

		protected override APInvoiceChargesApprovalBulk GetNewApprovalBulk(BusinessObjectFactory factory, ISecurityOverrideProvider interactiveSecurityOverrideProvider, APInvoiceChargesApprovalRequest approvalRequest)
		{
			return new APInvoiceChargesApprovalBulk(factory, interactiveSecurityOverrideProvider, approvalRequest);
		}

		protected override TransactionApprovalBulkForm<InvoicingBase, APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails> GetApprovalFormToSetDescription(APInvoiceChargesApprovalBulk approvalBulk, TransactionApprovalFormModes actionMode)
		{
			return new APInvoiceChargesApprovalBulkForm(approvalBulk, actionMode);
		}

		protected override ISecurityOverrideProviderWithApprovalRequest GetNewSecurityOverrideProviderCore(bool showApprovalRequestButton, bool alwaysCreateApprovalRequest, bool supportMultipleApprover = false)
		{
			return new InvoicingSecurityOverrideProvider(showApprovalRequestButton, alwaysCreateApprovalRequest);
		}

		string IPostingJobAndTransactionApprovalGUIProvider.PostRequest(APInvoiceChargesApprovalRequest request)
		{
			return PostManagerGUIWrapper.PostRequest(request);
		}

		IApprovalRequestPostingResult IPostingJobAndTransactionApprovalGUIProvider.PostRequestInBulk(APInvoiceChargesApprovalRequest request, IBulkPostingCache bulkPostingCache)
		{
			return PostManagerGUIWrapper.PostRequest(request, bulkPostingCache);
		}

		void IPostingJobAndTransactionApprovalGUIProvider.BulkPrintAPInvoicesAndCreditNotes(ZGuid[] postedTransactionPKs)
		{
			PostManagerGUIWrapper.BulkPrintAPInvoicesAndCreditNotes(postedTransactionPKs);
		}

		void IPostingJobAndTransactionApprovalGUIProvider.PrintAPInvoiceAndCreditNote(ZGuid postedTransactionPK)
		{
			PrintAPInvoiceAndCreditNote(new BusinessObjectFactory().Load<InvoicingBase>(postedTransactionPK));
		}

		protected override ISecurityOverrideProviderWithApprovalRequest GetNewSecurityOverrideProviderCoreForARCreditNote(bool showApprovalRequestButton, bool alwaysCreateApprovalRequest, bool supportMultipleApprover = false, ARCreditNoteApprovalRequest[] approvalRequests = null)
		{
			throw new NotImplementedException();
		}

		readonly ZForm parentForm;
	}
}
