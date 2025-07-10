using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class GovtTaxInvoicePrinterTest : TestCaseWithFactory
	{
		public void TestPrintGovtTaxInvoicesWithGovtTaxInvoiceOnly_OneWithPrinterAndOneWithoutPrinter()
		{
			SetupOneSequenceWithPrinterAndOneWithoutPrinter();
			GovtTaxInvoicePrinter printer = new GovtTaxInvoicePrinter(GovtTaxInvoicePrintTask.GovtTaxInvoice);
			TransactionHeader[] invoicesToPrint = new TransactionHeader[] { invoice1, invoice2 };
			printer.PrintGovtTaxInvoices(invoicesToPrint);

			ZQuery query = new ZQuery();
			BusinessObject[] printJobs = TestObjectCreator.Factory.Load(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintJob>(), query);
			AssertEquals("Should be 1 print jobs", 1, printJobs.Length);
			AssertEquals("LastFormShownDialogForTest", "Enterprise.DocumentEngine.GUI.DocDeliveryForm", ZFormModaliser.LastFormShownDialogForTest.GetType().ToString());
		}

		class ExceptionToThrow : IExceptionWhenRenderAndSave
		{
			readonly Exception ex;
			public ExceptionToThrow(Exception ex)
			{
				this.ex = ex;
			}

			public void Throw()
			{
				throw ex;
			}
		}

		public void TestPrintGovtTaxInvoicesWhenPrintError()
		{
			var staff = Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "PM")).FirstOrDefault();
			staff.GS_EmailAddress = "unit.test@cargowise.com";
			Factory.Save();

			SetupOneSequenceWithPrinterAndOneWithoutPrinter();
			GovtTaxInvoicePrinter printer = new GovtTaxInvoicePrinter(GovtTaxInvoicePrintTask.GovtTaxInvoice);
			TransactionHeader[] invoicesToPrint = new TransactionHeader[] { invoice1, invoice2 };

			UnitTestUserNotification.Instance.AddOKAnswer();
			using (ObjectFactory.Substitute<IExceptionWhenRenderAndSave>(new ExceptionToThrow(new InvalidOperationException("Object is currently in use elsewhere."))))
			{
				printer.PrintGovtTaxInvoices(invoicesToPrint);
			}

			var message = @"An error has occurred during the rendering of the selected document.
