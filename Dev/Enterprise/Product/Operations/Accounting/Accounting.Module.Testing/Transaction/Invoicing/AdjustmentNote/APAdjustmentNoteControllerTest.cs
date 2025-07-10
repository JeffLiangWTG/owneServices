using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APAdjustmentNoteController))]
	class APAdjustmentNoteControllerTest : InvoicingBaseControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.APAdjustmentNote;
		}

		protected override Type GetExpectedBusinessObjectType()
		{
			return typeof(APAdjustmentNote);
		}

		protected override Type GetExpectedFormType()
		{
			return typeof(GUI.AdjustmentNoteForm);
		}

		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.ReversePayablesAjdustmentNote; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.NewPayablesAjdustmentNote; }
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
			var transaction = Factory.NewWithValidTestData<APInvoice>();
			transaction.SaveAsIncomplete();
			return transaction;
		}

		protected override string MessageToShowWhenControllerMismatchForBusinessEntity => "The AP adjustment note cannot be displayed because another user has changed or deleted the record. Closing and re-opening this window will refresh your data.";

		#region INavigationControllerIDProvider

		public void TestGetValidControllerID()
		{
			var controller = Controller as INavigationControllerIDProvider;

			var nonRelatedBizO = Factory.NewWithValidTestData<ARInvoice>();
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			var adjustmentNote = Factory.NewWithValidTestData<APAdjustmentNote>();
			var incompleteAdjustmentNote = Factory.NewWithValidTestData<APAdjustmentNote>();
			incompleteAdjustmentNote.SaveAsIncomplete();

			AssertEquals("ControllerID for non-related bizO should be APAdjustmentNote.", ControllerIDs.APAdjustmentNote, controller.GetValidControllerID(nonRelatedBizO));
			AssertEquals("ControllerID for AP Invoice bizO should be APInvocie.", ControllerIDs.APInvoice, controller.GetValidControllerID(invoice));
			AssertEquals("ControllerID for AP Adjustment Note bizO should be APAdjustmentNote.", ControllerIDs.APAdjustmentNote, controller.GetValidControllerID(adjustmentNote));
			AssertEquals("ControllerID for AP Incomplete Adjustment Note bizO should be APIncompleteAdjustmentNote.", ControllerIDs.APIncompleteAdjustmentNote, controller.GetValidControllerID(incompleteAdjustmentNote));
		}

		public void TestReportInvalidDataSource()
		{
			var dataSource = Factory.NewWithValidTestData<APAdjustmentNote>();
			dataSource.SaveAsIncomplete();

			var controller = Controller as APAdjustmentNoteController;
			using (var form = controller.GetForm_ForTest(dataSource))
			{
				var expectedErrorMessage =
					string.Format(@"Message: ZController {0} is handling an invalid bizO Enterprise.Accounting.Business.ARAP.Invoicing.APAdjustmentNote. The correct controller ID should be APIncompleteAdjustmentNote
AH_TransactionType: INA
AH_Ledger: IN
Details:
Header: PK = {1}", Controller.ID, dataSource.PK.ToString());
				AssertEquals("Should have a developer notification exception.", 1, ExceptionReporterTestListener.Instance.Count);
				Assert("The developer notification exception should be about using invalid controller.", ExceptionReporterTestListener.Instance[0].InnerException.Message.Contains(expectedErrorMessage));
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestShouldLoadBusinessObject()
		{
			var controller = Controller as APAdjustmentNoteController;
			AssertEquals(true, controller.ShouldLoadBusinessObject);
		}

		#endregion
	}
}
