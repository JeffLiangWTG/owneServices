using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APCreditNoteController))]
	class APCreditNoteControllerTest : CreditNoteInvoiceControllerTestCase
	{
		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.ReversePayablesCreditNote; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.NewPayablesCreditNote; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.ViewPayablesTransaction; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForEdit
		{
			get { return Env.Security.ViewPayablesTransaction; }
		}

		protected override InvoicingBase ChangeStateOfParentTransactionHeaderRow()
		{
			var transaction = Factory.NewWithValidTestData<APCreditNote>();
			transaction.SaveAsIncomplete();
			return transaction;
		}

		protected override string MessageToShowWhenControllerMismatchForBusinessEntity => "The AP credit note cannot be displayed because another user has changed or deleted the record. Closing and re-opening this window will refresh your data.";

		public void TestGetCheckPointForNew()
		{
			AssertEquals("Should be NewPayablesCreditNote", ExpectedCheckPointForNew, Controller.GetCheckPointForNew(this.GetFormBusinessEntity()));

			InvoicingBase approval = Factory.NewWithValidTestData<UACreditNote>();
			Factory.Save();
			approval.AH_Ledger = LedgerTypes.AccountsPayable;
			approval.AH_TransactionType = TransactionTypes.CreditNote;
			approval = Factory.Load<APCreditNote>(approval.PK);

			AssertEquals("Should be None", Env.Security.None, Controller.GetCheckPointForNew(approval));
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.APCreditNote;
		}

		protected override Type GetExpectedBusinessObjectType()
		{
			return typeof(APCreditNote);
		}

		protected override Type GetExpectedFormType()
		{
			return typeof(GUI.CreditNoteForm);
		}

		protected override IDisposable PreventCreationOfCreditNotesRegistryConfiguration => AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

		#region INavigationControllerIDProvider

		public void TestGetValidControllerID()
		{
			var controller = Controller as INavigationControllerIDProvider;
			var nonRelatedBizO = Factory.NewWithValidTestData<ARInvoice>();
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			var creditNote = Factory.NewWithValidTestData<APCreditNote>();
			var incompleteCreditNote = Factory.NewWithValidTestData<APCreditNote>();
			incompleteCreditNote.SaveAsIncomplete();
			var creditNotePending = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			creditNotePending.AH_TransactionType = TransactionTypes.CreditNotePendingAllocation;

			AssertEquals(string.Format("ControllerID for non-related bizO should be {0}.", Controller.ID), Controller.ID, controller.GetValidControllerID(nonRelatedBizO));
			AssertEquals("ControllerID for AP Invoice bizO should be APInvoice.", ControllerIDs.APInvoice, controller.GetValidControllerID(invoice));
			AssertEquals("ControllerID for AP Credit Note bizO should be APCreditNote.", ControllerIDs.APCreditNote, controller.GetValidControllerID(creditNote));
			AssertEquals("ControllerID for AP Incomplete Credit Note bizO should be APIncompleteCreditNote.", ControllerIDs.APIncompleteCreditNote, controller.GetValidControllerID(incompleteCreditNote));
			AssertEquals("ControllerID for Transaction Pending Allocation bizO should be TransactionsPendingAllocation.", ControllerIDs.TransactionsPendingAllocation, controller.GetValidControllerID(creditNotePending));
		}

		public virtual void TestGetFormWithAPIncompletCreditNote()
		{
			var dataSource = Factory.NewWithValidTestData<APCreditNote>();
			dataSource.SaveAsIncomplete();

			var controller = Controller as APCreditNoteController;
			using (var form = controller.GetForm_ForTest(dataSource))
			{
				var expectedErrorMessage =
					string.Format(@"Message: ZController {0} is handling an invalid bizO Enterprise.Accounting.Business.ARAP.Invoicing.APCreditNote. The correct controller ID should be APIncompleteCreditNote
AH_TransactionType: INC
AH_Ledger: IN
Details:
Header: PK = {1}", Controller.ID, dataSource.PK.ToString());
				AssertEquals("Should have a developer notification exception.", 1, ExceptionReporterTestListener.Instance.Count);
				Assert("The developer notification exception should be about using invalid controller.", ExceptionReporterTestListener.Instance[0].InnerException.Message.Contains(expectedErrorMessage));
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestShowErrorMessageWhenInvalidDataSource()
		{
			var invalidSource = ChangeStateOfParentTransactionHeaderRow();

			Factory.Save();

			using (var form = Controller.ShowViewForm(invalidSource))
			{
				var expectedErrorMessage = Controller.AlreadyDeletedOrIrreversiblyChangedMessage;
				AssertEquals("Should not have a developer notification exception.", 0, ExceptionReporterTestListener.Instance.Count);
				AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShouldLoadBusinessObject()
		{
			var controller = Controller as APCreditNoteController;
			AssertEquals(true, controller.ShouldLoadBusinessObject);
		}

		#endregion
	}
}
