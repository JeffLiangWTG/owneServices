using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APCreditNoteFromDraftInvoiceController))]
	class APCreditNoteFromDraftInvoiceControllerTest : APInvoicingBaseFromDraftInvoiceControllerTest<APCreditNote>
	{
		public override Type ControllerToBashType => typeof(APCreditNoteFromDraftInvoiceController);

		protected override ControllerID GetControllerID() => ControllerIDs.APCreditNoteFromDraftInvoice;

		protected override ControllerID ExpectedFormControllerID => ControllerIDs.APCreditNote;

		protected override string TransactionType => TransactionTypes.CreditNote;

		protected override string EditFormCaption => "New AP Credit Note";

		protected override bool ExpectedShouldShowOriginalInvoiceReferenceFields => true;
	}
}
