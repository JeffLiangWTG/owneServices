using System.Collections.Generic;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.TransactionApproval.Testing
{
	[TestedType(typeof(APCreditNoteLinkedToApprovalController))]
	public class APCreditNoteLinkedToApprovalControllerTest : APTransactionsLinkedToApprovalControllerTest<APCreditNote, CreditNoteForm>
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.APCreditNoteLinkedToApproval;
		}

		protected override ControllerID GetRelativeControllerID()
		{
			return ControllerIDs.APIncompleteCreditNote;
		}

		protected override APCreditNote GetBusinessObject()
		{
			return TestObjectCreator.CreateAPInvoiceWithApprovalRequest<APCreditNote>(TestObjectCreator.Creditor1, 100);
		}

		protected override ControllerID ReportInvalidDataSource_CorrectControllerID => ControllerIDs.APCreditNote;

		protected override ZController GetController() => Controller as APCreditNoteLinkedToApprovalController;

		protected override IEnumerable<ControllerID> EditFormControllerIDs => new[] { ControllerIDs.APIncompleteCreditNote, ControllerIDs.APCreditNoteLinkedToApproval };

		protected override ControllerID NewFormControllerID => ControllerIDs.APCreditNoteNewForApproval;
	}
}
