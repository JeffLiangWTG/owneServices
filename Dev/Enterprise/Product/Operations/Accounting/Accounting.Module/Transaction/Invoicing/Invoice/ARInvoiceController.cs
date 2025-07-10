using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ARInvoiceController : CreditNoteInvoiceController
	{
		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			if (((ARInvoice)inMemorySourceEntity).IsJobRelated)
			{
				Globals.Message.ShowInformation(Res.GetString("e1c877d4-ed7e-48f9-a2fb-4688e2ffa18d", "You cannot copy Job-Related Invoice."), Res.GetString("259c084b-9eb0-45e9-97f5-fb1da2aa8e9b", "Copy Transaction"));
				return null;
			}

			IZForm form = base.ShowTemplateCopyFormFromBase(inMemorySourceEntity);
			if (form != null)
			{
				form.DisplayMode = ODisplayMode.Edit;
			}
			return form;
		}

		protected override void BeforeBaseReversing(IBusiness transaction)
		{
			base.BeforeBaseReversing(transaction);

			var invoice = transaction as ARInvoice;
			if (IsMultipleReversing && invoice != null)
			{
				if (invoice.IsSelfBillingInvoice && MultipleReversingProvider.PaidRelatedSelfBilledInvoicesSecurityCertificate != null)
				{
					invoice.PaidRelatedSelfBilledInvoicesSecurityCertificate = MultipleReversingProvider.PaidRelatedSelfBilledInvoicesSecurityCertificate;
				}
				else if (!invoice.IsSelfBillingInvoice && MultipleReversingProvider.PaidRelatedInvoicesSecurityCertificate != null)
				{
					invoice.PaidRelatedInvoicesSecurityCertificate = MultipleReversingProvider.PaidRelatedInvoicesSecurityCertificate;
				}
			}
		}

		protected override InteractiveSecurityOverrideProvider GetSecurityOverrideProviderForReversing(MultipleReversingProviderForHeader multipleReversingProvider)
		{
			return new ReverseReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideProvider(multipleReversingProvider);
		}

		protected override InteractiveSecurityOverrideProvider GetSecurityOverrideProviderForReversing(InvoicingBase invoicingBase)
		{
			return new ReverseReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideProvider(invoicingBase);
		}

		protected override void AfterBaseReversing(IBusiness transaction)
		{
			base.AfterBaseReversing(transaction);

			if (ApprovalUsersForReverseTransaction != null && Reversing.ReverseTransaction is InvoicingBase reverseInvoice)
			{
				reverseInvoice.ApprovingUserPKList = ApprovalUsersForReverseTransaction;
				reverseInvoice.ApprovalDate = ApprovalDateForReverseTransaction;
			}

			var invoice = transaction as ARInvoice;
			if (IsMultipleReversing && invoice != null)
			{
				if (invoice.IsSelfBillingInvoice && MultipleReversingProvider.PaidRelatedSelfBilledInvoicesSecurityCertificate == null &&
				invoice.PaidRelatedSelfBilledInvoicesSecurityCertificate != null)
				{
					MultipleReversingProvider.PaidRelatedSelfBilledInvoicesSecurityCertificate = invoice.PaidRelatedSelfBilledInvoicesSecurityCertificate;
				}
				else if (!invoice.IsSelfBillingInvoice && MultipleReversingProvider.PaidRelatedInvoicesSecurityCertificate == null &&
				invoice.PaidRelatedInvoicesSecurityCertificate != null)
				{
					MultipleReversingProvider.PaidRelatedInvoicesSecurityCertificate = invoice.PaidRelatedInvoicesSecurityCertificate;
				}
			}
		}

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			IZForm form;

			if (Reversing != null)
			{
				ARCreditNote aRCreditNote = businessEntity as ARCreditNote;
				form = GetNewCreditNoteForm(aRCreditNote);
			}
			else
			{
				ARInvoice aRInvoice = businessEntity as ARInvoice;
				aRInvoice.SubmittedFromInvoicingForm = true;
				form = GetNewInvoiceForm(aRInvoice);
			}
			return form;
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.ARInvoice; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ARInvoice); }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReverseReceivablesInvoice; }
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			var result = Env.Security.None;
			var arInvoice = bizObject as ARInvoice;
			if (arInvoice != null)
			{
				result = arInvoice.IsSelfBillingInvoice ? Env.Security.ReverseReceivablesSelfBilledInvoice : Env.Security.ReverseReceivablesInvoice;
			}
			return result;
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewReceivablesInvoice; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ViewReceivablesTransaction; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ViewReceivablesTransaction; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ARTransaction; }
		}

		protected override string AmendSecurityCheckPointCode
		{
			get { return SecurityCore.AmendTransactionWInvoice; }
		}
	}
}
