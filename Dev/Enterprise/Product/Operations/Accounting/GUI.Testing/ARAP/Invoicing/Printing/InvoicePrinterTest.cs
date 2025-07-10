using System;
using System.Collections;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP.Payment;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Shared;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.Testing
{
	public class InvoicePrinterTest : TestCaseWithFactory
	{
		protected virtual Type GetAPInvoiceType()
		{
			return typeof(APInvoice);
		}

		protected virtual Type GetARInvoiceType()
		{
			return typeof(ARInvoice);
		}

		class PrintInvoicePrinter : NoPrintInvoicePrinter
		{
			public PrintInvoicePrinter(DialogResult userChoice)
				: base(userChoice)
			{ }

			protected override void RunPrint(InvoicePrintTask printTask)
			{
				printTask.Run();
				docPackCount = printTask.TaskCount;
				reportCount = printTask[0].Count;
			}
			public int docPackCount;
			public int reportCount;
		}

		#region class NoPrintInvoicePrinter

		class NoPrintInvoicePrinter : InvoicePrinter
		{
			public NoPrintInvoicePrinter(DialogResult userChoice)
				: base()
			{
				fUserChoice = userChoice;
			}

			protected override DialogResult GetUserChoice(string message, string caption, MessageBoxButtons messageBoxButton, MessageBoxIcon messageBoxIcon)
			{
				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.AddAnswer(fUserChoice);
				return base.GetUserChoice(message, caption, messageBoxButton, messageBoxIcon);
			}

			readonly DialogResult fUserChoice;

			public ArrayList PrintingResults = new ArrayList();
		}

		TransactionPrintingResults DoTest(ZString oH_RL_NKClosestPort, String countryCode)
		{
			return DoTest(oH_RL_NKClosestPort, countryCode, DialogResult.Yes);
		}

		TransactionPrintingResults DoTest(ZString oH_RL_NKClosestPort, String countryCode, DialogResult userChoice)
		{
			TransactionPrintingResults result;

			NoPrintInvoicePrinter testPrinter = new NoPrintInvoicePrinter(userChoice);

			ARInvoice testARInv = Factory.NewWithValidTestData<ARInvoice>();
			using (ZForm testForm = new ZForm())
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
				OrgHeader testChinaOrg = Factory.NewWithValidTestData<OrgHeader>();
				testChinaOrg.OH_RL_NKClosestPort = oH_RL_NKClosestPort;
				Factory.Save();

				testARInv.AH_OH = testChinaOrg.PK;
				Factory.Save();

				result = testPrinter.PrintTransaction(testARInv, testForm, InvoicePrintContext.DontCare);
			}

			return result;
		}

		#endregion

		public void TestSelfBillingTemplateIsUsed()
		{
			InvoicePrinter printer = new InvoicePrinter();
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			InvoicingBase invoice = (InvoicingBase)Factory.NewWithValidTestData(GetAPInvoiceType());
			testObjectCreator.AALSHI.CompanyData.OB_APCostsSelfBilled = true;
			invoice.AH_OH = testObjectCreator.AALSHI.PK;
			Factory.Save();
			var printTaskRan = false;
			InvoicePrintHelper.ResetPrintTaskRun_ForTestOnly();
			InvoicePrintHelper.PrintTaskRun_ForTestOnly += delegate(InvoicePrintTask printTask)
			{
				printTaskRan = true;
				AssertEquals("There should be 1 reports in the pack", 1, printTask[0].Count);
				AssertEquals("APSelfBillingInvoice", printTask[0][0].Name);
			};
			printer.PrintSelfBillingInvoice_ForTestOnly(invoice);
			Assert("Print task should be run", printTaskRan);
		}

		public void TestPrintCostConfirmationDocument()
		{
			var menuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.PK, new ZGuid(InvoicePrintTask.MenuPkForAPInvoice)));
			var defaultPrinter = StmDefaultPrinter.LoadOrCreateDefaultPrinter(Factory, GlbStaff.CurrentUser, menuItem);
			defaultPrinter.SDP_SQ_Printer = Factory.New<StmPrintQueue>().PK;
			defaultPrinter.SDP_NumberOfCopies = 1;
			Factory.Save();

			InvoicePrinter printer = new InvoicePrinter();
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			InvoicingBase invoice = (InvoicingBase)Factory.NewWithValidTestData(GetAPInvoiceType());
			invoice.AH_OH = testObjectCreator.AALSHI.PK;
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			AccountingConfigurationRegistry.Instance.CostConfirmationDocumentSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.CostConfirmationDocumentSettingsCodes.Both);
			var printTaskRan = false;
			InvoicePrintHelper.ResetPrintTaskRun_ForTestOnly();
			InvoicePrintHelper.PrintTaskRun_ForTestOnly += delegate(InvoicePrintTask printTask)
			{
				printTaskRan = true;
				AssertEquals("There should be 2 reports in the pack", 2, printTask[0].Count);
				AssertEquals("Cost Confirmation Summary", printTask[0][0].Name);
				AssertEquals("Cost Confirmation Document", printTask[0][1].Name);
			};
			printer.PrintCostConfirmationDocument_ForTestOnly(invoice);
			Assert("Print task should be run", printTaskRan);

			AccountingConfigurationRegistry.Instance.CostConfirmationDocumentSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.CostConfirmationDocumentSettingsCodes.Summary);
			printer = new InvoicePrinter();
			printTaskRan = false;
			InvoicePrintHelper.ResetPrintTaskRun_ForTestOnly();
			InvoicePrintHelper.PrintTaskRun_ForTestOnly += delegate(InvoicePrintTask printTask)
			{
				printTaskRan = true;
				AssertEquals("There should be 1 reports in the pack", 1, printTask[0].Count);
				AssertEquals("Cost Confirmation Summary", printTask[0][0].Name);
			};
			printer.PrintCostConfirmationDocument_ForTestOnly(invoice);
			Assert("Print task should be run", printTaskRan);

			AccountingConfigurationRegistry.Instance.CostConfirmationDocumentSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.CostConfirmationDocumentSettingsCodes.Detail);
			printer = new InvoicePrinter();
			printTaskRan = false;
			InvoicePrintHelper.ResetPrintTaskRun_ForTestOnly();
			InvoicePrintHelper.PrintTaskRun_ForTestOnly += delegate(InvoicePrintTask printTask)
			{
				printTaskRan = true;
				AssertEquals("There should be 1 reports in the pack", 1, printTask[0].Count);
				AssertEquals("Cost Confirmation Document", printTask[0][0].Name);
			};
			printer.PrintCostConfirmationDocument_ForTestOnly(invoice);
			Assert("Print task should be run", printTaskRan);
		}

		public void TestPrintITAutofattura()
		{
			NoPrintInvoicePrinter testPrinter = new NoPrintInvoicePrinter(DialogResult.Yes);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			using (DocumentsDataRegistry.Instance.UseNewDocBuilderCostConfirmationDocument.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ZForm testForm = new ZForm())
			{
				APInvoice testAPInvoice = Factory.NewWithValidTestData<APInvoice>();
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPrinter.PrintTransaction(testAPInvoice, testForm, InvoicePrintContext.DontCare);
				Assert("No request for printing Autofattura (Registry false, No/Wrong SubType, No Compliance Number)", !UnitTestUserNotification.Instance.LastMessage.WasQuestion);

				AccountingConfigurationRegistry.Instance.PrintAutofatturaItalyDocument.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPrinter.PrintTransaction(testAPInvoice, testForm, InvoicePrintContext.DontCare);
				Assert("No request for printing Autofattura (Registry true, No/Wrong SubType, No Compliance Number)", !UnitTestUserNotification.Instance.LastMessage.WasQuestion);

				testAPInvoice.AH_ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.APS;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPrinter.PrintTransaction(testAPInvoice, testForm, InvoicePrintContext.DontCare);
				Assert("No request for printing Autofattura (Registry true, Correct SubType, No Compliance Number)", !UnitTestUserNotification.Instance.LastMessage.WasQuestion);

				testAPInvoice.AH_ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.APS;
				testAPInvoice.AH_TransactionReference = "APS-00001";
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPrinter.PrintTransaction(testAPInvoice, testForm, InvoicePrintContext.DontCare);
				Assert("The last message should ask user if they wish to print", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				Assert("The last message should ask user if they wish to print Autofattura", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("Do you want to print the Autofattura document for this transaction?"));

				AccountingConfigurationRegistry.Instance.PrintAutofatturaItalyDocument.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

				APCreditNote testAPCreditNote = Factory.NewWithValidTestData<APCreditNote>();
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPrinter.PrintTransaction(testAPCreditNote, testForm, InvoicePrintContext.DontCare);
				Assert("No request for printing Autofattura (Registry false, No/Wrong SubType, No Compliance Number)", !UnitTestUserNotification.Instance.LastMessage.WasQuestion);

				AccountingConfigurationRegistry.Instance.PrintAutofatturaItalyDocument.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPrinter.PrintTransaction(testAPCreditNote, testForm, InvoicePrintContext.DontCare);
				Assert("No request for printing Autofattura (Registry true, No/Wrong SubType, No Compliance Number)", !UnitTestUserNotification.Instance.LastMessage.WasQuestion);

				testAPCreditNote.AH_ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.APS;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPrinter.PrintTransaction(testAPCreditNote, testForm, InvoicePrintContext.DontCare);
				Assert("No request for printing Autofattura (Registry true, Correct SubType, No Compliance Number)", !UnitTestUserNotification.Instance.LastMessage.WasQuestion);

				testAPCreditNote.AH_ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.APS;
				testAPCreditNote.AH_TransactionReference = "APS-00001";
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPrinter.PrintTransaction(testAPCreditNote, testForm, InvoicePrintContext.DontCare);
				Assert("The last message should ask user if they wish to print", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				Assert("The last message should ask user if they wish to print Autofattura", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("Do you want to print the Autofattura document for this transaction?"));

				AccountingConfigurationRegistry.Instance.PrintAutofatturaItalyDocument.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

				APAdjustmentNote testAPAdjustmentNote = Factory.NewWithValidTestData<APAdjustmentNote>();
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPrinter.PrintTransaction(testAPAdjustmentNote, testForm, InvoicePrintContext.DontCare);
				Assert("No request for printing Autofattura (Registry false, No/Wrong SubType, No Compliance Number)", !UnitTestUserNotification.Instance.LastMessage.WasQuestion);

				AccountingConfigurationRegistry.Instance.PrintAutofatturaItalyDocument.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPrinter.PrintTransaction(testAPAdjustmentNote, testForm, InvoicePrintContext.DontCare);
				Assert("No request for printing Autofattura (Registry true, No/Wrong SubType, No Compliance Number)", !UnitTestUserNotification.Instance.LastMessage.WasQuestion);

				testAPAdjustmentNote.AH_ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.APS;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPrinter.PrintTransaction(testAPAdjustmentNote, testForm, InvoicePrintContext.DontCare);
				Assert("No request for printing Autofattura (Registry true, Correct SubType, No Compliance Number)", !UnitTestUserNotification.Instance.LastMessage.WasQuestion);

				testAPAdjustmentNote.AH_ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.APS;
				testAPAdjustmentNote.AH_TransactionReference = "APS-00001";
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPrinter.PrintTransaction(testAPAdjustmentNote, testForm, InvoicePrintContext.DontCare);
				Assert("The last message should ask user if they wish to print", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				Assert("The last message should ask user if they wish to print Autofattura", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("Do you want to print the Autofattura document for this transaction?"));

				AccountingConfigurationRegistry.Instance.PrintAutofatturaItalyDocument.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

				UAInvoice testUAInvoice = Factory.NewWithValidTestData<UAInvoice>();
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPrinter.PrintTransaction(testUAInvoice, testForm, InvoicePrintContext.DontCare);
				Assert("No request for printing Autofattura (Registry false, No/Wrong SubType, No Compliance Number)", !UnitTestUserNotification.Instance.LastMessage.WasQuestion);

				AccountingConfigurationRegistry.Instance.PrintAutofatturaItalyDocument.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPrinter.PrintTransaction(testUAInvoice, testForm, InvoicePrintContext.DontCare);
				Assert("No request for printing Autofattura (Registry true, No/Wrong SubType, No Compliance Number)", !UnitTestUserNotification.Instance.LastMessage.WasQuestion);

				testUAInvoice.AH_ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.APS;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPrinter.PrintTransaction(testUAInvoice, testForm, InvoicePrintContext.DontCare);
				Assert("No request for printing Autofattura (Registry true, Correct SubType, No Compliance Number)", !UnitTestUserNotification.Instance.LastMessage.WasQuestion);

				testUAInvoice.AH_ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.APS;
				testUAInvoice.AH_TransactionReference = "APS-00001";
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPrinter.PrintTransaction(testUAInvoice, testForm, InvoicePrintContext.DontCare);
				Assert("The last message should ask user if they wish to print", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				Assert("The last message should ask user if they wish to print Autofattura", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("Do you want to print the Autofattura document for this transaction?"));
			}
		}

		public void TestPrintOnlyWhenIsInDatabase()
		{
			NoPrintInvoicePrinter testPrinter = new NoPrintInvoicePrinter(DialogResult.Yes);
			APPayment testAPPayment = Factory.NewWithValidTestData<APPayment>();

			using (ZForm testForm = new ZForm())
			{
				AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

				TransactionPrintingResults result = testPrinter.PrintTransaction(testAPPayment, testForm, InvoicePrintContext.DontCare);

				AssertEquals(TransactionPrintingResults.None, result);
				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
				AssertContains(@"Attempt to print document for unsaved transaction", ExceptionReporterTestListener.Instance[0].Message);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestPrintShowWarningMessageWhenNotInDatabase()
		{
			NoPrintInvoicePrinter testPrinter = new NoPrintInvoicePrinter(DialogResult.Yes);
			InvoicingBase testInvoice = (InvoicingBase)Factory.NewWithValidTestData(GetARInvoiceType());

			using (ZForm testForm = new ZForm())
			{
				AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

				TransactionPrintingResults result = testPrinter.PrintTransaction(testInvoice, testForm, InvoicePrintContext.DontCare, true);

				AssertEquals(TransactionPrintingResults.None, result);
				AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

				AssertEquals("Attempt to print document for unsaved transaction", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPrintTransactionChecksCanPrint_NoInvoiceEDoc_IsPrinted()
		{
			using (AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (ZForm testForm = new ZForm())
			{
				var testARInv = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();
				Assert("Can print", testARInv.CheckCanPrintPostedInvoicingBase().Result);

				var testPrinter = new NoPrintInvoicePrinter(DialogResult.Yes);
				var result = testPrinter.PrintTransaction(testARInv, testForm, InvoicePrintContext.DontCare, true);
				AssertEquals("Invoice printed", TransactionPrintingResults.TransactionIsNotClassAInvoice, result);

				testARInv.AH_InvoicePrinted = true;
				Factory.Save();

				var expectedReason = @"Re-printed versions of a receivables document in your country/region must reflect the data used at the time of posting, re-printing is not allowed through this module.
Please go to the eDocs tab of the transaction and re-print the first invoice version stored there.";
				var canPrint = testARInv.CheckCanPrintPostedInvoicingBase();
				Assert("Cannot print", !canPrint.Result);
				AssertEquals("Reason", expectedReason, canPrint.ReasonForNotBeingAbleToPrint);

				testPrinter = new NoPrintInvoicePrinter(DialogResult.Yes);
				var expectedMessage = $@"Cannot print AR INV {testARInv.AH_TransactionNum}.
Reason: {expectedReason}";
				result = testPrinter.PrintTransaction(testARInv, testForm, InvoicePrintContext.DontCare, true);
				AssertEquals("No invoice printed", TransactionPrintingResults.None, result);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPrintTransactionChecksCanPrint_WithInvoiceEDoc()
		{
			using (AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (ZForm testForm = new ZForm())
			{
				var testARInv = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				var eDoc = (StorageDocsBase)testARInv.DocManagerInfo.AddFileOrDocument(contents: new byte[] { 0 }, filenameOnly: "Invoice", documentType: "INV", description: "Printed Invoice");
				eDoc.SC_IsSystemGenerated = true;
				testARInv.DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(false);
				Factory.Save();

				var testARInvReloaded = new BusinessObjectFactory().Load<ARInvoice>(testARInv.PK);

				var expectedReason = @"Re-printed versions of a receivables document in your country/region must reflect the data used at the time of posting, re-printing is not allowed through this module.
Please go to the eDocs tab of the transaction and re-print the first invoice version stored there.";
				var canPrint = testARInvReloaded.CheckCanPrintPostedInvoicingBase();
				Assert("Cannot print", !canPrint.Result);
				AssertEquals("Reason", expectedReason, canPrint.ReasonForNotBeingAbleToPrint);

				var testPrinter = new NoPrintInvoicePrinter(DialogResult.Yes);
				var expectedMessage = $@"Cannot print AR INV {testARInvReloaded.AH_TransactionNum}.
Reason: {expectedReason}";
				var result = testPrinter.PrintTransaction(testARInvReloaded, testForm, InvoicePrintContext.DontCare, true);
				AssertEquals("No invoice printed", TransactionPrintingResults.None, result);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPrintNormalInvoice()
		{
			var jobInvoice = new NoPrintInvoicePrinter(DialogResult.Yes);
			var mockARInvoice = Factory.New<ARInvoice>();

			try
			{
				ErrorReporter.Clear();
				Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
				jobInvoice.PrintNormalInvoice_ForTestOnly(mockARInvoice, "", "", InvoicePrintContext.DontCare);
				AssertEquals("The user requested printing of an item, but no print tasks were generated. Check that the post manager posts before printing.", ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestPrintNormalInvoice_CatchComplianceRelatedException()
		{
			var creator = new TestObjectCreator(Factory);
			var testPrinter = new NoPrintInvoicePrinter(DialogResult.Yes);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			{
				var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				sequence.XD_SequenceClass = "TXI";
				sequence.XD_StartNumber = 1;
				sequence.XD_EndNumber = 20;
				sequence.XD_NextNumber = 2;
				sequence.XD_Prefix = "AA";
				sequence.XD_MaximumNumberDigits = 8;
				sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
				sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
				Factory.Save();

				var invoice = creator.CreateARInvoice<ARInvoice>("001", creator.VND, 1.0m, creator.ABIGAS);
				invoice.AH_ComplianceSubType = "TXI";
				invoice.AH_TransactionReference = "AA00000001";
				var line = creator.CreateARInvoiceLine(invoice, null, creator.CC1, creator.VND, 1.0m, "Desc", 100m);
				line.AL_AT = creator.GST1.PK;
				Factory.Save();

				AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

				AssertNoExceptionThrown(() => testPrinter.PrintNormalInvoice_ForTestOnly(invoice, "", "", InvoicePrintContext.DontCare));

				AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

				AssertEquals("No Compliance Invoice Document will be Printed.\r\n This Compliance Book is not configured for printing.\r\n If you want to print a Compliance Document please amend your Compliance Book setups and nominate an appropriate document menu for printing.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPrintNormalInvoice__PrintCommonInvoiceforVN_NoComplianceRelatedException()
		{
			var creator = new TestObjectCreator(Factory);
			var testPrinter = new NoPrintInvoicePrinter(DialogResult.Yes);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ENT"))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				sequence.XD_SequenceClass = "TXI";
				sequence.XD_StartNumber = 1;
				sequence.XD_EndNumber = 20;
				sequence.XD_NextNumber = 2;
				sequence.XD_Prefix = "AA";
				sequence.XD_MaximumNumberDigits = 8;
				sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
				sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
				Factory.Save();

				var invoice = creator.CreateARInvoice<ARInvoice>("001", creator.VND, 1.0m, creator.ABIGAS);
				invoice.AH_ComplianceSubType = "TXI";
				invoice.AH_TransactionReference = "AA00000001";
				var line = creator.CreateARInvoiceLine(invoice, null, creator.CC1, creator.VND, 1.0m, "Desc", 100m);
				line.AL_AT = creator.GST1.PK;
				Factory.Save();

				AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

				AssertNoExceptionThrown(() => testPrinter.PrintNormalInvoice_ForTestOnly(invoice, "", "", InvoicePrintContext.DontCare));

				AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

				AssertEquals(string.Empty, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRePrintNormalInvoice()
		{
			using (AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (ZForm testForm = new ZForm())
			{
				var testARInv = Factory.NewWithValidTestData<ARInvoice>();
				testARInv.AH_InvoicePrinted = true;
				Factory.Save();

				Assert("Can not Print", !testARInv.CheckCanPrintPostedInvoicingBase().Result);

				var result = new NoPrintInvoicePrinter(DialogResult.Yes).PrintARTransaction_ForTestOnly(testForm, InvoicePrintContext.DontCare, testARInv);
				AssertEquals(AccountingConstants.ReprintingInvoiceMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
			}
		}

		public void TestPrintATransaction_CountryInChinaOrgInChina()
		{
			TransactionPrintingResults result = DoTest("CNSHA", Core.Constants.CountryCodes.China);
			AssertEquals(TransactionPrintingResults.TransactionIsNotClassAInvoice, result);
		}

		public void TestPrintATransaction_CountryInChinaOrgNotInChina()
		{
			TransactionPrintingResults result = DoTest("AUSYD", Core.Constants.CountryCodes.China);
			AssertEquals(TransactionPrintingResults.TransactionIsNotClassAInvoice, result);
		}

		public void TestPrintATransaction_CountryNotInChinaOrgInChina()
		{
			TransactionPrintingResults result = DoTest("CNSHA", Core.Constants.CountryCodes.Australia);
			AssertEquals(TransactionPrintingResults.TransactionIsNotClassAInvoice, result);
		}

		public void TestPrintATransaction_CountryNotInChinaOrgNotInChina()
		{
			TransactionPrintingResults result = DoTest("AUSYD", Core.Constants.CountryCodes.Australia);
			AssertEquals(TransactionPrintingResults.TransactionIsNotClassAInvoice, result);
		}

		public void TestPrintAnAPPayment()
		{
			NoPrintInvoicePrinter testPrinter = new NoPrintInvoicePrinter(DialogResult.Yes);
			APPayment testAPPayment = Factory.NewWithValidTestData<APPayment>();
			Factory.Save();

			using (ZForm testForm = new ZForm())
			{
				TransactionPrintingResults result = testPrinter.PrintTransaction(testAPPayment, testForm, InvoicePrintContext.DontCare);
				AssertEquals(TransactionPrintingResults.TransactionIsAPPayment, result);

				Assert("ChequeIsAutoPrinted flag was not set on print manager while printing APPayment", !testPrinter.ChequeIsAutoPrintedFlagWasSetOnPrintManager);

				AssertEquals("Should have opened Payment Documents Print Popup", typeof(PaymentDocumentsPrintPopup), ZFormModaliser.LastFormShownDialogForTest.GetType());
				((PaymentDocumentsPrintPopup)ZFormModaliser.LastFormShownDialogForTest).Close();
			}
		}

		public void TestPrintAnAPInvoice()
		{
			NoPrintInvoicePrinter testPrinter = new NoPrintInvoicePrinter(DialogResult.Yes);

			using (ZForm testForm = new ZForm())
			{
				APInvoice testAPInvoice = Factory.NewWithValidTestData<APInvoice>();
				testAPInvoice.IsSelfBillingInvoice = true;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPrinter.PrintTransaction(testAPInvoice, testForm, InvoicePrintContext.DontCare);

				Assert("The last message should ask user if they wish to print", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				Assert("The last message should ask user if they wish to print Self Billing AP Invoice", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(string.Format("Do you want to print Self Billing Invoice {0}", testAPInvoice.AH_TransactionNum)));

				APCreditNote testAPCreditNote = Factory.NewWithValidTestData<APCreditNote>();
				testAPCreditNote.IsSelfBillingInvoice = true;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPrinter.PrintTransaction(testAPCreditNote, testForm, InvoicePrintContext.DontCare);

				Assert("The last message should ask user if they wish to print", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				Assert("The last message should ask user if they wish to print Self Billing AP Credit Note", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(string.Format("Do you want to print Self Billing Credit Note {0}", testAPCreditNote.AH_TransactionNum)));

				AccountingConfigurationRegistry.Instance.PrintOptionWhenAPInvoicePosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				testAPInvoice = Factory.NewWithValidTestData<APInvoice>();
				testAPInvoice.IsSelfBillingInvoice = false;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPrinter.PrintTransaction(testAPInvoice, testForm, InvoicePrintContext.DontCare);

				Assert("The last message should ask user if they wish to print", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				Assert("The last message should ask user if they wish to print Self Billing AP Invoice", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("Do you want to print a Cost Confirmation Document for this transaction?"));

				testAPCreditNote = Factory.NewWithValidTestData<APCreditNote>();
				testAPCreditNote.IsSelfBillingInvoice = false;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPrinter.PrintTransaction(testAPCreditNote, testForm, InvoicePrintContext.DontCare);

				Assert("The last message should ask user if they wish to print", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				Assert("The last message should ask user if they wish to print Self Billing AP Credit Note", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("Do you want to print a Cost Confirmation Document for this transaction?"));

				var testAPInvoiceConvertedToUA = Factory.NewWithValidTestData<APInvoice>();
				testAPInvoiceConvertedToUA.IsSelfBillingInvoice = true;
				testAPInvoiceConvertedToUA.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
				testAPInvoiceConvertedToUA.AH_TransactionType = TransactionTypes.UAInvoice;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPrinter.PrintTransaction(testAPInvoiceConvertedToUA, testForm, InvoicePrintContext.DontCare);

				Assert("Must be nothing to happen", !UnitTestUserNotification.Instance.LastMessage.WasQuestion);

				var testAPCreditNoteConvertedToUA = Factory.NewWithValidTestData<APCreditNote>();
				testAPCreditNoteConvertedToUA.IsSelfBillingInvoice = true;
				testAPCreditNoteConvertedToUA.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
				testAPCreditNoteConvertedToUA.AH_TransactionType = TransactionTypes.UACreditNote;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPrinter.PrintTransaction(testAPCreditNoteConvertedToUA, testForm, InvoicePrintContext.DontCare);

				Assert("Must be nothing to happen", !UnitTestUserNotification.Instance.LastMessage.WasQuestion);

				UAInvoice testUAInvoice = Factory.NewWithValidTestData<UAInvoice>();
				testUAInvoice.IsSelfBillingInvoice = true;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPrinter.PrintTransaction(testUAInvoice, testForm, InvoicePrintContext.DontCare);

				Assert("Must be nothing to happen", !UnitTestUserNotification.Instance.LastMessage.WasQuestion);

				UACreditNote testUACreditNote = Factory.NewWithValidTestData<UACreditNote>();
				testUACreditNote.IsSelfBillingInvoice = true;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPrinter.PrintTransaction(testUACreditNote, testForm, InvoicePrintContext.DontCare);

				Assert("Must be nothing to happen", !UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			}
		}

		public void TestPrintAnAPPaymentWhenChequeIsAutoPrinted()
		{
			NoPrintInvoicePrinter testPrinter = new NoPrintInvoicePrinter(DialogResult.Yes);
			APPayment testAPPayment = Factory.NewWithValidTestData<APPayment>();
			((IChequeNumberAutoAllocation)testAPPayment).ChequeIsAutoPrinted = ZBool.True;
			Factory.Save();

			using (ZForm testForm = new ZForm())
			{
				TransactionPrintingResults result = testPrinter.PrintTransaction(testAPPayment, testForm, InvoicePrintContext.DontCare);
				AssertEquals(TransactionPrintingResults.TransactionIsAPPayment, result);

				Assert("ChequeIsAutoPrinted flag was set on print manager while printing APPayment", testPrinter.ChequeIsAutoPrintedFlagWasSetOnPrintManager);

				AssertEquals("Should have opened Payment Documents Print Popup", typeof(PaymentDocumentsPrintPopup), ZFormModaliser.LastFormShownDialogForTest.GetType());
				((PaymentDocumentsPrintPopup)ZFormModaliser.LastFormShownDialogForTest).Close();
			}
		}

		#region Enterprise Invoice

		public void TestPrintClassAInvoiceWithRegistrySetToENT()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			NoPrintInvoicePrinter printer = new NoPrintInvoicePrinter(DialogResult.Yes);
			AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.EnterpriseInvoice);
			ARInvoice classAInv = SetupClassAInvoice();
			using (ZForm testForm = new ZForm())
			{
				int previousMessageCount = UnitTestUserNotification.Instance.PreviousMessages.Length;
				printer.PrintTransaction(classAInv, testForm, InvoicePrintContext.DontCare);
				AssertEquals("1 message should have been shown", previousMessageCount + 1, UnitTestUserNotification.Instance.PreviousMessages.Length);
				Assert("The last message should ask user if they wish to print", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				Assert("The last message should ask user if they wish to print AR Invoice", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("Do you want to print invoice"));
			}
		}

		#endregion

		#region Both Govt and Enterprise Invoice

		public void TestPrintClassAInvoiceWithRegistrySetToALL()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			PrintInvoicePrinter printer = new PrintInvoicePrinter(DialogResult.Yes);

			AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice);
			ARInvoice classAInv = SetupClassAInvoice();
			using (ZForm form = new ZForm())
			{
				int previousMessageCount = UnitTestUserNotification.Instance.PreviousMessages.Length;
				printer.PrintTransaction(classAInv, form, InvoicePrintContext.PostFromBilling);
				AssertEquals("1 message should have been shown", previousMessageCount + 1, UnitTestUserNotification.Instance.PreviousMessages.Length);
				AssertEquals("There should be 1 docpack", 1, printer.docPackCount);
				AssertEquals("There should be 1 report in the pack", 1, printer.reportCount);
			}
		}

		#endregion

		#region Govt Invoice

		public void TestPrintClassAInvoiceWithRegistrySetToTAX()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			PrintInvoicePrinter printer = new PrintInvoicePrinter(DialogResult.Yes);

			AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.GovtTaxInvoice);
			ARInvoice classAInv = SetupClassAInvoice();
			using (ZForm form = new ZForm())
			{
				int previousMessageCount = UnitTestUserNotification.Instance.PreviousMessages.Length;
				printer.PrintTransaction(classAInv, form, InvoicePrintContext.DontCare);
				AssertEquals("1 message should have been shown", previousMessageCount + 1, UnitTestUserNotification.Instance.PreviousMessages.Length);
				AssertEquals("There should be 1 docpack", 1, printer.docPackCount);
				AssertEquals("There should be 1 report in the pack", 1, printer.reportCount);
			}
		}

		public void TestPrintClassAInvoiceWithRegistrySetToTAX_Indonesia()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			NoPrintInvoicePrinter printer = new NoPrintInvoicePrinter(DialogResult.Yes);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Indonesia))
			{
				AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.GovtTaxInvoice);

				AccComplianceSequence sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				sequence.XD_SequenceClass = "TXI";
				sequence.XD_StartNumber = 1;
				sequence.XD_EndNumber = 20;
				sequence.XD_NextNumber = 2;
				sequence.XD_Prefix = "010-000-11-";
				sequence.XD_MaximumNumberDigits = 8;
				StmMenuItem menu = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "ARInvoice ID FakturPajak"));
				sequence.XD_SU_MenuItem = menu.PK;
				sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
				sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
				Factory.Save();

				ARInvoice invoice = creator.CreateARInvoice<ARInvoice>("001", creator.AUD, 1.0m, creator.ABIGAS);
				invoice.AH_TransactionReference = "";
				invoice.AH_TransactionType = "INV";
				ARInvoiceLine line = creator.CreateARInvoiceLine(invoice, null, creator.CC1, creator.AUD, 1.0m, "Desc", 100m);
				line.AL_AT = creator.GST1.PK;
				Factory.Save();

				int previousMessageCount = UnitTestUserNotification.Instance.PreviousMessages.Length;
				AssertEquals("PreCondition - AH_TransactionReference is blank", "", invoice.AH_TransactionReference);
				printer.PrintTransaction(invoice, null, InvoicePrintContext.DontCare);
				AssertEquals("PostCondition - AH_TransactionReference is assigend", "010-000-11-00000002", invoice.AH_TransactionReference);
			}
		}

		#endregion

		ARInvoice SetupClassAInvoice()
		{
			ARInvoice classAInv = Factory.NewWithValidTestData<ARInvoice>();
			OrgHeader chinaOrg = Factory.NewWithValidTestData<OrgHeader>();
			chinaOrg.OH_RL_NKClosestPort = "CNSHA";
			classAInv.AH_OH = chinaOrg.PK;
			Factory.Save();
			return classAInv;
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
