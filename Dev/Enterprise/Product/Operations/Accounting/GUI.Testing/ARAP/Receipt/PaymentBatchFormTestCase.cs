using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	[TestedType(typeof(ReceiptBatchForm))]
	public class PaymentBatchFormTestCase : AccountingZFormBasherTest
	{
		public void TestSuccessfulPostWithMatching()
		{
			using (ReceiptBatchForm testBatchForm = new ReceiptBatchForm(BatchPoster))
			{
				testBatchForm.OpenedMatchFormsForTest = 0;
				testBatchForm.AutoCloseMatchingForms = ZBool.True;
				PrepareForSuccessfulPost();

				testBatchForm.Show();
				BatchPoster.MatchAfterPosting = ZBool.True;
				TestReceipt1.AH_OSExTaxAmount = 150;
				TestReceipt2.AH_OSExTaxAmount = 100;
				TestReceipt3.AH_OSExTaxAmount = 0.05;
				BatchPoster.RunPreSaveValidation();
				Assert("Batch Poster should not have errors any more", !BatchPoster.HasErrors);

				Assert("Batch poster should be editable", !BatchPoster.ReadOnly);
				testBatchForm.ValidateAndSave_ForTestOnly();
				Assert("Batch poster should not be set to readonly", !BatchPoster.ReadOnly);
				Assert("TestReceipt1 should have been saved", TestReceipt1.IsInDatabase);
				Assert("TestReceipt3 should have been saved", TestReceipt3.IsInDatabase);
				Assert("TestReceipt2 should have been saved", TestReceipt2.IsInDatabase);
				Assert("ReceiptBatchForm should now be closed", !testBatchForm.Visible);
				AssertEquals("There should be 3 matching forms opened during posting", 3, testBatchForm.OpenedMatchFormsForTest);
				Assert("The main form should have been opened during matching", testBatchForm.MainFormWasVisibleDuringMatching);
			}
		}

		public void TestTryMatchNonexistentReceipts()
		{
			using (ReceiptBatchForm testBatchForm = new ReceiptBatchForm(BatchPoster))
			{
				testBatchForm.OpenedMatchFormsForTest = 0;
				testBatchForm.AutoCloseMatchingForms = ZBool.True;
				PrepareForSuccessfulPost();

				testBatchForm.Show();
				BatchPoster.MatchAfterPosting = ZBool.True;
				TestReceipt1.AH_OSExTaxAmount = 150;
				BatchPoster.RunPreSaveValidation();
				testBatchForm.MatchReceipts_ForTestOnly();
				AssertEquals("The receipt you are trying to match does not exist.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPrintingMessageIsShownOnlyOnceIfFactorySavedTwice()
		{
			using (ReceiptBatchForm testBatchForm = new ReceiptBatchForm(BatchPoster))
			{
				PrepareForSuccessfulPost();
				testBatchForm.Show();
				BatchPoster.CreateDepositSlip = ZBool.True;
				TestReceipt1.AH_OSExTaxAmount = 150;
				TestReceipt2.AH_OSExTaxAmount = 100;
				TestReceipt3.AH_OSExTaxAmount = 0.05;
				BatchPoster.RunPreSaveValidation();
				Assert("Batch Poster should have no errors", !BatchPoster.HasErrors);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				Assert("Deposit slip should not be printed yet", !testBatchForm.DepositSlipPrintedForTest);
				AssertNull("Message should not be shown yet", UnitTestUserNotification.Instance.LastMessage.Text);
				Factory.Save();
				Assert("TestReceipt1 should have been saved", TestReceipt1.IsInDatabase);
				AssertEquals("Deposit slip printing question should be asked", "Do you want to print the Deposit Slip now?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Deposit slip should not be printed", !testBatchForm.DepositSlipPrintedForTest);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last shown message now is empty", UnitTestUserNotification.Instance.LastMessage.Text);
				Factory.Save();
				AssertNull("Last shown message should stay empty", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPrintDepositSlip_MessageNotShownIfFlagNotSet()
		{
			using (ReceiptBatchForm testBatchForm = new ReceiptBatchForm(BatchPoster))
			{
				PrepareForSuccessfulPost();
				testBatchForm.Show();
				BatchPoster.CreateDepositSlip = ZBool.False;
				TestReceipt1.AH_OSExTaxAmount = 150;
				TestReceipt2.AH_OSExTaxAmount = 100;
				TestReceipt3.AH_OSExTaxAmount = 0.05;
				BatchPoster.RunPreSaveValidation();
				Assert("Batch Poster should have no errors", !BatchPoster.HasErrors);
				Factory.Save();
				Assert("TestReceipt1 should have been saved", TestReceipt1.IsInDatabase);
				AssertNull("Deposit slip printing question should not be asked", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Deposit slip should not be printed", !testBatchForm.DepositSlipPrintedForTest);
			}
		}

		[ExpectNoExceptions()]
		public void TestPrintDepositSlip_AnsweringYES()
		{
			using (ReceiptBatchForm testBatchForm = new ReceiptBatchForm(BatchPoster))
			{
				PrepareForSuccessfulPost();
				testBatchForm.Show();
				BatchPoster.CreateDepositSlip = ZBool.True;
				TestReceipt1.AH_OSExTaxAmount = 150;
				TestReceipt2.AH_OSExTaxAmount = 100;
				TestReceipt3.AH_OSExTaxAmount = 0.05;
				BatchPoster.RunPreSaveValidation();
				Assert("Batch Poster should have no errors", !BatchPoster.HasErrors);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Assert("Deposit slip should not be printed yet", !testBatchForm.DepositSlipPrintedForTest);
				AssertNull("Message should not be shown yet", UnitTestUserNotification.Instance.LastMessage.Text);
				Factory.Save();
				Assert("TestReceipt1 should have been saved", TestReceipt1.IsInDatabase);
				AssertEquals("Deposit slip printing question should be asked", "Do you want to print the Deposit Slip now?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Deposit slip should be printed", testBatchForm.DepositSlipPrintedForTest);
			}
		}

		public void TestPrintDepositSlip_AnsweringNO()
		{
			using (ReceiptBatchForm testBatchForm = new ReceiptBatchForm(BatchPoster))
			{
				PrepareForSuccessfulPost();
				testBatchForm.Show();
				BatchPoster.CreateDepositSlip = ZBool.True;
				TestReceipt1.AH_OSExTaxAmount = 150;
				TestReceipt2.AH_OSExTaxAmount = 100;
				TestReceipt3.AH_OSExTaxAmount = 0.05;
				BatchPoster.RunPreSaveValidation();
				Assert("Batch Poster should have no errors", !BatchPoster.HasErrors);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				Assert("Deposit slip should not be printed yet", !testBatchForm.DepositSlipPrintedForTest);
				AssertNull("Message should not be shown yet", UnitTestUserNotification.Instance.LastMessage.Text);
				Factory.Save();
				Assert("TestReceipt1 should have been saved", TestReceipt1.IsInDatabase);
				AssertEquals("Deposit slip printing question should be asked", "Do you want to print the Deposit Slip now?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Deposit slip should not be printed", !testBatchForm.DepositSlipPrintedForTest);
			}
		}

		public void TestFormWillBeClosedAfterPostingWithoutMatching()
		{
			using (ReceiptBatchForm testBatchForm = new ReceiptBatchForm(BatchPoster))
			{
				testBatchForm.OpenedMatchFormsForTest = 0;
				PrepareForSuccessfulPost();

				testBatchForm.Show();
				BatchPoster.MatchAfterPosting = ZBool.False;
				TestReceipt1.AH_OSExTaxAmount = 150;
				TestReceipt2.AH_OSExTaxAmount = 100;
				TestReceipt3.AH_OSExTaxAmount = 0.05;
				BatchPoster.RunPreSaveValidation();
				Assert("Batch Poster should not have errors any more", !BatchPoster.HasErrors);

				testBatchForm.ValidateAndSave_ForTestOnly();
				Assert("TestReceipt1 should have been saved", TestReceipt1.IsInDatabase);
				AssertEquals("Should not be any matching forms opened", 0, testBatchForm.OpenedMatchFormsForTest);
				Assert("ReceiptBatchForm should be closed", !testBatchForm.Visible);
			}
		}

		public void TestWillNotSaveIfPostingObjectHasErrors()
		{
			using (ReceiptBatchForm testBatchForm = new ReceiptBatchForm(BatchPoster))
			{
				PrepareForSuccessfulPost();

				testBatchForm.Show();
				TestReceipt1.AH_OSExTaxAmount = 150;
				TestReceipt2.AH_OSExTaxAmount = 100;
				BatchPoster.RunPreSaveValidation();
				Assert("Batch Poster should have an error as amount is not set on one of the objects", BatchPoster.HasErrors);
				testBatchForm.ValidateAndSave_ForTestOnly();
				Assert("TestReceipt1 should not be saved", !TestReceipt1.IsInDatabase);

				TestReceipt3.AH_OSExTaxAmount = 0.05;
				BatchPoster.RunPreSaveValidation();
				Assert("Batch Poster should not have errors any more", !BatchPoster.HasErrors);

				testBatchForm.ValidateAndSave_ForTestOnly();
				Assert("TestReceipt1 should have been saved", TestReceipt1.IsInDatabase);
			}
		}

		public void TestTryingToSaveEmptyCollectionWillBringTheMessage()
		{
			BatchPoster = new ARReceiptBatchPoster(Factory);
			using (ReceiptBatchForm testBatchForm = new ReceiptBatchForm(BatchPoster))
			{
				PrepareForSuccessfulPost();
				testBatchForm.Show();
				BatchPoster.ReceiptBatch.RemoveAndDeleteAll();
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				testBatchForm.ValidateAndSave_ForTestOnly();
				AssertEquals("Should bring the error", "There is nothing to post. Fill in the receipts first.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("TestReceipt1 should not be saved", !TestReceipt1.IsInDatabase);
			}
		}

		public void TestRemoveReceiptFromBatch()
		{
			using (ReceiptBatchForm testBatchForm = new ReceiptBatchForm(BatchPoster))
			{
				PrepareForSuccessfulPost();
				testBatchForm.Show();
				AssertEquals("There should be 3 Receipts in the BatchPoster class", 3, BatchPoster.ReceiptBatch.Count);
				testBatchForm.ReceiptBatchGrid_ForTestOnly.Select(0);
				testBatchForm.DeleteMenuItem_ForTestOnly.PerformClick();
				AssertEquals("There should be 2 Receipts in the BatchPoster class", 2, BatchPoster.ReceiptBatch.Count);
			}
		}

		public void TestOnAskBeforeUpdateDescription()
		{
			using (ReceiptBatchForm testBatchForm = new ReceiptBatchForm(BatchPoster))
			{
				PrepareForSuccessfulPost();
				testBatchForm.Show();

				TestReceipt1.AH_Desc = "Something Different";
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				BatchPoster.Description = "2";
				AssertEquals("Message should be shown as there is more than one receipt in collection and one of them has different value", "Are you sure you want to set this Description for all receipts?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Description should be updated as the answer was YES", "2", BatchPoster.Description);
				AssertEquals("Description should be updated as the answer was YES", "2", TestReceipt1.AH_Desc);
				AssertEquals("Description should be updated as the answer was YES", "2", TestReceipt2.AH_Desc);
			}
		}

		public void TestOnAskBeforeUpdateReceiptType()
		{
			using (ReceiptBatchForm testBatchForm = new ReceiptBatchForm(BatchPoster))
			{
				PrepareForSuccessfulPost();
				testBatchForm.Show();

				TestReceipt1.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebitLine;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				BatchPoster.ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
				AssertEquals("Message should be shown as there is more than one receipt in collection and one of them has different value", "Are you sure you want to set this Receipt Type for all receipts?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("ReceiptType should be updated as the answer was YES", ZArchitecture.Core.ReceiptTypes.Cash, BatchPoster.ReceiptType);
				AssertEquals("ReceiptType should be updated as the answer was YES", ZArchitecture.Core.ReceiptTypes.Cash, TestReceipt1.AH_ReceiptType);
				AssertEquals("ReceiptType should be updated as the answer was YES", ZArchitecture.Core.ReceiptTypes.Cash, TestReceipt2.AH_ReceiptType);
			}
		}

		#region Implementation

		ARReceiptBatchPoster BatchPoster;
		OrgHeader TestOrg;
		OrgHeader TestOrg2;
		OrgHeader TestOrg3;
		ARReceipt TestReceipt1;
		ARReceipt TestReceipt2;
		ARReceipt TestReceipt3;
		AccBankAccount TestBank;
		AccountingPeriodTestHelper PeriodHelper;

		protected override void SetUp()
		{
			base.SetUp();

			BatchPoster = new ARReceiptBatchPoster(Factory);
			TestOrg = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg.OH_IsDebtor = true;
			TestOrg.CompanyData.OB_ARPreviousChequeDrawer = "Cheque Drawer";
			TestOrg.CompanyData.OB_ARPreviousChequeDrawerBank = "Drawer Bank";
			TestOrg.CompanyData.OB_ARPreviousChequeDrawerBankBranch = "Drawer Branch";
			TestBank = Factory.NewWithValidTestData<AccBankAccount>();
		}

		void PrepareForSuccessfulPost()
		{
			PeriodHelper = new AccountingPeriodTestHelper(Factory);
			PeriodHelper.SetupPeriods();
			TestOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg2.OH_IsDebtor = true;
			TestOrg2.ARSettlementGroupPK = TestOrg.PK;
			TestOrg2.CompanyData.OB_ARPreviousChequeDrawer = "Cheque Drawer";
			TestOrg2.CompanyData.OB_ARPreviousChequeDrawerBank = "Drawer Bank";
			TestOrg2.CompanyData.OB_ARPreviousChequeDrawerBankBranch = "Drawer Branch";
			TestOrg3 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg3.OH_IsDebtor = true;
			TestOrg3.CompanyData.OB_ARPreviousChequeDrawer = "Cheque Drawer";
			TestOrg3.CompanyData.OB_ARPreviousChequeDrawerBank = "Drawer Bank";
			TestOrg3.CompanyData.OB_ARPreviousChequeDrawerBankBranch = "Drawer Branch";

			AccBankAccount testBank2 = Factory.NewWithValidTestData<AccBankAccount>();
			testBank2.AB_IsDefaultReceiptBankAccount = ZBool.True;
			testBank2.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			TestOrg2.CompanyData.OB_RX_NKARDDefltCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			Factory.Save();

			TestReceipt1 = BatchPoster.ReceiptBatch.AddNew();
			TestReceipt1.AH_OH = TestOrg.PK;

			TestReceipt2 = BatchPoster.ReceiptBatch.AddNew();
			TestReceipt2.AH_OH = TestOrg2.PK;

			TestReceipt3 = BatchPoster.ReceiptBatch.AddNew();
			TestReceipt3.AH_OH = TestOrg3.PK;

			BatchPoster.BankAccountPK = TestBank.PK;
			TestBank.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			BatchPoster.RX_NK = Core.Constants.CurrencyCodes.UnitedStates;
			BatchPoster.ExchangeRate.Rate = 2.35M;
			BatchPoster.CreateDepositSlip = ZBool.True;
			//BatchPoster.MatchAfterPosting = ZBool.True;
			TestReceipt1.AH_ChequeOrReference = "1";
			TestReceipt2.AH_ChequeOrReference = "2";
			TestReceipt3.AH_ChequeOrReference = "3";
		}

		protected override Form GetFormToBashCore()
		{
			ARReceiptBatchPoster batchPoster = new ARReceiptBatchPoster(Factory);
			ReceiptBatchForm form = new ReceiptBatchForm(batchPoster);
			return form;
		}

		#endregion
	}
}
