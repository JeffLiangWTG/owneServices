using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class UACreditNoteController : CreditNoteInvoiceController
	{
		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.APUnapprovedInvoicesCancel; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.APUnapprovedInvoicesNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.APUnapprovedInvoices; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.UACreditNote; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(UACreditNote); }
		}

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			APCreditNote aPCreditNote = businessEntity as APCreditNote;
			aPCreditNote.SubmittedFromInvoicingForm = true;
			return GetNewCreditNoteForm((CreditNote)businessEntity);
		}

		protected override CreditNoteForm GetNewCreditNoteForm(CreditNote creditNote)
		{
			return new UACreditNoteForm(creditNote);
		}

		protected override bool ShouldHaveReversedBizo
		{
			get { return false; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
