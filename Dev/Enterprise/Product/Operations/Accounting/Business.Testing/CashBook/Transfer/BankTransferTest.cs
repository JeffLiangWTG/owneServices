using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccTransactionHeader;
using DealStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Deal;

namespace Enterprise.Accounting.Business.CashBook.Transfer.Testing
{
	[TestedType(typeof(BankTransfer))]
	public class BankTransferTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2023, 1, 1)]
		public void TestCreateExchangeDiff()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 1m);

			var bankTransfer = TestObjectCreator.CreateBankTransfer(ZDateTime.Today, TestObjectCreator.AUDBankAccount.PK, TestObjectCreator.USDBankAccount.PK, 200m, 1m);
			bankTransfer.ShouldCalculateExchangeVariance = true;
			bankTransfer.LocalSellAmount = 200m;

			AssertEquals("Pre-requisite", 0m, bankTransfer.ExRateGainLoss);
			Factory.Save();
			AssertEquals("CashbookExchangeDiff should not be saved when ExRateGainLoss is zero.", false, bankTransfer.ExchangeDiff.IsInDatabase);

			bankTransfer = TestObjectCreator.CreateBankTransfer(ZDateTime.Today, TestObjectCreator.AUDBankAccount.PK, TestObjectCreator.USDBankAccount.PK, 200m, 1m);
			bankTransfer.ShouldCalculateExchangeVariance = true;
			bankTransfer.LocalSellAmount = 100m;

			var errorMessage = AssertExceptionThrown<ZCannotSaveException>("Should have save exception when created CashbookExchangeDiff has validation error.", () => Factory.Save()).Message;
			AssertContains("This date does not fall into a valid accounting period’s date range.", errorMessage);

			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);
			Factory.Save();

			AssertEquals("CashbookExchangeDiff should be created when registry is on and ExRateGainLoss is not zero.", true, bankTransfer.ExchangeDiff.IsInDatabase);
		}

		[TestDate(2023, 1, 1)]
		public void TestCreateExchangeDiff_TransactionBelongsToGroup()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 1m);

			var bankTransfer = TestObjectCreator.CreateBankTransfer(ZDateTime.Today, TestObjectCreator.AUDBankAccount.PK, TestObjectCreator.USDBankAccount.PK, 200m, 1m);
			bankTransfer.ShouldCalculateExchangeVariance = true;
			bankTransfer.LocalSellAmount = 100m;
			Factory.Save();

			var query = new ZQuery(AccTransactionHeaderSchema.AH_AB, bankTransfer.BankTransferToPK);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, TransactionCategory.Codes.RealizedExchangeGainLoss);

			var exchangeDiff = Factory.LoadTop1<CashbookExchangeDiff>(query);
			AssertEquals(bankTransfer.TransactionBelongsToGroup, exchangeDiff.AH_TransactionBelongsToGroup);
		}

		public void TestPrepareBankTransferFromPayments()
		{
			AssertBankTransferPopulation(true, false, true);
			AssertBankTransferPopulation(false, false, true);
			AssertBankTransferPopulation(true, true, true);
			AssertBankTransferPopulation(false, true, true);
			AssertBankTransferPopulation(true, false, false);
			AssertBankTransferPopulation(false, false, false);
			AssertBankTransferPopulation(true, true, false);
			AssertBankTransferPopulation(false, true, false);
		}

		[TestDate(2021, 11, 15)]
		void AssertBankTransferPopulation(bool hasFee, bool testMultiplePayments, bool isPostedFromPaymentBatch)
		{
			var exemptTaxPK = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "EXEMPT", AccTaxRate.Types.Exempt, 0).PK;
			var deal1 = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted);
			deal1.Quote.QU_FeeAmount = hasFee ? 120.2 : ZDecimal.Zero;
			var deal2 = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted);
			deal2.Quote.QU_FeeAmount = hasFee ? 23.5 : ZDecimal.Zero;
			Factory.Save();

			var approval1 = Factory.Load<PaymentApprovalBase>(deal1.Quote.PaymentApproval.PK);
			var payment1 = Factory.NewWithValidTestData<APPayment>();
			payment1.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			payment1.AH_GC = GlbCompany.CurrentCompany.PK;
			approval1.AV_AH = payment1.PK;

			var approval2 = Factory.Load<PaymentApprovalBase>(deal2.Quote.PaymentApproval.PK);
			var payment2 = Factory.NewWithValidTestData<APPayment>();
			payment2.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			payment2.AH_GC = GlbCompany.CurrentCompany.PK;
			approval2.AV_AH = payment2.PK;

			BankTransfer transfer;
			ZString expectedRef;
			ZDecimal totalCost;
			ZDecimal totalFees;
			if (!testMultiplePayments)
			{
				ZString? paymentBatchRef = null;
				if (isPostedFromPaymentBatch)
				{
					paymentBatchRef = "ABCDEFG";
					expectedRef = ((ZString)(approval1.DealProvider + " PAYMENT BATCH " + paymentBatchRef)).Truncate(AccTransactionHeaderSchema.AH_ChequeOrReference.MaxLength);
				}
				else
				{
					expectedRef = ((ZString)(approval1.DealProvider + " PAYMENT " + payment1.AH_TransactionNum)).Truncate(AccTransactionHeaderSchema.AH_ChequeOrReference.MaxLength);
				}
				totalCost = payment1.DealTotalCost;
				totalFees = payment1.DealTotalFees;
				transfer = BankTransfer.PrepareBankTransferFromPayments(new List<Payment> { payment1 }, paymentBatchRef);
			}
			else
			{
				ZString? paymentBatchRef = null;
				if (isPostedFromPaymentBatch)
				{
					paymentBatchRef = "ABCDEFG";
					expectedRef = ((ZString)(approval1.DealProvider + " PAYMENT BATCH " + paymentBatchRef)).Truncate(AccTransactionHeaderSchema.AH_ChequeOrReference.MaxLength);
				}
				else
				{
					expectedRef = approval1.DealProvider + " MULTIPLE PAYMENTS";
				}
				totalCost = payment1.DealTotalCost + payment2.DealTotalCost;
				totalFees = payment1.DealTotalFees + payment2.DealTotalFees;
				transfer = BankTransfer.PrepareBankTransferFromPayments(new List<Payment> { payment1, payment2 }, paymentBatchRef);
			}

			AssertEquals("The To-Bank-Account should be the EPA account used in payment", payment1.AH_AB, transfer.BankTransferToPK);
			AssertEquals("Transaction Date should be today", ZDateTime.Today, transfer.TransactionDate);
			AssertEquals("Post Date should be today", ZDateTime.Today, transfer.AH_PostDate);
			AssertEquals(expectedRef, transfer.Reference);
			AssertEquals(totalCost, transfer.SellAmount);
			AssertEquals(totalCost, transfer.BuyAmount);
			AssertEquals(totalCost, transfer.LocalBuyAmount);

			AssertEquals("Enable Financial Charge boolean should be true if deal total fees > 0", totalFees > 0, transfer.EnableFinanceCharge);
			var expectedFinanceChargeBankPK = transfer.EnableFinanceCharge ? TestObjectCreator.AUDBankAccount.PK : ZGuid.Empty;
			AssertEquals("Finance Charge Bank should be equal to payment bank", expectedFinanceChargeBankPK, transfer.FinanceChargeBankPK);
			var expectedTotalFees = transfer.EnableFinanceCharge ? totalFees : ZDecimal.Zero;
			AssertEquals("Finance Charge Overseas Amount should be equal to deal total fees if finance charge enabled", expectedTotalFees, transfer.FinanceChargeOSAmount);
			var expectedTaxDate = transfer.EnableFinanceCharge ? ZDate.Today : ZDate.Empty;
			AssertEquals("Finance Charge Tax Date should be today if finance charge enabled", expectedTaxDate, transfer.FinanceChargeTaxDate);

			var expectedTaxID = transfer.EnableFinanceCharge ? exemptTaxPK : ZGuid.Empty;
			AssertEquals("Finance Charge Tax ID should be equal to GUID of PK of AccTaxRate with code 'EXEMPT' in company's country if finance charge is enabled", expectedTaxID, transfer.FinanceChargeTaxID);
		}

		public void TestPrepareBankTransferFromPaymentsForEPaymentWithForeignCurrencyWithoutBatch()
		{
			var payment1 = CreatePayment();
			var approval1 = CreatePaymentApproval(ZGuid.Empty, payment1.PK);
			var payment2 = CreatePayment();
			var approval2 = CreatePaymentApproval(ZGuid.Empty, payment2.PK);

			approval1.FundingBankAccountPK = TestObjectCreator.USDBankAccount.PK;
			approval2.FundingBankAccountPK = TestObjectCreator.USDBankAccount.PK;
			CreateQuote(approval1, "USD");
			CreateQuote(approval2, "USD");
			Factory.Save();

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 0.5m);
			var transfer = BankTransfer.PrepareBankTransferFromPayments(new List<Payment> { payment1, payment2 });
			AssertEquals("USD", transfer.BankTransferFundingInfoCalculator_ForTestOnly.FundingCurrency);
			AssertEquals(payment1.FundingBankAccountPK, transfer.BankTransferFromPK);
			AssertEquals(payment1.AH_AB, transfer.BankTransferToPK);
			AssertEquals(840m, transfer.SellAmount);
			AssertEquals(0.5m, transfer.SellExchangeRate);
			AssertEquals(80m, transfer.FinanceChargeOSAmount);
			AssertEquals(880m, transfer.BuyAmount);
			AssertEquals(true, transfer.ShouldCalculateExchangeVariance);
			AssertEquals(true, transfer.BankTransferToPK_ReadOnly);
			AssertHasWarning(transfer.FinanceChargeOSAmountInfo, "Finance Charge is calculated from Total Provider Fees in Funding Currency using the BUY exchange rate. If you override the Sell Exchange Rate, please also re-calculate and update Finance Charge Amount.");
		}

		public void TestPrepareBankTransferFromPaymentsForEPaymentWithLocalCurrencyWithoutBatch()
		{
			var payment1 = CreatePayment();
			var approval1 = CreatePaymentApproval(ZGuid.Empty, payment1.PK);
			var payment2 = CreatePayment();
			var approval2 = CreatePaymentApproval(ZGuid.Empty, payment2.PK);

			approval1.FundingBankAccountPK = TestObjectCreator.AUDBankAccount.PK;
			approval2.FundingBankAccountPK = TestObjectCreator.AUDBankAccount.PK;
			CreateQuote(approval1, "AUD");
			CreateQuote(approval2, "AUD");
			Factory.Save();

			var transfer = BankTransfer.PrepareBankTransferFromPayments(new List<Payment> { payment1, payment2 });
			AssertEquals("AUD", transfer.BankTransferFundingInfoCalculator_ForTestOnly.FundingCurrency);
			AssertEquals(payment1.FundingBankAccountPK, transfer.BankTransferFromPK);
			AssertEquals(payment1.AH_AB, transfer.BankTransferToPK);
			AssertEquals(840m, transfer.SellAmount);
			AssertEquals(1m, transfer.SellExchangeRate);
			AssertEquals(40m, transfer.FinanceChargeOSAmount);
			AssertEquals(840m, transfer.BuyAmount);
			AssertEquals(false, transfer.ShouldCalculateExchangeVariance);
			AssertEquals(true, transfer.BankTransferToPK_ReadOnly);
		}

		public void TestPrepareBankTransferFromPaymentsForEPaymentWithoutTodayExchangeRateWithoutBatch()
		{
			var payment = CreatePayment();
			var approval = CreatePaymentApproval(ZGuid.Empty, payment.PK);
			approval.FundingBankAccountPK = TestObjectCreator.USDBankAccount.PK;
			CreateQuote(approval, "USD");
			Factory.Save();

			var expectError = "Finance Charge is calculated from Total Provider Fees in Funding Currency using the Funding Currency's BUY rate. To post this Bank Transfer, please calculate the Bank Charge using the Funding Currency's exchange rate as entered.";
			var expectWarning = "Finance Charge is calculated from Total Provider Fees in Funding Currency using the BUY exchange rate. If you override the Sell Exchange Rate, please also re-calculate and update Finance Charge Amount.";
			var transfer = BankTransfer.PrepareBankTransferFromPayments(new List<Payment> { payment });
			AssertHasError(transfer.FinanceChargeOSAmountInfo, expectError);
			AssertNoWarning(transfer.FinanceChargeOSAmountInfo, expectWarning);

			transfer.FinanceChargeOSAmount = 10m;
			AssertNoError(transfer.FinanceChargeOSAmountInfo, expectError);
			AssertHasWarning(transfer.FinanceChargeOSAmountInfo, expectWarning);
		}

		public void TestPrepareBankTransferFromPaymentsForEPaymentWithForeignCurrencyWithBatch()
		{
			var paymentBatch = CreatePaymentBatch(TestObjectCreator.USDBankAccount.PK);
			var payment = CreatePayment();
			var approval = CreatePaymentApproval(paymentBatch.PK, payment.PK);
			CreateQuote(approval, "USD");
			Factory.Save();

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 0.5m);
			var transfer = BankTransfer.PrepareBankTransferFromPayments(new List<Payment> { payment }, paymentBatch.APB_BatchNumber);
			AssertEquals("USD", transfer.BankTransferFundingInfoCalculator_ForTestOnly.FundingCurrency);
			AssertEquals(paymentBatch.APB_AB_FundingBankAccount, transfer.BankTransferFromPK);
			AssertEquals(paymentBatch.APB_AB, transfer.BankTransferToPK);
			AssertEquals(420m, transfer.SellAmount);
			AssertEquals(0.5m, transfer.SellExchangeRate);
			AssertEquals(40m, transfer.FinanceChargeOSAmount);
			AssertEquals(440m, transfer.BuyAmount);
			AssertEquals(true, transfer.ShouldCalculateExchangeVariance);
			AssertEquals(true, transfer.BankTransferToPK_ReadOnly);
			AssertHasWarning(transfer.FinanceChargeOSAmountInfo, "Finance Charge is calculated from Total Provider Fees in Funding Currency using the BUY exchange rate. If you override the Sell Exchange Rate, please also re-calculate and update Finance Charge Amount.");
		}

		public void TestPrepareBankTransferFromPaymentsForEPaymentWithLocalCurrencyWithBatch()
		{
			var paymentBatch = CreatePaymentBatch(TestObjectCreator.AUDBankAccount.PK);
			var payment = CreatePayment();
			var approval = CreatePaymentApproval(paymentBatch.PK, payment.PK);
			CreateQuote(approval, "AUD");
			Factory.Save();

			var transfer = BankTransfer.PrepareBankTransferFromPayments(new List<Payment> { payment }, paymentBatch.APB_BatchNumber);
			AssertEquals("AUD", transfer.BankTransferFundingInfoCalculator_ForTestOnly.FundingCurrency);
			AssertEquals(paymentBatch.APB_AB_FundingBankAccount, transfer.BankTransferFromPK);
			AssertEquals(paymentBatch.APB_AB, transfer.BankTransferToPK);
			AssertEquals(420m, transfer.SellAmount);
			AssertEquals(1m, transfer.SellExchangeRate);
			AssertEquals(20m, transfer.FinanceChargeOSAmount);
			AssertEquals(420m, transfer.BuyAmount);
			AssertEquals(false, transfer.ShouldCalculateExchangeVariance);
			AssertEquals(true, transfer.BankTransferToPK_ReadOnly);
		}

		public void TestPrepareBankTransferFromPaymentsForEPaymentWithoutTodayExchangeRateWithBatch()
		{
			var paymentBatch = CreatePaymentBatch(TestObjectCreator.USDBankAccount.PK);
			var payment = CreatePayment();
			var approval = CreatePaymentApproval(paymentBatch.PK, payment.PK);
			CreateQuote(approval, "USD");
			Factory.Save();

			var expectError = "Finance Charge is calculated from Total Provider Fees in Funding Currency using the Funding Currency's BUY rate. To post this Bank Transfer, please calculate the Bank Charge using the Funding Currency's exchange rate as entered.";
			var expectWarning = "Finance Charge is calculated from Total Provider Fees in Funding Currency using the BUY exchange rate. If you override the Sell Exchange Rate, please also re-calculate and update Finance Charge Amount.";
			var transfer = BankTransfer.PrepareBankTransferFromPayments(new List<Payment> { payment }, paymentBatch.APB_BatchNumber);
			AssertHasError(transfer.FinanceChargeOSAmountInfo, expectError);
			AssertNoWarning(transfer.FinanceChargeOSAmountInfo, expectWarning);

			transfer.FinanceChargeOSAmount = 10m;
			AssertNoError(transfer.FinanceChargeOSAmountInfo, expectError);
			AssertHasWarning(transfer.FinanceChargeOSAmountInfo, expectWarning);
		}

		APPaymentApprovalWithoutAuthorisation CreatePaymentApproval(ZGuid paymentBatchPK, ZGuid paymentPK)
		{
			var approval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval.AV_AH = paymentPK;
			approval.AV_RX_NKPaymentCurrency = "CAD";
			approval.AV_Amount = 200m;
			approval.AV_PayExRate = 0.5m;
			approval.InitializeForPaymentBatch(() => false);
			approval.AV_APB_PaymentBatch = paymentBatchPK;
			return approval;
		}

		APPayment CreatePayment()
		{
			var payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			payment.AH_GC = GlbCompany.CurrentCompany.PK;
			return payment;
		}

		APPaymentBatchPoster CreatePaymentBatch(ZGuid fundingBankAccountPK)
		{
			var paymentBatch = Factory.New<APPaymentBatchPoster>();
			paymentBatch.APB_AB_FundingBankAccount = fundingBankAccountPK;
			paymentBatch.APB_AB = TestObjectCreator.AUDBankAccount.PK;
			return paymentBatch;
		}

		AccEPaymentQuote CreateQuote(APPaymentApprovalWithoutAuthorisation approval, ZString fromCurrency)
		{
			var deal = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, approval);
			deal.Quote.QU_FeeAmount = 20m;
			deal.Quote.QU_FromAmount = 400m;
			deal.Quote.QU_ExchangeRate = 0.5m;
			deal.Quote.QU_ExchangeRateInverted = 2m;
			deal.Quote.QU_ToAmount = 200m;
			deal.Quote.QU_RX_NKFromCurrency = fromCurrency;
			deal.Quote.QU_RX_NKToCurrency = "CAD";
			return deal.Quote;
		}

		public void TestBehaviourWhenInPopulateBankTransferWithEPaymentDataContext()
		{
			var testBankTransfer = new BankTransfer(Factory, null);
			TestBankTransfer.EnableFinanceCharge = true;
			testBankTransfer.FinanceChargeBankPK = TestObjectCreator.GBPBankAccount.PK;
			testBankTransfer.SetContext(BusinessContext.PopulateBankTransferWithEPaymentData);
			Assert(!testBankTransfer.BuySellAmountsAndRatesReadOnly);
			AssertEquals(ZGuid.Empty, testBankTransfer.BankTransferFromPK);
			AssertEquals(ZGuid.Empty, testBankTransfer.BankTransferToPK);
			testBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			AssertEquals(ZGuid.Empty, testBankTransfer.BankTransferFromPK);
			AssertEquals(TestObjectCreator.USDBankAccount.PK, testBankTransfer.BankTransferToPK);
			Assert(!testBankTransfer.BuySellAmountsAndRatesReadOnly);
			AssertEquals("USD", testBankTransfer.BuyCurrency);
			AssertEquals("USD", testBankTransfer.SellCurrency);
			testBankTransfer.SellAmount = 100m;
			testBankTransfer.BuyAmount = 100m;
			AssertEquals(100m, testBankTransfer.BuyAmount);
			AssertEquals(100m, testBankTransfer.SellAmount);
			AssertEquals(100m, testBankTransfer.LocalBuyAmount);
			testBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			AssertEquals(TestObjectCreator.AUDBankAccount.PK, testBankTransfer.BankTransferFromPK);
			AssertEquals(TestObjectCreator.USDBankAccount.PK, testBankTransfer.BankTransferToPK);
			AssertEquals(TestObjectCreator.GBPBankAccount.PK, testBankTransfer.FinanceChargeBankPK);
			AssertEquals("USD", testBankTransfer.BuyCurrency);
			AssertEquals("AUD", testBankTransfer.SellCurrency);
			Assert(!testBankTransfer.BuySellAmountsAndRatesReadOnly);
			AssertEquals("Buy amount should not be reset when From Bank Account is changed", 100m, testBankTransfer.BuyAmount);
			AssertEquals("Sell amount should not be reset when From Bank Account", 100m, testBankTransfer.SellAmount);
			AssertEquals("Local amount should not be reset when From Bank Account", 100m, testBankTransfer.LocalBuyAmount);
			testBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			AssertEquals("Buy amount should not be reset when To Bank Account is changed", 100m, testBankTransfer.BuyAmount);
			AssertEquals("Sell amount should not be reset when To Bank Account is changed", 100m, testBankTransfer.SellAmount);
			AssertEquals("LocalAmount amount should not be reset when To Bank Account is changed", 100m, testBankTransfer.LocalBuyAmount);
		}

		public void TestTransactionNumberGenerator()
		{
			TransactionNumberSequenceCustomisationCollection customisation = new TransactionNumberSequenceCustomisationCollection();
			TransactionNumberSequenceCustomisation element = customisation.AddNew();
			element.Order = 1;
			element.ElementName = TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderBranchCode;
			element.Include = true;
			element = customisation.AddNew();
			element.Length = 8;
			element.Order = 2;
			element.ElementName = TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber;
			element.Include = true;
			element = customisation.AddNew();
			element.Order = 50;
			element.ElementName = TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderDepartmentCode;
			element.Include = true;
			AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, customisation);

			Factory.Save();
			AssertEquals("Transaction number", "BNE00000001BRN", TestBankTransfer.TransferRowFrom.AH_TransactionNum);
			AssertEquals("Transaction number", "BNE00000001BRN", TestBankTransfer.TransferRowTo.AH_TransactionNum);
		}

		public void TestLocalAmountsAreSavedWithCorrectNumberOfDecimalPlaces()
		{
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();

			AccBankAccount foreignBank = Factory.NewWithValidTestData<AccBankAccount>();
			foreignBank.AB_RX_NKAccountCurrency = currency.RX_Code;

			AccBankAccount localBank = Factory.NewWithValidTestData<AccBankAccount>();
			foreignBank.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			TestBankTransfer.Reference = TestObjectCreator.GetRandomString(5);
			TestBankTransfer.SetFromBankAccountWithAsserts(foreignBank.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(localBank.PK);
			TestBankTransfer.SetSellAmountWithAsserts(30000000m);
			TestBankTransfer.SellZExchangeRate.Rate = 9060m;

			AssertEquals("Sell Amount", 30000000m, TestBankTransfer.SellAmount);
			AssertEquals("Sell Amount Exchange Rate", 9060m, TestBankTransfer.SellZExchangeRate.Rate);

			AssertEquals("Buy Amount", 3311.26m, TestBankTransfer.BuyAmount);
			AssertEquals("Buy Amount Exchange Rate", 1m, TestBankTransfer.BuyZExchangeRate.Rate);

			AssertEquals("Local Amount", 3311.26m, TestBankTransfer.LocalBuyAmount);

			AssertEquals("FromRow.AH_LocalExTaxAmount", 3311.26m, TestBankTransfer.TransferRowFrom.AH_LocalExTaxAmount);
			AssertEquals("FromRow.AH_OSExTaxAmount", 30000000m, TestBankTransfer.TransferRowFrom.AH_OSExTaxAmount);

			AssertEquals("ToRow.AH_LocalExTaxAmount", 3311.26m, TestBankTransfer.TransferRowTo.AH_LocalExTaxAmount);
			AssertEquals("ToRow.AH_OSExTaxAmount", 3311.26m, TestBankTransfer.TransferRowTo.AH_OSExTaxAmount);
		}

		#region Collections

		public void TestBankAccounts_ContainsOnlyActiveBanks()
		{
			AccBankAccount activeBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount inactiveBank = Factory.NewWithValidTestData<AccBankAccount>();
			inactiveBank.AB_IsActive = false;

			TestBankTransfer.BankAccounts.Load();
			AssertEquals("Should contain active bank", true, TestBankTransfer.BankAccounts.Contains(activeBank));
			AssertEquals("Should not contain inactive bank", false, TestBankTransfer.BankAccounts.Contains(inactiveBank));
		}

		public void TestTaxRates()
		{
			AccTaxRate activeTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			activeTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AccTaxRate otherCountryActiveTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			otherCountryActiveTaxRate.AT_RN_NKCountry = "GB";
			AccTaxRate inactiveTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			inactiveTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			inactiveTaxRate.AT_IsActive = false;

			TestBankTransfer.TaxRates.Load();
			AssertEquals("Should contain active tax rate", true, TestBankTransfer.TaxRates.Contains(activeTaxRate));
			AssertEquals("Should not contain other company tax rate", false, TestBankTransfer.TaxRates.Contains(otherCountryActiveTaxRate));
			AssertEquals("Should not contain inactive tax rate", false, TestBankTransfer.TaxRates.Contains(inactiveTaxRate));
		}

		public void TestTaxRatesCollection()
		{
			var rate = Factory.NewWithValidTestData<AccTaxRate>();
			rate.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			rate.AT_Type = AccTaxRate.Types.NotReportable;
			rate.AT_TaxSystemCode = "Other";

			var rate2 = Factory.NewWithValidTestData<AccTaxRate>();
			rate2.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			rate2.AT_Code = "BBCXYZ";
			rate2.AT_Type = AccTaxRate.Types.NotReportable;
			rate2.AT_TaxSystemCode = "Other1";

			Factory.Save();
			Assert("Collection should present only the VAT Tax System", TestBankTransfer.TaxRates.Cast<AccTaxRate>().All(item => item.AT_TaxSystemCode.IsEmpty));
		}

		#endregion

		public void TestSetFinanceChargeSetOnlyWhenFinanceChargeEnabled()
		{
			TestBankTransfer.EnableFinanceCharge = false;
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			AssertEquals(ZGuid.Empty, TestBankTransfer.FinanceCharge_ForTestOnly.AH_AB);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestBankTransfer.FinanceCharge_ForTestOnly.AH_RX_NKTransactionCurrency);
			TestBankTransfer.EnableFinanceCharge = true;
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.GBPBankAccount.PK);
			AssertEquals(TestObjectCreator.GBPBankAccount.PK, TestBankTransfer.FinanceCharge_ForTestOnly.AH_AB);
		}

		public void TestLoadedExistingRecordIsReadOnly()
		{
			TestBankTransfer.Reference = "Test";
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetSellAmountWithAsserts(1m);
			TestBankTransfer.SetBuyExchangeRateWithAsserts(0.7m);
			TestBankTransfer.SetSellAmountWithAsserts(100m);
			TestBankTransfer.FinanceChargeOSAmount = 10m;
			Factory.Save();

			BankTransfer loadedBankTransfer = new BankTransfer(new BusinessObjectFactory(), TestBankTransfer.TransferRowFrom);
			Assert("Should be read only", loadedBankTransfer.ReadOnly);
		}

		public void TestLoadExistingRecordsWithFinanceCharge()
		{
			TestBankTransfer.Reference = "Test";
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetSellAmountWithAsserts(1m);
			TestBankTransfer.SetBuyExchangeRateWithAsserts(0.7m);
			TestBankTransfer.SetSellAmountWithAsserts(100m);
			TestBankTransfer.EnableFinanceCharge = true;
			TestBankTransfer.FinanceChargeOSAmount = 10m;
			Factory.Save();

			BankTransfer loadedBankTransfer = new BankTransfer(new BusinessObjectFactory(), TestBankTransfer.TransferRowFrom);
			AssertEquals(TestBankTransfer.TransferRowFrom.PK, loadedBankTransfer.TransferRowFrom.PK);
			AssertEquals(TestBankTransfer.TransferRowTo.PK, loadedBankTransfer.TransferRowTo.PK);
			AssertEquals(true, loadedBankTransfer.EnableFinanceCharge);
			AssertEquals(TestBankTransfer.FinanceCharge_ForTestOnly.PK, loadedBankTransfer.FinanceCharge_ForTestOnly.PK);
		}

		public void TestLoadExistingRecordsWithoutFinanceCharge()
		{
			TestBankTransfer.Reference = "Test";
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetSellAmountWithAsserts(1m);
			TestBankTransfer.SetBuyExchangeRateWithAsserts(0.7m);
			TestBankTransfer.SetSellAmountWithAsserts(100m);
			TestBankTransfer.EnableFinanceCharge = false;
			Factory.Save();

			BankTransfer loadedBankTransfer = new BankTransfer(new BusinessObjectFactory(), TestBankTransfer.TransferRowFrom);
			AssertEquals(TestBankTransfer.TransferRowFrom.PK, loadedBankTransfer.TransferRowFrom.PK);
			AssertEquals(TestBankTransfer.TransferRowTo.PK, loadedBankTransfer.TransferRowTo.PK);
			AssertEquals(false, loadedBankTransfer.EnableFinanceCharge);
			AssertEquals(false, TestBankTransfer.FinanceCharge_ForTestOnly.PK == loadedBankTransfer.FinanceCharge_ForTestOnly.PK);
		}

		public void TestLoadExistingRecordsButExchangeDiffIsNotInDatabase()
		{
			TestBankTransfer.Reference = "Test";
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetSellAmountWithAsserts(1m);
			TestBankTransfer.SetBuyExchangeRateWithAsserts(0.7m);
			TestBankTransfer.SetSellAmountWithAsserts(100m);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedBankTransfer = new BankTransfer(newFactory, TestBankTransfer.TransferRowFrom);
			AssertEquals(TestBankTransfer.TransferRowFrom.PK, loadedBankTransfer.TransferRowFrom.PK);
			AssertEquals(TestBankTransfer.TransferRowTo.PK, loadedBankTransfer.TransferRowTo.PK);

			AssertEquals("Pre-requisite", false, loadedBankTransfer.ExchangeDiff.IsInDatabase);
			AssertNoExceptionThrown("Expect no error when reloading the same bank transfer multiple times, with new exchange rate difference.", () => new BankTransfer(newFactory, TestBankTransfer.TransferRowFrom));
		}

		public void TestSellAmountChanged()
		{
			//Sell = Local Currency, Buy = Foregin TransactionCurrency
			TestObjectCreator.AUDBankAccount.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);

			AssertEquals(1m, TestBankTransfer.SellExchangeRate);
			TestBankTransfer.SetBuyExchangeRateWithAsserts(0.5m);

			TestBankTransfer.SetSellAmountWithAsserts(100m);
			AssertEquals(50m, TestBankTransfer.BuyAmount);

			//Sell = Foregin Currency, Buy = Local TransactionCurrency
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestObjectCreator.AUDBankAccount.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);

			TestBankTransfer.SetSellExchangeRateWithAsserts(0.5m);
			AssertEquals(1m, TestBankTransfer.BuyExchangeRate);

			TestBankTransfer.SetSellAmountWithAsserts(100m);
			AssertEquals(200m, TestBankTransfer.BuyAmount);
		}

		public void TestSellAmountChangedForBothLocalCurrencyAccounts()
		{
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.AUDBankAccount2.PK);

			AssertEquals("SellExchangeRate should be 1", 1m, TestBankTransfer.SellExchangeRate);
			AssertEquals("BuyExchangeRate should be 1", 1m, TestBankTransfer.BuyExchangeRate);
			AssertEquals("BuyAmount", 0m, TestBankTransfer.BuyAmount);
			AssertEquals("SellAmount", 0m, TestBankTransfer.SellAmount);
			AssertEquals("LocalAmount", 0m, TestBankTransfer.LocalBuyAmount);

			TestBankTransfer.SetSellAmountWithAsserts(100m);
			AssertEquals("SellExchangeRate should be 1", 1m, TestBankTransfer.SellExchangeRate);
			AssertEquals("BuyExchangeRate should be 1", 1m, TestBankTransfer.BuyExchangeRate);
			AssertEquals("BuyAmount", 100m, TestBankTransfer.BuyAmount);
			AssertEquals("SellAmount", 100m, TestBankTransfer.SellAmount);
			AssertEquals("LocalAmount", 100m, TestBankTransfer.LocalBuyAmount);
		}

		public void TestBuyAmountChanged()
		{
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.GBPBankAccount.PK);

			TestBankTransfer.SetSellExchangeRateWithAsserts(0.8333m);
			TestBankTransfer.SetBuyExchangeRateWithAsserts(1m);

			TestBankTransfer.SetSellAmountWithAsserts(750m);
			AssertEquals(900.04m, TestBankTransfer.LocalBuyAmount);
			AssertEquals(900.04m, TestBankTransfer.BuyAmount);

			TestBankTransfer.SetBuyAmountWithAsserts(890m);
			AssertEquals(890m, TestBankTransfer.BuyAmount);
			AssertEquals(0.988845m, TestBankTransfer.BuyExchangeRate);
			AssertEquals(900.04m, TestBankTransfer.LocalBuyAmount);

			TestBankTransfer.SetBuyAmountWithAsserts(500);
			AssertEquals(500m, TestBankTransfer.BuyAmount);
			AssertEquals(0.555531m, TestBankTransfer.BuyExchangeRate);
			AssertEquals(900.04m, TestBankTransfer.LocalBuyAmount);
		}

		public void TestBuyAmountChangedForBothLocalCurrencyAccounts()
		{
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.AUDBankAccount2.PK);

			AssertEquals("SellExchangeRate should be 1", 1m, TestBankTransfer.SellExchangeRate);
			AssertEquals("BuyExchangeRate should be 1", 1m, TestBankTransfer.BuyExchangeRate);
			AssertEquals("BuyAmount", 0m, TestBankTransfer.BuyAmount);
			AssertEquals("SellAmount", 0m, TestBankTransfer.SellAmount);
			AssertEquals("LocalAmount", 0m, TestBankTransfer.LocalBuyAmount);

			TestBankTransfer.SetBuyAmountWithAsserts(100m);
			AssertEquals("SellExchangeRate should be 1", 1m, TestBankTransfer.SellExchangeRate);
			AssertEquals("BuyExchangeRate should be 1", 1m, TestBankTransfer.BuyExchangeRate);
			AssertEquals("BuyAmount", 100m, TestBankTransfer.BuyAmount);
			AssertEquals("SellAmount", 100m, TestBankTransfer.SellAmount);
			AssertEquals("LocalAmount", 100m, TestBankTransfer.LocalBuyAmount);
		}

		public void TestSellExchangeRateChanged()
		{
			//Sell = Foregin Currency, Buy = Local TransactionCurrency
			TestObjectCreator.AUDBankAccount.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);

			TestBankTransfer.SetSellAmountWithAsserts(100m);
			TestBankTransfer.SetSellExchangeRateWithAsserts(0.5m);
			AssertEquals(200m, TestBankTransfer.BuyAmount);
		}

		public void TestBuyExchangeRateChanged()
		{
			//Sell = Local Currency, Buy = Foregin TransactionCurrency
			TestObjectCreator.AUDBankAccount.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);

			TestBankTransfer.SetSellAmountWithAsserts(100m);
			TestBankTransfer.SetBuyExchangeRateWithAsserts(0.5m);
			AssertEquals(50m, TestBankTransfer.BuyAmount);
		}

		public void TestLocalAmountToBuyAmountRounding()
		{
			//Sell = Foregin Currency, Buy = Foregin TransactionCurrency
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.GBPBankAccount.PK);

			TestBankTransfer.SetSellExchangeRateWithAsserts(1.46m);
			TestBankTransfer.SetBuyExchangeRateWithAsserts(0.6m);

			TestBankTransfer.SetSellAmountWithAsserts(100m);
			AssertEquals(68.49m, TestBankTransfer.LocalBuyAmount);
			AssertEquals(41.10m, TestBankTransfer.BuyAmount);

			//Sell = Foregin Currency, Buy = Local TransactionCurrency
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			TestBankTransfer.SetBuyExchangeRateWithAsserts(1m);
			TestBankTransfer.SetSellExchangeRateWithAsserts(0.8333m);

			TestBankTransfer.SetSellAmountWithAsserts(750m);
			AssertEquals(900.04m, TestBankTransfer.BuyAmount);
			AssertEquals(900.04m, TestBankTransfer.LocalBuyAmount);
		}

		public void TestLocalBuyAmountChanged()
		{
			AssertEquals("PreCondition", false, TestBankTransfer.ShouldCalculateExchangeVariance);

			//Sell = Foreign Currency, Buy = Foreign Currency
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.GBPBankAccount.PK);
			TestBankTransfer.SetBuyExchangeRateWithAsserts(0.6m);

			TestBankTransfer.SetSellAmountWithAsserts(100m);
			TestBankTransfer.SetSellExchangeRateWithAsserts(0.5m);
			AssertEquals(120m, TestBankTransfer.BuyAmount);
			AssertEquals(200m, TestBankTransfer.LocalBuyAmount);
			TestBankTransfer.SetLocalBuyAmountWithAsserts(50m);
			AssertEquals(2m, TestBankTransfer.SellExchangeRate);
			AssertEquals(30m, TestBankTransfer.BuyAmount);

			//Sell = Foreign Currency, Buy = Local Currency
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			AssertEquals("Buy Exchange rate should be defaulted to 1 as BuyAmount and LocalAmount are of the same currency", 1m, TestBankTransfer.BuyExchangeRate);
			TestBankTransfer.SetSellExchangeRateWithAsserts(0.8333m);

			TestBankTransfer.SetSellAmountWithAsserts(750m);
			AssertEquals(900.04m, TestBankTransfer.BuyAmount);
			AssertEquals(900.04m, TestBankTransfer.LocalBuyAmount);

			TestBankTransfer.SetLocalBuyAmountWithAsserts(900m);
			AssertEquals(900m, TestBankTransfer.BuyAmount);
			AssertEquals(900m, TestBankTransfer.LocalBuyAmount);

			TestBankTransfer.SetLocalBuyAmountWithAsserts(100m);
			AssertEquals(100m, TestBankTransfer.BuyAmount);
			AssertEquals(100m, TestBankTransfer.LocalBuyAmount);
			AssertEquals(1m, TestBankTransfer.BuyExchangeRate);
			AssertEquals(7.5m, TestBankTransfer.SellExchangeRate);

			//Sell = Local Currency, Buy = Foreign Currency
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			AssertEquals("Sell exchange rate should be defaulted to 1 as SellAmount and LocalAmount are of the same currency", 1m, TestBankTransfer.SellExchangeRate);

			TestBankTransfer.SetBuyExchangeRateWithAsserts(0.666m);
			TestBankTransfer.SetLocalBuyAmountWithAsserts(150m);
			AssertEquals(99.9m, TestBankTransfer.BuyAmount);
			AssertEquals(150m, TestBankTransfer.LocalBuyAmount);
			AssertEquals(150m, TestBankTransfer.SellAmount);
			AssertEquals(0.666m, TestBankTransfer.BuyExchangeRate);
			AssertEquals(1m, TestBankTransfer.SellExchangeRate);

			TestBankTransfer.SetBuyAmountWithAsserts(100m);
			AssertEquals(100m, TestBankTransfer.BuyAmount);
			AssertEquals(150m, TestBankTransfer.LocalBuyAmount);
			AssertEquals(150m, TestBankTransfer.SellAmount);
			AssertEquals(0.666667m, TestBankTransfer.BuyExchangeRate);
			AssertEquals(1m, TestBankTransfer.SellExchangeRate);

			//Sell = Local Currency, Buy = Local Currency
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.AUDBankAccount2.PK);
			AssertEquals("Buy Exchange rate should be defaulted to 1 as BuyAmount and LocalAmount are of the same currency", 1m, TestBankTransfer.BuyExchangeRate);
			AssertEquals("Sell exchange rate should be defaulted to 1 as SellAmount and LocalAmount are of the same currency", 1m, TestBankTransfer.SellExchangeRate);

			TestBankTransfer.SetLocalBuyAmountWithAsserts(150m);
			AssertEquals(150m, TestBankTransfer.BuyAmount);
			AssertEquals(150m, TestBankTransfer.LocalBuyAmount);
			AssertEquals(150m, TestBankTransfer.SellAmount);
			AssertEquals(1m, TestBankTransfer.BuyExchangeRate);
			AssertEquals(1m, TestBankTransfer.SellExchangeRate);

			TestBankTransfer.SetBuyAmountWithAsserts(100m);
			AssertEquals(100m, TestBankTransfer.BuyAmount);
			AssertEquals(100m, TestBankTransfer.LocalBuyAmount);
			AssertEquals(100m, TestBankTransfer.SellAmount);
			AssertEquals(1m, TestBankTransfer.BuyExchangeRate);
			AssertEquals(1m, TestBankTransfer.SellExchangeRate);
		}

		public void TestLocalSellAmountChanged()
		{
			AssertEquals("PreCondition", false, TestBankTransfer.ShouldCalculateExchangeVariance);

			//Sell = Foreign Currency, Buy = Foreign Currency
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.GBPBankAccount.PK);
			TestBankTransfer.SetSellExchangeRateWithAsserts(0.5m);
			TestBankTransfer.SetBuyExchangeRateWithAsserts(0.6m);
			TestBankTransfer.SetSellAmountWithAsserts(50m);

			TestBankTransfer.SetLocalSellAmountWithAsserts(50m);

			AssertEquals(1m, TestBankTransfer.SellExchangeRate);
			AssertEquals(0.6m, TestBankTransfer.BuyExchangeRate);
			AssertEquals(50m, TestBankTransfer.SellAmount);
			AssertEquals(30m, TestBankTransfer.BuyAmount);
			AssertEquals(50m, TestBankTransfer.LocalSellAmount);
			AssertEquals(50m, TestBankTransfer.LocalBuyAmount);

			//Sell = Foreign Currency, Buy = Local Currency
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			AssertEquals("Buy Exchange rate should be defaulted to 1 as BuyAmount and LocalAmount are of the same currency", 1m, TestBankTransfer.BuyExchangeRate);
			TestBankTransfer.SetSellExchangeRateWithAsserts(0.5m);
			TestBankTransfer.SetSellAmountWithAsserts(100m);
			TestBankTransfer.SetLocalSellAmountWithAsserts(50m);

			AssertEquals(2m, TestBankTransfer.SellExchangeRate);
			AssertEquals(1m, TestBankTransfer.BuyExchangeRate);
			AssertEquals(100m, TestBankTransfer.SellAmount);
			AssertEquals(50m, TestBankTransfer.BuyAmount);
			AssertEquals(50m, TestBankTransfer.LocalSellAmount);
			AssertEquals(50m, TestBankTransfer.LocalBuyAmount);

			//Sell = Local Currency, Buy = Foreign Currency
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			AssertEquals("Sell exchange rate should be defaulted to 1 as SellAmount and LocalAmount are of the same currency", 1m, TestBankTransfer.SellExchangeRate);
			TestBankTransfer.SetLocalSellAmountWithAsserts(50m);
			TestBankTransfer.SetBuyExchangeRateWithAsserts(0.5m);

			AssertEquals(1m, TestBankTransfer.SellExchangeRate);
			AssertEquals(0.5m, TestBankTransfer.BuyExchangeRate);
			AssertEquals(50m, TestBankTransfer.SellAmount);
			AssertEquals(25m, TestBankTransfer.BuyAmount);
			AssertEquals(50m, TestBankTransfer.LocalSellAmount);
			AssertEquals(50m, TestBankTransfer.LocalBuyAmount);

			//Sell = Local Currency, Buy = Local Currency
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.AUDBankAccount2.PK);
			AssertEquals("Buy Exchange rate should be defaulted to 1 as BuyAmount and LocalAmount are of the same currency", 1m, TestBankTransfer.BuyExchangeRate);
			AssertEquals("Sell exchange rate should be defaulted to 1 as SellAmount and LocalAmount are of the same currency", 1m, TestBankTransfer.SellExchangeRate);
			TestBankTransfer.SetLocalSellAmountWithAsserts(50m);

			AssertEquals(1m, TestBankTransfer.SellExchangeRate);
			AssertEquals(1m, TestBankTransfer.BuyExchangeRate);
			AssertEquals(50m, TestBankTransfer.SellAmount);
			AssertEquals(50m, TestBankTransfer.BuyAmount);
			AssertEquals(50m, TestBankTransfer.LocalSellAmount);
			AssertEquals(50m, TestBankTransfer.LocalBuyAmount);
		}

		public void TestSellExchangeRateValue()
		{
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);

			TestBankTransfer.SetSellExchangeRateWithAsserts(1m);
			AssertEquals(false, TestBankTransfer.SellExchangeRateInfo.HasErrors());

			TestBankTransfer.SetSellExchangeRateWithAsserts(-1m);
			AssertEquals(true, TestBankTransfer.SellExchangeRateInfo.HasErrors());
		}

		public void TestBuyExchangeRateValue()
		{
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetSellAmountWithAsserts(100m);

			TestBankTransfer.SetBuyExchangeRateWithAsserts(1m);
			AssertEquals(false, TestBankTransfer.BuyExchangeRateInfo.HasErrors());

			TestBankTransfer.SetBuyExchangeRateWithAsserts(-1m);
			AssertEquals(true, TestBankTransfer.BuyExchangeRateInfo.HasErrors());
		}

		public void TestFinanceChargeExchangeRateValue()
		{
			TestBankTransfer.EnableFinanceCharge = true;
			TestBankTransfer.FinanceChargeBankPK = TestObjectCreator.USDBankAccount.PK;

			TestBankTransfer.FinanceChargeExchangeRate = 1m;
			AssertEquals(false, TestBankTransfer.FinanceChargeExchangeRateInfo.HasErrors());
			TestBankTransfer.FinanceChargeExchangeRate = -1m;
			AssertEquals(true, TestBankTransfer.FinanceChargeExchangeRateInfo.HasErrors());
		}

		public void TestTransferType()
		{
			AssertEquals(TransferType.TransferFrom, TestBankTransfer.TransferRowFrom.TransferType);
			AssertEquals(TransferType.TransferTo, TestBankTransfer.TransferRowTo.TransferType);
		}

		public void TestTransferRows()
		{
			AssertNotNull(TestBankTransfer.TransferRowFrom);
			AssertNotNull(TestBankTransfer.TransferRowTo);
			AssertNotNull(TestBankTransfer.FinanceCharge_ForTestOnly);
		}

		public void TestTransactionCount()
		{
			AssertEquals((byte)1, TestBankTransfer.TransferRowFrom.AH_TransactionCount);
			AssertEquals((byte)2, TestBankTransfer.TransferRowTo.AH_TransactionCount);
		}

		public void TestFinanceChargeDefaultValues()
		{
			AssertEquals(ReceiptTypes.EFT, TestBankTransfer.FinanceCharge_ForTestOnly.AH_ReceiptType);
		}

		public void TestEnableFinanceChargeDefaultValue()
		{
			AssertEquals(false, TestBankTransfer.EnableFinanceCharge);
			AssertEquals(false, TestBankTransfer.FinanceCharge_ForTestOnly.EnableFinanceCharge);
		}

		public void TestReadOnly()
		{
			Factory.Save();
			AssertEquals(true, TestBankTransfer.TransferRowFrom.ReadOnly);
			AssertEquals(true, TestBankTransfer.TransferRowTo.ReadOnly);
			AssertEquals(true, TestBankTransfer.FinanceCharge_ForTestOnly.ReadOnly);
			AssertEquals(true, TestBankTransfer.FinanceCharge_ForTestOnly.Lines[0].ReadOnly);
			AssertEquals(false, TestBankTransfer.BankTransferToPK_ReadOnly);
		}

		public void TestIsPosted()
		{
			TestBankTransfer.Reference = "Test";
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetSellAmountWithAsserts(1m);
			TestBankTransfer.SetBuyExchangeRateWithAsserts(0.7m);
			TestBankTransfer.SetSellAmountWithAsserts(100m);
			TestBankTransfer.EnableFinanceCharge = false;

			AssertEquals(false, TestBankTransfer.IsPosted);

			Factory.Save();
			AssertEquals(true, TestBankTransfer.IsPosted);
		}

		public void TestValidation()
		{
			AssertEquals(typeof(BankTransferValidation), TestBankTransfer.Validation.GetType());
		}

		public void TestInvoiceDate()
		{
			ZDateTime invoiceDate = new ZDateTime(2006, 1, 5);
			TestBankTransfer.TransactionDate = invoiceDate;

			AssertEquals(invoiceDate, TestBankTransfer.TransferRowFrom.AH_InvoiceDate);
			AssertEquals(invoiceDate, TestBankTransfer.TransferRowTo.AH_InvoiceDate);
		}

		public void TestPostDate()
		{
			ZDateTime aH_PostDate = new ZDateTime(2006, 2, 5);
			TestBankTransfer.AH_PostDate = aH_PostDate;

			AssertEquals(aH_PostDate, TestBankTransfer.TransferRowFrom.AH_PostDate);
			AssertEquals(aH_PostDate, TestBankTransfer.TransferRowTo.AH_PostDate);
		}

		public void TestDesc()
		{
			ZString testDesc = "Test Description";
			TestBankTransfer.Description = testDesc;

			AssertEquals(testDesc, TestBankTransfer.TransferRowFrom.AH_Desc);
			AssertEquals(testDesc, TestBankTransfer.TransferRowTo.AH_Desc);
		}

		public void TestReference()
		{
			ZString testDesc = "Test Reference";
			TestBankTransfer.Reference = testDesc;

			AssertEquals(testDesc, TestBankTransfer.TransferRowFrom.AH_ChequeOrReference);
			AssertEquals(testDesc, TestBankTransfer.TransferRowTo.AH_ChequeOrReference);
		}

		public void TestSetTransactionNumber()
		{
			ZString testTransactionNum = "00001000";
			TestBankTransfer.TransactionNumber = testTransactionNum;

			AssertEquals(testTransactionNum, TestBankTransfer.TransferRowFrom.AH_TransactionNum);
			AssertEquals(testTransactionNum, TestBankTransfer.TransferRowTo.AH_TransactionNum);
		}

		public void TestSetFromBankAccount()
		{
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			AssertEquals(TestObjectCreator.AUDBankAccount.PK, TestBankTransfer.TransferRowFrom.AH_AB);
		}

		public void TestSetToBankAccount()
		{
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			AssertEquals(TestObjectCreator.AUDBankAccount.PK, TestBankTransfer.TransferRowTo.AH_AB);
		}

		public void TestSellAmount()
		{
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.GBPBankAccount.PK);
			TestBankTransfer.SetSellAmountWithAsserts(120m);
			AssertEquals(120m, TestBankTransfer.TransferRowFrom.AH_OSExTaxAmount);
		}

		public void TestBuyAmount()
		{
			//Sell = Foregin Currency, Buy = Foregin TransactionCurrency
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.GBPBankAccount.PK);

			TestBankTransfer.SetBuyAmountWithAsserts(120m);
			AssertEquals(120m, TestBankTransfer.TransferRowTo.AH_OSExTaxAmount);

			TestBankTransfer.SetSellAmountWithAsserts(100m);
			TestBankTransfer.SetSellExchangeRateWithAsserts(0.5m);
			TestBankTransfer.SetBuyExchangeRateWithAsserts(0.5m);

			AssertEquals(100m, TestBankTransfer.BuyAmount);
			AssertEquals(200m, TestBankTransfer.LocalBuyAmount);
			AssertEquals(100m, TestBankTransfer.SellAmount);

			TestBankTransfer.SetBuyAmountWithAsserts(50m);
			AssertEquals(0.25m, TestBankTransfer.BuyExchangeRate);

			//Sell = Foreign Currency, Buy = Local TransactionCurrency
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			AssertEquals("BuyExchange rate should be defaulted to 1 as BuyAmount and LocalAmount are of the same currency", 1m, TestBankTransfer.BuyExchangeRate);
			TestBankTransfer.SetSellExchangeRateWithAsserts(0.8333m);

			TestBankTransfer.SetSellAmountWithAsserts(750m);
			AssertEquals(900.04m, TestBankTransfer.BuyAmount);
			AssertEquals(900.04m, TestBankTransfer.LocalBuyAmount);

			TestBankTransfer.SetBuyAmountWithAsserts(900m);
			AssertEquals(900m, TestBankTransfer.BuyAmount);
			AssertEquals(900m, TestBankTransfer.LocalBuyAmount);

			TestBankTransfer.SetBuyAmountWithAsserts(100m);
			AssertEquals(100m, TestBankTransfer.BuyAmount);
			AssertEquals(100m, TestBankTransfer.LocalBuyAmount);
			AssertEquals(7.5m, TestBankTransfer.SellExchangeRate);
		}

		public void TestSellCurrency()
		{
			TestObjectCreator.AUDBankAccount.AB_RX_NKAccountCurrency = TestObjectCreator.USD.RX_Code;
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);

			AssertEquals(TestObjectCreator.USD.RX_Code, TestBankTransfer.SellCurrency);
			AssertEquals(TestObjectCreator.USD.RX_Code, TestBankTransfer.TransferRowFrom.AH_RX_NKTransactionCurrency);
		}

		public void TestBuyCurrency()
		{
			TestObjectCreator.AUDBankAccount.AB_RX_NKAccountCurrency = TestObjectCreator.USD.RX_Code;
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);

			AssertEquals(TestObjectCreator.USD.RX_Code, TestBankTransfer.BuyCurrency);
			AssertEquals(TestObjectCreator.USD.RX_Code, TestBankTransfer.TransferRowTo.AH_RX_NKTransactionCurrency);
		}

		public void TestAH_TransactionCount()
		{
			AssertEquals((ZByte)1, TestBankTransfer.TransferRowFrom.AH_TransactionCount);
			AssertEquals((ZByte)2, TestBankTransfer.TransferRowTo.AH_TransactionCount);
			AssertEquals((ZByte)3, TestBankTransfer.FinanceCharge_ForTestOnly.AH_TransactionCount);
			AssertEquals((ZByte)7, TestBankTransfer.ExchangeDiff.AH_TransactionCount);
		}

		public void TestReversing()
		{
			TestBankTransfer.Reference = "Test";
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetSellAmountWithAsserts(1m);
			TestBankTransfer.SetBuyExchangeRateWithAsserts(0.7m);
			TestBankTransfer.SetSellAmountWithAsserts(100m);
			Factory.Save();

			BankTransfer testLoadedBankTransfer = new BankTransfer(new BusinessObjectFactory(), TestBankTransfer.TransferRowFrom);

			ReversingFactory revFactory = new ReversingFactory();
			ReversingBase reversing = revFactory.NewReversing(testLoadedBankTransfer);
			reversing.Reverse();

			AccTransactionHeader testReversedBankTransferFromRow = ((BankTransfer)((IReversing)testLoadedBankTransfer).ReverseTransaction).TransferRowFrom;
			AccTransactionHeader testReversedBankTransferToRow = ((BankTransfer)((IReversing)testLoadedBankTransfer).ReverseTransaction).TransferRowTo;

			AssertEquals((ZByte)5, testReversedBankTransferFromRow.AH_TransactionCount);
			AssertEquals((ZByte)4, testReversedBankTransferToRow.AH_TransactionCount);

			AssertEquals(TestObjectCreator.USDBankAccount.PK, testReversedBankTransferFromRow.AH_AB);
			AssertEquals(TestObjectCreator.AUDBankAccount.PK, testReversedBankTransferToRow.AH_AB);

			AssertEquals(0.7m, testReversedBankTransferFromRow.AH_ExchangeRate);
			AssertEquals(1m, testReversedBankTransferToRow.AH_ExchangeRate);

			AssertEquals(-100m, testReversedBankTransferFromRow.AH_InvoiceAmount);
			AssertEquals(100m, testReversedBankTransferToRow.AH_InvoiceAmount);

			AssertEquals(0m, testReversedBankTransferFromRow.AH_GSTAmount);
			AssertEquals(0m, testReversedBankTransferToRow.AH_GSTAmount);

			AssertEquals(0m, testReversedBankTransferFromRow.AH_WithholdingTax);
			AssertEquals(0m, testReversedBankTransferToRow.AH_WithholdingTax);

			AssertEquals(-70m, testReversedBankTransferFromRow.AH_OSTotal);
			AssertEquals(100m, testReversedBankTransferToRow.AH_OSTotal);

			var testReversedTransfer = ((BankTransfer)((IReversing)testLoadedBankTransfer).ReverseTransaction);

			AssertEquals("Original transaction number should be set", testLoadedBankTransfer.TransactionNumber, testReversedTransfer.OriginalTransactionNumber);
			AssertEquals("Original transaction type should be set", testLoadedBankTransfer.TransactionType, testReversedTransfer.OriginalTransactionType);
		}

		[TestDate(2006, 11, 15)]
		public void TestValidateAH_PostDate_Reversing()
		{
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ZDateTime originalPostDate = ZDateTime.Today.AddDays(-5);
			AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(Factory);
			AccPeriodManagement accPeriod = periodCalculator.GetPeriodManagementFromDate(originalPostDate);
			if (accPeriod == null)
			{
				accPeriod = Factory.New<AccPeriodManagement>();
				accPeriod.AM_GC_Company = GlbCompany.CurrentCompany.PK;
				accPeriod.AM_Period = periodCalculator.GetPeriodFromDate(originalPostDate);
				accPeriod.AM_Year = (short)(accPeriod.AM_Period / 100);
				accPeriod.AM_StartDate = new ZDateTime(originalPostDate.Year, originalPostDate.Month, 1);
				accPeriod.AM_EndDate = accPeriod.AM_StartDate.AddMonths(1).AddDays(-1);
			}
			Factory.Save();

			TestBankTransfer.AH_PostDate = originalPostDate;

			ReversingFactory reversingFactory = new ReversingFactory();
			ReversingBase reversing = reversingFactory.NewReversing(TestBankTransfer);
			reversing.Reverse();

			BankTransfer reverseBankTransfer = reversing.ReverseTransaction as BankTransfer;
			reverseBankTransfer.Validation.ValidateAH_PostDate();
			AssertEquals("AH_PostDateInfo.HasErrors()", false, reverseBankTransfer.AH_PostDateInfo.HasErrors());

			reverseBankTransfer.AH_PostDate = originalPostDate.AddDays(-1);
			AssertEquals("AH_PostDateInfo.HasErrors()", true, reverseBankTransfer.AH_PostDateInfo.HasErrors());
			string expectedError = "Reversing post date cannot be before the original post date of '" + TestBankTransfer.AH_PostDate.ToShortDateString() + "'.";
			AssertEquals("Contains(ExpectedError)", true, reverseBankTransfer.AH_PostDateInfo.GetErrors().Contains(expectedError));

			reverseBankTransfer.AH_PostDate = originalPostDate;
			AssertEquals("AH_PostDateInfo.HasErrors()", false, reverseBankTransfer.AH_PostDateInfo.HasErrors());

			reverseBankTransfer.AH_PostDate = originalPostDate.AddDays(1);
			AssertEquals("AH_PostDateInfo.HasErrors()", false, reverseBankTransfer.AH_PostDateInfo.HasErrors());

			reverseBankTransfer.TransferRowFrom.ReadOnly = true;
			reverseBankTransfer.TransferRowTo.ReadOnly = true;
			Assert("TransferRowFrom validation should be a TransactionReversalValidation", reverseBankTransfer.TransferRowFrom.Validation is TransactionReversalValidation);
			Assert("TransferRowTo validation should be a TransactionReversalValidation", reverseBankTransfer.TransferRowTo.Validation is TransactionReversalValidation);

			reverseBankTransfer.AH_PostDate = originalPostDate.AddDays(6);
			AssertEquals("AH_PostDateInfo should not have any error because the context is not set", false, reverseBankTransfer.AH_PostDateInfo.HasErrors());
			reverseBankTransfer.AH_PostDate = originalPostDate.AddYears(2);
			AssertEquals("AH_PostDateInfo should not have any error because the context is not set", false, reverseBankTransfer.AH_PostDateInfo.HasErrors());

			Factory.SetContext(BusinessContext.ReverseDateForm);

			expectedError = "The post date cannot be in the future";
			reverseBankTransfer.AH_PostDate = originalPostDate.AddDays(6);
			AssertEquals("We should get an error because the context is set : " + expectedError, true, reverseBankTransfer.AH_PostDateInfo.GetErrors().Contains(expectedError));

			expectedError = "This date does not fall into a valid accounting period’s date range.\r\nPlease go to Manage > General Ledger > Period Management > Set Up Next Accounting Year, to ensure there is an accounting period for the date you wish to post to.";
			reverseBankTransfer.AH_PostDate = originalPostDate.AddYears(2);
			AssertEquals("We should get an error because the context is set : " + expectedError, true, reverseBankTransfer.AH_PostDateInfo.GetErrors().Contains(expectedError));
		}

		public void TestSavedDataCheckAmounts()
		{
			TestBankTransfer.Reference = "Test";
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.GBPBankAccount.PK);
			TestBankTransfer.SetSellAmountWithAsserts(750m);
			TestBankTransfer.SetSellExchangeRateWithAsserts(0.8333m);
			TestBankTransfer.SetBuyExchangeRateWithAsserts(1m);

			AssertEquals(900.04m, TestBankTransfer.LocalBuyAmount);
			Factory.Save();

			BankTransfer testLoadedBankTransfer = new BankTransfer(new BusinessObjectFactory(), TestBankTransfer.TransferRowFrom);
			AccTransactionHeader testReversedBankTransferFromRow = testLoadedBankTransfer.TransferRowFrom;
			AccTransactionHeader testReversedBankTransferToRow = testLoadedBankTransfer.TransferRowTo;

			AssertEquals(-900.04m, testReversedBankTransferFromRow.AH_InvoiceAmount);
			AssertEquals(900.04m, testReversedBankTransferToRow.AH_InvoiceAmount);

			AssertEquals(-750m, testReversedBankTransferFromRow.AH_OSTotal);
			AssertEquals(900.04m, testReversedBankTransferToRow.AH_OSTotal);
		}

		public void TestSavedDataLocalAmountOverwritten()
		{
			TestBankTransfer.Reference = "Test";
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.GBPBankAccount.PK);
			TestBankTransfer.SetSellAmountWithAsserts(750m);
			TestBankTransfer.SetSellExchangeRateWithAsserts(0.8333m);
			TestBankTransfer.SetBuyExchangeRateWithAsserts(1m);

			AssertEquals(900.04m, TestBankTransfer.LocalBuyAmount);
			TestBankTransfer.SetLocalBuyAmountWithAsserts(900m);
			Factory.Save();

			BankTransfer testLoadedBankTransfer = new BankTransfer(new BusinessObjectFactory(), TestBankTransfer.TransferRowFrom);
			AccTransactionHeader testReversedBankTransferFromRow = testLoadedBankTransfer.TransferRowFrom;
			AccTransactionHeader testReversedBankTransferToRow = testLoadedBankTransfer.TransferRowTo;

			AssertEquals(-900.00m, testReversedBankTransferFromRow.AH_InvoiceAmount);
			AssertEquals(900.00m, testReversedBankTransferToRow.AH_InvoiceAmount);

			AssertEquals(-750m, testReversedBankTransferFromRow.AH_OSTotal);
			AssertEquals(900.00m, testReversedBankTransferToRow.AH_OSTotal);
		}

		public void TestSavedDataFinanceCharge()
		{
			TestBankTransfer.Reference = "Test";
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.GBPBankAccount.PK);
			TestBankTransfer.SetSellAmountWithAsserts(750m);
			TestBankTransfer.SetSellExchangeRateWithAsserts(0.8333m);
			TestBankTransfer.SetBuyExchangeRateWithAsserts(1m);

			TestBankTransfer.EnableFinanceCharge = true;
			TestBankTransfer.FinanceChargeOSAmount = 10m;
			TestBankTransfer.FinanceChargeExchangeRate = 1.46m;
			TestBankTransfer.FinanceChargeTaxID = TestObjectCreator.GST1.PK;
			TestBankTransfer.FinanceChargeOSTaxAmount = 1m;
			Factory.Save();

			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, TestBankTransfer.TransactionBelongsToGroup);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCount, (byte)3);
			AccTransactionHeader testFinanceCharge = Factory.LoadTop1<AccTransactionHeader>(filter);

			AssertEquals(LedgerTypes.CashBook, testFinanceCharge.AH_Ledger);
			AssertEquals(TransactionTypes.DirectPayment, testFinanceCharge.AH_TransactionType);
			AssertEquals((ZByte)3, testFinanceCharge.AH_TransactionCount);
			AssertEquals(ZString.Empty, testFinanceCharge.AH_TransactionReference);
			AssertEquals(ZDateTime.Today.ToShortDateString(), testFinanceCharge.AH_InvoiceDate.ToShortDateString());
			AssertEquals(ZString.Empty, testFinanceCharge.AH_TransactionCategory);
			AssertEquals(testFinanceCharge.AH_InvoiceDate, testFinanceCharge.AH_DueDate);
			AssertEquals(-6.85m, testFinanceCharge.AH_InvoiceAmount);
			AssertEquals(-0.68m, testFinanceCharge.AH_GSTAmount);
			AssertEquals(0m, testFinanceCharge.AH_WithholdingTax);
			AssertEquals(-11m, testFinanceCharge.AH_OSTotal);
			AssertEquals(TestObjectCreator.USDBankAccount.AB_RX_NKAccountCurrency, testFinanceCharge.AH_RX_NKTransactionCurrency);
			AssertEquals(1.46m, testFinanceCharge.AH_ExchangeRate);
			AssertEquals(0, testFinanceCharge.AH_AgePeriod);
			AssertEquals(0, testFinanceCharge.AH_PostPeriod);
			AssertEquals(ZDateTime.Today.ToShortDateString(), testFinanceCharge.AH_PostDate.ToShortDateString());
			AssertEquals(false, testFinanceCharge.AH_IsDisbursementCalc);
			AssertEquals("Test", testFinanceCharge.AH_ChequeOrReference);
			AssertEquals(ReceiptTypes.EFT, testFinanceCharge.AH_ReceiptType);
			AssertEquals(false, testFinanceCharge.AH_CashBasisGSTIndicator);
			AssertEquals(false, testFinanceCharge.AH_CashBasisGSTRealisedToGL);
			AssertEquals(string.Empty, testFinanceCharge.AH_ChequeDrawer);
			AssertEquals(ZString.Empty, testFinanceCharge.AH_DrawerBranch);
			AssertEquals(false, testFinanceCharge.AH_InvoiceApproved);
			AssertEquals(ZString.Empty, testFinanceCharge.AH_ConsolidatedInvoiceRef);
			AssertEquals(true, testFinanceCharge.AH_FullyPaidDate.IsEmpty);
			AssertEquals(false, testFinanceCharge.AH_InvoicePrinted);
			AssertEquals(false, testFinanceCharge.AH_IsCancelled);
			AssertEquals(true, testFinanceCharge.AH_DateClearedInCashbook.IsEmpty);
			AssertEquals(false, testFinanceCharge.AH_NotAllocated);
			AssertEquals(0m, testFinanceCharge.AH_OutstandingAmount);
			AssertEquals(false, testFinanceCharge.AH_PostedToEFT);
			AssertEquals("N", testFinanceCharge.AH_PostToGL);
			AssertEquals(string.Empty, testFinanceCharge.AH_ReceiptBatchNo);
			AssertEquals(true, testFinanceCharge.AH_OH.IsEmpty);
			AssertEquals(true, testFinanceCharge.AH_JH.IsEmpty);
			AssertEquals(TestObjectCreator.USDBankAccount.PK, testFinanceCharge.AH_AB);
			AssertEquals(GlbBranch.CurrentBranch.PK, testFinanceCharge.AH_GB);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, testFinanceCharge.AH_GE);
			AssertEquals(ZGuid.Empty, testFinanceCharge.AH_AG);
			AssertEquals(TestBankTransfer.TransactionBelongsToGroup, testFinanceCharge.AH_TransactionBelongsToGroup);
			AssertEquals(ZGuid.Empty, testFinanceCharge.AH_AH_InvoiceStatement);
		}

		public void TestSavedDataFinanceChargePostDateOnBackPosting()
		{
			TestBankTransfer.Reference = "Test";
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.GBPBankAccount.PK);
			TestBankTransfer.SetSellAmountWithAsserts(750m);
			TestBankTransfer.SetSellExchangeRateWithAsserts(0.8333m);
			TestBankTransfer.SetBuyExchangeRateWithAsserts(1m);
			TestBankTransfer.AH_PostDate = ZDateTime.Today.AddMonths(-1);

			TestBankTransfer.EnableFinanceCharge = true;
			TestBankTransfer.FinanceChargeOSAmount = 10m;
			TestBankTransfer.FinanceChargeExchangeRate = 1.46m;
			TestBankTransfer.FinanceChargeTaxID = TestObjectCreator.GST1.PK;
			TestBankTransfer.FinanceChargeOSTaxAmount = 1m;
			Factory.Save();

			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, TestBankTransfer.TransactionBelongsToGroup);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCount, (byte)3);
			AccTransactionHeader testFinanceCharge = Factory.LoadTop1<AccTransactionHeader>(filter);

			AssertEquals(TestBankTransfer.AH_PostDate, testFinanceCharge.AH_PostDate);
		}

		public void TestFinanceChargeOSTaxAmount_ReadOnly()
		{
			TestBankTransfer.EnableFinanceCharge = true;
			Assert(TestBankTransfer.FinanceChargeOSTaxAmountInfo.ReadOnly);

			TestBankTransfer.FinanceChargeTaxID = TestObjectCreator.GST1.PK;
			Assert(!TestBankTransfer.FinanceChargeOSTaxAmountInfo.ReadOnly);

			TestBankTransfer.FinanceChargeTaxID = TestObjectCreator.GSTFREE1.PK;
			Assert(TestBankTransfer.FinanceChargeOSTaxAmountInfo.ReadOnly);
		}

		public void TestSavedData()
		{
			TestBankTransfer.Reference = "Test";
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetSellAmountWithAsserts(1m);
			TestBankTransfer.SetBuyExchangeRateWithAsserts(0.7m);
			TestBankTransfer.SetSellAmountWithAsserts(100m);
			Factory.Save();

			ZQuery filter = new ZQuery();
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, TestBankTransfer.TransactionBelongsToGroup);
			TransactionHeaderCollection testCol = new TransactionHeaderCollection(Factory, filter);
			testCol.Load();
			AssertEquals(3, testCol.Count);

			filter.Clear();
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, TestBankTransfer.TransactionBelongsToGroup);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCount, (byte)1);
			BankTransferFromRow testBankTransferFromRow = Factory.LoadTop1<BankTransferFromRow>(filter);

			filter.Clear();
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, TestBankTransfer.TransactionBelongsToGroup);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCount, (byte)2);
			BankTransferToRow testBankTransferToRow = Factory.LoadTop1<BankTransferToRow>(filter);

			filter.Clear();
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, TestBankTransfer.TransactionBelongsToGroup);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCount, (byte)7);
			var exchangeDiff = Factory.LoadTop1<CashbookExchangeDiff>(filter);
			AssertEquals(false, exchangeDiff.IsSavedByFactory);

			AssertEquals(ZArchitecture.Core.LedgerTypes.CashBook, testBankTransferFromRow.AH_Ledger);
			AssertEquals(ZArchitecture.Core.TransactionTypes.Transfer, testBankTransferFromRow.AH_TransactionType);
			AssertEquals((ZByte)1, testBankTransferFromRow.AH_TransactionCount);
			AssertEquals(ZString.Empty, testBankTransferFromRow.AH_TransactionReference);
			AssertEquals("CASH BOOK TRANSFER", testBankTransferFromRow.AH_Desc);
			AssertEquals(ZDateTime.Today.ToShortDateString(), testBankTransferFromRow.AH_InvoiceDate.ToShortDateString());
			AssertEquals(ZString.Empty, testBankTransferFromRow.AH_TransactionCategory);
			AssertEquals(true, testBankTransferFromRow.AH_DueDate.IsEmpty);
			AssertEquals(-100m, testBankTransferFromRow.AH_InvoiceAmount);
			AssertEquals(0m, testBankTransferFromRow.AH_GSTAmount);
			AssertEquals(0m, testBankTransferFromRow.AH_WithholdingTax);
			AssertEquals(-100m, testBankTransferFromRow.AH_OSTotal);
			AssertEquals(TestObjectCreator.AUDBankAccount.AB_RX_NKAccountCurrency, testBankTransferFromRow.AH_RX_NKTransactionCurrency);
			AssertEquals(1m, testBankTransferFromRow.AH_ExchangeRate);
			AssertEquals(0, testBankTransferFromRow.AH_AgePeriod);
			AssertEquals(0, testBankTransferFromRow.AH_PostPeriod);
			AssertEquals(ZDateTime.Today.ToShortDateString(), testBankTransferFromRow.AH_PostDate.ToShortDateString());
			AssertEquals(false, testBankTransferFromRow.AH_IsDisbursementCalc);
			AssertEquals("Test", testBankTransferFromRow.AH_ChequeOrReference);
			AssertEquals(ReceiptTypes.EFT, testBankTransferFromRow.AH_ReceiptType);
			AssertEquals(false, testBankTransferFromRow.AH_CashBasisGSTIndicator);
			AssertEquals(false, testBankTransferFromRow.AH_CashBasisGSTRealisedToGL);
			AssertEquals(string.Empty, testBankTransferFromRow.AH_ChequeDrawer);
			AssertEquals(ZString.Empty, testBankTransferFromRow.AH_DrawerBranch);
			AssertEquals(false, testBankTransferFromRow.AH_InvoiceApproved);
			AssertEquals(ZString.Empty, testBankTransferFromRow.AH_ConsolidatedInvoiceRef);
			AssertEquals(ZDateTime.Today.ToShortDateString(), testBankTransferFromRow.AH_FullyPaidDate.ToShortDateString());
			AssertEquals(false, testBankTransferFromRow.AH_InvoicePrinted);
			AssertEquals(false, testBankTransferFromRow.AH_IsCancelled);
			AssertEquals(true, testBankTransferFromRow.AH_DateClearedInCashbook.IsEmpty);
			AssertEquals(false, testBankTransferFromRow.AH_NotAllocated);
			AssertEquals(0m, testBankTransferFromRow.AH_OutstandingAmount);
			AssertEquals(false, testBankTransferFromRow.AH_PostedToEFT);
			AssertEquals("N", testBankTransferFromRow.AH_PostToGL);
			AssertEquals(string.Empty, testBankTransferFromRow.AH_ReceiptBatchNo);
			AssertEquals(true, testBankTransferFromRow.AH_OH.IsEmpty);
			AssertEquals(true, testBankTransferFromRow.AH_JH.IsEmpty);
			AssertEquals(TestObjectCreator.AUDBankAccount.PK, testBankTransferFromRow.AH_AB);
			AssertEquals(GlbBranch.CurrentBranch.PK, testBankTransferFromRow.AH_GB);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, testBankTransferFromRow.AH_GE);
			AssertEquals(ZGuid.Empty, testBankTransferFromRow.AH_AG);
			AssertEquals(false, testBankTransferFromRow.AH_TransactionBelongsToGroup.IsEmpty);
			AssertEquals(ZGuid.Empty, testBankTransferFromRow.AH_AH_InvoiceStatement);

			//TestBankTransferToRow
			AssertEquals(testBankTransferToRow.AH_TransactionNum, testBankTransferFromRow.AH_TransactionNum);
			AssertEquals(100m, testBankTransferToRow.AH_InvoiceAmount);
			AssertEquals(70m, testBankTransferToRow.AH_OSTotal);
			AssertEquals(TestObjectCreator.USDBankAccount.AB_RX_NKAccountCurrency, testBankTransferToRow.AH_RX_NKTransactionCurrency);
			AssertEquals(0.7m, testBankTransferToRow.AH_ExchangeRate);
			AssertEquals((ZByte)2, testBankTransferToRow.AH_TransactionCount);
			AssertEquals(testBankTransferToRow.AH_TransactionBelongsToGroup, testBankTransferFromRow.AH_TransactionBelongsToGroup);
			AssertEquals(TestObjectCreator.USDBankAccount.PK, testBankTransferToRow.AH_AB);
		}

		public void TestBuyAmountInfoIsNotReadOnly()
		{
			TestObjectCreator.AUDBankAccount.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			Assert("BuyAmountInfo should not be read only", !TestBankTransfer.BuyAmountInfo.ReadOnly);

			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			Assert("BuyAmountInfo should not be read only", !TestBankTransfer.BuyAmountInfo.ReadOnly);

			TestObjectCreator.AUDBankAccount.AB_RX_NKAccountCurrency = TestObjectCreator.GBP.RX_Code;
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			Assert("BuyAmountInfo should not be read only", !TestBankTransfer.BuyAmountInfo.ReadOnly);
		}

		public void TestLocalAmountInfoIsNotReadOnly()
		{
			TestObjectCreator.AUDBankAccount.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			Assert("LocalBuyAmountInfo should be read only", TestBankTransfer.LocalBuyAmountInfo.ReadOnly);
			Assert("LocalSellAmountInfo should not be read only", !TestBankTransfer.LocalSellAmountInfo.ReadOnly);

			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			Assert("LocalBuyAmountInfo should not be read only", !TestBankTransfer.LocalBuyAmountInfo.ReadOnly);
			Assert("LocalSellAmountInfo should be read only because of local currency account", TestBankTransfer.LocalSellAmountInfo.ReadOnly);

			TestObjectCreator.AUDBankAccount.AB_RX_NKAccountCurrency = TestObjectCreator.GBP.RX_Code;
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			Assert("LocalBuyAmountInfo should not be read only", !TestBankTransfer.LocalBuyAmountInfo.ReadOnly);
			Assert("LocalSellAmountInfo should not be read only", !TestBankTransfer.LocalSellAmountInfo.ReadOnly);
		}

		public void TestAH_InvoiceAmountOnBothTransactionsTheSameOnBuyAmountChanged_ForeignToLocal()
		{
			TestObjectCreator.AUDBankAccount.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			TestBankTransfer.SetSellExchangeRateWithAsserts(1.5777M);
			TestBankTransfer.SetSellAmountWithAsserts(50000M);
			TestBankTransfer.SetBuyAmountWithAsserts(TestBankTransfer.BuyAmount - 0.01M); //slightly adjust the amount
			ZDecimal correctInvoiceAmount = TestBankTransfer.BuyAmount;
			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			BankTransferFromRow loadedFromRow = anotherFactory.Load<BankTransferFromRow>(TestBankTransfer.TransferRowFrom.PK);
			BankTransferToRow loadedToRow = anotherFactory.Load<BankTransferToRow>(TestBankTransfer.TransferRowTo.PK);
			AssertNotNull("Bank transfer should have been saved", loadedFromRow);
			AssertNotNull("Bank transfer should have been saved", loadedToRow);
			AssertNotEquals(loadedFromRow.PK, loadedToRow.PK);
			AssertEquals("From Row should have correct invoice amount", -correctInvoiceAmount, loadedFromRow.AH_InvoiceAmount);
			AssertEquals("Sell amount should be saved correctly", 50000M, loadedFromRow.AH_OSExTaxAmount);
			AssertEquals("To Row should have correct invoice amount", correctInvoiceAmount, loadedToRow.AH_InvoiceAmount);
			AssertEquals("Buy amount should be saved correctly", correctInvoiceAmount, loadedToRow.AH_OSExTaxAmount);
		}

		public void TestAH_InvoiceAmountOnBothTransactionsTheSameOnBuyAmountChanged_LocalToLocal()
		{
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.AUDBankAccount2.PK);
			TestBankTransfer.SetSellAmountWithAsserts(50000M);
			TestBankTransfer.SetBuyAmountWithAsserts(TestBankTransfer.BuyAmount - 0.01M); //slightly adjust the amount
			ZDecimal correctInvoiceAmount = TestBankTransfer.LocalBuyAmount;
			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			BankTransferFromRow loadedFromRow = anotherFactory.Load<BankTransferFromRow>(TestBankTransfer.TransferRowFrom.PK);
			BankTransferToRow loadedToRow = anotherFactory.Load<BankTransferToRow>(TestBankTransfer.TransferRowTo.PK);
			AssertNotNull("Bank transfer should have been saved", loadedFromRow);
			AssertNotNull("Bank transfer should have been saved", loadedToRow);
			AssertNotEquals(loadedFromRow.PK, loadedToRow.PK);
			AssertEquals("From Row should have correct invoice amount", -correctInvoiceAmount, loadedFromRow.AH_InvoiceAmount);
			AssertEquals("Sell amount should be saved correctly", 49999.99M, loadedFromRow.AH_OSExTaxAmount);
			AssertEquals("To Row should have correct invoice amount", correctInvoiceAmount, loadedToRow.AH_InvoiceAmount);
			AssertEquals("Buy amount should be saved correctly", TestBankTransfer.BuyAmount, loadedToRow.AH_OSExTaxAmount);
		}

		public void TestAH_InvoiceAmountOnBothTransactionsTheSameOnBuyAmountChanged_ForeignToForeign()
		{
			RefCurrency foreignCurrency = Factory.NewWithValidTestData<RefCurrency>();
			TestObjectCreator.AUDBankAccount.AB_RX_NKAccountCurrency = foreignCurrency.RX_Code;

			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			TestBankTransfer.SetSellExchangeRateWithAsserts(1.5777M);
			TestBankTransfer.SetBuyExchangeRateWithAsserts(2.2358M);
			TestBankTransfer.SetSellAmountWithAsserts(50000M);
			TestBankTransfer.SetBuyAmountWithAsserts(TestBankTransfer.BuyAmount - 0.01M); //slightly adjust the amount
			ZDecimal correctInvoiceAmount = TestBankTransfer.LocalBuyAmount;
			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			BankTransferFromRow loadedFromRow = anotherFactory.Load<BankTransferFromRow>(TestBankTransfer.TransferRowFrom.PK);
			BankTransferToRow loadedToRow = anotherFactory.Load<BankTransferToRow>(TestBankTransfer.TransferRowTo.PK);
			AssertNotNull("Bank transfer should have been saved", loadedFromRow);
			AssertNotNull("Bank transfer should have been saved", loadedToRow);
			AssertNotEquals(loadedFromRow.PK, loadedToRow.PK);
			AssertEquals("From Row should have correct invoice amount", -correctInvoiceAmount, loadedFromRow.AH_InvoiceAmount);
			AssertEquals("Sell amount should be saved correctly", 50000M, loadedFromRow.AH_OSExTaxAmount);
			AssertEquals("To Row should have correct invoice amount", correctInvoiceAmount, loadedToRow.AH_InvoiceAmount);
			AssertEquals("Buy amount should be saved correctly", TestBankTransfer.BuyAmount, loadedToRow.AH_OSExTaxAmount);
		}

		public void TestAH_InvoiceAmountOnBothTransactionsTheSameOnBuyAmountChanged_LocalToForeign()
		{
			RefCurrency foreignCurrency = Factory.NewWithValidTestData<RefCurrency>();
			TestObjectCreator.AUDBankAccount.AB_RX_NKAccountCurrency = foreignCurrency.RX_Code;

			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			TestBankTransfer.SetBuyExchangeRateWithAsserts(2.2358M);
			TestBankTransfer.SetSellExchangeRateWithAsserts(1m);
			TestBankTransfer.SetSellAmountWithAsserts(50000M);
			TestBankTransfer.SetBuyAmountWithAsserts(TestBankTransfer.BuyAmount - 0.01M); //slightly adjust the amount
			ZDecimal correctInvoiceAmount = TestBankTransfer.LocalBuyAmount;
			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			BankTransferFromRow loadedFromRow = anotherFactory.Load<BankTransferFromRow>(TestBankTransfer.TransferRowFrom.PK);
			BankTransferToRow loadedToRow = anotherFactory.Load<BankTransferToRow>(TestBankTransfer.TransferRowTo.PK);
			AssertNotNull("Bank transfer should have been saved", loadedFromRow);
			AssertNotNull("Bank transfer should have been saved", loadedToRow);
			AssertNotEquals(loadedFromRow.PK, loadedToRow.PK);
			AssertEquals("From Row should have correct invoice amount", -correctInvoiceAmount, loadedFromRow.AH_InvoiceAmount);
			AssertEquals("Sell amount should be saved correctly", 50000M, loadedFromRow.AH_OSExTaxAmount);
			AssertEquals("To Row should have correct invoice amount", correctInvoiceAmount, loadedToRow.AH_InvoiceAmount);
			AssertEquals("Buy amount should be saved correctly", TestBankTransfer.BuyAmount, loadedToRow.AH_OSExTaxAmount);
		}

		public void TestDefaultAH_PostDateReadOnly()
		{
			bool payablesAllowed = Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed;

			try
			{
				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				TestBankTransfer = new BankTransfer(Factory, null);
				Assert("AH_PostDateInfo should not be readonly", !TestBankTransfer.AH_PostDateInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				TestBankTransfer = new BankTransfer(Factory, null);
				Assert("AH_PostDateInfo should be readonly", TestBankTransfer.AH_PostDateInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				TestBankTransfer = new BankTransfer(Factory, null);
				Assert("AH_PostDateInfo should be readonly", TestBankTransfer.AH_PostDateInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				TestBankTransfer = new BankTransfer(Factory, null);
				Assert("AH_PostDateInfo should be readonly", TestBankTransfer.AH_PostDateInfo.ReadOnly);
			}
			finally
			{
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = payablesAllowed;
			}
		}

		public void TestTransferRowTo_Initialization()
		{
			AssertEquals(TestBankTransfer, TestBankTransfer.TransferRowTo.BankTransferParent);
			AssertEquals(false, TestBankTransfer.TransferRowTo.IsValidationSuspended);
			AssertEquals(0m, TestBankTransfer.TransferRowTo.AH_OSExTaxAmount);
			AssertEquals(0m, TestBankTransfer.TransferRowTo.AH_LocalExTaxAmount);
			AssertEquals(1m, TestBankTransfer.TransferRowTo.AH_ExchangeRate);
			AssertEquals((ZByte)2, TestBankTransfer.TransferRowTo.AH_TransactionCount);
			AssertEquals(TestBankTransfer.TransactionBelongsToGroup, TestBankTransfer.TransferRowTo.AH_TransactionBelongsToGroup);
		}

		public void TestExchangeDiff_Initialization()
		{
			AssertEquals(TestBankTransfer, TestBankTransfer.ExchangeDiff.BankTransferParent);
			AssertEquals(true, TestBankTransfer.ExchangeDiff.IsValidationSuspended);
			AssertEquals(0m, TestBankTransfer.ExchangeDiff.AH_OSExTaxAmount);
			AssertEquals(0m, TestBankTransfer.ExchangeDiff.AH_LocalExTaxAmount);
			AssertEquals(1m, TestBankTransfer.ExchangeDiff.AH_ExchangeRate);
			AssertEquals((ZByte)7, TestBankTransfer.ExchangeDiff.AH_TransactionCount);
			AssertEquals(TestBankTransfer.TransactionBelongsToGroup, TestBankTransfer.ExchangeDiff.AH_TransactionBelongsToGroup);
		}

		[TestDate(2022, 3, 20)]
		public void TestSaveNewBankTransfer()
		{
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.GBPBankAccount.PK);
			TestBankTransfer.SetSellAmountWithAsserts(100m);
			TestBankTransfer.SetSellExchangeRateWithAsserts(0.5m);
			TestBankTransfer.SetBuyExchangeRateWithAsserts(0.3m);

			TestBankTransfer.AssertSellProperties(100m, 0.5m, 200m);
			TestBankTransfer.AssertBuyProperties(60m, 0.3m, 200m);
			AssertSubTransaction("Pre-condition of TransferRowFrom", TestBankTransfer.TransferRowFrom, 100m, 200m, 0.5m, "USD");
			AssertSubTransaction("Pre-condition of TransferRowTo", TestBankTransfer.TransferRowTo, 60m, 200m, 0.3m, "GBP");
			AssertSubTransaction("Pre-condition of ExchangeDiff", TestBankTransfer.ExchangeDiff, 0m, 0m, 1m, "AUD");

			Factory.Save();

			TestBankTransfer.AssertSellProperties(100m, 0.5m, 200m);
			TestBankTransfer.AssertBuyProperties(60m, 0.3m, 200m);
			AssertSubTransaction("Verify TransferRowFrom", TestBankTransfer.TransferRowFrom, 100m, 200m, 0.5m, "USD");
			AssertSubTransaction("Verify TransferRowTo", TestBankTransfer.TransferRowTo, 60m, 200m, 0.3m, "GBP");
			AssertSubTransaction("Verify ExchangeDiff", TestBankTransfer.ExchangeDiff, 0m, 0m, 1m, "AUD");
			AssertEquals(true, TestBankTransfer.TransferRowFrom.IsInDatabase);
			AssertEquals(true, TestBankTransfer.TransferRowTo.IsInDatabase);
			AssertEquals(false, TestBankTransfer.ExchangeDiff.IsInDatabase);
		}

		public void TestSaveNewBankTransferWithExchangeRateVariance_WithForeignToCurrency()
		{
			ConfigureTestBankTransferWithExchangeRateVariance();

			Factory.Save();

			TestBankTransfer.AssertSellProperties(100m, 0.5m, 200m);
			TestBankTransfer.AssertBuyProperties(500m, 0.2m, 2500m);
			AssertSubTransaction("Verify TransferRowFrom", TestBankTransfer.TransferRowFrom, 100m, 200m, 0.5m, "USD");
			AssertSubTransaction("Verify TransferRowTo", TestBankTransfer.TransferRowTo, 500m, 200m, 2.5m, "GBP");
			AssertSubTransaction("Verify ExchangeDiff", TestBankTransfer.ExchangeDiff, 0m, 2300m, 1m, "AUD");
			AssertEquals(true, TestBankTransfer.TransferRowFrom.IsInDatabase);
			AssertEquals(true, TestBankTransfer.TransferRowTo.IsInDatabase);
			AssertEquals(true, TestBankTransfer.ExchangeDiff.IsInDatabase);
		}

		public void TestSaveNewBankTransferWithExchangeRateVariance_WithLocalToCurrency()
		{
			var year = ZDateTime.Now.Year;
			var testObjectCreatorInNewFactory = new TestObjectCreator(Factory.CreateNewFactory());
			testObjectCreatorInNewFactory.CreateTestPeriodsForEntireYear(year);
			testObjectCreatorInNewFactory.CreateTestPeriodsForEntireYear(year + 1);

			TestBankTransfer.BankTransferFromPK = TestObjectCreator.USDBankAccount.PK;
			TestBankTransfer.BankTransferToPK = TestObjectCreator.AUDBankAccount.PK;
			TestBankTransfer.ShouldCalculateExchangeVariance = true;
			TestBankTransfer.SellAmount = 100m;
			TestBankTransfer.SellExchangeRate = 0.5m;
			TestBankTransfer.BuyAmount = 500m;

			TestBankTransfer.AssertSellProperties(100m, 0.5m, 200m);
			TestBankTransfer.AssertBuyProperties(500m, 1m, 500m);
			AssertSubTransaction("Pre-condition of TransferRowFrom", TestBankTransfer.TransferRowFrom, 100m, 200m, 0.5m, "USD");
			AssertSubTransaction("Pre-condition of TransferRowTo", TestBankTransfer.TransferRowTo, 200m, 200m, 1m, "AUD");
			AssertSubTransaction("Pre-condition of ExchangeDiff", TestBankTransfer.ExchangeDiff, 0m, 0m, 1m, "AUD");

			Factory.Save();

			TestBankTransfer.AssertSellProperties(100m, 0.5m, 200m);
			TestBankTransfer.AssertBuyProperties(500m, 1m, 500m);
			AssertSubTransaction("Verify TransferRowFrom", TestBankTransfer.TransferRowFrom, 100m, 200m, 0.5m, "USD");
			AssertSubTransaction("Verify TransferRowTo", TestBankTransfer.TransferRowTo, 200m, 200m, 1m, "AUD");
			AssertSubTransaction("Verify ExchangeDiff", TestBankTransfer.ExchangeDiff, 300m, 300m, 1m, "AUD");
			AssertEquals(true, TestBankTransfer.TransferRowFrom.IsInDatabase);
			AssertEquals(true, TestBankTransfer.TransferRowTo.IsInDatabase);
			AssertEquals(true, TestBankTransfer.ExchangeDiff.IsInDatabase);
		}

		public void TestLoadExistedBankTransferWithExchangeRateVariance()
		{
			ConfigureTestBankTransferWithExchangeRateVariance();

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var reloadedTransferRowFrom = newFactory.Load<BankTransferFromRow>(TestBankTransfer.TransferRowFrom.PK);
			var reloadedBankTransfer = new BankTransfer(Factory, reloadedTransferRowFrom);

			TestBankTransfer.AssertSellProperties(100m, 0.5m, 200m);
			TestBankTransfer.AssertBuyProperties(500m, 0.2m, 2500m);
			AssertSubTransaction("Verify TransferRowFrom", reloadedBankTransfer.TransferRowFrom, 100m, 200m, 0.5m, "USD");
			AssertSubTransaction("Verify TransferRowTo", reloadedBankTransfer.TransferRowTo, 500m, 200m, 2.5m, "GBP");
			AssertSubTransaction("Verify ExchangeDiff", reloadedBankTransfer.ExchangeDiff, 0m, 2300m, 1m, "AUD");
		}

		public void TestLoadExistedBankTransferWithExchangeRateVariance_WithLocalToCurrency()
		{
			ConfigureTestBankTransferWithExchangeRateVariance();

			TestBankTransfer.BankTransferFromPK = TestObjectCreator.USDBankAccount.PK;
			TestBankTransfer.BankTransferToPK = TestObjectCreator.AUDBankAccount.PK;
			TestBankTransfer.ShouldCalculateExchangeVariance = true;
			TestBankTransfer.SellAmount = 100m;
			TestBankTransfer.SellExchangeRate = 0.5m;
			TestBankTransfer.BuyAmount = 250m;

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var reloadedTransferRowFrom = newFactory.Load<BankTransferFromRow>(TestBankTransfer.TransferRowFrom.PK);
			var reloadedBankTransfer = new BankTransfer(Factory, reloadedTransferRowFrom);

			TestBankTransfer.AssertSellProperties(100m, 0.5m, 200m);
			TestBankTransfer.AssertBuyProperties(250m, 1m, 250m);
			AssertSubTransaction("Verify TransferRowFrom", reloadedBankTransfer.TransferRowFrom, 100m, 200m, 0.5m, "USD");
			AssertSubTransaction("Verify TransferRowTo", reloadedBankTransfer.TransferRowTo, 200m, 200m, 1m, "AUD");
			AssertSubTransaction("Verify ExchangeDiff", reloadedBankTransfer.ExchangeDiff, 50m, 50m, 1m, "AUD");
		}

		public void TestShouldPostExchangeDiff()
		{
			AssertEquals(false, TestBankTransfer.ShouldPostExchangeDiff);

			TestBankTransfer.ShouldCalculateExchangeVariance = true;

			AssertEquals(false, TestBankTransfer.ShouldPostExchangeDiff);

			TestBankTransfer.LocalBuyAmount = 100m;
			TestBankTransfer.LocalSellAmount = 50m;

			AssertEquals(true, TestBankTransfer.ShouldPostExchangeDiff);
		}

		void ConfigureTestBankTransferWithExchangeRateVariance()
		{
			var year = ZDateTime.Now.Year;
			var testObjectCreatorInNewFactory = new TestObjectCreator(Factory.CreateNewFactory());
			testObjectCreatorInNewFactory.CreateTestPeriodsForEntireYear(year);
			testObjectCreatorInNewFactory.CreateTestPeriodsForEntireYear(year + 1);

			TestBankTransfer.BankTransferFromPK = TestObjectCreator.USDBankAccount.PK;
			TestBankTransfer.BankTransferToPK = TestObjectCreator.GBPBankAccount.PK;
			TestBankTransfer.ShouldCalculateExchangeVariance = true;
			TestBankTransfer.SellAmount = 100m;
			TestBankTransfer.SellExchangeRate = 0.5m;
			TestBankTransfer.BuyAmount = 500m;
			TestBankTransfer.BuyExchangeRate = 0.2m;

			TestBankTransfer.AssertSellProperties(100m, 0.5m, 200m);
			TestBankTransfer.AssertBuyProperties(500m, 0.2m, 2500m);
			AssertSubTransaction("Pre-condition of TransferRowFrom", TestBankTransfer.TransferRowFrom, 100m, 200m, 0.5m, "USD");
			AssertSubTransaction("Pre-condition of TransferRowTo", TestBankTransfer.TransferRowTo, 500m, 200, 2.5m, "GBP");
			AssertSubTransaction("Pre-condition of ExchangeDiff", TestBankTransfer.ExchangeDiff, 0m, 0m, 1m, "AUD");
		}

		void AssertSubTransaction(string assertMsg, AccTransactionHeader subTransaction, ZDecimal osAmt, ZDecimal localAmt, ZDecimal exRate, ZString currency)
		{
			var multipler = subTransaction.AH_TransactionCount == AccTransactionHeader.TransactionCountConstants.BankTransferFromRow ? -1m : 1m;
			AssertEquals(assertMsg, osAmt, subTransaction.AH_OSTotal * multipler);
			AssertEquals(assertMsg, localAmt, subTransaction.AH_InvoiceAmount * multipler);
			AssertEquals(assertMsg, exRate, subTransaction.AH_ExchangeRate);
			AssertEquals(assertMsg, currency, subTransaction.AH_RX_NKTransactionCurrency);
		}

		public void TestSetCancellationFlag()
		{
			AssertEquals("Pre-condition", false, TestBankTransfer.TransferRowFrom.AH_IsCancelled);
			AssertEquals(false, TestBankTransfer.TransferRowTo.AH_IsCancelled);
			AssertEquals(false, TestBankTransfer.FinanceCharge.AH_IsCancelled);
			AssertEquals(false, TestBankTransfer.ExchangeDiff.AH_IsCancelled);

			TestBankTransfer.SetCancellationFlag(true);

			AssertEquals(true, TestBankTransfer.TransferRowFrom.AH_IsCancelled);
			AssertEquals(true, TestBankTransfer.TransferRowTo.AH_IsCancelled);
			AssertEquals(true, TestBankTransfer.FinanceCharge.AH_IsCancelled);
			AssertEquals(true, TestBankTransfer.ExchangeDiff.AH_IsCancelled);

			TestBankTransfer.SetCancellationFlag(false);

			AssertEquals(false, TestBankTransfer.TransferRowFrom.AH_IsCancelled);
			AssertEquals(false, TestBankTransfer.TransferRowTo.AH_IsCancelled);
			AssertEquals(false, TestBankTransfer.FinanceCharge.AH_IsCancelled);
			AssertEquals(false, TestBankTransfer.ExchangeDiff.AH_IsCancelled);
		}

		public void TestIsReverseTransaction()
		{
			AssertEquals("Pre-condition", false, TestBankTransfer.TransferRowFrom.IsReverseTransaction);
			AssertEquals(false, TestBankTransfer.TransferRowTo.IsReverseTransaction);
			AssertEquals(false, TestBankTransfer.FinanceCharge.IsReverseTransaction);
			AssertEquals(false, TestBankTransfer.ExchangeDiff.IsReverseTransaction);

			TestBankTransfer.IsReverseTransaction = true;

			AssertEquals(true, TestBankTransfer.TransferRowFrom.IsReverseTransaction);
			AssertEquals(true, TestBankTransfer.TransferRowTo.IsReverseTransaction);
			AssertEquals(true, TestBankTransfer.FinanceCharge.IsReverseTransaction);
			AssertEquals(true, TestBankTransfer.ExchangeDiff.IsReverseTransaction);

			TestBankTransfer.IsReverseTransaction = false;

			AssertEquals(false, TestBankTransfer.TransferRowFrom.IsReverseTransaction);
			AssertEquals(false, TestBankTransfer.TransferRowTo.IsReverseTransaction);
			AssertEquals(false, TestBankTransfer.FinanceCharge.IsReverseTransaction);
			AssertEquals(false, TestBankTransfer.ExchangeDiff.IsReverseTransaction);
		}

		[ExpectNoExceptions]
		public void TestReversingReasonExceedsMaxLength()
		{
			string reverseReason = new string('d', AccTransactionHeaderSchema.AH_Desc.MaxLength + 1);
			TestBankTransfer.ReversingReason = reverseReason;
		}

		public void TestUnusualEntryOfData()
		{
			TestBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			TestBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			AssertEquals("Amounts reset after changing bank account", 0M, TestBankTransfer.TransferRowFrom.AH_InvoiceAmount);
			AssertEquals("Amounts reset after changing bank account", 0M, TestBankTransfer.TransferRowTo.AH_InvoiceAmount);
			TestBankTransfer.TransferRowFrom.AH_ExchangeRate = 1m;
			TestBankTransfer.TransferRowTo.AH_ExchangeRate = 1m;
			Factory.Save();
		}

		public void TestReadOnlyFieldsIfBanksNotSpecified()
		{
			var testBankTransfer = new BankTransfer(Factory, null);
			Assert("Precondition: new bank transfer object has no banks", testBankTransfer.BankTransferFromPK.IsEmpty);
			Assert("Precondition: new bank transfer object has no banks", testBankTransfer.BankTransferToPK.IsEmpty);
			Assert("Fields are readonly when object is new", testBankTransfer.BuyAmountInfo.ReadOnly);
			Assert("Fields are readonly when object is new", testBankTransfer.SellAmountInfo.ReadOnly);
			Assert("Fields are readonly when object is new", testBankTransfer.LocalBuyAmountInfo.ReadOnly);
			Assert("Fields are readonly when object is new", testBankTransfer.LocalSellAmountInfo.ReadOnly);
			Assert("Fields are readonly when object is new", testBankTransfer.TransferRowFrom.ExchangeRate.CurrencyInfo.ReadOnly);
			Assert("Fields are readonly when object is new", testBankTransfer.TransferRowTo.ExchangeRate.CurrencyInfo.ReadOnly);
			Assert("Fields are readonly when object is new", testBankTransfer.TransferRowFrom.ExchangeRate.RateInfo.ReadOnly);
			Assert("Fields are readonly when object is new", testBankTransfer.TransferRowTo.ExchangeRate.RateInfo.ReadOnly);
			testBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			testBankTransfer.SetFromBankAccountWithAsserts(ZGuid.Empty);
			Assert("Fields are readonly when only one bank specified", testBankTransfer.BuyAmountInfo.ReadOnly);
			Assert("Fields are readonly when only one bank specified", testBankTransfer.SellAmountInfo.ReadOnly);
			Assert("Fields are readonly when only one bank specified", testBankTransfer.LocalBuyAmountInfo.ReadOnly);
			Assert("Fields are readonly when only one bank specified", testBankTransfer.LocalSellAmountInfo.ReadOnly);
			Assert("Fields are readonly when object is new", testBankTransfer.TransferRowFrom.ExchangeRate.CurrencyInfo.ReadOnly);
			Assert("Fields are readonly when object is new", testBankTransfer.TransferRowTo.ExchangeRate.CurrencyInfo.ReadOnly);
			Assert("Fields are readonly when object is new", testBankTransfer.TransferRowFrom.ExchangeRate.RateInfo.ReadOnly);
			Assert("Fields are readonly when object is new", testBankTransfer.TransferRowTo.ExchangeRate.RateInfo.ReadOnly);
			testBankTransfer.SetToBankAccountWithAsserts(ZGuid.Empty);
			testBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			Assert("Fields are readonly when only one bank specified", testBankTransfer.BuyAmountInfo.ReadOnly);
			Assert("Fields are readonly when only one bank specified", testBankTransfer.SellAmountInfo.ReadOnly);
			Assert("Fields are readonly when only one bank specified", testBankTransfer.LocalBuyAmountInfo.ReadOnly);
			Assert("Fields are readonly when only one bank specified", testBankTransfer.LocalSellAmountInfo.ReadOnly);
			Assert("Fields are readonly when object is new", testBankTransfer.TransferRowFrom.ExchangeRate.CurrencyInfo.ReadOnly);
			Assert("Fields are readonly when object is new", testBankTransfer.TransferRowTo.ExchangeRate.CurrencyInfo.ReadOnly);
			Assert("Fields are readonly when object is new", testBankTransfer.TransferRowFrom.ExchangeRate.RateInfo.ReadOnly);
			Assert("Fields are readonly when object is new", testBankTransfer.TransferRowTo.ExchangeRate.RateInfo.ReadOnly);
			testBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			testBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.GBPBankAccount.PK);
			AssertEquals("Fields are writable when both banks specified", false, testBankTransfer.BuyAmountInfo.ReadOnly);
			AssertEquals("Fields are writable when both banks specified", false, testBankTransfer.SellAmountInfo.ReadOnly);
			AssertEquals("Fields are writable when both banks specified", false, testBankTransfer.LocalBuyAmountInfo.ReadOnly);
			AssertEquals("Fields are writable when both banks specified", false, testBankTransfer.LocalSellAmountInfo.ReadOnly);
			AssertEquals("Currency always readonly", true, testBankTransfer.TransferRowFrom.ExchangeRate.CurrencyInfo.ReadOnly);
			AssertEquals("Currency always readonly", true, testBankTransfer.TransferRowTo.ExchangeRate.CurrencyInfo.ReadOnly);
			AssertEquals("Fields are writable when both banks specified", false, testBankTransfer.TransferRowFrom.ExchangeRate.RateInfo.ReadOnly);
			AssertEquals("Fields are writable when both banks specified", false, testBankTransfer.TransferRowTo.ExchangeRate.RateInfo.ReadOnly);
			testBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			testBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			AssertEquals("Fields are writable when both banks specified", false, testBankTransfer.BuyAmountInfo.ReadOnly);
			AssertEquals("Fields are writable when both banks specified", false, testBankTransfer.SellAmountInfo.ReadOnly);
			AssertEquals("Fields are writable when both banks specified", false, testBankTransfer.LocalBuyAmountInfo.ReadOnly);
			AssertEquals("Fields are readonly due to local currency account", true, testBankTransfer.LocalSellAmountInfo.ReadOnly);
			AssertEquals("Currency always readonly", true, testBankTransfer.TransferRowFrom.ExchangeRate.CurrencyInfo.ReadOnly);
			AssertEquals("Currency always readonly", true, testBankTransfer.TransferRowTo.ExchangeRate.CurrencyInfo.ReadOnly);
			AssertEquals("Rate read only due to local currency account", true, testBankTransfer.TransferRowFrom.ExchangeRate.RateInfo.ReadOnly);
			AssertEquals("Fields are writable when both banks specified", false, testBankTransfer.TransferRowTo.ExchangeRate.RateInfo.ReadOnly);

			testBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			testBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.AUDBankAccount.PK);
			AssertEquals("Fields are writable when both banks specified", false, testBankTransfer.BuyAmountInfo.ReadOnly);
			AssertEquals("Fields are writable when both banks specified", false, testBankTransfer.SellAmountInfo.ReadOnly);
			AssertEquals("Currency always readonly", true, testBankTransfer.TransferRowFrom.ExchangeRate.CurrencyInfo.ReadOnly);
			AssertEquals("Currency always readonly", true, testBankTransfer.TransferRowTo.ExchangeRate.CurrencyInfo.ReadOnly);
			AssertEquals("Rate read only due to local currency account", true, testBankTransfer.TransferRowFrom.ExchangeRate.RateInfo.ReadOnly);
			AssertEquals("Rate read only due to local currency account", true, testBankTransfer.TransferRowTo.ExchangeRate.RateInfo.ReadOnly);
			AssertEquals("Fields are readonly due to local currency account", true, testBankTransfer.LocalSellAmountInfo.ReadOnly);
			AssertEquals("Fields are readonly due to local currency account", true, testBankTransfer.LocalBuyAmountInfo.ReadOnly);
		}

		public void TestDoNotValidateBranchDepartmentCombinationWhenReversing()
		{
			var bankTransfer = TestBankTransfer;
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			var currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			GlbBranchCombinationValidationTest.SetAllowedBranchDepartmentCombinations(currentBranch, new GlbDepartment[] { department });

			bankTransfer.GenerateReverseTransaction(true);
			var reversingBankTransfer = (BankTransfer)((IReversing)bankTransfer).ReverseTransaction;

			reversingBankTransfer.RunPreSaveValidation();
			reversingBankTransfer.TransferRowFrom.RunPreSaveValidation();
			reversingBankTransfer.TransferRowTo.RunPreSaveValidation();

			AssertEquals(reversingBankTransfer.TransferRowFrom.AH_GB, Env.CurrentBranch.PK);
			AssertEquals(reversingBankTransfer.TransferRowFrom.AH_GE, Env.CurrentDepartment.PK);

			AssertEquals(reversingBankTransfer.TransferRowTo.AH_GB, Env.CurrentBranch.PK);
			AssertEquals(reversingBankTransfer.TransferRowTo.AH_GE, Env.CurrentDepartment.PK);

			AssertNoErrors(reversingBankTransfer.TransferRowFrom.AH_GEInfo);
			AssertNoErrors(reversingBankTransfer.TransferRowTo.AH_GEInfo);
		}

		public void TestBankTransferShouldBalanceToZeroWhenGenerateReverseTransaction()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			{
				var bankTransfer = TestObjectCreator.CreateBankTransfer(ZDateTime.Today, TestObjectCreator.AUDBankAccount.PK, TestObjectCreator.CHNBankAccount.PK, 1401.62M, 0.113537M);
				bankTransfer.LocalSellAmount = 12345.06M;
				Factory.Save();

				bankTransfer.GenerateReverseTransaction(true);
				var reverseTransfer = (BankTransfer)bankTransfer.ReverseTransaction;
				AssertEquals(12345.06M, reverseTransfer.LocalSellAmount);
				AssertEquals(12345.06M, reverseTransfer.LocalBuyAmount);
				AssertEquals(-12345.06M, reverseTransfer.TransferRowFrom.AH_InvoiceAmount);
				AssertEquals(12345.06M, reverseTransfer.TransferRowTo.AH_InvoiceAmount);
				AssertEquals(-12345.06M, reverseTransfer.TransferRowFrom.AH_OSTotal);
				AssertEquals(1401.62M, reverseTransfer.TransferRowTo.AH_OSTotal);

				bankTransfer = TestObjectCreator.CreateBankTransfer(ZDateTime.Today, TestObjectCreator.CHNBankAccount.PK, TestObjectCreator.AUDBankAccount.PK, 12345.08M, 0.113537M);
				Factory.Save();

				bankTransfer.GenerateReverseTransaction(true);
				reverseTransfer = (BankTransfer)bankTransfer.ReverseTransaction;
				AssertEquals(12345.08M, reverseTransfer.LocalSellAmount);
				AssertEquals(12345.08M, reverseTransfer.LocalBuyAmount);
				AssertEquals(-12345.08M, reverseTransfer.TransferRowFrom.AH_InvoiceAmount);
				AssertEquals(12345.08M, reverseTransfer.TransferRowTo.AH_InvoiceAmount);
				AssertEquals(-1401.62M, reverseTransfer.TransferRowFrom.AH_OSTotal);
				AssertEquals(12345.08M, reverseTransfer.TransferRowTo.AH_OSTotal);
			}
		}

		public void TestFinanceChargeDescriptionDefaultValue()
		{
			DefaultNumberOfSupportingDocumentsCollection list = new DefaultNumberOfSupportingDocumentsCollection();
			list.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.FinanceCharge, (NoResString)"Cash Book Bank Transfer Finance Charge", (NoResString)"test测试測試", 1);

			using (AccountingConfigurationRegistry.Instance.VoucherNumberOfSupportingDocumentDefaults.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			{
				var bankTransfer = new BankTransfer(Factory, null);
				AssertEquals("Default Value should come from the registery", "TEST测试測試", bankTransfer.FinanceCharge_ForTestOnly.AH_Desc);
				Factory.Save();
				var savedBankTransfer = Factory.Load<BankTransfer>(bankTransfer.PK);
				AssertEquals(bankTransfer.FinanceCharge_ForTestOnly.AH_Desc, savedBankTransfer.FinanceCharge_ForTestOnly.AH_Desc);
			}
		}

		public void TestReversalFinanceChargeDescription()
		{
			BankTransfer bankTransfer = new BankTransfer(Factory, null);
			bankTransfer.TransactionNumber = "00020001";
			bankTransfer.FinanceCharge_ForTestOnly.AH_TransactionNum = "00088888";
			ReversingFactory reversingFactory = new ReversingFactory();
			ReversingBase reversing = reversingFactory.NewReversing(bankTransfer);
			reversing.Reverse();

			var reverseTransaction = (BankTransfer)bankTransfer.ReverseTransaction;
			reverseTransaction.ReversingCode = "IDE";
			reverseTransaction.ReversingReason = "test reason";

			AssertEquals("Reversed FinanceCharge description", "REVERSAL RELATED TO 00088888 test reason", reverseTransaction.FinanceCharge_ForTestOnly.AH_Desc);
		}

		public void TestFinanceChargeTaxDate()
		{
			TestBankTransfer.FinanceChargeOSAmount = 8m;
			var taxRate = CreateTaxRate();
			TestBankTransfer.FinanceChargeTaxID = taxRate.PK;
			AssertAmounts(ZDate.Today, 8M, 0.8M, 8.8M);

			TestBankTransfer.FinanceChargeTaxDate = ZDate.Today.AddDays(1);
			AssertAmounts(ZDate.Today.AddDays(1), 8M, 0.24M, 8.24M);

			void AssertAmounts(ZDate taxDate, decimal bankCharge, decimal taxAmount, decimal totalBankCharge)
			{
				AssertEquals(taxDate, TestBankTransfer.FinanceChargeTaxDate);
				AssertEquals(bankCharge, TestBankTransfer.FinanceChargeOSAmount);
				AssertEquals(taxAmount, TestBankTransfer.FinanceChargeOSTaxAmount);
				AssertEquals(totalBankCharge, TestBankTransfer.FinanceChargeOSTotal);
			}

			AccTaxRate CreateTaxRate()
			{
				var rate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
				rate.SetRate_ForTestOnly(10, 1, ZDate.Today.AddDays(-1), ZDate.Today);
				rate.SetRate_ForTestOnly(18, 6, ZDate.Today.AddDays(1), ZDate.Today.AddDays(2));
				return rate;
			}
		}

		[TestDate(2020, 5, 6)]
		public void TestFinanceChargeGovtChargeCode()
		{
			var transfer = TestObjectCreator.CreateBankTransfer(ZDateTime.Today, TestObjectCreator.AUDBankAccount.PK, TestObjectCreator.USDBankAccount.PK, 0m, 1m);
			transfer.FinanceChargeGovtChargeCode = "AAA";

			transfer.EnableFinanceCharge = true;
			Assert(!transfer.FinanceChargeGovtChargeCodeInfo.ReadOnly);

			transfer.EnableFinanceCharge = false;
			Assert(transfer.FinanceChargeGovtChargeCodeInfo.ReadOnly);

			ReversingFactory reversingFactory = new ReversingFactory();
			ReversingBase reversing = reversingFactory.NewReversing(transfer);
			reversing.Reverse();

			var reverseTransaction = (BankTransfer)transfer.ReverseTransaction;
			AssertEquals("Reversed FinanceCharge government charge code", "AAA", reverseTransaction.FinanceChargeGovtChargeCode);
		}

		public void TestBankTranferReversingValidatedForConcurrentReversal()
		{
			var handleError = TestBankTransfer as IHandleDeleteError;

			AssertNotNull("Implements IHandleError", handleError);
			Assert("RollbackAfterDeleteError false on new Bank Transfer", !handleError.RollbackAfterDeleteError);
			Assert("RebindAfterDeleteError is always false", !handleError.RebindAfterDeleteError);

			Factory.Save();

			Assert("RollbackAfterDeleteError is true by default", handleError.RollbackAfterDeleteError);
			Assert("DisableFormOnDeleteConcurrencyError is false by default", !handleError.DisableFormOnDeleteConcurrencyError);
			Assert("RebindAfterDeleteError is always false", !handleError.RebindAfterDeleteError);

			TestBankTransfer.TransferRowFrom.IsCancelled = true;

			Assert("RollbackAfterDeleteError on saved and cancelled Bank Transfer is false to prevent rollback to non-cancelled state", !handleError.RollbackAfterDeleteError);
			Assert("DisableFormOnDeleteConcurrencyError on saved and cancelled Bank Transfer is true", handleError.DisableFormOnDeleteConcurrencyError);
			Assert("RebindAfterDeleteError is always false", !handleError.RebindAfterDeleteError);
		}

		public void TestExRateGainLoss()
		{
			ConfigureTestBankTransferWithExchangeRateVariance();

			AssertEquals(2300m, TestBankTransfer.ExRateGainLoss);
			AssertExRateGainLoss(buyLocalAmount: 200m, sellLocalAmount: 100m, expectedGainLoss: 100m);
			AssertExRateGainLoss(buyLocalAmount: 100m, sellLocalAmount: 100m, expectedGainLoss: 0m);
			AssertExRateGainLoss(buyLocalAmount: 100m, sellLocalAmount: 200m, expectedGainLoss: -100m);
		}

		public void TestExRateGainLossWithSavedExchangeDiff()
		{
			ConfigureTestBankTransferWithExchangeRateVariance();
			Factory.Save();

			AssertEquals(2300m, TestBankTransfer.ExRateGainLoss);
			AssertExRateGainLoss(buyLocalAmount: 200m, sellLocalAmount: 100m, expectedGainLoss: 2300m);
			AssertExRateGainLoss(buyLocalAmount: 100m, sellLocalAmount: 100m, expectedGainLoss: 2300m);
			AssertExRateGainLoss(buyLocalAmount: 100m, sellLocalAmount: 200m, expectedGainLoss: 2300m);
		}

		public void TestExRateGainLossWhenBankTransferIsReversed()
		{
			ConfigureTestBankTransferWithExchangeRateVariance();

			TestBankTransfer.AssertSellProperties(100m, 0.5m, 200m);
			TestBankTransfer.AssertBuyProperties(500m, 0.2m, 2500m);
			AssertEquals(2300m, TestBankTransfer.ExRateGainLoss);

			TestBankTransfer.GenerateReverseTransaction(true);

			var reversal = TestBankTransfer.ReverseTransaction as BankTransfer;
			reversal.AssertSellProperties(500m, 0.2m, 2500m);
			reversal.AssertBuyProperties(100m, 0.5m, 200m);
			AssertEquals(-2300m, reversal.ExRateGainLoss);
		}

		void AssertExRateGainLoss(ZDecimal buyLocalAmount, ZDecimal sellLocalAmount, ZDecimal expectedGainLoss)
		{
			TestBankTransfer.LocalBuyAmount = buyLocalAmount;
			TestBankTransfer.LocalSellAmount = sellLocalAmount;
			AssertEquals(expectedGainLoss, TestBankTransfer.ExRateGainLoss);
		}

		public void TestShouldCalculateExchangeVariance()
		{
			var transfer = TestObjectCreator.CreateBankTransfer(ZDateTime.Today, TestObjectCreator.AUDBankAccount.PK, TestObjectCreator.USDBankAccount.PK, 0m, 1m);
			var resultForDummyEventForConfirming = false;
			var callingTimesForDummyEventForConfirming = 0;

			transfer.OnCancelingCalculateExchangeVariance += DummyEventForCanceling;
			AssertStatus_FromFalseToTrue();
			AssertStatus_FromTrueToFalse(confirmingResult: false, expectedShouldCalculateExchangeVariance: true, expectedEventCalledTimes: 1);
			AssertStatus_FromTrueToFalse(confirmingResult: true, expectedShouldCalculateExchangeVariance: false, expectedEventCalledTimes: 1);

			transfer.OnCancelingCalculateExchangeVariance -= DummyEventForCanceling;
			AssertStatus_FromFalseToTrue();
			AssertStatus_FromTrueToFalse(confirmingResult: false, expectedShouldCalculateExchangeVariance: false, expectedEventCalledTimes: 0);
			AssertStatus_FromFalseToTrue();
			AssertStatus_FromTrueToFalse(confirmingResult: true, expectedShouldCalculateExchangeVariance: false, expectedEventCalledTimes: 0);

			void AssertStatus_FromFalseToTrue()
			{
				AssertEquals("PreCondition", false, transfer.ShouldCalculateExchangeVariance);

				var callingTimesForDummyEventForConfirming_Before = callingTimesForDummyEventForConfirming;
				transfer.ShouldCalculateExchangeVariance = true;

				AssertEquals(0, callingTimesForDummyEventForConfirming - callingTimesForDummyEventForConfirming_Before);
				AssertEquals(true, transfer.ShouldCalculateExchangeVariance);
			}

			void AssertStatus_FromTrueToFalse(bool confirmingResult, bool expectedShouldCalculateExchangeVariance, int expectedEventCalledTimes)
			{
				AssertEquals("PreCondition", true, transfer.ShouldCalculateExchangeVariance);

				resultForDummyEventForConfirming = confirmingResult;

				var callingTimesForDummyEventForCanceling = callingTimesForDummyEventForConfirming;
				transfer.ShouldCalculateExchangeVariance = false;

				AssertEquals(expectedEventCalledTimes, callingTimesForDummyEventForConfirming - callingTimesForDummyEventForCanceling);
				AssertEquals(expectedShouldCalculateExchangeVariance, transfer.ShouldCalculateExchangeVariance);
			}

			bool DummyEventForCanceling()
			{
				callingTimesForDummyEventForConfirming++;
				return resultForDummyEventForConfirming;
			}
		}

		public void TestShouldCalculateExchangeVarianceReadOnly()
		{
			var transfer = TestObjectCreator.CreateBankTransfer(ZDateTime.Today, TestObjectCreator.AUDBankAccount.PK, TestObjectCreator.USDBankAccount.PK, 0m, 1m);
			AssertEquals("PreCondition", false, transfer.ShouldCalculateExchangeVariance);
			AssertEquals("PreCondition", false, transfer.ShouldCalculateExchangeVarianceIsReadOnly);

			transfer.ShouldCalculateExchangeVariance = true;
			transfer.BankTransferToPK = TestObjectCreator.AUDBankAccount2.PK;
			AssertEquals("ShouldCalculateExchangeVariance should be readonly when both currency are local.", true, transfer.ShouldCalculateExchangeVarianceIsReadOnly);
			AssertEquals("ShouldCalculateExchangeVariance should be set to false when it becomes readonly.", false, transfer.ShouldCalculateExchangeVariance);

			transfer.BankTransferFromPK = ZGuid.Invalid;
			AssertNull("PreCondition", transfer.TransferRowFrom.BankAccount);
			AssertEquals("ShouldCalculateExchangeVariance should not be readonly when bank account is null.", false, transfer.ShouldCalculateExchangeVarianceIsReadOnly);
		}

		public void TestExchangeRate()
		{
			BankTransfer testBankTransfer = new BankTransfer(Factory, null);
			TestObjectCreator.AUDBankAccount.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			testBankTransfer.BankTransferFromPK = TestObjectCreator.USDBankAccount.PK;
			testBankTransfer.BankTransferToPK = TestObjectCreator.AUDBankAccount.PK;
			AssertEquals("PreCondition", 1m, testBankTransfer.BuyExchangeRate);

			AssertEquals("PreCondition", false, testBankTransfer.ShouldCalculateExchangeVariance);

			testBankTransfer.SellAmount = 100m;
			testBankTransfer.SellZExchangeRate.Rate = 0.5m;
			AssertEquals(200m, testBankTransfer.LocalSellAmount);
			AssertEquals(200m, testBankTransfer.LocalBuyAmount);
			AssertEquals(200m, testBankTransfer.BuyAmount);

			testBankTransfer.SellZExchangeRate.Rate = 1m;
			AssertEquals(100m, testBankTransfer.LocalSellAmount);
			AssertEquals(100m, testBankTransfer.LocalBuyAmount);
			AssertEquals(100m, testBankTransfer.BuyAmount);

			testBankTransfer.BankTransferFromPK = TestObjectCreator.AUDBankAccount.PK;
			testBankTransfer.BankTransferToPK = TestObjectCreator.USDBankAccount.PK;
			AssertEquals("PreCondition", 1m, testBankTransfer.SellExchangeRate);

			testBankTransfer.SellAmount = 100m;
			AssertEquals("PreCondition", 100m, testBankTransfer.LocalBuyAmount);

			testBankTransfer.BuyZExchangeRate.Rate = 2m;
			AssertEquals(100m, testBankTransfer.LocalSellAmount);
			AssertEquals(100m, testBankTransfer.LocalBuyAmount);
			AssertEquals(200m, testBankTransfer.BuyAmount);
			AssertEquals(100m, testBankTransfer.SellAmount);

			testBankTransfer.BuyAmount = 100m;
			AssertEquals(1m, testBankTransfer.BuyExchangeRate);
		}

		public void TestBuyExchangeRateWhenCalculateExchangeVariance()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.EUR, 1.5m);

			var transfer = TestObjectCreator.CreateBankTransfer(ZDateTime.Today, TestObjectCreator.EURBankAccount.PK, TestObjectCreator.USDBankAccount.PK, 0, 1.5m);
			transfer.SellAmount = 150m;
			transfer.LocalSellAmount = 100m;
			transfer.BuyExchangeRate = 2m;
			CombineAssertions("PreCondition", () => {
				AssertEquals("LocalBuyAmount", 100m, transfer.LocalBuyAmount);
				AssertEquals("BuyExchangeRate", 2m, transfer.BuyExchangeRate);
				AssertEquals("BuyAmount", 200m, transfer.BuyAmount);
				AssertEquals("LocalSellAmount", 100m, transfer.LocalSellAmount);
				AssertEquals("SellExchangeRate", 1.5m, transfer.SellExchangeRate);
				AssertEquals("SellAmount", 150m, transfer.SellAmount);
			});

			transfer.ShouldCalculateExchangeVariance = true;
			transfer.BuyExchangeRate = 5m;

			CombineAssertions("Should not change amount fields which are relevant to Sell.", () => {
				AssertEquals("LocalSellAmount", 100m, transfer.LocalSellAmount);
				AssertEquals("SellExchangeRate", 1.5m, transfer.SellExchangeRate);
				AssertEquals("SellAmount", 150m, transfer.SellAmount);
			});

			AssertEquals("Change LocalBuyAmount", 40m, transfer.LocalBuyAmount);
			AssertEquals("Keep BuyAmount", 200m, transfer.BuyAmount);
		}

		public void TestSellExchangeRateWhenCalculateExchangeVariance()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 2m);

			var transfer = TestObjectCreator.CreateBankTransfer(ZDateTime.Today, TestObjectCreator.EURBankAccount.PK, TestObjectCreator.USDBankAccount.PK, 0, 1.5m);
			transfer.SellAmount = 150m;
			transfer.LocalSellAmount = 100m;
			transfer.BuyExchangeRate = 2m;
			CombineAssertions("PreCondition", () => {
				AssertEquals("LocalBuyAmount", 100m, transfer.LocalBuyAmount);
				AssertEquals("BuyExchangeRate", 2m, transfer.BuyExchangeRate);
				AssertEquals("BuyAmount", 200m, transfer.BuyAmount);
				AssertEquals("LocalSellAmount", 100m, transfer.LocalSellAmount);
				AssertEquals("SellExchangeRate", 1.5m, transfer.SellExchangeRate);
				AssertEquals("SellAmount", 150m, transfer.SellAmount);
			});

			transfer.ShouldCalculateExchangeVariance = true;
			transfer.SellExchangeRate = 5m;

			CombineAssertions("Should not change amount fields which are relevant to Buy.", () => {
				AssertEquals("LocalBuyAmount", 100m, transfer.LocalBuyAmount);
				AssertEquals("BuyExchangeRate", 2m, transfer.BuyExchangeRate);
				AssertEquals("BuyAmount", 200m, transfer.BuyAmount);
			});

			AssertEquals("LocalSellAmount", 30m, transfer.LocalSellAmount);
			AssertEquals("SellAmount", 150m, transfer.SellAmount);
		}

		public void TestAmountChangedWhenCalculateExchangeVariance()
		{
			TestBankTransfer.ShouldCalculateExchangeVariance = true;
			AssertEquals("PreCondition", true, TestBankTransfer.ShouldCalculateExchangeVariance);

			AssertBuyForeignSellForeign();
			AssertBuyLocalSellForeign();
			AssertBuyForeignSellLocal();

			void AssertBuyForeignSellForeign()
			{
				TestBankTransfer.BankTransferFromPK = TestObjectCreator.USDBankAccount.PK;
				TestBankTransfer.BankTransferToPK = TestObjectCreator.GBPBankAccount.PK;
				AssertEquals("PreCondition", true, TestBankTransfer.ShouldCalculateExchangeVariance);

				TestBankTransfer.SellZExchangeRate.Rate = 0.5m;
				TestBankTransfer.SellAmount = 200m;
				TestBankTransfer.BuyZExchangeRate.Rate = 2m;
				TestBankTransfer.BuyAmount = 100m;
				CombineAssertions("PreCondition", () => {
					AssertEquals("LocalBuyAmount", 50m, TestBankTransfer.LocalBuyAmount);
					AssertEquals("BuyExchangeRate", 2m, TestBankTransfer.BuyExchangeRate);
					AssertEquals("BuyAmount", 100m, TestBankTransfer.BuyAmount);
					AssertEquals("LocalSellAmount", 400m, TestBankTransfer.LocalSellAmount);
					AssertEquals("SellExchangeRate", 0.5m, TestBankTransfer.SellExchangeRate);
					AssertEquals("SellAmount", 200m, TestBankTransfer.SellAmount);
				});

				AssertBuyAmountNotChanged(() => {
					TestBankTransfer.SellAmount = 300m;
				});
				AssertEquals("LocalSellAmount", 600m, TestBankTransfer.LocalSellAmount);
				AssertEquals("SellAmount", 300m, TestBankTransfer.SellAmount);
				AssertEquals("SellExchangeRate", 0.5m, TestBankTransfer.SellExchangeRate);

				AssertBuyAmountNotChanged(() => {
					TestBankTransfer.LocalSellAmount = 250m;
				});
				AssertEquals("LocalSellAmount", 250m, TestBankTransfer.LocalSellAmount);
				AssertEquals("SellAmount", 300m, TestBankTransfer.SellAmount);
				AssertEquals("SellExchangeRate", 1.2m, TestBankTransfer.SellExchangeRate);

				AssertSellAmountNotChanged(() => {
					TestBankTransfer.BuyAmount = 150m;
				});
				AssertEquals("LocalBuyAmount", 75m, TestBankTransfer.LocalBuyAmount);
				AssertEquals("BuyAmount", 150m, TestBankTransfer.BuyAmount);
				AssertEquals("BuyExchangeRate", 2m, TestBankTransfer.BuyExchangeRate);

				AssertSellAmountNotChanged(() => {
					TestBankTransfer.LocalBuyAmount = 600m;
				});
				AssertEquals("LocalBuyAmount", 600m, TestBankTransfer.LocalBuyAmount);
				AssertEquals("BuyAmount", 150m, TestBankTransfer.BuyAmount);
				AssertEquals("BuyExchangeRate", 0.25m, TestBankTransfer.BuyExchangeRate);
			}

			void AssertBuyLocalSellForeign()
			{
				TestBankTransfer.BankTransferFromPK = TestObjectCreator.USDBankAccount.PK;
				TestBankTransfer.BankTransferToPK = TestObjectCreator.AUDBankAccount.PK;
				AssertEquals("PreCondition", true, TestBankTransfer.ShouldCalculateExchangeVariance);

				TestBankTransfer.SellZExchangeRate.Rate = 0.5m;
				TestBankTransfer.SellAmount = 200m;
				TestBankTransfer.BuyAmount = 100m;
				CombineAssertions("PreCondition", () => {
					AssertEquals("LocalBuyAmount", 100m, TestBankTransfer.LocalBuyAmount);
					AssertEquals("BuyExchangeRate", 1m, TestBankTransfer.BuyExchangeRate);
					AssertEquals("BuyAmount", 100m, TestBankTransfer.BuyAmount);
					AssertEquals("LocalSellAmount", 400m, TestBankTransfer.LocalSellAmount);
					AssertEquals("SellExchangeRate", 0.5m, TestBankTransfer.SellExchangeRate);
					AssertEquals("SellAmount", 200m, TestBankTransfer.SellAmount);
				});

				AssertBuyAmountNotChanged(() => {
					TestBankTransfer.SellAmount = 300m;
				});
				AssertEquals("LocalSellAmount", 600m, TestBankTransfer.LocalSellAmount);
				AssertEquals("SellAmount", 300m, TestBankTransfer.SellAmount);
				AssertEquals("SellExchangeRate", 0.5m, TestBankTransfer.SellExchangeRate);

				AssertBuyAmountNotChanged(() => {
					TestBankTransfer.LocalSellAmount = 250m;
				});
				AssertEquals("LocalSellAmount", 250m, TestBankTransfer.LocalSellAmount);
				AssertEquals("SellAmount", 300m, TestBankTransfer.SellAmount);
				AssertEquals("SellExchangeRate", 1.2m, TestBankTransfer.SellExchangeRate);

				AssertSellAmountNotChanged(() => {
					TestBankTransfer.BuyAmount = 150m;
				});
				AssertEquals("LocalBuyAmount", 150m, TestBankTransfer.LocalBuyAmount);
				AssertEquals("BuyAmount", 150m, TestBankTransfer.BuyAmount);
				AssertEquals("BuyExchangeRate", 1m, TestBankTransfer.BuyExchangeRate);

				AssertSellAmountNotChanged(() => {
					TestBankTransfer.LocalBuyAmount = 600m;
				});
				AssertEquals("LocalBuyAmount", 600m, TestBankTransfer.LocalBuyAmount);
				AssertEquals("BuyAmount", 600m, TestBankTransfer.BuyAmount);
				AssertEquals("BuyExchangeRate", 1m, TestBankTransfer.BuyExchangeRate);
			}

			void AssertBuyForeignSellLocal()
			{
				TestBankTransfer.BankTransferFromPK = TestObjectCreator.AUDBankAccount.PK;
				TestBankTransfer.BankTransferToPK = TestObjectCreator.GBPBankAccount.PK;
				TestBankTransfer.ShouldCalculateExchangeVariance = true;
				AssertEquals("PreCondition", true, TestBankTransfer.ShouldCalculateExchangeVariance);

				TestBankTransfer.SellAmount = 200m;
				TestBankTransfer.BuyZExchangeRate.Rate = 2m;
				TestBankTransfer.BuyAmount = 100m;
				CombineAssertions("PreCondition", () => {
					AssertEquals("LocalBuyAmount", 50m, TestBankTransfer.LocalBuyAmount);
					AssertEquals("BuyExchangeRate", 2m, TestBankTransfer.BuyExchangeRate);
					AssertEquals("BuyAmount", 100m, TestBankTransfer.BuyAmount);
					AssertEquals("LocalSellAmount", 200m, TestBankTransfer.LocalSellAmount);
					AssertEquals("SellExchangeRate", 1m, TestBankTransfer.SellExchangeRate);
					AssertEquals("SellAmount", 200m, TestBankTransfer.SellAmount);
				});

				AssertBuyAmountNotChanged(() => {
					TestBankTransfer.SellAmount = 300m;
				});
				AssertEquals("LocalSellAmount", 300m, TestBankTransfer.LocalSellAmount);
				AssertEquals("SellAmount", 300m, TestBankTransfer.SellAmount);
				AssertEquals("SellExchangeRate", 1m, TestBankTransfer.SellExchangeRate);

				AssertBuyAmountNotChanged(() => {
					TestBankTransfer.LocalSellAmount = 250m;
				});
				AssertEquals("LocalSellAmount", 250m, TestBankTransfer.LocalSellAmount);
				AssertEquals("SellAmount", 250m, TestBankTransfer.SellAmount);
				AssertEquals("SellExchangeRate", 1m, TestBankTransfer.SellExchangeRate);

				AssertSellAmountNotChanged(() => {
					TestBankTransfer.BuyAmount = 150m;
				});
				AssertEquals("LocalBuyAmount", 75m, TestBankTransfer.LocalBuyAmount);
				AssertEquals("BuyAmount", 150m, TestBankTransfer.BuyAmount);
				AssertEquals("BuyExchangeRate", 2m, TestBankTransfer.BuyExchangeRate);

				AssertSellAmountNotChanged(() => {
					TestBankTransfer.LocalBuyAmount = 600m;
				});
				AssertEquals("LocalBuyAmount", 600m, TestBankTransfer.LocalBuyAmount);
				AssertEquals("BuyAmount", 150m, TestBankTransfer.BuyAmount);
				AssertEquals("BuyExchangeRate", 0.25m, TestBankTransfer.BuyExchangeRate);
			}

			void AssertBuyAmountNotChanged(Action method)
			{
				var originalLocalBuyAmount = TestBankTransfer.LocalBuyAmount;
				var originalBuyExchangeRate = TestBankTransfer.BuyExchangeRate;
				var originalBuyAmount = TestBankTransfer.BuyAmount;

				method();

				CombineAssertions("Should not change amount fields which are relevant to Buy.", () => {
					AssertEquals("LocalBuyAmount", originalLocalBuyAmount, TestBankTransfer.LocalBuyAmount);
					AssertEquals("BuyExchangeRate", originalBuyExchangeRate, TestBankTransfer.BuyExchangeRate);
					AssertEquals("BuyAmount", originalBuyAmount, TestBankTransfer.BuyAmount);
				});
			}

			void AssertSellAmountNotChanged(Action method)
			{
				var originalLocalSellAmount = TestBankTransfer.LocalSellAmount;
				var originalSellExchangeRate = TestBankTransfer.SellExchangeRate;
				var originalSellAmount = TestBankTransfer.SellAmount;

				method();

				CombineAssertions("Should not change amount fields which are relevant to Sell.", () => {
					AssertEquals("LocalSellAmount", originalLocalSellAmount, TestBankTransfer.LocalSellAmount);
					AssertEquals("SellExchangeRate", originalSellExchangeRate, TestBankTransfer.SellExchangeRate);
					AssertEquals("SellAmount", originalSellAmount, TestBankTransfer.SellAmount);
				});
			}
		}

		public void TestBankTranferReversingAlsoReverseRelatedEXX_ForeignToForeignCurrency()
			=> AssertBankTranferReversingAlsoReverseRelatedEXX(
				TestObjectCreator.USDBankAccount,
				TestObjectCreator.GBPBankAccount,
				sellExchangeRate: 0.5m,
				buyExchangeRate: 0.2m,
				sellAmount: 100m,
				buyAmount: 500m,
				expectedReversedBuyOSAmount: 500m,
				expectedReversedBuyLocalAmount: 200m,
				expectedReversedBuyExchangeRate: 2.5m,
				expectedReversedSellOSAmount: 100m,
				expectedReversedSellLocalAmount: 200m,
				expectedReversedSellExchangeRate: 0.5m
			);

		public void TestBankTranferReversingAlsoReverseRelatedEXX_LocalToForeignCurrency()
			=> AssertBankTranferReversingAlsoReverseRelatedEXX(
				TestObjectCreator.AUDBankAccount,
				TestObjectCreator.USDBankAccount,
				sellExchangeRate: 1m,
				buyExchangeRate: 1.2m,
				sellAmount: 500m,
				buyAmount: 500m,
				expectedReversedBuyOSAmount: 500m,
				expectedReversedBuyLocalAmount: 500m,
				expectedReversedBuyExchangeRate: 1m,
				expectedReversedSellOSAmount: 500m,
				expectedReversedSellLocalAmount: 500m,
				expectedReversedSellExchangeRate: 1m
			);

		public void TestBankTranferReversingAlsoReverseRelatedEXX_ForeignToLocalCurrency()
			=> AssertBankTranferReversingAlsoReverseRelatedEXX(
				TestObjectCreator.USDBankAccount,
				TestObjectCreator.AUDBankAccount,
				sellExchangeRate: 0.5m,
				buyExchangeRate: 1m,
				sellAmount: 100m,
				buyAmount: 500m,
				expectedReversedBuyOSAmount: 200m,
				expectedReversedBuyLocalAmount: 200m,
				expectedReversedBuyExchangeRate: 1m,
				expectedReversedSellOSAmount: 100m,
				expectedReversedSellLocalAmount: 200m,
				expectedReversedSellExchangeRate: 0.5m
			);

		void AssertBankTranferReversingAlsoReverseRelatedEXX(
			AccBankAccount fromAccount,
			AccBankAccount toAccount,
			ZDecimal sellExchangeRate,
			ZDecimal buyExchangeRate,
			ZDecimal sellAmount,
			ZDecimal buyAmount,
			ZDecimal expectedReversedBuyOSAmount,
			ZDecimal expectedReversedBuyLocalAmount,
			ZDecimal expectedReversedBuyExchangeRate,
			ZDecimal expectedReversedSellOSAmount,
			ZDecimal expectedReversedSellLocalAmount,
			ZDecimal expectedReversedSellExchangeRate)
		{
			var year = ZDateTime.Now.Year;
			var testObjectCreatorInNewFactory = new TestObjectCreator(Factory.CreateNewFactory());
			testObjectCreatorInNewFactory.CreateTestPeriodsForEntireYear(year);
			testObjectCreatorInNewFactory.CreateTestPeriodsForEntireYear(year + 1);

			TestBankTransfer.BankTransferFromPK = fromAccount.PK;
			TestBankTransfer.BankTransferToPK = toAccount.PK;
			TestBankTransfer.ShouldCalculateExchangeVariance = true;
			TestBankTransfer.SellAmount = sellAmount;
			TestBankTransfer.SellExchangeRate = sellExchangeRate;
			TestBankTransfer.BuyAmount = buyAmount;
			TestBankTransfer.BuyExchangeRate = buyExchangeRate;

			Factory.Save();

			var query1 = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, TestBankTransfer.TransactionBelongsToGroup);
			query1.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference);
			var allExchangeDiff1 = Factory.Load<CashbookExchangeDiff>(query1);
			AssertEquals("Precondition", 1, allExchangeDiff1.Length);

			var originalExchangeDiff = allExchangeDiff1[0];
			AssertEquals("Precondition", false, originalExchangeDiff.IsReversed);
			AssertEquals(false, TestBankTransfer.TransferRowTo.IsReversed);

			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(TestBankTransfer);
			reversing.Reverse();
			Factory.Save();

			AssertEquals("Original ExchangeDiff should be reversed.", true, originalExchangeDiff.IsReversed);
			AssertEquals("TransferRowTo should be reversed.", true, TestBankTransfer.TransferRowTo.IsReversed);

			var query2 = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, ((BankTransfer)TestBankTransfer.ReverseTransaction).TransactionBelongsToGroup);
			query2.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference);
			allExchangeDiff1 = Factory.Load<CashbookExchangeDiff>(query1);
			var allExchangeDiff2 = Factory.Load<CashbookExchangeDiff>(query2);
			CombineAssertions("There will only be 2 CashbookExchangeDiff.", () =>
			{
				AssertEquals(1, allExchangeDiff1.Length);
				AssertEquals(1, allExchangeDiff2.Length);
				AssertEquals("Have the original one.", TransactionCountConstants.BankTransferExchangeDiff, allExchangeDiff1[0].AH_TransactionCount);
				AssertEquals("Have the reversal one.", TransactionCountConstants.BankTransferExchangeDiffWhenReversing, allExchangeDiff2[0].AH_TransactionCount);
			});

			var reverseBankTransfer = (BankTransfer)TestBankTransfer.ReverseTransaction;
			var reverseExchangeDiff = reverseBankTransfer.ExchangeDiff;

			CombineAssertions("Reversed TransferRowFrom values should be set properly.", () =>
			{
				AssertEquals("AH_OSTotalAmount", expectedReversedBuyOSAmount, reverseBankTransfer.TransferRowFrom.AH_OSTotalAmount);
				AssertEquals("AH_LocalTotalAmount", expectedReversedBuyLocalAmount, reverseBankTransfer.TransferRowFrom.AH_LocalTotalAmount);
				AssertEquals("AH_ExchangeRate", expectedReversedBuyExchangeRate, reverseBankTransfer.TransferRowFrom.AH_ExchangeRate);
				AssertEquals("AH_TransactionCount", TransactionCountConstants.BankTransferFromRowWhenReversing, reverseBankTransfer.TransferRowFrom.AH_TransactionCount);
				AssertEquals("AH_TransactionBelongsToGroup", reverseBankTransfer.TransactionBelongsToGroup, reverseBankTransfer.TransferRowFrom.AH_TransactionBelongsToGroup);
				AssertEquals("AH_PostDate", ZDateTime.Today.Date, reverseBankTransfer.TransferRowFrom.AH_PostDate.Date);
				AssertEquals("AH_InvoiceDate", ZDateTime.Today.Date, reverseBankTransfer.TransferRowFrom.AH_InvoiceDate.Date);
				AssertEquals("AH_Desc", $"REVERSAL RELATED TO {TestBankTransfer.TransactionNumber}", reverseBankTransfer.TransferRowFrom.AH_Desc);
			});

			CombineAssertions("Reversed TransferRowTo values should be set properly.", () =>
			{
				AssertEquals("AH_OSTotalAmount", expectedReversedSellOSAmount, reverseBankTransfer.TransferRowTo.AH_OSTotalAmount);
				AssertEquals("AH_LocalTotalAmount", expectedReversedSellLocalAmount, reverseBankTransfer.TransferRowTo.AH_LocalTotalAmount);
				AssertEquals("AH_ExchangeRate", expectedReversedSellExchangeRate, reverseBankTransfer.TransferRowTo.AH_ExchangeRate);
				AssertEquals("AH_TransactionCount", TransactionCountConstants.BankTransferToRowWhenReversing, reverseBankTransfer.TransferRowTo.AH_TransactionCount);
				AssertEquals("AH_TransactionBelongsToGroup", reverseBankTransfer.TransactionBelongsToGroup, reverseBankTransfer.TransferRowTo.AH_TransactionBelongsToGroup);
				AssertEquals("AH_PostDate", ZDateTime.Today.Date, reverseBankTransfer.TransferRowTo.AH_PostDate.Date);
				AssertEquals("AH_InvoiceDate", ZDateTime.Today.Date, reverseBankTransfer.TransferRowTo.AH_InvoiceDate.Date);
				AssertEquals("AH_Desc", $"REVERSAL RELATED TO {TestBankTransfer.TransactionNumber}", reverseBankTransfer.TransferRowTo.AH_Desc);
			});

			CombineAssertions("Reversed ExchangeDiff values should be set properly.", () =>
			{
				AssertEquals("AH_TransactionCount", TransactionCountConstants.BankTransferExchangeDiffWhenReversing, reverseExchangeDiff.AH_TransactionCount);
				AssertEquals("TransactionBelongsToGroup", reverseBankTransfer.TransactionBelongsToGroup, reverseExchangeDiff.AH_TransactionBelongsToGroup);
				AssertEquals("AH_TransactionCategory", TransactionCategory.Codes.RealizedExchangeGainLoss, reverseExchangeDiff.AH_TransactionCategory);
				AssertEquals("AH_AG", originalExchangeDiff.AH_AG, reverseExchangeDiff.AH_AG);
				AssertEquals("AH_AB", originalExchangeDiff.AH_AB, reverseExchangeDiff.AH_AB);
				AssertEquals("AH_RX_NKTransactionCurrency", originalExchangeDiff.AH_RX_NKTransactionCurrency, reverseExchangeDiff.AH_RX_NKTransactionCurrency);
				AssertEquals("AH_ExchangeRate", originalExchangeDiff.AH_ExchangeRate, reverseExchangeDiff.AH_ExchangeRate);
				AssertEquals("AH_OSTotal", -originalExchangeDiff.AH_OSTotal, reverseExchangeDiff.AH_OSTotal);
				AssertEquals("AH_LocalTotalAmount", -originalExchangeDiff.AH_LocalTotalAmount, reverseExchangeDiff.AH_LocalTotalAmount);
				AssertEquals("AH_PostDate", ZDateTime.Today.Date, reverseExchangeDiff.AH_PostDate.Date);
				AssertEquals("AH_InvoiceDate", ZDateTime.Today.Date, reverseExchangeDiff.AH_InvoiceDate.Date);
				AssertEquals("AH_Desc", $"REVERSAL RELATED TO {originalExchangeDiff.AH_TransactionNum}", reverseExchangeDiff.AH_Desc);
			});
		}

		public void TestRedefaultExchangeRateWhenCalculateExchangeVariance_BuyCurrencyIsForeignCurrency()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 2m);

			var bankTransfer = TestObjectCreator.CreateBankTransfer(ZDateTime.Today, TestObjectCreator.AUDBankAccount.PK, TestObjectCreator.USDBankAccount.PK, 100m, 1m);
			bankTransfer.BuyExchangeRate = 1.5m;
			CombineAssertions("PreCondition", () =>
			{
				AssertEquals("SellAmount", 100m, bankTransfer.SellAmount);
				AssertEquals("SellExchangeRate", 1m, bankTransfer.SellExchangeRate);
				AssertEquals("LocalSellAmount", 100m, bankTransfer.LocalSellAmount);
				AssertEquals("BuyAmount", 150m, bankTransfer.BuyAmount);
				AssertEquals("BuyExchangeRate", 1.5m, bankTransfer.BuyExchangeRate);
				AssertEquals("LocalBuyAmount", 100m, bankTransfer.LocalBuyAmount);
				AssertEquals("ExRateGainLoss", 0m, bankTransfer.ExRateGainLoss);
			});

			bankTransfer.ShouldCalculateExchangeVariance = true;

			CombineAssertions("Should not change amount fields which are relevant to Sell.", () => {
				AssertEquals("SellAmount", 100m, bankTransfer.SellAmount);
				AssertEquals("SellExchangeRate", 1m, bankTransfer.SellExchangeRate);
				AssertEquals("LocalSellAmount", 100m, bankTransfer.LocalSellAmount);
			});

			AssertEquals("BuyAmount will not change", 150m, bankTransfer.BuyAmount);
			AssertEquals("BuyExchangeRate will be re-defaulted", 2m, bankTransfer.BuyExchangeRate);
			AssertEquals("LocalBuyAmount will change", 75m, bankTransfer.LocalBuyAmount);
			AssertEquals("ExRateGainLoss", -25m, bankTransfer.ExRateGainLoss);
		}

		public void TestRedefaultExchangeRateWhenCalculateExchangeVariance_SellCurrencyIsForeignCurrency()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 2m);

			var bankTransfer = TestObjectCreator.CreateBankTransfer(ZDateTime.Today, TestObjectCreator.USDBankAccount.PK, TestObjectCreator.AUDBankAccount.PK, 100m, 1m);
			bankTransfer.SellAmount = 150m;
			bankTransfer.LocalSellAmount = 100m;
			CombineAssertions("PreCondition", () =>
			{
				AssertEquals("SellAmount", 150m, bankTransfer.SellAmount);
				AssertEquals("SellExchangeRate", 1.5m, bankTransfer.SellExchangeRate);
				AssertEquals("LocalSellAmount", 100m, bankTransfer.LocalSellAmount);
				AssertEquals("BuyAmount", 100m, bankTransfer.BuyAmount);
				AssertEquals("BuyExchangeRate", 1m, bankTransfer.BuyExchangeRate);
				AssertEquals("LocalBuyAmount", 100m, bankTransfer.LocalBuyAmount);
				AssertEquals("ExRateGainLoss", 0m, bankTransfer.ExRateGainLoss);
			});

			bankTransfer.ShouldCalculateExchangeVariance = true;

			CombineAssertions("Should not change amount fields which are relevant to Buy.", () => {
				AssertEquals("BuyAmount", 100m, bankTransfer.BuyAmount);
				AssertEquals("BuyExchangeRate", 1m, bankTransfer.BuyExchangeRate);
				AssertEquals("LocalBuyAmount", 100m, bankTransfer.LocalBuyAmount);
			});

			AssertEquals("SellAmount will not change", 150m, bankTransfer.SellAmount);
			AssertEquals("SellExchangeRate will be re-defaulted", 2m, bankTransfer.SellExchangeRate);
			AssertEquals("LocalSellAmount will change", 75m, bankTransfer.LocalSellAmount);
			AssertEquals("ExRateGainLoss will change", 25m, bankTransfer.ExRateGainLoss);
		}

		public void TestRedefaultExchangeRateWhenCalculateExchangeVariance_BothCurrenciesAreForeignCurrencies()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.EUR, 3m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 5m);

			var bankTransfer = TestObjectCreator.CreateBankTransfer(ZDateTime.Today, TestObjectCreator.EURBankAccount.PK, TestObjectCreator.USDBankAccount.PK, 100m, 1m);
			bankTransfer.SellAmount = 150m;
			bankTransfer.SellExchangeRate = 1.5m;
			bankTransfer.BuyAmount = 200m;
			CombineAssertions("PreCondition", () =>
			{
				AssertEquals("SellAmount", 150m, bankTransfer.SellAmount);
				AssertEquals("SellExchangeRate", 1.5m, bankTransfer.SellExchangeRate);
				AssertEquals("LocalSellAmount", 100m, bankTransfer.LocalSellAmount);
				AssertEquals("BuyAmount", 200m, bankTransfer.BuyAmount);
				AssertEquals("BuyExchangeRate", 2m, bankTransfer.BuyExchangeRate);
				AssertEquals("LocalBuyAmount", 100m, bankTransfer.LocalBuyAmount);
				AssertEquals("ExRateGainLoss", 0m, bankTransfer.ExRateGainLoss);
			});

			bankTransfer.ShouldCalculateExchangeVariance = true;

			AssertEquals("SellAmount will not change", 150m, bankTransfer.SellAmount);
			AssertEquals("BuyAmount will not change", 200m, bankTransfer.BuyAmount);
			AssertEquals("SellExchangeRate will be re-defaulted", 3m, bankTransfer.SellExchangeRate);
			AssertEquals("BuyExchangeRate will be re-defaulted", 5m, bankTransfer.BuyExchangeRate);
			AssertEquals("LocalSellAmount will change", 50m, bankTransfer.LocalSellAmount);
			AssertEquals("LocalBuyAmount will change", 40m, bankTransfer.LocalBuyAmount);
			AssertEquals("ExRateGainLoss will change", -10m, bankTransfer.ExRateGainLoss);
		}

		public void TestFinanceChargeAndSellAmountUpdatedWhenChangeSellExchangeRate_FromPaymentBatch()
		{
			var paymentBatch = CreatePaymentBatch(TestObjectCreator.USDBankAccount.PK);
			var payment = CreatePayment();
			var approval = CreatePaymentApproval(paymentBatch.PK, payment.PK);
			var quote = CreateQuote(approval, "USD");
			AssertEquals(20m, quote.QU_FeeAmount);
			Factory.Save();

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 0.5m);
			var transfer = BankTransfer.PrepareBankTransferFromPayments(new List<Payment> { payment }, paymentBatch.APB_BatchNumber);
			AssertEquals("USD", transfer.BankTransferFundingInfoCalculator_ForTestOnly.FundingCurrency);
			AssertEquals(420m, transfer.SellAmount);
			AssertEquals(0.5m, transfer.SellExchangeRate);
			AssertEquals(440m, transfer.BuyAmount);
			AssertEquals(40m, transfer.FinanceChargeOSAmount);
			AssertEquals(true, transfer.ShouldCalculateExchangeVarianceIsReadOnly);

			transfer.SellExchangeRate = 0m;
			AssertEquals(420m, transfer.SellAmount);
			AssertEquals(0m, transfer.FinanceChargeOSAmount);
			AssertEquals(400m, transfer.BuyAmount);
			AssertEquals(true, transfer.ShouldCalculateExchangeVarianceIsReadOnly);

			transfer.SellExchangeRate = 0.1m;
			AssertEquals(420m, transfer.SellAmount);
			AssertEquals(200m, transfer.FinanceChargeOSAmount);
			AssertEquals(600m, transfer.BuyAmount);
			AssertEquals(true, transfer.ShouldCalculateExchangeVarianceIsReadOnly);
		}

		public void TestFinanceChargeNotUpdateWhenChangeSellExchangeRate_NotFromPaymentBatch()
		{
			var transfer = new BankTransfer(Factory, null);
			transfer.BankTransferFromPK = TestObjectCreator.USDBankAccount.PK;
			transfer.BankTransferToPK = TestObjectCreator.AUDBankAccount.PK;
			transfer.SellExchangeRate = 0.5m;
			transfer.FinanceChargeOSAmount = 50m;
			AssertEquals(50m, transfer.FinanceChargeOSAmount);
			AssertEquals(false, transfer.ShouldCalculateExchangeVarianceIsReadOnly);

			transfer.SellExchangeRate = 0.1m;
			AssertEquals(50m, transfer.FinanceChargeOSAmount);
			AssertEquals(false, transfer.ShouldCalculateExchangeVarianceIsReadOnly);
		}

		public void TestFinanceChargeAndSellAmountUpdatedWhenChangeFromBankAccount_FromPaymentBatch()
		{
			var paymentBatch = CreatePaymentBatch(TestObjectCreator.USDBankAccount.PK);
			var payment = CreatePayment();
			var approval = CreatePaymentApproval(paymentBatch.PK, payment.PK);
			var quote = CreateQuote(approval, "USD");
			AssertEquals(20m, quote.QU_FeeAmount);
			AssertNotNull(TestObjectCreator.GBPBankAccount);
			Factory.Save();

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 0.5m);
			var transfer = BankTransfer.PrepareBankTransferFromPayments(new List<Payment> { payment }, paymentBatch.APB_BatchNumber);
			AssertEquals("USD", transfer.BankTransferFundingInfoCalculator_ForTestOnly.FundingCurrency);
			AssertEquals(420m, transfer.SellAmount);
			AssertEquals(0.5m, transfer.SellExchangeRate);
			AssertEquals(440m, transfer.BuyAmount);
			AssertEquals(40m, transfer.FinanceChargeOSAmount);
			AssertEquals(true, transfer.ShouldCalculateExchangeVarianceIsReadOnly);

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.GBP, 0.1m);
			transfer.BankTransferFromPK = TestObjectCreator.GBPBankAccount.PK;
			AssertEquals(420m, transfer.SellAmount);
			AssertEquals(0.1m, transfer.SellExchangeRate);
			AssertEquals(600m, transfer.BuyAmount);
			AssertEquals(200m, transfer.FinanceChargeOSAmount);
			AssertEquals(true, transfer.ShouldCalculateExchangeVarianceIsReadOnly);
		}

		public void TestFinanceChargeNotUpdatedWhenChangeFromBankAccount_NotFromPaymentBatch()
		{
			AssertNotNull(TestObjectCreator.GBPBankAccount);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.GBP, 0.1m);
			Factory.Save();

			var transfer = new BankTransfer(Factory, null);
			transfer.BankTransferFromPK = TestObjectCreator.USDBankAccount.PK;
			transfer.BankTransferToPK = TestObjectCreator.AUDBankAccount.PK;
			transfer.SellExchangeRate = 0.5m;
			transfer.FinanceChargeOSAmount = 50m;
			AssertEquals(50m, transfer.FinanceChargeOSAmount);
			AssertEquals(false, transfer.ShouldCalculateExchangeVarianceIsReadOnly);

			transfer.BankTransferFromPK = TestObjectCreator.GBPBankAccount.PK;
			AssertEquals(0.1m, transfer.SellExchangeRate);
			AssertEquals(50m, transfer.FinanceChargeOSAmount);
			AssertEquals(false, transfer.ShouldCalculateExchangeVarianceIsReadOnly);
		}

		#region IDataExportBatchSource Members

		public void TestIsDataExportBatchSupported()
		{
			AssertEquals("IsDataExportBatchSupported", ((IDataExportBatchSource)TestBankTransfer.TransferRowFrom).IsDataExportBatchSupported, ((IDataExportBatchSource)TestBankTransfer).IsDataExportBatchSupported);
		}

		public void TestRelatedBatchCollection()
		{
			var batch = TestObjectCreator.CreateDataExportBatchForHeader(TestBankTransfer.TransferRowFrom);
			Factory.Save();
			AssertCollectionContains("Public collection contains batch", batch, TestBankTransfer.DataExportBatchCollection);
		}

		public void TestTransactionHeaderNotCreateNewObject()
		{
			var bankTransfer = new BankTransfer(Factory, null);

			var fromBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var toBanKAccount = Factory.NewWithValidTestData<AccBankAccount>();

			bankTransfer.BankTransferFromPK = fromBankAccount.PK;
			bankTransfer.BankTransferToPK = toBanKAccount.PK;
			bankTransfer.SellAmount = 400;

			bankTransfer.EnableFinanceCharge = true;
			bankTransfer.EnableFinanceCharge = false;

			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region ReversalStatusCode

		public void TestReversalStatusCode_ShouldBeEmptyn()
		{
			AssertEquals(nameof(TestBankTransfer.ReversalStatusCode), ZString.Empty, TestBankTransfer.ReversalStatusCode);
		}

		public void TestReversalStatusCode_ReadOnly__ShouldBeTrue()
		{
			AssertEquals(nameof(ITransaction.ReversalStatusCode_ReadOnly), true, (TestBankTransfer as ITransaction).ReversalStatusCode_ReadOnly);
		}

		public void ReversalStatusCodeList_ShouldBeNull()
		{
			AssertNull(nameof(ITransaction.ReversalStatusCodeList), (TestBankTransfer as ITransaction).ReversalStatusCodeList);
		}

		#endregion ReversalStatusCode

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new BankTransfer(Factory, null);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestBankTransfer = new BankTransfer(Factory, null);
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
		BankTransfer TestBankTransfer;

		#endregion
	}
}
