using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.CountryCompliance.TurkeyComplianceInfo;
using static Enterprise.MasterFiles.Business.TurkeyOrgCusCodeInfo;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing
{
	public class TransactionCreationRestrictionHelperTest : TestCaseWithFactory
	{
		public void TestAllowToCreatePaymentApproval_NonCurrentCompanyData()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var helper = new TransactionCreationRestrictionHelper();

			var currentCompanyData = testObjectCreator.ABIGAS.CompanyData;
			currentCompanyData.OB_ARTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.All;

			var nonCurrentCompanyData = testObjectCreator.ABIGAS.GetCompanyDataForGlbCompany(testObjectCreator.NonCurrentCompany);
			nonCurrentCompanyData.OB_ARTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.None;
			Factory.Save();

			var arPayment = Factory.New<ARPaymentApprovalWithAuthorisation>();
			arPayment.AV_OH = testObjectCreator.ABIGAS.PK;

			arPayment.AV_GC = testObjectCreator.NonCurrentCompany.PK;
			AssertEquals(true, helper.AllowToCreatePaymentApproval(arPayment, out ResourceString errorMessage1));
			AssertNullOrEmpty(errorMessage1);

			arPayment.AV_GC = GlbCompany.CurrentCompany.PK;
			AssertEquals(false, helper.AllowToCreatePaymentApproval(arPayment, out ResourceString errorMessage2));
			AssertNotNullOrEmpty(errorMessage2);
		}

		public void TestAllowToCreateTransaction_NonCurrentCompanyData()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var helper = new TransactionCreationRestrictionHelper();

			var currentCompanyData = testObjectCreator.ABIGAS.CompanyData;
			currentCompanyData.OB_ARTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.All;

			var nonCurrentCompanyData = testObjectCreator.ABIGAS.GetCompanyDataForGlbCompany(testObjectCreator.NonCurrentCompany);
			nonCurrentCompanyData.OB_ARTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.None;
			Factory.Save();

			var transaction = Factory.NewWithValidTestData<ARInvoice>();
			transaction.AH_OH = testObjectCreator.ABIGAS.PK;

			transaction.AH_GC = testObjectCreator.NonCurrentCompany.PK;
			AssertEquals(true, helper.AllowToCreateTransaction(transaction, out ResourceString errorMessage1));
			AssertNullOrEmpty(errorMessage1);

			transaction.AH_GC = GlbCompany.CurrentCompany.PK;
			AssertEquals(false, helper.AllowToCreateTransaction(transaction, out ResourceString errorMessage2));
			AssertNotNullOrEmpty(errorMessage2);
		}

		public void TestAllowToCreatePaymentApproval_AR()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var helper = new TransactionCreationRestrictionHelper();

			ARPaymentApprovalWithAuthorisation arPayment = Factory.New<ARPaymentApprovalWithAuthorisation>();
			arPayment.AV_OH = testObjectCreator.ABIGAS.PK;

			testObjectCreator.ABIGAS.CompanyData.OB_ARTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.All;
			Assert(!helper.AllowToCreatePaymentApproval(arPayment, out ResourceString errorMessage1));
			AssertEquals($@"You cannot create transaction '' because organization '{arPayment.Header.OH_Code}' has an AR transaction creation restriction policy set to ALL.
Transaction creation restriction policy is configured under Organization > A/R or A/P > Configuration > Company Data.", errorMessage1);

			testObjectCreator.ABIGAS.CompanyData.OB_ARTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.OutstandingBalance;
			Assert(!helper.AllowToCreatePaymentApproval(arPayment, out ResourceString errorMessage2));
			AssertEquals($@"You cannot create transaction '' because organization '{arPayment.Header.OH_Code}' has an AR transaction creation restriction policy set to BAL.
Transaction creation restriction policy is configured under Organization > A/R or A/P > Configuration > Company Data.", errorMessage2);

			testObjectCreator.ABIGAS.CompanyData.OB_ARTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.Invoice;
			Assert(helper.AllowToCreatePaymentApproval(arPayment, out ResourceString errorMessage3));
			AssertNullOrEmpty(errorMessage3);

			testObjectCreator.ABIGAS.CompanyData.OB_ARTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.None;
			Assert(helper.AllowToCreatePaymentApproval(arPayment, out ResourceString errorMessage4));
			AssertNullOrEmpty(errorMessage4);

			arPayment.AV_OH = ZGuid.Empty;
			Assert(helper.AllowToCreatePaymentApproval(arPayment, out ResourceString errorMessage5));
			AssertNullOrEmpty(errorMessage5);

			arPayment.AV_OH = ZGuid.NewZGuid();
			Assert(helper.AllowToCreatePaymentApproval(arPayment, out ResourceString errorMessage6));
			AssertNullOrEmpty(errorMessage6);
		}

		public void TestAllowToCreatePaymentApproval_AP()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var helper = new TransactionCreationRestrictionHelper();

			APPaymentApprovalWithAuthorisation apPayment = Factory.New<APPaymentApprovalWithAuthorisation>();
			apPayment.AV_OH = testObjectCreator.ABIGAS.PK;

			testObjectCreator.ABIGAS.CompanyData.OB_APTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.All;
			Assert(!helper.AllowToCreatePaymentApproval(apPayment, out ResourceString errorMessage1));
			AssertEquals($@"You cannot create transaction '' because organization '{apPayment.Header.OH_Code}' has an AP transaction creation restriction policy set to ALL.
Transaction creation restriction policy is configured under Organization > A/R or A/P > Configuration > Company Data.", errorMessage1);

			testObjectCreator.ABIGAS.CompanyData.OB_APTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.OutstandingBalance;
			Assert(helper.AllowToCreatePaymentApproval(apPayment, out ResourceString errorMessage2));
			AssertNullOrEmpty(errorMessage2);

			testObjectCreator.ABIGAS.CompanyData.OB_APTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.Invoice;
			Assert(helper.AllowToCreatePaymentApproval(apPayment, out ResourceString errorMessage3));
			AssertNullOrEmpty(errorMessage3);

			testObjectCreator.ABIGAS.CompanyData.OB_APTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.None;
			Assert(helper.AllowToCreatePaymentApproval(apPayment, out ResourceString errorMessage4));
			AssertNullOrEmpty(errorMessage4);
		}

		public void TestAllowToCreatePaymentApproval_CompanyDataNotCreatedIfNotExist()
		{
			var helper = new TransactionCreationRestrictionHelper();
			var apPayment = Factory.New<AccPaymentApproval>();
			var org = Factory.New<OrgHeader>();
			apPayment.AV_GC = GlbCompany.CurrentCompany.PK;
			apPayment.AV_OH = org.PK;

			var companyData = OrgCompanyData.Load(org.Factory, org.PK, GlbCompany.CurrentCompany.PK);
			AssertNull("Precondition", companyData);

			helper.AllowToCreatePaymentApproval(apPayment, out var _);

			companyData = OrgCompanyData.Load(org.Factory, org.PK, GlbCompany.CurrentCompany.PK);
			AssertNull(companyData);
		}

		public void TestAllowToCreateTransaction()
		{
			var helper = new TransactionCreationRestrictionHelper();

			var transaction = SetupTestData<AccTransactionHeader>(Core.Constants.TransactionCreationRestriction.None);
			transaction.Header.CompanyData.OB_ARTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.Invoice;
			transaction.Header.CompanyData.OB_APTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.None;
			transaction.AH_TransactionType = TransactionTypes.Invoice;

			transaction.AH_Ledger = LedgerTypes.AccountsPayable;
			Assert(helper.AllowToCreateTransaction(transaction, out ResourceString errorMessage1));

			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			Assert(!helper.AllowToCreateTransaction(transaction, out ResourceString errorMessage2));
			AssertEquals($@"You cannot create transaction '{transaction.AH_TransactionNum}' because organization '{transaction.Header.OH_Code}' has an AR transaction creation restriction policy set to INV.
Transaction creation restriction policy is configured under Organization > A/R or A/P > Configuration > Company Data.", errorMessage2);

			transaction.AH_OH = ZGuid.Empty;
			Assert(helper.AllowToCreateTransaction(transaction, out ResourceString errorMessage3));
		}

		public void TestCheckOrgHeaderAllowsPosting_WhenComplianceSubtypeIsEAR() =>
			TestCheckOrgHeaderAllowsPosting(@"An email address is required for this Debtor to allow the receivables transaction to be successfully posted.
Before attempting to post the charges, please update the Organization record for the Debtor to include a valid email address (Maintain > Master Data > Organization).

This can be updated at Maintain > Master Data > Organization. (Details > Config > Registration Numbers/Codes sub-tab)", ComplianceSubTypeCodes.EAR, true);

		public void TestCheckOrgHeaderAllowsPosting_WhenComplianceSubtypeIsNotEAR() =>
			TestCheckOrgHeaderAllowsPosting(@"A Post Box Alias is required for this debtor to allow the receivables transaction to be successfully posted.
Before attempting to post the charges, please update the organization record for the debtor to include a valid Post Box Alias email address using the registration number type PEC.

This can be updated at Maintain > Master Data > Organization. (Details > Config > Registration Numbers/Codes sub-tab)", ComplianceSubTypeCodes.EIN, false);

		void TestCheckOrgHeaderAllowsPosting(string expectedMessage, string complianceSubType, bool assertForErrors)
		{
			var helper = new TransactionCreationRestrictionHelper();

			var transaction = SetupTestData<ARInvoice>(TransactionCreationRestriction.None);
			transaction.Header.CompanyData.OB_ARTransactionCreationRestriction = TransactionCreationRestriction.Invoice;
			transaction.Header.CompanyData.OB_APTransactionCreationRestriction = TransactionCreationRestriction.None;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_ComplianceSubType = complianceSubType;

			Assert(helper.CheckOrgHeaderAllowsPosting(transaction, out ResourceString errorMessage1, out bool isErrorMessage));

			var testObjectCreator = new TestObjectCreator(transaction.Factory);

			using (testObjectCreator.SetUpForTestingEInvoicingTurkey_Receivables(transaction.AH_GB.ToGuid(), DateTime.Today.AddDays(-30)))
			{
				Assert(!helper.CheckOrgHeaderAllowsPosting(transaction, out ResourceString errorMessage2, out isErrorMessage));
				AssertEquals(expectedMessage, errorMessage2);

				var pecCusCode = transaction.Header.CustomsCodes.AddNew(OrgCusCodes.PEC, "debtor@testmailaddress.com");
				Assert(helper.CheckOrgHeaderAllowsPosting(transaction, out ResourceString errorMessage3, out isErrorMessage));
				AssertNull(errorMessage3);
			}
		}

		public void TestAllowToCreateTransaction_InDatabase()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var transaction = Factory.NewWithValidTestData<APInvoice>();
			transaction.AH_OH = testObjectCreator.Creditor1.PK;
			Factory.Save();
			var incompleteTransaction = Factory.NewWithValidTestData<APInvoice>();
			incompleteTransaction.AH_OH = testObjectCreator.Creditor1.PK;
			incompleteTransaction.SaveAsIncomplete();
			Factory.Save();

			transaction.Header.CompanyData.OB_ARTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.All;
			transaction.Header.CompanyData.OB_APTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.All;
			Factory.Save();

			transaction.AH_Desc = "New desc";
			Assert("Pre Condition:", transaction.IsInDatabase);
			AssertEquals("Pre Condition:", LedgerTypes.AccountsPayable, transaction.AH_Ledger);
			Factory.Save();
			AssertEquals("AH_Ledger not change", LedgerTypes.AccountsPayable, transaction.AH_Ledger);
			AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);

			var reloadedInvoice = Factory.Load<InvoicingBase>(incompleteTransaction.PK);
			reloadedInvoice.RestoreSavedData();
			Assert("Pre Condition:", reloadedInvoice.IsInDatabase);
			AssertEquals("Pre Condition:", LedgerTypes.IncompleteTransactions, reloadedInvoice.AH_Ledger);
			reloadedInvoice.MoveFromIncompleteToPayableLedger();
			var ex = AssertExceptionThrown<OnSavingCriticalCheckException<AccTransactionHeader>>("Should have error", () => Factory.Save());
			AssertContains(@"AP transaction creation restriction policy set to ALL.
Transaction creation restriction policy is configured under Organization > A/R or A/P > Configuration > Company Data.", ex.DeveloperErrorMessage);
		}

		public void TestAllowToCreateTransaction_NON()
		{
			var helper = new TransactionCreationRestrictionHelper();

			var transaction = SetupTestData<AccTransactionHeader>(TransactionCreationRestriction.None);
			AssertIsAllowToCreateTransaction(transaction, new[] { true, true, true, true, true });

			transaction.AH_IsCancelled = true;
			AssertIsAllowToCreateTransaction(transaction, new[] { true, true, true, true, true });

			transaction.AH_Ledger = LedgerTypes.CashBook;
			Assert(helper.AllowToCreateTransaction(transaction, out ResourceString errorMessage1));

			transaction.AH_OH = ZGuid.Empty;
			Assert(helper.AllowToCreateTransaction(transaction, out ResourceString errorMessage2));
		}

		public void TestAllowToCreateTransaction_INV()
		{
			var transaction = SetupTestData<AccTransactionHeader>(TransactionCreationRestriction.Invoice);
			AssertIsAllowToCreateTransaction(transaction, new[] { false, false, false, true, true });

			transaction.AH_IsCancelled = true;
			AssertIsAllowToCreateTransaction(transaction, new[] { true, true, true, true, true });
		}

		public void TestAllowToCreateTransaction_ALL()
		{
			var transaction = SetupTestData<AccTransactionHeader>(TransactionCreationRestriction.All);
			AssertIsAllowToCreateTransaction(transaction, new[] { false, false, false, false, false });

			transaction.AH_IsCancelled = true;
			AssertIsAllowToCreateTransaction(transaction, new[] { true, true, true, true, true });
		}

		public void TestAllowToCreateTransaction_APBAL()
		{
			var transaction = SetupTestData_BAL(5M, -2M);

			transaction.AH_OutstandingAmount = 1m;

			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			AssertIsAllowToCreateTransaction_BAL(transaction, new[] { false, false, false, false, false });

			transaction.AH_Ledger = LedgerTypes.AccountsPayable;
			AssertIsAllowToCreateTransaction_BAL(transaction, new[] { true, true, true, true, true });

			transaction.AH_OutstandingAmount = -1m;

			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			AssertIsAllowToCreateTransaction_BAL(transaction, new[] { true, true, true, true, true });

			transaction.AH_Ledger = LedgerTypes.AccountsPayable;
			AssertIsAllowToCreateTransaction_BAL(transaction, new[] { false, false, false, false, false });
		}

		public void TestAllowToCreateTransaction_ARBAL()
		{
			var transaction = SetupTestData_BAL(2M, -5M);

			transaction.AH_OutstandingAmount = 1m;

			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			AssertIsAllowToCreateTransaction_BAL(transaction, new[] { false, false, false, false, false });

			transaction.AH_Ledger = LedgerTypes.AccountsPayable;
			AssertIsAllowToCreateTransaction_BAL(transaction, new[] { true, true, true, true, true });

			transaction.AH_OutstandingAmount = -1m;

			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			AssertIsAllowToCreateTransaction_BAL(transaction, new[] { true, true, true, true, true });

			transaction.AH_Ledger = LedgerTypes.AccountsPayable;
			AssertIsAllowToCreateTransaction_BAL(transaction, new[] { false, false, false, false, false });
		}

		public void TestAllowToCreateTransaction_BAL_InDatabasePositiveAP()
		{
			var org = SetupTestData(5M, -2M);
			var transaction = CreateTransactionInDBForBAL(LedgerTypes.AccountsPayable, org, 5m);

			transaction.AH_OutstandingAmount = 0m;
			AssertEquals("Pre-condition", true, transaction.AH_OutstandingAmountInfo.HasChanges);

			AssertIsAllowToCreateTransaction_BAL(transaction, new[] { false, false, false, false, false });
		}

		public void TestAllowToCreateTransaction_BAL_InDatabaseNegativeAP()
		{
			var org = SetupTestData(5M, -2M);
			var transaction = CreateTransactionInDBForBAL(LedgerTypes.AccountsPayable, org, -5m);

			transaction.AH_OutstandingAmount = 0m;
			AssertEquals("Pre-condition", true, transaction.AH_OutstandingAmountInfo.HasChanges);

			AssertIsAllowToCreateTransaction_BAL(transaction, new[] { true, true, true, true, true });
		}

		public void TestAllowToCreateTransaction_BAL_InDatabaseNegativeAR()
		{
			var org = SetupTestData(5M, -2M);
			var transaction = CreateTransactionInDBForBAL(LedgerTypes.AccountsReceivable, org, -5m);

			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_InvoiceAmount = -5m;
			transaction.AH_OutstandingAmount = -5m;
			Factory.Save();

			transaction.AH_OutstandingAmount = 0m;
			AssertEquals("Pre-condition", true, transaction.AH_OutstandingAmountInfo.HasChanges);

			AssertIsAllowToCreateTransaction_BAL(transaction, new[] { false, false, false, false, false });
		}

		public void TestAllowToCreateTransaction_BAL_InDatabasePositiveAR()
		{
			var org = SetupTestData(5M, -2M);
			var transaction = CreateTransactionInDBForBAL(LedgerTypes.AccountsReceivable, org, 5m);

			transaction.AH_OutstandingAmount = 0m;
			AssertEquals("Pre-condition", true, transaction.AH_OutstandingAmountInfo.HasChanges);

			AssertIsAllowToCreateTransaction_BAL(transaction, new[] { true, true, true, true, true });
		}

		public void TestAllowToReverseTransaction_BAL()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var org = testObjectCreator.CreateOrgHeader("Org1", true, true, true, true, true, true);

			var positiveARAdjustmentNote = testObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("001", 100.00m, 0m, new ZDateTime(2008, 06, 15), org.PK);
			testObjectCreator.CreateAdjusmentNoteLine(positiveARAdjustmentNote, testObjectCreator.CommentChargeCode.PK, 100.00m, 0m);

			var negativeARAdjustmentNote = testObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("002", -100.00m, 0m, new ZDateTime(2009, 06, 15), org.PK);
			testObjectCreator.CreateAdjusmentNoteLine(negativeARAdjustmentNote, testObjectCreator.CommentChargeCode.PK, -100.00m, 0m);

			var positiveAPAdjustmentNote = testObjectCreator.CreateAdjustmentNote<APAdjustmentNote>("003", 100.00m, 0m, new ZDateTime(2010, 06, 15), org.PK);
			testObjectCreator.CreateAdjusmentNoteLine(positiveAPAdjustmentNote, testObjectCreator.CommentChargeCode.PK, 100.00m, 0m);

			var negativeAPAdjustmentNote = testObjectCreator.CreateAdjustmentNote<APAdjustmentNote>("004", -100.00m, 0m, new ZDateTime(2011, 06, 15), org.PK);
			testObjectCreator.CreateAdjusmentNoteLine(negativeAPAdjustmentNote, testObjectCreator.CommentChargeCode.PK, -100.00m, 0m);
			Factory.Save();

			org.CompanyData.OB_APTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.OutstandingBalance;
			org.CompanyData.OB_ARTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.OutstandingBalance;
			Factory.Save();

			AssertShouldExistTransactionCreationRestrictionError(positiveARAdjustmentNote, false);
			AssertShouldExistTransactionCreationRestrictionError(negativeARAdjustmentNote, true);
			AssertShouldExistTransactionCreationRestrictionError(positiveAPAdjustmentNote, false);
			AssertShouldExistTransactionCreationRestrictionError(negativeAPAdjustmentNote, true);

			void AssertShouldExistTransactionCreationRestrictionError(AdjustmentNote adj, bool shouldExistError)
			{
				var factory = new BusinessObjectFactory();
				AdjustmentNote adjustmentNote;
				if (adj.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					adjustmentNote = factory.Load<APAdjustmentNote>(adj.PK);
				}
				else
				{
					adjustmentNote = factory.Load<ARAdjustmentNote>(adj.PK);
				}

				var reversing = new ReversingFactory().NewReversing(adjustmentNote);
				reversing.Reverse();
				if (adj.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					((AccTransactionHeader)reversing.ReverseTransaction).AH_TransactionNum = "Reverse" + adj.AH_TransactionNum;
				}

				if (shouldExistError)
				{
					var ex = AssertExceptionThrown<OnSavingCriticalCheckException>(() => factory.Save());
					AssertEquals("Should be blocked by transaction creation restriction", nameof(CriticalValidationErrorType.CreateTransactionRestriction), ex.ErrorType);
				}
				else
				{
					AssertNoExceptionThrown(() => factory.Save());
				}
			}
		}

		void AssertIsAllowToCreateTransaction_BAL(AccTransactionHeader transaction, bool[] results)
		{
			var helper = new TransactionCreationRestrictionHelper();
			List<string> transactionTypes = new List<string> { TransactionTypes.Invoice, TransactionTypes.AdjustmentNote, TransactionTypes.CreditNote, TransactionTypes.Receipt, TransactionTypes.Payment };

			foreach (var transactionType in transactionTypes)
			{
				transaction.AH_TransactionType = transactionType;
				AssertEquals(results[transactionTypes.IndexOf(transactionType)], helper.AllowToCreateTransaction(transaction, out ResourceString errorMessage));
			}
		}

		void AssertIsAllowToCreateTransaction(AccTransactionHeader transaction, bool[] results)
		{
			var helper = new TransactionCreationRestrictionHelper();
			List<string> ledgers = new List<string> { LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable };
			List<string> transactionTypes = new List<string> { TransactionTypes.Invoice, TransactionTypes.AdjustmentNote, TransactionTypes.CreditNote, TransactionTypes.Receipt, TransactionTypes.Payment };

			foreach (var ledger in ledgers)
			{
				foreach (var transactionType in transactionTypes)
				{
					transaction.AH_Ledger = ledger;
					transaction.AH_TransactionType = transactionType;

					AssertEquals(results[transactionTypes.IndexOf(transactionType)], helper.AllowToCreateTransaction(transaction, out ResourceString errorMessage));
				}
			}
		}

		AccTransactionHeader SetupTestData_BAL(ZDecimal amountAR, ZDecimal amountAP)
		{
			var org = SetupTestData(amountAR, amountAP);
			return CreateTransaction<AccTransactionHeader>(org, TransactionCreationRestriction.OutstandingBalance);
		}

		OrgHeader SetupTestData(ZDecimal amountAR, ZDecimal amountAP)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsCreditor = true;
			org.OH_IsDebtor = true;

			var transactionSaved = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionSaved.AH_Ledger = LedgerTypes.AccountsReceivable;
			transactionSaved.AH_TransactionType = TransactionTypes.Invoice;
			transactionSaved.AH_OH = org.PK;
			transactionSaved.AH_OutstandingAmount = amountAR;
			transactionSaved.AH_InvoiceAmount = amountAR;

			transactionSaved = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionSaved.AH_Ledger = LedgerTypes.AccountsPayable;
			transactionSaved.AH_TransactionType = TransactionTypes.Invoice;
			transactionSaved.AH_OH = org.PK;
			transactionSaved.AH_OutstandingAmount = amountAP;
			transactionSaved.AH_InvoiceAmount = amountAP;

			Factory.Save();

			return org;
		}

		T SetupTestData<T>(string transactionCreationRestriction) where T : AccTransactionHeader
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsCreditor = true;
			org.OH_IsDebtor = true;
			return CreateTransaction<T>(org, transactionCreationRestriction);
		}

		T CreateTransaction<T>(OrgHeader org, string transactionCreationRestriction) where T : AccTransactionHeader
		{
			org.CompanyData.OB_ARTransactionCreationRestriction = transactionCreationRestriction;
			org.CompanyData.OB_APTransactionCreationRestriction = transactionCreationRestriction;
			Factory.Save();

			var transaction = Factory.NewWithValidTestData<T>();
			transaction.AH_OH = org.PK;
			return transaction;
		}

		AccTransactionHeader CreateTransactionInDBForBAL(string ledger, OrgHeader org, ZDecimal amount)
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = ledger;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_OH = org.PK;
			transaction.AH_InvoiceAmount = amount;
			transaction.AH_OutstandingAmount = amount;
			Factory.Save();

			org.CompanyData.OB_ARTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.OutstandingBalance;
			org.CompanyData.OB_APTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.OutstandingBalance;
			Factory.Save();

			return transaction;
		}
	}
}
