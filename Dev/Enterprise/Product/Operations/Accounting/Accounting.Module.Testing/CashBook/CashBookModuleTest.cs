using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.Accounting.Business.CashBook.OpeningPayment;
using Enterprise.Accounting.Business.CashBook.OpeningReceipt;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.ARAP.Payment;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(CashBookTransactionModule))]
	sealed class CashBookModuleTest : FilterGridModuleWithMultipleReversingTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.CashbookTransaction;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			collection.AddRange(GetBusinessObjectsToGetControllersFor());
		}

		protected override BusinessObject[] GetBusinessObjectsToGetControllersFor()
		{
			BankTransferFromRow bankTransferFrom1 = Factory.NewWithValidTestData<BankTransferFromRow>();
			BankTransferToRow bankTransferTo1 = Factory.NewWithValidTestData<BankTransferToRow>();
			bankTransferFrom1.AH_TransactionBelongsToGroup = bankTransferTo1.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
			bankTransferTo1.AH_OH = TestObjectCreator.LocalClient.PK;
			bankTransferFrom1.AH_OH = TestObjectCreator.LocalClient2.PK;
			bankTransferTo1.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			bankTransferFrom1.AH_AB = TestObjectCreator.AUDBankAccount2.PK;
			bankTransferFrom1.AH_OSExTaxAmount = bankTransferTo1.AH_OSExTaxAmount = 10M;
			bankTransferFrom1.AH_RX_NKTransactionCurrency = bankTransferTo1.AH_RX_NKTransactionCurrency = "AUD";

			BankTransferFromRow bankTransferFrom2 = Factory.NewWithValidTestData<BankTransferFromRow>();
			BankTransferToRow bankTransferTo2 = Factory.NewWithValidTestData<BankTransferToRow>();
			bankTransferFrom2.AH_TransactionBelongsToGroup = bankTransferTo2.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
			bankTransferTo2.AH_OH = TestObjectCreator.LocalClient.PK;
			bankTransferFrom2.AH_OH = TestObjectCreator.LocalClient2.PK;
			bankTransferTo2.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			bankTransferFrom2.AH_AB = TestObjectCreator.AUDBankAccount2.PK;
			bankTransferFrom2.AH_OSExTaxAmount = bankTransferTo2.AH_OSExTaxAmount = 10M;
			bankTransferFrom2.AH_RX_NKTransactionCurrency = bankTransferTo2.AH_RX_NKTransactionCurrency = "AUD";

			return new BusinessObject[] { Factory.NewWithValidTestData<OpeningReceipt>(),
												Factory.NewWithValidTestData<OpeningPayment>(),
												Factory.NewWithValidTestData<DirectReceipt>(),
												Factory.NewWithValidTestData<DirectPayment>(),
												bankTransferFrom1,
												bankTransferTo2,
												Factory.NewWithValidTestData<CashbookExchangeDiff>()
											};
		}

		public void TestGetActionMenu()
		{
			using (CashBookTransactionModule module = (CashBookTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				MenuItem[] standardMenuItems = module.GetNewStandardMenuItems_ForTestOnly();
				AssertEquals(0, Array.IndexOf(standardMenuItems, standardMenuItems.FindByText("View")));
				AssertEquals(1, Array.IndexOf(standardMenuItems, standardMenuItems.FindByText("New")));
				AssertEquals(2, Array.IndexOf(standardMenuItems, standardMenuItems.FindByText("Edit")));
				AssertEquals(3, Array.IndexOf(standardMenuItems, standardMenuItems.FindByText("Copy")));
				AssertEquals(4, Array.IndexOf(standardMenuItems, standardMenuItems.FindByText("Reverse")));

				var newMenuItem = standardMenuItems.FindByText("New");
				AssertNotNull("There should be an 'opening receipt' menu item", newMenuItem.MenuItems.FindByText(module.NewOpeningReceiptText_ForTestOnly));
				AssertNotNull("There should be an 'opening payment' menu item", newMenuItem.MenuItems.FindByText(module.NewOpeningPaymentText_ForTestOnly));
				AssertNotNull("There should be a 'bank currency adjustment' menu item", newMenuItem.MenuItems.FindByText(module.NewBankCurrencyAdjustmentText_ForTestOnly));
				AssertNotNull("There should be a 'bank transfer' menu item", newMenuItem.MenuItems.FindByText(module.NewBankTransferText_ForTestOnly));
				AssertNotNull("There should be a 'direct receipt' menu item", newMenuItem.MenuItems.FindByText(module.NewDirectReceiptText_ForTestOnly));
				AssertNotNull("There should be a 'direct payment' menu item", newMenuItem.MenuItems.FindByText(module.NewDirectPaymentText_ForTestOnly));

				MenuItem[] additionalMenuItems = module.GetNewAdditionalMenuItems_ForTestOnly();
				var actionMenuItem = additionalMenuItems.FindByText("Actions");
				AssertNotNull("There should be an 'import' menu item", actionMenuItem.MenuItems.FindByText("D&ata Transfer"));
				AssertNotNull("There should be a 'Print Accounting Journal' menu item", actionMenuItem.MenuItems.FindByText("Print Accounting Journal"));
				AssertNotNull("There should be a 'print' menu item", actionMenuItem.MenuItems.FindByText(module.PrintMenuItemText_ForTestOnly));
			}
		}

		public void TestRegenerateJournalEntriesActionMenuItem()
		{
			using (CashBookTransactionModule testModule = (CashBookTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var actionMenu = testModule.GetNewActionMenuItems_ForTestOnly();
				var regenerateJournalEntriesMenuItem = actionMenu.FindByText("Regenerate Journal Entries (CWSupport Only)");
				AssertNotNull("Menu item should exist", regenerateJournalEntriesMenuItem);

				AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				actionMenu = testModule.GetNewActionMenuItems_ForTestOnly();
				regenerateJournalEntriesMenuItem = actionMenu.FindByText("Regenerate Journal Entries (CWSupport Only)");
				AssertNull("Menu item should not exist", regenerateJournalEntriesMenuItem);

				AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var nonSupportStaff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, SQLComparisonOperator.NotEqual, User.SupportUserName));
				using (Env.SetTemporaryUserContext(nonSupportStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					actionMenu = testModule.GetNewActionMenuItems_ForTestOnly();
					regenerateJournalEntriesMenuItem = actionMenu.FindByText("Regenerate Journal Entries (CWSupport Only)");
					AssertNull("Menu item should not exist", regenerateJournalEntriesMenuItem);
				}
			}
		}

		public void TestRegenerateJournalEntries()
		{
			var mockDataRecover = new Mock<IGeneralLedgerDataRecover>();

			using (ObjectFactory.Substitute(mockDataRecover.Object))
			using (var module = ZModuleFactory.Instance.Create(ModuleID) as CashBookTransactionModule)
			{
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var testDirectPayment = Factory.NewWithValidTestData<DirectPayment>();
					testDirectPayment.Lines.AddNew();
					testDirectPayment.Lines[0].AL_OSExTaxAmount = 15m;
					testDirectPayment.Lines[0].AL_LocalExTaxAmount = 15m;
					var testCashbookExchangeDiff = Factory.NewWithValidTestData<CashbookExchangeDiff>();

					Factory.Save();

					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();

					AssertEquals(2, module.SelectedBusinessObjects_ForTestOnly.Length);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					module.HandleRegenerateJournalEntries_ForTestOnly(this, new EventArgs());
					mockDataRecover.Verify(x => x.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, It.Is<BusinessObject[]>(y => y.Length == 1 && y.FirstOrDefault().PK == testCashbookExchangeDiff.PK)), Times.Once);
					mockDataRecover.Verify(x => x.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, It.Is<BusinessObject[]>(y => y.Length == testDirectPayment.Lines.Count && y.All(z => testDirectPayment.Lines.Any(a => a.PK == z.PK)))), Times.Once);

					var testOpeningPayment = Factory.NewWithValidTestData<OpeningPayment>();
					Factory.Save();

					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();

					AssertEquals(3, module.SelectedBusinessObjects_ForTestOnly.Length);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					module.HandleRegenerateJournalEntries_ForTestOnly(this, new EventArgs());
					AssertEquals("Journal Entries cannot be regenerated for Opening Receipts or Payments.", UnitTestUserNotification.Instance.LastMessage.Text);
					mockDataRecover.Verify(x => x.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, It.Is<BusinessObject[]>(y => y.Length == 1 && y.FirstOrDefault().PK == testCashbookExchangeDiff.PK)), Times.Once);
					mockDataRecover.Verify(x => x.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, It.Is<BusinessObject[]>(y => y.Length == testDirectPayment.Lines.Count && y.All(z => testDirectPayment.Lines.Any(a => a.PK == z.PK)))), Times.Once);
					mockDataRecover.Verify(x => x.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, It.Is<BusinessObject[]>(y => y.Length == 1 && y.FirstOrDefault().PK == testOpeningPayment.PK)), Times.Never);
				}
			}
		}

		public void TestChinaHasPrintAccountingVoucherMenuItem()
		{
			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
				using (CashBookTransactionModule module = (CashBookTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
				{
					MenuItem[] actionMenuItems = module.GetNewActionMenuItems_ForTestOnly();
					AssertNotNull("There should be a 'print accounting voucher' menu item", actionMenuItems.FindByText(module.PrintAccountingVoucherText_ForTestOnly));
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		public void TestTaiwanHasPrintAccountingVoucherMenuItem()
		{
			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
				using (CashBookTransactionModule module = (CashBookTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
				{
					MenuItem[] actionMenuItems = module.GetNewActionMenuItems_ForTestOnly();
					AssertNotNull("There should be a 'print accounting voucher' menu item", actionMenuItems.FindByText(module.PrintAccountingVoucherText_ForTestOnly));
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		public void TestReplaceTransferToWithTransferFromWhenPrintAccountingVoucher()
		{
			TransactionHeader[] originalTransactonHeaders = new TransactionHeader[2];
			BankTransferToRow transferTo1 = Factory.NewWithValidTestData<BankTransferToRow>();
			transferTo1.AH_TransactionCount = 2;

			BankTransferFromRow transferFrom1 = Factory.NewWithValidTestData<BankTransferFromRow>();
			transferFrom1.AH_TransactionNum = transferTo1.AH_TransactionNum;
			transferFrom1.AH_TransactionCount = 1;

			BankTransferFromRow transferFrom2 = Factory.NewWithValidTestData<BankTransferFromRow>();
			transferFrom2.AH_TransactionCount = 1;

			BankTransferToRow transferTo2 = Factory.NewWithValidTestData<BankTransferToRow>();
			transferTo2.AH_TransactionNum = transferFrom2.AH_TransactionNum;
			transferTo2.AH_TransactionCount = 2;

			originalTransactonHeaders[0] = transferTo1;
			originalTransactonHeaders[1] = transferFrom2;
			TransactionHeader[] updatedTransactonHeaders = CashBookTransactionModule.ReplaceTransferToWithTransferFromWhenPrintingAccountingVoucher_ForTestOnly(originalTransactonHeaders);
			AssertEquals("BankTransferFromRow", updatedTransactonHeaders[0].GetType().Name);
			AssertEquals(transferFrom1.PK, updatedTransactonHeaders[0].PK);
			AssertEquals("BankTransferFromRow", updatedTransactonHeaders[1].GetType().Name);
			AssertEquals(transferFrom2.PK, updatedTransactonHeaders[1].PK);

			transferTo1.AH_TransactionCount = 5;
			transferFrom1.AH_TransactionCount = 4;
			transferTo2.AH_TransactionCount = 5;
			transferFrom2.AH_TransactionCount = 4;

			updatedTransactonHeaders = CashBookTransactionModule.ReplaceTransferToWithTransferFromWhenPrintingAccountingVoucher_ForTestOnly(originalTransactonHeaders);
			AssertEquals("BankTransferFromRow", updatedTransactonHeaders[0].GetType().Name);
			AssertEquals(transferFrom1.PK, updatedTransactonHeaders[0].PK);
			AssertEquals("BankTransferFromRow", updatedTransactonHeaders[1].GetType().Name);
			AssertEquals(transferFrom2.PK, updatedTransactonHeaders[1].PK);
		}

		public void TestPositivePayMenuItems()
		{
			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				using (CashBookTransactionModule module = (CashBookTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
				{
					MenuItem[] actionMenuItems = module.GetNewActionMenuItems_ForTestOnly();
					AssertNull("There should not be a 'Export Positive Pay' menu item", actionMenuItems.FindByText(module.ExportPositivePayMenuItemText_ForTestOnly));
					AssertNull("There should not be a 'Customize Positive Pay Export' menu item", actionMenuItems.FindByText(module.CustomizePositivePayExportMenuItemText_ForTestOnly));
					AssertNotNull("There should be a 'Export Check Payments' menu item", actionMenuItems.FindByText(module.ExportCheckPaymentsMenuItemText_ForTestOnly));
					AssertNotNull("There should be a 'Customize Check Payments Export' menu item", actionMenuItems.FindByText(module.CustomizeCheckPaymentsExportMenuItemText_ForTestOnly));
				}

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
				using (CashBookTransactionModule module = (CashBookTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
				{
					MenuItem[] actionMenuItems = module.GetNewActionMenuItems_ForTestOnly();
					AssertNotNull("There should be a 'Export Positive Pay' menu item", actionMenuItems.FindByText(module.ExportPositivePayMenuItemText_ForTestOnly));
					AssertNotNull("There should be a 'Customize Positive Pay Export' menu item", actionMenuItems.FindByText(module.CustomizePositivePayExportMenuItemText_ForTestOnly));
					AssertNull("There should not be a 'Export Check Payments' menu item", actionMenuItems.FindByText(module.ExportCheckPaymentsMenuItemText_ForTestOnly));
					AssertNull("There should not be a 'Customize Check Payments Export' menu item", actionMenuItems.FindByText(module.CustomizeCheckPaymentsExportMenuItemText_ForTestOnly));
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		public void TestReAllocateCheckNumberNoTransactionIsSelectedToReAllocate()
		{
			using (CashBookTransactionModule testModule = new CashBookTransactionModule())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testModule.HandleReallocateCheckNumber_ForTestOnly(null, new EventArgs());
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

			DirectPayment payment = Factory.NewWithValidTestData<DirectPayment>();
			payment.AH_AB = bank.PK;
			payment.AH_ReceiptType = ReceiptTypes.Cash;

			APPayment apPayment = Factory.NewWithValidTestData<APPayment>();
			apPayment.AH_AB = bank.PK;
			apPayment.AH_ReceiptType = ReceiptTypes.Cash;

			Factory.Save();

			using (CashBookTransactionModule testModule = new CashBookTransactionModule())
			{
				using (ZForm form = new ZForm())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					CashBookFilterBusinessObject cashBookFilterBO = (CashBookFilterBusinessObject)testModule.FilterBusinessObject;
					((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).Property = "";
					((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).IsActive = true;
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

					payment.AH_ReceiptType = ReceiptTypes.Cheque;
					payment.AH_ChequeOrReference = "000001";
					apPayment.AH_ReceiptType = ReceiptTypes.Cheque;
					apPayment.AH_ChequeOrReference = "000002";
					Factory.Save();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					testModule.HandleReprint_ForTestOnly(null, null);
					AssertEquals("ExpectedMessage", null, UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("LastMessage", ZFormModaliser.LastFormShownDialogForTest is PaymentDocumentsPrintPopup);
					AssertEquals("000003", payment.AH_ChequeOrReference);
					AssertEquals("000004", apPayment.AH_ChequeOrReference);

					StmALog[] logs = payment.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Check number has been updated during reprinting. Was 000001 Now 000003"));
					AssertEquals(1, logs.Length);
					logs = apPayment.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Check number has been updated during reprinting. Was 000002 Now 000004"));
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

			DirectPayment payment = Factory.NewWithValidTestData<DirectPayment>();
			payment.AH_AB = bank.PK;
			payment.AH_ReceiptType = ReceiptTypes.Cash;

			APPayment apPayment = Factory.NewWithValidTestData<APPayment>();
			apPayment.AH_AB = bank.PK;
			apPayment.AH_ReceiptType = ReceiptTypes.Cash;

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

			using (CashBookTransactionModule testModule = new CashBookTransactionModule())
			{
				using (ZForm form = new ZForm())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					CashBookFilterBusinessObject cashBookFilterBO = (CashBookFilterBusinessObject)testModule.FilterBusinessObject;
					((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).Property = "";
					((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).IsActive = true;
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

					payment.AH_ReceiptType = ReceiptTypes.Cheque;
					payment.AH_ChequeOrReference = "000001";
					apPayment.AH_ReceiptType = ReceiptTypes.Cheque;
					apPayment.AH_ChequeOrReference = "000002";
					Factory.Save();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					testModule.HandleReallocateCheckNumber_ForTestOnly(null, null);
					AssertEquals("ExpectedMessage", null, UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("LastMessage", ZFormModaliser.LastFormShownDialogForTest is ChequeNumberReallocationForm);
					AssertEquals("000003", payment.AH_ChequeOrReference);
					AssertEquals("000004", apPayment.AH_ChequeOrReference);
					AssertEquals("000004", charge.JR_ChequeNo);
					AssertEquals("000004", cost.E6_ChequeOrReference);

					StmALog[] logs = payment.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Check number has been updated and check was not reprinted. Was 000001 Now 000003"));
					AssertEquals(1, logs.Length);
					logs = apPayment.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Check number has been updated and check was not reprinted. Was 000002 Now 000004"));
					AssertEquals(1, logs.Length);
					logs = payment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, ZArchitecture.Business.Events.EditedARecord.Code));
					AssertEquals(2, logs.Length);
					logs = apPayment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, ZArchitecture.Business.Events.EditedARecord.Code));
					AssertEquals(2, logs.Length);
				}
			}
		}

		string BaseTestFilePath => BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.Module\";

		TransactionHeader[] GetTransactionHeaders()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupSinglePeriod(200911, new ZDateTime(2009, 11, 01), new ZDateTime(2009, 11, 30));

			OrgHeader orgHeader = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "AALSHI");
			AssertNotNull("OrgHeader", orgHeader);

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ZDateTime date = new ZDateTime(2009, 11, 4);

			RefCurrency aUD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Australia);

			AccBankAccount bankAccount = BankAccount;

			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_StartNo = 1;
			chequeBook.AK_LastNo = 9999;
			chequeBook.AK_CurrentNo = 1;

			//1 - Unmatched Payment

			APPayment unmatchedPayment = Factory.New<APPayment>();
			unmatchedPayment.AH_InvoiceDate = date;
			unmatchedPayment.AH_PostDate = date;
			unmatchedPayment.AH_OH = orgHeader.PK;
			unmatchedPayment.AH_ReceiptType = ReceiptTypes.Cheque;
			unmatchedPayment.AH_AB = bankAccount.PK;
			unmatchedPayment.ChequeBook = chequeBook.PK;
			unmatchedPayment.AH_ChequeOrReference = "000001";
			unmatchedPayment.AH_RX_NKTransactionCurrency = aUD.RX_Code;
			unmatchedPayment.AH_ExchangeRate = 1m;
			unmatchedPayment.AH_LocalExTaxAmount = 100.00m;
			unmatchedPayment.AH_OSExTaxAmount = 100.00m;

			AssertEquals("unmatched payment should have no errors: " + unmatchedPayment.NotificationsIncludingChildren.ToUniqueMessageListString(), false, unmatchedPayment.HasErrors);

			Factory.Save();

			//2 - Matched Payment & Invoice

			APInvoice invoice1 = Factory.New<APInvoice>();
			invoice1.AH_OH = orgHeader.PK;
			invoice1.AH_PostDate = date;
			invoice1.AH_InvoiceDate = date;
			invoice1.AH_DueDate = date;
			invoice1.AH_TransactionNum = "004";

			APInvoiceLine line1 = (APInvoiceLine)invoice1.Lines.AddNew();

			AccGLHeader gLHeader2 = Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "7100.40.10");
			AssertNotNull("GLHeader2", gLHeader2);

			line1.GenericCharge = gLHeader2.PK;
			line1.AL_LocalExTaxAmount = 200.00m;
			line1.AL_OSExTaxAmount = 200.00m;

			AccTaxRate gSTtaxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);

			line1.AL_AT = gSTtaxRate.PK;
			invoice1.AH_OutstandingAmount = 0m;
			invoice1.AH_FullyPaidDate = date;

			APPayment matchedPayment1 = Factory.New<APPayment>();
			matchedPayment1.AH_PostDate = date;
			matchedPayment1.AH_InvoiceDate = date;
			matchedPayment1.AH_OH = orgHeader.PK;
			matchedPayment1.AH_ReceiptType = ReceiptTypes.Cheque;
			matchedPayment1.AH_AB = bankAccount.PK;
			matchedPayment1.ChequeBook = chequeBook.PK;
			matchedPayment1.AH_ChequeOrReference = "000002";
			matchedPayment1.AH_RX_NKTransactionCurrency = aUD.RX_Code;
			matchedPayment1.AH_ExchangeRate = 1m;
			matchedPayment1.AH_LocalExTaxAmount = 220.00m;
			matchedPayment1.AH_OSExTaxAmount = 220.00m;
			matchedPayment1.AH_OutstandingAmount = 0m;
			matchedPayment1.AH_FullyPaidDate = date;

			AssertEquals("matchedPayment1 should have no errors: " + matchedPayment1.NotificationsIncludingChildren.ToUniqueMessageListString(), false, matchedPayment1.HasErrors);

			TransactionMatchLink matchLink1 = ((IMatching)invoice1).CurrentMatchGroup.AddNew();
			matchLink1.AP_AH = invoice1.PK;
			matchLink1.AP_MatchGroupNum = "98765";
			matchLink1.AP_Amount = invoice1.AH_InvoiceAmount + invoice1.AH_GSTAmount;

			TransactionMatchLink matchLink2 = ((IMatching)invoice1).CurrentMatchGroup.AddNew();
			matchLink2.AP_AH = matchedPayment1.PK;
			matchLink2.AP_MatchGroupNum = matchLink1.AP_MatchGroupNum;
			matchLink2.AP_Amount = matchedPayment1.AH_InvoiceAmount;
			TestObjectCreator.SetupMatchLinkMatchDate(invoice1);

			Factory.Save();

			//3 - Direct Payment

			DirectPayment directPayment = Factory.New<DirectPayment>();
			directPayment.AH_TransactionNum = "00001000";
			directPayment.AH_InvoiceDate = date;
			directPayment.AH_PostDate = date;
			directPayment.AH_AB = bankAccount.PK;
			directPayment.AH_RX_NKTransactionCurrency = aUD.RX_Code;
			directPayment.AH_ExchangeRate = 1m;
			directPayment.AH_ReceiptType = ReceiptTypes.Cheque;
			directPayment.ChequeBookPK = chequeBook.PK;
			directPayment.AH_ChequeOrReference = "000003";
			directPayment.AH_ChequeDrawer = "JOHN SMITH";
			directPayment.AH_DrawerBank = "88884321";
			directPayment.AH_DrawerBranch = "4321765";

			AccGLHeader gLHeader3 = Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "3510.00.00");
			AssertNotNull("GLHeader3", gLHeader3);

			DirectPaymentLine directPaymentLine1 = (DirectPaymentLine)directPayment.Lines.AddNew();
			directPaymentLine1.AL_AG = gLHeader2.PK;
			directPaymentLine1.AL_OSExTaxAmount = 1000.00m;
			directPaymentLine1.AL_LocalWHTAmount = 0.00m;
			directPaymentLine1.AL_AT = gSTtaxRate.PK;

			AccTaxRate fREEGST = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "FREEGST", AccTaxRate.Types.Rated, 0);

			DirectPaymentLine directPaymentLine2 = (DirectPaymentLine)directPayment.Lines.AddNew();
			directPaymentLine2.AL_AG = gLHeader3.PK;
			directPaymentLine2.AL_OSExTaxAmount = 10000.00m;
			directPaymentLine2.AL_LocalWHTAmount = 0.00m;
			directPaymentLine2.AL_AT = fREEGST.PK;

			Factory.Save();

			return new TransactionHeader[] { unmatchedPayment, matchedPayment1, directPayment };
		}

		AccBankAccount bankAccount;
		AccBankAccount BankAccount
		{
			get
			{
				if (bankAccount == null)
				{
					AccGLHeader gLHeader = Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "6110.10.10");
					AssertNotNull("GLHeader", gLHeader);

					bankAccount = Factory.New<AccBankAccount>();
					bankAccount.AB_Code = "AAA";
					bankAccount.AB_Desc = "AAA BANK ACCOUNT";
					bankAccount.AB_AG = gLHeader.PK;
					bankAccount.AB_BankName = "AAA BANK";
					bankAccount.AB_BankAddress = "123 SOME STREET, SYDNEY, NSW, 2000";
					bankAccount.AB_BankAccountName = "EAGLE DATAMATION INTERNATIONAL";
					bankAccount.AB_BSB = "12345678";
					bankAccount.AB_AccountNum = "87654321";
					bankAccount.AB_BankAbbreviation = "AAA";
					bankAccount.AB_RX_NKAccountCurrency = "AUD";
					bankAccount.AB_AllowAutoDDR = true;
					bankAccount.AB_AutoDDRFormat = "BTM";
					bankAccount.AB_DetailedDepositSlip = true;
					bankAccount.AB_IsDefaultReceiptBankAccount = false;
				}
				return bankAccount;
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 07, 14)]
		public void TestCustomizePositivePayExport()
		{
			TestCustomizePositivePayExportCore(Core.Constants.CountryCodes.UnitedStates, "PositivePay", false);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 07, 14)]
		public void TestCustomizeCheckPaymentsExport()
		{
			TestCustomizePositivePayExportCore(Core.Constants.CountryCodes.Australia, "CheckPayment", false);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 07, 14)]
		public void TestExportPositivePayEndToEnd()
		{
			TestExportTransactionsEndToEnd(Core.Constants.CountryCodes.UnitedStates, "PositivePay");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 07, 14)]
		public void TestExportCheckPaymentsEndToEnd()
		{
			TestExportTransactionsEndToEnd(Core.Constants.CountryCodes.Australia, "CheckPayment");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 07, 14)]
		public void TestExportPositivePayEndToEndCsv()
		{
			TestExportTransactionsEndToEnd(Core.Constants.CountryCodes.UnitedStates, "PositivePay", false);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 07, 14)]
		public void TestExportCheckPaymentsEndToEndCsv()
		{
			TestExportTransactionsEndToEnd(Core.Constants.CountryCodes.Australia, "CheckPayment", false);
		}

		void PrepareExportSetting(string key, string outputFilePath, bool isFixedWidth)
		{
			string settingsFilePath = BaseTestFilePath + @"CashBook\DataExport\Settings\PositivePayExportSettingsWithNoFileExpression.xml";
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(File.ReadAllText(settingsFilePath));
			XmlNode node = xmlDocument.SelectSingleNode("/DataExportWizardSettings/FileNameExpression");
			node.InnerText = "\"" + outputFilePath.Replace("\\", "\\\\") + "\"";

			StringWriter stringWriter = new StringWriter();
			XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
			xmlDocument.WriteTo(xmlTextWriter);
			xmlTextWriter.Flush();
			xmlTextWriter.Close();
			string xml = stringWriter.ToString();

			AssertContains("<FixedWidth>True</FixedWidth>", xml);
			if (!isFixedWidth)
			{
				xml = xml.Replace("<FixedWidth>True</FixedWidth>", "<FixedWidth>False</FixedWidth>");
			}

			string contextPrefix = "DEW:";
			string contextKey = key;
			StmModuleFilterSettingsStorage storage = new StmModuleFilterSettingsStorage(contextPrefix, contextKey);
			storage.SaveSettings("TestingPositivePay", xml);

			StmData data = Factory.New<StmData>();
			data.SD_Name = "PositivePayDataExportSetting";
			data.SD_Owner = BankAccount.PK;
			ZBlob blob = ZBlob.FromAscii("TestingPositivePay");
			data.SD_BinaryValue = blob;
			Factory.Save();
		}

		void AssertExportFileEncoding(string outputFilePath)
		{
			using (var stream = new FileStream(outputFilePath, FileMode.Open, FileAccess.Read))
			{
				var reader = new BinaryReader(stream, Encoding.Default);
				var buffer = reader.ReadBytes(2);
				// Here we only consider UTF8 and ANSI.
				// Please add new byte comparison here if need to introduce other encoding standards.
				var result = (buffer[0] == 0XEF && buffer[1] == 0XBB) ? Encoding.UTF8 : Encoding.Default;

				AssertEquals(Encoding.Default, result);
			}
		}

		[TestDate(2018, 07, 14)]
		public void TestExportTransactionsEndToEnd(string countryCode, string key, bool isFixedWidth = true)
		{
			var originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			string outputFilePath = Env.TempPath + "\\" + "PositivePayExport.csv";
			try
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);

				PrepareExportSetting(key, outputFilePath, isFixedWidth);
				GetTransactionHeaders();

				using (CashBookTransactionModule testModule = new CashBookTransactionModule())
				{
					using (ZForm form = new ZForm())
					{
						form.Controls.Add(testModule.EmbeddedControl);
						form.Show();

						testModule.PerformSearch_ForTest();

						BusinessObjectCollection testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
						testCollection.Load();
						AssertEquals(3, testCollection.Count);

						testModule.DisplayGrid.SelectAllElements();

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						testModule.HandleExportPositivePay_ForTestOnly(null, new EventArgs());
						AssertEquals(string.Format("File successfully created at {0}", outputFilePath), UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}

				Assert(File.Exists(outputFilePath));

				if (!isFixedWidth)
				{
					AssertExportFileEncoding(outputFilePath);
				}
			}
			finally
			{
				if (File.Exists(outputFilePath))
				{
					File.Delete(outputFilePath);
				}

				GlbCompany.CurrentCompany.SetCountry(originalCountry);
			}
		}

		[TestDate(2018, 07, 14)]
		public void TestCustomizePositivePayExportCore(string countryCode, string key, bool isFixedWidth = true)
		{
			var originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			string outputFilePath = Env.TempPath + "\\" + "PositivePayExport.csv";
			try
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);

				PrepareExportSetting(key, outputFilePath, isFixedWidth);
				GetTransactionHeaders();

				using (CashBookTransactionModule testModule = new CashBookTransactionModule())
				{
					using (ZForm form = new ZForm())
					{
						form.Controls.Add(testModule.EmbeddedControl);
						form.Show();

						testModule.PerformSearch_ForTest();

						BusinessObjectCollection testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
						testCollection.Load();

						testModule.DisplayGrid.Select(0);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						testModule.IsMockTestCustomizePositivePayExport = true;
						testModule.HandleCustomizePositivePayExport_ForTestOnly(null, new EventArgs());

						AssertEquals(Encoding.ASCII, testModule.CustomizePositivePayExportWizardEncodingForTest);
					}
				}

				AssertEquals(false, File.Exists(outputFilePath));
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountry);
			}
		}

		public void TestExportPositivePayErrorMessage_NothingSelected()
		{
			AssertExportPositivePayErrorMessage("Select at least one transaction.");
		}

		public void TestExportPositivePayErrorMessage_AlreadyExported()
		{
			TestExportTransactionsErrorMessage_AlreadyExported(Core.Constants.CountryCodes.UnitedStates, "Positive Pay");
		}

		public void TestExportCheckPaymentsErrorMessage_AlreadyExported()
		{
			TestExportTransactionsErrorMessage_AlreadyExported(Core.Constants.CountryCodes.Australia, "Check Payment");
		}

		void TestExportTransactionsErrorMessage_AlreadyExported(string countryCode, string exportTransaction)
		{
			var originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);

				ZString errorMessage = "The following transaction(s) have already been exported for " + exportTransaction + ":\r\n\r\n";
				int count = 1;

				for (int i = 0; i <= 10; i++)
				{
					APPayment payment = Factory.NewWithValidTestData<APPayment>();
					payment.AH_ReceiptType = ReceiptTypes.Cheque;
					payment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
					payment.AH_TransactionNum = "0000100" + i.ToString();
					payment.AH_ExchangeRate = 1m;

					GenExportBatchSequence batchSequence = Factory.New<GenExportBatchSequence>();
					batchSequence.XB_BatchNumber = 1;
					batchSequence.XB_ParentID = payment.PK;
					batchSequence.XB_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
					batchSequence.XB_Sequence = i + 1;
					batchSequence.XB_Type = Core.Constants.DataExportBatchSubTypes.Codes.PositivePayFile;

					if (count <= 10)
					{
						errorMessage += payment.AH_TransactionType + " " + payment.AH_TransactionNum + "\r\n";
					}
					else
					{
						errorMessage += "\r\n" + "Too many transactions to display.  Listing only the first ten transactions.  There are more transactions that have already been exported." + "\r\n";
						break;
					}
					count++;
				}

				errorMessage += "\r\n" + "Please only select transaction(s) to be exported that have not yet been exported.";

				Factory.Save();

				AssertExportPositivePayErrorMessage(errorMessage);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountry);
			}
		}

		public void TestExportPositivePayErrorMessage_SameBankAccount()
		{
			APPayment payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_ReceiptType = ReceiptTypes.Cheque;
			payment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			payment.AH_ExchangeRate = 1m;

			APPayment payment2 = Factory.NewWithValidTestData<APPayment>();
			payment2.AH_ReceiptType = ReceiptTypes.Cheque;
			payment2.AH_AB = TestObjectCreator.USDBankAccount.PK;
			payment2.AH_ExchangeRate = 1m;

			Factory.Save();

			AssertExportPositivePayErrorMessage("All selected transactions must have the same Bank Account.");
		}

		public void TestExportPositivePayErrorMessage_TransactionTypePayment()
		{
			APReceipt receipt = Factory.NewWithValidTestData<APReceipt>();
			receipt.AH_ReceiptType = ReceiptTypes.Cheque;
			receipt.AH_AB = TestObjectCreator.AUDBankAccount.PK;

			Factory.Save();

			AssertExportPositivePayErrorMessage("All selected transactions must be Payments or Direct Payments.");
		}

		public void TestExportPositivePayErrorMessage_ReceiptTypeCheque()
		{
			APPayment payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_ReceiptType = ReceiptTypes.Cash;
			payment.AH_AB = TestObjectCreator.AUDBankAccount.PK;

			Factory.Save();

			AssertExportPositivePayErrorMessage(string.Format("All selected transactions must have a Receipt / Payment Method of Check ({0}).", ReceiptTypes.Cheque));
		}

		void AssertExportPositivePayErrorMessage(ZString errorMessage)
		{
			using (CashBookTransactionModule testModule = new CashBookTransactionModule())
			{
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					testModule.PerformSearch_ForTest();

					BusinessObjectCollection testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();

					testModule.DisplayGrid.SelectAllElements();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandleExportPositivePay_ForTestOnly(null, new EventArgs());
					AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestPrintSecurityReceipt()
		{
			bool oldValue = Env.Security.PrintCashBookReceipt.IsAllowed;

			try
			{
				Env.Security.PrintCashBookReceipt.IsAllowed = false;

				using (CashBookTransactionModule testModule = new CashBookTransactionModule())
				{
					using (ZForm form = new ZForm())
					{
						ARReceipt testBizO = Factory.NewWithValidTestData<ARReceipt>();
						Factory.Save();

						form.Controls.Add(testModule.EmbeddedControl);
						form.Show();

						CashBookFilterBusinessObject cashBookFilterBO = (CashBookFilterBusinessObject)testModule.FilterBusinessObject;
						((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).Property = "";
						((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).IsActive = true;

						testModule.PerformSearch_ForTest();

						BusinessObjectCollection testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
						testCollection.Load();
						AssertEquals(1, testCollection.Count);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						testModule.HandlePrint_ForTestOnly(null, new EventArgs());
						AssertEquals(Env.Security.PrintCashBookReceipt.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
			finally
			{
				Env.Security.PrintCashBookReceipt.IsAllowed = oldValue;
			}
		}

		public void TestPrintSecurityPayment()
		{
			bool oldValue = Env.Security.PrintCashBookPayment.IsAllowed;

			try
			{
				Env.Security.PrintCashBookPayment.IsAllowed = false;

				using (CashBookTransactionModule testModule = new CashBookTransactionModule())
				{
					using (ZForm form = new ZForm())
					{
						APPayment testBizO = Factory.NewWithValidTestData<APPayment>();
						Factory.Save();

						form.Controls.Add(testModule.EmbeddedControl);
						form.Show();

						CashBookFilterBusinessObject cashBookFilterBO = (CashBookFilterBusinessObject)testModule.FilterBusinessObject;
						((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).Property = "";
						((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).IsActive = true;

						testModule.PerformSearch_ForTest();

						BusinessObjectCollection testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
						testCollection.Load();
						AssertEquals(1, testCollection.Count);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						testModule.HandlePrint_ForTestOnly(null, new EventArgs());
						AssertEquals(Env.Security.PrintCashBookPayment.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
			finally
			{
				Env.Security.PrintCashBookPayment.IsAllowed = oldValue;
			}
		}

		public void TestPrintSecurityDirectReceipt()
		{
			bool oldValue = Env.Security.PrintCashBookDirectReceipt.IsAllowed;

			try
			{
				Env.Security.PrintCashBookDirectReceipt.IsAllowed = false;

				using (CashBookTransactionModule testModule = new CashBookTransactionModule())
				{
					using (ZForm form = new ZForm())
					{
						DirectReceipt testBizO = Factory.NewWithValidTestData<DirectReceipt>();
						Factory.Save();

						form.Controls.Add(testModule.EmbeddedControl);
						form.Show();

						CashBookFilterBusinessObject cashBookFilterBO = (CashBookFilterBusinessObject)testModule.FilterBusinessObject;
						((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).Property = "";
						((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).IsActive = true;

						testModule.PerformSearch_ForTest();

						BusinessObjectCollection testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
						testCollection.Load();
						AssertEquals(1, testCollection.Count);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						testModule.HandlePrint_ForTestOnly(null, new EventArgs());
						AssertEquals(Env.Security.PrintCashBookDirectReceipt.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
			finally
			{
				Env.Security.PrintCashBookDirectReceipt.IsAllowed = oldValue;
			}
		}

		public void TestPrintSecurityDirectPayment()
		{
			bool oldValue = Env.Security.PrintCashBookDirectPayment.IsAllowed;

			try
			{
				Env.Security.PrintCashBookDirectPayment.IsAllowed = false;

				using (CashBookTransactionModule testModule = new CashBookTransactionModule())
				{
					using (ZForm form = new ZForm())
					{
						DirectPayment testBizO = Factory.NewWithValidTestData<DirectPayment>();
						Factory.Save();

						form.Controls.Add(testModule.EmbeddedControl);
						form.Show();

						CashBookFilterBusinessObject cashBookFilterBO = (CashBookFilterBusinessObject)testModule.FilterBusinessObject;
						((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).Property = "";
						((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).IsActive = true;

						testModule.PerformSearch_ForTest();

						BusinessObjectCollection testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
						testCollection.Load();
						AssertEquals(1, testCollection.Count);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						testModule.HandlePrint_ForTestOnly(null, new EventArgs());
						AssertEquals(Env.Security.PrintCashBookDirectPayment.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
			finally
			{
				Env.Security.PrintCashBookDirectPayment.IsAllowed = oldValue;
			}
		}

		public void TestPrintSecurityTransfer()
		{
			bool oldValue = Env.Security.PrintCashBookBankTransfer.IsAllowed;

			try
			{
				Env.Security.PrintCashBookBankTransfer.IsAllowed = false;

				using (CashBookTransactionModule testModule = new CashBookTransactionModule())
				{
					using (ZForm form = new ZForm())
					{
						var transfer = new BankTransfer(Factory, null);
						transfer.BankTransferFromPK = TestObjectCreator.AUDBankAccount.PK;
						transfer.BankTransferToPK = TestObjectCreator.USDBankAccount.PK;
						transfer.BuyExchangeRate = 2M;
						transfer.SellAmount = 100m;
						Factory.Save();

						form.Controls.Add(testModule.EmbeddedControl);
						form.Show();

						CashBookFilterBusinessObject cashBookFilterBO = (CashBookFilterBusinessObject)testModule.FilterBusinessObject;
						((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).Property = "";
						((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).IsActive = true;

						testModule.PerformSearch_ForTest();

						BusinessObjectCollection testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
						testCollection.Load();
						AssertEquals(2, testCollection.Count);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						testModule.HandlePrint_ForTestOnly(null, new EventArgs());
						AssertEquals(Env.Security.PrintCashBookBankTransfer.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
			finally
			{
				Env.Security.PrintCashBookBankTransfer.IsAllowed = oldValue;
			}
		}

		public void TestPrintSecurityOpeningReceipt()
		{
			bool oldValue = Env.Security.PrintCashBookOpeningReceipt.IsAllowed;

			try
			{
				Env.Security.PrintCashBookOpeningReceipt.IsAllowed = false;

				using (CashBookTransactionModule testModule = new CashBookTransactionModule())
				{
					using (ZForm form = new ZForm())
					{
						OpeningReceipt testBizO = Factory.NewWithValidTestData<OpeningReceipt>();
						Factory.Save();

						form.Controls.Add(testModule.EmbeddedControl);
						form.Show();

						CashBookFilterBusinessObject cashBookFilterBO = (CashBookFilterBusinessObject)testModule.FilterBusinessObject;
						((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).Property = "";
						((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).IsActive = true;

						testModule.PerformSearch_ForTest();

						BusinessObjectCollection testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
						testCollection.Load();
						AssertEquals(1, testCollection.Count);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						testModule.HandlePrint_ForTestOnly(null, new EventArgs());
						AssertEquals(Env.Security.PrintCashBookOpeningReceipt.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
			finally
			{
				Env.Security.PrintCashBookOpeningReceipt.IsAllowed = oldValue;
			}
		}

		public void TestPrintSecurityOpeningPayment()
		{
			bool oldValue = Env.Security.PrintCashBookOpeningPayment.IsAllowed;

			try
			{
				Env.Security.PrintCashBookOpeningPayment.IsAllowed = false;

				using (CashBookTransactionModule testModule = new CashBookTransactionModule())
				{
					using (ZForm form = new ZForm())
					{
						OpeningPayment testBizO = Factory.NewWithValidTestData<OpeningPayment>();
						Factory.Save();

						form.Controls.Add(testModule.EmbeddedControl);
						form.Show();

						CashBookFilterBusinessObject cashBookFilterBO = (CashBookFilterBusinessObject)testModule.FilterBusinessObject;
						((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).Property = "";
						((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).IsActive = true;

						testModule.PerformSearch_ForTest();

						BusinessObjectCollection testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
						testCollection.Load();
						AssertEquals(1, testCollection.Count);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						testModule.HandlePrint_ForTestOnly(null, new EventArgs());
						AssertEquals(Env.Security.PrintCashBookOpeningPayment.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
			finally
			{
				Env.Security.PrintCashBookOpeningPayment.IsAllowed = oldValue;
			}
		}

		public void TestPrintSecurityExchangeDifference()
		{
			bool oldValue = Env.Security.PrintCashBookBankCurrencyAdjustment.IsAllowed;

			try
			{
				Env.Security.PrintCashBookBankCurrencyAdjustment.IsAllowed = false;

				using (CashBookTransactionModule testModule = new CashBookTransactionModule())
				{
					using (ZForm form = new ZForm())
					{
						CashbookExchangeDiff testBizO = Factory.NewWithValidTestData<CashbookExchangeDiff>();
						Factory.Save();

						form.Controls.Add(testModule.EmbeddedControl);
						form.Show();

						CashBookFilterBusinessObject cashBookFilterBO = (CashBookFilterBusinessObject)testModule.FilterBusinessObject;
						((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).Property = "";
						((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).IsActive = true;

						testModule.PerformSearch_ForTest();

						BusinessObjectCollection testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
						testCollection.Load();
						AssertEquals(1, testCollection.Count);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						testModule.HandlePrint_ForTestOnly(null, new EventArgs());
						AssertEquals(Env.Security.PrintCashBookBankCurrencyAdjustment.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
			finally
			{
				Env.Security.PrintCashBookBankCurrencyAdjustment.IsAllowed = oldValue;
			}
		}

		public void TestAuditTransaction()
		{
			const string creator = "JYW";
			string auditor = GlbStaff.CurrentUser.GS_Code;

			var cashbook = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			cashbook.AH_SystemCreateUser = creator;
			var cashbook1 = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			cashbook1.AH_SystemCreateUser = creator;

			Factory.Save();

			AssertNotEquals("Precondition: The two users should not be identical.", creator, auditor);
			AssertEquals("Precondition: Auditer should be empty.", ZString.Empty, cashbook.AH_GS_NKAuditedBy);
			AssertEquals("Precondition: Auditer should be empty.", ZString.Empty, cashbook1.AH_GS_NKAuditedBy);

			using (CashBookTransactionModule testModule = new CashBookTransactionModule())
			{
				using (ZForm form = new ZForm())
				{
					var menu = testModule.GetNewActionMenuItems_ForTestOnly();
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					testModule.PerformSearch_ForTest();

					var testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals("Precondition: There should be 2 records.", 2, testCollection.Count);
					AssertEquals("Precondition: Allow to audit.", true, testModule.AuditSecurityCheckpoint_ForTestOnly.IsAllowed);

					testModule.DisplayGrid.SelectAllElements();
					UnitTestUserNotification.Instance.ClearMessages();
					menu.FindByText(AccountingConstants.AuditAndCashActionText.AuditTransactionText).PerformClick();

					testCollection.Load();
					foreach (var cb in testCollection)
					{
						AssertEquals(auditor, ((CashbookExchangeDiff)cb).AH_GS_NKAuditedBy);
					}
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
			}
		}

		public void TestUndoAuditTransaction()
		{
			const string creator = "JYW";
			string auditor = GlbStaff.CurrentUser.GS_Code;

			var cashbook = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			cashbook.AH_SystemCreateUser = creator;
			cashbook.AH_GS_NKAuditedBy = auditor;
			var cashbook1 = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			cashbook1.AH_SystemCreateUser = creator;
			cashbook1.AH_GS_NKAuditedBy = auditor;

			Factory.Save();

			AssertNotEquals("Precondition: The two users should not be identical.", creator, auditor);
			AssertEquals("Precondition: Auditer should be auditor.", auditor, cashbook.AH_GS_NKAuditedBy);
			AssertEquals("Precondition: Auditer should be auditor.", auditor, cashbook1.AH_GS_NKAuditedBy);

			using (var testModule = new CashBookTransactionModule())
			{
				using (var form = new ZForm())
				{
					var menu = testModule.GetNewActionMenuItems_ForTestOnly();
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					testModule.PerformSearch_ForTest();

					var testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals("Precondition: There should be 2 records.", 2, testCollection.Count);
					AssertEquals("Precondition: Allow to undo audit.", true, testModule.UndoAuditSecurityCheckpoint_ForTestOnly.IsAllowed);

					testModule.DisplayGrid.SelectAllElements();
					UnitTestUserNotification.Instance.ClearMessages();
					menu.FindByText(AccountingConstants.AuditAndCashActionText.UndoAuditTransactionText).PerformClick();

					testCollection.Load();
					foreach (var cb in testCollection)
					{
						AssertEquals(ZString.Empty, ((CashbookExchangeDiff)cb).AH_GS_NKAuditedBy);
					}
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
			}
		}

		public void TestRecordCashier()
		{
			const string creator = "JYW";
			string cashier = GlbStaff.CurrentUser.GS_Code;

			var arReceipt = Factory.NewWithValidTestData<ARReceipt>();
			arReceipt.AH_SystemCreateUser = creator;
			var arPayment = Factory.NewWithValidTestData<ARPayment>();
			arPayment.AH_SystemCreateUser = creator;
			var apReceipt = Factory.NewWithValidTestData<APReceipt>();
			apReceipt.AH_SystemCreateUser = creator;
			var apPayment = Factory.NewWithValidTestData<APPayment>();
			apPayment.AH_SystemCreateUser = creator;
			var directReceipt = Factory.NewWithValidTestData<DirectReceipt>();
			directReceipt.AH_SystemCreateUser = creator;
			var directPayment = Factory.NewWithValidTestData<DirectPayment>();
			directPayment.AH_SystemCreateUser = creator;
			var transfer = new BankTransfer(Factory, null)
			{
				TransferRowFrom = { AH_SystemCreateUser = creator },
				TransferRowTo = { AH_SystemCreateUser = creator }
			};

			Factory.Save();

			AssertNotEquals("Precondition: The two users should not be identical.", creator, cashier);
			AssertEquals("Precondition: Auditer should be empty.", ZString.Empty, arReceipt.AH_GS_NKCashier);
			AssertEquals("Precondition: Auditer should be empty.", ZString.Empty, arPayment.AH_GS_NKCashier);
			AssertEquals("Precondition: Auditer should be empty.", ZString.Empty, apReceipt.AH_GS_NKCashier);
			AssertEquals("Precondition: Auditer should be empty.", ZString.Empty, apPayment.AH_GS_NKCashier);
			AssertEquals("Precondition: Auditer should be empty.", ZString.Empty, directReceipt.AH_GS_NKCashier);
			AssertEquals("Precondition: Auditer should be empty.", ZString.Empty, directPayment.AH_GS_NKCashier);
			AssertEquals("Precondition: Auditer should be empty.", ZString.Empty, transfer.TransferRowFrom.AH_GS_NKCashier);
			AssertEquals("Precondition: Auditer should be empty.", ZString.Empty, transfer.TransferRowTo.AH_GS_NKCashier);

			using (CashBookTransactionModule testModule = new CashBookTransactionModule())
			{
				using (ZForm form = new ZForm())
				{
					var menu = testModule.GetNewActionMenuItems_ForTestOnly();
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					testModule.PerformSearch_ForTest();

					var testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals("Precondition: There should be 8 records.", 8, testCollection.Count);
					AssertEquals("Precondition: Allow to record cashier.", true, testModule.RecordCashierSecurityCheckpoint_ForTestOnly.IsAllowed);

					testModule.DisplayGrid.SelectAllElements();
					UnitTestUserNotification.Instance.ClearMessages();
					menu.FindByText(AccountingConstants.AuditAndCashActionText.RecordCashierText).PerformClick();

					testCollection.Load();
					foreach (var transaction in testCollection)
					{
						AssertEquals(cashier, ((AccTransactionHeader)transaction).AH_GS_NKCashier);
					}
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
			}
		}

		public void TestClearCashier()
		{
			const string creator = "JYW";
			string cashier = GlbStaff.CurrentUser.GS_Code;

			var cashbook = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			cashbook.AH_SystemCreateUser = creator;
			cashbook.AH_GS_NKCashier = cashier;
			var cashbook1 = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			cashbook1.AH_SystemCreateUser = creator;
			cashbook1.AH_GS_NKCashier = cashier;

			Factory.Save();

			AssertNotEquals("Precondition: The two users should not be identical.", creator, cashier);
			AssertEquals("Precondition: Cashier should be cashier.", cashier, cashbook.AH_GS_NKCashier);
			AssertEquals("Precondition: Cashier should be cashier.", cashier, cashbook1.AH_GS_NKCashier);

			using (var testModule = new CashBookTransactionModule())
			{
				using (var form = new ZForm())
				{
					var menu = testModule.GetNewActionMenuItems_ForTestOnly();
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					testModule.PerformSearch_ForTest();

					var testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals("Precondition: There should be 2 records.", 2, testCollection.Count);
					AssertEquals("Precondition: Allow to clean cashier.", true, testModule.ClearCashierSecurityCheckpoint_ForTestOnly.IsAllowed);

					testModule.DisplayGrid.SelectAllElements();
					UnitTestUserNotification.Instance.ClearMessages();
					menu.FindByText(AccountingConstants.AuditAndCashActionText.ClearCashierText).PerformClick();

					testCollection.Load();
					foreach (var cb in testCollection)
					{
						AssertEquals(ZString.Empty, ((CashbookExchangeDiff)cb).AH_GS_NKCashier);
					}
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
			}
		}

		public void TestGetNewController()
		{
			using (CashBookTransactionModule module = (CashBookTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				ZController defaultController = module.GetNewController_ForTestOnly(null);
				AssertEquals("The default controller should be controller for direct payment", ControllerIDs.DirectPayment, defaultController.ID);

				OpeningPayment opy = Factory.New<OpeningPayment>();
				ZController opyController = module.GetNewController_ForTestOnly(opy);
				AssertEquals("Should be controller for opening payment", ControllerIDs.OpeningPayment, opyController.ID);

				OpeningReceipt orc = Factory.New<OpeningReceipt>();
				ZController orcController = module.GetNewController_ForTestOnly(orc);
				AssertEquals("Should be controller for opening Receipt", ControllerIDs.OpeningReceipt, orcController.ID);

				DirectPayment dpy = Factory.New<DirectPayment>();
				ZController dpyController = module.GetNewController_ForTestOnly(dpy);
				AssertEquals("Should be controller for direct payment", ControllerIDs.DirectPayment, dpyController.ID);

				DirectReceipt drc = Factory.New<DirectReceipt>();
				ZController drcController = module.GetNewController_ForTestOnly(drc);
				AssertEquals("Should be controller for direct receipt", ControllerIDs.DirectReceipt, drcController.ID);

				CashbookExchangeDiff exx = Factory.New<CashbookExchangeDiff>();
				ZController exxController = module.GetNewController_ForTestOnly(exx);
				AssertEquals("Should be controller for bank currency adjustment", ControllerIDs.BankCurrencyAdjustment, exxController.ID);

				BankTransferFromRow trf = Factory.New<BankTransferFromRow>();
				ZController trfController = module.GetNewController_ForTestOnly(trf);
				AssertEquals("Should be controller for bank transfer", ControllerIDs.BankTransfer, trfController.ID);
			}
		}

		public void TestHandlePrintOpeningReceipt()
		{
			OpeningReceipt testOpeningReceipt = Factory.NewWithValidTestData<OpeningReceipt>();
			Factory.Save();

			using (CashBookTransactionModule module = (CashBookTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					CashBookFilterBusinessObject cashBookFilterBO = (CashBookFilterBusinessObject)module.FilterBusinessObject;
					((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).Property = "";
					((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).IsActive = true;

					module.PerformSearch_ForTest();

					BusinessObjectCollection testCollection = (BusinessObjectCollection)module.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(1, testCollection.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					module.HandlePrint_ForTestOnly(this, new EventArgs());
					AssertEquals(CashBookTransactionModule.CannotPrintOpeningReceipt_ForTestOnly, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestHandlePrintOpeningPayment()
		{
			OpeningPayment testOpeningReceipt = Factory.NewWithValidTestData<OpeningPayment>();
			Factory.Save();

			using (CashBookTransactionModule module = (CashBookTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					CashBookFilterBusinessObject cashBookFilterBO = (CashBookFilterBusinessObject)module.FilterBusinessObject;
					((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).Property = "";
					((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).IsActive = true;

					module.PerformSearch_ForTest();

					BusinessObjectCollection testCollection = (BusinessObjectCollection)module.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(1, testCollection.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					module.HandlePrint_ForTestOnly(this, new EventArgs());
					AssertEquals(CashBookTransactionModule.CannotPrintOpeningPayment_ForTestOnly, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestHandlePrintExchangeDiff()
		{
			CashbookExchangeDiff testExchangeDiff = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			Factory.Save();

			using (CashBookTransactionModule module = (CashBookTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					CashBookFilterBusinessObject cashBookFilterBO = (CashBookFilterBusinessObject)module.FilterBusinessObject;
					((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).Property = "";
					((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).IsActive = true;

					module.PerformSearch_ForTest();

					BusinessObjectCollection testCollection = (BusinessObjectCollection)module.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(1, testCollection.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					module.HandlePrint_ForTestOnly(this, new EventArgs());
					AssertEquals(CashBookTransactionModule.CannotPrintExchangeDifference_ForTestOnly, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestPrintAccountingJournalForCB()
		{
			using (CashBookTransactionModule testModule = new CashBookTransactionModule())
			{
				using (ZForm form = new ZForm())
				{
					var cashBookFilterBO = (CashBookFilterBusinessObject)testModule.FilterBusinessObject;
					((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).Property = "";
					((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).IsActive = true;

					testModule.PerformSearch_ForTest();

					var testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(0, testCollection.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandlePrintAccountingJournal_ForTestOnly(null, new EventArgs());
					AssertEquals("Please select transaction(s) to print", UnitTestUserNotification.Instance.LastMessage.Text);

					DirectReceipt testBizO = Factory.NewWithValidTestData<DirectReceipt>();
					Factory.Save();

					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					testModule.PerformSearch_ForTest();

					testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(1, testCollection.Count);

					testModule.DisplayGrid.SelectAllElements();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandlePrintAccountingJournal_ForTestOnly(null, new EventArgs());
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestAllowEdit()
		{
			using (CashBookTransactionModule module = (CashBookTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				Assert(module.AllowEdit);
			}
		}

		public void TestDeleteMenuItemText()
		{
			using (CashBookTransactionModule module = new CashBookTransactionModule())
			{
				AssertEquals("&Reverse", module.GetDeleteMenuItemText_ForTestOnly().Caption);
				AssertEquals("Creates a new reversed item(s) to offset the currently selected item(s) (shortcut Del)", module.GetDeleteMenuItemText_ForTestOnly().FullDescription);
			}
		}

		public void TestUniversalCopyIsntAccessibleInMenuForCB()
		{
			using (var testModule = new CashBookTransactionModule())
			{
				using (var form1 = new ZForm())
				{
					form1.Controls.Add(testModule.EmbeddedControl);
					form1.Show();
					testModule.PerformSearch_ForTest();
					AssertNull(testModule.DisplayGrid.ContextMenu.MenuItems.FindByName("UniversalCopy"));
				}
			}
		}

		public void TestCopyOpeningReceipt()
		{
			OpeningReceipt testOpeningReceipt = Factory.NewWithValidTestData<OpeningReceipt>();
			Factory.Save();

			using (CashBookTransactionModule module = (CashBookTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					CashBookFilterBusinessObject cashBookFilterBO = (CashBookFilterBusinessObject)module.FilterBusinessObject;
					((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).Property = "";
					((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).IsActive = true;

					module.PerformSearch_ForTest();

					BusinessObjectCollection testCollection = (BusinessObjectCollection)module.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(1, testCollection.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					module.HandleTemplateCopyClick_ForTestOnly(this, new EventArgs());
					AssertEquals(CashBookTransactionModule.CannotCopyTransaction_ForTestOnly, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestCopyOpeningPayment()
		{
			OpeningPayment testOpeningPayment = Factory.NewWithValidTestData<OpeningPayment>();
			Factory.Save();

			using (CashBookTransactionModule module = (CashBookTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					CashBookFilterBusinessObject cashBookFilterBO = (CashBookFilterBusinessObject)module.FilterBusinessObject;
					((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).Property = "";
					((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).IsActive = true;

					module.PerformSearch_ForTest();

					BusinessObjectCollection testCollection = (BusinessObjectCollection)module.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(1, testCollection.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					module.HandleTemplateCopyClick_ForTestOnly(this, new EventArgs());
					AssertEquals(CashBookTransactionModule.CannotCopyTransaction_ForTestOnly, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestCopyBankCurrencyAdjustment()
		{
			CashbookExchangeDiff testExchangeDiff = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			Factory.Save();

			using (CashBookTransactionModule module = (CashBookTransactionModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					CashBookFilterBusinessObject cashBookFilterBO = (CashBookFilterBusinessObject)module.FilterBusinessObject;
					((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).Property = "";
					((ModuleTextFilter)cashBookFilterBO["Transaction Type"]).IsActive = true;

					module.PerformSearch_ForTest();

					BusinessObjectCollection testCollection = (BusinessObjectCollection)module.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(1, testCollection.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					module.HandleTemplateCopyClick_ForTestOnly(this, new EventArgs());
					AssertEquals(CashBookTransactionModule.CannotCopyTransaction_ForTestOnly, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}
	}
}
