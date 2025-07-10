using System;
using System.Collections;
using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP.Payment;
using Enterprise.Accounting.GUI.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class JobInvoicingPrinterTest : InvoicePrinterTest
	{
		#region class NoPrintJobInvoicingPrinter

		class NoPrintJobInvoicingPrinter : JobInvoicingPrinter
		{
			public NoPrintJobInvoicingPrinter(DialogResult userChoice)
				: base(null)
			{
				fUserChoice = userChoice;
				JobInvoincePrinted += new JobInvoincePrintedHandler(NoPrintJobInvoicingPrinter_JobInvoincingPrinted);
			}

			protected override DialogResult GetUserChoice(string message, string caption, MessageBoxButtons messageBoxButton, MessageBoxIcon messageBoxIcon)
			{
				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.AddAnswer(fUserChoice);
				return base.GetUserChoice(message, caption, messageBoxButton, messageBoxIcon);
			}

			readonly DialogResult fUserChoice;
			public InvoicePrintTask PrintTask;

			public ArrayList PrintingResults = new ArrayList();
			void NoPrintJobInvoicingPrinter_JobInvoincingPrinted(TransactionPrintingResults printingResult)
			{
				PrintingResults.Add(printingResult);
			}
		}

		#endregion

		public void TestNewTaskHasJobParent()
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			var mockJobParent = new Mock<IJobHeaderParent>();
			var printer = new JobInvoicingPrinter(mockJobParent.Object);
			AssertEquals("JobParent", mockJobParent.Object, printer.JobParent_ForTestOnly);
			InvoicePrintTask printTask = printer.NewTask_ForTestOnly(invoice, InvoicePrintContext.DontCare);
			AssertEquals("NewTask().JobParent", mockJobParent.Object, printTask.JobParent);
		}

		public void TestNewTask_PrintCommonInvoiceforVN()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.VietNam))
			using (AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ENT"))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var invoice = Factory.New<ARInvoice>();
				var mockJobParent = new Mock<IJobHeaderParent>();
				var printer = new JobInvoicingPrinter(mockJobParent.Object);
				var printTask = printer.NewTask_ForTestOnly(invoice, InvoicePrintContext.DontCare);

				AssertEquals("Print task should not be GovtTaxInvoicePrintTask.", false, printTask is GovtTaxInvoicePrintTask);
				Assert("Print task should be InvoicePrintTask.", printTask is InvoicePrintTask);
			}
		}

		public void TestPrint()
		{
			NoPrintJobInvoicingPrinter testPrinter = new NoPrintJobInvoicingPrinter(DialogResult.Yes);
			ARInvoice testARInv = Factory.NewWithValidTestData<ARInvoice>();

			using (ZForm testForm = new ZForm())
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
				OrgHeader testChinaOrg = Factory.NewWithValidTestData<OrgHeader>();
				testChinaOrg.OH_RL_NKClosestPort = "CNSHA";
				testARInv.AH_OH = testChinaOrg.PK;
				Factory.Save();

				APPayment testAPPayment = Factory.NewWithValidTestData<APPayment>();
				APInvoice testAPInvoice = Factory.NewWithValidTestData<APInvoice>();
				testAPInvoice.IsSelfBillingInvoice = true;
				APCreditNote testAPCreditNote = Factory.NewWithValidTestData<APCreditNote>();
				testAPCreditNote.IsSelfBillingInvoice = true;
				Factory.Save();

				testPrinter.Print(testForm, InvoicePrintContext.DontCare, testARInv, testAPPayment, testAPInvoice, testAPCreditNote);

				AssertEquals(4, testPrinter.PrintingResults.Count);
				AssertEquals(TransactionPrintingResults.TransactionIsNotClassAInvoice, testPrinter.PrintingResults[0]);
				AssertEquals(TransactionPrintingResults.TransactionIsAPPayment, testPrinter.PrintingResults[1]);
				AssertEquals(TransactionPrintingResults.None, testPrinter.PrintingResults[2]);
				AssertEquals(TransactionPrintingResults.None, testPrinter.PrintingResults[3]);

				AssertEquals("Should have opened Payment Documents Print Popup", typeof(PaymentDocumentsPrintPopup), ZFormModaliser.LastFormShownDialogForTest.GetType());
				((PaymentDocumentsPrintPopup)ZFormModaliser.LastFormShownDialogForTest).Close();
			}
		}

		public void TestPrintPrintOnlyEligibleTransctions()
		{
			using (AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var testPrinter = new NoPrintJobInvoicingPrinter(DialogResult.Yes);
				var testARInv = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				using (ZForm testForm = new ZForm())
				{
					testPrinter.Print(testForm, InvoicePrintContext.DontCare, testARInv);

					AssertEquals(1, testPrinter.PrintingResults.Count);
					AssertEquals(TransactionPrintingResults.TransactionIsNotClassAInvoice, testPrinter.PrintingResults[0]);

					Assert("Attach INV document to EDocs", InvoicePrintTask.AttachARInvoiceToEdocs_ForTestOnly(testARInv, new NotificationBuffer()));
					Factory.Save();
					AssertEquals("AR Invoice should have invoice attached in EDocs", 1, testARInv.DocManagerInfo.AllEDocs.Count);
					var eDoc = testARInv.DocManagerInfo.AllEDocs[0];
					AssertEquals("INV", eDoc.DocType);
					Assert(eDoc.IsSystemGenerated);

					testPrinter = new NoPrintJobInvoicingPrinter(DialogResult.Yes);
					testPrinter.Print(testForm, InvoicePrintContext.DontCare, testARInv);

					AssertEquals(0, testPrinter.PrintingResults.Count);
					var expected = @"Re-printed versions of a receivables document in your country/region must reflect the data used at the time of posting, hence re-printing of following transactions is not allowed through this module. You can go to the eDocs tab of a transaction and re-print the first invoice version stored there.
AR INV 00001000";
					AssertEquals("Expect a 'not eligible to print' error", expected, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}
	}
}