Please review the template and/or network connection before printing this document again." + System.Environment.NewLine + System.Environment.NewLine;

			AssertMultilineASCIIEquals(message + @"Severity: [Fatal Error (without error report)] Message: [The report could not be generated due to a transient graphics failure in Windows. However, the report will be rendered fine when next attempted. Error details:
Object is currently in use elsewhere.] Cell: [N/A] Sheetname: [(unknown)]", UnitTestUserNotification.Instance.LastMessage.Text);

			ZQuery query = new ZQuery();
			BusinessObject[] printJobs = TestObjectCreator.Factory.Load(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintJob>(), query);
			AssertEquals("Should be 0 print jobs", 0, printJobs.Length);
			AssertEquals("Transaction 1 is NOT printed", false, invoice1.AH_InvoicePrinted);
			AssertEquals("Transaction 2 is NOT printed", false, invoice2.AH_InvoicePrinted);
		}

		public void TestPrintGovtTaxInvoicesWithEnterpriseInvoiceOnly_OneWithPrinterAndOneWithoutPrinter()
		{
			SetupOneSequenceWithPrinterAndOneWithoutPrinter();
			GovtTaxInvoicePrinter printer = new GovtTaxInvoicePrinter(GovtTaxInvoicePrintTask.EnterpriseInvoice);
			TransactionHeader[] invoicesToPrint = new TransactionHeader[] { invoice1, invoice2 };
			printer.PrintGovtTaxInvoices(invoicesToPrint);

			ZQuery query = new ZQuery();
			BusinessObject[] printJobs = TestObjectCreator.Factory.Load(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintJob>(), query);
			AssertEquals("Should be 0 print jobs", 0, printJobs.Length);
			AssertEquals("LastFormShownDialogForTest", "Enterprise.DocumentEngine.GUI.DocDeliveryForm", ZFormModaliser.LastFormShownDialogForTest.GetType().ToString());
		}

		public void TestPrintGovtTaxInvoicesWithBothTypesOfInvoices_OneWithPrinterAndOneWithoutPrinter()
		{
			SetupOneSequenceWithPrinterAndOneWithoutPrinter();
			GovtTaxInvoicePrinter printer = new GovtTaxInvoicePrinter(GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice);
			TransactionHeader[] invoicesToPrint = new TransactionHeader[] { invoice1, invoice2 };
			printer.PrintGovtTaxInvoices(invoicesToPrint);

			ZQuery query = new ZQuery();
			BusinessObject[] printJobs = TestObjectCreator.Factory.Load(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintJob>(), query);
			AssertEquals("Should be 1 print jobs", 1, printJobs.Length);
			AssertEquals("LastFormShownDialogForTest", "Enterprise.DocumentEngine.GUI.DocDeliveryForm", ZFormModaliser.LastFormShownDialogForTest.GetType().ToString());
		}

		public void TestPrintGovtTaxInvoicesWithGovtTaxInvoiceOnly_BothWithPrinter()
		{
			SetupSequenceBothWithPrinter();
			GovtTaxInvoicePrinter printer = new GovtTaxInvoicePrinter(GovtTaxInvoicePrintTask.GovtTaxInvoice);
			TransactionHeader[] invoicesToPrint = new TransactionHeader[] { invoice1, invoice2 };
			printer.PrintGovtTaxInvoices(invoicesToPrint);

			ZQuery query = new ZQuery();
			BusinessObject[] printJobs = TestObjectCreator.Factory.Load(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintJob>(), query);
			AssertEquals("Should be 2 print jobs", 2, printJobs.Length);
			AssertEquals("LastFormShownDialogForTest", null, ZFormModaliser.LastFormShownDialogForTest);
		}

		public void TestPrintGovtTaxInvoicesWithEnterpriseInvoiceOnly_BothWithPrinter()
		{
			SetupSequenceBothWithPrinter();
			GovtTaxInvoicePrinter printer = new GovtTaxInvoicePrinter(GovtTaxInvoicePrintTask.EnterpriseInvoice);
			TransactionHeader[] invoicesToPrint = new TransactionHeader[] { invoice1, invoice2 };
			printer.PrintGovtTaxInvoices(invoicesToPrint);

			ZQuery query = new ZQuery();
			BusinessObject[] printJobs = TestObjectCreator.Factory.Load(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintJob>(), query);
			AssertEquals("Should be 0 print jobs", 0, printJobs.Length);
			AssertEquals("LastFormShownDialogForTest", "Enterprise.DocumentEngine.GUI.DocDeliveryForm", ZFormModaliser.LastFormShownDialogForTest.GetType().ToString());
		}

		public void TestPrintGovtTaxInvoicesWithBothTypesOfInvoices_BothWithPrinter()
		{
			SetupSequenceBothWithPrinter();
			GovtTaxInvoicePrinter printer = new GovtTaxInvoicePrinter(GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice);
			TransactionHeader[] invoicesToPrint = new TransactionHeader[] { invoice1, invoice2 };
			printer.PrintGovtTaxInvoices(invoicesToPrint);

			ZQuery query = new ZQuery();
			BusinessObject[] printJobs = TestObjectCreator.Factory.Load(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintJob>(), query);
			AssertEquals("Should be 2 print jobs", 2, printJobs.Length);
			AssertEquals("LastFormShownDialogForTest", "Enterprise.DocumentEngine.GUI.DocDeliveryForm", ZFormModaliser.LastFormShownDialogForTest.GetType().ToString());
		}

		public void TestPrintGovtTaxInvoicesWithGovtTaxInvoiceOnly_BothWithoutPrinter()
		{
			SetupSequenceNoneWithPrinter();
			GovtTaxInvoicePrinter printer = new GovtTaxInvoicePrinter(GovtTaxInvoicePrintTask.GovtTaxInvoice);
			TransactionHeader[] invoicesToPrint = new TransactionHeader[] { invoice1, invoice2 };
			printer.PrintGovtTaxInvoices(invoicesToPrint);

			ZQuery query = new ZQuery();
			BusinessObject[] printJobs = TestObjectCreator.Factory.Load(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintJob>(), query);
			AssertEquals("Should be 0 print jobs", 0, printJobs.Length);
			AssertEquals("LastFormShownDialogForTest", "Enterprise.DocumentEngine.GUI.DocDeliveryForm", ZFormModaliser.LastFormShownDialogForTest.GetType().ToString());
		}

		public void TestPrintGovtTaxInvoicesWithEnterpriseInvoiceOnly_BothWithoutPrinter()
		{
			SetupSequenceNoneWithPrinter();
			GovtTaxInvoicePrinter printer = new GovtTaxInvoicePrinter(GovtTaxInvoicePrintTask.EnterpriseInvoice);
			TransactionHeader[] invoicesToPrint = new TransactionHeader[] { invoice1, invoice2 };
			printer.PrintGovtTaxInvoices(invoicesToPrint);

			ZQuery query = new ZQuery();
			BusinessObject[] printJobs = TestObjectCreator.Factory.Load(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintJob>(), query);
			AssertEquals("Should be 0 print jobs", 0, printJobs.Length);
			AssertEquals("LastFormShownDialogForTest", "Enterprise.DocumentEngine.GUI.DocDeliveryForm", ZFormModaliser.LastFormShownDialogForTest.GetType().ToString());
		}

		public void TestPrintGovtTaxInvoicesWithBothTypesOfInvoices_BothWithoutPrinter()
		{
			SetupSequenceNoneWithPrinter();
			GovtTaxInvoicePrinter printer = new GovtTaxInvoicePrinter(GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice);
			TransactionHeader[] invoicesToPrint = new TransactionHeader[] { invoice1, invoice2 };
			printer.PrintGovtTaxInvoices(invoicesToPrint);

			ZQuery query = new ZQuery();
			BusinessObject[] printJobs = TestObjectCreator.Factory.Load(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintJob>(), query);
			AssertEquals("Should be 0 print jobs", 0, printJobs.Length);
			AssertEquals("LastFormShownDialogForTest", "Enterprise.DocumentEngine.GUI.DocDeliveryForm", ZFormModaliser.LastFormShownDialogForTest.GetType().ToString());
		}

		public void TestPrintGovtTaxInvoicesWithZSaveConcurrencyException()
		{
			SetupOneSequenceWithPrinterAndOneWithoutPrinter();
			var printer = new GovtTaxInvoicePrinterWithConcurrencyException(GovtTaxInvoicePrintTask.EnterpriseInvoice);
			var invoices = new TransactionHeader[] { invoice1, invoice2 };

			AssertNoExceptionThrown("ZSaveConcurrencyException should be handled", () => printer.PrintGovtTaxInvoices(invoices));
		}

		class GovtTaxInvoicePrinterWithConcurrencyException : GovtTaxInvoicePrinter
		{
			public GovtTaxInvoicePrinterWithConcurrencyException(ZString invoicePrintingOptionCode)
				: base(invoicePrintingOptionCode)
			{
			}

			protected override void AllocateComplianceSequenceNumberAndPrint(ZString invoicePrintingOptionCode, TransactionHeader[] transactionsReloaded, BusinessObjectFactory factory)
			{
				base.AllocateComplianceSequenceNumberAndPrint(invoicePrintingOptionCode, transactionsReloaded, factory);
				throw new ZSaveConcurrencyException(new ZDataConcurrencyException(null, null, null), factory);
			}
		}

		public void TestAllocateComplianceNumberWithoutPrinting_WhenComplianceBookHasNoPrintingInfo()
		{
			var sequenceTXI = TestObjectCreator.SetupComplianceSequence(ZGuid.Empty, "TXI", "ABC", 1, 100, 50);
			var sequenceTCR = TestObjectCreator.SetupComplianceSequence(ZGuid.Empty, "TCR", "XYZ", 1000, 2000, 1500);
			invoice1.AH_TransactionReference = ZString.Empty;
			invoice2.AH_TransactionReference = ZString.Empty;
			TestObjectCreator.Factory.Save();

			GovtTaxInvoicePrinter printer = new GovtTaxInvoicePrinter(GovtTaxInvoicePrintTask.AllocateSequenceNumberOnly);
			printer.PrintGovtTaxInvoices(new TransactionHeader[] { invoice1, invoice2 });

			Assert("Transaction Reference shouldn't be empty", !invoice1.AH_TransactionReference.IsEmpty);
			AssertEquals("Invoice Compliance Sequence Number", "ABC000000050", invoice1.AH_TransactionReference);
			Assert("Transaction Reference shouldn't be empty", !invoice2.AH_TransactionReference.IsEmpty);
			AssertEquals("Invoice Compliance Sequence Number", "XYZ000001500", invoice2.AH_TransactionReference);

			BusinessObject[] printJobs = TestObjectCreator.Factory.Load(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintJob>(), new ZQuery());
			AssertEquals("Should be no print jobs (because we're only allocating the compliance sequence number))", 0, printJobs.Length);
		}

		public void TestAllocateComplianceNumberWithoutPrinting_WhenComplianceBookHasPrePrintedConfigurations()
		{
			//Setup a compliance book with pre-printed document
			SetupSequenceBothWithPrinter();
			sequenceTXI.XD_RollupBehaviourWhenMaxExceeded = Constants.ComplianceRollupBehaviourType.SinglePageReferAttached;
			sequenceTCR.XD_RollupBehaviourWhenMaxExceeded = Constants.ComplianceRollupBehaviourType.SinglePageSummarize;
			invoice1.AH_TransactionReference = ZString.Empty;
			invoice2.AH_TransactionReference = ZString.Empty;
			TestObjectCreator.Factory.Save();

			GovtTaxInvoicePrinter printer = new GovtTaxInvoicePrinter(GovtTaxInvoicePrintTask.AllocateSequenceNumberOnly);
			printer.PrintGovtTaxInvoices(new TransactionHeader[] { invoice1, invoice2 });

			Assert("Transaction Reference should remain empty because compliance book as preprinted configuration", invoice1.AH_TransactionReference.IsEmpty);
			Assert("Transaction Reference should remain empty because compliance book as preprinted configuration", invoice2.AH_TransactionReference.IsEmpty);

			string message = "Some compliance sequence numbers were not allocated because the compliance book/s used have pre-printed invoice configurations. The compliance sequence number will be allocated when you print the document.";
			AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestAllocateComplianceNumberWithoutPrinting_WhenComplianceBookHasNoSeparateDocumentRequiredConfigurations()
		{
			SetupSequenceBothWithPrinter();
			sequenceTXI.XD_RollupBehaviourWhenMaxExceeded = Constants.ComplianceRollupBehaviourType.MultiPageNoLimitation; //This means the document can be printed at any time and isn't printed to pre-printed paper
			sequenceTCR.XD_RollupBehaviourWhenMaxExceeded = Constants.ComplianceRollupBehaviourType.MultiPageNoLimitation; //This means the document can be printed at any time and isn't printed to pre-printed paper
			invoice1.AH_TransactionReference = ZString.Empty;
			invoice2.AH_TransactionReference = ZString.Empty;
			TestObjectCreator.Factory.Save();

			GovtTaxInvoicePrinter printer = new GovtTaxInvoicePrinter(GovtTaxInvoicePrintTask.AllocateSequenceNumberOnly);
			printer.PrintGovtTaxInvoices(new TransactionHeader[] { invoice1, invoice2 });

			Assert("Transaction Reference shouldn't be empty", !invoice1.AH_TransactionReference.IsEmpty);
			AssertEquals("Invoice Compliance Sequence Number", "ABC000000050", invoice1.AH_TransactionReference);
			Assert("Transaction Reference shouldn't be empty", !invoice2.AH_TransactionReference.IsEmpty);
			AssertEquals("Invoice Compliance Sequence Number", "XYZ000001500", invoice2.AH_TransactionReference);

			BusinessObject[] printJobs = TestObjectCreator.Factory.Load(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintJob>(), new ZQuery());
			AssertEquals("Should be no print jobs (because we're only allocating the compliance sequence number))", 0, printJobs.Length);
		}

		public void TestOnePrinterCanNotHandleDifferentsubTyes()
		{
			SetupSequenceBothWithSamePrinter();
			GovtTaxInvoicePrinter printer = new GovtTaxInvoicePrinter(GovtTaxInvoicePrintTask.GovtTaxInvoice);
			TransactionHeader[] invoicesToPrint = new TransactionHeader[] { invoice1, invoice2 };
			printer.PrintGovtTaxInvoices(invoicesToPrint);

			ZQuery query = new ZQuery();
			BusinessObject[] printJobs = TestObjectCreator.Factory.Load(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintJob>(), query);
			AssertEquals("Should be 0 print jobs", 0, printJobs.Length);
			AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals(@"Please check the Printer Setups configured against your Compliance Invoice Books.
 When printing to pre-printed pre-numbered paper stock, Active Compliance books cannot share the same printer.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestPrintGovtTaxInvoiceWithPartialAllocationOccured()
		{
			AccountingMasterFilesRegistry.Instance.ComplianceAllowPartialSequenceNumberAllocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetupSequenceBothWithPrinter();
			sequenceTCR.XD_NextNumber = 2001;
			sequenceTCR.XD_IsActive = false;
			invoice1.AH_TransactionReference = "";
			invoice2.AH_TransactionReference = "";
			invoice1.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice1.AH_GC = GlbCompany.CurrentCompany.PK;
			invoice2.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice2.AH_GC = GlbCompany.CurrentCompany.PK;

			TestObjectCreator.Factory.Save();
			GovtTaxInvoicePrinter printer = new GovtTaxInvoicePrinter(GovtTaxInvoicePrintTask.GovtTaxInvoice);
			TransactionHeader[] invoicesToPrint = new TransactionHeader[] { invoice1, invoice2 };
			printer.PrintGovtTaxInvoices(invoicesToPrint);

			ZQuery query = new ZQuery();
			BusinessObject[] printJobs = TestObjectCreator.Factory.Load(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintJob>(), query);
			AssertEquals("Should be 1 print jobs", 1, printJobs.Length);
			AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("Some transactions have skipped assigning a Government Compliance Number because existing Compliance Invoice Book setups are missing or do not have enough remaining numbers available.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestGovernmentPrintTaskBookHasNoMenuErrorOccuredWhenRegistryIsOff()
		{
			SetupSequenceBothWithPrinter();
			GovtTaxInvoicePrinter printer = new GovtTaxInvoicePrinter(GovtTaxInvoicePrintTask.GovtTaxInvoice);
			TransactionHeader[] invoicesToPrint = new TransactionHeader[] { invoice1, invoice2 };

			sequenceTCR.XD_SU_MenuItem = ZGuid.Empty;
			TestObjectCreator.Factory.Save();

			AccountingMasterFilesRegistry.Instance.SuppressShowComplianceBookHasNoTemplateWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			printer.PrintGovtTaxInvoices(invoicesToPrint);

			ZQuery query = new ZQuery();
			BusinessObject[] printJobs = TestObjectCreator.Factory.Load(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintJob>(), query);
			AssertEquals("Should be 1 print jobs", 1, printJobs.Length);
			AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals(@"Some transactions have skipped printing due to their Compliance Invoice Book setups do not fully support government invoice printing: No Compliance Invoice Document will be Printed.
 This Compliance Book is not configured for printing.
 If you want to print a Compliance Document please amend your Compliance Book setups and nominate an appropriate document menu for printing.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestGovernmentPrintTaskBookHasNoMenuErrorOccuredWhenRegistryIsOn()
		{
			SetupSequenceBothWithPrinter();
			GovtTaxInvoicePrinter printer = new GovtTaxInvoicePrinter(GovtTaxInvoicePrintTask.GovtTaxInvoice);
			TransactionHeader[] invoicesToPrint = new TransactionHeader[] { invoice1, invoice2 };

			sequenceTCR.XD_SU_MenuItem = ZGuid.Empty;
			TestObjectCreator.Factory.Save();

			AccountingMasterFilesRegistry.Instance.SuppressShowComplianceBookHasNoTemplateWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			printer.PrintGovtTaxInvoices(invoicesToPrint);

			ZQuery query = new ZQuery();
			BusinessObject[] printJobs = TestObjectCreator.Factory.Load(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintJob>(), query);
			AssertEquals("Should be 1 print jobs", 1, printJobs.Length);
			AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("Warning message should be suppressed due to registry setting", null, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestGovernmentPrintTaskBookDoesNotAskReallocationQuestionIfComplianceDocumentAllocationIsManual()
		{
			SetupSequenceBothWithPrinter();
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print))
			{
				var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "10001", TestObjectCreator.AUD, 1.0m, 100.0m, 0.0m, 100.0m, 0.0m, TestObjectCreator.AALSHI, TestObjectCreator.CC10.PK);
				invoice = TestObjectCreator.SetupComplianceInvoice("TXI");
				invoice.AH_TransactionReference = "ABC000000001";

				var transactions = new TransactionHeader[] { invoice };
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.RepeatAnswerForEntireSessionUserOption = true;

				GovtTaxInvoicePrinter printer = new GovtTaxInvoicePrinter(GovtTaxInvoicePrintTask.GovtTaxInvoice);
				TransactionHeader[] invoicesToPrint = new TransactionHeader[] { invoice };
				printer.PrintGovtTaxInvoices(invoicesToPrint);

				AssertEquals("Compliance number should be re-allocated", "ABC000000050", invoice.AH_TransactionReference);
				AssertEquals("Last Message Text: Print - The last message should be the question for re-allocation", "Do you want to assign a new sequence number instead of reusing the same number again?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}

			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual))
			{
				var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "10002", TestObjectCreator.AUD, 1.0m, 100.0m, 0.0m, 100.0m, 0.0m, TestObjectCreator.AALSHI, TestObjectCreator.CC10.PK);
				invoice = TestObjectCreator.SetupComplianceInvoice("TXI");
				invoice.AH_TransactionReference = "ABC000000002";

				var transactions = new TransactionHeader[] { invoice };
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				GovtTaxInvoicePrinter printer = new GovtTaxInvoicePrinter(GovtTaxInvoicePrintTask.GovtTaxInvoice);
				TransactionHeader[] invoicesToPrint = new TransactionHeader[] { invoice };
				printer.PrintGovtTaxInvoices(invoicesToPrint);

				AssertEquals("Print task should not re-allocate compliance number", "ABC000000002", invoice.AH_TransactionReference);
				AssertNull("Last Message Text: Manual - The last message should be null", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestGovtInvoiceReprintValidation()
		{
			SetupSequenceBothWithSamePrinter();

			foreach (string printOption in new[] { GovtTaxInvoicePrintTask.GovtTaxInvoice, GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice })
			{
				foreach (bool allowReprint in new[] { true, false })
				{
					var options = FormattableString.Invariant($"{printOption}_{allowReprint}");
					using (AccountingMasterFilesRegistry.Instance.SuppressShowComplianceBookHasNoTemplateWarning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					using (AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowReprint))
					{
						var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "10001", TestObjectCreator.AUD, 1.0m, 100.0m, 0.0m, 100.0m, 0.0m, TestObjectCreator.AALSHI, TestObjectCreator.CC10.PK);
						invoice = TestObjectCreator.SetupComplianceInvoice("TXI");
						invoice.AH_TransactionReference = "ABC000000001";

						var transactions = new TransactionHeader[] { invoice };
						Factory.Save();

						//First Print
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						UnitTestUserNotification.Instance.RepeatAnswerForEntireSessionUserOption = true;
						GovtTaxInvoicePrinter printer = new GovtTaxInvoicePrinter(printOption);
						printer.PrintGovtTaxInvoices(transactions);
						Factory.Save();

						//Attempt to reprint
						printer.PrintGovtTaxInvoices(transactions);
						var expected = allowReprint
							? "Invoice has been printed already. Do you want to reprint it?"
							: FormattableString.Invariant($@"Re-printed versions of a receivables document in your country/region must reflect the data used at the time of posting, hence re-printing of following transactions is not allowed through this module. You can go to the eDocs tab of a transaction and re-print the first invoice version stored there.
AR INV {invoice.AH_TransactionNum}");
						AssertEquals(options + " - Expect a 'not eligible to print' error", expected, UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					}
				}
			}
		}

		[TestDate(2022, 05, 15)]
		public void TestAllocateMultipleComplianceNumbersWithoutPrinting_WhenOrderedByPostDateEnabled_WithErrors()
		{
			var currComp = GlbCompany.CurrentCompany;
			var registry = AccountingMasterFilesRegistry.Instance;
			using (registry.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(currComp.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print))
			using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(currComp.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code))
			{
				var lastDateUsed = new ZDate(2022, 04, 15);
				var subTypeARI = TestObjectCreator.SetupComplianceSequence(ZGuid.Empty, "ARI", "ARI-", 1, 100, 1);

				var previousInvoiceWithEarlierDate = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.EUR, 1);
				previousInvoiceWithEarlierDate.AH_PostDate = lastDateUsed;
				previousInvoiceWithEarlierDate.AH_XD_ComplianceBook = subTypeARI.PK;
				Factory.Save();

				var invoicesWithLastPostError = new ARInvoice[5];
				for (var i = 4; i >= 0; i--)
				{
					invoicesWithLastPostError[i] = TestObjectCreator.SetupComplianceInvoice("ARI", invoiceAndPostDate: lastDateUsed.AddDays(i - 1));
				}

				GovtTaxInvoicePrinter printer = new GovtTaxInvoicePrinter(GovtTaxInvoicePrintTask.AllocateSequenceNumberOnly);
				printer.PrintGovtTaxInvoices(invoicesWithLastPostError);

				var expectedErrorMessage = @"Compliance Numbers cannot be allocated.
 Last posted transaction with the same Compliance Sub Type ARI has Post Date = 15-Apr-22, that is greater than the current one(s).";
				AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();

				var invoicesWithSparseBookError = new ARInvoice[5];
				for (var i = 0; i < 5; i++)
				{
					invoicesWithSparseBookError[i] = TestObjectCreator.SetupComplianceInvoice("ARI", invoiceAndPostDate: lastDateUsed.AddDays(i + 1));
				}

				printer = new GovtTaxInvoicePrinter(GovtTaxInvoicePrintTask.AllocateSequenceNumberOnly);
				printer.PrintGovtTaxInvoices(invoicesWithSparseBookError);

				expectedErrorMessage = @"Compliance Numbers cannot be allocated.
 There is some transaction with the same Compliance Sub Type ARI in earlier Post Date and Compliance Number empty.
 Please allocate Compliance Number to all transactions with Post Date < 16-Apr-22.";
				AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2022, 05, 15)]
		public void TestAllocateMultipleComplianceNumbersWithoutPrinting_WhenOrderedByPostDateEnabled_CorrectlyAllocated()
		{
			var currComp = GlbCompany.CurrentCompany;
			var registry = AccountingMasterFilesRegistry.Instance;
			using (registry.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(currComp.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print))
			using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(currComp.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code))
			{
				var lastDateUsed = new ZDate(2022, 04, 15);
				var subTypeARI = TestObjectCreator.SetupComplianceSequence(ZGuid.Empty, "ARI", "ARI-", 1, 100, 1);

				var invoices = new ARInvoice[5];
				for (var i = 0; i < 5; i++)
				{
					invoices[i] = TestObjectCreator.SetupComplianceInvoice("ARI", invoiceAndPostDate: lastDateUsed.AddDays(i + 1));
				}

				GovtTaxInvoicePrinter printer = new GovtTaxInvoicePrinter(GovtTaxInvoicePrintTask.AllocateSequenceNumberOnly);
				printer.PrintGovtTaxInvoices(invoices);

				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				for (var i = 0; i < 5; i++)
				{
					AssertEquals("Compliance number for Invoice #" + (i + 1), "ARI-00000000" + (i + 1), invoices[i].AH_TransactionReference);
				}
			}
		}

		ARInvoice invoice1, invoice2;
		AccComplianceSequence sequenceTXI, sequenceTCR;
		protected override void SetUp()
		{
			base.SetUp();

			StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Peru);

			invoice1 = TestObjectCreator.SetupComplianceInvoice("TXI");
			invoice2 = TestObjectCreator.SetupComplianceInvoice("TCR");
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice2.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice1.AH_TransactionReference = "ABC000000005";
			invoice2.AH_TransactionReference = "XYZ000001010";
			TestObjectCreator.Factory.Save();
		}
		protected string StoredCountry;

		protected override void TearDown()
		{
			if (!string.IsNullOrEmpty(StoredCountry) && StoredCountry != GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				GlbCompany.CurrentCompany.SetCountry(StoredCountry);
			}

			base.TearDown();
		}

		void SetupOneSequenceWithPrinterAndOneWithoutPrinter()
		{
			ZGuid menuPK1 = TestObjectCreator.SetupComplianceMenuAndPivot("Govt Compliance Inv With Printer");
			sequenceTXI = TestObjectCreator.SetupComplianceSequence(menuPK1, "TXI", "ABC", 1, 100, 50);
			sequenceTXI.XD_SQ_DocumentPrintQueue = TestObjectCreator.Factory.NewWithValidTestData<StmPrintQueue>().PK;

			ZGuid menuPK2 = TestObjectCreator.SetupComplianceMenuAndPivot("Govt Compliance Crd Without Printer");
			sequenceTCR = TestObjectCreator.SetupComplianceSequence(menuPK2, "TCR", "XYZ", 1000, 2000, 1500);
			TestObjectCreator.Factory.Save();
		}

		void SetupSequenceBothWithPrinter()
		{
			ZGuid menuPK1 = TestObjectCreator.SetupComplianceMenuAndPivot("Govt Compliance Inv With Printer");
			sequenceTXI = TestObjectCreator.SetupComplianceSequence(menuPK1, "TXI", "ABC", 1, 100, 50);
			sequenceTXI.XD_SQ_DocumentPrintQueue = TestObjectCreator.Factory.NewWithValidTestData<StmPrintQueue>().PK;

			ZGuid menuPK2 = TestObjectCreator.SetupComplianceMenuAndPivot("Govt Compliance Crd With Printer");
			sequenceTCR = TestObjectCreator.SetupComplianceSequence(menuPK2, "TCR", "XYZ", 1000, 2000, 1500);
			sequenceTCR.XD_SQ_DocumentPrintQueue = TestObjectCreator.Factory.NewWithValidTestData<StmPrintQueue>().PK;
			TestObjectCreator.Factory.Save();
		}

		void SetupSequenceNoneWithPrinter()
		{
			ZGuid menuPK1 = TestObjectCreator.SetupComplianceMenuAndPivot("Govt Compliance Inv Without Printer");
			sequenceTXI = TestObjectCreator.SetupComplianceSequence(menuPK1, "TXI", "ABC", 1, 100, 50);

			ZGuid menuPK2 = TestObjectCreator.SetupComplianceMenuAndPivot("Govt Compliance Crd Without Printer");
			sequenceTCR = TestObjectCreator.SetupComplianceSequence(menuPK2, "TCR", "XYZ", 1000, 2000, 1500);
			TestObjectCreator.Factory.Save();
		}

		void SetupSequenceBothWithSamePrinter()
		{
			ZGuid menuPK1 = TestObjectCreator.SetupComplianceMenuAndPivot("Govt Compliance Inv With Printer");
			sequenceTXI = TestObjectCreator.SetupComplianceSequence(menuPK1, "TXI", "ABC", 1, 100, 50);
			sequenceTXI.XD_SQ_DocumentPrintQueue = TestObjectCreator.Factory.NewWithValidTestData<StmPrintQueue>().PK;

			ZGuid menuPK2 = TestObjectCreator.SetupComplianceMenuAndPivot("Govt Compliance Crd Without Printer");
			sequenceTCR = TestObjectCreator.SetupComplianceSequence(menuPK2, "TCR", "XYZ", 1000, 2000, 1500);
			sequenceTCR.XD_SQ_DocumentPrintQueue = sequenceTXI.XD_SQ_DocumentPrintQueue;
			TestObjectCreator.Factory.Save();
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		TestObjectCreator fTestObjectCreator;
	}
}
