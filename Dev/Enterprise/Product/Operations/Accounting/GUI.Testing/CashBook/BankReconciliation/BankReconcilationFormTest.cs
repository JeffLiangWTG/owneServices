using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.AccStatement;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.GUI.BankStatement;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using AccStatement = Enterprise.Accounting.Business.Base.AccStatement;

namespace Enterprise.Accounting.GUI.CashBook.BankReconciliation.Testing
{
	[TestedType(typeof(BankReconcilationForm))]
	public class BankReconcilationFormTest : ZFormBasherTest
	{
		public void TestCheckReferenceNumberTextFilterMaxLength()
		{
			Business.CashBook.BankReconciliation bankRecon = new Business.CashBook.BankReconciliation(Factory);
			using (BankReconcilationForm form = new BankReconcilationForm(bankRecon))
			{
				form.Show();
				AssertEquals("Check Reference Number text filter max length", Math.Max(AccTransactionHeaderSchema.AH_ChequeOrReference.MaxLength, AccTransactionHeaderSchema.AH_ReceiptBatchNo.MaxLength), form.TextSearchFilterTextBox_ForTestOnly.MaxLength);
			}
		}

		public void TestCase_BankReconciliationDoesNotGivePageValidationError_WhileSaving()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var bankAccount = creator.AUDBankAccount;

			SetStatement("DR", "CHQ", 130, "TEST1", bankAccount, 1, true);
			SetStatement("DR", "CHQ", 140, "TEST1", bankAccount, 2, false);
			SetStatement("CR", "CHQ", 150, "TEST2", bankAccount, 3, false);

			Factory.Save();

			Business.CashBook.BankReconciliation bankRecon = new Business.CashBook.BankReconciliation(Factory);
			bankRecon.BankAccountPK = bankAccount.PK;
			bankRecon.ReconcileDate = ZDateTime.Now;
			bankRecon.StatementDate = ZDateTime.Now;

			using (BankReconcilationForm form = new BankReconcilationForm(bankRecon))
			{
				form.Show();

				bankRecon.RunPreSaveValidation();

				AssertNoErrors(bankRecon.GetStatements()[0].AS_PageNumberInfo);
				AssertNoErrors(bankRecon.GetStatements()[1].AS_PageNumberInfo);
			}
		}

		public void TestReallocateCheckNumbersMenuItem()
		{
			Business.CashBook.BankReconciliation bankRecon = new Business.CashBook.BankReconciliation(Factory);
			using (BankReconcilationForm form = new BankReconcilationForm(bankRecon))
			{
				MenuItem reAllocate = form.BankReconGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Re-Allocate Check Number");
				AssertNotNull(reAllocate);
			}
		}

		public void TestReallocateCheckNumbers()
		{
			AccStatement.BankStatement bank = Factory.NewWithValidTestData<AccStatement.BankStatement>();

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

			Business.CashBook.BankReconciliation bankRecon = new Business.CashBook.BankReconciliation(Factory);
			using (BankReconcilationForm form = new BankReconcilationForm(bankRecon))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();
				bankRecon.BankAccountPK = bank.PK;
				bankRecon.ReconcileDate = ZDateTime.Now;
				bankRecon.StatementDate = ZDateTime.Now;

				form.HandleReallocateCheckNumber_ForTestOnly(null, null);
				AssertEquals("ExpectedMessage", "Please select transaction(s) to re-allocate", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.BankReconGrid_ForTestOnly.SelectAllElements();
				form.HandleReallocateCheckNumber_ForTestOnly(null, null);
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
				form.FindFilterButton_ForTestOnly.PerformClick();
				form.BankReconGrid_ForTestOnly.SelectAllElements();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.HandleReallocateCheckNumber_ForTestOnly(null, null);
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

		public void TestValidateBeforeLoad()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			AccBankAccount bankAccount = creator.AUDBankAccount;
			ARReceipt undepositedReceipt = creator.CreateARReceipt(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100m, creator.AUDBankAccount.PK);

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Business.CashBook.BankReconciliation bankRec = new Business.CashBook.BankReconciliation(Factory);
			using (BankReconcilationForm form = new BankReconcilationForm(bankRec))
			{
				form.Show();

				bankRec.BankAccountPK = bankAccount.PK;
				AssertNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);

				bankRec.ReconcileDate = ZDateTime.Now;
				AssertNotNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);
				string expectedMessage = "Bank Reconciliation has found following issues.";
				Assert("ExpectedMessage", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				bankRec.StatementDate = ZDateTime.Now;
				AssertNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestBankRecon_BankOrDatesGoingToChange()
		{
			AccStatement.BankStatement bank = Factory.NewWithValidTestData<AccStatement.BankStatement>();
			DirectPayment payment = Factory.NewWithValidTestData<DirectPayment>();
			payment.AH_AB = bank.PK;
			Factory.Save();

			Business.CashBook.BankReconciliation bankRecon = new Business.CashBook.BankReconciliation(Factory);
			using (BankReconcilationForm form = new BankReconcilationForm(bankRecon))
			{
				form.Show();
				AssertNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);

				bankRecon.BankAccountPK = bank.PK;
				AssertNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);

				bankRecon.ReconcileDate = ZDateTime.Now;
				AssertNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);

				bankRecon.StatementDate = ZDateTime.Now;
				AssertNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);

