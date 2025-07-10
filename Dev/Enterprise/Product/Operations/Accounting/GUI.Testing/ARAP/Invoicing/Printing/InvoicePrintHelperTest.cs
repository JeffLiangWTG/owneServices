using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Shared;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.Testing
{
	public class InvoicePrintHelperTest : TestCaseWithFactory
	{
		public void TestPrintSelfBillingInvoice()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			InvoicingBase invoice = Factory.NewWithValidTestData<APInvoice>();
			testObjectCreator.AALSHI.CompanyData.OB_APCostsSelfBilled = true;
			invoice.AH_OH = testObjectCreator.AALSHI.PK;
			Factory.Save();
			bool printTaskRan = false;
			InvoicePrintHelper.ResetPrintTaskRun_ForTestOnly();
			InvoicePrintHelper.PrintTaskRun_ForTestOnly += delegate(InvoicePrintTask printTask)
			{
				printTaskRan = true;
				AssertEquals("There should be 1 reports in the pack", 1, printTask[0].Count);
				AssertEquals("APSelfBillingInvoice", printTask[0][0].Name);
			};
			InvoicePrintHelper.PrintSelfBillingInvoice(invoice);
			Assert("Print task should be run", printTaskRan);
		}

		public void TestPrintSelfBillingInvoice_TransactionToPrintHasNull()
		{
			InvoicingBase invoice = Factory.NewWithValidTestData<APInvoice>();
			AssertNoExceptionThrown("should throw no exception", () => InvoicePrintHelper.PrintSelfBillingInvoice(invoice, null));
		}

		public void TestPrintCostConfirmationDocument()
		{
			var stmMenuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.PK, new ZGuid(InvoicePrintTask.MenuPkForAPInvoice)));
			var defaultPrinter = StmDefaultPrinter.LoadOrCreateDefaultPrinter(Factory, GlbStaff.CurrentUser, stmMenuItem);
			defaultPrinter.SDP_SQ_Printer = Factory.New<StmPrintQueue>().PK;
			defaultPrinter.SDP_NumberOfCopies = 1;
			Factory.Save();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			InvoicingBase invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = testObjectCreator.AALSHI.PK;
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			AccountingConfigurationRegistry.Instance.CostConfirmationDocumentSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.CostConfirmationDocumentSettingsCodes.Both);
			bool printTaskRan = false;
			InvoicePrintHelper.ResetPrintTaskRun_ForTestOnly();
			InvoicePrintHelper.PrintTaskRun_ForTestOnly += delegate(InvoicePrintTask printTask)
			{
				printTaskRan = true;
			};
			InvoicePrintHelper.PrintCostConfirmationDocument(invoice);
			AssertEquals("Print task should be not run if user canceled operation.", false, printTaskRan);
			AssertType(typeof(CostConfirmationDocTypePopupForm), ZFormModaliser.LastFormShownDialogForTest);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			AccountingConfigurationRegistry.Instance.CostConfirmationDocumentSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.CostConfirmationDocumentSettingsCodes.Both);
			printTaskRan = false;
			InvoicePrintHelper.ResetPrintTaskRun_ForTestOnly();
			InvoicePrintHelper.PrintTaskRun_ForTestOnly += delegate(InvoicePrintTask printTask)
			{
				printTaskRan = true;
				AssertEquals("There should be 2 reports in the pack", 2, printTask[0].Count);
				AssertEquals("Cost Confirmation Summary", printTask[0][0].Name);
				AssertEquals("Cost Confirmation Document", printTask[0][1].Name);
			};

			InvoicePrintHelper.PrintTaskRun_ForTestOnly += delegate(InvoicePrintTask printTask)
			{
				var instructions = new DeliveryInstructions(printTask.GetFirstDocumentPack());
				instructions.Language = Core.Constants.Languages.German;

				printTask.RunTaskWithInstructions(instructions);
				printTaskRan = true;
				AssertEquals("There should be 2 reports in the pack after language changed", 2, printTask[0].Count);
				AssertEquals("Cost Confirmation Summary", printTask[0][0].Name);
				AssertEquals("Cost Confirmation Document", printTask[0][1].Name);
			};
			InvoicePrintHelper.PrintCostConfirmationDocument(invoice);
			Assert("Print task should be run", printTaskRan);

			AccountingConfigurationRegistry.Instance.CostConfirmationDocumentSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.CostConfirmationDocumentSettingsCodes.Summary);
			printTaskRan = false;
			InvoicePrintHelper.ResetPrintTaskRun_ForTestOnly();
			InvoicePrintHelper.PrintTaskRun_ForTestOnly += delegate(InvoicePrintTask printTask)
			{
				printTaskRan = true;
				AssertEquals("There should be 1 reports in the pack", 1, printTask[0].Count);
				AssertEquals("Cost Confirmation Summary", printTask[0][0].Name);
			};
			InvoicePrintHelper.PrintCostConfirmationDocument(invoice);
			Assert("Print task should be run", printTaskRan);

			AccountingConfigurationRegistry.Instance.CostConfirmationDocumentSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.CostConfirmationDocumentSettingsCodes.Detail);
			printTaskRan = false;
			InvoicePrintHelper.ResetPrintTaskRun_ForTestOnly();
			InvoicePrintHelper.PrintTaskRun_ForTestOnly += delegate(InvoicePrintTask printTask)
			{
				printTaskRan = true;
				AssertEquals("There should be 1 reports in the pack", 1, printTask[0].Count);
				AssertEquals("Cost Confirmation Document", printTask[0][0].Name);
			};
			InvoicePrintHelper.PrintCostConfirmationDocument(invoice);
			Assert("Print task should be run", printTaskRan);
		}

		public void TestPrintCostConfirmationDocumentWithDocBuilderRegistryOnAndOff()
		{
			var stmMenuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.PK, new ZGuid(InvoicePrintTask.MenuPkForAPInvoice)));
			var defaultPrinter = StmDefaultPrinter.LoadOrCreateDefaultPrinter(Factory, GlbStaff.CurrentUser, stmMenuItem);
			defaultPrinter.SDP_SQ_Printer = Factory.New<StmPrintQueue>().PK;
			defaultPrinter.SDP_NumberOfCopies = 1;
			Factory.Save();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			InvoicingBase invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = testObjectCreator.AALSHI.PK;
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			AccountingConfigurationRegistry.Instance.CostConfirmationDocumentSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.CostConfirmationDocumentSettingsCodes.Both);

			DocumentsDataRegistry.Instance.UseNewDocBuilderCostConfirmationDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var printTaskRan = false;
			InvoicePrintHelper.ResetPrintTaskRun_ForTestOnly();
			InvoicePrintHelper.PrintTaskRun_ForTestOnly += delegate(InvoicePrintTask printTask)
			{
				printTaskRan = true;
				AssertEquals("There should be 2 reports in the pack", 2, printTask[0].Count);
				AssertEquals("Cost Confirmation Summary", printTask[0][0].Name);
				AssertEquals("Cost Confirmation Document", printTask[0][1].Name);
				AssertEquals("Menu path should be empty", "", printTask[0].StmMenuCommand.SU_MenuPath);
			};
			InvoicePrintHelper.PrintCostConfirmationDocument(invoice);
			Assert("Print task should be run", printTaskRan);

			DocumentsDataRegistry.Instance.UseNewDocBuilderCostConfirmationDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			printTaskRan = false;
			InvoicePrintHelper.ResetPrintTaskRun_ForTestOnly();
			InvoicePrintHelper.PrintTaskRun_ForTestOnly += delegate(InvoicePrintTask printTask)
			{
				printTaskRan = true;
				AssertEquals("There should be 2 reports in the pack", 2, printTask[0].Count);
				AssertEquals("Cost Confirmation Summary", printTask[0][0].Name);
				AssertEquals("Cost Confirmation Document", printTask[0][1].Name);
				AssertEquals("Menu path should be 'Legacy Documents'", "Legacy Documents", printTask[0].StmMenuCommand.SU_MenuPath);
			};
			InvoicePrintHelper.PrintCostConfirmationDocument(invoice);
			Assert("Print task should be run", printTaskRan);
		}

		public void TestPrintCostConfirmationDocumentWithDocBuilderRegistryOnAndOff_UnapprovedInvoice()
		{
			var stmMenuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.PK, new ZGuid(InvoicePrintTask.MenuPkForAPInvoice)));
			var defaultPrinter = StmDefaultPrinter.LoadOrCreateDefaultPrinter(Factory, GlbStaff.CurrentUser, stmMenuItem);
			defaultPrinter.SDP_SQ_Printer = Factory.New<StmPrintQueue>().PK;
			defaultPrinter.SDP_NumberOfCopies = 1;
			Factory.Save();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			InvoicingBase invoice = testObjectCreator.CreateInvoice(typeof(UAInvoice), testObjectCreator.AUD, 1M, testObjectCreator.AALSHI);
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			AccountingConfigurationRegistry.Instance.CostConfirmationDocumentSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.CostConfirmationDocumentSettingsCodes.Both);

			DocumentsDataRegistry.Instance.UseNewDocBuilderCostConfirmationDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var printTaskRan = false;
			InvoicePrintHelper.ResetPrintTaskRun_ForTestOnly();
			InvoicePrintHelper.PrintTaskRun_ForTestOnly += delegate(InvoicePrintTask printTask)
			{
				printTaskRan = true;
				AssertEquals("There should be 2 reports in the pack", 2, printTask[0].Count);
				AssertEquals("Cost Confirmation Summary", printTask[0][0].Name);
				AssertEquals("Cost Confirmation Document", printTask[0][1].Name);
				AssertEquals("Menu path should be empty", "", printTask[0].StmMenuCommand.SU_MenuPath);
			};
			InvoicePrintHelper.PrintCostConfirmationDocument(invoice);
			Assert("Print task should be run", printTaskRan);

			DocumentsDataRegistry.Instance.UseNewDocBuilderCostConfirmationDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			printTaskRan = false;
			InvoicePrintHelper.ResetPrintTaskRun_ForTestOnly();
			InvoicePrintHelper.PrintTaskRun_ForTestOnly += delegate(InvoicePrintTask printTask)
			{
				printTaskRan = true;
				AssertEquals("There should be 2 reports in the pack", 2, printTask[0].Count);
				AssertEquals("Cost Confirmation Summary", printTask[0][0].Name);
				AssertEquals("Cost Confirmation Document", printTask[0][1].Name);
				AssertEquals("Menu path should be 'Legacy Documents'", "Legacy Documents", printTask[0].StmMenuCommand.SU_MenuPath);
			};
			InvoicePrintHelper.PrintCostConfirmationDocument(invoice);
			Assert("Print task should be run", printTaskRan);
		}

		public void TestGetEligibleForPrintingTransactions()
		{
			using (AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var expected = "There is no eligible transaction for printing";
				AssertEquals("Should return no transactions for null", Enumerable.Empty<TransactionHeader>(), InvoicePrintHelper.GetEligibleForPrintingTransactions(null));
				AssertEquals("Expect a 'nothing to print' error", expected, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				AssertEquals("Should return no transactions for empty enumerable", Enumerable.Empty<TransactionHeader>(), InvoicePrintHelper.GetEligibleForPrintingTransactions(Enumerable.Empty<TransactionHeader>()));
				AssertEquals("Expect a 'nothing to print' error", expected, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var creator = new TestObjectCreator(Factory);
				var invoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "10001", creator.AUD, 1.0m, 100.0m, 0.0m, 100.0m, 0.0m, creator.AALSHI, creator.CC10.PK);
				var creditNote = creator.CreateInvoiceWithLine(typeof(ARCreditNote), "10002", creator.AUD, 1.0m, 100.0m, 0.0m, 100.0m, 0.0m, creator.AALSHI, creator.CC10.PK);
				var transactions = new TransactionHeader[] { invoice, creditNote };

				AssertContainsExactElementsInAnyOrder("Should return all transactions", transactions, InvoicePrintHelper.GetEligibleForPrintingTransactions(transactions));
				AssertNull("Expect no notifications", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();
				AssertContainsExactElementsInAnyOrder("Should return all transactions", transactions, InvoicePrintHelper.GetEligibleForPrintingTransactions(transactions));
				AssertNull("Expect no notifications", UnitTestUserNotification.Instance.LastMessage.Text);

				Assert("Attaching should be successful", InvoicePrintTask.AttachARInvoiceToEdocs_ForTestOnly(invoice, new NotificationBuffer()));
				Factory.Save();
				AssertEquals("AR Invoice should have invoice attached in EDocs", 1, invoice.DocManagerInfo.AllEDocs.Count);
				var eDoc = invoice.DocManagerInfo.AllEDocs[0];
				AssertEquals("INV", eDoc.DocType);
				Assert(eDoc.IsSystemGenerated);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				expected = @"Re-printed versions of a receivables document in your country/region must reflect the data used at the time of posting, hence re-printing of following transactions is not allowed through this module. You can go to the eDocs tab of a transaction and re-print the first invoice version stored there.
AR INV 00001000
Do you want to print rest of the transactions?

";
				AssertEquals("Should return no transactions", Enumerable.Empty<TransactionHeader>(), InvoicePrintHelper.GetEligibleForPrintingTransactions(transactions));
				AssertEquals("Expect a confirmation request", expected, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Continue Printing", UnitTestUserNotification.Instance.LastMessage.Caption);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertContainsExactElementsInAnyOrder("Should return one transaction", new TransactionHeader[] { creditNote }, InvoicePrintHelper.GetEligibleForPrintingTransactions(transactions));
				AssertEquals("Expect a confirmation request", expected, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Continue Printing", UnitTestUserNotification.Instance.LastMessage.Caption);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Assert("Attaching should be successful", InvoicePrintTask.AttachARInvoiceToEdocs_ForTestOnly(creditNote, new NotificationBuffer()));
				Factory.Save();
				AssertEquals("AR Credit Note should have an INV document attached in EDocs", 1, creditNote.DocManagerInfo.AllEDocs.Count);
				eDoc = creditNote.DocManagerInfo.AllEDocs[0];
				AssertEquals("INV", eDoc.DocType);
				Assert(eDoc.IsSystemGenerated);

				expected = @"Re-printed versions of a receivables document in your country/region must reflect the data used at the time of posting, hence re-printing of following transactions is not allowed through this module. You can go to the eDocs tab of a transaction and re-print the first invoice version stored there.
AR INV 00001000
AR CRD 00001000";
				AssertEquals("Should return no transactions", Enumerable.Empty<TransactionHeader>(), InvoicePrintHelper.GetEligibleForPrintingTransactions(transactions));
				AssertEquals("Expect a 'not eligible to print' error", expected, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			InvoicePrintHelper.ResetPrintTaskRun_ForTestOnly();
		}

		protected override void TearDown()
		{
			InvoicePrintHelper.ResetPrintTaskRun_ForTestOnly();
			base.TearDown();
		}
	}
}
