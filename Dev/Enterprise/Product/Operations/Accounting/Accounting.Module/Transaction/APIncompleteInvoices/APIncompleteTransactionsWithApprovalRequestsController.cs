using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module.Transaction
{
	public abstract class APIncompleteTransactionsWithApprovalRequestsController : APIncompleteTransactionsController
	{
		protected override SecurityCheckpoint GetEditCheckPointForDirectEnteredTransaction(InvoicingBase invoice) =>
			invoice.HasApprovalRequest ? GetSecurityCheckpointForAPInvoiceChargesApprovalRequestIfApprovedForPostingWithFallbackToEditCheckpoint(invoice, Env.Security.APInvoiceApproval_Edit_DirectEntered) : Env.Security.APIncompleteInvoicesEdit_DirectEntered;

		protected override SecurityCheckpoint GetEditCheckPointForUniveralXMLImportedTransaction(InvoicingBase invoice) =>
			invoice.HasApprovalRequest ? GetSecurityCheckpointForAPInvoiceChargesApprovalRequestIfApprovedForPostingWithFallbackToEditCheckpoint(invoice, Env.Security.APInvoiceApproval_Edit_ImportSourced) : Env.Security.APIncompleteInvoicesEdit_ImportSourced;

		protected override SecurityCheckpoint GetEditHeaderCheckPointForDirectEnteredTransaction(InvoicingBase invoice) =>
			invoice.HasApprovalRequest ? GetSecurityCheckpointForAPInvoiceChargesApprovalRequestIfApprovedForPostingWithFallbackToEditCheckpoint(invoice, Env.Security.APInvoiceApproval_Edit_DirectEntered_EditInvoiceHeader) : Env.Security.APIncompleteInvoicesEdit_DirectEntered_EditInvoiceHeader;

		protected override SecurityCheckpoint GetEditHeaderCheckPointForUniveralXMLImportedTransaction(InvoicingBase invoice) =>
			invoice.HasApprovalRequest ? GetSecurityCheckpointForAPInvoiceChargesApprovalRequestIfApprovedForPostingWithFallbackToEditCheckpoint(invoice, Env.Security.APInvoiceApproval_Edit_ImportSourced_EditInvoiceHeader) : Env.Security.APIncompleteInvoicesEdit_ImportSourced_EditInvoiceHeader;

		SecurityCheckpoint GetSecurityCheckpointForAPInvoiceChargesApprovalRequestIfApprovedForPostingWithFallbackToEditCheckpoint(InvoicingBase invoice, SecurityCheckpoint editCheckPointIfNotPosting)
		{
			return invoice.Factory.HasContext(APInvoiceChargesApprovalRequest.Context.Posting) ? Env.Security.APInvoiceApproval_Post : editCheckPointIfNotPosting;
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			IZForm result = null;
			var invoice = sourceEntity as InvoicingBase;
			if (invoice != null && invoice.HasApprovalRequest)
			{
				var newFactory = new BusinessObjectFactory();
				var selectedInvoiceInNewFactory = newFactory.Load<InvoicingBase>(invoice.PK);
				if (selectedInvoiceInNewFactory != null)
				{
					var selectedRequestInNewFactory = selectedInvoiceInNewFactory.APInvoiceTransactionRelatedApprovalRequest;
					if (selectedRequestInNewFactory != null)
					{
						var (invoiceToEdit, restoreSavedDataResult) = selectedRequestInNewFactory.GetLinkedInvoice();
						if (restoreSavedDataResult != null && restoreSavedDataResult.Result != InvoicingBase.RestoreSavedDataResult.ResultType.Success)
						{
							Globals.Message.ShowError(Res.GetString("35B01D5A-029A-4E73-B9D7-D4EF7F0622E2", "Request ({0}): {1}.", selectedRequestInNewFactory.FormatedRequestId, restoreSavedDataResult.Error));
						}
						if (invoiceToEdit == null)
						{
							Globals.Message.ShowError(Res.GetString("3807B7C5-EA20-455B-8E96-0701B6953A25", "A transaction can't be found for request ({0}).", selectedRequestInNewFactory.FormatedRequestId));
						}
						else
						{
							newFactory.SetContext(APInvoiceChargesApprovalRequest.Context.Editing);
							result = base.ShowViewForm(invoiceToEdit);
						}
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("6848078e-c847-4f97-aeb3-b5dfc425703f", "Approval request is not found."));
					}
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("04be2d73-c812-48bf-8a22-6539b3ca67e7", "Selected invoice is not found."));
				}
			}
			else
			{
				result = base.ShowViewForm(sourceEntity);
			}

			return result;
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			IZForm result = null;

			var invoice = sourceEntity as InvoicingBase;
			if (invoice != null && invoice.HasApprovalRequest)
			{
				var newFactory = new BusinessObjectFactory();
				var selectedInvoiceInNewFactory = newFactory.Load<InvoicingBase>(invoice.PK);
				if (selectedInvoiceInNewFactory != null)
				{
					var selectedRequestInNewFactory = selectedInvoiceInNewFactory.APInvoiceTransactionRelatedApprovalRequest;
					if (selectedRequestInNewFactory != null)
					{
						var (invoiceToEdit, restoreSavedDataResult) = selectedRequestInNewFactory.GetLinkedInvoice();
						if (restoreSavedDataResult != null && restoreSavedDataResult.Result != InvoicingBase.RestoreSavedDataResult.ResultType.Success)
						{
							Globals.Message.ShowError(Res.GetString("35B01D5A-029A-4E73-B9D7-D4EF7F0622E2", "Request ({0}): {1}.", selectedRequestInNewFactory.FormatedRequestId, restoreSavedDataResult.Error));
						}
						if (invoiceToEdit == null)
						{
							Globals.Message.ShowError(Res.GetString("3807B7C5-EA20-455B-8E96-0701B6953A25", "A transaction can't be found for request ({0}).", selectedRequestInNewFactory.FormatedRequestId));
						}
						else
						{
							newFactory.SetContext(selectedRequestInNewFactory.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Approved ? APInvoiceChargesApprovalRequest.Context.Posting : APInvoiceChargesApprovalRequest.Context.Editing);
							result = base.ShowEditForm(invoiceToEdit);
						}
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("6848078e-c847-4f97-aeb3-b5dfc425703f", "Approval request is not found."));
					}
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("04be2d73-c812-48bf-8a22-6539b3ca67e7", "Selected invoice is not found."));
				}
			}
			else
			{
				result = base.ShowEditForm(sourceEntity);
			}

			return result;
		}

		protected override IZForm ShowCancelFormCore(BusinessObject sourceEntity)
		{
			IZForm result = null;
			var invoice = sourceEntity as InvoicingBase;
			if (invoice != null && invoice.HasApprovalRequest)
			{
				Globals.Message.ShowError(Res.GetString("d402f69a-2735-4176-83be-bd61c667157d", "To cancel invoice with approval request you should cancel its approval request in Invoice Approval module."));
			}
			else
			{
				result = base.ShowCancelFormCore(sourceEntity);
			}

			return result;
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			AccountingZForm result = null;
			var invoice = sourceEntity as InvoicingBase;
			if (invoice != null && invoice.HasApprovalRequest)
			{
				Globals.Message.ShowError(Res.GetString("6c517fe0-970b-4346-bafd-c79a74ed61f6", "Invoice with approval request can't be deleted."));
			}
			else
			{
				result = (AccountingZForm)base.ShowDeleteForm(sourceEntity);
			}

			return result;
		}
	}
}