				bankRecon.MergedTransactions[0].IsCleared = true;
				bankRecon.BankAccountPK = ZGuid.Empty;
				AssertNotNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);
				string expectedMessage = "Changing the Bank will reset the current bank reconciliation.\r\nDo you want to proceed with the change?";
				AssertEquals("ExpectedMessage", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				bankRecon.BankAccountPK = bank.PK;
				AssertNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);

				bankRecon.ReconcileDate = ZDateTime.Now;
				bankRecon.StatementDate = ZDateTime.Now;

				bankRecon.MergedTransactions[0].IsCleared = true;
				bankRecon.ReconcileDate = ZDateTime.Empty;
				AssertNotNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);
				expectedMessage = "Changing the Reconcile Date will reset the current bank reconciliation.\r\nDo you want to proceed with the change?";
				AssertEquals("ExpectedMessage", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				bankRecon.ReconcileDate = ZDateTime.Now;
				AssertNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);

				bankRecon.MergedTransactions[0].IsCleared = true;
				bankRecon.StatementDate = ZDateTime.Empty;
				AssertNotNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);
				expectedMessage = "Changing the Statement Date will reset the current bank reconciliation.\r\nDo you want to proceed with the change?";
				AssertEquals("ExpectedMessage", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestClosedBalanceRefreshBinding()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			AccBankAccount bankAccount = creator.AUDBankAccount;

			Factory.Save();

			Business.CashBook.BankReconciliation bankRec = new Business.CashBook.BankReconciliation(Factory);
			using (BankReconcilationForm form = new BankReconcilationForm(bankRec))
			{
				form.Show();

				bankRec.BankAccountPK = bankAccount.PK;
				AssertEquals("ClosingBalance precondition", 0M, ZDecimal.Parse(form.ClosingBalanceCalcEdit_ForTestOnly.Text));

				ZDecimal expectedValue = 777M;
				SetTextBoxValueWithRebinding(form, form.ClosingBalanceCalcEdit_ForTestOnly, expectedValue.ToString());
				AssertEquals("Refreshed ClosingBalance", expectedValue, ZDecimal.Parse(form.ClosingBalanceCalcEdit_ForTestOnly.Text));

				expectedValue = 111M;
				SetTextBoxValueWithRebinding(form, form.ClosingBalanceCalcEdit_ForTestOnly, expectedValue.ToString());
				AssertEquals("Refreshed ClosingBalance", expectedValue, ZDecimal.Parse(form.ClosingBalanceCalcEdit_ForTestOnly.Text));

				expectedValue = 0M;
				SetTextBoxValueWithRebinding(form, form.ClosingBalanceCalcEdit_ForTestOnly, expectedValue.ToString());
				AssertEquals("Refreshed ClosingBalance", expectedValue, ZDecimal.Parse(form.ClosingBalanceCalcEdit_ForTestOnly.Text));
			}
		}

		public void TestEnterBankStatementWorksOnlyIfNoChangesInTransactions()
		{
			AccStatement.BankStatement bank = Factory.NewWithValidTestData<AccStatement.BankStatement>();
			DirectPayment payment = Factory.NewWithValidTestData<DirectPayment>();
			payment.AH_AB = bank.PK;
			Factory.Save();

			Business.CashBook.BankReconciliation bankRecon = new Business.CashBook.BankReconciliation(Factory);
			using (BankReconcilationForm form = new BankReconcilationForm(bankRecon))
			{
				form.Show();

				bankRecon.BankAccountPK = bank.PK;
				bankRecon.ReconcileDate = ZDateTime.Now;
				bankRecon.StatementDate = ZDateTime.Now;
				AssertNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);

				form.BankStatementButton_ForTestOnly.PerformClick();
				AssertNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertType(typeof(BankStatementForm), ZFormModaliser.LastFormShownForTest);
				ZFormModaliser.LastFormShownForTest.Dispose();

				bankRecon.MergedTransactions[0].IsCleared = true;
				form.BankStatementButton_ForTestOnly.PerformClick();
				AssertNotNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);
				string expectedMessage = "Please save your changes before entering bank statement.";
				AssertEquals("ExpectedMessage", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestEnterBankStatement_DoesNotSaveEDocWhenNoChanges()
		{
			// Note: this behaviour exists to ensure all changes to the reconciliation history are tracked.
			// Changing the BankStatementForm to share a BusinessObjectFactory with the BankReconciliation would achieve the same aim.

			var newFactory = new BusinessObjectFactory();
			var bank = newFactory.NewWithValidTestData<AccStatement.BankStatement>();
			newFactory.Save();

			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today);
			Factory.Save();

			var bankRecon = new Business.CashBook.BankReconciliation(newFactory);
			bankRecon.BankAccountPK = bank.PK;
			bankRecon.ReconcileDate = ZDateTime.Now;
			bankRecon.StatementDate = ZDateTime.Now;
			bankRecon.ClosingBalance = 1000m;
			newFactory.Save();

			var docManager = bankRecon.BankAccount.DocManagerInfo();
			AssertEquals("Precondition: no eDocs attached", 0, docManager.Files.Count);
			using (var reconciliationForm = new BankReconcilationFormForTest(bankRecon))
			{
				reconciliationForm.Show();
				reconciliationForm.BankStatementButton_Click_ForTestOnly(reconciliationForm, EventArgs.Empty);
				AssertType(typeof(BankStatementForm), ZFormModaliser.ActiveForm);
				var bankStatementForm = (BankStatementForm)ZFormModaliser.ActiveForm;

				bankStatementForm.Close();

				docManager = bankRecon.BankAccount.DocManagerInfo();
				AssertEquals("No eDoc should be added", 0, docManager.Files.Count);

				reconciliationForm.Close();
			}
		}

		public void TestEnterBankStatement_SavesEDocOfHistoryWhenChanges()
		{
			// Note: this behaviour exists to ensure all changes to the reconciliation history are tracked.
			// Changing the BankStatementForm to share a BusinessObjectFactory with the BankReconciliation would achieve the same aim.

			var newFactory = new BusinessObjectFactory();
			var bank = newFactory.NewWithValidTestData<AccStatement.BankStatement>();
			bank.AB_Desc = "Description";
			bank.AB_BankName = "Bank Of WiseTech";
			bank.AB_BankAddress = "WiseTech Kitchen";
			bank.AB_BankAbbreviation = "WTG";
			newFactory.Save();

			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today);
			Factory.Save();

			var bankRecon = new Business.CashBook.BankReconciliation(newFactory);
			bankRecon.BankAccountPK = bank.PK;
			bankRecon.ReconcileDate = ZDateTime.Now;
			bankRecon.StatementDate = ZDateTime.Now;
			bankRecon.ClosingBalance = 1000m;
			newFactory.Save();

			var docManager = bankRecon.BankAccount.DocManagerInfo();
			AssertEquals("Precondition: no eDocs attached", 0, docManager.Files.Count);
			using (var reconciliationForm = new BankReconcilationFormForTest(bankRecon))
			{
				reconciliationForm.Show();
				reconciliationForm.BankStatementButton_Click_ForTestOnly(reconciliationForm, EventArgs.Empty);
				AssertType(typeof(BankStatementForm), ZFormModaliser.ActiveForm);
				var bankStatementForm = (BankStatementForm)ZFormModaliser.ActiveForm;

				var newStatement = bankStatementForm.BusinessEntity.Factory.New<Statement>();
				newStatement.AS_AB = bank.PK;
				newStatement.AS_DebitCredit = Statement.DEBIT;
				newStatement.AS_Amount = 120m;
				newStatement.AS_ChequeOrReference = "Ref";
				newStatement.AS_Type = "EFT";
				newStatement.AS_PageNumber = 1;
				newStatement.AS_StatementDate = bankRecon.StatementDate;
				newStatement.Factory.Save();

				var result = bankStatementForm.FireSaveButton();
				bankStatementForm.Close();

				docManager = bankRecon.BankAccount.DocManagerInfo();
				AssertEquals("One eDoc should be added", 1, docManager.Files.Count);
				var storageFile = (BusinessObject)docManager.Files[0];
				Assert("eDoc should be saved to database.", storageFile.IsInDatabase);

				reconciliationForm.Close();
			}
		}

		public void TestBankStatementForm_OnClosedHandler_ShouldHandleZSaveExceptionFromSaveInternal()
		{
			var newFactory = new BusinessObjectFactory();
			var bank = newFactory.NewWithValidTestData<AccStatement.BankStatement>();
			bank.AB_Desc = "Description";
			bank.AB_BankName = "Bank Of WiseTech";
			bank.AB_BankAddress = "WiseTech Kitchen";
			bank.AB_BankAbbreviation = "WTG";
			bank.AB_LastReconcileDate = ZDateTime.Now.AddDays(-3);
			newFactory.Save();

			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today);
			Factory.Save();

			var bankRecon = new Business.CashBook.BankReconciliation(newFactory);
			bankRecon.BankAccountPK = bank.PK;
			bankRecon.ReconcileDate = ZDateTime.Now;
			bankRecon.StatementDate = ZDateTime.Now;
			bankRecon.ClosingBalance = 1000m;

			using (var reconciliationForm = new BankReconcilationFormForTest(bankRecon))
			{
				reconciliationForm.Show();
				reconciliationForm.BankStatementButton_Click_ForTestOnly(reconciliationForm, EventArgs.Empty);
				var bankStatementForm = (BankStatementForm)ZFormModaliser.ActiveForm;

				var newFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };
				var bankInOtherFactory = newFactory2.Load<AccStatement.BankStatement>(bank.PK);
				bankInOtherFactory.AB_LastReconcileDate = ZDateTime.Now.AddDays(-1);
				newFactory2.Save();

				AssertNoExceptionThrown(() => bankStatementForm.Close());
				AssertContains("The following objects have critical changes and cannot be merged:", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2021, 7, 7, 9, 0 ,0)]
		public void TestTransactionLineDetails()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			{
				var placeOfSupplyList = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany);

				var bank = Factory.NewWithValidTestData<AccStatement.BankStatement>();
				bank.AB_Desc = "Description";
				Factory.Save();

				var testObjectCreator = new TestObjectCreator(Factory);
				testObjectCreator.GLHeader1.AG_AccountType = AccountTypesList.Codes.BalanceSheet;
				testObjectCreator.GLHeader1.AG_Description = "Description";
				AssertNotNull(testObjectCreator.GSTFREE1);
				AssertNotNull(testObjectCreator.GST1);
				Factory.Save();

				var bankRecon = new Business.CashBook.BankReconciliation(Factory);
				using (BankReconcilationFormForTest reconciliationForm = new BankReconcilationFormForTest(bankRecon))
				{
					reconciliationForm.Show();

					bankRecon.BankAccountPK = bank.PK;
					bankRecon.ReconcileDate = ZDateTime.Now;
					bankRecon.StatementDate = ZDateTime.Now;

					var directReceipt = testObjectCreator.CreateCashBookTransaction<BankReconDirectReceipt>();
					directReceipt.AH_ReceiptType = ReceiptTypes.Cash;
					directReceipt.AH_AB = bank.PK;

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					var initializeData = new EventHandler((sender, e) =>
					{
						var transactionForm = (BankTransactionForm)sender;
						directReceipt.Lines[0].AL_AG = testObjectCreator.GLHeader1.PK;
						directReceipt.Lines[0].AL_Desc = "Desc Test 1";
						directReceipt.Lines[0].AL_OSExTaxAmount = 10M;
						directReceipt.Lines[0].AL_AT = testObjectCreator.GSTFREE1.PK;
						directReceipt.Lines[0].AL_TaxDate = ZDate.Today.AddDays(-1);
						directReceipt.Lines[0].AL_A9_VATClass = testObjectCreator.TaxMsg1.PK;
						directReceipt.Lines[0].AL_GovtChargeCode = "Govt Test 1";
						directReceipt.Lines[0].AL_PlaceOfSupply = placeOfSupplyList[0].Code;
						directReceipt.Lines[0].AL_Calc_InputGSTVATRecoverablePercentage = 40m;
						transactionForm.TransactionsBizO_ForTestOnly.DirectReceipts.Add(directReceipt);
						transactionForm.ApplyButton.PerformClick();
					});
					AssertEquals(0, bankRecon.AdditionalTransactions.Headers.Count);
					reconciliationForm.BankTransactionFormLoad += initializeData;
					reconciliationForm.BankTransactionButton_ForTestOnly.PerformClick();
					AssertEquals(1, bankRecon.AdditionalTransactions.Headers.Count);
					var receipt = bankRecon.AdditionalTransactions.Headers.OfType<DirectTransactionHeaderBase>().Single(x => x.TransactionType_ForTestOnly == TransactionTypes.DirectReceipt);
					AssertEquals(testObjectCreator.GLHeader1.PK, receipt.Lines[0].AL_AG);
					AssertEquals("Desc Test 1", receipt.Lines[0].AL_Desc);
					AssertEquals(10M, receipt.Lines[0].AL_OSExTaxAmount);
					AssertEquals(0M, receipt.Lines[0].AL_OSTaxAmount);
					AssertEquals(testObjectCreator.GSTFREE1.PK, receipt.Lines[0].AL_AT);
					AssertEquals(ZDate.Today.AddDays(-1), receipt.Lines[0].AL_TaxDate);
					AssertEquals(testObjectCreator.TaxMsg1.PK, receipt.Lines[0].AL_A9_VATClass);
					AssertEquals("Govt Test 1", receipt.Lines[0].AL_GovtChargeCode);
					AssertEquals(placeOfSupplyList[0].Code, receipt.Lines[0].AL_PlaceOfSupply);
					AssertEquals(40m, receipt.Lines[0].AL_Calc_InputGSTVATRecoverablePercentage);

					reconciliationForm.BankTransactionFormLoad -= initializeData;
					initializeData = (sender, e) =>
					{
						var transactionForm = (BankTransactionForm)sender;
						directReceipt = transactionForm.TransactionsBizO_ForTestOnly.DirectReceipts[0];
						directReceipt.Lines[0].AL_AG = testObjectCreator.GLHeader2.PK;
						directReceipt.Lines[0].AL_Desc = "Desc Test 2";
						directReceipt.Lines[0].AL_OSExTaxAmount = 20M;
						directReceipt.Lines[0].AL_AT = testObjectCreator.GST1.PK;
						directReceipt.Lines[0].AL_TaxDate = ZDate.Today.AddDays(-2);
						directReceipt.Lines[0].AL_A9_VATClass = testObjectCreator.TaxMsg2.PK;
						directReceipt.Lines[0].AL_GovtChargeCode = "Govt Test 2";
						directReceipt.Lines[0].AL_PlaceOfSupply = placeOfSupplyList[1].Code;
						directReceipt.Lines[0].AL_Calc_InputGSTVATRecoverablePercentage = 60m;
						transactionForm.ApplyButton.PerformClick();
					};
					reconciliationForm.BankTransactionFormLoad += initializeData;
					reconciliationForm.BankTransactionButton_ForTestOnly.PerformClick();
					receipt = bankRecon.AdditionalTransactions.Headers.OfType<DirectTransactionHeaderBase>().Single(x => x.TransactionType_ForTestOnly == TransactionTypes.DirectReceipt);
					AssertEquals(testObjectCreator.GLHeader2.PK, receipt.Lines[0].AL_AG);
					AssertEquals("Desc Test 2", receipt.Lines[0].AL_Desc);
					AssertEquals(20M, receipt.Lines[0].AL_OSExTaxAmount);
					AssertEquals(2M, receipt.Lines[0].AL_OSTaxAmount);
					AssertEquals(testObjectCreator.GST1.PK, receipt.Lines[0].AL_AT);
					AssertEquals(ZDate.Today.AddDays(-2), receipt.Lines[0].AL_TaxDate);
					AssertEquals(testObjectCreator.TaxMsg2.PK, receipt.Lines[0].AL_A9_VATClass);
					AssertEquals("Govt Test 2", receipt.Lines[0].AL_GovtChargeCode);
					AssertEquals(placeOfSupplyList[1].Code, receipt.Lines[0].AL_PlaceOfSupply);
					AssertEquals(60m, receipt.Lines[0].AL_Calc_InputGSTVATRecoverablePercentage);
				}
			}
		}

		[TestDate(2021, 7, 7, 9, 0 ,0)]
		public void TestTransactionHeaderDetails()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			var bank = Factory.NewWithValidTestData<AccStatement.BankStatement>();
			bank.AB_Desc = "Description";
			Factory.Save();

			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today);
			testObjectCreator.GLHeader1.AG_AccountType = AccountTypesList.Codes.BalanceSheet;
			testObjectCreator.GLHeader1.AG_Description = "Description";
			var taxRate = testObjectCreator.GSTFREE1;
			Factory.Save();

			var bankRecon = new Business.CashBook.BankReconciliation(Factory);
			using (AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (BankReconcilationFormForTest reconciliationForm = new BankReconcilationFormForTest(bankRecon))
			{
				reconciliationForm.Show();

				bankRecon.BankAccountPK = bank.PK;
				bankRecon.ReconcileDate = ZDateTime.Now;
				bankRecon.StatementDate = ZDateTime.Now;

				var directReceipt = testObjectCreator.CreateCashBookTransaction<BankReconDirectReceipt>();
				directReceipt.AH_ReceiptType = ReceiptTypes.Cash;
				directReceipt.AH_InvoiceDate = ZDateTime.Today;
				directReceipt.AH_PostDate = ZDateTime.Today;
				directReceipt.AH_AB = bank.PK;
				directReceipt.Lines[0].AL_AG = testObjectCreator.GLHeader1.PK;
				directReceipt.Lines[0].AL_OSExTaxAmount = 10M;
				directReceipt.Lines[0].AL_AT = taxRate.PK;
				directReceipt.Lines[0].AL_TaxRateNumerator = 11;

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var initializeData = new EventHandler((sender, e) =>
				{
					var transactionForm = (BankTransactionForm)sender;
					directReceipt.AH_InvoiceDate = ZDateTime.Today.AddDays(-1);
					directReceipt.AH_PostDate = ZDateTime.Today.AddDays(-1);
					transactionForm.TransactionsBizO_ForTestOnly.DirectReceipts.Add(directReceipt);
					transactionForm.ApplyButton.PerformClick();
				});
				reconciliationForm.BankTransactionFormLoad += initializeData;

				AssertEquals(0, bankRecon.AdditionalTransactions.Headers.Count);
				reconciliationForm.BankTransactionButton_ForTestOnly.PerformClick();
				AssertEquals(1, bankRecon.AdditionalTransactions.Headers.Count);
				var receipt = bankRecon.AdditionalTransactions.Headers.OfType<DirectTransactionHeaderBase>().Single(x => x.TransactionType_ForTestOnly == TransactionTypes.DirectReceipt);
				AssertEquals(ZDateTime.Today.AddDays(-1), receipt.AH_InvoiceDate);
				AssertEquals(ZDateTime.Today.AddDays(-1), receipt.AH_PostDate);
				AssertEquals("CASH", receipt.AH_ChequeOrReference);
				AssertNullOrEmpty(receipt.AH_ChequeDrawer);
				AssertNullOrEmpty(receipt.AH_DrawerBank);
				AssertNullOrEmpty(receipt.AH_DrawerBranch);

				reconciliationForm.BankTransactionFormLoad -= initializeData;
				initializeData = (sender, e) =>
				{
					var transactionForm = (BankTransactionForm)sender;
					directReceipt = transactionForm.TransactionsBizO_ForTestOnly.DirectReceipts[0];
					directReceipt.AH_InvoiceDate = ZDateTime.Today.AddDays(-2);
					directReceipt.AH_PostDate = ZDateTime.Today.AddDays(-2);
					directReceipt.AH_ReceiptType = ReceiptTypes.Cheque;
					directReceipt.AH_ChequeOrReference = "2";
					directReceipt.AH_ChequeDrawer = "ChequeDrawer";
					directReceipt.AH_DrawerBank = "DrawerBank";
					directReceipt.AH_DrawerBranch = "DrawerBranch";
					transactionForm.ApplyButton.PerformClick();
				};
				reconciliationForm.BankTransactionFormLoad += initializeData;
				reconciliationForm.BankTransactionButton_ForTestOnly.PerformClick();
				receipt = bankRecon.AdditionalTransactions.Headers.OfType<DirectTransactionHeaderBase>().Single(x => x.TransactionType_ForTestOnly == TransactionTypes.DirectReceipt);
				AssertEquals(ZDateTime.Today.AddDays(-2), receipt.AH_InvoiceDate);
				AssertEquals(ZDateTime.Today.AddDays(-2), receipt.AH_PostDate);
				AssertEquals(ReceiptTypes.Cheque, receipt.AH_ReceiptType);
				AssertEquals("2", receipt.AH_ChequeOrReference);
				AssertEquals("ChequeDrawer", receipt.AH_ChequeDrawer);
				AssertEquals("DrawerBank", receipt.AH_DrawerBank);
				AssertEquals("DrawerBranch", receipt.AH_DrawerBranch);
			}
		}

		[TestDate(2021, 7, 7, 9, 0 ,0)]
		public void TestTransactionLineSubAccounts()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			var bank = Factory.NewWithValidTestData<AccStatement.BankStatement>();
			bank.AB_Desc = "Description";
			bank.AB_BankName = "Bank Of WiseTech";
			bank.AB_BankAddress = "WiseTech Kitchen";
			bank.AB_BankAbbreviation = "WTG";
			Factory.Save();

			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.GLHeader1.AG_AccountType = AccountTypesList.Codes.BalanceSheet;
			testObjectCreator.GLHeader1.AG_Description = "Description";
			testObjectCreator.CreateGLHeaderSubAccount(testObjectCreator.GLHeader1, OrgHeaderSchema.Constants.Prefix, true);
			testObjectCreator.CreateGLHeaderSubAccount(testObjectCreator.GLHeader1, AccGroupsSchema.Constants.Prefix, true);
			testObjectCreator.CreateGLHeaderSubAccount(testObjectCreator.GLHeader1, GlbStaffSchema.Constants.Prefix, true);
			testObjectCreator.CreateGLHeaderSubAccount(testObjectCreator.GLHeader1, GlbGroupSchema.Constants.Prefix, true);
			var taxRate = testObjectCreator.GSTFREE1;
			AssertNotNull(testObjectCreator.AR1);
			AssertNotNull(testObjectCreator.ABIGAS);
			AssertNotNull(testObjectCreator.GS1);
			AssertNotNull(testObjectCreator.GG1);
			Factory.Save();

			var bankRecon = new Business.CashBook.BankReconciliation(Factory);
			using (BankReconcilationFormForTest reconciliationForm = new BankReconcilationFormForTest(bankRecon))
			{
				reconciliationForm.Show();

				bankRecon.BankAccountPK = bank.PK;
				bankRecon.ReconcileDate = ZDateTime.Now;
				bankRecon.StatementDate = ZDateTime.Now;

				AssertEquals("Precondition: AdditionalTransactions.Headers.Count", 0, bankRecon.AdditionalTransactions.Headers.Count);

				var directPayment = testObjectCreator.CreateCashBookTransaction<BankReconDirectPayment>();
				directPayment.AH_ReceiptType = ReceiptTypes.Cash;
				directPayment.AH_AB = bank.PK;
				var directPaymentLine = directPayment.Lines[0];
				directPaymentLine.AL_AG = testObjectCreator.GLHeader1.PK;
				directPaymentLine.AL_OSExTaxAmount = 10M;
				directPaymentLine.AL_AT = taxRate.PK;
				directPaymentLine.SubAccounts.SubAccountElements.Single(x => x.SubAccountTypeParentTableCode == AccGroupsSchema.Constants.Prefix).SubAccountParentId = testObjectCreator.AR1.PK;
				directPaymentLine.SubAccounts.SubAccountElements.Single(x => x.SubAccountTypeParentTableCode == OrgHeaderSchema.Constants.Prefix).SubAccountParentId = testObjectCreator.ABIGAS.PK;
				directPaymentLine.SubAccounts.SubAccountElements.Single(x => x.SubAccountTypeParentTableCode == GlbStaffSchema.Constants.Prefix).SubAccountParentId = testObjectCreator.GS1.PK;
				directPaymentLine.SubAccounts.SubAccountElements.Single(x => x.SubAccountTypeParentTableCode == GlbGroupSchema.Constants.Prefix).SubAccountParentId = testObjectCreator.GG1.PK;
				var directReceipt = testObjectCreator.CreateCashBookTransaction<BankReconDirectReceipt>();
				directReceipt.AH_ReceiptType = ReceiptTypes.Cash;
				directReceipt.AH_AB = bank.PK;
				var directReceiptLine = directReceipt.Lines[0];
				directReceiptLine.AL_AG = testObjectCreator.GLHeader1.PK;
				directReceiptLine.AL_OSExTaxAmount = 10M;
				directReceiptLine.AL_AT = taxRate.PK;
				directReceiptLine.AL_TaxRateNumerator = 11;
				directReceiptLine.SubAccounts.SubAccountElements.Single(x => x.SubAccountTypeParentTableCode == AccGroupsSchema.Constants.Prefix).SubAccountParentId = testObjectCreator.AR1.PK;
				directReceiptLine.SubAccounts.SubAccountElements.Single(x => x.SubAccountTypeParentTableCode == OrgHeaderSchema.Constants.Prefix).SubAccountParentId = testObjectCreator.AALSHI.PK;
				directReceiptLine.SubAccounts.SubAccountElements.Single(x => x.SubAccountTypeParentTableCode == GlbStaffSchema.Constants.Prefix).SubAccountParentId = testObjectCreator.GS1.PK;
				directReceiptLine.SubAccounts.SubAccountElements.Single(x => x.SubAccountTypeParentTableCode == GlbGroupSchema.Constants.Prefix).SubAccountParentId = testObjectCreator.GG1.PK;

				EventHandler initializeData = (sender, e) =>
				{
					BankTransactionForm transactionForm = (BankTransactionForm)sender;
					transactionForm.TransactionsBizO_ForTestOnly.DirectReceipts.Add(directReceipt);
					transactionForm.TransactionsBizO_ForTestOnly.DirectPayments.Add(directPayment);
					transactionForm.ApplyButton.PerformClick();
				};
				reconciliationForm.BankTransactionFormLoad += initializeData;
				AssertEquals("Before Click BankTransactionButton", 0, bankRecon.AdditionalTransactions.Headers.Count);
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				reconciliationForm.BankTransactionButton_ForTestOnly.PerformClick();
				AssertEquals("After Click BankTransactionButton", 2, bankRecon.AdditionalTransactions.Headers.Count);
				AssertType<BankTransactionForm>(ZFormModaliser.LastFormShownDialogForTest);
				var payment = bankRecon.AdditionalTransactions.Headers.OfType<DirectTransactionHeaderBase>().Single(x => x.TransactionType_ForTestOnly == TransactionTypes.DirectPayment);
				var receipt = bankRecon.AdditionalTransactions.Headers.OfType<DirectTransactionHeaderBase>().Single(x => x.TransactionType_ForTestOnly == TransactionTypes.DirectReceipt);
				AssertEquals("ORG: ABIGAS, SEG: AR1, STR: GS1, SGP: GG1", SubAccountHelper.GetMultiSubAccountTypeCode(payment.Lines[0], Factory));
				AssertEquals("ORG: AALSHI, SEG: AR1, STR: GS1, SGP: GG1", SubAccountHelper.GetMultiSubAccountTypeCode(receipt.Lines[0], Factory));

				reconciliationForm.BankTransactionFormLoad -= initializeData;
				initializeData = (sender, e) =>
				{
					BankTransactionForm transactionForm = (BankTransactionForm)sender;
					AssertEquals("ORG: ABIGAS, SEG: AR1, STR: GS1, SGP: GG1", SubAccountHelper.GetMultiSubAccountTypeCode(transactionForm.TransactionsBizO_ForTestOnly.DirectPayments[0].Lines[0], Factory));
					AssertEquals("ORG: AALSHI, SEG: AR1, STR: GS1, SGP: GG1", SubAccountHelper.GetMultiSubAccountTypeCode(transactionForm.TransactionsBizO_ForTestOnly.DirectReceipts[0].Lines[0], Factory));

					transactionForm.TransactionsBizO_ForTestOnly.DirectPayments[0].Lines[0].SubAccounts.SubAccountElements.Single(x => x.SubAccountTypeParentTableCode == OrgHeaderSchema.Constants.Prefix).SubAccountParentId = testObjectCreator.AALSHI.PK;
					transactionForm.TransactionsBizO_ForTestOnly.DirectReceipts[0].Lines[0].SubAccounts.SubAccountElements.Single(x => x.SubAccountTypeParentTableCode == OrgHeaderSchema.Constants.Prefix).SubAccountParentId = testObjectCreator.ABIGAS.PK;
					transactionForm.ApplyButton.PerformClick();
				};
				reconciliationForm.BankTransactionFormLoad += initializeData;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				reconciliationForm.BankTransactionButton_ForTestOnly.PerformClick();
				AssertEquals("ORG: AALSHI, SEG: AR1, STR: GS1, SGP: GG1", SubAccountHelper.GetMultiSubAccountTypeCode(payment.Lines[0], Factory));
				AssertEquals("ORG: ABIGAS, SEG: AR1, STR: GS1, SGP: GG1", SubAccountHelper.GetMultiSubAccountTypeCode(receipt.Lines[0], Factory));
			}
		}

		[TestDate(2021, 7, 7, 9, 0 ,0)]
		public void TestBankTransactionsChequeDrawerReadOnly()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			var bank = Factory.NewWithValidTestData<AccStatement.BankStatement>();
			bank.AB_Desc = "Description";
			bank.AB_BankName = "Bank Of WiseTech";
			bank.AB_BankAddress = "WiseTech Kitchen";
			bank.AB_BankAbbreviation = "WTG";
			Factory.Save();

			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.GLHeader1.AG_AccountType = AccountTypesList.Codes.BalanceSheet;
			testObjectCreator.GLHeader1.AG_Description = "Description";
			var taxRate = testObjectCreator.GSTFREE1;
			Factory.Save();

			var bankRecon = new Business.CashBook.BankReconciliation(Factory);
			using (BankReconcilationFormForTest reconciliationForm = new BankReconcilationFormForTest(bankRecon))
			{
				reconciliationForm.Show();

				bankRecon.BankAccountPK = bank.PK;
				bankRecon.ReconcileDate = ZDateTime.Now;
				bankRecon.StatementDate = ZDateTime.Now;

				AssertEquals("Precondition: AdditionalTransactions.Headers.Count", 0, bankRecon.AdditionalTransactions.Headers.Count);
				AssertEquals("Precondition: AdditionalTransactions.BankReconTransactions.Count", 0, bankRecon.AdditionalTransactions.BankReconTransactions.Count);
				AssertEquals("Precondition: CombinedTransactions.Count", 0, bankRecon.CombinedTransactions.Count);
				AssertEquals("Precondition: MergedTransactions.Count", 0, bankRecon.MergedTransactions.Count);

				var directPayment = testObjectCreator.CreateCashBookTransaction<BankReconDirectPayment>();
				directPayment.AH_ReceiptType = ReceiptTypes.Cash;
				directPayment.AH_AB = bank.PK;
				directPayment.Lines[0].AL_AG = testObjectCreator.GLHeader1.PK;
				directPayment.Lines[0].AL_OSExTaxAmount = 10M;
				directPayment.Lines[0].AL_AT = taxRate.PK;
				var directReceipt1 = testObjectCreator.CreateCashBookTransaction<BankReconDirectReceipt>();
				directReceipt1.AH_ReceiptType = ReceiptTypes.Cash;
				directReceipt1.AH_AB = bank.PK;
				directReceipt1.Lines[0].AL_AG = testObjectCreator.GLHeader1.PK;
				directReceipt1.Lines[0].AL_OSExTaxAmount = 10M;
				directReceipt1.Lines[0].AL_AT = taxRate.PK;
				directReceipt1.Lines[0].AL_TaxRateNumerator = 11;
				var directReceipt2 = testObjectCreator.CreateCashBookTransaction<BankReconDirectReceipt>();
				directReceipt2.AH_ReceiptType = ReceiptTypes.Cheque;
				directReceipt2.AH_ChequeDrawer = "Test";
				directReceipt2.AH_DrawerBank = "Test";
				directReceipt2.AH_DrawerBranch = "Test";
				directReceipt2.AH_ChequeOrReference = "123";
				directReceipt2.AH_AB = bank.PK;
				directReceipt2.Lines[0].AL_AG = testObjectCreator.GLHeader1.PK;
				directReceipt2.Lines[0].AL_OSExTaxAmount = 20M;
				directReceipt2.Lines[0].AL_AT = taxRate.PK;
				directReceipt2.Lines[0].AL_TaxRateNumerator = 12;

				EventHandler initializeData = (sender, e) =>
				{
					BankTransactionForm transactionForm = (BankTransactionForm)sender;
					transactionForm.TransactionsBizO_ForTestOnly.DirectReceipts.Add(directReceipt1);
					transactionForm.TransactionsBizO_ForTestOnly.DirectReceipts.Add(directReceipt2);
					transactionForm.TransactionsBizO_ForTestOnly.DirectPayments.Add(directPayment);
					transactionForm.ApplyButton.PerformClick();
				};

				reconciliationForm.BankTransactionFormLoad += initializeData;

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				reconciliationForm.BankTransactionButton_ForTestOnly.PerformClick();
				AssertEquals("AdditionalTransactions.Headers.Count", 3, bankRecon.AdditionalTransactions.Headers.Count);
				AssertType("Precondition: BankStatementForm must be shown", typeof(BankTransactionForm), ZFormModaliser.LastFormShownDialogForTest);

				var tempAdditionalTransactions = ZFormModaliser.LastIBusinessShownOnDialogForTest as DirectTransactionsBusinessObject;
				AssertEquals(false, tempAdditionalTransactions.DirectPayments[0].AH_ChequeDrawerInfo.ReadOnly);
				AssertEquals(true, tempAdditionalTransactions.DirectReceipts.OfType<DirectTransactionHeaderBase>().FirstOrDefault(x => x.PK == directReceipt1.PK).AH_ChequeDrawerInfo.ReadOnly);
				AssertEquals(false, tempAdditionalTransactions.DirectReceipts.OfType<DirectTransactionHeaderBase>().FirstOrDefault(x => x.PK == directReceipt2.PK).AH_ChequeDrawerInfo.ReadOnly);
			}
		}

		public void TestBankTransactionButton()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AccStatement.BankStatement bank = newFactory.NewWithValidTestData<AccStatement.BankStatement>();
			bank.AB_Desc = "Description";
			bank.AB_BankName = "Bank Of WiseTech";
			bank.AB_BankAddress = "WiseTech Kitchen";
			bank.AB_BankAbbreviation = "WTG";
			newFactory.Save();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today);
			testObjectCreator.GLHeader1.AG_AccountType = AccountTypesList.Codes.BalanceSheet;
			testObjectCreator.GLHeader1.AG_Description = "Description";
			AccTaxRate taxRate = testObjectCreator.GSTFREE1;
			Factory.Save();

			Business.CashBook.BankReconciliation bankRecon = new Business.CashBook.BankReconciliation(newFactory);
			using (BankReconcilationFormForTest reconciliationForm = new BankReconcilationFormForTest(bankRecon))
			{
				reconciliationForm.Show();

				bankRecon.BankAccountPK = bank.PK;
				bankRecon.ReconcileDate = ZDateTime.Now;
				bankRecon.StatementDate = ZDateTime.Now;

				AssertEquals("Precondition: AdditionalTransactions.Headers.Count", 0, bankRecon.AdditionalTransactions.Headers.Count);
				AssertEquals("Precondition: AdditionalTransactions.BankReconTransactions.Count", 0, bankRecon.AdditionalTransactions.BankReconTransactions.Count);
				AssertEquals("Precondition: CombinedTransactions.Count", 0, bankRecon.CombinedTransactions.Count);
				AssertEquals("Precondition: MergedTransactions.Count", 0, bankRecon.MergedTransactions.Count);

				BankReconDirectPayment directPayment = Factory.NewWithValidTestData<BankReconDirectPayment>();
				directPayment.AH_ReceiptType = ReceiptTypes.Cash;
				directPayment.AH_AB = bank.PK;
				directPayment.Lines.AddNew();
				directPayment.Lines[0].FillWithValidTestData();
				directPayment.Lines[0].AL_AG = testObjectCreator.GLHeader1.PK;
				directPayment.Lines[0].AL_OSExTaxAmount = 10M;
				directPayment.Lines[0].AL_AT = taxRate.PK;
				BankReconDirectReceipt directReceipt = Factory.NewWithValidTestData<BankReconDirectReceipt>();
				directReceipt.AH_ReceiptType = ReceiptTypes.Cash;
				directReceipt.AH_AB = bank.PK;
				directReceipt.Lines.AddNew();
				directReceipt.Lines[0].FillWithValidTestData();
				directReceipt.Lines[0].AL_AG = testObjectCreator.GLHeader1.PK;
				directReceipt.Lines[0].AL_OSExTaxAmount = 20M;
				directReceipt.Lines[0].AL_AT = taxRate.PK;
				directReceipt.Lines[0].AL_TaxRateNumerator = 11;

				EventHandler initializeData = (sender, e) =>
				{
					BankTransactionForm transactionForm = (BankTransactionForm)sender;
					transactionForm.TransactionsBizO_ForTestOnly.DirectPayments.Add(directPayment);
					transactionForm.TransactionsBizO_ForTestOnly.DirectReceipts.Add(directReceipt);
					transactionForm.ApplyButton.PerformClick();
				};

				reconciliationForm.BankTransactionFormLoad += initializeData;

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				reconciliationForm.BankTransactionButton_ForTestOnly.PerformClick();
				AssertType("Precondition: BankStatementForm must be shown", typeof(BankTransactionForm), ZFormModaliser.LastFormShownDialogForTest);

				AssertEquals("AdditionalTransactions.Headers.Count", 2, bankRecon.AdditionalTransactions.Headers.Count);
				AssertEquals("AdditionalTransactions.BankReconTransactions.Count", 2, bankRecon.AdditionalTransactions.BankReconTransactions.Count);
				AssertEquals("CombinedTransactions.Count", 2, bankRecon.CombinedTransactions.Count);
				AssertEquals("MergedTransactions.Count", 2, bankRecon.MergedTransactions.Count);

				AssertEquals("New receipt must have correct factory.", bankRecon.CombinedTransactions.Factory, bankRecon.AdditionalTransactions.Headers[0].Factory);
				AssertEquals("New receipt must have correct factory.", bankRecon.CombinedTransactions.Factory, bankRecon.AdditionalTransactions.BankReconTransactions[0].Factory);
				AssertEquals("directReceipt must be the same in Headers.", directReceipt.PK, bankRecon.AdditionalTransactions.Headers[0].PK);
				AssertEquals("directReceipt must be the same in BankReconTransactions.", directReceipt.RelatedDepositBatch.PK, bankRecon.AdditionalTransactions.BankReconTransactions[0].PK);
				AssertEquals("New BankReconTransaction must call OnLoad to populate Debit.", 22.2M, bankRecon.AdditionalTransactions.BankReconTransactions[0].Debit);
				Assert("AL_OSTaxAmountInfo.ReadOnly", !bankRecon.AdditionalTransactions.Headers[0].Lines[0].AL_OSTaxAmountInfo.ReadOnly);

				AssertEquals("New payment must have correct factory.", bankRecon.CombinedTransactions.Factory, bankRecon.AdditionalTransactions.Headers[1].Factory);
				AssertEquals("New payment must have correct factory.", bankRecon.CombinedTransactions.Factory, bankRecon.AdditionalTransactions.BankReconTransactions[1].Factory);
				AssertEquals("directPayment must be the same in Headers.", directPayment.PK, bankRecon.AdditionalTransactions.Headers[1].PK);
				AssertEquals("directPayment must be the same in BankReconTransactions.", directPayment.PK, bankRecon.AdditionalTransactions.BankReconTransactions[1].PK);
				AssertEquals("New BankReconTransaction must call OnLoad to populate Credit.", 10M, bankRecon.AdditionalTransactions.BankReconTransactions[1].Credit);
				Assert("AL_OSTaxAmountInfo.ReadOnly", bankRecon.AdditionalTransactions.Headers[1].Lines[0].AL_OSTaxAmountInfo.ReadOnly);

				AssertEquals("New payment must be cleared by default.", true, bankRecon.MergedTransactions[0].IsCleared);
				AssertEquals("New receipt must be cleared by default.", true, bankRecon.MergedTransactions[1].IsCleared);

				AssertEquals("Snapshot / change tracking finds the two added transactions", 2, bankRecon.TransactionsAddedInThisSession.Count);

				reconciliationForm.BankTransactionFormLoad -= initializeData;

				initializeData = (sender, e) =>
				{
					BankTransactionForm transactionForm = (BankTransactionForm)sender;
					transactionForm.TransactionsBizO_ForTestOnly.DirectPayments.RemoveAll();
					transactionForm.TransactionsBizO_ForTestOnly.DirectReceipts.RemoveAll();
					transactionForm.ApplyButton.PerformClick();
				};

				BankReconDirectReceipt additionalTransactionsHeaders_0 = (BankReconDirectReceipt)bankRecon.AdditionalTransactions.Headers[0];
				BankReconDirectPayment additionalTransactionsHeaders_1 = (BankReconDirectPayment)bankRecon.AdditionalTransactions.Headers[1];
				BankReconTransaction additionalTransactions_BankReconTransactions_0 = bankRecon.AdditionalTransactions.BankReconTransactions[0];
				BankReconTransaction additionalTransactions_BankReconTransactions_1 = bankRecon.AdditionalTransactions.BankReconTransactions[1];

				reconciliationForm.BankTransactionFormLoad += initializeData;
				reconciliationForm.BankTransactionButton_ForTestOnly.PerformClick();
				AssertType("Precondition: BankStatementForm must be shown", typeof(BankTransactionForm), ZFormModaliser.LastFormShownDialogForTest);

				AssertEquals("AdditionalTransactions.Headers.Count", 0, bankRecon.AdditionalTransactions.Headers.Count);
				AssertEquals("AdditionalTransactions.BankReconTransactions.Count", 0, bankRecon.AdditionalTransactions.BankReconTransactions.Count);
				AssertEquals("CombinedTransactions.Count", 0, bankRecon.CombinedTransactions.Count);
				AssertEquals("MergedTransactions.Count", 0, bankRecon.MergedTransactions.Count);

				AssertEquals("additionalTransactionsHeaders_0 mast be deleted.", true, additionalTransactionsHeaders_0.IsDeleted);
				AssertEquals("additionalTransactionsHeaders_1 mast be deleted.", true, additionalTransactionsHeaders_0.IsDeleted);
				AssertEquals("AdditionalTransactions_BankReconTransactions_0 mast be deleted.", true, additionalTransactions_BankReconTransactions_0.IsDeleted);
				AssertEquals("AdditionalTransactions_BankReconTransactions_1 mast be deleted.", true, additionalTransactions_BankReconTransactions_0.IsDeleted);

				AssertEquals("Snapshot / change tracking finds the two removed transactions", 2, bankRecon.TransactionsRemovedInThisSession.Count);
			}
		}

		public void TestReallocateCheckNumbersShowErrorIfTransactionIsNotInDB()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AccStatement.BankStatement bank = newFactory.NewWithValidTestData<AccStatement.BankStatement>();
			newFactory.Save();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today);
			testObjectCreator.GLHeader1.AG_AccountType = AccountTypesList.Codes.BalanceSheet;
			testObjectCreator.GLHeader1.AG_Description = "Description";
			AccTaxRate taxRate = testObjectCreator.GSTFREE1;
			Factory.Save();

			Business.CashBook.BankReconciliation bankRecon = new Business.CashBook.BankReconciliation(newFactory);
			using (BankReconcilationFormForTest reconciliationForm = new BankReconcilationFormForTest(bankRecon))
			{
				reconciliationForm.Show();

				bankRecon.BankAccountPK = bank.PK;
				bankRecon.ReconcileDate = ZDateTime.Now;
				bankRecon.StatementDate = ZDateTime.Now;

				AssertEquals("Precondition: AdditionalTransactions.Headers.Count", 0, bankRecon.AdditionalTransactions.Headers.Count);
				AssertEquals("Precondition: AdditionalTransactions.BankReconTransactions.Count", 0, bankRecon.AdditionalTransactions.BankReconTransactions.Count);
				AssertEquals("Precondition: CombinedTransactions.Count", 0, bankRecon.CombinedTransactions.Count);
				AssertEquals("Precondition: MergedTransactions.Count", 0, bankRecon.MergedTransactions.Count);

				BankReconDirectPayment directPayment = Factory.NewWithValidTestData<BankReconDirectPayment>();
				directPayment.AH_ReceiptType = ReceiptTypes.Cash;
				directPayment.AH_AB = bank.PK;
				directPayment.Lines.AddNew();
				directPayment.Lines[0].FillWithValidTestData();
				directPayment.Lines[0].AL_AG = testObjectCreator.GLHeader1.PK;
				directPayment.Lines[0].AL_OSExTaxAmount = 10M;
				directPayment.Lines[0].AL_AT = taxRate.PK;
				BankReconDirectReceipt directReceipt = Factory.NewWithValidTestData<BankReconDirectReceipt>();
				directReceipt.AH_ReceiptType = ReceiptTypes.Cash;
				directReceipt.AH_AB = bank.PK;
				directReceipt.Lines.AddNew();
				directReceipt.Lines[0].FillWithValidTestData();
				directReceipt.Lines[0].AL_AG = testObjectCreator.GLHeader1.PK;
				directReceipt.Lines[0].AL_OSExTaxAmount = 20M;
				directReceipt.Lines[0].AL_AT = taxRate.PK;

				EventHandler initializeData = (sender, e) =>
				{
					BankTransactionForm transactionForm = (BankTransactionForm)sender;
					transactionForm.TransactionsBizO_ForTestOnly.DirectPayments.Add(directPayment);
					transactionForm.ApplyButton.PerformClick();
				};

				reconciliationForm.BankTransactionFormLoad += initializeData;

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				reconciliationForm.BankTransactionButton_ForTestOnly.PerformClick();
				AssertType("Precondition: BankStatementForm must be shown", typeof(BankTransactionForm), ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Precondition: MergedTransactions.Count", 1, bankRecon.MergedTransactions.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				reconciliationForm.BankReconGrid_ForTestOnly.SelectAllElements();
				reconciliationForm.HandleReallocateCheckNumber_ForTestOnly(null, null);
				ZString expectedMessage = "The check numbers on transactions you have selected cannot be re-allocated because some transactions are not saved.";
				AssertEquals("ExpectedMessage", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2013, 01, 03, 14, 50, 00)]
		public void TestDoesntSetClearDateOnExistingTxn()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AccStatement.BankStatement bank = newFactory.NewWithValidTestData<AccStatement.BankStatement>();
			newFactory.Save();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today);
			testObjectCreator.GLHeader1.AG_AccountType = AccountTypesList.Codes.BalanceSheet;
			testObjectCreator.GLHeader1.AG_Description = "Description";
			AccTaxRate taxRate = testObjectCreator.GSTFREE1;

			var arReceiptToNotClear = (ARReceipt)testObjectCreator.CreateReceiptOrPayment(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100M, bank.PK);

			Factory.Save();

			Business.CashBook.BankReconciliation bankRecon = new Business.CashBook.BankReconciliation(newFactory);
			using (BankReconcilationFormForTest reconciliationForm = new BankReconcilationFormForTest(bankRecon))
			{
				reconciliationForm.Show();

				bankRecon.BankAccountPK = bank.PK;
				bankRecon.ReconcileDate = ZDateTime.Now;
				bankRecon.StatementDate = ZDateTime.Now;

				BankReconDirectReceipt directReceipt = null;
				BankReconDirectReceipt directReceipt2 = null;

				EventHandler initializeData = (sender, e) =>
				{
					BankTransactionForm transactionForm = (BankTransactionForm)sender;
					var directReceiptsGrid = transactionForm.GetControl<ZGrid>("DirectReceiptsGrid");

					transactionForm.NewDirectReceiptButton_Click_ForTestOnly(null, null);
					transactionForm.NewDirectReceiptButton_Click_ForTestOnly(null, null);

					directReceipt = directReceiptsGrid.List[0] as BankReconDirectReceipt;
					directReceipt.AH_ReceiptType = ReceiptTypes.Cash;
					directReceipt.AH_AB = bank.PK;
					directReceipt.Lines.AddNew();
					directReceipt.Lines[0].FillWithValidTestData();
					directReceipt.Lines[0].AL_AG = testObjectCreator.GLHeader1.PK;
					directReceipt.Lines[0].AL_OSExTaxAmount = 20M;
					directReceipt.Lines[0].AL_AT = taxRate.PK;

					directReceipt2 = directReceiptsGrid.List[1] as BankReconDirectReceipt;
					directReceipt2.AH_ReceiptType = ReceiptTypes.Cash;
					directReceipt2.AH_AB = bank.PK;
					directReceipt2.Lines.AddNew();
					directReceipt2.Lines[0].FillWithValidTestData();
					directReceipt2.Lines[0].AL_AG = testObjectCreator.GLHeader1.PK;
					directReceipt2.Lines[0].AL_OSExTaxAmount = 30M;
					directReceipt2.Lines[0].AL_AT = taxRate.PK;

					transactionForm.ApplyButton.PerformClick();
				};

				reconciliationForm.BankTransactionFormLoad += initializeData;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				reconciliationForm.BankTransactionButton_ForTestOnly.PerformClick();
				reconciliationForm.BankTransactionFormLoad -= initializeData;

				var directReceiptBatchPKs = reconciliationForm.TheBankRecon.AdditionalTransactions.DirectReceiptBatchPKs.ToList();
				AssertEquals("Direct receipt batch PKs were copied correctly form bank txn form to this form.", 2, directReceiptBatchPKs.Count);
				Assert("Direct receipt batch PKs were copied correctly form bank txn form to this form.", directReceiptBatchPKs.Contains(directReceipt.RelatedDepositBatch.PK));
				Assert("Direct receipt batch PKs were copied correctly form bank txn form to this form.", directReceiptBatchPKs.Contains(directReceipt2.RelatedDepositBatch.PK));

				reconciliationForm.FireSaveButton();

				var factory2 = new BusinessObjectFactory();
				var directReceiptReloaded = factory2.Load<BankReconDirectReceipt>(directReceipt.PK);
				var directReceipt2Reloaded = factory2.Load<BankReconDirectReceipt>(directReceipt2.PK);
				var arReceiptToNotClearReloaded = factory2.Load<ARReceipt>(arReceiptToNotClear.PK);
				AssertNotNull("The direct receipts added through the transaction form have a related deposit batch", directReceiptReloaded.RelatedDepositBatch);
				AssertNotNull("The direct receipts added through the transaction form have a related deposit batch", directReceipt2Reloaded.RelatedDepositBatch);
				AssertEquals("The direct receipts added through the transaction form, which by default is ticked, is now cleared (checking batch record).", ZDateTime.Now, directReceiptReloaded.RelatedDepositBatch.AH_DateClearedInCashbook);
				AssertEquals("The direct receipts added through the transaction form, which by default is ticked, is now cleared (checking batch record).", ZDateTime.Now, directReceipt2Reloaded.RelatedDepositBatch.AH_DateClearedInCashbook);
				AssertEquals("The direct receipts added through the transaction form, which by default is ticked, is now cleared (checking receipt record).", ZDateTime.Now, directReceiptReloaded.AH_DateClearedInCashbook);
				AssertEquals("The direct receipts added through the transaction form, which by default is ticked, is now cleared (checking receipt record).", ZDateTime.Now, directReceipt2Reloaded.AH_DateClearedInCashbook);
				AssertEquals("The original receipt is not cleared because the user didn't touch it.", ZDateTime.Empty, arReceiptToNotClearReloaded.AH_DateClearedInCashbook);
			}
		}

		public void TestSaveButton_AttachesEDocToBankAccount()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var bankAccount = creator.AUDBankAccount;
			var unclearedPayment = SetTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Payment, 120.0m, bankAccount, "");
			Factory.Save();

			Business.CashBook.BankReconciliation bankRecon = new Business.CashBook.BankReconciliation(Factory);
			bankRecon.BankAccountPK = bankAccount.PK;
			bankRecon.ReconcileDate = ZDateTime.Now;
			bankRecon.StatementDate = ZDateTime.Now;

			using (BankReconcilationForm form = new BankReconcilationForm(bankRecon))
			{
				form.Show();

				var unclearedTran = bankRecon.MergedTransactions.FindByPK(unclearedPayment.PK) as BankReconTransaction;
				unclearedTran.IsCleared = true;

				form.FireSaveButton();

				var docManagerInfo = bankRecon.BankAccount.DocManagerInfo();
				AssertEquals("One eDoc should be saved", 1, docManagerInfo.Files.Count);
				var storageFile = (BusinessObject)docManagerInfo.Files[0];
				Assert("eDoc should be saved to database.", storageFile.IsInDatabase);
				var docType = (RefDocType)storageFile["DocType"];
				AssertEquals("RefDocType should match ", "BRC", docType.RT_DocType);
				var fileAsAttachment = (Enterprise.Integration.DocumentEngine.IDeliveryEmailAttachment)storageFile;
				AssertGreaterThan($"eDoc should be larger than zero bytes.", fileAsAttachment.FileSizeInBytes, 0);
			}
		}

		public void TestSaveButton_DoesNotAttachEDoc_WhenDisabledViaRegistry()
		{
			using (AccountingConfigurationRegistry.Instance.AutomaticallySaveReconciliationReportWhenSavingBankReconciliation.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				TestObjectCreator creator = new TestObjectCreator(Factory);
				var bankAccount = creator.AUDBankAccount;
				var unclearedPayment = SetTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Payment, 120.0m, bankAccount, "");
				Factory.Save();

				Business.CashBook.BankReconciliation bankRecon = new Business.CashBook.BankReconciliation(Factory);
				bankRecon.BankAccountPK = bankAccount.PK;
				bankRecon.ReconcileDate = ZDateTime.Now;
				bankRecon.StatementDate = ZDateTime.Now;

				using (BankReconcilationForm form = new BankReconcilationForm(bankRecon))
				{
					form.Show();

					var unclearedTran = bankRecon.MergedTransactions.FindByPK(unclearedPayment.PK) as BankReconTransaction;
					unclearedTran.IsCleared = true;

					form.FireSaveButton();

					var docManagerInfo = bankRecon.BankAccount.DocManagerInfo();
					AssertEquals("No eDoc should be saved", 0, docManagerInfo.Files.Count);
				}
			}
		}

		public void TestBankReconciliationGridImportDataIsDisabled()
		{
			var bankRecon = new Business.CashBook.BankReconciliation(Factory);
			using (var form = new BankReconcilationForm(bankRecon))
			{
				var isGridImportDisabled = form.BankReconGrid_ForTestOnly.DisableImportDataMenuItem;
				Assert(isGridImportDisabled);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new BankReconcilationForm(new Business.CashBook.BankReconciliation(Factory));
		}

		void SetTextBoxValueWithRebinding(Form form, TextBox textBox, string value)
		{
			textBox.Focus();
			textBox.Text = value;
			form.SelectNextControl(textBox, true, true, false, true);
		}

		Statement SetStatement(string debitCredit, string type, decimal amount, string reference, AccBankAccount fBankAccount, int pageNumber, bool isCleared)
		{
			Statement testStatment = Factory.New(typeof(Statement)) as Statement;

			testStatment.AS_AB = fBankAccount.PK;
			testStatment.AS_DebitCredit = debitCredit;
			testStatment.AS_Type = type;
			testStatment.AS_Amount = amount;
			testStatment.AS_ChequeOrReference = reference;
			testStatment.AS_StatementDate = Env.Time.CurrentLocalDateTime;
			testStatment.AS_PageNumber = (ZShort)pageNumber;
			testStatment.AS_IsCleared = isCleared;

			return testStatment;
		}

		AccTransactionHeader SetTransaction(string ledger, string transactionType, decimal localAmount, AccBankAccount fBankAccount, string receiptBatchNo)
		{
			AccTransactionHeader transaction = Factory.New(typeof(AccTransactionHeader)) as AccTransactionHeader;

			transaction.AH_Ledger = ledger;
			transaction.AH_TransactionType = transactionType;
			transaction.AH_InvoiceAmount = localAmount;
			transaction.AH_OutstandingAmount = localAmount;
			transaction.AH_OSTotal = localAmount;

			//Mandatory values
			transaction.AH_InvoiceDate = Env.Time.CurrentLocalDateTime;
			transaction.AH_PostDate = Env.Time.CurrentLocalDateTime;
			transaction.AH_GC = GlbCompany.CurrentCompany.PK;
			transaction.AH_GB = GlbBranch.CurrentBranch.PK;
			transaction.AH_GE = GlbDepartment.CurrentDepartment.PK;

			//Others
			transaction.AH_TransactionNum = ledger + transactionType + (nextTransactionNumber++).ToString(Culture.Invariant);
			transaction.AH_AB = fBankAccount.PK;
			transaction.AH_ReceiptBatchNo = receiptBatchNo;

			return transaction;
		}
		int nextTransactionNumber = 1000;

		class BankReconcilationFormForTest : BankReconcilationForm
		{
			public BankReconcilationFormForTest(Business.CashBook.BankReconciliation bankRecon)
				: base(bankRecon)
			{
			}

			internal override BankTransactionFormHelper GetBankTransactionFormHelper(AccBankAccount bankAccount, ZDateTime statementDate, DirectTransactionsBusinessObject transactions, bool isReadOnly)
			{
				BankTransactionFormHelper helper = base.GetBankTransactionFormHelper(bankAccount, statementDate, transactions, isReadOnly);
				helper.BankTransactionFormLoad += new EventHandler(helper_BankTransactionFormLoad);
				return helper;
			}

			void helper_BankTransactionFormLoad(object sender, EventArgs e)
			{
				if (BankTransactionFormLoad != null)
				{
					BankTransactionFormLoad(sender, e);
				}
			}

			public Business.CashBook.BankReconciliation TheBankRecon
			{
				get
				{
					return BankRecon;
				}
			}

			internal event EventHandler BankTransactionFormLoad;
		}

		#endregion
	}
}
