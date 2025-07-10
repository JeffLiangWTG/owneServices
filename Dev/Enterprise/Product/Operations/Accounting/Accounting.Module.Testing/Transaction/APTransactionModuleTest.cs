using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.GUI.ARAP.Payment;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;
using BusinessContext = Enterprise.Integration.Accounting.BusinessContext;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APTransactionModuleStrip))]
	public class APTransactionModuleTest : TransactionModuleStripTest
	{
		public void TestReAllocateCheckNumberNoTransactionIsSelectedToReAllocate()
		{
			using (APTransactionModuleStrip testModule = APModule)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testModule.HandleReallocateCheckNumber_ForTestOnly(null, new EventArgs());
				Assert("AP Invoice surpports workflow", testModule.SupportsWorkflow);
				AssertEquals(ChequeNumberReallocator.NoTransactionIsSelectedToReAllocate, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestReprint()
		{
			Business.Base.AccStatement.BankStatement bank = Factory.NewWithValidTestData<Business.Base.AccStatement.BankStatement>();

			AccChequeBook checkbook = Factory.NewWithValidTestData<AccChequeBook>();
			checkbook.AK_AutoPrintCheque = true;
			checkbook.AK_StartNo = 1;
			checkbook.AK_LastNo = 100;
			checkbook.AK_CurrentNo = 3;
			checkbook.AK_AB = bank.PK;
			checkbook.AK_GB = GlbBranch.CurrentBranch.PK;

			APPayment apPayment1 = Factory.NewWithValidTestData<APPayment>();
			apPayment1.AH_AB = bank.PK;
			apPayment1.AH_ReceiptType = ReceiptTypes.Cash;

			APPayment apPayment2 = Factory.NewWithValidTestData<APPayment>();
			apPayment2.AH_AB = bank.PK;
			apPayment2.AH_ReceiptType = ReceiptTypes.Cash;

			Factory.Save();

			using (APTransactionModuleStrip testModule = APModule)
			{
				using (ZForm form = new ZForm())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					testModule.PerformSearch_ForTest();
					AssertEquals(2, testModule.GridCollection.Count);

					testModule.HandleReprint_ForTestOnly(null, null);
					AssertEquals("ExpectedMessage", "Please select transaction(s) to print", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.DisplayGrid.SelectAllElements();
					testModule.HandleReprint_ForTestOnly(null, null);
					ZString expectedMessage = @"The transaction/s you have selected cannot be reprinted. Please select a different print option or change the transaction/s selected for reprinting.
Reprinting is a special print action used to reprint checks recorded against an Auto Print Check Book.  
The reprint option can only be used to reprint a set of Check transactions belonging to one Check Book. Note: Check Payments that have been cleared in the cash book bank reconciliation cannot be reprinted.";
					AssertEquals("ExpectedMessage", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					apPayment1.AH_ReceiptType = ReceiptTypes.Cheque;
					apPayment1.AH_ChequeOrReference = "000001";
					apPayment2.AH_ReceiptType = ReceiptTypes.Cheque;
					apPayment2.AH_ChequeOrReference = "000002";
					Factory.Save();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					testModule.HandleReprint_ForTestOnly(null, null);
					AssertEquals("ExpectedMessage", null, UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("LastMessage", ZFormModaliser.LastFormShownDialogForTest is PaymentDocumentsPrintPopup);
					ZFormModaliser.LastFormShownDialogForTest.Dispose();
					AssertEquals("000003", apPayment1.AH_ChequeOrReference);
					AssertEquals("000004", apPayment2.AH_ChequeOrReference);

					StmALog[] logs = apPayment1.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Check number has been updated during reprinting. Was 000001 Now 000003"));
					AssertEquals(1, logs.Length);
					logs = apPayment2.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Check number has been updated during reprinting. Was 000002 Now 000004"));
					AssertEquals(1, logs.Length);
				}
			}
		}

		public void TestReallocateCheckNumbers()
		{
			Business.Base.AccStatement.BankStatement bank = Factory.NewWithValidTestData<Business.Base.AccStatement.BankStatement>();

			AccChequeBook checkbook = Factory.NewWithValidTestData<AccChequeBook>();
			checkbook.AK_AutoPrintCheque = true;
			checkbook.AK_StartNo = 1;
			checkbook.AK_LastNo = 100;
			checkbook.AK_CurrentNo = 3;
			checkbook.AK_AB = bank.PK;
			checkbook.AK_GB = GlbBranch.CurrentBranch.PK;

			APPayment apPayment1 = Factory.NewWithValidTestData<APPayment>();
			apPayment1.AH_AB = bank.PK;
			apPayment1.AH_ReceiptType = ReceiptTypes.Cash;

			APPayment apPayment2 = Factory.NewWithValidTestData<APPayment>();
			apPayment2.AH_AB = bank.PK;
			apPayment2.AH_ReceiptType = ReceiptTypes.Cash;

			JobConsolCost cost = Factory.NewWithValidTestData<JobConsolCost>();
			cost.E6_PaymentType = ReceiptTypes.Cheque;
			cost.E6_AB_BankAccount = bank.PK;
			cost.E6_AK_ChequeBook = checkbook.PK;
			cost.E6_ChequeOrReference = "000002";

			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_PaymentType = ReceiptTypes.Cheque;
			charge.JR_AB = bank.PK;
			charge.JR_AK = checkbook.PK;
			charge.JR_ChequeNo = "000002";
			charge.JR_E6 = cost.PK;

			Factory.Save();

			using (APTransactionModuleStrip testModule = APModule)
			{
				using (ZForm form = new ZForm())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					testModule.PerformSearch_ForTest();
					AssertEquals(2, testModule.GridCollection.Count);

					testModule.HandleReallocateCheckNumber_ForTestOnly(null, null);
					AssertEquals("ExpectedMessage", "Please select transaction(s) to re-allocate", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.DisplayGrid.SelectAllElements();
					testModule.HandleReallocateCheckNumber_ForTestOnly(null, null);
					ZString expectedMessage = @"The check number/s on transaction/s you have selected cannot be re-allocated. Please change the transaction/s selected for check number re-allocation.
Re-allocation is a special action used to allocate new check numbers recorded against an Auto Print Check Book.  
The re-allocation option can only be used on set of Check transactions belonging to one Check Book. Note: Check Numbers recorded on payments that have been cleared in the cash book bank reconciliation cannot be re-allocated.";
					AssertEquals("ExpectedMessage", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					apPayment1.AH_ReceiptType = ReceiptTypes.Cheque;
					apPayment1.AH_ChequeOrReference = "000001";
					apPayment2.AH_ReceiptType = ReceiptTypes.Cheque;
					apPayment2.AH_ChequeOrReference = "000002";
					Factory.Save();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					testModule.HandleReallocateCheckNumber_ForTestOnly(null, null);
					AssertEquals("ExpectedMessage", null, UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("LastMessage", ZFormModaliser.LastFormShownDialogForTest is ChequeNumberReallocationForm);
					AssertEquals("000003", apPayment1.AH_ChequeOrReference);
					AssertEquals("000004", apPayment2.AH_ChequeOrReference);
					AssertEquals("000004", charge.JR_ChequeNo);
					AssertEquals("000004", cost.E6_ChequeOrReference);

					StmALog[] logs = apPayment1.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Check number has been updated and check was not reprinted. Was 000001 Now 000003"));
					AssertEquals(1, logs.Length);
					logs = apPayment2.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Check number has been updated and check was not reprinted. Was 000002 Now 000004"));
					AssertEquals(1, logs.Length);
					logs = apPayment1.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, ZArchitecture.Business.Events.EditedARecord.Code));
					AssertEquals(2, logs.Length);
					logs = apPayment2.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, ZArchitecture.Business.Events.EditedARecord.Code));
					AssertEquals(2, logs.Length);
				}
			}
		}

		public void TestRelatedTransactionOrganisationRefreshedOnReload()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			InvoicingLineBase invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			invoiceLine.AL_AC = TestObjectCreator.CC1.PK;
			invoiceLine.AL_JH = TestObjectCreator.Job1.PK;
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_JH = TestObjectCreator.Job1.PK;
			charge.JR_AL_APLine = invoiceLine.PK;
			charge.JR_AT_CostGSTRate = invoiceLine.AL_AT;

			ARInvoice relatedInvoice = Factory.NewWithValidTestData<ARInvoice>();
			relatedInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			InvoicingLineBase relatedInvoiceLine = (InvoicingLineBase)relatedInvoice.Lines.AddNew();
			relatedInvoiceLine.AL_AC = TestObjectCreator.CC1.PK;
			relatedInvoiceLine.AL_JH = TestObjectCreator.Job1.PK;
			JobCharge relatedCharge = Factory.NewWithValidTestData<JobCharge>();
			relatedCharge.JR_AC = TestObjectCreator.CC1.PK;
			relatedCharge.JR_JH = TestObjectCreator.Job1.PK;
			relatedCharge.JR_AL_ARLine = relatedInvoiceLine.PK;
			relatedCharge.JR_AT_SellGSTRate = relatedInvoiceLine.AL_AT;

			Factory.Save();

			using (APTransactionModuleStrip testModule = APModule)
			{
				testModule.PerformSearch_ForTest();
				AssertEquals("Should find 1 invoice", 1, testModule.GridCollection.Count);
				AssertEquals("Debtor", "AALSHI:A.A.L. SHIPPING AGENCIES P/L", ((FilteredTransactionHeaderCollectionView)testModule.GridCollection)[0].RelatedTransactionDebtorsAsString);

				ARInvoice relatedInvoice2 = Factory.NewWithValidTestData<ARInvoice>();
				relatedInvoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
				InvoicingLineBase relatedInvoice2Line = (InvoicingLineBase)relatedInvoice2.Lines.AddNew();
				relatedInvoice2Line.AL_AC = TestObjectCreator.CC1.PK;
				relatedInvoice2Line.AL_JH = TestObjectCreator.Job1.PK;
				JobCharge relatedCharge2 = Factory.NewWithValidTestData<JobCharge>();
				relatedCharge2.JR_AC = TestObjectCreator.CC1.PK;
				relatedCharge2.JR_JH = TestObjectCreator.Job1.PK;
				relatedCharge2.JR_AL_ARLine = relatedInvoice2Line.PK;
				Factory.Save();

				testModule.PerformSearch_ForTest();
				AssertEquals("Should find 1 invoice", 1, testModule.GridCollection.Count);
				var relatedTransactionDebtorsAsString = ((FilteredTransactionHeaderCollectionView)testModule.GridCollection)[0].RelatedTransactionDebtorsAsString;
				CombineAssertions("Debtor should be updated", () =>
				{
					AssertContains("AALSHI:A.A.L. SHIPPING AGENCIES P/L", relatedTransactionDebtorsAsString);
					AssertContains("ABIGAS:ABI GAS & TOOLS", relatedTransactionDebtorsAsString);
					AssertContains(",", relatedTransactionDebtorsAsString);
				});
			}
		}

		public virtual void TestSecurityCheckpoint()
		{
			AssertEquals("Should have correct Security CheckPoint", Env.Security.PayablesTransactions, APModule.SecurityCheckpoint);
		}

		public override void TestImportRemittanceFileSecurityCheckpoint()
		{
			AssertEquals("Should have correct Import Remittance File Security Checkpoint", Env.Security.ImportRemittanceFilePayables, APModule.ImportRemittanceFileSecurityCheckpoint_ForTestOnly);
		}

		public void TestPrintSecurity()
		{
			bool oldValue = Env.Security.PrintPayableTransactions.IsAllowed;

			try
			{
				Env.Security.PrintPayableTransactions.IsAllowed = false;
				using (APTransactionModuleStrip testModule = APModule)
				{
					using (ZForm form = new ZForm())
					{
						APReceipt testBizO = Factory.NewWithValidTestData<APReceipt>();
						testBizO.AH_OH = TestObjectCreator.AALSHI.PK;
						Factory.Save();

						ZStringBuilder expectedMessage = new ZStringBuilder("The following transaction(s) cannot be printed\r\n");
						expectedMessage.AppendLine(string.Format("Transaction {0} cannot be printed because {1}\r\n", testBizO.AH_TransactionNum, Env.Security.PrintPayableTransactions.ErrorMessageForNotAllowed));

						form.Controls.Add(testModule.EmbeddedControl);
						form.Show();
						testModule.PerformSearch_ForTest();

						BusinessObjectCollection testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
						testCollection.Load();
						AssertEquals(1, testCollection.Count);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						testModule.HandlePrint_ForTestOnly(null, new EventArgs());
						AssertEquals(expectedMessage.ToString(), UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
			finally
			{
				Env.Security.PrintPayableTransactions.IsAllowed = oldValue;
			}
		}

		public void TestHandlePrintCannotPrintTransactionsAPAdjustmentNote() =>
			AssertHandlePrintCannotPrintTransactions<APAdjustmentNote>(null);

		public void TestHandlePrintCannotPrintTransactionsAPContra() =>
			AssertHandlePrintCannotPrintTransactions<APContraRow>("Contra");

		public void TestHandlePrintCannotPrintTransactionsAPCreditNote() =>
			AssertHandlePrintCannotPrintTransactions<APCreditNote>(null);

		public void TestHandlePrintCannotPrintTransactionsAPDiscount() =>
			AssertHandlePrintCannotPrintTransactions<APDiscount>("Discount");

		public void TestHandlePrintCannotPrintTransactionsAPExchangeDifference() =>
			AssertHandlePrintCannotPrintTransactions<APExchangeDifference>("Exchange Difference");

		public void TestHandlePrintCannotPrintTransactionsAPInvoice() =>
			AssertHandlePrintCannotPrintTransactions<APInvoice>(null);

		public void TestHandlePrintCannotPrintTransactionsAPJournal() =>
			AssertHandlePrintCannotPrintTransactions<APJournal>("Journal");

		public void TestHandlePrintCannotPrintTransactionsAPOverpayment() =>
			AssertHandlePrintCannotPrintTransactions<APOverpayment>("Overpayment");

		public void TestHandlePrintCannotPrintTransactionsAPTransfer() =>
			AssertHandlePrintCannotPrintTransactions<APTransferFromRow>("Transfer");

		void AssertHandlePrintCannotPrintTransactions<T>(string message) where T : AccTransactionHeader
		{
			var orgHeaderPK = TestObjectCreator.CreateOrgHeader("TSTREGORG", true, true).PK;

			using (APTransactionModuleStrip module = (APTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			{
				using (ZForm form = new ZForm())
				{
					var testHeader = Factory.NewWithValidTestData<T>();
					testHeader.AH_OH = orgHeaderPK;
					testHeader.AH_AG = TestObjectCreator.GLHeader1.PK;
					Factory.Save();

					form.Controls.Add(module.EmbeddedControl);
					form.Show();
					module.PerformSearch_ForTest();

					var testCollection = (BusinessObjectCollection)module.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(1, testCollection.Count);

					if (module.CurrentBusinessObjectInGrid_ForTestOnly != null)
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						module.HandlePrint_ForTestOnly(this, new EventArgs());
						if (message == null)
						{
							AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
						}
						else
						{
							AssertEquals(GetExpectedPrintErrorMessage(testHeader.AH_TransactionNum, message, false), UnitTestUserNotification.Instance.LastMessage.Text);
						}
					}
				}
			}
		}

		public override void TestPrintingToolBarButtons()
		{
			ToolBarButton[] buttons = APModule.ToolBarButtons;
			ToolBarButton printButton = buttons.FindByText("Print");
			AssertNotNull("There should be a Button named Print", printButton);
			AssertNotNull("The Print button should have a dropdown menu", printButton.DropDownMenu);

			MenuItem[] menuItemArray = new MenuItem[8];
			printButton.DropDownMenu.MenuItems.CopyTo(menuItemArray, 0);
			List<MenuItem> zMenuItems = new List<MenuItem>();
			zMenuItems.Add(menuItemArray[0]);
			zMenuItems.Add(menuItemArray[1]);
			zMenuItems.Add(menuItemArray[2]);
			zMenuItems.Add(menuItemArray[3]);
			zMenuItems.Add(menuItemArray[4]);
			zMenuItems.Add(menuItemArray[5]);
			zMenuItems.Add(menuItemArray[6]);
			zMenuItems.Add(menuItemArray[7]);
			AssertNotNull("There should be a suboption called 'PrintTransaction'", zMenuItems.FindByText(APModule.PrintTransactionMenuText_ForTestOnly));
			AssertNotNull("There should be a suboption called 'PrintMatchDoc'", zMenuItems.FindByText(APModule.PrintMatchDocMenuText_ForTestOnly));
			AssertNotNull("There should be a suboption called 'PrintSelfBillingInvoice'", zMenuItems.FindByText(APModule.PrintSelfBillingInvoiceMenuText_ForTestOnly));
			AssertNotNull("There should be a suboption called 'MarkAsNotPrinted'", zMenuItems.FindByText(APModule.MarkAsNotPrintedMenuText_ForTestOnly));
			AssertNotNull("There should be a suboption called 'Print Accounting Journal'", zMenuItems.FindByText(AccountingJournalPrintHelper.PrintAccountingJournalText));
		}

		public void TestPrintingToolBarButtonForITAutofattura()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				ToolBarButton[] buttons = APModule.ToolBarButtons;
				ToolBarButton printButton = buttons.FindByText("Print");
				MenuItem[] menuItemArray = new MenuItem[9];
				printButton.DropDownMenu.MenuItems.CopyTo(menuItemArray, 0);
				List<MenuItem> zMenuItems = new List<MenuItem>();
				zMenuItems.Add(menuItemArray[0]);
				zMenuItems.Add(menuItemArray[1]);
				zMenuItems.Add(menuItemArray[2]);
				zMenuItems.Add(menuItemArray[3]);
				zMenuItems.Add(menuItemArray[4]);
				zMenuItems.Add(menuItemArray[5]);
				zMenuItems.Add(menuItemArray[6]);
				zMenuItems.Add(menuItemArray[7]);
				zMenuItems.Add(menuItemArray[8]);
				AssertNotNull("There should be a suboption called 'Print Autofattura (IT)'", zMenuItems.FindByText(APModule.GetPrintAutofatturaInvoiceMenuText));
			}
		}

		public void TestPayInvoicesActionMenuItem()
		{
			MenuItem[] actionMenu = APModule.GetNewActionMenuItems_ForTestOnly();
			MenuItem payInvoicesMenuItem = actionMenu.FindByText("Pay Invoices");
			AssertNotNull("Menu item should exist", payInvoicesMenuItem);
		}

		public void TestBulkAPInvoicePostingActionMenuItem()
		{
			var module = APModule;
			MenuItem[] actionMenu = module.GetNewActionMenuItems_ForTestOnly();
			MenuItem bulkAPInvoicePostingMenuItem = actionMenu.FindByText("Bulk AP Invoice Posting");
			AssertNotNull("Menu item should exist", bulkAPInvoicePostingMenuItem);
			bulkAPInvoicePostingMenuItem.PerformClick();
			using (var form = (ZForm)module.LastShownBulkAPInvoicePostingForm_ForTestOnly)
			{
				AssertNotNull("Bulk AP Invoice posting form should be shown", form);
				AssertEquals("Bulk AP Invoice has context", true, form.BusinessEntity.Factory.HasContext(BusinessContext.APBulkInvoicePoster));
			}
		}

		public void TestFilterSelectedTransactions_NothingSelected()
		{
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			BusinessObject[] emptyCollection = Array.Empty<BusinessObject>();
			APModule.FilterSelectedTransactions_ForTestOnly(emptyCollection);
			AssertEquals("Message should be shown", APTransactionModuleStrip.NothingSelectedMessage_ForTestOnly, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestFilterSelectedTransactions_NullInput()
		{
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			APModule.FilterSelectedTransactions_ForTestOnly(null);
			AssertEquals("Message should be shown", APTransactionModuleStrip.NothingSelectedMessage_ForTestOnly, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestFilterSelectedTransactions_NothingToPostAmongSelectedTransactions()
		{
			APInvoice testAPInv = Factory.NewWithValidTestData<APInvoice>();
			testAPInv.AH_FullyPaidDate = ZDateTime.Today;
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			BusinessObject[] transactionsCollection = new BusinessObject[1];
			transactionsCollection[0] = testAPInv;

			TransactionHeaderCollection filteredTransactions = APModule.FilterSelectedTransactions_ForTestOnly(transactionsCollection);
			AssertEquals("The collection should be empty", 0, filteredTransactions.Count);
			AssertEquals("Message should be shown", APTransactionModuleStrip.NothingToPostAmongSelectedTransactionsMessage_ForTestOnly, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestFilterSelectedTransactions_SelectedGroupsWithoutTransactions()
		{
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			OrgHeader testOrg = ModuleFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader testOrg2 = ModuleFactory.NewWithValidTestData<OrgHeader>();
			testOrg2.APSettlementGroupPK = testOrg.PK;

			APInvoice testAPInv = ModuleFactory.NewWithValidTestData<APInvoice>();
			testAPInv.AH_OH = testOrg2.PK;
			TestObjectCreator.CreateInvoiceLine(testAPInv, testAPInv.TransactionCurrency, testAPInv.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m);

			APInvoice testAPInv2 = ModuleFactory.NewWithValidTestData<APInvoice>();
			testAPInv2.AH_OH = testOrg.PK;
			TestObjectCreator.CreateInvoiceLine(testAPInv2, testAPInv2.TransactionCurrency, testAPInv2.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m);

			ModuleFactory.Save();

			BusinessObject[] transactionsCollection = new BusinessObject[1];
			transactionsCollection[0] = testAPInv;
			TransactionHeaderCollection filteredTransactions = APModule.FilterSelectedTransactions_ForTestOnly(transactionsCollection);
			AssertEquals("The collection should not be empty", 1, filteredTransactions.Count);

			transactionsCollection = new BusinessObject[2];
			transactionsCollection[0] = testAPInv;
			transactionsCollection[1] = testAPInv2;

			filteredTransactions = APModule.FilterSelectedTransactions_ForTestOnly(transactionsCollection);
			AssertEquals("The collection should contain both transactions", 2, filteredTransactions.Count);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestFilterSelectedTransactions_SettlementGroupWithMultiReference()
		{
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			OrgHeader testOrg = ModuleFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader testOrg2 = ModuleFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader testOrg3 = ModuleFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader testOrg4 = ModuleFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader testOrg5 = ModuleFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader testOrg6 = ModuleFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader testOrg7 = ModuleFactory.NewWithValidTestData<OrgHeader>();
			testOrg2.APSettlementGroupPK = testOrg.PK;
			testOrg.APSettlementGroupPK = testOrg3.PK;
			testOrg5.APSettlementGroupPK = testOrg6.PK;
			testOrg6.APSettlementGroupPK = testOrg7.PK;

			APInvoice testAPInv = ModuleFactory.NewWithValidTestData<APInvoice>();
			testAPInv.AH_OH = testOrg2.PK;
			TestObjectCreator.CreateInvoiceLine(testAPInv, testAPInv.TransactionCurrency, testAPInv.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m);

			APInvoice testAPInv2 = ModuleFactory.NewWithValidTestData<APInvoice>();
			testAPInv2.AH_OH = testOrg.PK;
			TestObjectCreator.CreateInvoiceLine(testAPInv2, testAPInv2.TransactionCurrency, testAPInv2.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m);

			APInvoice testAPInv3 = ModuleFactory.NewWithValidTestData<APInvoice>();
			testAPInv3.AH_OH = testOrg3.PK;
			TestObjectCreator.CreateInvoiceLine(testAPInv3, testAPInv3.TransactionCurrency, testAPInv3.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m);

			APInvoice testAPInv4 = ModuleFactory.NewWithValidTestData<APInvoice>();
			testAPInv4.AH_OH = testOrg4.PK;
			TestObjectCreator.CreateInvoiceLine(testAPInv4, testAPInv4.TransactionCurrency, testAPInv4.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m);

			APInvoice testAPInv5 = ModuleFactory.NewWithValidTestData<APInvoice>();
			testAPInv5.AH_OH = testOrg5.PK;
			TestObjectCreator.CreateInvoiceLine(testAPInv5, testAPInv5.TransactionCurrency, testAPInv5.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m);

			APInvoice testAPInv6 = ModuleFactory.NewWithValidTestData<APInvoice>();
			testAPInv6.AH_OH = testOrg6.PK;
			TestObjectCreator.CreateInvoiceLine(testAPInv6, testAPInv6.TransactionCurrency, testAPInv6.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m);

			APInvoice testAPInv7 = ModuleFactory.NewWithValidTestData<APInvoice>();
			testAPInv7.AH_OH = testOrg7.PK;
			TestObjectCreator.CreateInvoiceLine(testAPInv7, testAPInv7.TransactionCurrency, testAPInv7.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m);

			ModuleFactory.Save();

			BusinessObject[] transactionsCollection = new BusinessObject[7];
			transactionsCollection[0] = testAPInv;
			transactionsCollection[1] = testAPInv2;
			transactionsCollection[2] = testAPInv3;
			transactionsCollection[3] = testAPInv4;
			transactionsCollection[4] = testAPInv5;
			transactionsCollection[5] = testAPInv6;
			transactionsCollection[6] = testAPInv7;

			TransactionHeaderCollection filteredTransactions = APModule.FilterSelectedTransactions_ForTestOnly(transactionsCollection);
			AssertEquals("The collection should contain 3 transactions", 3, filteredTransactions.Count);
			Assert("The collection should contain TestAPInv3", filteredTransactions.Contains(testAPInv3));
			Assert("The collection should contain TestAPInv4", filteredTransactions.Contains(testAPInv4));
			Assert("The collection should contain TestAPInv7", filteredTransactions.Contains(testAPInv7));
			ZString expectedMessage = APTransactionModuleStrip.SettlementGroupWithMultiReferenceMessageHeader + System.Environment.NewLine + testOrg.OH_Code + System.Environment.NewLine + testOrg6.OH_Code + System.Environment.NewLine + APTransactionModuleStrip.SettlementGroupWithMultiReferenceMessageEnd;
			AssertEquals("Message should be shown", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestFilterSelectedTransactions()
		{
			SetupDataForPayInvoicesTest();
			TransactionHeaderCollection filteredTransactions = APModule.FilterSelectedTransactions_ForTestOnly(TransactionsCollection);
			AssertEquals("Collection should contain 6 elements", 6, filteredTransactions.Count);
			Assert("Collection should contain TestAPInv", filteredTransactions.Contains(TestAPInv));
			Assert("Collection should contain TestAPInv2", filteredTransactions.Contains(TestAPInv2));
			Assert("Collection should contain TestAPInv3", filteredTransactions.Contains(TestAPInv3));
			Assert("Collection should contain TestAPRec", filteredTransactions.Contains(TestAPRec));
			Assert("Collection should contain TestAPRec2", filteredTransactions.Contains(TestAPRec2));
			Assert("Collection should contain TestAPInv6", filteredTransactions.Contains(TestAPInv6));
		}

		#region Notional WHT Tests

		public void TestHandlePayInvoices_WithNotionalWHT()
		{
			using (var testModule = APModule)
			using (ZForm form = new ZForm())
			{
				APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
				invoice.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
				invoice.AH_FullyPaidDate = ZDateTime.Empty;
				invoice.AH_TransactionType = "INV";
				invoice.AH_TransactionNum = "00000001";
				TestObjectCreator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, invoice.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m);
				Factory.Save();

				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();
				BusinessObjectCollection testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
				UnitTestUserNotification.Instance.ClearMessages();

				testModule.PerformSearch_ForTest();
				testCollection.Load();
				Assert("Precondition: at least one transaction should be available", testCollection.Any());
				testModule.DisplayGrid.SelectAllElements();
				GetPayInvoicesMenuItem(testModule).PerformClick();
				AssertEquals("No error messages should be triggered", null, UnitTestUserNotification.Instance.LastMessage.Text);

				APInvoice invoice2 = Factory.NewWithValidTestData<APInvoice>();
				invoice2.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
				invoice2.AH_FullyPaidDate = ZDateTime.Empty;
				invoice2.AH_TransactionType = "INV";
				invoice2.AH_TransactionNum = "00000002";
				TestObjectCreator.CreateInvoiceLine(invoice2, invoice2.TransactionCurrency, invoice2.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m);
				Factory.Save();
				var loaderMock = new Mock<IWHTAmountLoader>();
				loaderMock.Setup(x => x.GetNotionalWHT(It.IsAny<ZGuid>())).Returns(20m);
				testModule.WHTAmountLoaderSubstituter_ForTestOnly = (factory) => TaxFrameworkObjectFactory.SubstituteWHTAmountLoader_ForTestOnly(factory, loaderMock.Object);

				UnitTestUserNotification.Instance.ClearMessages();
				testModule.PerformSearch_ForTest();
				testCollection.Load();
				Assert("Precondition: at least one transaction should be available", testCollection.Any());
				testModule.DisplayGrid.SelectAllElements();
				GetPayInvoicesMenuItem(testModule).PerformClick();

				using (testModule.LastShownAPPaymentBatchPostingForm_ForTestOnly)
				{
					var expectedErrorMessage = @"AP Invoices with a Notional WHT cannot be included in a payment batch.
• INV 00000001, INV 00000002";
					AssertMultilineASCIIEquals("Error messages show when invoice with Notional WHT is selected", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestAvailabilityOfRealizeWHTActionMenu()
		{
			AssertMenuItem(false);

			var taxTestHelper = new AccountingTestObjectCreator(new BusinessObjectFactory());
			taxTestHelper.ConfigureTaxFrameworkAtCompanyLevel(GlbCompany.CurrentCompany, LedgerTypes.AccountsPayable, TaxSuperTypeList.StandardPaymentRetention.Code);
			AssertMenuItem(true);

			void AssertMenuItem(bool isMenuItemAdded)
			{
				using (var testModule = (ZFilterGridModule)GetModule())
				using (var form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					AssertEquals(isMenuItemAdded, GetWHTRealizationMenuItem(testModule) != null);
				}
			}
		}

		public void TestWHTJournalDependenciesSharesCreationManagerInstance()
		{
			AssertType("Precondition: WithholdingJournalCreatorForMultipleInvoices", typeof(WithholdingJournalCreatorForMultipleInvoices), APModule.WithholdingJournalCreatorForMultipleInvoices);
			AssertType("Precondition: WithholdingJournalRealizerForMultipleInvoices", typeof(WithholdingJournalRealizerForMultipleInvoices), APModule.WithholdingJournalRealizerForMultipleInvoices);
			var withholdingJournalCreator = ((WithholdingJournalCreatorForMultipleInvoices)APModule.WithholdingJournalCreatorForMultipleInvoices).WithholdingJournalCreator;
			AssertType("Precondition: WithholdingJournalCreator", typeof(WithholdingJournalCreatorForSingleInvoice), withholdingJournalCreator);
			var withholdingJournalRealizer = ((WithholdingJournalRealizerForMultipleInvoices)APModule.WithholdingJournalRealizerForMultipleInvoices).WithholdingJournalRealizer;
			AssertType("Precondition: WithholdingJournalRealizer", typeof(WithholdingJournalRealizerForSingleInvoice), withholdingJournalRealizer);

			AssertEquals("WithholdingJournalCreationManager must be the same instance as it has state that is shared between creator and realizer",
				((WithholdingJournalCreatorForSingleInvoice)withholdingJournalCreator).WithholdingJournalCreationManager,
				((WithholdingJournalRealizerForSingleInvoice)withholdingJournalRealizer).WithholdingJournalCreationManager);
		}

		public void TestIWHTJournalCreationActionMenu_NoTransactionSelected_ShowsErrorMessageAndNoJournalForm()
		{
			ActivateWHTRealizationMenuItem();

			using (var testModule = APModule)
			using (var form = new ZForm())
			{
				form.Controls.Add(testModule.EmbeddedControl);

				GetWHTRealizationMenuItem(testModule).PerformClick();
				AssertEquals("ExpectedMessage", "Please select Invoices first", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestIWHTJournalCreationActionMenu_PassesJournalCollectionReturnedCreatorToJournalForm()
		{
			ActivateWHTRealizationMenuItem();

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			TestObjectCreator.CreateInvoiceLine(apInvoice, 10);
			Factory.Save();

			var createWHTJournalsMock = new Mock<IWithholdingJournalCreatorForMultipleInvoices>();
			var realizeWHTJournalsMock = new Mock<IWithholdingJournalRealizerForMultipleInvoices>();

			var withholdingJournalsPerInvoiceCollection = new[]
			{
				new WithholdingJournalsPerInvoice(apInvoice, Array.Empty<WithholdingJournalForDisplay>())
			};

			createWHTJournalsMock.Setup(x => x.Create(It.IsAny<IReadOnlyCollection<InvoicingBase>>())).Returns((withholdingJournalsPerInvoiceCollection, string.Empty));

			ClickOnWHTRealizationMenuItem(createWHTJournalsMock, realizeWHTJournalsMock);

			var lastFormShown = ZFormModaliser.LastFormShownDialogForTest;
			AssertType<WithholdingJournalParentPivotForm>("Precondition: lastFormShown", lastFormShown);
			var parentPivotForm = (WithholdingJournalParentPivotForm)lastFormShown;

			AssertEquals(withholdingJournalsPerInvoiceCollection, parentPivotForm.JournalCollectionPerInvoice);
		}

		public void TestIWHTJournalCreationActionMenu_PassesJournalCollectionReturnedCreatorToJournalForm_WhenCreatorAlsoReturnErrors()
		{
			ActivateWHTRealizationMenuItem();

			var apInvoice1 = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			TestObjectCreator.CreateInvoiceLine(apInvoice1, 10);
			var apInvoice2 = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			TestObjectCreator.CreateInvoiceLine(apInvoice2, 10);
			Factory.Save();

			var createWHTJournalsMock = new Mock<IWithholdingJournalCreatorForMultipleInvoices>();
			var realizeWHTJournalsMock = new Mock<IWithholdingJournalRealizerForMultipleInvoices>();

			var withholdingJournalsPerInvoiceCollection = new[]
{
				new WithholdingJournalsPerInvoice(apInvoice2, Array.Empty<WithholdingJournalForDisplay>())
			};

			createWHTJournalsMock.Setup(x => x.Create(It.IsAny<IReadOnlyCollection<InvoicingBase>>())).Returns((withholdingJournalsPerInvoiceCollection, "Some error"));

			ClickOnWHTRealizationMenuItem(createWHTJournalsMock, realizeWHTJournalsMock);

			AssertNotNullOrEmpty("Postcondition: LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);
			var lastFormShown = ZFormModaliser.LastFormShownDialogForTest;
			AssertType<WithholdingJournalParentPivotForm>("Precondition: lastFormShown", lastFormShown);
			var parentPivotForm = (WithholdingJournalParentPivotForm)lastFormShown;
			AssertEquals(withholdingJournalsPerInvoiceCollection, parentPivotForm.JournalCollectionPerInvoice);
		}

		public void TestIWHTJournalCreationActionMenu_PassesRealizerFromModuleToJournalForm()
		{
			ActivateWHTRealizationMenuItem();

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			TestObjectCreator.CreateInvoiceLine(apInvoice, 10);
			Factory.Save();

			var createWHTJournalsMock = new Mock<IWithholdingJournalCreatorForMultipleInvoices>();
			var realizeWHTJournalsMock = new Mock<IWithholdingJournalRealizerForMultipleInvoices>();

			var withholdingJournalsPerInvoiceCollection = new[]
			{
				new WithholdingJournalsPerInvoice(apInvoice, Array.Empty<WithholdingJournalForDisplay>())
			};

			createWHTJournalsMock.Setup(x => x.Create(It.IsAny<IReadOnlyCollection<InvoicingBase>>())).Returns((withholdingJournalsPerInvoiceCollection, string.Empty));

			ClickOnWHTRealizationMenuItem(createWHTJournalsMock, realizeWHTJournalsMock);

			var lastFormShown = ZFormModaliser.LastFormShownDialogForTest;
			AssertType<WithholdingJournalParentPivotForm>("Postcondition: lastFormShown", lastFormShown);
			var parentPivotForm = (WithholdingJournalParentPivotForm)lastFormShown;

			AssertEquals(realizeWHTJournalsMock.Object, parentPivotForm.WithholdingJournalRealizerForMultipleInvoices);
		}

		[ExpectNoExceptions]
		public void TestIWHTJournalCreationActionMenu_PassesCorrectDataToCreateWithholdingJournals()
		{
			ActivateWHTRealizationMenuItem();

			var apInvoice1 = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			TestObjectCreator.CreateInvoiceLine(apInvoice1, 10);
			var apInvoice2 = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			TestObjectCreator.CreateInvoiceLine(apInvoice2, 10);
			Factory.Save();

			var createWHTJournalsMock = new Mock<IWithholdingJournalCreatorForMultipleInvoices>();
			var realizeWHTJournalsMock = new Mock<IWithholdingJournalRealizerForMultipleInvoices>();

			createWHTJournalsMock.Setup(x => x.Create(It.IsAny<IReadOnlyCollection<InvoicingBase>>())).Returns((Array.Empty<WithholdingJournalsPerInvoice>(), "error"));

			using (var testModule = GetNewModuleObject(createWHTJournalsMock.Object, realizeWHTJournalsMock.Object))
			{
				using (var form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();
					testModule.PerformSearch_ForTest();
					testModule.DisplayGrid.SelectAllElements();
					GetWHTRealizationMenuItem(testModule).PerformClick();

					var moduleFactory = testModule.GridCollection.Factory;
					createWHTJournalsMock.Verify(x => x.Create(It.Is<IReadOnlyCollection<InvoicingBase>>(invoices => new[] { apInvoice1.PK, apInvoice2.PK }.ContainsSameElementsInAnyOrder(invoices.Select(x => x.PK)))), "All selected invoices are passed.");
					createWHTJournalsMock.Verify(x => x.Create(It.Is<IReadOnlyCollection<InvoicingBase>>(invoices => invoices.All(x => x.Factory != moduleFactory))), "all invoices are in a factroy different to a module factory.");
				}
			}
		}

		public void TestIWHTJournalCreationActionMenu_SelectedBusinessObjectsNotInvoicingBase_ShowsErrorMessageAndNoJournalForm()
		{
			ActivateWHTRealizationMenuItem();

			Factory.NewWithValidTestData<APReceipt>();
			Factory.Save();

			var createWHTJournalsMock = new Mock<IWithholdingJournalCreatorForMultipleInvoices>(MockBehavior.Strict);
			var realizeWHTJournalsMock = new Mock<IWithholdingJournalRealizerForMultipleInvoices>(MockBehavior.Strict);

			ClickOnWHTRealizationMenuItem(createWHTJournalsMock, realizeWHTJournalsMock);

			var expectedErrorMessage = "Please select Invoices first";
			AssertEquals("LastMessage", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownDialogForTest);
		}

		[ExpectNoExceptions]
		public void TestIWHTJournalCreationActionMenu_SelectedBusinessObjectsContainInvoicingBase_ProceedsToCreation()
		{
			ActivateWHTRealizationMenuItem();

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			TestObjectCreator.CreateInvoiceLine(apInvoice, 10);
			Factory.NewWithValidTestData<APReceipt>();
			Factory.Save();

			var createWHTJournalsMock = new Mock<IWithholdingJournalCreatorForMultipleInvoices>(MockBehavior.Strict);
			var realizeWHTJournalsMock = new Mock<IWithholdingJournalRealizerForMultipleInvoices>(MockBehavior.Strict);

			createWHTJournalsMock.Setup(x => x.Create(It.IsAny<IReadOnlyCollection<InvoicingBase>>())).Returns((Array.Empty<WithholdingJournalsPerInvoice>(), "error"));

			ClickOnWHTRealizationMenuItem(createWHTJournalsMock, realizeWHTJournalsMock);

			createWHTJournalsMock.Verify(x => x.Create(It.IsAny<IReadOnlyCollection<InvoicingBase>>()));
		}

		public void TestIWHTJournalCreationActionMenu_CreateReturnsNoError_ShowsJournalFormAndNoError()
		{
			ActivateWHTRealizationMenuItem();

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			TestObjectCreator.CreateInvoiceLine(apInvoice, 10);
			Factory.Save();

			var createWHTJournalsMock = new Mock<IWithholdingJournalCreatorForMultipleInvoices>();
			var realizeWHTJournalsMock = new Mock<IWithholdingJournalRealizerForMultipleInvoices>();

			var withholdingJournalsPerInvoiceCollection = new[]
			{
				new WithholdingJournalsPerInvoice(apInvoice, Array.Empty<WithholdingJournalForDisplay>())
			};

			createWHTJournalsMock.Setup(x => x.Create(It.IsAny<IReadOnlyCollection<InvoicingBase>>())).Returns((withholdingJournalsPerInvoiceCollection, string.Empty));

			ClickOnWHTRealizationMenuItem(createWHTJournalsMock, realizeWHTJournalsMock);

			AssertType<WithholdingJournalParentPivotForm>("LastFormShownDialogForTest", ZFormModaliser.LastFormShownDialogForTest);
			AssertNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestIWHTJournalCreationActionMenu_CreateReturnsErrorMessage_ShowsErrorMessageAndNoJournalForm()
		{
			ActivateWHTRealizationMenuItem();

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			TestObjectCreator.CreateInvoiceLine(apInvoice, 10);
			Factory.Save();

			var createWHTJournalsMock = new Mock<IWithholdingJournalCreatorForMultipleInvoices>();
			var realizeWHTJournalsMock = new Mock<IWithholdingJournalRealizerForMultipleInvoices>();

			var expectedErrorMessage = "Error: No valid invoices found for withholding journal creation.";
			createWHTJournalsMock.Setup(x => x.Create(It.IsAny<IReadOnlyCollection<InvoicingBase>>())).Returns((Array.Empty<WithholdingJournalsPerInvoice>(), expectedErrorMessage));

			ClickOnWHTRealizationMenuItem(createWHTJournalsMock, realizeWHTJournalsMock);

			AssertEquals("LastMessage", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownDialogForTest);
		}

		public void TestIWHTJournalCreationActionMenu_MultipleInvoices_CreateReturnsErrorMessageAndJournals_ShowsErrorMessageAndJournalForm()
		{
			ActivateWHTRealizationMenuItem();

			var apInvoice1 = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			TestObjectCreator.CreateInvoiceLine(apInvoice1, 10);
			var apInvoice2 = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			TestObjectCreator.CreateInvoiceLine(apInvoice2, 10);
			Factory.Save();

			var createWHTJournalsMock = new Mock<IWithholdingJournalCreatorForMultipleInvoices>();
			var realizeWHTJournalsMock = new Mock<IWithholdingJournalRealizerForMultipleInvoices>();

			var withholdingJournalsPerInvoiceCollection = new[]
{
				new WithholdingJournalsPerInvoice(apInvoice2, Array.Empty<WithholdingJournalForDisplay>())
			};

			createWHTJournalsMock.Setup(x => x.Create(It.IsAny<IReadOnlyCollection<InvoicingBase>>())).Returns((withholdingJournalsPerInvoiceCollection, "INV2: Some error"));

			ClickOnWHTRealizationMenuItem(createWHTJournalsMock, realizeWHTJournalsMock);

			var expectedErrorMessage =
@"INV2: Some error

Successfully generated journals will be shown after this message is closed.";
			AssertMultilineASCIIEquals("LastMessage", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertType<WithholdingJournalParentPivotForm>("LastFormShownDialogForTest", ZFormModaliser.LastFormShownDialogForTest);
		}

		static MenuItem GetWHTRealizationMenuItem(ZFilterGridModule module) => module.ActionsMenuItem.MenuItems.FindByText("Realize Payments Basis Withholding Tax");
		static MenuItem GetPayInvoicesMenuItem(ZFilterGridModule module) => module.ActionsMenuItem.MenuItems.FindByText("Pay Invoices");

		static void ActivateWHTRealizationMenuItem()
		{
			var taxTestHelper = new AccountingTestObjectCreator(new BusinessObjectFactory());
			taxTestHelper.ConfigureTaxFrameworkAtCompanyLevel(GlbCompany.CurrentCompany, LedgerTypes.AccountsPayable, TaxSuperTypeList.StandardPaymentRetention.Code);
		}

		void ClickOnWHTRealizationMenuItem(Mock<IWithholdingJournalCreatorForMultipleInvoices> createWHTJournalsMock, Mock<IWithholdingJournalRealizerForMultipleInvoices> realizeWHTJournalsMock)
		{
			using (var testModule = GetNewModuleObject(createWHTJournalsMock.Object, realizeWHTJournalsMock.Object))
			{
				using (var form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();
					testModule.PerformSearch_ForTest();
					testModule.DisplayGrid.SelectAllElements();
					GetWHTRealizationMenuItem(testModule).PerformClick();
				}
			}
		}

		#endregion

		public void TestNZCustomsAPTransactionMenuItem()
		{
			RefCountry country1 = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia);
			RefCountry country2 = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.NewZealand);

			AssertNotNull("Country1 should exist", country1);
			AssertNotNull("Country2 should exist", country2);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country1.Code;

			using (APTransactionModuleStrip apTransactionModule = (APTransactionModuleStrip)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				MenuItem[] actionMenu = apTransactionModule.FormActionMenu;
				MenuItem dataTransferItem = actionMenu.FindByText("Actions").MenuItems.FindByText("D&ata Transfer");
				AssertNotNull("Data Transfer menu item should exist", dataTransferItem);
				MenuItem nZCustomsAPTransactionMenuItem = dataTransferItem.MenuItems.FindByText("Import NZ Customs CSV Transaction");
				AssertNull("Menu item should not exist", nZCustomsAPTransactionMenuItem);
			}

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country2.Code;

			using (APTransactionModuleStrip apTransactionModule = (APTransactionModuleStrip)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				MenuItem[] actionMenu = apTransactionModule.FormActionMenu;
				MenuItem dataTransferItem = actionMenu.FindByText("Actions").MenuItems.FindByText("D&ata Transfer");
				AssertNotNull("Data Transfer menu item should exist", dataTransferItem);
				MenuItem nZCustomsAPTransactionMenuItem = dataTransferItem.MenuItems.FindByText("Import NZ Customs CSV Transaction");
				AssertNotNull("NZ Customs AP Transaction menu item should exist", nZCustomsAPTransactionMenuItem);
			}
		}

		[ExpectNoExceptions]
		public void TestPrintSelfBilledInvoiceRunsWithoutExceptionWhenDocBuilderEnabled()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			using (APTransactionModuleStrip testModule = APModule)
			{
				using (ZForm form = new ZForm())
				{
					APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
					invoice.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
					Factory.Save();

					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();
					testModule.PerformSearch_ForTest();

					BusinessObjectCollection testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(1, testCollection.Count);

					testModule.HandlePrintSelfBillingInvoice_ForTestOnly(null, EventArgs.Empty);
				}
			}
		}

		public void TestHandlePrintSelfBillingInvoiceOnlyHandlesSBCTransactions()
		{
			using (APTransactionModuleStrip testModule = APModule)
			{
				using (ZForm form = new ZForm())
				{
					APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
					invoice.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
					TestObjectCreator.CreateInvoiceLine(invoice, 500);
					Factory.Save();

					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();
					testModule.PerformSearch_ForTest();

					BusinessObjectCollection testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(1, testCollection.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.DisplayGrid.SelectAllElements();
					testModule.HandlePrintSelfBillingInvoice_ForTestOnly(null, EventArgs.Empty);
					AssertEquals(APTransactionModuleStrip.CannotPrintAsASelfBilledTransactionMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					invoice.AH_TransactionCategory = Constants.TransactionCategory.Codes.SelfBilling;
					Factory.Save();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandlePrintSelfBillingInvoice_ForTestOnly(null, EventArgs.Empty);
					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestHandlePrintAutofatturaInvoice()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			using (DocumentsDataRegistry.Instance.UseNewDocBuilderCostConfirmationDocument.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (APTransactionModuleStrip testModule = APModule)
			using (ZForm form = new ZForm())
			{
				var errorMessage = "Autofattura (IT) can only be printed for Invoice, Credit Note and Adjustment transactions that have a Compliance Sub Type = APS and allocated Compliance Number";
				APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
				invoice.AH_ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.API;
				invoice.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
				TestObjectCreator.CreateInvoiceLine(invoice, 500);
				Factory.Save();

				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();
				testModule.PerformSearch_ForTest();

				BusinessObjectCollection testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
				testCollection.Load();
				AssertEquals(1, testCollection.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testModule.DisplayGrid.SelectAllElements();
				testModule.HandlePrintAutofatturaInvoice_ForTestOnly(testModule.GetPrintAutofatturaInvoiceMenuText, EventArgs.Empty);
				AssertEquals("Autofattura cannot be printed (Wrong SubType or empty, No Compliance Number)", errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				invoice.AH_ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.APS;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testModule.DisplayGrid.SelectAllElements();
				testModule.HandlePrintAutofatturaInvoice_ForTestOnly(testModule.GetPrintAutofatturaInvoiceMenuText, EventArgs.Empty);
				AssertEquals("Autofattura cannot be printed (No Compliance Number)", errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				invoice.AH_TransactionReference = "APS-00001";
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testModule.DisplayGrid.SelectAllElements();
				testModule.HandlePrintAutofatturaInvoice_ForTestOnly(testModule.GetPrintAutofatturaInvoiceMenuText, EventArgs.Empty);
				AssertEquals("Autofattura can be printed", null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestHandleExportRemittanceInfo()
		{
			Business.Base.AccStatement.BankStatement bank = Factory.NewWithValidTestData<Business.Base.AccStatement.BankStatement>();

			APPayment apPayment1 = Factory.NewWithValidTestData<APPayment>();
			apPayment1.AH_AB = bank.PK;
			apPayment1.AH_ReceiptType = ReceiptTypes.Cash;

			Factory.Save();

			using (APTransactionModuleStrip testModule = APModule)
			{
				using (ZForm form = new ZForm())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					testModule.PerformSearch_ForTest();
					AssertEquals(1, testModule.GridCollection.Count);

					testModule.HandleExportRemittanceInfo_ForTestOnly(testModule, EventArgs.Empty);
					AssertEquals("ExpectedMessage", "There are no outstanding invoices, credit notes or adjustment notes to export.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.DisplayGrid.SelectAllElements();
					testModule.HandleExportRemittanceInfo_ForTestOnly(testModule, EventArgs.Empty);
					Assert("Export was not risen any exceptions.", true);
				}
			}
		}

		public void TestHandleMatchRunPreSaveValidation()
		{
			Business.Base.AccStatement.BankStatement bank = Factory.NewWithValidTestData<Business.Base.AccStatement.BankStatement>();

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			apInvoice.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			apInvoice.AH_OSTotal = -500M;
			apInvoice.AH_InvoiceAmount = -500M;
			apInvoice.AH_OutstandingAmount = -500M;
			TestObjectCreator.CreateInvoiceLine(apInvoice, apInvoice.TransactionCurrency, apInvoice.AH_ExchangeRate, 500m, 0m, 0m, 500m, 0m, 0m);

			var apCreditNote = Factory.NewWithValidTestData<APCreditNote>();
			apCreditNote.AH_OH = apInvoice.AH_OH;
			apCreditNote.AH_OSTotal = 200M;
			apCreditNote.AH_InvoiceAmount = 200M;
			apCreditNote.AH_OutstandingAmount = 200M;
			TestObjectCreator.CreateInvoiceLine(apCreditNote, apCreditNote.TransactionCurrency, apCreditNote.AH_ExchangeRate, 200m, 0m, 0m, 200m, 0m, 0m);

			var apPayment = Factory.NewWithValidTestData<APPayment>();
			apPayment.AH_ReceiptType = ReceiptTypes.eNettDirectDebit;
			apPayment.AH_AB = bank.PK;
			apPayment.AH_ReceiptType = ReceiptTypes.Cash;
			apPayment.AH_OH = apInvoice.AH_OH;
			apPayment.AH_OSTotal = 300M;
			apPayment.AH_InvoiceAmount = 300M;
			apPayment.AH_OutstandingAmount = 300M;

			Factory.Save();

			using (APTransactionModuleStrip testModule = APModule)
			{
				using (ZForm form = new ZForm())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					testModule.PerformSearch_ForTest();
					AssertEquals(3, testModule.GridCollection.Count);

					testModule.DisplayGrid.SelectAllElements();
					testModule.HandleMatch_ForTestOnly(this, EventArgs.Empty);
					AssertNotNull("Matching Form should be shown due to Validation error", testModule.MatchingFormShown_ForTestOnly);

					((Form)testModule.MatchingFormShown_ForTestOnly).Close();
					testModule.MatchingFormShown_ForTestOnly = null;
				}
			}
		}

		public void TestOverrideGovernmentAllocatedNumberMenuItemUnauthorized()
		{
			using (APTransactionModuleStrip testModule = APModule)
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentAllocatedNumberBehavior.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (ZForm form1 = new ZForm())
			{
				Env.Security.PayablesModifyGovernmentAllocatedNumber.IsAllowed = false;

				var menu = testModule.GetNewActionMenuItems_ForTestOnly();
				var menuItem = menu.FindByText("Override Government Allocated Number");
				AssertNotNull("Menu item should exist", menuItem);

				form1.Controls.Add(testModule.EmbeddedControl);
				form1.Show();

				menuItem.PerformClick();
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Payables -> Payables Transactions -> Modify Government Allocated Number", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOverrideGovernmentAllocatedNumberMenuItem()
		{
			using (APTransactionModuleStrip testModule = APModule)
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentAllocatedNumberBehavior.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (ZForm form1 = new ZForm())
			{
				Env.Security.PayablesModifyGovernmentAllocatedNumber.IsAllowed = true;

				var menu = testModule.GetNewActionMenuItems_ForTestOnly();
				var menuItem = menu.FindByText("Override Government Allocated Number");
				AssertNotNull("Menu item should exist", menuItem);

				form1.Controls.Add(testModule.EmbeddedControl);
				form1.Show();

				testModule.PerformSearch_ForTest();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				menuItem.PerformClick();
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				AssertEquals("The information should read as follows: ", "Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);

				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
				Journal journal = CreateJournal();
				Factory.Save();

				testModule.PerformSearch_ForTest();
				BusinessObjectCollection collection = testModule.GridCollection as BusinessObjectCollection;
				collection.Load();
				AssertEquals("TestCollection should contain new transactions", 1, collection.Count);

				testModule.DisplayGrid.SelectAllElements();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				AssertEquals("The information should read as follows: ", "The 'Override Government Allocated Number' Action menu can only be run for INV, CRD and ADJ transaction types only.", UnitTestUserNotification.Instance.LastMessage.Text);

				var creditNote = CreateCreditNote();
				Factory.Save();

				testModule.PerformSearch_ForTest();
				collection.Load();
				AssertEquals("TestCollection should contain new transactions", 2, collection.Count);

				testModule.DisplayGrid.SelectAllElements();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				AssertEquals("The information should read as follows: ", "The 'Override Government Allocated Number' Action menu can only be run for INV, CRD and ADJ transaction types only.", UnitTestUserNotification.Instance.LastMessage.Text);

				journal.DeleteFromDB();

				var invoice = CreateInvoice();
				Factory.Save();

				testModule.PerformSearch_ForTest();

				collection.Load();
				AssertEquals("TestCollection should contain new transactions", 2, collection.Count);

				testModule.DisplayGrid.SelectAllElements();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();

				AssertType("LastFormShownDialogForTest", typeof(OverrideGovernmentAllocatedNumberForm), ZFormModaliser.LastFormShownForTest);

				var form = (OverrideGovernmentAllocatedNumberForm)ZFormModaliser.LastFormShownForTest;
				var bizo = (OverrideGovernmentAllocatedNumberHelper)form.BusinessEntity;

				AssertEquals("2 business object in grid for Override Government Allocated Number", 2, bizo.WrappedObjects.Count);

				bizo.WrappedObjects.Cast<InvoicingBase>().First(x => x.AH_TransactionType == TransactionTypes.Invoice).AH_GovernmentAllocatedID = "123-GOV-ID";
				bizo.WrappedObjects.Cast<InvoicingBase>().First(x => x.AH_TransactionType == TransactionTypes.CreditNote).AH_GovernmentAllocatedID = "123-GOV-ID";

				ContinueWithSave saveResult = form.FireSaveButton();

				AssertEquals("Precondition: form should be saved correctly", ContinueWithSave.Yes, saveResult);

				var newFactory = new BusinessObjectFactory();
				var invoiceInNewFactory = newFactory.Load<InvoicingBase>(invoice.PK);
				var creditNoteInNewFactory = newFactory.Load<InvoicingBase>(creditNote.PK);

				AssertEquals("Government Allocated Number should be updated", "123-GOV-ID", invoiceInNewFactory.AH_GovernmentAllocatedID);
				AssertEquals("Government Allocated Number should be updated", "123-GOV-ID", creditNoteInNewFactory.AH_GovernmentAllocatedID);
			}
		}

		public void TestQueryCacheClearedBeforeEverySearch()
		{
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			apInvoice.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			apInvoice.AH_OSTotal = -500M;
			apInvoice.AH_InvoiceAmount = -500M;
			apInvoice.AH_OutstandingAmount = -500M;
			TestObjectCreator.CreateInvoiceLine(apInvoice, 500);

			Factory.Save();

			using (APTransactionModuleStrip testModule = APModule)
			{
				using (ZForm form = new ZForm())
				{
					var filteredCollection = testModule.GridCollection as FilteredTransactionHeaderCollectionView;
					if (filteredCollection != null && filteredCollection.CollectionToFilter != null)
					{
						var factoryCacheManagerInstance = PersistentFactoryCacheManager.Instance;
						if (factoryCacheManagerInstance != null)
						{
							var weakReference = factoryCacheManagerInstance.persistentFactoryWeakReferences.Find((x) =>
							{
								var localCopy = x.Target as BusinessObjectFactory;
								return localCopy != null && localCopy.Equals(filteredCollection.CollectionToFilter.Factory);
							});
							factoryCacheManagerInstance.persistentFactoryWeakReferences.Remove(weakReference);
						}
					}

					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();
					testModule.PerformSearch_ForTest();
					AssertEquals(1, testModule.GridCollection.Count);

					var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
					orgHeader.OH_Code = "TEST13";
					var apInvoice1 = Factory.NewWithValidTestData<APInvoice>();
					apInvoice1.AH_OH = orgHeader.PK;
					apInvoice1.AH_OSTotal = -500M;
					apInvoice1.AH_InvoiceAmount = -500M;
					apInvoice1.AH_OutstandingAmount = -500M;
					TestObjectCreator.CreateInvoiceLine(apInvoice1, 500);

					Factory.Save();

					testModule.PerformSearch_ForTest();
					AssertEquals(2, testModule.GridCollection.Count);
				}
			}
		}

		[TestDate(2020, 10, 11)]
		public void TestOverrideInvoiceReferences()
		{
			using (var module = (APTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			{
				var overrideReferenceMenu = module.GetOverrideDetailsMenuItems_ForTestOnly().FindByText(module.OverrideInvoiceReferencesMenuText_ForTestOnly);
				using (ZForm form1 = new ZForm())
				{
					form1.Controls.Add(module.EmbeddedControl);
					form1.Show();

					var collection = module.GridCollection as BusinessObjectCollection;
					collection.Load();
					AssertEquals("Collection should be empty", 0, collection.Count);

					overrideReferenceMenu.PerformClick();
					AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);

					TestObjectCreator.CreateTestPeriods(new ZDateTime(2020, 1, 1));

					var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV001", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
					invoice.AH_OH = TestObjectCreator.AALSHI.PK;
					invoice.InvoiceRemittanceReference = "test001";
					invoice.AH_TransactionNum = "TN001";
					invoice.AH_PostDate = ZDateTime.Today;
					invoice.AH_InvoiceDate = new ZDateTime(new ZDateTime(ZDateTime.Today).Year, 1, 5);
					invoice.AH_ChequeOrReference = "COR001";

					var creditNote = TestObjectCreator.CreateInvoiceWithLine(typeof(APCreditNote), "INV002", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
					creditNote.AH_OH = TestObjectCreator.AALSHI.PK;
					var apPayment = TestObjectCreator.CreateAPPayment(1, 10, ZDateTime.Today, ZDateTime.Today, TestObjectCreator.AALSHI.PK, TestObjectCreator.AUDBankAccount.PK);
					Factory.Save();

					UnitTestUserNotification.Instance.ClearMessages();
					module.PerformSearch_ForTest();
					collection.Load();
					AssertEquals("Collection should contain 3 transactions", 3, collection.Count);
					module.DisplayGrid.SelectAllElements();
					overrideReferenceMenu.PerformClick();
					AssertEquals("You can only override invoice remittance reference for AP Invoices, Credit and Adjustment Notes.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					module.PerformSearch_ForTest();
					collection.Load();
					AssertEquals("Collection should contain 3 transactions", 3, collection.Count);
					module.DisplayGrid.SelectAllElements(x => x is InvoicingBase invoicingBase && invoicingBase.AH_TransactionType == TransactionTypes.Invoice);
					AssertEquals(1, module.DisplayGrid.SelectedElements.Length);

					overrideReferenceMenu.PerformClick();

					AssertType("LastFormShownDialogForTest", typeof(OverrideInvoiceReferenceForm), ZFormModaliser.LastFormShownForTest);
					var form = (OverrideInvoiceReferenceForm)ZFormModaliser.LastFormShownForTest;
					var bizo = (OverrideInvoiceReferenceHelper)form.BusinessEntity;

					AssertEquals("1 business object in grid for OverrideInvoiceReference", 1, bizo.WrappedObjects.Count);

					var modifiedInvoice = bizo.WrappedObjects.Cast<InvoicingBase>().First(x => x.AH_TransactionType == TransactionTypes.Invoice);
					modifiedInvoice.InvoiceRemittanceReference = "test002";
					modifiedInvoice.AH_TransactionNum = "TN002";
					modifiedInvoice.AH_InvoiceDate = new ZDateTime(new ZDateTime(ZDateTime.Today).Year, 1, 3);
					modifiedInvoice.AH_ChequeOrReference = "COR002";

					ContinueWithSave saveResult = form.FireSaveButton();

					AssertEquals("Precondition: form should be saved correctly", ContinueWithSave.Yes, saveResult);

					var newFactory = new BusinessObjectFactory();
					var invoiceInNewFactory = newFactory.Load<InvoicingBase>(invoice.PK);

					AssertEquals("Invoice Remittance Reference should be updated", "test002", invoiceInNewFactory.InvoiceRemittanceReference);
					AssertEquals("Invoice Transaction number should be updated", "TN002", invoiceInNewFactory.AH_TransactionNum);
					AssertEquals("Invoice Date should be updated", new ZDateTime(new ZDateTime(ZDateTime.Today).Year, 1, 3), invoiceInNewFactory.AH_InvoiceDate);
					AssertEquals("Invoice Supplier Cost Reference should be updated", "COR002", invoiceInNewFactory.AH_ChequeOrReference);
				}
			}
		}		

		public virtual void TestRegenerateJournalEntriesActionMenuItem()
		{
			var module = APModule;
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var actionMenu = module.GetNewActionMenuItems_ForTestOnly();
			var regenerateJournalEntriesMenuItem = actionMenu.FindByText("Regenerate Journal Entries (CWSupport Only)");
			AssertNotNull("Menu item should exist", regenerateJournalEntriesMenuItem);

			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			actionMenu = module.GetNewActionMenuItems_ForTestOnly();
			regenerateJournalEntriesMenuItem = actionMenu.FindByText("Regenerate Journal Entries (CWSupport Only)");
			AssertNull("Menu item should exist", regenerateJournalEntriesMenuItem);

			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var nonSupportStaff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, SQLComparisonOperator.NotEqual, User.SupportUserName));
			using (Env.SetTemporaryUserContext(nonSupportStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				actionMenu = module.GetNewActionMenuItems_ForTestOnly();
				regenerateJournalEntriesMenuItem = actionMenu.FindByText("Regenerate Journal Entries (CWSupport Only)");
				AssertNull("Menu item should exist", regenerateJournalEntriesMenuItem);
			}
		}

		[TestDate(2022, 01, 01)]
		public virtual void TestRegenerateJournalEntries()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2022);
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateAPSuspenseControlAccount().PK.ToGuid());
			var mockDataRecover = new Mock<IGeneralLedgerDataRecover>();

			using (ObjectFactory.Substitute(mockDataRecover.Object))
			using (var module = ZModuleFactory.Instance.Create(ModuleID) as APTransactionModuleStrip)
			{
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var apInvoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1, 100, 10, 100, 10);
					var line = apInvoice.Lines[0];
					line.AL_AT = TestObjectCreator.GST1.PK;
					line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;

					var matchLinkGroup = new TransactionMatchLinkGroup(Factory);
					var matchLink = matchLinkGroup.AddNew();
					matchLink.AP_MatchDate = ZDateTime.Today;
					matchLink.AP_MatchGroupNum = "M1";
					matchLink.AP_AH = apInvoice.PK;

					var cashBasisVAT = TestObjectCreator.CreateCashBasisVAT(line, -100, -10, matchLink);

					var taxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
					taxTransaction.ATT_AH = apInvoice.PK;
					taxTransaction.ATT_LocalTaxAmount = 10m;
					taxTransaction.ATT_OSTaxAmount = 200m;
					taxTransaction.ATT_AT_TaxID = Factory.NewWithValidTestData<AccTaxRate>().PK;
					taxTransaction.ATT_Basis = "MAT";
					taxTransaction.ATT_RateNumerator = 2;
					taxTransaction.ATT_RateDenominator = 1;
					taxTransaction.ATT_A9_TaxMessage = Factory.NewWithValidTestData<AccInvMsg>().PK;
					taxTransaction.ATT_GB = GlbBranch.CurrentBranch.PK;
					taxTransaction.ATT_GC = GlbCompany.CurrentCompany.PK;
					taxTransaction.ATT_GE_Department = GlbDepartment.CurrentDepartment.PK;
					taxTransaction.ATT_PostDate = ZDate.Today;
					taxTransaction.ATT_AG_LedgerControlAccount = Factory.NewWithValidTestData<AccGLHeader>().PK;
					taxTransaction.ATT_AG_TaxPendingControlAccount = Factory.NewWithValidTestData<AccGLHeader>().PK;

					var apJournal = TestObjectCreator.CreateJournal<APJournal>(100m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK);
					Factory.Save();

					var taxGLMovements = Factory.Load<AccTaxGLMovement>(new ZQuery(AccTaxGLMovementSchema.ATM_ATT_TaxTransaction, taxTransaction.PK));

					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();
					AssertEquals(2, module.SelectedBusinessObjects_ForTestOnly.Length);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					module.HandleRegenerateJournalEntries_ForTestOnly(this, new EventArgs());
					mockDataRecover.Verify(x => x.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, It.Is<BusinessObject[]>(y => y.Length == 1 && y.FirstOrDefault().PK == apJournal.PK)), Times.Once);
					mockDataRecover.Verify(x => x.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, It.Is<BusinessObject[]>(y => y.Length == apInvoice.Lines.Count && y.All(z => apInvoice.Lines.Any(a => a.PK == z.PK)))), Times.Once);
					mockDataRecover.Verify(x => x.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_YC_CashBasisVAT, It.Is<BusinessObject[]>(y => y.Length == 1 && y.FirstOrDefault().PK == cashBasisVAT.PK)), Times.Once);
					mockDataRecover.Verify(x => x.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_ATM_TaxGLMovement, It.Is<BusinessObject[]>(y => y.Length == taxGLMovements.Length && y.All(z => taxGLMovements.Any(a => a.PK == z.PK)))), Times.Once);
				}
			}
		}

		[TestDate(2022, 01, 01)]
		public void TestPopupConfirmForm_WhenRegenerateJournalEntries()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2022);
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateAPSuspenseControlAccount().PK.ToGuid());

			var complianceReportGEN = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReportGEN.ACR_ReportType = "TST";
			complianceReportGEN.ACR_DateFrom = ZDate.Today.AddDays(-2);
			complianceReportGEN.ACR_DateTo = ZDate.Today.AddDays(2);
			complianceReportGEN.ACR_Status = AccComplianceReport.Status.ReportGenerated;

			Factory.Save();

			var mockServiceTaskNudger = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(mockServiceTaskNudger.Object))
			using (var module = ZModuleFactory.Instance.Create(ModuleID) as APTransactionModuleStrip)
			{
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();
					module.PerformSearch_ForTest();

					var transactions = GetBusinessObjectsToGetControllersFor();
					Factory.Save();

					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectSingleElement(transactions[0]);
					AssertEquals(1, module.SelectedBusinessObjects_ForTestOnly.Length);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					module.HandleRegenerateJournalEntries_ForTestOnly(this, new EventArgs());
					Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Are you sure you want to Delete and Regenerate General Ledger Data for selected Transactions?"));
					AssertEquals("General Ledger Data for selected Transactions have been regenerated", UnitTestUserNotification.Instance.LastMessage.Text);

					var complianceReportFIN = Factory.NewWithValidTestData<AccComplianceReport>();
					complianceReportFIN.ACR_ReportType = "TST";
					complianceReportFIN.ACR_DateFrom = ZDate.Today.AddDays(-2);
					complianceReportFIN.ACR_DateTo = ZDate.Today.AddDays(2);
					complianceReportFIN.ACR_Status = AccComplianceReport.Status.ReportFinalised;

					Factory.Save();

					var gldData = Factory.LoadTop1<AccGeneralLedgerData>(new ZQuery());
					var sql = $@"INSERT INTO AccComplianceReportTransactionPivot (ACL_PK, ACL_ParentID, ACL_ParentTableCode, ACL_GC_Company, ACL_ACR_Report, ACL_ReportSequence) VALUES
							(NEWID(), '{gldData.PK}', 'GLD', '{GlbCompany.CurrentCompany.PK}', '{complianceReportFIN.PK}', 1)
							";
					TestConnection.ExecuteNonQuery(sql);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					module.HandleRegenerateJournalEntries_ForTestOnly(this, new EventArgs());
					var msg = @"You are going to change the status of Compliance Report that uses the GLD data from ""FIN - Report Finalised"" to ""INV - Report Invalidated by Transactions Updated""";
					Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(msg));
				}
			}
		}

		public void TestMalaysiaShouldHaveEInvoicingActionMenuItems()
		{
			AssertShouldHaveEInvoicingActionMenuItems(CountryCodes.Malaysia, true);

			var countrys = new List<string>() { CountryCodes.KoreaSouth, CountryCodes.Mexico, CountryCodes.VietNam, CountryCodes.China, CountryCodes.Turkey };
			foreach (var country in countrys)
			{
				AssertShouldHaveEInvoicingActionMenuItems(country);
			}
		}

		void AssertShouldHaveEInvoicingActionMenuItems(string countryCode, bool shouldHaveEInvoicingActionMenuItems = false)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				using (var module = ZModuleFactory.Instance.Create(ModuleID) as APTransactionModuleStrip)
				{
					var menuItems = module.GetEInvoicingActionMenuItems_ForTestOnly();

					Assert(!menuItems.Any());

					using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						menuItems = module.GetEInvoicingActionMenuItems_ForTestOnly();
						Assert(shouldHaveEInvoicingActionMenuItems ? menuItems.Any() : !menuItems.Any());
					}
				}
			}
		}

		public void TestOverrideGovernmentAllocatedNumberAvailability()
		{
			using (APModule)
			{
				using (var form = new ZForm())
				{
					var menu = APModule.GetNewActionMenuItems_ForTestOnly();
					var menuitem = menu.FindByText("Override Government Allocated Number");
					AssertNull(menuitem);
				}
			}

			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentAllocatedNumberBehavior.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var menu = APModule.GetNewActionMenuItems_ForTestOnly();
				var menuitem = menu.FindByText("Override Government Allocated Number");
				AssertNotNull(menuitem);
			}
		}

		#region MatchTransactionsMenuItem

		protected override InvoicingBase CreateTransactionsForMatchTransactionPopup()
		{
			TestObjectCreator.CreateAPInvoice<APInvoice>("AP100001", TestObjectCreator.AUD, 1.0m, 250m, 25m, 0m, 250m, 25m, 0m, TestObjectCreator.Creditor1);
			TestObjectCreator.CreateAPInvoice<APInvoice>("AP100002", TestObjectCreator.AUD, 1.0m, 350m, 35m, 0m, 350m, 35m, 0m, TestObjectCreator.Creditor1);
			return TestObjectCreator.CreateAPInvoice<APInvoice>("AP100003", TestObjectCreator.AUD, 1.0m, 450m, 45m, 0m, 450m, 45m, 0m, TestObjectCreator.Creditor1);
		}

		protected override Type ExpectedModuleTypeForMatchTransactionPopup => typeof(APMatchingModule);

		protected override SecurityCheckpoint SecurityCheckpointForMatchTransactions => Env.Security.MatchPayablesTransactions;

		#endregion

		#region Implementations

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.APTransaction;
		}

		protected override Journal CreateJournal()
		{
			return Factory.NewWithValidTestData<APJournal>();
		}

		protected override InvoicingBase CreateInvoice()
		{
			return Factory.NewWithValidTestData<APInvoice>();
		}

		protected override InvoicingBase CreateCreditNote()
		{
			return Factory.NewWithValidTestData<APCreditNote>();
		}

		protected override Receipt CreateReceipt()
		{
			return Factory.NewWithValidTestData<APReceipt>();
		}

		protected override Payment CreatePayment()
		{
			return Factory.NewWithValidTestData<APPayment>();
		}

		APTransactionModuleStrip APModule => (APTransactionModuleStrip)TestTransactionModule;

		protected override BusinessObject[] GetBusinessObjectsToGetControllersFor()
		{
			APContraRow apContraRow = Factory.NewWithValidTestData<APContraRow>();
			ARContraRow arContraRow = Factory.NewWithValidTestData<ARContraRow>();
			arContraRow.AH_TransactionNum = apContraRow.AH_TransactionNum;
			apContraRow.AH_TransactionBelongsToGroup = arContraRow.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
			arContraRow.AH_OH = TestObjectCreator.ABIGAS.PK;
			apContraRow.AH_OH = TestObjectCreator.AALSHI.PK;

			APTransferFromRow transferFrom1 = Factory.NewWithValidTestData<APTransferFromRow>();
			APTransferToRow transferTo1 = Factory.NewWithValidTestData<APTransferToRow>();
			transferTo1.AH_TransactionCount = 2;
			transferTo1.AH_TransactionNum = transferFrom1.AH_TransactionNum;
			transferFrom1.AH_TransactionBelongsToGroup = transferTo1.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
			transferTo1.AH_OH = TestObjectCreator.Creditor1.PK;
			transferFrom1.AH_OH = TestObjectCreator.Creditor2.PK;
			transferFrom1.AH_OSExTaxAmount = transferTo1.AH_OSExTaxAmount = 10M;

			APTransferFromRow transferFrom2 = Factory.NewWithValidTestData<APTransferFromRow>();
			APTransferToRow transferTo2 = Factory.NewWithValidTestData<APTransferToRow>();
			transferTo2.AH_TransactionCount = 2;
			transferTo2.AH_TransactionNum = transferFrom2.AH_TransactionNum;
			transferFrom2.AH_TransactionBelongsToGroup = transferTo2.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
			transferTo2.AH_OH = TestObjectCreator.Creditor1.PK;
			transferFrom2.AH_OH = TestObjectCreator.Creditor2.PK;
			transferFrom2.AH_OSExTaxAmount = transferTo2.AH_OSExTaxAmount = 10M;

			var aRReceipt = Factory.NewWithValidTestData<APReceipt>();
			var aPPayment = Factory.NewWithValidTestData<APPayment>();
			var bank = Factory.NewWithValidTestData<AccBankAccount>();
			aRReceipt.AH_AB = bank.PK;
			aPPayment.AH_AB = bank.PK;

			return new BusinessObject[] { apContraRow,
												transferFrom1,
												transferTo2,
												Factory.NewWithValidTestData(typeof(APInvoice)),
												Factory.NewWithValidTestData(typeof(APCreditNote)),
												Factory.NewWithValidTestData(typeof(APAdjustmentNote)),
												Factory.NewWithValidTestData(typeof(APJournal)),
												aPPayment,
												aRReceipt };
		}

		public override void TestPromptToPrintComplianceDocumentWithRollup()
		{
			PromptToPrintComplianceDocumentCore(AssertForNotPromptToPrintComplianceDocument, true, true);
			PromptToPrintComplianceDocumentCore(AssertForNotPromptToPrintComplianceDocument, true, false);
		}

		public override void TestPromptToPrintComplianceDocumentWithoutRollup()
		{
			PromptToPrintComplianceDocumentCore(AssertForNotPromptToPrintComplianceDocument, false, true);
			PromptToPrintComplianceDocumentCore(AssertForNotPromptToPrintComplianceDocument, false, false);
		}

		protected override ControllerID InvoiceControllerID => ControllerIDs.APInvoice;

		protected override ControllerID ExchangeDifferenceControllerID => ControllerIDs.APExchangeDifference;

		protected override ControllerID DiscountControllerID => ControllerIDs.APDiscount;

		protected override ControllerID OverpaymentControllerID => ControllerIDs.APOverpayment;

		protected override AccTransactionHeader GetInvoiceToTestControllerID()
		{
			return Factory.NewWithValidTestData<APInvoice>();
		}

		protected override Invoice GetExistingInvoiceForTest => APInv;

		BusinessObjectFactory ModuleFactory => APModule.Factory_ForTesTonly;

		void SetupDataForPayInvoicesTest()
		{
			OrgHeader testOrg = ModuleFactory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_Code = TestObjectCreator.GetRandomString(10);
			testOrg.CompanyData.OB_IsCreditor = true;
			testOrg.APSettlementGroupPK = testOrg.PK;

			OrgHeader testOrg2 = ModuleFactory.NewWithValidTestData<OrgHeader>();
			testOrg2.OH_Code = TestObjectCreator.GetRandomString(10);
			testOrg2.CompanyData.OB_IsCreditor = true;
			testOrg2.APSettlementGroupPK = testOrg.PK;

			OrgHeader testOrg3 = ModuleFactory.NewWithValidTestData<OrgHeader>();
			testOrg3.OH_Code = TestObjectCreator.GetRandomString(10);
			testOrg3.CompanyData.OB_IsCreditor = true;
			testOrg3.APSettlementGroupPK = ZGuid.Empty;

			OrgHeader testOrg4 = ModuleFactory.NewWithValidTestData<OrgHeader>();
			testOrg4.OH_Code = TestObjectCreator.GetRandomString(10);
			testOrg4.CompanyData.OB_IsCreditor = true;
			testOrg4.APSettlementGroupPK = testOrg3.PK;

			OrgHeader testOrg7 = ModuleFactory.NewWithValidTestData<OrgHeader>();
			testOrg7.OH_Code = TestObjectCreator.GetRandomString(10);
			testOrg7.CompanyData.OB_IsCreditor = true;
			testOrg7.APSettlementGroupPK = testOrg7.PK;

			OrgHeader testOrg6 = ModuleFactory.NewWithValidTestData<OrgHeader>();
			testOrg6.OH_Code = TestObjectCreator.GetRandomString(10);
			testOrg6.CompanyData.OB_IsCreditor = true;
			testOrg6.APSettlementGroupPK = testOrg7.PK;

			OrgHeader testOrg5 = ModuleFactory.NewWithValidTestData<OrgHeader>();
			testOrg5.OH_Code = TestObjectCreator.GetRandomString(10);
			testOrg5.CompanyData.OB_IsCreditor = true;
			testOrg5.APSettlementGroupPK = testOrg6.PK;

			OrgHeader testOrg8 = ModuleFactory.NewWithValidTestData<OrgHeader>();
			testOrg8.OH_Code = TestObjectCreator.GetRandomString(10);
			testOrg8.CompanyData.OB_IsCreditor = true;
			testOrg8.APSettlementGroupPK = testOrg8.PK;

			OrgHeader testOrg9 = ModuleFactory.NewWithValidTestData<OrgHeader>();
			testOrg9.OH_Code = TestObjectCreator.GetRandomString(10);
			testOrg9.CompanyData.OB_IsCreditor = true;
			testOrg9.APSettlementGroupPK = testOrg8.PK;

			TestAPInv = ModuleFactory.NewWithValidTestData<APInvoice>();
			TestAPInv.AH_OH = testOrg.PK;
			TestAPInv.AH_LocalExTaxAmount = 94M;
			TestAPInv.AH_OSExTaxAmount = 94M;
			TestObjectCreator.CreateInvoiceLine(TestAPInv, TestAPInv.TransactionCurrency, TestAPInv.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m);

			TestAPInv2 = ModuleFactory.NewWithValidTestData<APInvoice>();
			TestAPInv2.AH_OH = testOrg2.PK;
			TestObjectCreator.CreateInvoiceLine(TestAPInv2, TestAPInv2.TransactionCurrency, TestAPInv2.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m);

			TestAPRec = ModuleFactory.NewWithValidTestData<APReceipt>();
			TestAPRec.AH_OH = testOrg3.PK;
			TestAPRec.AH_LocalExTaxAmount = 94M;
			TestAPRec.AH_OSExTaxAmount = 94M;

			TestAPRec2 = ModuleFactory.NewWithValidTestData<APReceipt>();
			TestAPRec2.AH_OH = testOrg4.PK;
			TestAPRec2.AH_LocalExTaxAmount = 94M;
			TestAPRec2.AH_OSExTaxAmount = 94M;

			TestAPInv3 = ModuleFactory.NewWithValidTestData<APInvoice>();
			TestAPInv3.AH_OH = testOrg9.PK;
			TestObjectCreator.CreateInvoiceLine(TestAPInv3, TestAPInv3.TransactionCurrency, TestAPInv3.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m);

			APInvoice testAPInv4 = ModuleFactory.NewWithValidTestData<APInvoice>();
			testAPInv4.AH_OH = testOrg5.PK;
			TestObjectCreator.CreateInvoiceLine(testAPInv4, testAPInv4.TransactionCurrency, testAPInv4.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m);

			APInvoice testAPInv5 = ModuleFactory.NewWithValidTestData<APInvoice>();
			testAPInv5.AH_OH = testOrg6.PK;
			TestObjectCreator.CreateInvoiceLine(testAPInv5, testAPInv5.TransactionCurrency, testAPInv5.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m);

			TestAPInv6 = ModuleFactory.NewWithValidTestData<APInvoice>();
			TestAPInv6.AH_OH = testOrg7.PK;
			TestObjectCreator.CreateInvoiceLine(TestAPInv6, TestAPInv6.TransactionCurrency, TestAPInv6.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m);

			ModuleFactory.Save();

			TransactionsCollection = new BusinessObject[8];
			TransactionsCollection[0] = TestAPInv;
			TransactionsCollection[1] = TestAPInv2;
			TransactionsCollection[2] = TestAPRec;
			TransactionsCollection[3] = TestAPRec2;
			TransactionsCollection[4] = TestAPInv3;
			TransactionsCollection[5] = testAPInv4;
			TransactionsCollection[6] = testAPInv5;
			TransactionsCollection[7] = TestAPInv6;
		}

		APInvoice TestAPInv;
		APInvoice TestAPInv2;
		APInvoice TestAPInv3;
		APInvoice TestAPInv6;
		APReceipt TestAPRec;
		APReceipt TestAPRec2;
		BusinessObject[] TransactionsCollection;

		protected virtual APTransactionModuleStrip GetNewModuleObject(IWithholdingJournalCreatorForMultipleInvoices withholdingJournalCreatorForMultipleInvoices, IWithholdingJournalRealizerForMultipleInvoices withholdingJournalRealizerForMultipleInvoices)
		{
			return new APTransactionModuleStrip((withholdingJournalCreatorForMultipleInvoices, withholdingJournalRealizerForMultipleInvoices));
		}

		#endregion
	}
}
