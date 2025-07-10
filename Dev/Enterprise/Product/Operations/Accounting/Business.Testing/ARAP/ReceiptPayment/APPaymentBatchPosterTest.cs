using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using Env = Enterprise.Environment.Env;
using ReceiptTypes = Enterprise.ZArchitecture.Core.ReceiptTypes;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	[TestedType(typeof(APPaymentBatchPoster))]
	public class APPaymentBatchPosterTest : AccPaymentBatchTest
	{
		#region Prepare Test Data

		void SetupDataForBaseTest(bool shouldCreatePaymentBatch = true)
		{
			BatchPostingHelper.PrepareForBaseTest(shouldCreatePaymentBatch);
			CopyBatchPostingHelperData();
		}

		void SetupDataForPostingTest(bool shouldSetupDataForBaseTest = true)
		{
			if (shouldSetupDataForBaseTest)
			{
				SetupDataForBaseTest(false);
			}

			BatchPostingHelper.SetupDataForPostingTest();
			BatchPoster = BatchPostingHelper.BatchPoster;
			PaymentApproval1 = BatchPostingHelper.PaymentApproval1;
			PaymentApproval2 = BatchPostingHelper.PaymentApproval2;
			PaymentApproval3 = BatchPostingHelper.PaymentApproval3;
		}

		void SetupDataForSingleForeignCurrencyPaymentTest()
		{
			SetupDataForBaseTest(false);

			var localCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			var foreignCurrency = BatchPostingHelper.TestObjectCreator.USD;
			AssertNotEquals("Precondition: local currency is not the foreign currency", localCurrency.RX_Code, foreignCurrency.RX_Code);

			TestAPInv = (APInvoice)BatchPostingHelper.TestObjectCreator.CreateInvoice(typeof(APInvoice), "0001", foreignCurrency, 1.02m);
			TestAPInv.AH_RX_NKTransactionCurrency = foreignCurrency.RX_Code;
			TestAPInv.AH_OH = BatchPostingHelper.TestObjectCreator.Creditor1.PK;
			BatchPostingHelper.TestObjectCreator.CreateInvoiceLine(TestAPInv, foreignCurrency, 1.02m, 1000m, 100m, 0m);

			//A partially paid AP invoice
			TestAPInv2 = (APInvoice)BatchPostingHelper.TestObjectCreator.CreateInvoice(typeof(APInvoice), "0002", foreignCurrency, 1.06m);
			TestAPInv2.AH_RX_NKTransactionCurrency = foreignCurrency.RX_Code;
			TestAPInv2.AH_OH = BatchPostingHelper.TestObjectCreator.Creditor1.PK;
			BatchPostingHelper.TestObjectCreator.CreateInvoiceLine(TestAPInv2, foreignCurrency, 1.06m, 2000m, 200m, 0m);
			IMatching invoiceAsIMatching = TestAPInv2;
			invoiceAsIMatching.OSPartialPaymentAmount = -300m;
			invoiceAsIMatching.PartiallyPay();
			invoiceAsIMatching.GenerateMatchLinks();
			var partialPay = BatchPostingHelper.TestObjectCreator.CreateAPPayment(1.06m, 300m, ZDateTime.Today, ZDateTime.Today, TestAPInv2.Header.PK, BatchPostingHelper.TestObjectCreator.USDBankAccount.PK);
			TransactionMatchLink payLink = invoiceAsIMatching.CurrentMatchGroup.AddNew();
			payLink.AP_AH = partialPay.PK;
			payLink.AP_Amount = partialPay.AH_InvoiceAmount;
			partialPay.AH_FullyPaidDate = ZDateTime.Today;
			partialPay.AH_OutstandingAmount = 0M;
			TestObjectCreator.SetupMatchLinkMatchDate(invoiceAsIMatching);

			TestAPInv3 = (APInvoice)BatchPostingHelper.TestObjectCreator.CreateInvoice(typeof(APInvoice), "0003", foreignCurrency, 1.02m);
			TestAPInv3.AH_RX_NKTransactionCurrency = foreignCurrency.RX_Code;
			TestAPInv3.AH_OH = BatchPostingHelper.TestObjectCreator.Creditor2.PK;
			BatchPostingHelper.TestObjectCreator.CreateInvoiceLine(TestAPInv3, foreignCurrency, 1.02m, 3000m, 300m, 0m);

			TestAPInv4 = (APInvoice)BatchPostingHelper.TestObjectCreator.CreateInvoice(typeof(APInvoice), "0004", localCurrency, 1m);
			TestAPInv4.AH_RX_NKTransactionCurrency = localCurrency.RX_Code;
			TestAPInv4.AH_OH = BatchPostingHelper.TestObjectCreator.Creditor2.PK;
			BatchPostingHelper.TestObjectCreator.CreateInvoiceLine(TestAPInv4, localCurrency, 1m, 4000m, 400m, 0m);

			Factory.Save();

			BatchPoster = Factory.New<APPaymentBatchPoster>();
			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory);
			transactions.AddRange(TestAPInv, TestAPInv2, TestAPInv3, TestAPInv4);
			BatchPoster.SetDefaultValuesByTransactions(transactions);
			AssertEquals("Number of payments in the collection", 2, BatchPoster.PaymentApprovalCollection.Count);
			PaymentApproval1 = BatchPostingHelper.FindPaymentApprovalThatContainSpecificTransaction(BatchPoster.PaymentApprovalCollection, TestAPInv);
			PaymentApproval2 = BatchPostingHelper.FindPaymentApprovalThatContainSpecificTransaction(BatchPoster.PaymentApprovalCollection, TestAPInv3);
		}

		void SetupDataForMultipleTransactionsTest()
		{
			SetupDataForBaseTest(false);
			BatchPoster = BatchPostingHelper.CreateBatchPosterForBaseTest(BatchPostingHelper.GetTransactionsForPosting());

			TestOrg2 = BatchPostingHelper.TestOrg2;
			TestOrg3 = BatchPostingHelper.TestOrg3;
			TestAPInv2 = BatchPostingHelper.TestAPInv2;
			TestAPRec = BatchPostingHelper.TestAPRec;
		}

		void CopyBatchPostingHelperData()
		{
			TestOrg = BatchPostingHelper.TestOrg;
			TestBank = BatchPostingHelper.TestBank;
			TestCheques = BatchPostingHelper.TestCheques;
			TestCheques.AK_GB = GlbBranch.CurrentBranch.PK;
			TestAPInv = BatchPostingHelper.TestAPInv;
			BatchPoster = BatchPostingHelper.BatchPoster;
		}

		ZGuid BatchPoster_OnSelectBankAccountFromDefault(AccBankAccountCollection bankAccountCollection)
		{
			AssertEquals("There should be 2 banks in collection", 2, bankAccountCollection.Count);
			Assert("Collection should contain TestBank", bankAccountCollection.Contains(TestBank));
			Assert("Collection should contain TestBank2", bankAccountCollection.Contains(AnotherBank));
			return AnotherBank.PK;
		}

		TransactionHeaderCollection GetTransactionsForSplittingTest()
		{
			TransactionHeaderCollection transactions = BatchPostingHelper.GetTransactionsForSplittingTest();
			TestOrg2 = BatchPostingHelper.TestOrg2;
			TestOrg3 = BatchPostingHelper.TestOrg3;
			TestAPInv2 = BatchPostingHelper.TestAPInv2;
			TestAPRec = BatchPostingHelper.TestAPRec;
			TestOrg6 = BatchPostingHelper.TestOrg6;
			TestOrg7 = BatchPostingHelper.TestOrg7;
			TestAPRec2 = BatchPostingHelper.TestAPRec2;
			TestAPInv3 = BatchPostingHelper.TestAPInv3;
			TestAPInv4 = BatchPostingHelper.TestAPInv4;
			TestAPInv5 = BatchPostingHelper.TestAPInv5;
			TestAPInv6 = BatchPostingHelper.TestAPInv6;
			return transactions;
		}

		TransactionHeaderCollection GetSingleTransactionForPosting()
		{
			TransactionHeaderCollection transactions = BatchPostingHelper.GetSingleTransactionForPosting();
			return transactions;
		}

		#endregion

		public void TestCodeDescriptionProperty()
		{
			APPaymentBatchPoster batch = Factory.New<APPaymentBatchPoster>();
			batch.APB_BatchNumber = "00001001";
			AssertEquals("Code Property Should be APB_BatchNumber because it's unique for AP Payment Batch", "00001001", ((CargoWise.Integration.ICodeDescription)batch).Code);
			AssertEquals("AP Payment Batch", ((CargoWise.Integration.ICodeDescription)batch).Description);
			AssertEquals("00001001 - AP Payment Batch", batch.HumanReadableShortcutName);
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesAPPaymentBatchPoster()
		{
			SetupDataForBaseTest();

			var localList = new List<string>
				{
					nameof(BatchPoster.PaymentItemsTotalAmount),
					nameof(BatchPoster.Balance),
					nameof(BatchPoster.DiscountAmount),
					nameof(BatchPoster.ExchangeDifferenceAmount)
				};

			var osList = new List<string>
				{
					nameof(BatchPoster.OSOverpaymentAmount)
				};
			BatchPoster.PaymentForBinding.MatchingBaseObject.AddMiscellaneousTransaction(Factory.New<APOverpayment>());

			var tester = new DecimalPlacesAttributeTester(BatchPoster);
			tester.CheckLocalCurrency(localList, nameof(BatchPoster.LocalDecimals));
			tester.CheckNonLocalCurrency(osList, nameof(BatchPoster.OSDecimals), nameof(BatchPoster.PaymentForBinding.MatchingBaseObject.OverpaymentCurrent.AH_RX_NKTransactionCurrency), BatchPoster.PaymentForBinding.MatchingBaseObject.OverpaymentCurrent);
		}

		public void TestPostMatchSuccessful()
		{
			SetupDataForPostingTest();

			AssertEquals("There should be 3 PaymentApproval objects created", 3, BatchPoster.PaymentApprovalCollection.Count);
			AssertEquals("Should be 1 transaction in MatchedTransactions", 1, BatchPoster.PaymentApprovalCollection[0].MatchingBaseObject.MatchedTransactions.Count);
			AssertEquals("Should be 1 transaction in MatchedTransactions", 1, BatchPoster.PaymentApprovalCollection[1].MatchingBaseObject.MatchedTransactions.Count);
			AssertEquals("Should be 1 transaction in MatchedTransactions", 1, BatchPoster.PaymentApprovalCollection[2].MatchingBaseObject.MatchedTransactions.Count);

			BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_AK = TestCheques.PK;
			AssertEquals("There cheque number should be set correctly", "000001", BatchPoster.PaymentApprovalCollection[0].AV_ChequeOrReference);
			AssertEquals("There cheque number should be set correctly", "000002", BatchPoster.PaymentApprovalCollection[1].AV_ChequeOrReference);
			AssertEquals("There cheque number should be set correctly", "000003", BatchPoster.PaymentApprovalCollection[2].AV_ChequeOrReference);
			AssertEquals("Payment amount should be defaulted", BatchPoster.PaymentApprovalCollection[0].AV_Amount, BatchPoster.PaymentApprovalCollection[0].AV_Calc_LocalAmount - BatchPoster.PaymentApprovalCollection[0].MatchingBaseObject.Balance);
			AssertEquals("Transaction Payment amount should be set", ((IMatching)TestAPInv).OSPartialPaymentAmount, ((IMatching)TestAPInv).OutstandingAmount);

			Assert("There should be error in collection", BatchPoster.HasErrors);
			BatchPostingHelper.AssertBatchPosterWillNotMatchAndPost(BatchPoster);

			BatchPoster.RunPreSaveValidation();
			Assert("There should be no errors in collection", !BatchPoster.HasErrors);
			BatchPostingHelper.AssertBatchPosterWillMatchAndPost(BatchPoster, 3, 3, 2);
		}

		public void TestAnErrorInThePaymentBatchCollectionWillPreventFromSaving()
		{
			SetupDataForBaseTest();

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			JobConsolCost cost = consol.GetApportionments().CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			cost.E6_OSCostAmount = 10m;
			cost.E6_LocalCostAmount = 10m;

			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_E6 = cost.PK;
			charge.JR_JH = job.PK;
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge.JR_AB = TestCheques.BankAccount.PK;
			charge.JR_ChequeNo = "000002";
			charge.JR_OSCostAmt = 10m;
			charge.JR_LocalCostAmt = 10m;

			SetupDataForPostingTest(false);

			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			BatchPoster.APB_AK = TestCheques.PK;
			BatchPoster.APB_ChequeOrReference = "000001";
			BatchPoster.RunPreSaveValidationCore_ForTestOnly();
			Assert("Valid Start Cheque Number", !BatchPoster.APB_ChequeOrReferenceInfo.HasErrors());

			Assert("Should contain the error as the second PaymentApproval in the batch contains Cheque Number error.", BatchPoster.HasErrors);
			AssertHasError(BatchPoster.PaymentApprovalCollection[1].AV_ChequeOrReferenceInfo, "Check number 000002 is already used on Consol " + consol.JK_UniqueConsignRef + ".");
			BatchPostingHelper.AssertBatchPosterWillNotMatchAndPost(BatchPoster);

			BatchPoster.APB_ChequeOrReference = "000003";
			BatchPostingHelper.AssertBatchPosterWillMatchAndPost(BatchPoster, -1, 3, -1);
		}

		public void TestEmptyBatchCanKeepSameChequeNumber()
		{
			SetupDataForPostingTest();

			const string chequeNumber = "000001";
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			BatchPoster.APB_AK = TestCheques.PK;
			BatchPoster.APB_ChequeOrReference = chequeNumber;
			Factory.Save();

			var approvalUseChequeNumber = BatchPoster.PaymentApprovalCollection[0];
			while (BatchPoster.PaymentApprovalCollection.Any())
			{
				BatchPoster.RemovePaymentFromBatch(BatchPoster.PaymentApprovalCollection[0]);
			}
			AssertEquals(chequeNumber, BatchPoster.APB_ChequeOrReference);
			AssertEquals(chequeNumber, approvalUseChequeNumber.ChequeOrReference);

			BatchPoster.RunPreSaveValidationCore_ForTestOnly();
			AssertEquals("check number 000001 is used in removed approval, but will keep it", false, BatchPoster.HasErrors);
		}

		public void TestPost1PaymentApprovalWith2Invoices()
		{
			SetupDataForBaseTest(false);

			ZDecimal invoice1OSAmount = 216m;
			ZDecimal invoice1ExchangeRate = 0.72m;
			ZDecimal invoice1LocalAmount = 300m;
			RefCurrency invoice1Currency = BatchPostingHelper.TestObjectCreator.USD;

			ZDecimal invoice2OSAmount = 375m;
			ZDecimal invoice2ExchangeRate = 0.75m;
			ZDecimal invoice2LocalAmount = 500m;
			RefCurrency invoice2Currency = BatchPostingHelper.TestObjectCreator.USD;

			ZDecimal approvalOSAmount = 600m;
			ZDecimal approvalExchangeRate = 0.75m;
			ZDecimal approvalLocalAmount = 800m;
			RefCurrency approvalCurrency = BatchPostingHelper.TestObjectCreator.USD;

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ZDateTime paymentDate = ZDateTime.Now.AddDays(-1);
			ZDateTime postDate = ZDateTime.Now.AddDays(-5);

			BatchPostingHelper.TestSaveReloadAndPostPaymentApprovalItems_Core(
						invoice1OSAmount, invoice1ExchangeRate, invoice1LocalAmount, invoice1Currency,
						invoice2OSAmount, invoice2ExchangeRate, invoice2LocalAmount, invoice2Currency,
						approvalOSAmount, approvalExchangeRate, approvalLocalAmount, approvalCurrency, paymentDate, postDate);
		}

		public void TestPostAsPaymentApprovals()
		{
			SetupDataForBaseTest(false);

			ZDecimal invoice1OSAmount = 216m;
			ZDecimal invoice1ExchangeRate = 0.72m;
			ZDecimal invoice1LocalAmount = 300m;
			RefCurrency invoice1Currency = BatchPostingHelper.TestObjectCreator.USD;

			ZDecimal invoice2OSAmount = 375m;
			ZDecimal invoice2ExchangeRate = 0.75m;
			ZDecimal invoice2LocalAmount = 500m;
			RefCurrency invoice2Currency = BatchPostingHelper.TestObjectCreator.USD;

			ZDecimal approvalOSAmount = 600m;
			ZDecimal approvalExchangeRate = 0.75m;
			ZDecimal approvalLocalAmount = 800m;
			RefCurrency approvalCurrency = BatchPostingHelper.TestObjectCreator.USD;

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ZDateTime paymentDate = ZDateTime.Now.AddDays(-1);
			ZDateTime postDate = ZDateTime.Now.AddDays(-5);

			BatchPostingHelper.TestSaveReloadAndPostPaymentApprovalItems_Core(
						invoice1OSAmount, invoice1ExchangeRate, invoice1LocalAmount, invoice1Currency,
						invoice2OSAmount, invoice2ExchangeRate, invoice2LocalAmount, invoice2Currency,
						approvalOSAmount, approvalExchangeRate, approvalLocalAmount, approvalCurrency, paymentDate, postDate,
						true);
		}

		public void TestGrouping()
		{
			SetupDataForBaseTest(false);

			TransactionHeaderCollection collection = GetTransactionsForSplittingTest();
			BatchPostingHelper.TestAPInv.AH_OH = BatchPostingHelper.TestAPInv2.AH_OH = new TestObjectCreator(Factory).AALSHI.PK;
			BatchPostingHelper.TestAPInv.AH_RequisitionStatus = "ABC";
			BatchPostingHelper.TestAPInv2.AH_RequisitionStatus = "XYZ";
			BatchPoster = APPaymentBatchPoster.Create_ForTestOnly(Factory, true, false, false);
			BatchPoster.SetDefaultValuesByTransactions(collection);
			BatchPoster.APB_AB = TestBank.PK;

			var paymentApproval = BatchPostingHelper.FindPaymentApprovalThatContainSpecificTransaction(BatchPoster.PaymentApprovalCollection, TestAPInv);
			AssertEquals("PaymentApproval1 should contain only 1 transaction", 1, paymentApproval.MatchingBaseObject.MatchedTransactions.Count);
			paymentApproval = BatchPostingHelper.FindPaymentApprovalThatContainSpecificTransaction(BatchPoster.PaymentApprovalCollection, TestAPInv2);
			AssertEquals("PaymentApproval2 should contain only 1 transaction", 1, paymentApproval.MatchingBaseObject.MatchedTransactions.Count);
		}

		#region Validation Tests

		public void TestAccountDetailsNotFound()
		{
			SetupDataForBaseTest();

			TestBank.AB_AllowAutoDDR = ZBool.True;
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_AK = TestCheques.PK;
			BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			BatchPoster.RunPreSaveValidation();

			Assert("BatchPoster should have error as the PaymentApproval in the batch has error on AV_OH property", BatchPoster.HasErrors);
			AssertHasErrors("Payee/OrgHeader should have errors", BatchPoster.PaymentApprovalCollection[0].AV_OHInfo);
			string expectedMessage = string.Format("An AP Bank Account could not be found with currency {0} and payment type DDR for the payee {1}.\r\n\r\nPlease set up an AP Account for the organization {1} under the AP Details tab, by right-clicking this grid and selecting \"Edit Payment Organization Detail\", with the currency {0} and payment type of DDR.", Core.Constants.CurrencyCodes.Australia, BatchPoster.PaymentApprovalCollection[0].Header.OH_Code);
			AssertContains("Incorrect error message", expectedMessage, BatchPoster.PaymentApprovalCollection[0].AV_OHInfo.GetErrors().GetFirstMessage());
		}

		bool IsCurrencyLocal(string currency)
		{
			return GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency == currency;
		}

		public void TestChangingBankAccountUpdateCurrencyForAllPayments()
		{
			SetupDataForSingleForeignCurrencyPaymentTest();

			BatchPoster.APB_AB = TestObjectCreator.GBPBankAccount.PK;
			Assert("bank account currency is foreign", !IsCurrencyLocal(BatchPoster.BankAccount.AB_RX_NKAccountCurrency));
			Assert("When Bank account currency is foreign, all payments are updated with the bank account currency", BatchPoster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().All(p => p.CurrencyCode == BatchPoster.BankAccount.AB_RX_NKAccountCurrency));

			BatchPoster.APB_AB = TestObjectCreator.AUDBankAccount.PK;
			Assert("bank account currency is local", IsCurrencyLocal(BatchPoster.BankAccount.AB_RX_NKAccountCurrency));

			var singleCurrencyPayment = BatchPoster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().First(x => x.MatchingBaseObject.MatchedTransactions.Cast<IMatching>().Select(y => y.CurrencyCode).Distinct().ToList().Count == 1);
			var multiCurrencyPayment = BatchPoster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().First(x => x.MatchingBaseObject.MatchedTransactions.Cast<IMatching>().Select(y => y.CurrencyCode).Distinct().ToList().Count > 1);
			Assert(singleCurrencyPayment.MatchingBaseObject.MatchedTransactions.Cast<IMatching>().AllSame(x => x.CurrencyCode));
			Assert(!multiCurrencyPayment.MatchingBaseObject.MatchedTransactions.Cast<IMatching>().AllSame(x => x.CurrencyCode));

			AssertEquals("If Bank account is local and all transactions use the same currency, then payment use the transaction currency", singleCurrencyPayment.MatchingBaseObject.MatchedTransactions.Cast<IMatching>().First().CurrencyCode, singleCurrencyPayment.CurrencyCode);
			AssertEquals("If Bank account is local and transactions use different currencies, then payment use local currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, multiCurrencyPayment.CurrencyCode);

			AssertEquals(TestObjectCreator.USD.Code, singleCurrencyPayment.CurrencyCode);
			AssertEquals(TestObjectCreator.AUD.Code, multiCurrencyPayment.CurrencyCode);
			PaymentApproval1.AV_Status = PaymentApprovalStatus.Cancelled;
			PaymentApproval2.AV_Status = PaymentApprovalStatus.Posted;
			BatchPoster.APB_AB = TestObjectCreator.GBPBankAccount.PK;
			AssertEquals("Not changed", TestObjectCreator.USD.Code, singleCurrencyPayment.CurrencyCode);
			AssertEquals("Not changed", TestObjectCreator.AUD.Code, multiCurrencyPayment.CurrencyCode);
		}

		public void TestAmountsFieldsUpDateCorrectly_DifferentForeignCurrencies()
		{
			SetupDataForBaseTest();

			var foreignCurrency1 = Factory.NewWithValidTestData<RefCurrency>();
			var foreignCurrency2 = Factory.NewWithValidTestData<RefCurrency>();
			var payment = BatchPoster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().First();
			MakePaymentAndTransactionUseDifferentCurrenciesForTesting(payment, foreignCurrency1, foreignCurrency2);

			TestBank.AB_RX_NKAccountCurrency = foreignCurrency2.Code;
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_AK = TestCheques.PK;
			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			var singleCurrencyList = payment.MatchingBaseObject.MatchedTransactions.Cast<IMatching>().Select(x => x.CurrencyCode).Distinct().ToList();
			payment.AV_Amount = 0;
			AssertEquals(1, singleCurrencyList.Count);
			AssertEquals(foreignCurrency1.Code, singleCurrencyList.First());
			AssertNotEquals(singleCurrencyList.First(), payment.AV_RX_NKPaymentCurrency);

			AssertEquals(-94m, payment.MatchedTransactionsBalanceWithPaymentAmount);
			AssertEquals(0m, payment.AV_Amount);
			AssertEquals(0m, payment.AV_Calc_LocalAmount);
			AssertEquals(0m, payment.AV_PayExRate);

			payment.AV_PayExRate = 2;
			AssertEquals(0m, payment.MatchedTransactionsBalanceWithPaymentAmount);
			AssertEquals(188m, payment.AV_Amount);
			AssertEquals(94m, payment.AV_Calc_LocalAmount);
			AssertEquals(2m, payment.AV_PayExRate);

			payment.AV_Amount = 47;
			AssertEquals(-70.5m, payment.MatchedTransactionsBalanceWithPaymentAmount);
			AssertEquals(47m, payment.AV_Amount);
			AssertEquals(23.5m, payment.AV_Calc_LocalAmount);
			AssertEquals(2m, payment.AV_PayExRate);

			payment.AV_PayExRate = 4;
			AssertEquals(-70.5m, payment.MatchedTransactionsBalanceWithPaymentAmount);
			AssertEquals(94m, payment.AV_Amount);
			AssertEquals(23.5m, payment.AV_Calc_LocalAmount);
			AssertEquals(4m, payment.AV_PayExRate);

			payment.AV_Amount = 100;
			AssertEquals(-69m, payment.MatchedTransactionsBalanceWithPaymentAmount);
			AssertEquals(100m, payment.AV_Amount);
			AssertEquals(25m, payment.AV_Calc_LocalAmount);
			AssertEquals(4m, payment.AV_PayExRate);

			payment.AV_Calc_LocalAmount = 50;
			AssertEquals(-44m, payment.MatchedTransactionsBalanceWithPaymentAmount);
			AssertEquals(100m, payment.AV_Amount);
			AssertEquals(50m, payment.AV_Calc_LocalAmount);
			AssertEquals(2m, payment.AV_PayExRate);

			payment.AV_PayExRate = 1;
			AssertEquals(-44m, payment.MatchedTransactionsBalanceWithPaymentAmount);
			AssertEquals(50m, payment.AV_Amount);
			AssertEquals(50m, payment.AV_Calc_LocalAmount);
			AssertEquals(1m, payment.AV_PayExRate);

			var foreignCurrency3 = Factory.NewWithValidTestData<RefCurrency>();
			var exrate2 = foreignCurrency3.ExchangeRates.AddNew();
			exrate2.RE_ExpiryDate = ZDateTime.Today;
			exrate2.RE_StartDate = ZDateTime.Today;
			exrate2.RE_ExRateType = nameof(ExchangeRateType.Buy);
			exrate2.RE_SellRate = 0.5m;
			Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();
			payment.AV_RX_NKPaymentCurrency = foreignCurrency3.RX_Code;
			AssertEquals(0m, payment.MatchedTransactionsBalanceWithPaymentAmount);
			AssertEquals(47m, payment.AV_Amount);
			AssertEquals(94m, payment.AV_Calc_LocalAmount);
			AssertEquals(0.5m, payment.AV_PayExRate);
		}

		public void TestAmountsFieldsUpDateCorrectly_SameForeignCurrencies()
		{
			SetupDataForBaseTest();

			var foreignCurrency1 = Factory.NewWithValidTestData<RefCurrency>();
			var payment = BatchPoster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().First();
			MakePaymentAndTransactienUseSameCurrencyForTesting(payment, foreignCurrency1);

			TestBank.AB_RX_NKAccountCurrency = foreignCurrency1.Code;
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_AK = TestCheques.PK;
			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			var singleCurrencyList = payment.MatchingBaseObject.MatchedTransactions.Cast<IMatching>().Select(x => x.CurrencyCode).Distinct().ToList();
			payment.AV_Amount = 0;
			AssertEquals(1, singleCurrencyList.Count);
			AssertEquals(foreignCurrency1.Code, singleCurrencyList.First());

			AssertEquals(-94m, payment.MatchedTransactionsBalanceWithPaymentAmount);
			AssertEquals(0m, payment.AV_Amount);
			AssertEquals(0m, payment.AV_Calc_LocalAmount);
			AssertEquals(0m, payment.AV_PayExRate);

			payment.AV_PayExRate = 4;
			AssertEquals(-94m, payment.MatchedTransactionsBalanceWithPaymentAmount);
			AssertEquals(0m, payment.AV_Amount);
			AssertEquals(0m, payment.AV_Calc_LocalAmount);
			AssertEquals(4m, payment.AV_PayExRate);

			payment.AV_Calc_LocalAmount = 94;
			AssertEquals(-94m, payment.MatchedTransactionsBalanceWithPaymentAmount);
			AssertEquals(0m, payment.AV_Amount);
			AssertEquals(0m, payment.AV_Calc_LocalAmount);
			AssertEquals(0m, payment.AV_PayExRate);

			payment.AV_PayExRate = 4;
			AssertEquals(-94m, payment.MatchedTransactionsBalanceWithPaymentAmount);
			AssertEquals(0m, payment.AV_Amount);
			AssertEquals(0m, payment.AV_Calc_LocalAmount);
			AssertEquals(4m, payment.AV_PayExRate);

			payment.AV_Amount = 94;
			AssertEquals(-70.5m, payment.MatchedTransactionsBalanceWithPaymentAmount);
			AssertEquals(94m, payment.AV_Amount);
			AssertEquals(23.5m, payment.AV_Calc_LocalAmount);
			AssertEquals(4m, payment.AV_PayExRate);

			payment.AV_PayExRate = 2;
			AssertEquals(-47m, payment.MatchedTransactionsBalanceWithPaymentAmount);
			AssertEquals(94m, payment.AV_Amount);
			AssertEquals(47m, payment.AV_Calc_LocalAmount);
			AssertEquals(2m, payment.AV_PayExRate);

			payment.AV_Amount = 188;
			AssertEquals(0m, payment.MatchedTransactionsBalanceWithPaymentAmount);
			AssertEquals(188m, payment.AV_Amount);
			AssertEquals(94m, payment.AV_Calc_LocalAmount);
			AssertEquals(2m, payment.AV_PayExRate);

			payment.AV_Calc_LocalAmount = 47;
			AssertEquals(-47m, payment.MatchedTransactionsBalanceWithPaymentAmount);
			AssertEquals(188m, payment.AV_Amount);
			AssertEquals(47m, payment.AV_Calc_LocalAmount);
			AssertEquals(4m, payment.AV_PayExRate);

			payment.AV_PayExRate = 1;
			AssertEquals(94m, payment.MatchedTransactionsBalanceWithPaymentAmount);
			AssertEquals(188m, payment.AV_Amount);
			AssertEquals(188m, payment.AV_Calc_LocalAmount);
			AssertEquals(1m, payment.AV_PayExRate);

			var foreignCurrency2 = Factory.NewWithValidTestData<RefCurrency>();
			var exrate2 = foreignCurrency2.ExchangeRates.AddNew();
			exrate2.RE_ExpiryDate = ZDateTime.Today;
			exrate2.RE_StartDate = ZDateTime.Today;
			exrate2.RE_ExRateType = nameof(ExchangeRateType.Buy);
			exrate2.RE_SellRate = 0.5m;
			Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();
			payment.AV_RX_NKPaymentCurrency = foreignCurrency2.RX_Code;
			AssertEquals(0m, payment.MatchedTransactionsBalanceWithPaymentAmount);
			AssertEquals(47m, payment.AV_Amount);
			AssertEquals(94m, payment.AV_Calc_LocalAmount);
			AssertEquals(0.5m, payment.AV_PayExRate);
		}

		public void TestPostDate_ReadOnly()
		{
			SetupDataForBaseTest();

			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();
			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			AssertEquals("Allowed to post future so editable", false, BatchPoster.APB_PostDateInfo.ReadOnly);

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;
			AssertEquals("Allowed to post future so editable", false, BatchPoster.APB_PostDateInfo.ReadOnly);

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			AssertEquals("Allowed to post past so editable", false, BatchPoster.APB_PostDateInfo.ReadOnly);
		}

		#endregion

		#region Public Method Tests

		public void TestSplitTransactionsToPaymentBatchAndPost()
		{
			SetupDataForBaseTest(false);

			var transactionHeaderCollection = GetTransactionsForSplittingTest();
			BatchPoster = Factory.New<APPaymentBatchPoster>();
			AssertEquals("BatchPoster should not have any changes so far", false, BatchPoster.HasChanges);

			BatchPoster.APB_AB = TestBank.PK;
			AssertEquals("BatchPoster should not have changes ", true, BatchPoster.HasChanges);

			BatchPoster.SetDefaultValuesByTransactions(transactionHeaderCollection);
			AssertEquals("BatchPoster changes should be reset by method SetDefaultsByTransactions", false, BatchPoster.HasChanges);
			AssertEquals("Should be 5 PaymentApprovals in collection", 5, BatchPoster.PaymentApprovalCollection.Count);
			var paymentApproval = BatchPostingHelper.FindPaymentApprovalThatContainSpecificTransaction(BatchPoster.PaymentApprovalCollection, TestAPInv);

			paymentApproval.AV_PayExRate = 3;
			Assert("BatchPoster should have changes as PaymentApproval now has, and it should be registered as Child object", BatchPoster.HasChanges);
			paymentApproval.AV_PayExRate = 1;

			BatchPoster.SetPaymentDetails(BatchPoster.PaymentApprovalCollection[0]);
			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_AK = TestCheques.PK;

			//Should be 4 Payments crated:
			//Payment 1: APInv1
			//Payment 2: APInv2, APInv3
			//Payment 3: APRec, APInv4, APRec2
			//Payment 4: APInv5
			//Payment 5: APInv6

			AssertNotNull("There should be a payment approval created, containing TestAPInv", paymentApproval);
			AssertEquals("PaymentApproval1 should contain only 1 transaction", 1, paymentApproval.MatchingBaseObject.MatchedTransactions.Count);
			AssertEquals("Organization on the PaymentApproval1 should be set correctly", TestOrg.PK, paymentApproval.AV_OH);
			Assert("AV_ABInfo should be set to read only as the SetIsCreatedFromPaymentBatchPoster() has been called", paymentApproval.AV_ABInfo.ReadOnly);
			AssertEquals("AV_Amount should be defaulted", paymentApproval.AV_Calc_LocalAmount - paymentApproval.MatchingBaseObject.Balance, paymentApproval.AV_Amount);

			paymentApproval = BatchPostingHelper.FindPaymentApprovalThatContainSpecificTransaction(BatchPoster.PaymentApprovalCollection, TestAPInv2);
			AssertNotNull("Should be a payment approval created, containing TestAPInv2", paymentApproval);
			AssertEquals("PaymentApproval2 should contain 2 transactions", 2, paymentApproval.MatchingBaseObject.MatchedTransactions.Count);
			Assert("PaymentApproval2 should contain TestAPInv3", paymentApproval.MatchingBaseObject.MatchedTransactions.Contains(TestAPInv3));
			AssertEquals("Organization on the PaymentApproval2 should be set correctly", TestOrg2.PK, paymentApproval.AV_OH);

			paymentApproval = BatchPostingHelper.FindPaymentApprovalThatContainSpecificTransaction(BatchPoster.PaymentApprovalCollection, TestAPRec);
			AssertNotNull("Should be a payment approval created, containing TestAPRec", TestAPRec);
			AssertEquals("PaymentApproval3 should contain 3 transactions", 3, paymentApproval.MatchingBaseObject.MatchedTransactions.Count);
			Assert("PaymentApproval3 should contain TestAPInv4", paymentApproval.MatchingBaseObject.MatchedTransactions.Contains(TestAPInv4));
			Assert("PaymentApproval3 should contain TestAPRec2", paymentApproval.MatchingBaseObject.MatchedTransactions.Contains(TestAPRec2));
			AssertEquals("Organization on the PaymentApproval3 should be set correctly", TestOrg3.PK, paymentApproval.AV_OH);

			paymentApproval = BatchPostingHelper.FindPaymentApprovalThatContainSpecificTransaction(BatchPoster.PaymentApprovalCollection, TestAPInv5);
			AssertNotNull("Should be a payment approval created, containing TestAPInv5", paymentApproval);
			AssertEquals("PaymentApproval4 should contain only 1 transaction", 1, paymentApproval.MatchingBaseObject.MatchedTransactions.Count);
			AssertEquals("Organization on the PaymentApproval4 should be set correctly", TestOrg6.PK, paymentApproval.AV_OH);

			paymentApproval = BatchPostingHelper.FindPaymentApprovalThatContainSpecificTransaction(BatchPoster.PaymentApprovalCollection, TestAPInv6);
			AssertNotNull("Should be a payment approval created, containing TestAPInv6", paymentApproval);
			AssertEquals("PaymentApproval5 should contain only 1 transaction", 1, paymentApproval.MatchingBaseObject.MatchedTransactions.Count);
			AssertEquals("Organization on the PaymentApproval4 should be set correctly", TestOrg7.PK, paymentApproval.AV_OH);

			BatchPostingHelper.AssertBatchPosterWillMatchAndPost(BatchPoster, 8, 5, -1);
		}

		public void TestCheckForDefaultBankAccounts()
		{
			SetupDataForBaseTest(false);

			Assert("TestOrg doesn't have default bank specified.", TestOrg.CompanyData.OB_AB_APDefaultBankAccount.IsEmpty);
			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.SetDefaultValuesByTransactions(GetSingleTransactionForPosting());
			BatchPoster.CheckForDefaultBankAccounts();
			AssertEquals("The Bank account should not be set as the TestOrg doesn't have default bank specified.", ZGuid.Empty, BatchPoster.APB_AB);

			TestOrg.CompanyData.OB_AB_APDefaultBankAccount = TestBank.PK;
			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.SetDefaultValuesByTransactions(GetSingleTransactionForPosting());
			BatchPoster.CheckForDefaultBankAccounts();
			AssertEquals("The Bank account should be defaulted to TestBank.", TestBank.PK, BatchPoster.APB_AB);

			BatchPoster = Factory.New<APPaymentBatchPoster>();
			var transactionsForPosting = BatchPostingHelper.GetTransactionsForPosting(false);
			AnotherBank = Factory.NewWithValidTestData<AccBankAccount>();
			BatchPostingHelper.TestOrg2.CompanyData.OB_AB_APDefaultBankAccount = AnotherBank.PK;
			BatchPoster.SetDefaultValuesByTransactions(transactionsForPosting);
			BatchPoster.OnSelectBankAccountFromDefault += new APPaymentBatchPoster.DefaultBankSelectionEventHandler(BatchPoster_OnSelectBankAccountFromDefault);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			BatchPoster.CheckForDefaultBankAccounts();
			AssertEquals("User should be asked to select the bank", "There is more than one Default Bank Account found for these SettlementGroups. " + System.Environment.NewLine + "Would you like to choose one?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("The Bank account should not be defaulted as answer is set to NO", ZGuid.Empty, BatchPoster.APB_AB);

			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.SetDefaultValuesByTransactions(transactionsForPosting);
			BatchPoster.OnSelectBankAccountFromDefault += new APPaymentBatchPoster.DefaultBankSelectionEventHandler(BatchPoster_OnSelectBankAccountFromDefault);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			BatchPoster.CheckForDefaultBankAccounts();
			AssertEquals("The Bank account should be defaulted to AnotherBank", AnotherBank.PK, BatchPoster.APB_AB);
		}

		public void TestSetPaymentDetails()
		{
			SetupDataForMultipleTransactionsTest();
			BatchPoster.SetPaymentDetails(null);
			AssertNull(BatchPoster.PaymentForBinding);
			AssertEquals("The MatchingCollection should be empty", 0, BatchPoster.MatchingCollection.Count);
			BatchPoster.SetPaymentDetails(BatchPoster.PaymentApprovalCollection[1]);
			AssertEquals("PaymentForBinding_ForTestOnly should be initialized as PaymentApproval2", BatchPoster.PaymentApprovalCollection[1], BatchPoster.PaymentForBinding);
			AssertEquals("MatchedTransactions collection should be set correctly", BatchPoster.PaymentApprovalCollection[1].MatchingBaseObject.MatchedTransactions.Count, BatchPoster.MatchingCollection.Count);
		}

		public void TestResetPaymentMatchingCollectionFromCurrentPayment()
		{
			SetupDataForBaseTest();
			BatchPoster.MatchingCollection.RemoveAll();
			AssertEquals("Matching collection should be empty", 0, BatchPoster.MatchingCollection.Count);
			BatchPoster.ResetPaymentMatchingCollectionFromCurrentPayment();
			AssertEquals("Matching collection should be reloaded", 1, BatchPoster.MatchingCollection.Count);
			Assert("Matching collection should contain TestAPInv", BatchPoster.MatchingCollection.Contains(TestAPInv));
		}

		public void TestResetPaymentMatchingCollection()
		{
			SetupDataForBaseTest();
			AssertEquals("Matching collection should contain 1 transaction", 1, BatchPoster.MatchingCollection.Count);
			BatchPoster.ResetPaymentMatchingCollection_ForTestOnly(null);
			AssertEquals("Matching collection should be empty", 0, BatchPoster.MatchingCollection.Count);
			BatchPoster.ResetPaymentMatchingCollection_ForTestOnly(BatchPoster.PaymentForBinding.MatchingBaseObject.MatchedTransactions);
			AssertEquals("Matching collection should be reloaded", 1, BatchPoster.MatchingCollection.Count);
		}

		public void TestRemoveTransactionFromPayment()
		{
			SetupDataForBaseTest(false);

			TestAPInv2 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInv2.AH_OH = TestOrg.PK;
			TestAPInv2.AH_LocalExTaxAmount = 94M;
			TestAPInv2.AH_OSExTaxAmount = 94M;
			TestAPInv2.AH_Desc = "For Payment Approval 2";

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory);
			transactions = GetSingleTransactionForPosting();
			transactions.Add(TestAPInv2);

			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.SetDefaultValuesByTransactions(transactions);
			BatchPoster.OnPaymentDeletedFromBatch += new EventHandler(BatchPostingHelper.BatchPoster_OnPaymentDeletedFromBatch);
			BatchPoster.SetPaymentDetails(BatchPoster.PaymentApprovalCollection[0]);
			AssertEquals("There should be 1 payment in the collection", 1, BatchPoster.PaymentApprovalCollection.Count);
			AssertEquals("There should be 1 approval linked to AccPaymentBatch", 1, BatchPoster.PaymentApprovalCollection.Count);
			AssertEquals("Matching collection should contain 2 transactions", 2, BatchPoster.MatchingCollection.Count);
			AssertEquals("MatchingBaseObject should contain 2 transactions too", 2, BatchPoster.PaymentForBinding.MatchingBaseObject.MatchedTransactions.Count);
			BatchPoster.RemoveTransactionFromPayment((TransactionHeader)BatchPoster.MatchingCollection[0]);
			AssertEquals("Matching collection should contain 1 transaction", 1, BatchPoster.MatchingCollection.Count);
			AssertEquals("MatchingBaseObject should contain 1 transaction", 1, BatchPoster.PaymentForBinding.MatchingBaseObject.MatchedTransactions.Count);
			Assert("Event should not have been fired yet", !BatchPostingHelper.OnPaymentDeletedFromBatch_EventFiredDuringTest);
			BatchPoster.RemoveTransactionFromPayment((TransactionHeader)BatchPoster.MatchingCollection[0]);
			AssertEquals("The payment should be deleted", 0, BatchPoster.PaymentApprovalCollection.Count);
			AssertEquals("The approval should be unlinked from AccPaymentBatch and removed from collection", 0, BatchPoster.PaymentApprovalCollection.Count);
			Assert("Event should have been fired", BatchPostingHelper.OnPaymentDeletedFromBatch_EventFiredDuringTest);
		}

		public void TestIsPaymentBatchEmpty()
		{
			BatchPoster = Factory.New<APPaymentBatchPoster>();
			AssertEquals("PaymentBatch should be empty", 0, BatchPoster.PaymentApprovalCollection.Count);
			Assert("ISPaymentBatchEmpty should return true", BatchPoster.ISPaymentBatchEmpty);
			APPaymentApprovalWithoutAuthorisation newPaymentApproval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			BatchPoster.PaymentApprovalCollection.Add(newPaymentApproval);
			Assert("ISPaymentBatchEmpty should return false", !BatchPoster.ISPaymentBatchEmpty);
		}

		public void TestRemovePaymentFromBatch()
		{
			SetupDataForBaseTest(false);

			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.SetDefaultValuesByTransactions(GetSingleTransactionForPosting());
			BatchPoster.OnPaymentDeletedFromBatch += new EventHandler(BatchPostingHelper.BatchPoster_OnPaymentDeletedFromBatch);
			BatchPoster.SetPaymentDetails(BatchPoster.PaymentApprovalCollection[0]);

			var payment = BatchPoster.PaymentApprovalCollection[0];
			AssertEquals(false, BatchPoster.IsInDatabase);
			AssertEquals(false, payment.IsInDatabase);
			AssertEquals("There should be 1 payment in the collection", 1, BatchPoster.PaymentApprovalCollection.Count);
			AssertEquals("There should be 1 approval linked to AccPaymentBatch", 1, BatchPoster.PaymentApprovalCollection.Count);
			Assert("Event should not have been fired yet", !BatchPostingHelper.OnPaymentDeletedFromBatch_EventFiredDuringTest);

			BatchPoster.RemovePaymentFromBatch(payment);

			AssertEquals("The payment should be removed", 0, BatchPoster.PaymentApprovalCollection.Count);
			AssertEquals("The payment should be deleted", true, payment.IsDeleted);
			using (((IBusinessObjectInternals)payment).SuppressReportRowDeletedError())
			{
				AssertEquals("The payment batch should be unlinked", ZGuid.Empty, payment.AV_APB_PaymentBatch);
			}
			AssertEquals("The approval should be unlinked from AccPaymentBatch and removed from collection", 0, BatchPoster.PaymentApprovalCollection.Count);
			Assert("Event should have been fired", BatchPostingHelper.OnPaymentDeletedFromBatch_EventFiredDuringTest);
		}

		public void TestRemovePaymentFromBatch_WhenPaymentIsInDatabase()
		{
			SetupDataForBaseTest();

			BatchPoster.APB_AK = TestCheques.PK;
			BatchPoster.OnPaymentDeletedFromBatch += new EventHandler(BatchPostingHelper.BatchPoster_OnPaymentDeletedFromBatch);
			BatchPostingHelper.AssertBatchPosterWillMatchAndPost(BatchPoster, 1, 1, 2);

			var payment = BatchPoster.PaymentApprovalCollection[0];
			AssertEquals(true, BatchPoster.IsInDatabase);
			AssertEquals(true, payment.IsInDatabase);
			AssertEquals("000001", payment.AV_ChequeOrReference);
			AssertEquals("There should be 1 payment in the collection", 1, BatchPoster.PaymentApprovalCollection.Count);
			AssertEquals("There should be 1 approval linked to AccPaymentBatch", 1, BatchPoster.PaymentApprovalCollection.Count);
			Assert("Event should not have been fired yet", !BatchPostingHelper.OnPaymentDeletedFromBatch_EventFiredDuringTest);

			payment.AV_ChequeOrReference = "000002";
			AssertEquals(true, BatchPoster.HasChanges);
			BatchPoster.SetHasChangesToFalse_ForTestOnly();
			AssertEquals(false, BatchPoster.HasChanges);
			BatchPoster.RemovePaymentFromBatch(payment);
			AssertEquals(true, BatchPoster.HasChanges);

			AssertEquals("The payment should be removed", 0, BatchPoster.PaymentApprovalCollection.Count);
			AssertEquals("The payment should not be deleted", false, payment.IsDeleted);
			AssertEquals("The payment batch should be unlinked", ZGuid.Empty, payment.AV_APB_PaymentBatch);
			AssertEquals("The changes in payment should be undo", "000001", payment.AV_ChequeOrReference);
			AssertEquals("The approval should be unlinked from AccPaymentBatch and removed from collection", 0, BatchPoster.PaymentApprovalCollection.Count);
			Assert("Event should have been fired", BatchPostingHelper.OnPaymentDeletedFromBatch_EventFiredDuringTest);
		}

		public void TestMatchTransactions()
		{
			SetupDataForMultipleTransactionsTest();
			Assert("Payment Batch should not be read only", !BatchPoster.PaymentApprovalCollection.ReadOnly);

			BatchPostingHelper.AssertBatchPosterWillMatchAndPost(BatchPoster, -1, 3, -1);

			AssertNotNull("Fully Paid Date on TestAPInv should not be null", TestAPInv.AH_FullyPaidDate);
			AssertEquals("Outstanding amount on TestAPInv should be 0", 0M, TestAPInv.AH_OutstandingAmount);
			Assert("The payment should be in matched transactions", BatchPoster.PaymentApprovalCollection[0].MatchingBaseObject.MatchedTransactions.Contains(BatchPoster.PaymentApprovalCollection[0]));

			AssertNotNull("Fully Paid Date on TestAPInv2 should not be null", TestAPInv2.AH_FullyPaidDate);
			AssertEquals("Outstanding amount on TestAPInv2 should be 0", 0M, TestAPInv2.AH_OutstandingAmount);
			Assert("The payment should be in matched transactions", BatchPoster.PaymentApprovalCollection[1].MatchingBaseObject.MatchedTransactions.Contains(BatchPoster.PaymentApprovalCollection[1]));

			//Assert("Payment Batch should be read only", BatchPoster.ReadOnly);
		}

		public void TestMatchTransactions_WhenPaymentApprovalIsCancelledOrPosted()
		{
			SetupDataForPostingTest();

			PaymentApproval1.AV_Status = PaymentApprovalStatus.Posted;
			PaymentApproval2.AV_Status = PaymentApprovalStatus.Cancelled;

			AssertContains("There is no data collected for this PK.", CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(PaymentApproval1.PK, CriticalValidationInfoCollectorServiceKeyType.PaymentApprovalBaseMatchDetails));
			AssertContains("There is no data collected for this PK.", CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(PaymentApproval2.PK, CriticalValidationInfoCollectorServiceKeyType.PaymentApprovalBaseMatchDetails));
			AssertContains("There is no data collected for this PK.", CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(PaymentApproval3.PK, CriticalValidationInfoCollectorServiceKeyType.PaymentApprovalBaseMatchDetails));

			BatchPoster.MatchTransactions();

			AssertContains("Did not call PaymentApprovalMatchingBase.Match for PaymentApproval1", "There is no data collected for this PK.", CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(PaymentApproval1.PK, CriticalValidationInfoCollectorServiceKeyType.PaymentApprovalBaseMatchDetails));
			AssertContains("Did not call PaymentApprovalMatchingBase.Match for PaymentApproval2", "There is no data collected for this PK.", CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(PaymentApproval2.PK, CriticalValidationInfoCollectorServiceKeyType.PaymentApprovalBaseMatchDetails));
			AssertNotContains("Called PaymentApprovalMatchingBase.Match for PaymentApproval3", "There is no data collected for this PK.", CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(PaymentApproval3.PK, CriticalValidationInfoCollectorServiceKeyType.PaymentApprovalBaseMatchDetails));
		}

		public void TestUpdatePaymentApprovalStatus_PaymentBatchIsPersisted()
		{
			SetupDataForBaseTest();
			Factory.Save();
			BatchPoster.PostPaymentsAsPaymentApprovals = true;
			AssertEquals("Precondition", true, BatchPoster.IsInDatabase);

			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettingsExample());
			BatchPoster.PaymentApprovalCollection[0].AV_Amount = 1500m;

			BatchPoster.UpdatePaymentApprovalStatus();

			BatchPoster.PaymentApprovalCollectionWithoutCancelledOrPosted.ForEach(x =>
				AssertEquals("Should NOT update the status of payment approvals when the batch is saved.", PaymentApprovalStatus.FullyApproved, x.AV_Status));
		}

		public void TestUpdatePaymentApprovalStatus_PostNewPaymentBatch()
		{
			SetupDataForBaseTest();
			BatchPoster.PostPaymentsAsPaymentApprovals = true;
			AssertEquals("Precondition", false, BatchPoster.IsInDatabase);

			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettingsExample());
			BatchPoster.PaymentApprovalCollection[0].AV_Amount = 1500m;

			BatchPoster.UpdatePaymentApprovalStatus();

			BatchPoster.PaymentApprovalCollectionWithoutCancelledOrPosted.ForEach(x =>
				AssertEquals("Should update the status of payment approvals when the batch is NOT saved.", PaymentApprovalStatus.AwaitingApproval, x.AV_Status));
		}

		public void TestUpdatePaymentApprovalStatus_SaveNewPaymentBatch()
		{
			SetupDataForBaseTest();
			BatchPoster.PostPaymentsAsPaymentApprovals = false;
			AssertEquals("Precondition", false, BatchPoster.IsInDatabase);

			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettingsExample());
			BatchPoster.PaymentApprovalCollection[0].AV_Amount = 1500m;

			BatchPoster.UpdatePaymentApprovalStatus();

			BatchPoster.PaymentApprovalCollectionWithoutCancelledOrPosted.ForEach(x =>
				AssertEquals("Should force approve all non-posted/cancelled payment approvals when save a new batch.", PaymentApprovalStatus.FullyApproved, x.AV_Status));
		}

		public void TestAllocationOrPrintingFailed()
		{
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			Factory.Save();
			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			BatchPoster.APB_AK = testChequeBook.PK;
			BatchPoster.APB_ChequeOrReference = "1";
			BatchPoster.ChequeBook.AK_IsActive = ZBool.False;
			BatchPoster.AllocationOrPrintingFailed();
			Assert("Cheque Book should be reloaded", BatchPoster.ChequeBook.AK_IsActive);
			Assert("ChequeOrReference field should be reset", BatchPoster.APB_ChequeOrReference.IsEmpty);
		}

		#endregion

		#region Properties Tests

		public void TestFundingBankAccount()
		{
			SetupDataForBaseTest();

			BatchPoster.APB_PaymentType = ReceiptTypes.EPayment;
			var testBank = TestObjectCreator.CHNBankAccount;
			AssertNotEquals("Pre-requisite", testBank.AB_RX_NKAccountCurrency, BatchPoster.LocalCurrency);
			AssertEquals("Funding Bank Account Currency should be local currency of login company when Funding bank account is empty.", BatchPoster.LocalCurrency, BatchPoster.FundingBankAccountCurrency);

			BatchPoster.APB_AB_FundingBankAccount = testBank.PK;
			AssertEquals("Pre-requisite", testBank, BatchPoster.FundingBankAccount);
			AssertEquals("Funding Bank Account Currency should be Funding bank account's currency.", testBank.AB_RX_NKAccountCurrency, BatchPoster.FundingBankAccountCurrency);

			BatchPoster.APB_PaymentType = ReceiptTypes.Cash;
			AssertEquals("Funding Bank Account should be set to empty when payment type is not EPA.", ZGuid.Empty, BatchPoster.APB_AB_FundingBankAccount);
		}

		public void TestFundingBankAccountValidation_ChangeCurrencyWithActiveDeal()
			=> AssertFundingBankAccountSavingError(
				TestObjectCreator.AUDBankAccount.PK,
				TestObjectCreator.CHNBankAccount.PK,
				hasActiveDeal: true,
				hasError: true
			);

		public void TestFundingBankAccountValidation_SameCurrencyWithActiveDeal()
			=> AssertFundingBankAccountSavingError(
				TestObjectCreator.AUDBankAccount.PK,
				TestObjectCreator.AUDBankAccount2.PK,
				hasActiveDeal: true,
				hasError: false
			);

		public void TestFundingBankAccountValidation_ChangeCurrencyWithInActiveDeal()
			=> AssertFundingBankAccountSavingError(
				TestObjectCreator.AUDBankAccount.PK,
				TestObjectCreator.CHNBankAccount.PK,
				hasActiveDeal: false,
				hasError: false
			);

		public void TestFundingBankAccountValidation_SameCurrencyWithInActiveDeal()
			=> AssertFundingBankAccountSavingError(
				TestObjectCreator.AUDBankAccount.PK,
				TestObjectCreator.AUDBankAccount2.PK,
				hasActiveDeal: false,
				hasError: false
			);

		public void TestFundingBankAccountValidation_ChangeCurrencyWithActiveDeal_EmptyFundingBankAccount()
			=> AssertFundingBankAccountSavingError(
				ZGuid.Empty,
				TestObjectCreator.CHNBankAccount.PK,
				hasActiveDeal: true,
				hasError: true
			);

		public void TestFundingBankAccountValidation_ChangeCurrencyWithActiveDeal_EmptyFundingBankAccount2()
			=> AssertFundingBankAccountSavingError(
				TestObjectCreator.CHNBankAccount.PK,
				ZGuid.Empty,
				hasActiveDeal: true,
				hasError: true
			);

		public void TestFundingBankAccountValidation_SameCurrencyWithActiveDeal_EmptyFundingBankAccount()
			=> AssertFundingBankAccountSavingError(
				ZGuid.Empty,
				ZGuid.Empty,
				hasActiveDeal: true,
				hasError: false
			);

		void AssertFundingBankAccountSavingError(
			ZGuid originalFundingBankAccount,
			ZGuid fundingBankAccount,
			bool hasActiveDeal,
			bool hasError)
		{
			SetupDataForBaseTest();

			BatchPoster.APB_PaymentType = ReceiptTypes.EPayment;
			BatchPoster.APB_AB_FundingBankAccount = originalFundingBankAccount;
			AssertEquals("PreRequisite", 1, BatchPoster.PaymentApprovalCollection.Count);
			AssertEquals("PreRequisite", 1, BatchPoster.PaymentApprovalCollectionWithoutCancelledOrPosted.Count());

			var paymentApprovalWithoutAuthorisationCollection = BatchPoster.PaymentApprovalCollection.OfType<APPaymentApprovalWithoutAuthorisation>().ToArray();
			TestObjectCreator.CreateValidEPaymentDealForStatus(hasActiveDeal ? EPaymentStatusCodes.Deal.Accepted : EPaymentStatusCodes.Deal.Cancelled, paymentApprovalWithoutAuthorisationCollection[0]);
			AssertNoNotifications(BatchPoster.APB_AB_FundingBankAccountInfo);
			AssertNoExceptionThrown(() => Factory.Save());

			BatchPoster.APB_AB_FundingBankAccount = fundingBankAccount;

			if (hasError)
			{
				AssertHasError(BatchPoster.APB_AB_FundingBankAccountInfo, "This payment batch has an active E-Payment Deal. Change of Funding Currency is not permitted.");
				var exception = AssertExceptionThrown<ZCannotSaveException>(() => Factory.Save());
				AssertEquals("Another user created an active E-Payment Deal for this payment batch. Change of Funding Currency is not permitted.", exception.Message);
			}
			else
			{
				var isDifferentCurrency = BatchPoster.OriginalFundingBankAccountCurrency != BatchPoster.FundingBankAccountCurrency;
				if (isDifferentCurrency)
				{
					AssertEquals("PreRequisite", 1, BatchPoster.EPaymentQuotesSummary.Count);
				}

				AssertNoNotifications(BatchPoster.APB_AB_FundingBankAccountInfo);
				AssertNoExceptionThrown(() => Factory.Save());

				if (isDifferentCurrency)
				{
					AssertEquals("Active quote should be discarded when funding bank currency changed.", EPaymentStatusCodes.Quote.Discarded, BatchPoster.PaymentApprovalCollectionWithoutCancelledOrPosted.First().CurrentDealQuote.QU_Status);
					AssertEquals("Summary should be refreshed.", 0, BatchPoster.EPaymentQuotesSummary.Count);
				}
			}
		}

		public void TestPostPaymentsAsPaymentApprovals()
		{
			AccountingConfigurationRegistry.Instance.PayInvoicesDefaultPostPaymentsAsPaymentApprovals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var posterwithdefaultTrue = Factory.New<APPaymentBatchPoster>();
			AssertEquals("PostPaymentsAsPaymentApprovals should default to PayInvoicesDefaultPostPaymentsAsPaymentApprovals registry setting", true, posterwithdefaultTrue.PostPaymentsAsPaymentApprovals);
			posterwithdefaultTrue.PostPaymentsAsPaymentApprovals = false;
			AssertEquals("PostPaymentsAsPaymentApprovals should be able to be overriden", false, posterwithdefaultTrue.PostPaymentsAsPaymentApprovals);

			AccountingConfigurationRegistry.Instance.PayInvoicesDefaultPostPaymentsAsPaymentApprovals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var posterwithdefaultFalse = Factory.New<APPaymentBatchPoster>();
			AssertEquals("PostPaymentsAsPaymentApprovals should default to PayInvoicesDefaultPostPaymentsAsPaymentApprovals registry setting", false, posterwithdefaultFalse.PostPaymentsAsPaymentApprovals);
			posterwithdefaultFalse.PostPaymentsAsPaymentApprovals = true;
			AssertEquals("PostPaymentsAsPaymentApprovals should be able to be overriden", true, posterwithdefaultFalse.PostPaymentsAsPaymentApprovals);
		}

		public void TestPaymentType()
		{
			SetupDataForBaseTest();
			AssertEquals("Precondition: Default receipt type is cheque", ReceiptTypes.Cheque, BatchPoster.APB_PaymentType);

			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_PaymentType = ReceiptTypes.Cash;
			AssertEquals("Bank account should be left the same", TestBank.PK, BatchPoster.APB_AB);
			AssertEquals("Cheque/Reference number should change to CASH", ReceiptTypes.Cash, BatchPoster.APB_ChequeOrReference);
			Assert("ChequeBook field should clear", BatchPoster.APB_AK.IsEmpty);
			Assert("ChequeBook should become readonly", BatchPoster.APB_AKInfo.ReadOnly);

			// Cheque
			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			AssertEquals("Bank account should be left the same", TestBank.PK, BatchPoster.APB_AB);
			Assert("Cheque/Reference number should become empty", BatchPoster.APB_ChequeOrReference.IsEmpty);
			// only for payment
			Assert("ChequeBook field should be editable", !BatchPoster.APB_AKInfo.ReadOnly);

			// CreditCard
			BatchPoster.APB_ChequeOrReference = "234";

			BatchPoster.APB_PaymentType = ReceiptTypes.CreditCard;
			Assert("ChequeOrReference should be cleared", BatchPoster.APB_ChequeOrReference.IsEmpty);
			Assert("ChequeBook field must be cleared", BatchPoster.APB_AK.IsEmpty);
			Assert("ChequeBook should be readonly", BatchPoster.APB_AKInfo.ReadOnly);

			// Direct Debit
			BatchPoster.APB_ChequeOrReference = "345";
			BatchPoster.APB_AK = TestCheques.PK;

			BatchPoster.APB_PaymentType = ReceiptTypes.DirectDebit;
			Assert("ChequeOrReference should be cleared", BatchPoster.APB_ChequeOrReference.IsEmpty);
			Assert("ChequeBook field must be cleared", BatchPoster.APB_AK.IsEmpty);
			Assert("ChequeBook should be readonly", BatchPoster.APB_AKInfo.ReadOnly);

			// Periodic Payment
			BatchPoster.APB_ChequeOrReference = "345";
			BatchPoster.APB_AK = TestCheques.PK;

			BatchPoster.APB_PaymentType = ReceiptTypes.EFT;
			Assert("ChequeOrReference should be cleared", BatchPoster.APB_ChequeOrReference.IsEmpty);
			Assert("ChequeBook field must be cleared", BatchPoster.APB_AK.IsEmpty);
			Assert("ChequeBook should be readonly", BatchPoster.APB_AKInfo.ReadOnly);

			BatchPoster.APB_ChequeOrReference = "345";
			BatchPoster.APB_AK = TestCheques.PK;

			BatchPoster.APB_PaymentType = ReceiptTypes.ScheduledEFT;
			Assert("ChequeOrReference should be cleared", BatchPoster.APB_ChequeOrReference.IsEmpty);
			Assert("ChequeBook field must be cleared", BatchPoster.APB_AK.IsEmpty);
			Assert("ChequeBook should be readonly", BatchPoster.APB_AKInfo.ReadOnly);

			BatchPoster.APB_ChequeOrReference = "345";
			BatchPoster.APB_AK = TestCheques.PK;

			BatchPoster.APB_PaymentType = ReceiptTypes.CollectionRequest;
			Assert("ChequeOrReference should be cleared", BatchPoster.APB_ChequeOrReference.IsEmpty);
			Assert("ChequeBook field must be cleared", BatchPoster.APB_AK.IsEmpty);
			Assert("ChequeBook should be readonly", BatchPoster.APB_AKInfo.ReadOnly);
		}

		public void TestSetPaymentTypeDoesChangeValueIfNotInDatabase()
		{
			SetupDataForBaseTest();
			BatchPoster.APB_PaymentType = ReceiptTypes.DirectDebitLine;
			BatchPoster.APB_ChequeOrReference = "TEST001";
			AssertEquals("TEST001", BatchPoster.APB_ChequeOrReference);

			BatchPoster.APB_PaymentType = ReceiptTypes.DirectDebit;
			AssertEquals("", BatchPoster.APB_ChequeOrReference);
		}

		public void TestUpdatePaymentTypeForAllPayments()
		{
			SetupDataForPostingTest();

			AssertEquals("PaymentType should be defaulted to CHQ", ReceiptTypes.Cheque, BatchPoster.APB_PaymentType);
			AssertEquals("PaymentApproval1 should have AV_PaymentType defaulted to CHQ", ReceiptTypes.Cheque, PaymentApproval1.AV_PaymentType);
			AssertEquals("PaymentApproval2 should have AV_PaymentType defaulted to CHQ", ReceiptTypes.Cheque, PaymentApproval2.AV_PaymentType);
			AssertEquals("PaymentApproval3 should have AV_PaymentType defaulted to CHQ", ReceiptTypes.Cheque, PaymentApproval3.AV_PaymentType);

			BatchPoster.APB_PaymentType = ReceiptTypes.Cash;
			AssertEquals("PaymentType should be set to CSH", ReceiptTypes.Cash, BatchPoster.APB_PaymentType);
			AssertEquals("PaymentApproval1 should have changed its AV_PaymentType", ReceiptTypes.Cash, PaymentApproval1.AV_PaymentType);
			AssertEquals("PaymentApproval2 should have changed its AV_PaymentType", ReceiptTypes.Cash, PaymentApproval2.AV_PaymentType);
			AssertEquals("PaymentApproval3 should have changed its AV_PaymentType", ReceiptTypes.Cash, PaymentApproval3.AV_PaymentType);

			BatchPoster.APB_PaymentType = ZString.Empty;
			AssertEquals("PaymentType should be empty", ZString.Empty, BatchPoster.APB_PaymentType);
			AssertEquals("PaymentApproval1.AV_PaymentType should be empty", ZString.Empty, PaymentApproval1.AV_PaymentType);
			AssertEquals("PaymentApproval2.AV_PaymentType should be empty", ZString.Empty, PaymentApproval2.AV_PaymentType);
			AssertEquals("PaymentApproval3.AV_PaymentType should be empty", ZString.Empty, PaymentApproval3.AV_PaymentType);

			BatchPoster.APB_PaymentType = "BLA";
			AssertEquals("PaymentType should change", "BLA", BatchPoster.APB_PaymentType);
			AssertEquals("PaymentApproval1.AV_PaymentType should be changed even if the value is invalid", "BLA", PaymentApproval1.AV_PaymentType);
			AssertEquals("PaymentApproval2.AV_PaymentType should be changed even if the value is invalid", "BLA", PaymentApproval2.AV_PaymentType);
			AssertEquals("PaymentApproval3.AV_PaymentType should be changed even if the value is invalid", "BLA", PaymentApproval3.AV_PaymentType);

			PaymentApproval1.AV_Status = PaymentApprovalStatus.Cancelled;
			PaymentApproval2.AV_Status = PaymentApprovalStatus.Posted;
			BatchPoster.APB_PaymentType = ReceiptTypes.Cash;
			AssertEquals("PaymentType should change", ReceiptTypes.Cash, BatchPoster.APB_PaymentType);
			AssertEquals("PaymentApproval1.AV_PaymentType should not be changed", "BLA", PaymentApproval1.AV_PaymentType);
			AssertEquals("PaymentApproval2.AV_PaymentType should not be changed", "BLA", PaymentApproval2.AV_PaymentType);
			AssertEquals("PaymentApproval3.AV_PaymentType should be changed", ReceiptTypes.Cash, PaymentApproval3.AV_PaymentType);
		}

		public void TestUpdatePostDateForAllPayments()
		{
			SetupDataForPostingTest();

			AssertEquals("PostDate should be defaulted", ZDateTime.Now.Date, BatchPoster.APB_PostDate.Date);
			AssertEquals("PaymentApproval1 should have AV_PostDate defaulted", ZDateTime.Now.Date, PaymentApproval1.AV_PostDate.Date);
			AssertEquals("PaymentApproval2 should have AV_PostDate defaulted", ZDateTime.Now.Date, PaymentApproval2.AV_PostDate.Date);
			AssertEquals("PaymentApproval3 should have AV_PostDate defaulted", ZDateTime.Now.Date, PaymentApproval3.AV_PostDate.Date);

			var newDateValue1 = ZDateTime.Now.AddDays(2);

			BatchPoster.APB_PostDate = newDateValue1;
			AssertEquals("PostDate should be set to new value", newDateValue1, BatchPoster.APB_PostDate);
			AssertEquals("PaymentApproval1 should have changed its AV_PostDate", newDateValue1, PaymentApproval1.AV_PostDate);
			AssertEquals("PaymentApproval2 should have changed its AV_PostDate", newDateValue1, PaymentApproval2.AV_PostDate);
			AssertEquals("PaymentApproval3 should have changed its AV_PostDate", newDateValue1, PaymentApproval3.AV_PostDate);

			PaymentApproval1.AV_Status = PaymentApprovalStatus.Cancelled;
			PaymentApproval2.AV_Status = PaymentApprovalStatus.Posted;
			var newDateValue2 = ZDateTime.Now.AddDays(4);
			BatchPoster.APB_PostDate = newDateValue2;
			AssertEquals("PostDate should be set to new value", newDateValue2, BatchPoster.APB_PostDate);
			AssertEquals("PaymentApproval1 should not have changed its AV_PostDate", newDateValue1, PaymentApproval1.AV_PostDate);
			AssertEquals("PaymentApproval2 should not have changed its AV_PostDate", newDateValue1, PaymentApproval2.AV_PostDate);
			AssertEquals("PaymentApproval3 should have changed its AV_PostDate", newDateValue2, PaymentApproval3.AV_PostDate);
		}

		public void TestUpdateDateForAllPayments()
		{
			SetupDataForPostingTest();

			AssertEquals("Date should be defaulted", ZDateTime.Now.Date, BatchPoster.APB_PaymentDate.Date);
			AssertEquals("PaymentApproval1 should have AV_PaymentDate defaulted", ZDateTime.Now.Date, PaymentApproval1.AV_PaymentDate.Date);
			AssertEquals("PaymentApproval2 should have AV_PaymentDate defaulted", ZDateTime.Now.Date, PaymentApproval2.AV_PaymentDate.Date);
			AssertEquals("PaymentApproval3 should have AV_PaymentDate defaulted", ZDateTime.Now.Date, PaymentApproval3.AV_PaymentDate.Date);

			var newDateValue1 = ZDateTime.Now.AddDays(2);

			BatchPoster.APB_PaymentDate = newDateValue1;
			AssertEquals("Date should be set to new value", newDateValue1, BatchPoster.APB_PaymentDate);
			AssertEquals("PaymentApproval1 should have changed its AV_PaymentDate", newDateValue1, PaymentApproval1.AV_PaymentDate);
			AssertEquals("PaymentApproval2 should have changed its AV_PaymentDate", newDateValue1, PaymentApproval2.AV_PaymentDate);
			AssertEquals("PaymentApproval3 should have changed its AV_PaymentDate", newDateValue1, PaymentApproval3.AV_PaymentDate);

			var newDateValue2 = ZDateTime.Now.AddDays(4);

			PaymentApproval1.AV_Status = PaymentApprovalStatus.Cancelled;
			PaymentApproval2.AV_Status = PaymentApprovalStatus.Posted;
			BatchPoster.APB_PaymentDate = newDateValue2;
			AssertEquals("Date should be set to new value", newDateValue2, BatchPoster.APB_PaymentDate);
			AssertEquals("PaymentApproval1 should not have changed its AV_PaymentDate", newDateValue1, PaymentApproval1.AV_PaymentDate);
			AssertEquals("PaymentApproval2 should not have changed its AV_PaymentDate", newDateValue1, PaymentApproval2.AV_PaymentDate);
			AssertEquals("PaymentApproval3 should have changed its AV_PaymentDate", newDateValue2, PaymentApproval3.AV_PaymentDate);
		}

		public void TestChangingBankAccountResetsChequeBook()
		{
			SetupDataForBaseTest();
			BatchPoster.APB_AB = ZGuid.Empty;
			var testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			BatchPoster.APB_AK = testChequeBook.PK;
			BatchPoster.APB_AB = TestBank.PK;
			Assert("Cheque Book should reset to empty when bank account is changed", BatchPoster.APB_AK.IsEmpty);
			BatchPoster.APB_AB = ZGuid.Empty;
			BatchPoster.APB_AK = TestCheques.PK;
			BatchPoster.APB_AB = TestBank.PK;
			AssertEquals("Cheque Book should not be reseted as the old value belong to the selected bank", TestCheques.PK, BatchPoster.APB_AK);
		}

		public void TestChangingBankAccountResetsChequeBookCollection()
		{
			SetupDataForBaseTest();

			RefCurrency currency1 = Factory.NewWithValidTestData<RefCurrency>();
			RefCurrency currency2 = Factory.NewWithValidTestData<RefCurrency>();

			AccBankAccount bank1 = Factory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount bank2 = Factory.NewWithValidTestData<AccBankAccount>();

			bank1.AB_RX_NKAccountCurrency = currency1.RX_Code;
			bank2.AB_RX_NKAccountCurrency = currency2.RX_Code;

			AccChequeBook cheques1 = Factory.NewWithValidTestData<AccChequeBook>();
			AccChequeBook cheques2 = Factory.NewWithValidTestData<AccChequeBook>();

			cheques1.AK_AB = bank1.PK;
			cheques1.AK_GB = GlbBranch.CurrentBranch.PK;
			cheques2.AK_AB = bank2.PK;
			cheques2.AK_GB = GlbBranch.CurrentBranch.PK;

			//Factory.Save();

			BatchPoster.APB_AB = bank1.PK;
			BatchPoster.ChequeBooks.Load();
			Assert("ChequeBook collection should contain Cheques1", BatchPoster.ChequeBooks.Contains(cheques1));
			Assert("ChequeBook collection should not contain Cheques2", !BatchPoster.ChequeBooks.Contains(cheques2));

			BatchPoster.APB_AB = bank2.PK;
			BatchPoster.ChequeBooks.Load();
			Assert("ChequeBook collection should contain Cheques2", BatchPoster.ChequeBooks.Contains(cheques2));
			Assert("ChequeBook collection should not contain Cheques1", !BatchPoster.ChequeBooks.Contains(cheques1));
		}

		public void TestUpdateBankAccountForAllPayments()
		{
			SetupDataForPostingTest();

			AccBankAccount newBank = Factory.NewWithValidTestData<AccBankAccount>();

			BatchPoster.APB_AB = newBank.PK;
			AssertEquals("BankAccountPK should be set to new value", newBank.PK, BatchPoster.APB_AB);
			AssertEquals("PaymentApproval1 should have changed its AV_AB", newBank.PK, PaymentApproval1.AV_AB);
			AssertEquals("PaymentApproval2 should have changed its AV_AB", newBank.PK, PaymentApproval2.AV_AB);
			AssertEquals("PaymentApproval3 should have changed its AV_AB", newBank.PK, PaymentApproval3.AV_AB);

			BatchPoster.APB_AB = ZGuid.Empty;
			AssertEquals("BankAccountPK should be empty", ZGuid.Empty, BatchPoster.APB_AB);
			AssertEquals("PaymentApproval1.AV_AB should be empty", ZGuid.Empty, PaymentApproval1.AV_AB);
			AssertEquals("PaymentApproval2.AV_AB should be empty", ZGuid.Empty, PaymentApproval2.AV_AB);
			AssertEquals("PaymentApproval3.AV_AB should be empty", ZGuid.Empty, PaymentApproval3.AV_AB);

			BatchPoster.APB_AB = ZGuid.Invalid;
			Assert("BankAccountPK should change", !BatchPoster.APB_AB.IsValid);
			Assert("PaymentApproval1.AV_AB should be changed even if the value is invalid", !PaymentApproval1.AV_AB.IsValid);
			Assert("PaymentApproval2.AV_AB should be changed even if the value is invalid", !PaymentApproval2.AV_AB.IsValid);
			Assert("PaymentApproval3.AV_AB should be changed even if the value is invalid", !PaymentApproval3.AV_AB.IsValid);

			PaymentApproval1.AV_Status = PaymentApprovalStatus.Cancelled;
			PaymentApproval2.AV_Status = PaymentApprovalStatus.Posted;
			BatchPoster.APB_AB = newBank.PK;
			AssertEquals("BankAccountPK should be set to new value", newBank.PK, BatchPoster.APB_AB);
			AssertEquals("PaymentApproval1.AV_AB should not be changed", ZGuid.Invalid, PaymentApproval1.AV_AB);
			AssertEquals("PaymentApproval2.AV_AB should not be changed", ZGuid.Invalid, PaymentApproval2.AV_AB);
			AssertEquals("PaymentApproval3.AV_AB should be changed", newBank.PK, PaymentApproval3.AV_AB);
		}

		public void TestChangingChequeBookPKChangesChequeOrReference()
		{
			SetupDataForBaseTest();
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_PaymentType = ReceiptTypes.Cash;
			BatchPoster.APB_ChequeOrReference = "BLAH!";
			AssertEquals("ChequeOrReference should be set", "BLAH!", BatchPoster.APB_ChequeOrReference);
			BatchPoster.APB_AK = TestCheques.PK;
			AssertEquals("ChequeOrReference should not be changed", "BLAH!", BatchPoster.APB_ChequeOrReference);
			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			AssertEquals("ChequeOrReference should be reseted", ZString.Empty, BatchPoster.APB_ChequeOrReference);
			BatchPoster.APB_AK = TestCheques.PK;
			AssertEquals("ChequeOrReference should be changed", "000001", BatchPoster.APB_ChequeOrReference);
			BatchPoster.APB_AK = ZGuid.Invalid;
			Assert("ChequeOrReference should be empty", BatchPoster.APB_ChequeOrReference.IsEmpty);
		}

		public void TestUpdateChequeBookPKForAllPayments()
		{
			SetupDataForPostingTest();

			BatchPoster.APB_AK = TestCheques.PK;
			AssertEquals("ChequeBookPK should be set to new value", TestCheques.PK, BatchPoster.APB_AK);
			AssertEquals("PaymentApproval1 should have changed its AV_AB", TestCheques.PK, PaymentApproval1.AV_AK);
			AssertEquals("PaymentApproval2 should have changed its AV_AB", TestCheques.PK, PaymentApproval2.AV_AK);
			AssertEquals("PaymentApproval3 should have changed its AV_AB", TestCheques.PK, PaymentApproval3.AV_AK);

			BatchPoster.APB_AK = ZGuid.Empty;
			AssertEquals("ChequeBookPK should be empty", ZGuid.Empty, BatchPoster.APB_AK);
			AssertEquals("PaymentApproval1.AV_AK should be empty", ZGuid.Empty, PaymentApproval1.AV_AK);
			AssertEquals("PaymentApproval2.AV_AK should be empty", ZGuid.Empty, PaymentApproval2.AV_AK);
			AssertEquals("PaymentApproval3.AV_AK should be empty", ZGuid.Empty, PaymentApproval3.AV_AK);

			BatchPoster.APB_AK = ZGuid.Invalid;
			Assert("ChequeBookPK should change", !BatchPoster.APB_AK.IsValid);
			Assert("PaymentApproval1.AV_AK should be changed even if the value is invalid", !PaymentApproval1.AV_AK.IsValid);
			Assert("PaymentApproval2.AV_AK should be changed even if the value is invalid", !PaymentApproval2.AV_AK.IsValid);
			Assert("PaymentApproval3.AV_AK should be changed even if the value is invalid", !PaymentApproval3.AV_AK.IsValid);

			PaymentApproval1.AV_Status = PaymentApprovalStatus.Cancelled;
			PaymentApproval2.AV_Status = PaymentApprovalStatus.Posted;
			BatchPoster.APB_AK = TestCheques.PK;
			AssertEquals("ChequeBookPK should be empty", TestCheques.PK, BatchPoster.APB_AK);
			AssertEquals("PaymentApproval1.AV_AK should not have changed", ZGuid.Invalid, PaymentApproval1.AV_AK);
			AssertEquals("PaymentApproval2.AV_AK should not have changed", ZGuid.Invalid, PaymentApproval2.AV_AK);
			AssertEquals("PaymentApproval3.AV_AK should have changed", TestCheques.PK, PaymentApproval3.AV_AK);
		}

		void AssertCurrencySummaryRow(string currency, decimal exRate, bool readOnly)
		{
			var currencySummaryRow = BatchPoster.CurrencySummary.SummaryRows.GetSummaryRow(currency);
			AssertEquals(currency, currencySummaryRow.Currency);
			AssertEquals(exRate, currencySummaryRow.ExchangeRate);
			AssertEquals(readOnly, currencySummaryRow.ExchangeRateInfo.ReadOnly);
		}

		public void TestUpdatingPaymentsCurrencyUpdateCurrencySummaryRows()
		{
			SetupDataForMultipleTransactionsTest();
			BatchPoster.APB_PaymentType = ReceiptTypes.Cash;
			BatchPoster.APB_AB = TestBank.PK;
			var currencySummaryRows = BatchPoster.CurrencySummary.SummaryRows;

			Assert("all payments are AUD", BatchPoster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().All(x => x.CurrencyCode == TestObjectCreator.AUD.RX_Code));
			AssertEquals("Only one line in the currency summary", 1, currencySummaryRows.Count);
			AssertCurrencySummaryRow(TestObjectCreator.AUD.RX_Code, 1m, true);

			BatchPoster.PaymentForBinding = BatchPoster.PaymentApprovalCollection[0];
			BatchPoster.PaymentApprovalCollection[0].AV_RX_NKPaymentCurrency = TestObjectCreator.USD.RX_Code;

			AssertEquals(2, currencySummaryRows.Count);
			AssertCurrencySummaryRow(TestObjectCreator.AUD.RX_Code, 1m, true);
			AssertCurrencySummaryRow(TestObjectCreator.USD.RX_Code, 0.75m, false);

			BatchPoster.PaymentForBinding = BatchPoster.PaymentApprovalCollection[2];
			BatchPoster.PaymentApprovalCollection[2].AV_RX_NKPaymentCurrency = TestObjectCreator.EUR.RX_Code;

			AssertEquals(3, currencySummaryRows.Count);
			AssertCurrencySummaryRow(TestObjectCreator.AUD.RX_Code, 1m, true);
			AssertCurrencySummaryRow(TestObjectCreator.USD.RX_Code, 0.75m, false);
			AssertCurrencySummaryRow(TestObjectCreator.EUR.RX_Code, 0m, false);

			BatchPoster.PaymentApprovalCollection[2].AV_RX_NKPaymentCurrency = TestObjectCreator.USD.RX_Code;

			AssertEquals(2, currencySummaryRows.Count);
			AssertCurrencySummaryRow(TestObjectCreator.AUD.RX_Code, 1m, true);
			AssertCurrencySummaryRow(TestObjectCreator.USD.RX_Code, 0.75m, false);
		}

		public void TestUpdatingBankAccountCurrencyUpdateCurrencySummaryRows()
		{
			SetupDataForMultipleTransactionsTest();
			BatchPoster.APB_PaymentType = ReceiptTypes.Cash;
			BatchPoster.APB_AB = TestBank.PK;

			var currencySummaryRows = BatchPoster.CurrencySummary.SummaryRows;

			BatchPoster.PaymentForBinding = BatchPoster.PaymentApprovalCollection[1];
			BatchPoster.PaymentApprovalCollection[1].AV_RX_NKPaymentCurrency = TestObjectCreator.USD.RX_Code;

			AssertEquals(2, currencySummaryRows.Count);
			AssertCurrencySummaryRow(TestObjectCreator.AUD.RX_Code, 1m, true);
			AssertCurrencySummaryRow(TestObjectCreator.USD.RX_Code, 0.75m, false);

			BatchPoster.APB_AB = TestObjectCreator.USDBankAccount.PK;
			Assert("all payments are USD", BatchPoster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().All(x => x.CurrencyCode == TestObjectCreator.USD.RX_Code));

			AssertEquals(1, currencySummaryRows.Count);
			AssertCurrencySummaryRow(TestObjectCreator.USD.RX_Code, 0.75m, false);
		}

		public void TestCurrencySettingsOnPayments()
		{
			SetupDataForMultipleTransactionsTest();
			BatchPoster.APB_PaymentType = ReceiptTypes.Cash;
			BatchPoster.APB_AB = TestBank.PK;
			var payment1 = BatchPoster.PaymentApprovalCollection[0];

			payment1.AV_RX_NKPaymentCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals(1m, payment1.AV_PayExRate);
			Assert("exRate is readOnly when the currency is local", payment1.AV_PayExRateInfo.ReadOnly);
			Assert("local payment amount is readOnly when the currency is local", payment1.AV_Calc_LocalAmountInfo.ReadOnly);
			Assert("OS payment amount is always editable", !payment1.AV_AmountInfo.ReadOnly);

			payment1.AV_RX_NKPaymentCurrency = TestObjectCreator.USD.RX_Code;
			AssertEquals("use Today's rate", 0.75m, payment1.AV_PayExRate);
			Assert("exRate is editable when the currency is foreign", !payment1.AV_PayExRateInfo.ReadOnly);
			Assert("local payment amount is editable when the currency is foreign", !payment1.AV_Calc_LocalAmountInfo.ReadOnly);
			Assert("OS payment amount is always editable", !payment1.AV_AmountInfo.ReadOnly);

			payment1.AV_RX_NKPaymentCurrency = TestObjectCreator.EUR.RX_Code;
			AssertEquals("default is zero if there is no Today's rate", 0m, payment1.AV_PayExRate);
			Assert("exRate is editable when the currency is foreign", !payment1.AV_PayExRateInfo.ReadOnly);
			Assert("local payment amount is editable when the currency is foreign", !payment1.AV_Calc_LocalAmountInfo.ReadOnly);
			Assert("OS payment amount is always editable", !payment1.AV_AmountInfo.ReadOnly);
		}

		public void TestSet_ChequeOrReference_CASH()
		{
			SetupDataForBaseTest();
			AssertEquals("Message should not be shown during setting defaults", null, UnitTestUserNotification.Instance.LastMessage.Text);

			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_PaymentType = ReceiptTypes.Cash;
			BatchPoster.APB_ChequeOrReference = "1";
			BatchPoster.PaymentApprovalCollection[0].AV_ChequeOrReference = "2";
			BatchPoster.APB_ChequeOrReference = "3";
			AssertEquals("Message should not be shown as there is only one payment approval", null, UnitTestUserNotification.Instance.LastMessage.Text);

			SetupDataForPostingTest(false);
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_PaymentType = ReceiptTypes.Cash;
			BatchPoster.APB_ChequeOrReference = "1";
			BatchPoster.PaymentApprovalCollection[2].AV_ChequeOrReference = "2";
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			BatchPoster.APB_ChequeOrReference = "3";
			AssertEquals("Message should be shown as one of the payment approvals is different from another", "Are you sure you want to reset the Reference number for all payments using the new Start Reference No?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("ChequeOrReference should not be updated as the answer was NO", "1", BatchPoster.APB_ChequeOrReference);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			BatchPoster.APB_ChequeOrReference = "3";
			AssertEquals("ChequeOrReference should be updated as the answer was YES", "3", BatchPoster.APB_ChequeOrReference);
			AssertEquals("ChequeOrReference should be updated as the answer was YES", "3", BatchPoster.PaymentApprovalCollection[2].AV_ChequeOrReference);
			BatchPoster.APB_ChequeOrReference = "";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			BatchPoster.APB_ChequeOrReference = "1";
			AssertEquals("Message should not be shown as all PaymentApprovals have empty ChequeOrReference", null, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSet_ChequeOrReference_CHEQUE()
		{
			SetupDataForBaseTest();
			AssertEquals("Message should not be shown during setting defaults", null, UnitTestUserNotification.Instance.LastMessage.Text);

			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			BatchPoster.APB_ChequeOrReference = "1";
			AssertEquals("Value should be padded with zeros", "000001", BatchPoster.APB_ChequeOrReference);
			BatchPoster.PaymentApprovalCollection[0].AV_ChequeOrReference = "2";
			BatchPoster.APB_ChequeOrReference = "3";
			AssertEquals("Message should not be shown as there is only one payment approval", null, UnitTestUserNotification.Instance.LastMessage.Text);

			SetupDataForPostingTest(false);
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			BatchPoster.APB_ChequeOrReference = "1";
			BatchPoster.PaymentApprovalCollection[2].AV_ChequeOrReference = "2";
			BatchPoster.PaymentApprovalCollection[1].AV_ChequeOrReference = "3";
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			BatchPoster.APB_ChequeOrReference = "3";
			AssertEquals("Message should be shown as PaymentApproval 2 has bigger Cheque number than PaymentApproval 3", "Are you sure you want to recalculate the Check number for all payments using the new Start Reference No?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("ChequeOrReference should not be updated as the answer was NO", "000001", BatchPoster.APB_ChequeOrReference);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			BatchPoster.APB_ChequeOrReference = "3";
			AssertEquals("ChequeOrReference should be updated as the answer was YES", "000003", BatchPoster.APB_ChequeOrReference);
			AssertEquals("ChequeOrReference should be updated as the answer was YES", "000005", BatchPoster.PaymentApprovalCollection[2].AV_ChequeOrReference);
			BatchPoster.APB_ChequeOrReference = "";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			BatchPoster.APB_ChequeOrReference = "1";
			AssertEquals("Message should not be shown as all PaymentApprovals have empty ChequeOrReference", null, UnitTestUserNotification.Instance.LastMessage.Text);
			BatchPoster.PaymentApprovalCollection[2].AV_ChequeOrReference = "000004";
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			BatchPoster.APB_ChequeOrReference = "2";
			AssertEquals("Message should be shown as PaymentApproval3' ChequeOrReference is different from PaymentApproval2' by 2 points", "Are you sure you want to recalculate the Check number for all payments using the new Start Reference No?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("ChequeOrReference should be updated as the answer was YES", "000002", BatchPoster.APB_ChequeOrReference);
			AssertEquals("ChequeOrReference should be updated as the answer was YES", "000004", BatchPoster.PaymentApprovalCollection[2].AV_ChequeOrReference);
		}

		public void TestNoExceptionThrown_AskUserIfNeededBeforeChequeOrReferenceUpdate()
		{
			SetupDataForPostingTest();

			BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_AK = TestCheques.PK;
			BatchPoster.APB_ChequeOrReference = "1";
			BatchPoster.MatchTransactions();
			BatchPoster.PostPaymentsAsPaymentApprovals = true;
			Factory.Save();

			AssertEquals(ReceiptTypes.Cheque, BatchPoster.APB_PaymentType);

			PaymentApproval1.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			PaymentApproval1.AV_ChequeOrReference = "Cash";
			Factory.Save();

			AssertNoExceptionThrown(() =>
			{
				BatchPoster.APB_ChequeOrReference = "2";
			});
		}

		public void TestTotalEpaymentCostAmount()
		{
			SetupDataForBaseTest();

			BatchPoster.PaymentApprovalCollection.RemoveAll();
			var approval1 = (APPaymentApprovalWithoutAuthorisation)TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithoutAuthorisation), ReceiptTypes.EFT, TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
			approval1.AV_Amount = 100m;
			var deal1 = TestObjectCreator.CreateEPaymentDeal(approval1);
			deal1.Quote.QU_FeeAmount = 10m;

			BatchPoster.PaymentApprovalCollection.Add(approval1);
			Factory.Save();

			AssertEquals(1, BatchPoster.PaymentApprovalCollection.Count);
			AssertEquals("Index 0", approval1, BatchPoster.PaymentApprovalCollection[0]);
			AssertEquals("approval1:(100 + 10)", 110m, BatchPoster.TotalEPaymentCostAmount);

			var approval2 = (APPaymentApprovalWithoutAuthorisation)TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithoutAuthorisation), ReceiptTypes.EFT, TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
			approval2.AV_Amount = 88m;
			var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus("ACP", approval2);
			quote2.QU_FeeAmount = 0.5;
			BatchPoster.PaymentApprovalCollection.Add(approval2);
			Factory.Save();

			AssertEquals(2, BatchPoster.PaymentApprovalCollection.Count);
			AssertEquals("Index 1", approval2, BatchPoster.PaymentApprovalCollection[1]);
			AssertEquals("approval1(100 + 10), approval2(0)", 110m, BatchPoster.TotalEPaymentCostAmount);

			var deal2 = TestObjectCreator.CreateEPaymentDeal(approval2, quote2);
			quote2.QU_FeeAmount = 0.8;
			Factory.Save();

			AssertEquals("approval1(100 + 10), approval2 (88 + 0.8)", 110m + 88.8m, BatchPoster.TotalEPaymentCostAmount);

			var approval3 = (APPaymentApprovalWithoutAuthorisation)TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithoutAuthorisation), ReceiptTypes.EFT, TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
			approval3.AV_Amount = 200m;
			var deal3_1 = TestObjectCreator.CreateEPaymentDeal(approval3);
			deal3_1.Quote.QU_FeeAmount = 6m;
			deal3_1.AED_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-5);
			BatchPoster.PaymentApprovalCollection.Add(approval3);
			Factory.Save();

			AssertEquals(3, BatchPoster.PaymentApprovalCollection.Count);
			AssertEquals("Index 2", approval3, BatchPoster.PaymentApprovalCollection[2]);
			AssertEquals("approval1(100 + 10), approval2(88 + 0.8), approval3(200 + 6)", 110m + 88.8m + 206m, BatchPoster.TotalEPaymentCostAmount);

			var deal3_2 = TestObjectCreator.CreateEPaymentDeal(approval3);
			deal3_2.Quote.QU_FeeAmount = 8m;
			deal3_2.AED_SystemCreateTimeUtc = ZDateTime.Now;
			Factory.Save();

			AssertEquals("approval1(100 + 10), approval2(88 + 0.8), approval3(200 + 8)", 110m + 88.8m + 208m, BatchPoster.TotalEPaymentCostAmount);
		}

		public void TestPopulateChequeNumberFromChequeBook()
		{
			SetupDataForBaseTest();

			TestBank.AB_ChequeNumDigits = (ZByte)5;
			TestBank.AB_GC = GlbCompany.CurrentCompany.PK;
			TestCheques.AK_CurrentNo = 34;

			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_AK = TestCheques.PK;  // cheque number should default to 34

			AssertEquals("Cheque Number should default to the current number in the DB for the selected cheque book",
				"00034", BatchPoster.APB_ChequeOrReference);

			BatchPoster.APB_PaymentType = ReceiptTypes.Cash;

			AssertEquals("Bank should remain", TestBank.PK, BatchPoster.APB_AB);
			AssertEquals("Cheque Book should be cleared", ZGuid.Empty, BatchPoster.APB_AK);

			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			BatchPoster.APB_AK = TestCheques.PK;

			AssertEquals("Cheque Number should default to the current number in the DB for the selected cheque book",
				"00034", BatchPoster.APB_ChequeOrReference);
		}

		public void TestSavingChequeNumberGreaterThanCurrentNumber()
		{
			SetupDataForMultipleTransactionsTest();

			TestBank.AB_GC = GlbCompany.CurrentCompany.PK;
			TestCheques.AK_CurrentNo = 66;
			TestCheques.AK_GB = GlbBranch.CurrentBranch.PK;

			// Cheque num should default to 66
			BatchPoster.APB_ChequeOrReference = "77";
			BatchPostingHelper.BatchPoster = BatchPoster;
			BatchPostingHelper.AssertBatchPosterWillMatchAndPost(BatchPoster, -1, 3, -1);

			TestCheques.Reload();
			AssertEquals("Current number should become 77 + 3", 80m, TestCheques.AK_CurrentNo);
		}

		[SuspendCriticalValidation]
		public void TestCurrentNumberStaysSameIfEnteredNumberBelongsToCancelledPayment()
		{
			SetupDataForBaseTest();

			TestBank.AB_ChequeNumDigits = (ZByte)5;
			TestBank.AB_GC = GlbCompany.CurrentCompany.PK;
			TestCheques.AK_CurrentNo = 34;
			Factory.Save();

			// Create a cancelled payment
			APPayment testAPPay = Factory.NewWithValidTestData<APPayment>();
			testAPPay.AH_AB = TestBank.PK;
			testAPPay.ChequeBook = TestCheques.PK;
			testAPPay.AH_IsCancelled = true;
			((IMatching)testAPPay).CurrentMatchGroup.AddNew().AP_AH = testAPPay.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(testAPPay);

			Factory.Save();

			TestCheques.Reload();
			AssertEquals("Cheque number should be 00034", "00034", testAPPay.AH_ChequeOrReference);
			AssertEquals("Current number should increment", 35m, TestCheques.AK_CurrentNo);

			TestCheques.AK_CurrentNo = 67;
			//Factory.Save();

			TestCheques.AK_GB = GlbBranch.CurrentBranch.PK;
			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.SetDefaultValuesByTransactions(GetSingleTransactionForPosting());
			BatchPoster.SetPaymentDetails(BatchPoster.PaymentApprovalCollection[0]);
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_AK = TestCheques.PK;
			BatchPoster.APB_ChequeOrReference = "34";

			BatchPostingHelper.AssertBatchPosterWillMatchAndPost(BatchPoster, -1, 2, 2);

			TestCheques.Reload();
			AssertEquals("Current number should not be incremented since the cheque number was for a cancelled payment", 67m, TestCheques.AK_CurrentNo);
		}

		public void TestUpdateChequeOrReferenceForAllPayments()
		{
			SetupDataForPostingTest();
			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_ChequeOrReference = "1";
			AssertEquals("ChequeOrReference should be set to 000001", "000001", BatchPoster.APB_ChequeOrReference);
			AssertEquals("PaymentApproval1 should have changed its AV_ChequeOrReference", "000001", PaymentApproval1.AV_ChequeOrReference);
			AssertEquals("PaymentApproval2 should have changed its AV_ChequeOrReference", "000002", PaymentApproval2.AV_ChequeOrReference);
			AssertEquals("PaymentApproval3 should have changed its AV_ChequeOrReference", "000003", PaymentApproval3.AV_ChequeOrReference);

			BatchPoster.APB_ChequeOrReference = ZString.Empty;
			Assert("ChequeOrReference should be empty", BatchPoster.APB_ChequeOrReference.IsEmpty);
			Assert("PaymentApproval1.AV_ChequeOrReference should be empty", PaymentApproval1.AV_ChequeOrReference.IsEmpty);
			Assert("PaymentApproval2.AV_ChequeOrReference should be empty", PaymentApproval2.AV_ChequeOrReference.IsEmpty);
			Assert("PaymentApproval3.AV_ChequeOrReference should be empty", PaymentApproval3.AV_ChequeOrReference.IsEmpty);

			BatchPoster.APB_PaymentType = ReceiptTypes.Cash;
			BatchPoster.APB_ChequeOrReference = "2";
			AssertEquals("ChequeOrReference should change", "2", BatchPoster.APB_ChequeOrReference);
			AssertEquals("PaymentApproval1.AV_ChequeOrReference should be changed even if the value is invalid", "2", PaymentApproval1.AV_ChequeOrReference);
			AssertEquals("PaymentApproval2.AV_ChequeOrReference should be changed even if the value is invalid", "2", PaymentApproval2.AV_ChequeOrReference);
			AssertEquals("PaymentApproval3.AV_ChequeOrReference should be changed even if the value is invalid", "2", PaymentApproval3.AV_ChequeOrReference);

			BatchPoster.APB_ChequeOrReference = ZString.Empty;
			Assert("ChequeOrReference should be empty", BatchPoster.APB_ChequeOrReference.IsEmpty);
			Assert("PaymentApproval1.AV_ChequeOrReference should be empty", PaymentApproval1.AV_ChequeOrReference.IsEmpty);
			Assert("PaymentApproval2.AV_ChequeOrReference should be empty", PaymentApproval2.AV_ChequeOrReference.IsEmpty);
			Assert("PaymentApproval3.AV_ChequeOrReference should be empty", PaymentApproval3.AV_ChequeOrReference.IsEmpty);

			PaymentApproval1.AV_Status = PaymentApprovalStatus.Cancelled;
			PaymentApproval2.AV_Status = PaymentApprovalStatus.Posted;

			BatchPoster.APB_ChequeOrReference = "2";
			AssertNotEquals("Percondition", ReceiptTypes.Cheque, BatchPoster.APB_PaymentType);
			AssertEquals("ChequeOrReference should change", "2", BatchPoster.APB_ChequeOrReference);
			Assert("PaymentApproval1.AV_ChequeOrReference should be empty", PaymentApproval1.AV_ChequeOrReference.IsEmpty);
			Assert("PaymentApproval2.AV_ChequeOrReference should be empty", PaymentApproval2.AV_ChequeOrReference.IsEmpty);
			AssertEquals("PaymentApproval3.AV_ChequeOrReference should be changed", "2", PaymentApproval3.AV_ChequeOrReference);

			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_ChequeOrReference = "3";
			AssertEquals("Percondition", ReceiptTypes.Cheque, BatchPoster.APB_PaymentType);
			AssertEquals("ChequeOrReference should change", "000003", BatchPoster.APB_ChequeOrReference);
			Assert("PaymentApproval1.AV_ChequeOrReference should be empty", PaymentApproval1.AV_ChequeOrReference.IsEmpty);
			Assert("PaymentApproval2.AV_ChequeOrReference should be empty", PaymentApproval2.AV_ChequeOrReference.IsEmpty);
			AssertEquals("PaymentApproval3.AV_ChequeOrReference should be changed", "000003", PaymentApproval3.AV_ChequeOrReference);
		}

		public void TestUpdateCardSecurityCodeForAllPayments()
		{
			SetupDataForPostingTest();
			BatchPoster.CardSecurityCode = "1111";
			AssertEquals("CardSecurityCode should be set", "1111", BatchPoster.CardSecurityCode);
			AssertEquals("PaymentApproval1 should have changed", "1111", PaymentApproval1.CreditCardSecurityCode);
			AssertEquals("PaymentApproval2 should have changed", "1111", PaymentApproval2.CreditCardSecurityCode);
			AssertEquals("PaymentApproval3 should have changed", "1111", PaymentApproval3.CreditCardSecurityCode);

			PaymentApproval1.AV_Status = PaymentApprovalStatus.Cancelled;
			PaymentApproval2.AV_Status = PaymentApprovalStatus.Posted;
			BatchPoster.CardSecurityCode = "2222";
			AssertEquals("CardSecurityCode should be set", "2222", BatchPoster.CardSecurityCode);
			AssertEquals("PaymentApproval1 should not have changed", "1111", PaymentApproval1.CreditCardSecurityCode);
			AssertEquals("PaymentApproval2 should not have changed", "1111", PaymentApproval2.CreditCardSecurityCode);
			AssertEquals("PaymentApproval3 should have changed", "2222", PaymentApproval3.CreditCardSecurityCode);
		}

		public void TestLocalCurrency()
		{
			SetupDataForBaseTest();
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, BatchPoster.LocalCurrency);
		}

		public void TestForeignCurrency()
		{
			SetupDataForBaseTest();
			AssertEquals(BatchPoster.PaymentApprovalCollection[0].MatchingBaseObject.ForeignCurrency, BatchPoster.ForeignCurrency);
			BatchPoster = Factory.New<APPaymentBatchPoster>();
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, BatchPoster.ForeignCurrency);
		}

		public void TestPaymentItemsTotalAmount()
		{
			SetupDataForBaseTest();
			AssertEquals(BatchPoster.PaymentApprovalCollection[0].PaymentItemsTotalAmount, BatchPoster.PaymentItemsTotalAmount);
			BatchPoster = Factory.New<APPaymentBatchPoster>();
			AssertEquals(ZDecimal.Zero, BatchPoster.PaymentItemsTotalAmount);
		}

		public void TestBalance_ChangingAV_AmountWillRefreshBalance()
		{
			SetupDataForBaseTest();
			AssertEquals(BatchPoster.PaymentForBinding.MatchedTransactionsBalanceWithPaymentAmount, BatchPoster.Balance);
			ZDecimal oldBalanceValue = BatchPoster.Balance;
			BatchPoster.PaymentApprovalCollection[0].AV_Amount = 100m;
			AssertNotEquals("MatchedTransactions Balance should be changed", BatchPoster.PaymentForBinding.MatchedTransactionsBalanceWithPaymentAmount, oldBalanceValue);
			AssertEquals("Balance should be updated", BatchPoster.Balance, BatchPoster.PaymentForBinding.MatchedTransactionsBalanceWithPaymentAmount);
		}

		public void TestBalance_ChangingAV_Calc_Local_AmountWillRefreshBalance()
		{
			SetupDataForBaseTest();
			AssertEquals(BatchPoster.PaymentForBinding.MatchedTransactionsBalanceWithPaymentAmount, BatchPoster.Balance);
			ZDecimal oldBalanceValue = BatchPoster.Balance;
			BatchPoster.PaymentApprovalCollection[0].AV_Calc_LocalAmount = 100m;
			AssertNotEquals("MatchedTransactions Balance should be changed", BatchPoster.PaymentForBinding.MatchedTransactionsBalanceWithPaymentAmount, oldBalanceValue);
			AssertEquals("Balance should be updated", BatchPoster.Balance, BatchPoster.PaymentForBinding.MatchedTransactionsBalanceWithPaymentAmount);
		}

		public void TestBalance_ChangingAV_PayExRateWillRefreshBalance()
		{
			SetupDataForBaseTest();
			AssertEquals(BatchPoster.PaymentForBinding.MatchedTransactionsBalanceWithPaymentAmount, BatchPoster.Balance);
			ZDecimal oldBalanceValue = BatchPoster.Balance;
			BatchPoster.PaymentApprovalCollection[0].AV_PayExRate = 100m;
			AssertNotEquals("MatchedTransactions Balance should be changed", BatchPoster.PaymentForBinding.MatchedTransactionsBalanceWithPaymentAmount, oldBalanceValue);
			AssertEquals("Balance should be updated", BatchPoster.Balance, BatchPoster.PaymentForBinding.MatchedTransactionsBalanceWithPaymentAmount);
		}

		public void TestBalance_ResetPaymentMatchingCollectionWillRefreshBalance()
		{
			SetupDataForBaseTest();

			TestAPInv2 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInv2.AH_OH = TestOrg.PK;
			TestAPInv2.AH_LocalExTaxAmount = 94M;
			TestAPInv2.AH_OSExTaxAmount = 94M;
			TestAPInv2.AH_Desc = "For Payment Approval 2";

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory);
			transactions = GetSingleTransactionForPosting();
			transactions.Add(TestAPInv2);

			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.SetDefaultValuesByTransactions(transactions);
			BatchPoster.SetPaymentDetails(BatchPoster.PaymentApprovalCollection[0]);
			AssertEquals(BatchPoster.PaymentForBinding.MatchedTransactionsBalanceWithPaymentAmount, BatchPoster.Balance);
			AssertEquals("Matching collection should contain 2 transaction", 2, BatchPoster.MatchingCollection.Count);
			ZDecimal oldBalanceValue = BatchPoster.Balance;

			BatchPoster.PaymentForBinding.MatchingBaseObject.MatchedTransactions.Remove(TestAPInv);
			BatchPoster.ResetPaymentMatchingCollection_ForTestOnly(BatchPoster.PaymentForBinding.MatchingBaseObject.MatchedTransactions);
			AssertEquals("Matching collection should be reloaded", 1, BatchPoster.MatchingCollection.Count);
			AssertNotEquals("MatchedTransactions Balance should be changed", BatchPoster.PaymentForBinding.MatchedTransactionsBalanceWithPaymentAmount, oldBalanceValue);
			AssertEquals("Balance should be updated", BatchPoster.Balance, BatchPoster.PaymentForBinding.MatchedTransactionsBalanceWithPaymentAmount);
		}

		public void TestBalance_ChangingPayExRateWillRefreshBalance()
		{
			SetupDataForBaseTest();
			AssertEquals(BatchPoster.PaymentForBinding.MatchedTransactionsBalanceWithPaymentAmount, BatchPoster.Balance);
			ZDecimal oldBalanceValue = BatchPoster.Balance;
			BatchPoster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().First().AV_PayExRate = 2m;
			AssertNotEquals("MatchedTransactions Balance should be changed", BatchPoster.PaymentForBinding.MatchedTransactionsBalanceWithPaymentAmount, oldBalanceValue);
			AssertEquals("Balance should be updated", BatchPoster.Balance, BatchPoster.PaymentForBinding.MatchedTransactionsBalanceWithPaymentAmount);
		}

		void MakePaymentAndTransactienUseSameCurrencyForTesting(PaymentApprovalBase payment, RefCurrency currency = null)
		{
			currency = currency ?? payment.Factory.NewWithValidTestData<RefCurrency>();
			MakePaymentAndTransactionUseDifferentCurrenciesForTesting(payment, currency, currency);
		}

		void MakePaymentAndTransactionUseDifferentCurrenciesForTesting(PaymentApprovalBase payment, RefCurrency transactionCurrency = null, RefCurrency paymentCurrency = null)
		{
			transactionCurrency = transactionCurrency ?? payment.Factory.NewWithValidTestData<RefCurrency>();
			paymentCurrency = paymentCurrency ?? payment.Factory.NewWithValidTestData<RefCurrency>();
			payment.AV_RX_NKPaymentCurrency = paymentCurrency.Code;
			payment.MatchingBaseObject.MatchedTransactions.Cast<TransactionHeader>().ForEach(x => x.AH_RX_NKTransactionCurrency = transactionCurrency.Code);
		}

		public void TestCreditor()
		{
			SetupDataForBaseTest();
			Assert(!BatchPoster.PaymentApprovalCollection[0].AV_OH.IsEmpty);
			AssertEquals(BatchPoster.PaymentApprovalCollection[0].AV_OH, BatchPoster.Creditor);
			BatchPoster = Factory.New<APPaymentBatchPoster>();
			AssertEquals(ZGuid.Empty, BatchPoster.Creditor);
		}

		public void TestOSOverpaymentAmount()
		{
			SetupDataForBaseTest();
			AssertEquals(BatchPoster.PaymentApprovalCollection[0].OSOverpaymentAmount, BatchPoster.OSOverpaymentAmount);
			BatchPoster = Factory.New<APPaymentBatchPoster>();
			AssertEquals(ZDecimal.Zero, BatchPoster.OSOverpaymentAmount);
		}

		public void TestPaymentAmountsForSingleForeignCurrencyPayments()
		{
			SetupDataForSingleForeignCurrencyPaymentTest();
			BatchPoster.PaymentForBinding = PaymentApproval1;
			BatchPoster.APB_AB = BatchPostingHelper.TestObjectCreator.USDBankAccount.PK;
			var payment = BatchPoster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().First();

			payment.AV_PayExRate = 1.04m;
			AssertEquals("Payment approval amount for single foreign currency invoices should equal the sum of the OS amounts", 3000m, PaymentApproval1.AV_Amount);
			AssertEquals("Payment approval local amount for mixed currency invoices should equal the sum of the local amounts", 7635.31m, PaymentApproval2.AV_Calc_LocalAmount);

			payment.AV_PayExRate = 2.99;
			AssertEquals("Payment approval amount for single foreign currency invoices should not change when the exchange rate is updated", 3000m, PaymentApproval1.AV_Amount);

			BatchPoster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().ForEach(x => x.AV_Amount = 0m);

			BatchPoster.APB_AB = BatchPostingHelper.TestObjectCreator.GBPBankAccount.PK;
			AssertEquals("Bank account currency", "GBP", payment.CurrencyCode);
			Assert("Setting a different foreign currency bank account should not re-default the payment amounts", !BatchPoster.PaymentApprovalCollection.Any(p => ((APPaymentApprovalWithoutAuthorisation)p).AV_Amount > 0));
		}

		public void TestPaymentAmountsForSingleForeignCurrencyPayments_WhenPaymentApprovalIsCancelled()
		{
			SetupDataForSingleForeignCurrencyPaymentTest();
			BatchPoster.PaymentForBinding = PaymentApproval1;

			PaymentApproval1.AV_Status = PaymentApprovalStatus.Cancelled;

			AssertNotEquals("Percondition", BatchPoster.LocalCurrency, PaymentApproval1.CurrencyCode);
			AssertEquals("Percondition", true, PaymentApproval1.MatchingBaseObject.IsAllPaidInTheSameCurrency(PaymentApproval1.AV_RX_NKPaymentCurrency));

			AssertEquals(0m, PaymentApproval1.AV_Amount);
			BatchPoster.APB_AB = BatchPostingHelper.TestObjectCreator.USDBankAccount.PK;
			AssertEquals("AV_Amount not changed", 0m, PaymentApproval1.AV_Amount);
		}

		public void TestPaymentAmountsForSingleForeignCurrencyPayments_WhenPaymentApprovalIsPosted()
		{
			SetupDataForSingleForeignCurrencyPaymentTest();
			BatchPoster.PaymentForBinding = PaymentApproval1;

			PaymentApproval1.AV_Status = PaymentApprovalStatus.Posted;

			AssertNotEquals("Percondition", BatchPoster.LocalCurrency, PaymentApproval1.CurrencyCode);
			AssertEquals("Percondition", true, PaymentApproval1.MatchingBaseObject.IsAllPaidInTheSameCurrency(PaymentApproval1.AV_RX_NKPaymentCurrency));

			AssertEquals(0m, PaymentApproval1.AV_Amount);
			BatchPoster.APB_AB = BatchPostingHelper.TestObjectCreator.USDBankAccount.PK;
			AssertEquals("AV_Amount not changed", 0m, PaymentApproval1.AV_Amount);
		}

		public void TestApplyExchangeGainLossNoAdjustmentNeededMessage()
		{
			SetupDataForBaseTest();

			var localCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			var foreignCurrency = BatchPostingHelper.TestObjectCreator.USD;
			AssertNotEquals("Precondition: local currency is not the foreign currency", localCurrency.RX_Code, foreignCurrency.RX_Code);

			TestAPInv = (APInvoice)BatchPostingHelper.TestObjectCreator.CreateInvoice(typeof(APInvoice), "0001", foreignCurrency, 1.02m);
			TestAPInv.AH_RX_NKTransactionCurrency = foreignCurrency.RX_Code;
			TestAPInv.AH_OH = BatchPostingHelper.TestObjectCreator.Creditor1.PK;
			BatchPostingHelper.TestObjectCreator.CreateInvoiceLine(TestAPInv, foreignCurrency, 1.02m, 1000m, 100m, 0m);

			TestAPInv2 = (APInvoice)BatchPostingHelper.TestObjectCreator.CreateInvoice(typeof(APInvoice), "0002", foreignCurrency, 1.02m);
			TestAPInv2.AH_RX_NKTransactionCurrency = foreignCurrency.RX_Code;
			TestAPInv2.AH_OH = BatchPostingHelper.TestObjectCreator.Creditor1.PK;
			BatchPostingHelper.TestObjectCreator.CreateInvoiceLine(TestAPInv2, foreignCurrency, 1.02m, 1000m, 100m, 0m);

			Factory.Save();

			BatchPoster = Factory.New<APPaymentBatchPoster>();
			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory);
			transactions.AddRange(TestAPInv, TestAPInv2);
			BatchPoster.SetDefaultValuesByTransactions(transactions);
			AssertEquals("Number of payments in the collection", 1, BatchPoster.PaymentApprovalCollection.Count);

			var notifications = new NotificationCollection();
			BatchPoster.APB_AB = BatchPostingHelper.TestObjectCreator.USDBankAccount.PK;
			var payment = BatchPoster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().First().AV_PayExRate = 1.02m;
			BatchPoster.ApplyExchangeGainLossToAllSingleForeignCurrencyPayments(notifications);
			AssertEquals("User notification", "No adjustment to the exchange gain/loss was required.", notifications[0].Message);
		}

		public void TestApplyExchangeGainLossToMultiForeignCurrencyPayments()
		{
			SetupDataForSingleForeignCurrencyPaymentTest();
			var notifications = new NotificationCollection();

			BatchPoster.ApplyExchangeGainLossToAllSingleForeignCurrencyPayments(notifications);
			AssertEquals("User notification", "One or more payments were skipped because they were using a local currency", notifications[0].Message);

			BatchPoster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().Where(x => x.CurrencyCode == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency).ForEach(y => y.AV_RX_NKPaymentCurrency = TestObjectCreator.USD.RX_Code);
			PaymentApproval1.AV_Amount = 2870.88;
			PaymentApproval2.AV_Amount = 7635.30;

			notifications.Clear();
			BatchPoster.ApplyExchangeGainLossToAllSingleForeignCurrencyPayments(notifications);

			AssertEquals(0, notifications.Count);
			AssertEquals(-2545.10m, PaymentApproval2.ExchangeDifferenceAmount);
			AssertEquals(-956.96m, PaymentApproval1.ExchangeDifferenceAmount);
			AssertEquals(0m, PaymentApproval2.MatchedTransactionsBalanceWithPaymentAmount);
			AssertEquals(0m, PaymentApproval1.MatchedTransactionsBalanceWithPaymentAmount);
		}

		public void TestDeleteCachedMiscTransactionsWhenRemovePaymentFromBatch()
		{
			SetupDataForBaseTest();

			var foreignCurrency = BatchPostingHelper.TestObjectCreator.USD;

			TestAPInv = (APInvoice)BatchPostingHelper.TestObjectCreator.CreateInvoice(typeof(APInvoice), "0001", foreignCurrency, 1.02m, BatchPostingHelper.TestObjectCreator.Creditor1);
			BatchPostingHelper.TestObjectCreator.CreateInvoiceLine(TestAPInv, foreignCurrency, 1.02m, 1000m, 100m, 0m);

			TestAPInv2 = (APInvoice)BatchPostingHelper.TestObjectCreator.CreateInvoice(typeof(APInvoice), "0002", foreignCurrency, 1.02m, BatchPostingHelper.TestObjectCreator.Creditor2);
			BatchPostingHelper.TestObjectCreator.CreateInvoiceLine(TestAPInv2, foreignCurrency, 1.02m, 1000m, 100m, 0m);

			Factory.Save();

			BatchPoster = Factory.New<APPaymentBatchPoster>();
			var transactions = new TransactionHeaderCollection(Factory);
			transactions.AddRange(TestAPInv, TestAPInv2);
			BatchPoster.SetDefaultValuesByTransactions(transactions);
			AssertEquals("Number of payments in the collection", 2, BatchPoster.PaymentApprovalCollection.Count);

			var notifications = new NotificationCollection();
			BatchPoster.APB_AB = BatchPostingHelper.TestObjectCreator.USDBankAccount.PK;
			BatchPoster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().First().AV_PayExRate = 1.04m;
			BatchPoster.ApplyExchangeGainLossToAllSingleForeignCurrencyPayments(notifications);
			AssertEquals("An exchange difference has been created", 2, BatchPoster.PaymentApprovalCollection[0].MatchingBaseObject.MatchedTransactions.Count);
			var exx = BatchPoster.PaymentApprovalCollection[0].MatchingBaseObject.MatchedTransactions.Where(x => x is ExchangeDifference).First();

			BatchPoster.RemovePaymentFromBatch(BatchPoster.PaymentApprovalCollection[0]);

			Assert("Related misc transactions have been deleted correctly", exx.IsDeleted);
			AssertNoExceptionThrown("Shouldn't throw critical validation failure", Factory.Save);
		}

		public void TestExchangeDifferenceAmount()
		{
			SetupDataForBaseTest();

			TestAPInv2 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInv2.AH_OH = TestOrg.PK;
			TestAPInv2.AH_LocalExTaxAmount = 94M;
			TestAPInv2.AH_OSExTaxAmount = 94M;
			TestAPInv2.AH_Desc = "For Payment Approval 2";

			var transactions = GetSingleTransactionForPosting();
			transactions.Add(TestAPInv2);

			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.SetDefaultValuesByTransactions(transactions);
			BatchPoster.SetPaymentDetails(BatchPoster.PaymentApprovalCollection[0]);
			AssertEquals(ZDecimal.Zero, BatchPoster.Balance);

			BatchPoster.RemoveTransactionFromPayment(TestAPInv2);
			AssertNotEquals(ZDecimal.Zero, BatchPoster.Balance);

			TransactionHeader miscTrans = BatchPoster.PaymentForBinding.MatchingBaseObject.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
			BatchPoster.PaymentApprovalCollection[0].MatchingBaseObject.AddMiscellaneousTransaction(miscTrans);

			AssertNotEquals(ZDecimal.Zero, BatchPoster.PaymentApprovalCollection[0].ExchangeDifferenceAmount);
			AssertEquals(BatchPoster.PaymentApprovalCollection[0].ExchangeDifferenceAmount, BatchPoster.ExchangeDifferenceAmount);
		}

		public void TestDiscountAmount()
		{
			SetupDataForBaseTest();

			TestAPInv2 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInv2.AH_OH = TestOrg.PK;
			TestAPInv2.AH_LocalExTaxAmount = 94M;
			TestAPInv2.AH_OSExTaxAmount = 94M;
			TestAPInv2.AH_Desc = "For Payment Approval 2";

			var transactions = GetSingleTransactionForPosting();
			transactions.Add(TestAPInv2);

			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.SetDefaultValuesByTransactions(transactions);
			BatchPoster.SetPaymentDetails(BatchPoster.PaymentApprovalCollection[0]);
			AssertEquals(ZDecimal.Zero, BatchPoster.Balance);

			BatchPoster.RemoveTransactionFromPayment(TestAPInv2);
			AssertNotEquals(ZDecimal.Zero, BatchPoster.Balance);

			TransactionHeader miscTrans = BatchPoster.PaymentForBinding.MatchingBaseObject.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount);
			BatchPoster.PaymentApprovalCollection[0].MatchingBaseObject.AddMiscellaneousTransaction(miscTrans);

			AssertNotEquals(ZDecimal.Zero, BatchPoster.PaymentApprovalCollection[0].DiscountAmount);
			AssertEquals(BatchPoster.PaymentApprovalCollection[0].DiscountAmount, BatchPoster.DiscountAmount);
		}

		public void TestSet_PaymentForBinding()
		{
			SetupDataForBaseTest(false);
			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.SetDefaultValuesByTransactions(GetSingleTransactionForPosting());
			BatchPoster.PaymentForBinding = BatchPoster.PaymentApprovalCollection[0];
			AssertEquals("Payment MatchingCollection should be initialized", 1, BatchPoster.MatchingCollection.Count);
			Assert("Payment MatchingCollection should contain TestAPInv", BatchPoster.MatchingCollection.Contains(TestAPInv));
			BatchPoster.PaymentForBinding = null;
			AssertEquals("Payment MatchingCollection should be reseted", 0, BatchPoster.MatchingCollection.Count);
		}

		public void TestIsPosted()
		{
			SetupDataForBaseTest(false);

			TestAPInv2 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInv2.AH_OH = BatchPostingHelper.CreateCreditorTestOrg(null).PK;
			TestAPInv2.AH_LocalExTaxAmount = 94M;
			TestAPInv2.AH_OSExTaxAmount = 94M;
			TestAPInv2.AH_Desc = "Payment Approval 2";

			var transactions = GetSingleTransactionForPosting();
			transactions.Add(TestAPInv2);

			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.SetDefaultValuesByTransactions(transactions);
			BatchPoster.SetPaymentDetails(BatchPoster.PaymentApprovalCollection[0]);
			AssertEquals("Precondition", 2, BatchPoster.PaymentApprovalCollection.Count);
			AssertEquals(false, BatchPoster.PaymentApprovalCollection[0].IsPosted);
			AssertEquals(false, BatchPoster.PaymentApprovalCollection[1].IsPosted);

			AssertEquals("None of the transactions are posted", false, BatchPoster.IsPosted);

			BatchPoster.PaymentApprovalCollection[0].AV_Status = PaymentApprovalStatus.Posted;
			AssertEquals("Precondition", 2, BatchPoster.PaymentApprovalCollection.Count);
			AssertEquals(true, BatchPoster.PaymentApprovalCollection[0].IsPosted);
			AssertEquals(false, BatchPoster.PaymentApprovalCollection[1].IsPosted);

			AssertEquals("Some transactions are not posted", false, BatchPoster.IsPosted);

			BatchPoster.PaymentApprovalCollection[1].AV_Status = PaymentApprovalStatus.Posted;
			AssertEquals("Precondition", 2, BatchPoster.PaymentApprovalCollection.Count);
			AssertEquals(true, BatchPoster.PaymentApprovalCollection[0].IsPosted);
			AssertEquals(true, BatchPoster.PaymentApprovalCollection[1].IsPosted);

			AssertEquals("All transactions are posted", true, BatchPoster.IsPosted);

			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.SetDefaultValuesByTransactions(new TransactionHeaderCollection(Factory));
			AssertEquals("Precondition", 0, BatchPoster.PaymentApprovalCollection.Count);

			AssertEquals("Transaction collection is empty", false, BatchPoster.IsPosted);
		}

		public void TestLoadPayments()
		{
			ZDateTime dateTime = ZDateTime.Now;
			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			AccPaymentBatch batch = Factory.NewWithValidTestData<AccPaymentBatch>();

			PaymentApprovalBase payment1 = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			PaymentApprovalBase payment2 = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			PaymentApprovalBase payment3 = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			PaymentApprovalBase payment4 = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();

			payment1.AV_APB_PaymentBatch = batch.PK;
			payment2.AV_APB_PaymentBatch = batch.PK;
			payment3.AV_APB_PaymentBatch = batch.PK;
			payment4.AV_APB_PaymentBatch = batch.PK;

			batch.APB_PaymentType = payment2.AV_PaymentType = payment3.AV_PaymentType = payment4.AV_PaymentType = ReceiptTypes.Cash;
			batch.APB_PaymentDate = payment2.AV_PaymentDate = payment3.AV_PaymentDate = payment4.AV_PaymentDate = dateTime;
			batch.APB_AB = payment2.AV_AB = payment3.AV_AB = payment4.AV_AB = bankAccount.PK;
			batch.APB_AK = payment2.AV_AK = payment3.AV_AK = payment4.AV_AK = chequeBook.PK;

			payment2.AV_Status = PaymentApprovalStatus.Cancelled;
			payment3.AV_Status = PaymentApprovalStatus.Posted;
			payment4.AV_Status = PaymentApprovalStatus.FullyApproved;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var poster = newFactory.Load<APPaymentBatchPoster>(batch.PK);
			poster.LoadPayments();

			AssertEquals(payment1.PK, poster.PaymentApprovalCollection[0].PK);
			AssertEquals(payment3.PK, poster.PaymentApprovalCollection[1].PK);
			AssertEquals(payment2.PK, poster.PaymentApprovalCollection[2].PK);
			AssertEquals(payment4.PK, poster.PaymentApprovalCollection[3].PK);
		}

		public void TestPaymentApprovalCollection()
		{
			SetupDataForPostingTest();
			var collection = BatchPoster.PaymentApprovalCollection;

			AssertEquals(typeof(APPaymentApprovalWithoutAuthorisationCollection), collection.GetType());

			Factory.Save();
			var newFactory = Factory.CreateNewFactory();
			BatchPoster = newFactory.Load<APPaymentBatchPoster>(BatchPoster.PK);
			BatchPoster.LoadPayments();
			collection = BatchPoster.PaymentApprovalCollection;

			AssertEquals(typeof(APPaymentApprovalWithAuthorisationCollection), collection.GetType());
		}

		#endregion

		#region Implementation Tests

		public void TestSetDefaultValues()
		{
			SetupDataForBaseTest();

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BatchPoster.APB_PaymentDate = ZDateTime.Now.AddDays(-2);
			BatchPoster.APB_PostDate = ZDateTime.Now.AddDays(-2);
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_PaymentType = ReceiptTypes.Cash;

			BatchPoster.SetDefaultValues_ForTestOnly();

			AssertEquals(ZDateTime.Now.Date, BatchPoster.APB_PaymentDate.Date);
			AssertEquals(ZDateTime.Now.Date, BatchPoster.APB_PostDate.Date);
			AssertEquals(ZGuid.Empty, BatchPoster.APB_AB);
			AssertEquals(ReceiptTypes.Cheque, BatchPoster.APB_PaymentType);
			AssertEquals(ZDateTime.Now.Date, BatchPoster.APB_PostDate.Date);
			Assert(!BatchPoster.APB_PostDateInfo.ReadOnly);

			Assert("Batch poster should have no changes", !BatchPoster.HasChanges);

			AccountingConfigurationRegistry.Instance.DefaultPaymentType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReceiptTypes.Cash);
			BatchPoster = Factory.New<APPaymentBatchPoster>();
			AssertEquals(ReceiptTypes.Cash, BatchPoster.APB_PaymentType);
		}

		[TestDate(2021, 08, 23, 08, 12, 59)]
		public void TestSetDefaultValuesWhenPaymentIsInDatabase()
		{
			SetupDataForBaseTest(false);

			var payment1 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			var payment2 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();

			var batch = Factory.NewWithValidTestData<AccPaymentBatch>();
			payment1.AV_APB_PaymentBatch = batch.PK;
			payment2.AV_APB_PaymentBatch = batch.PK;

			batch.APB_AB = TestBank.PK;
			batch.APB_AK = TestCheques.PK;
			batch.APB_PaymentType = ReceiptTypes.Cheque;
			batch.APB_ChequeOrReference = "0012345";
			batch.APB_PaymentDate = ZDateTime.Now.AddDays(3);
			batch.APB_PostDate = ZDateTime.Now;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			batch = newFactory.Load<AccPaymentBatch>(batch.PK);
			BatchPoster = newFactory.Load<APPaymentBatchPoster>(batch.PK);
			BatchPoster.LoadPayments();

			AssertEquals(2, BatchPoster.PaymentApprovalCollection.Count);
			AssertEquals("Percondition", true, BatchPoster.PaymentApprovalCollection[0].IsInDatabase);
			AssertEquals("Percondition", true, BatchPoster.PaymentApprovalCollection[1].IsInDatabase);

			AssertEquals(TestBank.PK, BatchPoster.APB_AB);
			AssertEquals(TestCheques.PK, BatchPoster.APB_AK);
			AssertEquals(ReceiptTypes.Cheque, BatchPoster.APB_PaymentType);
			AssertEquals("0012345", BatchPoster.APB_ChequeOrReference);
			AssertEquals(new ZDateTime(2021, 08, 26, 08, 12, 00), BatchPoster.APB_PaymentDate);
			AssertEquals(new ZDateTime(2021, 08, 23, 08, 12, 00), BatchPoster.APB_PostDate);
		}

		public void TestDefaultChequeOrReference()
		{
			SetupDataForBaseTest(false);

			var list = AccountingConfigurationRegistry.Instance.PaymentReceiptTypeReferenceNumberRegistryDefaults.Value;
			var element = list.Cast<PaymentReceiptTypeReferenceNumber>().First(x => x.Type == ReceiptTypes.CreditCard);
			element.ReferenceNumber = "Test1";
			AccountingConfigurationRegistry.Instance.PaymentReceiptTypeReferenceNumberRegistryDefaults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var batchPoster = Factory.New<APPaymentBatchPoster>();
			batchPoster.APB_PaymentType = ReceiptTypes.CreditCard;
			AssertEquals(ReceiptTypes.CreditCard, batchPoster.APB_PaymentType);
			AssertEquals("Test1", batchPoster.APB_ChequeOrReference);

			batchPoster.APB_PaymentType = ReceiptTypes.Cash;
			AssertEquals(ReceiptTypes.Cash, batchPoster.APB_PaymentType);
			AssertEquals(ReceiptTypes.Cash, batchPoster.APB_ChequeOrReference);
		}

		public void TestDefaultPostDateReadOnly()
		{
			bool payablesAllowed = Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed;

			try
			{
				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				BatchPoster = Factory.New<APPaymentBatchPoster>();
				Assert("AV_PostDate should not be readonly", !BatchPoster.APB_PostDateInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				BatchPoster = Factory.New<APPaymentBatchPoster>();
				Assert("AV_PostDate should not be readonly", !BatchPoster.APB_PostDateInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				BatchPoster = Factory.New<APPaymentBatchPoster>();
				Assert("AV_PostDate should not be readonly", !BatchPoster.APB_PostDateInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				BatchPoster = Factory.New<APPaymentBatchPoster>();
				Assert("AV_PostDate should not be readonly", !BatchPoster.APB_PostDateInfo.ReadOnly);
			}
			finally
			{
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = payablesAllowed;
			}
		}

		public void TestSetAmountsForMatchingTransaction()
		{
			SetupDataForBaseTest();
			AssertEquals(BatchPoster.PaymentApprovalCollection[0].OSPartialPaymentAmount, BatchPoster.PaymentApprovalCollection[0].OSOutstandingAmount);
		}

		public void TestInitializeTransactionCollection()
		{
			SetupDataForBaseTest();
			TransactionHeaderCollection transactions = null;
			Assert("Should return False", !BatchPoster.InitializeTransactionCollection_ForTestOnly(transactions));
			transactions = new TransactionHeaderCollection(Factory);
			Assert("Should return False", !BatchPoster.InitializeTransactionCollection_ForTestOnly(transactions));
			transactions = BatchPostingHelper.MakeCollectionToBeUnsorted(BatchPostingHelper.GetTransactionsForPosting());
			Assert("Collection should be unsorted", !BatchPostingHelper.CollectionIsSorted(transactions));
			Assert("Should return True", BatchPoster.InitializeTransactionCollection_ForTestOnly(transactions));
			Assert("Collection should be sorted by SettlementGroup", BatchPostingHelper.CollectionIsSorted(transactions));
		}

		#endregion

		#region LookUps Tests

		public void TestBankAccounts()
		{
			GlbBranch newBranch = Factory.NewWithValidTestData<GlbBranch>();

			AccBankAccount bank1 = Factory.NewWithValidTestData<AccBankAccount>();
			bank1.AB_GB = GlbBranch.CurrentBranch.PK;

			AccBankAccount bank2 = Factory.NewWithValidTestData<AccBankAccount>();
			bank2.AB_GB = ZGuid.Empty;

			AccBankAccount bank3 = Factory.NewWithValidTestData<AccBankAccount>();
			bank3.AB_GB = newBranch.PK;

			BatchPoster = Factory.New<APPaymentBatchPoster>();

			AssertNotNull("BankAccounts should not be null", BatchPoster.BankAccounts);
			AssertEquals("BankAccounts Type", typeof(AccBankAccountCollection), BatchPoster.BankAccounts.GetType());
			BatchPoster.BankAccounts.Load();
			Assert("BankAccounts contains TestBank", BatchPoster.BankAccounts.Contains(bank1.PK));
			Assert("BankAccounts contains Bank 2", BatchPoster.BankAccounts.Contains(bank2.PK));
			Assert("BankAccounts contains Bank 3", !BatchPoster.BankAccounts.Contains(bank3.PK));
		}

		public void TestBankAccounts_ContainsOnlyActiveBanks()
		{
			SetupDataForBaseTest(false);
			AccBankAccount inactiveBank = Factory.NewWithValidTestData<AccBankAccount>();
			inactiveBank.AB_IsActive = false;

			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.BankAccounts.Load();
			Assert("Should contain active bank", BatchPoster.BankAccounts.Contains(TestBank));
			Assert("Should not contain inactive bank", !BatchPoster.BankAccounts.Contains(inactiveBank));
		}

		public void TestChequeBooks()
		{
			GlbBranch newBranch = Factory.NewWithValidTestData<GlbBranch>();

			AccBankAccount bank1 = Factory.NewWithValidTestData<AccBankAccount>();

			AccChequeBook bank1Book1 = Factory.NewWithValidTestData<AccChequeBook>();
			bank1Book1.AK_AB = bank1.PK;
			bank1Book1.AK_GB = GlbBranch.CurrentBranch.PK;

			AccChequeBook bank1Book2 = Factory.NewWithValidTestData<AccChequeBook>();
			bank1Book2.AK_AB = bank1.PK;
			bank1Book2.AK_GB = newBranch.PK;

			AccBankAccount bank2 = Factory.NewWithValidTestData<AccBankAccount>();

			AccChequeBook bank2Book1 = Factory.NewWithValidTestData<AccChequeBook>();
			bank2Book1.AK_AB = bank2.PK;
			bank2Book1.AK_GB = GlbBranch.CurrentBranch.PK;

			AccChequeBook bank2Book2 = Factory.NewWithValidTestData<AccChequeBook>();
			bank2Book2.AK_AB = bank2.PK;
			bank2Book2.AK_GB = newBranch.PK;

			BatchPoster = Factory.New<APPaymentBatchPoster>();

			AssertNotNull("ChequeBooks should not be null", BatchPoster.ChequeBooks);
			AssertEquals("ChequeBooks Type", typeof(ActiveChequeBookCollection), BatchPoster.ChequeBooks.GetType());

			BatchPoster.APB_AB = ZGuid.Empty;
			BatchPoster.ChequeBooks.Load();
			AssertEquals("ChequeBooks Count", 2, BatchPoster.ChequeBooks.Count);
			Assert("ChequeBooks contains Bank1Book1", BatchPoster.ChequeBooks.Contains(bank1Book1.PK));
			Assert("ChequeBooks contains Bank2Book2", BatchPoster.ChequeBooks.Contains(bank2Book1.PK));

			BatchPoster.APB_AB = bank1.PK;
			BatchPoster.ChequeBooks.Load();
			AssertEquals("ChequeBooks Count", 1, BatchPoster.ChequeBooks.Count);
			Assert("ChequeBooks contains Bank1Book1", BatchPoster.ChequeBooks.Contains(bank1Book1.PK));
			Assert("ChequeBooks contains Bank1Book2", !BatchPoster.ChequeBooks.Contains(bank1Book2.PK));

			BatchPoster.APB_AB = bank2.PK;
			BatchPoster.ChequeBooks.Load();
			AssertEquals("ChequeBooks Count", 1, BatchPoster.ChequeBooks.Count);
			Assert("ChequeBooks contains Bank2Book1", BatchPoster.ChequeBooks.Contains(bank2Book1.PK));
			Assert("ChequeBooks contains Bank2Book2", !BatchPoster.ChequeBooks.Contains(bank2Book2.PK));
		}

		public void TestHeaders()
		{
			SetupDataForBaseTest();
			AssertEquals("Headers collection should be of valid type", typeof(CreditorCollection), BatchPoster.Headers.GetType());
		}

		#endregion

		#region AutoAllocation Tests

		public void TestCalc_ChequeNumberIsAutoAllocatedLabel()
		{
			var testBookWithAutoAllocation = BatchPostingHelper.GetAutoPrintChequeBook();
			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			Assert("Label should be empty so far", BatchPoster.Calc_ChequeNumberIsAutoAllocatedLabel.IsEmpty);

			var testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			BatchPoster.APB_AK = testChequeBook.PK;
			Assert("Cheque book not IsAutoPrint", BatchPoster.Calc_ChequeNumberIsAutoAllocatedLabel.IsEmpty);

			BatchPoster.APB_AK = testBookWithAutoAllocation.PK;
			AssertEquals("Cheque book IsAutoPrint", AccountingConstants.ChequeLabelConstants.ChequeNumberIsAutoAllocatedLabel, BatchPoster.Calc_ChequeNumberIsAutoAllocatedLabel);
			BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			Assert("Payment type not 'Cheque'", BatchPoster.Calc_ChequeNumberIsAutoAllocatedLabel.IsEmpty);
			BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			BatchPoster.APB_AK = testBookWithAutoAllocation.PK;
			AssertEquals("Pyment type is 'Cheque' again", AccountingConstants.ChequeLabelConstants.ChequeNumberIsAutoAllocatedLabel, BatchPoster.Calc_ChequeNumberIsAutoAllocatedLabel);
		}

		public void TestCalc_ChequeIsAutoPrintedLabel()
		{
			var testBookWithAutoAllocation = BatchPostingHelper.GetAutoPrintChequeBook();
			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			Assert("Label should be empty so far", BatchPoster.Calc_ChequeIsAutoPrintedLabel.IsEmpty);

			var testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			BatchPoster.APB_AK = testChequeBook.PK;
			Assert("Cheque book not IsAutoPrint", BatchPoster.Calc_ChequeIsAutoPrintedLabel.IsEmpty);

			BatchPoster.APB_AK = testBookWithAutoAllocation.PK;
			AssertEquals("Cheque book IsAutoPrint", AccountingConstants.ChequeLabelConstants.ChequeAutoPrintedLabel, BatchPoster.Calc_ChequeIsAutoPrintedLabel);
			BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			Assert("Payment type not 'Cheque'", BatchPoster.Calc_ChequeIsAutoPrintedLabel.IsEmpty);
			BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			BatchPoster.APB_AK = testBookWithAutoAllocation.PK;
			AssertEquals("Pyment type is 'Cheque' again", AccountingConstants.ChequeLabelConstants.ChequeAutoPrintedLabel, BatchPoster.Calc_ChequeIsAutoPrintedLabel);
		}

		public virtual void TestSetChequeBookPK()
		{
			var testBookWithAutoAllocation = BatchPostingHelper.GetAutoPrintChequeBook();
			testBookWithAutoAllocation.AK_CurrentNo = 2;
			var testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_CurrentNo = 2;

			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			BatchPoster.APB_AK = testChequeBook.PK;
			AssertEquals("Cheque Number should be populated", "2", BatchPoster.APB_ChequeOrReference);
			AssertEquals("Cheque Number should not be readonly", false, BatchPoster.APB_ChequeOrReference_ReadOnly);
			AssertEquals("Cheque Number should not be readonly", false, BatchPoster.APB_ChequeOrReferenceInfo.ReadOnly);

			BatchPoster.APB_AK = testBookWithAutoAllocation.PK;
			AssertEquals("Cheque number should be reset", true, BatchPoster.APB_ChequeOrReference.IsEmpty);
			AssertEquals("Cheque number should be read only", true, BatchPoster.APB_ChequeOrReference_ReadOnly);
			AssertEquals("Cheque number should be read only", true, BatchPoster.APB_ChequeOrReferenceInfo.ReadOnly);

			BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			BatchPoster.APB_AK = ZGuid.Empty;
			BatchPoster.APB_AK = testBookWithAutoAllocation.PK;
			AssertEquals("Cheque Number should be populated", "CSH", BatchPoster.APB_ChequeOrReference);
		}

		#endregion

		#region AccPaymentBatch Tests

		public void TestSetupPaymentsInitWithExistingPaymentBatch()
		{
			SetupDataForBaseTest(false);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1.5m, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2.5m, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));

			var invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV", TestObjectCreator.USD, 0.7m, 700m, 0m, 1000m, 0m);
			invoice1.AH_OH = TestObjectCreator.Creditor2.PK;
			var invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV", TestObjectCreator.CNY, 1m, 1000m, 0m, 1000m, 0m);
			invoice2.AH_OH = TestObjectCreator.Creditor1.PK;
			Factory.Save();

			var transactions = new TransactionHeaderCollection(Factory);
			transactions.Add(invoice1);
			transactions.Add(invoice2);

			var today = ZDateTime.Now;
			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.SetDefaultValuesByTransactions(transactions);
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_AK = TestCheques.PK;
			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			BatchPoster.APB_ChequeOrReference = "0012345";
			BatchPoster.APB_PaymentDate = today.AddDays(3);
			BatchPoster.APB_PostDate = today;

			var payment1 = BatchPoster.PaymentApprovalCollection.OfType<PaymentApprovalBase>().Single(x => x.AV_PayExRate == 2.5m);
			var payment2 = BatchPoster.PaymentApprovalCollection.OfType<PaymentApprovalBase>().Single(x => x.AV_PayExRate == 1.5m);
			var paymentMatchingBaseObject1 = payment1.MatchingBaseObject;
			var paymentMatchingBaseObject2 = payment2.MatchingBaseObject;
			AssertEquals("Percondition", -600m, paymentMatchingBaseObject1.Balance);
			AssertEquals("Percondition", -533.33m, paymentMatchingBaseObject2.Balance);
			var exchangeDifference = paymentMatchingBaseObject1.GetMiscellaneousTransaction(TransactionTypes.ExchangeDifference);
			var discount = paymentMatchingBaseObject1.GetMiscellaneousTransaction(TransactionTypes.Discount);
			exchangeDifference.BindableOSAmount = 400m;
			discount.BindableOSAmount = 200m;
			paymentMatchingBaseObject1.AddMiscellaneousTransaction(exchangeDifference);
			paymentMatchingBaseObject1.AddMiscellaneousTransaction(discount);

			AssertEquals(400m, payment1.AV_Calc_LocalAmount);
			AssertEquals(400m, payment1.ExchangeDifferenceAmount);
			AssertEquals(200m, payment1.DiscountAmount);
			AssertEquals(3, payment1.MatchingBaseObject.MatchedTransactions.Count);
			AssertEquals(-400m, payment1.MatchingBaseObject.MatchedTransactions.Balance);
			AssertEquals(0m, payment1.MatchingBaseObject.Balance);

			AssertEquals(466.67m, payment2.AV_Calc_LocalAmount);
			AssertEquals(0m, payment2.ExchangeDifferenceAmount);
			AssertEquals(0m, payment2.DiscountAmount);
			AssertEquals(1, payment2.MatchingBaseObject.MatchedTransactions.Count);
			AssertEquals(-1000m, payment2.MatchingBaseObject.MatchedTransactions.Balance);
			AssertEquals(-533.33m, payment2.MatchingBaseObject.Balance);

			using (Factory.SetTempContext(BusinessContext.SavingPaymentApprovalAsDraft))
			{
				BatchPoster.MatchTransactions();
				Factory.Save();
			}

			var newFactory = new BusinessObjectFactory();
			var accPaymentBatch = newFactory.Load<APPaymentBatchPoster>(BatchPoster.PK);
			BatchPoster = accPaymentBatch;
			BatchPoster.LoadPayments();
			AssertEquals(false, BatchPoster.HasChanges);

			AssertEquals(TestBank.PK, BatchPoster.APB_AB);
			AssertEquals(TestCheques.PK, BatchPoster.APB_AK);
			AssertEquals(ReceiptTypes.Cheque, BatchPoster.APB_PaymentType);
			AssertEquals("0012345", BatchPoster.APB_ChequeOrReference);
			AssertEquals(today.AddDays(3).ToSmallDateTimeFloor(), BatchPoster.APB_PaymentDate);
			AssertEquals(today.ToSmallDateTimeFloor(), BatchPoster.APB_PostDate);

			AssertEquals(2, BatchPoster.CurrencySummary.SummaryRows.Count);
			AssertEquals(1.5m, BatchPoster.CurrencySummary.SummaryRows.OfType<CurrencySummaryRow>().Single(x => x.Currency == TestObjectCreator.USD.Code && x.Amount == 700M).ExchangeRate);
			AssertEquals(2.5m, BatchPoster.CurrencySummary.SummaryRows.OfType<CurrencySummaryRow>().Single(x => x.Currency == TestObjectCreator.CNY.Code && x.Amount == 1000M).ExchangeRate);

			payment1 = BatchPoster.PaymentApprovalCollection.OfType<PaymentApprovalBase>().Single(x => x.AV_PayExRate == 2.5m);
			payment2 = BatchPoster.PaymentApprovalCollection.OfType<PaymentApprovalBase>().Single(x => x.AV_PayExRate == 1.5m);

			AssertEquals(400m, payment1.AV_Calc_LocalAmount);
			AssertEquals(400m, payment1.ExchangeDifferenceAmount);
			AssertEquals(200m, payment1.DiscountAmount);
			AssertEquals(4, payment1.MatchingBaseObject.MatchedTransactions.Count);
			AssertEquals(0m, payment1.MatchingBaseObject.MatchedTransactions.Balance);
			AssertEquals(0m, payment1.MatchingBaseObject.Balance);

			AssertEquals(466.67m, payment2.AV_Calc_LocalAmount);
			AssertEquals(0m, payment2.ExchangeDifferenceAmount);
			AssertEquals(0m, payment2.DiscountAmount);
			AssertEquals(2, payment2.MatchingBaseObject.MatchedTransactions.Count);
			AssertEquals(-533.33m, payment2.MatchingBaseObject.MatchedTransactions.Balance);
			AssertEquals(-533.33m, payment2.MatchingBaseObject.Balance);

			AssertType<APPaymentBatchApprovalMatching>(payment1.MatchingBaseObject);
			AssertType<APPaymentBatchApprovalMatching>(payment2.MatchingBaseObject);
		}

		public void TestInitWithExistingPaymentBatch_ShouldPostPayment()
		{
			AssertInitWithExistingPaymentBatchCore_ShouldNotPostPayment(true);
		}

		public void TestInitWithExistingPaymentBatch_ShouldNotPostPayment()
		{
			AssertInitWithExistingPaymentBatchCore_ShouldNotPostPayment(false);
		}

		public void TestInitWithExistingPaymentBatch_WhenIsNotCheque()
		{
			SetupDataForBaseTest(false);

			var today = ZDateTime.Now;
			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_AK = TestCheques.PK;
			BatchPoster.APB_ChequeOrReference = "0012345";
			BatchPoster.APB_PaymentType = ReceiptTypes.Cash;
			BatchPoster.APB_PaymentDate = today.AddDays(3);
			BatchPoster.APB_PostDate = today;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var accPaymentBatch = newFactory.Load<APPaymentBatchPoster>(BatchPoster.PK);
			BatchPoster = accPaymentBatch;

			AssertEquals("Percondition", ReceiptTypes.Cheque, AccountingConfigurationRegistry.Instance.DefaultPaymentType.Value);

			AssertEquals(TestBank.PK, BatchPoster.APB_AB);
			AssertEquals(true, BatchPoster.APB_AK.IsEmpty);
			AssertEquals(ReceiptTypes.Cash, BatchPoster.APB_PaymentType);
			AssertEquals(ReceiptTypes.Cash, BatchPoster.APB_ChequeOrReference);
			AssertEquals(today.AddDays(3).ToSmallDateTimeFloor(), BatchPoster.APB_PaymentDate);
			AssertEquals(today.ToSmallDateTimeFloor(), BatchPoster.APB_PostDate);
		}

		void AssertInitWithExistingPaymentBatchCore_ShouldNotPostPayment(bool shouldPostPaymentsAsPaymentApprovals)
		{
			SetupDataForBaseTest(false);

			using (AccountingConfigurationRegistry.Instance.PayInvoicesDefaultPostPaymentsAsPaymentApprovals.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, shouldPostPaymentsAsPaymentApprovals))
			{
				var apInv = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1, ZDateTime.Today);
				var transactions = new TransactionHeaderCollection(Factory);
				transactions.Add(apInv);
				BatchPoster = Factory.New<APPaymentBatchPoster>();
				BatchPoster.SetDefaultValuesByTransactions(transactions);
				BatchPoster.APB_AB = TestBank.PK;
				BatchPoster.APB_AK = TestCheques.PK;
				BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
				BatchPoster.APB_ChequeOrReference = "0012345";
				BatchPoster.MatchTransactions();
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var accPaymentBatch = newFactory.Load<APPaymentBatchPoster>(BatchPoster.PK);
				BatchPoster = accPaymentBatch;
				BatchPoster.LoadPayments();
				AssertEquals(false, BatchPoster.HasChanges);
				AssertEquals(1, BatchPoster.PaymentApprovalCollection.Count);
				AssertEquals(false, BatchPoster.PaymentApprovalCollection[0].ShouldPostPayment_ForTestOnly);
			}
		}

		public void TestAccPaymentBatchpropertiesSetViaAPPaymentBatchPoster()
		{
			SetupDataForBaseTest();
			var accPaymentBatch = BatchPoster;

			BatchPoster.APB_AB = ZGuid.Empty;
			BatchPoster.APB_AK = ZGuid.Empty;
			BatchPoster.APB_PaymentType = ZString.Empty;
			BatchPoster.APB_ChequeOrReference = ZString.Empty;
			BatchPoster.APB_PaymentDate = ZDateTime.Empty;
			BatchPoster.APB_PostDate = ZDateTime.Empty;

			AssertEquals(ZGuid.Empty, accPaymentBatch.APB_AB);
			AssertEquals(ZGuid.Empty, accPaymentBatch.APB_AK);
			AssertEquals(ZString.Empty, accPaymentBatch.APB_PaymentType);
			AssertEquals(ZString.Empty, accPaymentBatch.APB_ChequeOrReference);
			AssertEquals(ZDateTime.Empty, accPaymentBatch.APB_PaymentDate);
			AssertEquals(ZDateTime.Empty, accPaymentBatch.APB_PostDate);

			var today = ZDateTime.Now;
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_AK = TestCheques.PK;
			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			BatchPoster.APB_ChequeOrReference = "0012345";
			BatchPoster.APB_PaymentDate = today.AddDays(3);
			BatchPoster.APB_PostDate = today;

			AssertEquals(TestBank.PK, accPaymentBatch.APB_AB);
			AssertEquals(TestCheques.PK, accPaymentBatch.APB_AK);
			AssertEquals(ReceiptTypes.Cheque, accPaymentBatch.APB_PaymentType);
			AssertEquals("0012345", accPaymentBatch.APB_ChequeOrReference);
			AssertEquals(today.AddDays(3), accPaymentBatch.APB_PaymentDate);
			AssertEquals(today, accPaymentBatch.APB_PostDate);
		}

		public void TestPaymentApprovalCollectionContainsAllApprovalsAfterAllApprovalsAreCreated()
		{
			var apInv1 = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1, ZDateTime.Today);
			var apInv2 = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor2, ZDateTime.Today);

			var transactions = new TransactionHeaderCollection(Factory);
			transactions.Add(apInv1);
			transactions.Add(apInv2);

			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.SetDefaultValuesByTransactions(transactions);

			AssertEquals("2 approvals should be created.", 2, BatchPoster.PaymentApprovalCollection.Count);
			AssertEquals("PaymentApprovals collection should contain the 2 approvals created by batch poster.", 2, BatchPoster.PaymentApprovalCollection.Count);
		}

		public void TestApplyExchangeGainLossToAllSingleForeignCurrencyPayments()
		{
			SetupDataForBaseTest(false);

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1.5m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2.5m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.TWD, "BUY", 3.5m);

			BatchPoster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			AssertEquals(false, BatchPoster.HasChanges);

			var payment1 = BatchPoster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
			var payment2 = BatchPoster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
			var payment3 = BatchPoster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);
			payment1.AV_Status = PaymentApprovalStatus.Cancelled;
			payment2.AV_Status = PaymentApprovalStatus.Posted;
			payment3.AV_Status = PaymentApprovalStatus.FullyApproved;
			Factory.Save();

			AssertEquals("Percondition", true, payment1.IsInDatabase);
			AssertEquals("Percondition", true, payment2.IsInDatabase);
			AssertEquals("Percondition", true, payment3.IsInDatabase);
			AssertEquals("Percondition", -333.3300m, payment1.MatchingBaseObject.Balance);
			AssertEquals("Percondition", -200.0000m, payment2.MatchingBaseObject.Balance);
			AssertEquals("Percondition", -142.8600m, payment3.MatchingBaseObject.Balance);
			AssertNull("Percondition", payment1.MatchingBaseObject.ExchangeDiffCurrent);
			AssertNull("Percondition", payment2.MatchingBaseObject.ExchangeDiffCurrent);
			AssertNull("Percondition", payment3.MatchingBaseObject.ExchangeDiffCurrent);

			var notifications = new NotificationCollection();
			BatchPoster.ApplyExchangeGainLossToAllSingleForeignCurrencyPayments(notifications);
			AssertEquals("User notification", "One or more payments were skipped because they were Posted Or Canceled.", notifications[0].Message);

			AssertNull(payment1.MatchingBaseObject.ExchangeDiffCurrent);
			AssertNull(payment2.MatchingBaseObject.ExchangeDiffCurrent);
			AssertEquals(142.8600m, payment3.MatchingBaseObject.ExchangeDiffCurrent.AH_OutstandingAmount);

			AssertEquals(-333.3300m, payment1.MatchingBaseObject.Balance);
			AssertEquals(-200.0000m, payment2.MatchingBaseObject.Balance);
			AssertEquals(0m, payment3.MatchingBaseObject.Balance);
		}

		protected override AccPaymentBatch GetFirstCreatedAccPaymentBatch()
		{
			SetupDataForBaseTest();
			return BatchPoster;
		}

		#endregion

		#region Concurrency Check Tests

		public void TestPaymentApprovalChangedAfterQuoteCreatedByAnotherUser()
		{
			var paymentBatch = Factory.New<APPaymentBatchPoster>();
			paymentBatch.APB_AB = TestObjectCreator.AUDBankAccount.PK;
			var approval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval.AV_Amount = 200m;
			approval.AV_Status = PaymentApprovalStatus.Draft;
			approval.InitializeForPaymentBatch(() => false);
			approval.AV_APB_PaymentBatch = paymentBatch.PK;
			Factory.Save();

			paymentBatch.ClearPaymentApprovalCollection_ForTestOnly();
			paymentBatch.LoadPayments();

			foreach (PaymentApprovalBase approvalFromPaymentBatch in paymentBatch.PaymentApprovalCollection)
			{
				AssertEquals(true, approvalFromPaymentBatch.IsLoadedFromPaymentBatch);
				AssertEquals(0, approvalFromPaymentBatch.PaymentQuotes.Count);
			}

			CreateQuoteInAnotherFactory(approval);

			approval.AV_Amount -= 10m;
			AssertExceptionThrown<ZCannotSaveException>(() => Factory.Save());

			var newFactory = new BusinessObjectFactory() { NameForDebugging = "NewFactoryForReload" };
			var testObjectCreatorInNewFactory = new TestObjectCreator(newFactory);
			var paymentBatchInNewFactory = newFactory.Load<APPaymentBatchPoster>(paymentBatch.PK);
			paymentBatchInNewFactory.LoadPayments();
			paymentBatch.PaymentApprovalCollection[0].AV_Amount -= 10m;
			AssertNoExceptionThrown(() => newFactory.Save());
		}

		public void TestFundingCurrencyChangedAfterQuoteCreatedByAnotherUser()
		{
			var paymentBatch = Factory.New<APPaymentBatchPoster>();
			paymentBatch.APB_AB = TestObjectCreator.AUDBankAccount.PK;
			var approval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval.AV_Amount = 200m;
			approval.AV_Status = PaymentApprovalStatus.Draft;
			approval.InitializeForPaymentBatch(() => false);
			approval.AV_APB_PaymentBatch = paymentBatch.PK;
			Factory.Save();

			paymentBatch.ClearPaymentApprovalCollection_ForTestOnly();
			paymentBatch.LoadPayments();

			foreach (PaymentApprovalBase approvalFromPaymentBatch in paymentBatch.PaymentApprovalCollection)
			{
				AssertEquals(true, approvalFromPaymentBatch.IsLoadedFromPaymentBatch);
				AssertEquals(0, approvalFromPaymentBatch.PaymentQuotes.Count);
			}

			CreateQuoteInAnotherFactory(approval);

			AssertEquals(ZGuid.Empty, paymentBatch.APB_AB_FundingBankAccount);
			paymentBatch.APB_AB_FundingBankAccount = TestObjectCreator.USDBankAccount.PK;
			AssertExceptionThrown<ZCannotSaveException>(() => Factory.Save());

			var newFactory = new BusinessObjectFactory() { NameForDebugging = "NewFactoryForReload" };
			var testObjectCreatorInNewFactory = new TestObjectCreator(newFactory);
			var paymentBatchInNewFactory = newFactory.Load<APPaymentBatchPoster>(paymentBatch.PK);
			paymentBatchInNewFactory.LoadPayments();
			AssertEquals(ZGuid.Empty, paymentBatchInNewFactory.APB_AB_FundingBankAccount);
			paymentBatchInNewFactory.APB_AB_FundingBankAccount = testObjectCreatorInNewFactory.USDBankAccount.PK;
			AssertNoExceptionThrown(() => newFactory.Save());
		}

		AccEPaymentQuote CreateQuoteInAnotherFactory(PaymentApprovalBase paymentApproval)
		{
			var tempFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var originQuotes = tempFactory.Load<AccEPaymentQuote>(new ZQuery());
			foreach (var originQuote in originQuotes)
			{
				originQuote.QU_Status = EPaymentStatusCodes.Quote.Discarded;
			}

			var testObjectCreatorInTempFactory = new TestObjectCreator(tempFactory);
			var quote = testObjectCreatorInTempFactory.CreateValidEPaymentQuoteForStatus(EPaymentStatusCodes.Quote.Queued, paymentApproval);
			tempFactory.Save();

			return quote;
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			SetupDataForPostingTest();
			BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			return BatchPoster;
		}

		PaymentBatchPostingTestHelper BatchPostingHelper => batchPostingHelper ?? (batchPostingHelper = new PaymentBatchPostingTestHelper(Factory));
		PaymentBatchPostingTestHelper batchPostingHelper;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		OrgHeader TestOrg;
		OrgHeader TestOrg2;
		OrgHeader TestOrg3;
		OrgHeader TestOrg6;
		OrgHeader TestOrg7;
		APInvoice TestAPInv;
		APInvoice TestAPInv2;
		APInvoice TestAPInv3;
		APInvoice TestAPInv4;
		APInvoice TestAPInv5;
		APInvoice TestAPInv6;
		APReceipt TestAPRec;
		APReceipt TestAPRec2;

		APPaymentBatchPoster BatchPoster;
		AccBankAccount TestBank;
		AccBankAccount AnotherBank;
		AccChequeBook TestCheques;

		PaymentApprovalBase PaymentApproval1;
		PaymentApprovalBase PaymentApproval2;
		PaymentApprovalBase PaymentApproval3;

		#endregion
	}
}
