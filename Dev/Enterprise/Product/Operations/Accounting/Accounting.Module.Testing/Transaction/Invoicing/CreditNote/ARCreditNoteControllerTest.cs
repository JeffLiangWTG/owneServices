using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARCreditNoteController))]
	public class ARCreditNoteControllerTest : CreditNoteInvoiceControllerTestCase
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ARCreditNote;
		}

		protected override Type GetExpectedBusinessObjectType()
		{
			return typeof(ARCreditNote);
		}

		protected override Type GetExpectedFormType()
		{
			return typeof(GUI.CreditNoteForm);
		}

		protected override IDisposable PreventCreationOfCreditNotesRegistryConfiguration => AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.ReverseReceivablesCreditNote; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.NewReceivablesCreditNote; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.ViewReceivablesTransaction; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForEdit
		{
			get { return Env.Security.ViewReceivablesTransaction; }
		}

		protected override CreditNoteInvoiceController GetController()
		{
			return new ARCreditNoteController();
		}

		protected override bool IsNewAllowed
		{
			get { return Env.Security.NewReceivablesCreditNote.IsAllowed; }
			set { Env.Security.NewReceivablesCreditNote.IsAllowed = value; }
		}
	}
}
