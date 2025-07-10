using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(PaymentBatchModule))]
	public class PaymentBatchModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.PaymentBatch;
		}

		public void TestAllowedAction()
		{
			using (var module = (PaymentBatchModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				AssertEquals(true, module.AllowView);
				AssertEquals(true, module.AllowEdit);
				AssertEquals(false, module.AllowNew);
				AssertEquals(false, module.AllowDelete);
			}
		}

		public void TestMenuItems()
		{
			using (var module = (PaymentBatchModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				var viewMenuItem = module.FormActionMenu.FindByText("View") as ZMenuItem;
				var printMenuItem = module.FormActionMenu.FindByText("Print") as ZMenuItem;
				var editMenuItem = module.FormActionMenu.FindByText("Edit") as ZMenuItem;

				AssertNotNull(viewMenuItem);
				AssertNotNull(viewMenuItem);
				AssertNotNull(editMenuItem);
			}
		}

		#region Test Print PaymentBatch

		public void PreparePaymentBatch()
		{
			var batches = Factory.Load<AccPaymentBatch>(new ZQuery());
			var approvals = Factory.Load<AccPaymentApproval>(new ZQuery());
			AssertEquals(0, batches.Length);
			AssertEquals(0, approvals.Length);

			var bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_ChequeNumDigits = 6;
			var cheque = Factory.NewWithValidTestData<AccChequeBook>();
			cheque.AK_StartNo = 1;
			cheque.AK_LastNo = 100;
			cheque.AK_CurrentNo = 1;
			cheque.AK_AB = bank.PK;

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV", TestObjectCreator.LocalCurrency, 1m, 100m, 10m, 100m, 10m);
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			var creditNote = TestObjectCreator.CreateInvoiceWithLine(typeof(APCreditNote), "CRD", TestObjectCreator.LocalCurrency, 1m, 100m, 10m, 100m, 10m);
			creditNote.AH_OH = TestObjectCreator.Creditor1.PK;
			Factory.Save();

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory);
			transactions.Add(invoice);
			transactions.Add(creditNote);

			var batchPoster = Factory.New<APPaymentBatchPoster>();
			batchPoster.SetDefaultValuesByTransactions(transactions);
			batchPoster.APB_AB = bank.PK;
			batchPoster.SetPaymentDetails(batchPoster.PaymentApprovalCollection[0]);
			batchPoster.APB_AB = bank.PK;
			batchPoster.APB_AK = cheque.PK;
			Factory.Save();

			batches = Factory.Load<AccPaymentBatch>(new ZQuery());
			approvals = Factory.Load<AccPaymentApproval>(new ZQuery());
			AssertEquals(1, batches.Length);
			AssertEquals(2, approvals.Length);
		}

		public void TestPrintPaymentBatch_WhenNoneSelectedBusinessObjects()
		{
			PreparePaymentBatch();

			using (var module = (PaymentBatchModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				var printMenuItem = module.FormActionMenu.FindByText("Print") as ZMenuItem;
				printMenuItem.PerformClick();
				AssertEquals("Please select a payment batch to print.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPrintPaymentBatch_WhenNotAllowedPrintPaymentBatch()
		{
			PreparePaymentBatch();

			Env.Security.PrintPaymentBatch.IsAllowed = false;

			using (var module = (PaymentBatchModule)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				module.PerformSearch_ForTest();
				module.DisplayGrid.SelectAllElements();
				AssertEquals(1, module.GridCollection.Count);
				AssertEquals(1, module.GetSelectedBusinessObjects().Length);

				var printMenuItem = module.FormActionMenu.FindByText("Print") as ZMenuItem;
				printMenuItem.PerformClick();
				AssertEquals(Env.Security.PrintPaymentBatch.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPrintPaymentBatch_WhenNoneRelatedPaymentApproval()
		{
			var batch = Factory.NewWithValidTestData<AccPaymentBatch>();
			Factory.Save();
			var approvals = Factory.Load<AccPaymentApproval>(new ZQuery(AccPaymentApprovalSchema.AV_APB_PaymentBatch, batch.PK));
			AssertEquals("Percondition", 0, approvals.Length);

			using (var module = (PaymentBatchModule)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				module.PerformSearch_ForTest();
				module.DisplayGrid.SelectAllElements();
				AssertEquals(1, module.GridCollection.Count);
				AssertEquals(1, module.GetSelectedBusinessObjects().Length);

				var printMenuItem = module.FormActionMenu.FindByText("Print") as ZMenuItem;
				printMenuItem.PerformClick();
				AssertEquals("The selected payment batch does not include any approval request.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPrintPaymentBatch()
		{
			PreparePaymentBatch();

			using (var module = (PaymentBatchModule)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				module.PerformSearch_ForTest();
				module.DisplayGrid.SelectAllElements();
				AssertEquals(1, module.GridCollection.Count);
				AssertEquals(1, module.GetSelectedBusinessObjects().Length);

				var mock = new Mock<IPrintTaskUIProvider>();
				mock.Setup(d => d.ShowDocDeliveryUI(It.IsAny<PrintTask>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ISecurityCheckpoint>()))
				.Returns((PrintTask printTask, DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint) =>
				{
					var result = ZFormModaliser.ShowDialogAndDispose(new DocDeliveryForm(printTask, instructions, modifyDocumentCheckPoint)) == DialogResult.OK;
					AssertEquals(1, instructions.DeliverablesToBePrinted.Count);
					AssertEquals(instructions.DocumentsToBeDelivered[0].DocumentName, "Payment Batch Listing");
					return result;
				});

				using (new PrintTaskUIProviderFactory.OverriderForTesting(mock.Object))
				{
					var printMenuItem = module.FormActionMenu.FindByText("Print") as ZMenuItem;
					printMenuItem.PerformClick();
					var docDeliveryForm = ZFormModaliser.LastFormShownDialogForTest as DocDeliveryForm;
					var deliveryInstruction = ZFormModaliser.LastIBusinessShownOnDialogForTest as DeliveryInstructions;
					AssertNotNull(docDeliveryForm);
					AssertNotNull(deliveryInstruction);
				}
			}
		}

		#endregion

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
