using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Transaction.Testing
{
	[TestedType(typeof(APIncompleteAdjustmentNotesController))]
	public class APIncompleteAdjustmentNotesControllerTest : APIncompleteTransactionsControllerTest<APAdjustmentNote, AdjustmentNoteForm>
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.APIncompleteAdjustmentNote;
		}

		protected override ControllerID GetRelativeControllerID()
		{
			return ControllerIDs.APAdjustmentNote;
		}

		protected override void AssertOtherGetValidControllerIDCase(INavigationControllerIDProvider controller)
		{
			base.AssertOtherGetValidControllerIDCase(controller);

			var creditNote = Factory.NewWithValidTestData<APCreditNote>();

			AssertEquals("ControllerID for AP Credit Note bizO should be APCreditNote.", ControllerIDs.APCreditNote, controller.GetValidControllerID(creditNote));
		}

		protected override InvoicingBase TransactionImportedFromUniversalXML => null;
	}
}
