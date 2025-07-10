using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	[TestedType(typeof(TransactionHeaderProcessTask))]
	public class TransactionHeaderProcessTaskTest : ProcessTaskTest
	{
		public void TestParentCotrollerID()
		{
			ARInvoice aRInvoice = Factory.New<ARInvoice>();
			var processTaskForARInvoice = ((IWorkflowProvider)aRInvoice).WorkflowItems.AddNew();
			processTaskForARInvoice = processTaskForARInvoice as TransactionHeaderProcessTask;
			AssertNotNull(processTaskForARInvoice);
			AssertEquals(ControllerIDs.ARInvoice, processTaskForARInvoice.ParentControllerID);

			APInvoice aPInvoice = Factory.New<APInvoice>();
			var processTaskForAPInvoice = ((IWorkflowProvider)aPInvoice).WorkflowItems.AddNew();
			processTaskForAPInvoice = processTaskForAPInvoice as TransactionHeaderProcessTask;
			AssertNotNull(processTaskForAPInvoice);
			AssertEquals(ControllerIDs.APInvoice, processTaskForAPInvoice.ParentControllerID);

			//Create AP Incomplete Invoice
			//Add Task
			//Assert ControllerID is ControllerIDs.APIncompleteInvoice
			aPInvoice.MakeAsIncomplete(out var subTypeMessage);
			AssertNullOrEmpty(subTypeMessage);
			AssertEquals("Pre-Condition", LedgerTypes.IncompleteTransactions, aPInvoice.AH_Ledger);
			var processTaskForAPIncompleteInvoice = ((IWorkflowProvider)aPInvoice).WorkflowItems.AddNew();
			AssertNotNull(processTaskForAPIncompleteInvoice);
			AssertEquals(ControllerIDs.APIncompleteInvoice, processTaskForAPIncompleteInvoice.ParentControllerID);

			ARCreditNote aRCreditNote = Factory.New<ARCreditNote>();
			var processTaskForARCreditNote = ((IWorkflowProvider)aRCreditNote).WorkflowItems.AddNew();
			processTaskForARCreditNote = processTaskForARCreditNote as TransactionHeaderProcessTask;
			AssertNotNull(processTaskForARCreditNote);
			AssertEquals(ControllerIDs.ARCreditNote, processTaskForARCreditNote.ParentControllerID);

			APCreditNote aPCreditNote = Factory.New<APCreditNote>();
			var processTaskForAPCreditNote = ((IWorkflowProvider)aPCreditNote).WorkflowItems.AddNew();
			processTaskForAPCreditNote = processTaskForAPCreditNote as TransactionHeaderProcessTask;
			AssertNotNull(processTaskForAPCreditNote);
			AssertEquals(ControllerIDs.APCreditNote, processTaskForAPCreditNote.ParentControllerID);

			//Create AP Incomplete CreditNote
			//Add Task
			//Assert ControllerID is ControllerIDs.APIncompleteCreditNote
			aPCreditNote.MakeAsIncomplete(out subTypeMessage);
			AssertNullOrEmpty(subTypeMessage);
			AssertEquals("Pre-Condition", LedgerTypes.IncompleteTransactions, aPCreditNote.AH_Ledger);
			var processTaskForAPIncompleteCreditNote = ((IWorkflowProvider)aPCreditNote).WorkflowItems.AddNew();
			AssertNotNull(processTaskForAPIncompleteCreditNote);
			AssertEquals(ControllerIDs.APIncompleteCreditNote, processTaskForAPIncompleteCreditNote.ParentControllerID);

			ARAdjustmentNote aRAdjustmentNote = Factory.New<ARAdjustmentNote>();
			var processTaskForARAdjustmentNote = ((IWorkflowProvider)aRAdjustmentNote).WorkflowItems.AddNew();
			processTaskForARAdjustmentNote = processTaskForARAdjustmentNote as TransactionHeaderProcessTask;
			AssertNotNull(processTaskForARAdjustmentNote);
			AssertEquals(ControllerIDs.ARAdjustmentNote, processTaskForARAdjustmentNote.ParentControllerID);

			APAdjustmentNote aPAdjustmentNote = Factory.New<APAdjustmentNote>();
			var processTaskForAPAdjustmentNote = ((IWorkflowProvider)aPAdjustmentNote).WorkflowItems.AddNew();
			processTaskForAPAdjustmentNote = processTaskForAPAdjustmentNote as TransactionHeaderProcessTask;
			AssertNotNull(processTaskForAPAdjustmentNote);
			AssertEquals(ControllerIDs.APAdjustmentNote, processTaskForAPAdjustmentNote.ParentControllerID);

			//Create AP Incomplete AdJustmentNote
			//Add Task
			//Assert ControllerID is ControllerIDs.APIncompleteAdjustmentNote
			aPAdjustmentNote.MakeAsIncomplete(out subTypeMessage);
			AssertNullOrEmpty(subTypeMessage);
			AssertEquals("Pre-Condition", LedgerTypes.IncompleteTransactions, aPAdjustmentNote.AH_Ledger);
			var processTaskForAPIncompleteAdjustmentNote = ((IWorkflowProvider)aPAdjustmentNote).WorkflowItems.AddNew();
			AssertNotNull(processTaskForAPIncompleteAdjustmentNote);
			AssertEquals(ControllerIDs.APIncompleteAdjustmentNote, processTaskForAPIncompleteAdjustmentNote.ParentControllerID);

			UAInvoice uAInvoice = Factory.New<UAInvoice>();
			var processTaskForUAInvoice = ((IWorkflowProvider)uAInvoice).WorkflowItems.AddNew();
			processTaskForUAInvoice = processTaskForUAInvoice as TransactionHeaderProcessTask;
			AssertNotNull(processTaskForUAInvoice);
			AssertEquals(ControllerIDs.UAInvoice, processTaskForUAInvoice.ParentControllerID);

			UACreditNote uACreditNote = Factory.New<UACreditNote>();
			var processTaskForUACreditNote = ((IWorkflowProvider)uACreditNote).WorkflowItems.AddNew();
			processTaskForUACreditNote = processTaskForUACreditNote as TransactionHeaderProcessTask;
			AssertNotNull(processTaskForUACreditNote);
			AssertEquals(ControllerIDs.UACreditNote, processTaskForUACreditNote.ParentControllerID);

			TransactionPendingAllocation transactionPendingAllocation = Factory.New<TransactionPendingAllocation>();
			var processTaskForTransactionPendingAllocation = ((IWorkflowProvider)transactionPendingAllocation).WorkflowItems.AddNew();
			processTaskForTransactionPendingAllocation = processTaskForTransactionPendingAllocation as TransactionHeaderProcessTask;
			AssertNotNull(processTaskForTransactionPendingAllocation);
			AssertEquals(ControllerIDs.TransactionsPendingAllocation, processTaskForTransactionPendingAllocation.ParentControllerID);
		}

		public void TestParentCotrollerIDForARAPPayment()
		{
			var arPayment = Factory.New<ARPayment>();
			var processTaskForARPayment = ((IWorkflowProvider)arPayment).WorkflowItems.AddNew();
			processTaskForARPayment = processTaskForARPayment as TransactionHeaderProcessTask;
			AssertNotNull(processTaskForARPayment);
			AssertEquals(ControllerIDs.ZARPayment, processTaskForARPayment.ParentControllerID);

			var apPayment = Factory.New<APPayment>();
			var processTaskForAPPayment = ((IWorkflowProvider)apPayment).WorkflowItems.AddNew();
			processTaskForAPPayment = processTaskForAPPayment as TransactionHeaderProcessTask;
			AssertNotNull(processTaskForAPPayment);
			AssertEquals(ControllerIDs.ZAPPayment, processTaskForAPPayment.ParentControllerID);
		}

		public void TestParentCotrollerIDForARAPReceipt()
		{
			var arReceipt = Factory.New<ARReceipt>();
			var processTaskForARReceipt = ((IWorkflowProvider)arReceipt).WorkflowItems.AddNew();
			processTaskForARReceipt = processTaskForARReceipt as TransactionHeaderProcessTask;
			AssertNotNull(processTaskForARReceipt);
			AssertEquals(ControllerIDs.ZARReceipt, processTaskForARReceipt.ParentControllerID);

			var apReceipt = Factory.New<APReceipt>();
			var processTaskForAPReceipt = ((IWorkflowProvider)apReceipt).WorkflowItems.AddNew();
			processTaskForAPReceipt = processTaskForAPReceipt as TransactionHeaderProcessTask;
			AssertNotNull(processTaskForAPReceipt);
			AssertEquals(ControllerIDs.ZAPReceipt, processTaskForAPReceipt.ParentControllerID);
		}

		public void TestParentCotrollerIDWithoutParent()
		{
			var task1 = Factory.New<TransactionHeaderProcessTask>();
			AssertEquals("ParentControllerID", null, task1.ParentControllerID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			return ((IWorkflowProvider)invoice).WorkflowItems.AddNew();
		}
	}
}
