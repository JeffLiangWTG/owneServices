using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APInvoiceFromDraftInvoiceController))]
	class APInvoiceFromDraftInvoiceControllerTest : APInvoicingBaseFromDraftInvoiceControllerTest<APInvoice>
	{
		public override Type ControllerToBashType => typeof(APInvoiceFromDraftInvoiceController);

		protected override string TransactionType => TransactionTypes.Invoice;

		protected override ControllerID GetControllerID() => ControllerIDs.APInvoiceFromDraftInvoice;

		protected override ControllerID ExpectedFormControllerID => ControllerIDs.APInvoice;

		protected override string EditFormCaption => "New AP Invoice";

		protected override bool ExpectedShouldShowOriginalInvoiceReferenceFields => false;
	}
}
