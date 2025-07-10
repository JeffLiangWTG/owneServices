using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Unmatching;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.ZArchitecture.GUI.ZForm;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(ViewMatchGroupForm))]
	public class TestViewMatchGroupForm : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			UnmatchingRow testUnmatchingRow = new UnmatchingRow(Factory);
			return new ViewMatchGroupForm(testUnmatchingRow);
		}

		ARInvoice TestARInv;
		ARReceipt TestARReceipt;
		AROverpayment TestAROverpayment;
		APInvoice TestAPInv;
		APReceipt TestAPReceipt;
		APOverpayment TestAPOverpayment;
		TransactionMatchLink ARInvMatch;
		TransactionMatchLink APInvMatch;
		UnmatchingRow MatchGroupRow;
		ViewMatchGroupForm ViewForm;

		void SetupDataForTest()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			TestARInv = Factory.NewWithValidTestData<ARInvoice>();
			TestObjectCreator.CreateInvoiceLine(TestARInv, TestARInv.TransactionCurrency, TestARInv.AH_ExchangeRate, 90m, 0m, 0m, 90m, 0m, 0m);
			TestARInv.AH_LocalOutstandingAmount = 0M;

			TestAPInv = Factory.NewWithValidTestData<APInvoice>();
			TestObjectCreator.CreateInvoiceLine(TestAPInv, TestAPInv.TransactionCurrency, TestAPInv.AH_ExchangeRate, 90m, 0m, 0m, 90m, 0m, 0m);
			TestAPInv.AH_LocalOutstandingAmount = 0m;

			ARInvMatch = ((IMatching)TestARInv).CurrentMatchGroup.AddNew();
			ARInvMatch.AP_AH = TestARInv.PK;
			ARInvMatch.AP_Amount = 90m;
			ARInvMatch.AP_MatchGroupNum = "M00001889";

			APInvMatch = ((IMatching)TestARInv).CurrentMatchGroup.AddNew();
			APInvMatch.AP_AH = TestAPInv.PK;
			APInvMatch.AP_Amount = -90m;
			APInvMatch.AP_MatchGroupNum = "M00001889";
			TestObjectCreator.SetupMatchLinkMatchDate(TestARInv);

			MatchGroupRow = new UnmatchingRow(Factory);
		}

		void SetupDataForAROverpaymentTest()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			TestARInv = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR1234", TestObjectCreator.AUD, 1.0m, 100m, 0m, 100m, 0m);
			TestARInv.AH_OH = TestObjectCreator.Debtor.PK;
			TestARInv.AH_OutstandingAmount = 0M;

			TestARReceipt = TestObjectCreator.CreateARReceipt(0m, 110m, TestARInv.AH_PostDate, TestARInv.AH_PostDate, TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);
			TestARReceipt.AH_InvoiceAmount = -TestARReceipt.AH_OSTotalAmount;
			TestARReceipt.AH_OutstandingAmount = 0M;

			TestAROverpayment = TestObjectCreator.CreateOverpayment<AROverpayment>(10m, TestARInv.AH_PostDate, TestObjectCreator.Debtor.PK);

			ARInvMatch = ((IMatching)TestARInv).CurrentMatchGroup.AddNew();
			ARInvMatch.AP_AH = TestARInv.PK;
			ARInvMatch.AP_Amount = 100m;
			ARInvMatch.AP_MatchGroupNum = "M00001987";
			ARInvMatch.AP_MatchDate = TestARInv.AH_PostDate;

			var receiptMatchGroup = ((IMatching)TestARReceipt).CurrentMatchGroup.AddNew();
			receiptMatchGroup.AP_AH = TestARReceipt.PK;
			receiptMatchGroup.AP_Amount = -110m;
			receiptMatchGroup.AP_MatchGroupNum = "M00001987";
			receiptMatchGroup.AP_MatchDate = TestARInv.AH_PostDate;

			var overpaymentMatchGroup = ((IMatching)TestAROverpayment).CurrentMatchGroup.AddNew();
			overpaymentMatchGroup.AP_AH = TestAROverpayment.PK;
			overpaymentMatchGroup.AP_Amount = 10m;
			overpaymentMatchGroup.AP_MatchGroupNum = "M00001987";
			overpaymentMatchGroup.AP_MatchDate = TestARInv.AH_PostDate;

			MatchGroupRow = new UnmatchingRow(Factory);
			MatchGroupRow.MatchGroupNum = "M00001987";
		}

		void SetupDataForAPOverpaymentTest()
		{
			// Note this scenario is not particularly realistic as AP overpayments are not allowed.
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			TestAPInv = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "AP1234", TestObjectCreator.AUD, 1.0m, 200m, 0m, 200m, 0m);
			TestAPInv.AH_OH = TestObjectCreator.Debtor.PK;
			TestAPInv.AH_LocalOutstandingAmount = 0M;

			TestAPReceipt = TestObjectCreator.CreateAPReceipt(0m, 250m, TestAPInv.AH_PostDate, TestAPInv.AH_PostDate, TestObjectCreator.Creditor1.PK, TestObjectCreator.AUDBankAccount.PK);
			TestAPReceipt.AH_InvoiceAmount = TestAPReceipt.AH_OSTotalAmount;
			TestAPReceipt.AH_LocalOutstandingAmount = 0M;

			TestAPOverpayment = TestObjectCreator.CreateOverpayment<APOverpayment>(50m, TestAPInv.AH_PostDate, TestObjectCreator.Debtor.PK);

			APInvMatch = ((IMatching)TestAPInv).CurrentMatchGroup.AddNew();
			APInvMatch.AP_AH = TestAPInv.PK;
			APInvMatch.AP_Amount = -200m;
			APInvMatch.AP_MatchGroupNum = "M00001988";
			APInvMatch.AP_MatchDate = TestAPInv.AH_PostDate;

			var receiptMatchGroup = ((IMatching)TestAPReceipt).CurrentMatchGroup.AddNew();
			receiptMatchGroup.AP_AH = TestAPReceipt.PK;
			receiptMatchGroup.AP_Amount = 250m;
			receiptMatchGroup.AP_MatchGroupNum = "M00001988";
			receiptMatchGroup.AP_MatchDate = TestAPInv.AH_PostDate;

			var overpaymentMatchGroup = ((IMatching)TestAPOverpayment).CurrentMatchGroup.AddNew();
			overpaymentMatchGroup.AP_AH = TestAPOverpayment.PK;
			overpaymentMatchGroup.AP_Amount = -50m;
			overpaymentMatchGroup.AP_MatchGroupNum = "M00001988";
			overpaymentMatchGroup.AP_MatchDate = TestAPInv.AH_PostDate;

			MatchGroupRow = new UnmatchingRow(Factory);
			MatchGroupRow.MatchGroupNum = "M00001988";
		}

		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		#endregion

		[SuspendCriticalValidation]
		[TestDate(2020, 09, 10)]
		public void TestUnMatchJournalNotThrowExceptionWhenNoReverseTransaction()
		{
			AccountingConfigurationRegistry.Instance.PopupUnMatchTransactionDescriptionOverrideOnUnMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			Factory.Save();

			var testAPJournal = TestObjectCreator.CreateJournal<APJournal>(200m, ZDateTime.Today, TestObjectCreator.Debtor.PK);
			testAPJournal.AH_OutstandingAmount = 0m;
			TestAPOverpayment = TestObjectCreator.CreateOverpayment<APOverpayment>(200m, ZDateTime.Today, TestObjectCreator.Debtor.PK);

			var journalMatchGroup = ((IMatching)testAPJournal).CurrentMatchGroup.AddNew();
			journalMatchGroup.AP_AH = testAPJournal.PK;
			journalMatchGroup.AP_Amount = -200m;
			journalMatchGroup.AP_MatchGroupNum = "M00001988";
			journalMatchGroup.AP_MatchDate = testAPJournal.AH_PostDate;

			var overpaymentMatchGroup = ((IMatching)TestAPOverpayment).CurrentMatchGroup.AddNew();
			overpaymentMatchGroup.AP_AH = TestAPOverpayment.PK;
			overpaymentMatchGroup.AP_Amount = 200m;
			overpaymentMatchGroup.AP_MatchGroupNum = "M00001988";
			overpaymentMatchGroup.AP_MatchDate = testAPJournal.AH_PostDate;

			MatchGroupRow = new UnmatchingRow(Factory);
			MatchGroupRow.MatchGroupNum = "M00001988";

			Factory.Save();

			using (ViewForm = new ViewMatchGroupForm(MatchGroupRow))
			{
				ViewForm.Show();

				ViewForm.DisplayMode = ODisplayMode.Delete;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertNoExceptionThrown(ViewForm.OPostingButtonsUserControl1_ForTestOnly.SaveAndCloseButton.PerformClick);

				ZFormModaliser.LastFormShownDialogForTest.Dispose();
			}
		}

		public void TestChangeUnMatchTransactionDescriptionInForm()
		{
			AccountingConfigurationRegistry.Instance.PopupUnMatchTransactionDescriptionOverrideOnUnMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			Factory.Save();

			var aRMatchingBase = new APMatchingBase(Factory)
			{
				PrimaryOrganization = TestObjectCreator.AALSHI.PK,
				MatchDate = ZDateTime.Today
			};

			AssertDescriptionChanged(aRMatchingBase);

			var aPMatchingBase = new APMatchingBase(Factory)
			{
				PrimaryOrganization = TestObjectCreator.AALSHI.PK,
				MatchDate = ZDateTime.Today
			};

			AssertDescriptionChanged(aPMatchingBase);
		}

		void AssertDescriptionChanged(MatchingBase testMatchingBase)
		{
			Overpayment testOVP = (Overpayment)testMatchingBase.GetMiscellaneousTransaction(TransactionTypes.Overpayment, -10M);
			testOVP.AH_Desc = "Test Overpayment";
			testMatchingBase.AddMiscellaneousTransaction(testOVP);

			Discount testDSC = (Discount)testMatchingBase.GetMiscellaneousTransaction(TransactionTypes.Discount, 10M);
			testDSC.AH_Desc = "Test Discount";
			testMatchingBase.AddMiscellaneousTransaction(testDSC);

			ExchangeDifference testEXX = (ExchangeDifference)testMatchingBase.GetMiscellaneousTransaction(TransactionTypes.ExchangeDifference, 10M);
			testEXX.AH_Desc = "Test ExchangeDifference";
			testMatchingBase.AddMiscellaneousTransaction(testEXX);

			Journal testJNL = (Journal)testMatchingBase.GetMiscellaneousTransaction(TransactionTypes.Journal);
			testJNL.AH_Desc = "Test Journal";
			testJNL.AH_OSTotal = -10M;
			testJNL.AH_InvoiceAmount = -10M;
			testJNL.AH_OutstandingAmount = -10M;
			testMatchingBase.AddMiscellaneousTransaction(testJNL);

			testMatchingBase.MatchAndClearTransactions();
			var unmatchingRow = new UnmatchingRow(Factory)
			{
				MatchGroupNum = testMatchingBase.MatchGroupNumber,
				MatchDate = ZDateTime.Today,
				UnmatchDate = ZDateTime.Today,
			};

			using (ViewForm = new ViewMatchGroupForm(unmatchingRow))
			{
				ViewForm.Show();

				ViewForm.DisplayMode = ODisplayMode.Delete;
				AssertEquals("Precondition: This button should be unmatch button.", "Unmatch", ViewForm.OPostingButtonsUserControl1_ForTestOnly.SaveAndCloseButton.Text);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ViewForm.OPostingButtonsUserControl1_ForTestOnly.SaveAndCloseButton.PerformClick();

				AssertNotNull("A dialog should have been shown", ZFormModaliser.LastFormShownDialogForTest);
				var transactionDescriptionForm = (OverrideTransactionDescriptionForm)ZFormModaliser.LastFormShownDialogForTest;
				AssertEquals("transactionDescriptionForm should be OverrideTransactionDescriptionForm.", typeof(OverrideTransactionDescriptionForm), transactionDescriptionForm.GetType());
				transactionDescriptionForm.Show();
				var wrappedObjs = (transactionDescriptionForm.LastDataSourceForTest as OverrideInvoiceDetailsHelper)?.WrappedObjects;

				AssertEquals("There are should be 4 transaction headers.", 4, wrappedObjs.Count);
				var pks = wrappedObjs.GetPKs();
				AssertCollectionContains(testOVP.ReverseTransaction.PK, pks);
				AssertCollectionContains(testDSC.ReverseTransaction.PK, pks);
				AssertCollectionContains(testEXX.ReverseTransaction.PK, pks);
				AssertCollectionContains(testJNL.ReverseTransaction.PK, pks);

				var expectedDesc = "This is expected transaction header description.";
				wrappedObjs.ForEach(x => (x as AccTransactionHeader).AH_Desc = expectedDesc);
				AssertEquals("Transactionheader should saved successfully.", ContinueWithSave.Yes, transactionDescriptionForm.FireSaveButton());

				var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				query.AddToFilter(AccTransactionHeaderSchema.PK, wrappedObjs.Select(x => x.PK));
				var headers = Factory.Load<AccTransactionHeader>(query).ToList();
				AssertEquals("There are should be 4 transaction headers.", 4, headers.Count);

				headers.ForEach(x => AssertEquals("Description of transaction header should be what we expected.", expectedDesc, x.AH_Desc));

				transactionDescriptionForm.Dispose();
			}
		}

		public void TestShowPreDeleteDialogs_WithARJournal()
		{
			SetupDataForTest();
			Factory.Save();

			// Test Success
			MatchGroupRow.MatchGroupNum = "M00001889";
			using (ViewForm = new ViewMatchGroupForm(MatchGroupRow))
			{
				ContinueWithDelete unmatchResult = ViewForm.ShowPreDeleteDialogs_ForTestOnly();
				AssertEquals("LastMessage shown should be 'You are about to unmatch this match group. Do you want to proceed?', indicating success",
					"You are about to unmatch this match group. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			// Test AR Journal
			var testJournal = Factory.NewWithValidTestData<ARJournal>();
			testJournal.AH_OSTotal = 90M;
			testJournal.AH_InvoiceAmount = 90M;
			testJournal.AH_LocalOutstandingAmount = 0m;
			testJournal.AH_FullyPaidDate = DateTime.Now;
			testJournal.AH_TransactionCategory = Core.Constants.TransactionCategory.Codes.CashAdvanceReceived;

			var journalMatch = ((IMatching)testJournal).CurrentMatchGroup.AddNew();
			journalMatch.AP_MatchGroupNum = "M00001890";
			journalMatch.AP_Amount = 90m;
			journalMatch.AP_AH = testJournal.PK;

			var testAPInv2 = Factory.NewWithValidTestData<APInvoice>();
			TestObjectCreator.CreateInvoiceLine(testAPInv2, testAPInv2.TransactionCurrency, testAPInv2.AH_ExchangeRate, 90m, 0m, 0m, 90m, 0m, 0m);
			testAPInv2.AH_LocalOutstandingAmount = 0m;
			APInvMatch = ((IMatching)testJournal).CurrentMatchGroup.AddNew();
			APInvMatch.AP_AH = testAPInv2.PK;
			APInvMatch.AP_Amount = -90m;
			APInvMatch.AP_MatchGroupNum = "M00001890";
			TestObjectCreator.SetupMatchLinkMatchDate(testJournal);

			Factory.Save();
			MatchGroupRow = new UnmatchingRow(Factory);
			MatchGroupRow.MatchGroupNum = "M00001890";
			MatchGroupRow.MatchDate = ZDateTime.Today;
			MatchGroupRow.UnmatchDate = MatchGroupRow.MatchDate;
			using (ViewForm = new ViewMatchGroupForm(MatchGroupRow))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ContinueWithDelete unmatchResult = ViewForm.ShowPreDeleteDialogs_ForTestOnly();
				AssertEquals("Unmatch should be unsuccessful due to group containing a journal", ContinueWithDelete.No, unmatchResult);
				AssertEquals(@"This group cannot be unmatched because it contains an AR journal with a category of CAR or CAI, which is matched with a transaction with a paid Advance Payment Request.
The transaction with the paid Advance Payment Request must be reversed to unmatch a journal with one of these categories.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowPreDeleteDialogs_WithAPJournal()
		{
			SetupDataForTest();
			Factory.Save();

			// Test Success
			MatchGroupRow.MatchGroupNum = "M00001889";
			using (ViewForm = new ViewMatchGroupForm(MatchGroupRow))
			{
				ContinueWithDelete unmatchResult = ViewForm.ShowPreDeleteDialogs_ForTestOnly();
				AssertEquals("LastMessage shown should be 'You are about to unmatch this match group. Do you want to proceed?', indicating success",
					"You are about to unmatch this match group. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			// Test AP Journal
			var testJournal = Factory.NewWithValidTestData<APJournal>();
			testJournal.AH_OSTotal = 90M;
			testJournal.AH_InvoiceAmount = 90M;
			testJournal.AH_LocalOutstandingAmount = 0m;
			testJournal.AH_FullyPaidDate = DateTime.Now;
			testJournal.AH_TransactionCategory = Core.Constants.TransactionCategory.Codes.CashAdvancePaid;

			var journalMatch = ((IMatching)testJournal).CurrentMatchGroup.AddNew();
			journalMatch.AP_MatchGroupNum = "M00001890";
			journalMatch.AP_Amount = 90m;
			journalMatch.AP_AH = testJournal.PK;

			var testAPInv2 = Factory.NewWithValidTestData<APInvoice>();
			TestObjectCreator.CreateInvoiceLine(testAPInv2, testAPInv2.TransactionCurrency, testAPInv2.AH_ExchangeRate, 90m, 0m, 0m, 90m, 0m, 0m);
			testAPInv2.AH_LocalOutstandingAmount = 0m;
			APInvMatch = ((IMatching)testJournal).CurrentMatchGroup.AddNew();
			APInvMatch.AP_AH = testAPInv2.PK;
			APInvMatch.AP_Amount = -90m;
			APInvMatch.AP_MatchGroupNum = "M00001890";
			TestObjectCreator.SetupMatchLinkMatchDate(testJournal);

			Factory.Save();
			MatchGroupRow = new UnmatchingRow(Factory);
			MatchGroupRow.MatchGroupNum = "M00001890";
			MatchGroupRow.MatchDate = ZDateTime.Today;
			MatchGroupRow.UnmatchDate = MatchGroupRow.MatchDate;
			using (ViewForm = new ViewMatchGroupForm(MatchGroupRow))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ContinueWithDelete unmatchResult = ViewForm.ShowPreDeleteDialogs_ForTestOnly();
				AssertEquals("Unmatch should be unsuccessful due to group containing a journal", ContinueWithDelete.No, unmatchResult);
				AssertEquals(@"This group cannot be unmatched because it contains an AP journal with a category of CAP or CAI, which is matched with a transaction with a paid Advance Payment Request.
The transaction with the paid Advance Payment Request must be reversed to unmatch a journal with one of these categories.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowPreDeleteDialogs_WithPayment()
		{
			SetupDataForTest();
			Factory.Save();

			// Test Success
			MatchGroupRow.MatchGroupNum = "M00001889";
			using (ViewForm = new ViewMatchGroupForm(MatchGroupRow))
			{
				ContinueWithDelete unmatchResult = ViewForm.ShowPreDeleteDialogs_ForTestOnly();
				AssertEquals("LastMessage shown should be 'You are about to unmatch this match group. Do you want to proceed?', indicating success",
					"You are about to unmatch this match group. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			// Test Payment
			APPayment testPay = Factory.NewWithValidTestData<APPayment>();
			testPay.AH_OSTotal = 90M;
			testPay.AH_InvoiceAmount = 90M;
			testPay.AH_LocalOutstandingAmount = 0m;
			testPay.AH_FullyPaidDate = DateTime.Now;

			TransactionMatchLink payMatch = ((IMatching)testPay).CurrentMatchGroup.AddNew();
			payMatch.AP_MatchGroupNum = "M00001890";
			payMatch.AP_Amount = 90m;
			payMatch.AP_AH = testPay.PK;

			var testAPInv2 = Factory.NewWithValidTestData<APInvoice>();
			TestObjectCreator.CreateInvoiceLine(testAPInv2, testAPInv2.TransactionCurrency, testAPInv2.AH_ExchangeRate, 90m, 0m, 0m, 90m, 0m, 0m);
			testAPInv2.AH_LocalOutstandingAmount = 0m;
			APInvMatch = ((IMatching)testPay).CurrentMatchGroup.AddNew();
			APInvMatch.AP_AH = testAPInv2.PK;
			APInvMatch.AP_Amount = -90m;
			APInvMatch.AP_MatchGroupNum = "M00001890";
			TestObjectCreator.SetupMatchLinkMatchDate(testPay);

			Factory.Save();
			MatchGroupRow = new UnmatchingRow(Factory);
			MatchGroupRow.MatchGroupNum = "M00001890";
			MatchGroupRow.MatchDate = ZDateTime.Today;
			MatchGroupRow.UnmatchDate = MatchGroupRow.MatchDate;
			using (ViewForm = new ViewMatchGroupForm(MatchGroupRow))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Env.Security.PayablesAllowUnmatchingPaymentMatching.IsAllowed = false;
				ContinueWithDelete unmatchResult = ViewForm.ShowPreDeleteDialogs_ForTestOnly();
				AssertEquals("Unmatch should be unsuccessful due to group containing a payment", ContinueWithDelete.No,
					unmatchResult);
				AssertEquals("Message should be 'contains payment'", SecurityCore.SecurityErrorMessage + " Unmatch Payment Matching.",
					UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Env.Security.PayablesAllowUnmatchingPaymentMatching.IsAllowed = true;
				unmatchResult = ViewForm.ShowPreDeleteDialogs_ForTestOnly();
				AssertEquals("Unmatch should be successful though group containing a payment", ContinueWithDelete.Yes,
					unmatchResult);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				MatchGroupRow.UnmatchDate = MatchGroupRow.MatchDate.AddDays(-1);
				AssertHasErrors("Precondition:", MatchGroupRow.UnmatchDateInfo);
				unmatchResult = ViewForm.ShowPreDeleteDialogs_ForTestOnly();
				AssertEquals("Unmatch should not be successful if there are any validation errors.", ContinueWithDelete.No,
					unmatchResult);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				MatchGroupRow.UnmatchDate = MatchGroupRow.MatchDate;
				AssertNoErrors("Precondiotion:", MatchGroupRow.UnmatchDateInfo);
				unmatchResult = ViewForm.ShowPreDeleteDialogs_ForTestOnly();
				AssertEquals("Unmatch should be successful if there are no any validation errors.", ContinueWithDelete.Yes,
					unmatchResult);
			}
		}

		public void TestNoUnmatchWithInactiveOrg()
		{
			SetupDataForTest();
			TestARInv.AH_OH = TestObjectCreator.Creditor1.PK;
			TestObjectCreator.Creditor1.OH_IsActive = true;

			Factory.Save();

			// Test Success
			var matchGroupRow = new UnmatchingRow(Factory);
			matchGroupRow.MatchGroupNum = "M00001889";

			using (ViewForm = new ViewMatchGroupForm(matchGroupRow))
			{
				ContinueWithDelete unmatchResult = ViewForm.ShowPreDeleteDialogs_ForTestOnly();
				AssertEquals("LastMessage shown should be 'You are about to unmatch this match group. Do you want to proceed?', indicating success",
					"You are about to unmatch this match group. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			TestObjectCreator.Creditor1.OH_IsActive = false;

			Factory.Save();

			using (ViewForm = new ViewMatchGroupForm(matchGroupRow))
			{
				var expectedMessage = "Data Error occurred while trying to unmatch match group " + matchGroupRow.MatchGroupNum + ". The associated Organization is inactive.";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var unmatchResult = ViewForm.ShowPreDeleteDialogs_ForTestOnly();
				AssertEquals("Unmatch should be unsuccessful due to org being inactive", ContinueWithDelete.No, unmatchResult);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDataError()
		{
			SetupDataForTest();

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var aRInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
			var matchLink = TestObjectCreator.CreateMatchLinkToPayARInvoice(aRInvoice, null, null, "M00001889");
			Factory.Save();

			aRInvoice.AH_OutstandingAmount = 200m;

			var matchGroup = new UnmatchingRow(Factory);
			matchGroup.MatchGroupNum = "M00001889";

			using (var form = new ViewMatchGroupForm(matchGroup))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var unmatchResult = form.ShowPreDeleteDialogs_ForTestOnly();
				AssertEquals("Unmatch should be unsuccessful", ContinueWithDelete.No, unmatchResult);
				AssertEquals("Should be a data error result", UnmatchingResult.DataErrorAddingMatchAmountExceedOriginalInvoiceAmount, matchGroup.CanUnmatchThisMatchGroup);
				var expectedMessage = string.Format("Data Error occurred while trying to unmatch match group {0}. Adding match amount exceeds original invoice amount.", matchGroup.MatchGroupNum);
				AssertEquals("Error message must be shown to the user", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				aRInvoice.AH_OutstandingAmount = 0m;
				aRInvoice.AH_InvoiceAmount = -100m;
				unmatchResult = form.ShowPreDeleteDialogs_ForTestOnly();
				AssertEquals("Unmatch should be unsuccessful", ContinueWithDelete.No, unmatchResult);
				AssertEquals("Should be a data error result", UnmatchingResult.DataErrorInvoiceAmountAndMatchLinkAmountHasOppositeSigns, matchGroup.CanUnmatchThisMatchGroup);
				expectedMessage = string.Format("Data Error occurred while trying to unmatch match group {0}. Both invoice amount and match amount should either be positive or negative.", matchGroup.MatchGroupNum);
				AssertEquals("Error message must be shown to the user", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				aRInvoice.AH_InvoiceAmount = 100m;
				aRInvoice.AH_OutstandingAmount = -100m;
				unmatchResult = form.ShowPreDeleteDialogs_ForTestOnly();
				AssertEquals("Unmatch should be unsuccessful", ContinueWithDelete.No, unmatchResult);
				AssertEquals("Should be a data error result", UnmatchingResult.DataErrorAddingMatchAmountDecreaseAbsValueOfOutstandingAmount, matchGroup.CanUnmatchThisMatchGroup);
				expectedMessage = string.Format("Data Error occurred while trying to unmatch match group {0}. Adding match amount decreases absolute value of outstanding amount.", matchGroup.MatchGroupNum);
				AssertEquals("Error message must be shown to the user", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[SuspendCriticalValidation]
		public void TestUnmatchOverpaymentARContextCheckpointAllowed()
			=> TestUnmatchCheckpoint(true, Env.Security.ReceivablesUnMatchTransactionOverpaymentType, "Manage -> Receivables -> Match Transactions -> Unmatch Receivable Transactions -> Overpayment Transactions", SetupDataForAROverpaymentTest);
		[SuspendCriticalValidation]
		public void TestUnmatchOverpaymentARContextCheckpointDenied()
			=> TestUnmatchCheckpoint(false, Env.Security.ReceivablesUnMatchTransactionOverpaymentType, "Manage -> Receivables -> Match Transactions -> Unmatch Receivable Transactions -> Overpayment Transactions", SetupDataForAROverpaymentTest);

		[SuspendCriticalValidation]
		public void TestUnmatchOverpaymentAPContextCheckpointAllowed()
			=> TestUnmatchCheckpoint(true, Env.Security.PayablesUnMatchTransactionsOverpaymentType, "Manage -> Payables -> Match Transactions -> Unmatch Payables Transactions -> Overpayment Transactions", SetupDataForAPOverpaymentTest);
		[SuspendCriticalValidation]
		public void TestUnmatchOverpaymentAPContextCheckpointDenied()
			=> TestUnmatchCheckpoint(false, Env.Security.PayablesUnMatchTransactionsOverpaymentType, "Manage -> Payables -> Match Transactions -> Unmatch Payables Transactions -> Overpayment Transactions", SetupDataForAPOverpaymentTest);

		void TestUnmatchCheckpoint(bool checkpointIsAllowed, SecurityCheckpoint checkpoint, string expectedCheckpointPath, Action setupData)
		{
			setupData();
			Factory.Save();

			checkpoint.IsAllowed = checkpointIsAllowed;
			using (ViewForm = new ViewMatchGroupForm(MatchGroupRow))
			{
				ViewForm.ShowPreDeleteDialogs_ForTestOnly();

				if (checkpointIsAllowed)
				{
					AssertEquals("Confirmation to delete should be shown", "You are about to unmatch this match group. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				else
				{
					AssertEquals("Security checkpoint error should be shown", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

" + expectedCheckpointPath, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			checkpoint.ClearOverriddenSecurityValue();
		}

		public void TestSetReadOnlyIncludingChildren()
		{
			MatchGroupRow = new UnmatchingRow(Factory);
			using (var testForm = new ViewMatchGroupForm(MatchGroupRow))
			{
				testForm.DisplayMode = ODisplayMode.Delete;
				testForm.Show();
				Application.DoEvents();

				foreach (Control control in testForm.Controls)
				{
					Assert(control.Name + " must be editable for reversing", !control.GetReadOnly());
				}
				foreach (ZPropertyInfo info in MatchGroupRow.ZPropertyInfoHash)
				{
					if (info.Name != MatchGroupRow.UnmatchDateInfo.Name)
					{
						AssertEquals(info.Name + " must be readonly for reversing", true, info.ReadOnly);
					}
					else
					{
						AssertEquals(info.Name + " must not be readonly for reversing", false, info.ReadOnly);
					}
				}
			}

			using (var testForm = new ViewMatchGroupForm(MatchGroupRow))
			{
				testForm.DisplayMode = ODisplayMode.ReadOnly;
				testForm.Show();
				Application.DoEvents();

				foreach (ZPropertyInfo info in MatchGroupRow.ZPropertyInfoHash)
				{
					AssertEquals(info.Name + " must be readonly if it's view mode", true, info.ReadOnly);
				}
			}
		}

		public void TestUnmatchDateEditVisibility()
		{
			MatchGroupRow = new UnmatchingRow(Factory);
			using (var testForm = new ViewMatchGroupForm(MatchGroupRow))
			{
				testForm.DisplayMode = ODisplayMode.Delete;
				testForm.Show();
				Application.DoEvents();

				AssertEquals("Should be visible for reversing.", true, testForm.UnmatchDateEdit_ForTestOnly.Visible);
			}

			using (var testForm = new ViewMatchGroupForm(MatchGroupRow))
			{
				testForm.DisplayMode = ODisplayMode.ReadOnly;
				testForm.Show();
				Application.DoEvents();

				AssertEquals("Should be visible if it's view mode.", false, testForm.UnmatchDateEdit_ForTestOnly.Visible);
			}
		}
	}
}
