using System.Collections.Generic;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Transaction.Testing
{
	[TestedType(typeof(APIncompleteCreditNotesController))]
	public class APIncompleteCreditNotesControllerTest : APIncompleteTransactionsWithApprovalRequestsControllerTest<APCreditNote, CreditNoteForm>
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.APIncompleteCreditNote;
		}

		protected override ControllerID GetRelativeControllerID()
		{
			return ControllerIDs.APCreditNote;
		}

		protected override string ExpectedTransactionType
		{
			get { return "AP Credit Note"; }
		}

		protected override void AssertOtherGetValidControllerIDCase(INavigationControllerIDProvider controller)
		{
			base.AssertOtherGetValidControllerIDCase(controller);

			var invoice = Factory.NewWithValidTestData<APInvoice>();

			AssertEquals("ControllerID for AP Invoice bizO should be APInvoice.", ControllerIDs.APInvoice, controller.GetValidControllerID(invoice));
		}

		protected override ZController GetController() => Controller as APIncompleteCreditNotesController;

		protected override IEnumerable<ControllerID> EditFormControllerIDs => new[] { ControllerIDs.APIncompleteCreditNote, ControllerIDs.APCreditNoteLinkedToApproval };

		protected override ControllerID NewFormControllerID => ControllerIDs.APCreditNoteNewForApproval;
	}
}
