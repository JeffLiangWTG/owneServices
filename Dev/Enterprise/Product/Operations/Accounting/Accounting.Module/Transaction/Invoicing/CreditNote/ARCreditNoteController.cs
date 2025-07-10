using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ARCreditNoteController : CreditNoteInvoiceController
	{
		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			IZForm form;

			if (Reversing != null)
			{
				ARInvoice aRInvoice = businessEntity as ARInvoice;
				form = GetNewInvoiceForm(aRInvoice);
			}
			else
			{
				ARCreditNote aRCreditNote = businessEntity as ARCreditNote;
				aRCreditNote.SubmittedFromInvoicingForm = true;
				form = GetNewCreditNoteForm(aRCreditNote);
			}
			return form;
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			IZForm form = null;
			if (businessEntity is ARCreditNote creditNote && AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(creditNote.AH_Ledger, creditNote.AH_GC))
			{
				Globals.Message.Show(AccountingMasterFilesUtils.ARCreditNoteDisallowedMessage);
			}
			else
			{
				form = base.ShowFormForNewEntityCore(businessEntity);
			}
			return form;
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.ARCreditNote; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ARCreditNote); }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReverseReceivablesCreditNote; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewReceivablesCreditNote; }
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
			get
			{
				return ModuleIDs.ARTransaction;
			}
		}

		protected override string AmendSecurityCheckPointCode
		{
			get { return SecurityCore.AmendTransactionWCreditNote; }
		}
	}
}
