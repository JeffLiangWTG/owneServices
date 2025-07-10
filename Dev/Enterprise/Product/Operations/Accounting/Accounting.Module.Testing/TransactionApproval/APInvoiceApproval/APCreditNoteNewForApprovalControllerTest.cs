using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Module.Testing;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.TransactionApproval.Testing
{
	[TestedType(typeof(APCreditNoteNewForApprovalController))]
	class APCreditNoteNewForApprovalControllerTest : APCreditNoteControllerTest
	{
		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.APInvoiceApproval_Cancel; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.APInvoiceApproval_NewCreditNote; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.APInvoiceApproval; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForEdit
		{
			get { return Env.Security.APInvoiceApproval_Edit; }
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.APCreditNoteNewForApproval;
		}

		public override void TestGetFormWithAPIncompletCreditNote()
		{
			var transaction = Factory.NewWithValidTestData<APCreditNote>();
			transaction.SaveAsIncomplete();
			Factory.Save();
			AssertEquals("IN", transaction.AH_Ledger);
			AssertEquals("INC", transaction.AH_TransactionType);

			var controller = Controller as APCreditNoteNewForApprovalController;
			using (var form = controller.GetForm_ForTest(transaction))
			{
				AssertEquals("Should not have any developer notification exception.", 0, ExceptionReporterTestListener.Instance.Count);
			}
		}

		protected override InvoicingBase ChangeStateOfParentTransactionHeaderRow()
		{
			var transaction = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			Factory.Save();
			return transaction;
		}
	}
}
