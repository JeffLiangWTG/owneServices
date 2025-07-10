using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.CashBook.ExchangeDifference.Testing
{
	[TestedType(typeof(CashbookExchangeDiff))]
	public class CashbookExchangeDiffTest : TransactionHeaderTest
	{
		#region Defaults

		public void TestDefaultTransactionDate()
		{
			AssertEquals("TransactionDate should be today", ZDateTime.Today, ExchangeDiff.AH_InvoiceDate);
		}

		public void TestDefaultDescription()
		{
			AssertEquals("Description should be 'Bank Currency Adjustment'", "CASH BOOK EXCHANGE DIFFERENCE", ExchangeDiff.AH_Desc);
		}

		public void TestDefaultTransactionCategory()
		{
			AssertEquals("Transaction Category should be 'UNR'", Core.Constants.TransactionCategory.Codes.UnrealizedExchangeGainLoss, ExchangeDiff.AH_TransactionCategory);
		}

		#endregion

		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		[TestDate(2000, 1, 21)]
		[SuspendCriticalValidation]
		public void TestControlAccount()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			Factory.Save();

			AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.GLHeader2.PK.ToGuid());

			var directPayment = Factory.NewWithValidTestData<DirectPayment.DirectPayment>();
			directPayment.AH_OSTotal = -11m;
			directPayment.AH_InvoiceAmount = -10m;
			directPayment.AH_GSTAmount = -1m;
			directPayment.AH_AB = bank.PK;
			directPayment.AH_ExchangeRate = 1m;
			directPayment.Lines.AddNew(directPayment.DependentTransactionLineType);
			directPayment.Lines[0].AL_LineAmount = -10m;
			directPayment.Lines[0].AL_OSAmount = -11m;
			directPayment.Lines[0].AL_GSTVAT = -1m;
			Factory.Save();

			var exchangeDiff = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			exchangeDiff.AH_InvoiceDate = new ZDateTime(2000, 1, 20);
			exchangeDiff.AH_PostDate = new ZDateTime(2000, 1, 21);
			exchangeDiff.AH_AB = bank.PK;

			exchangeDiff.AH_ExchangeRate = 1m;
			Assert(exchangeDiff.ForeignCurrencyGainLoss == 0);
			AssertEquals(testObjectCreator.GLHeader1.PK, exchangeDiff.ExchangeGainLossAccount);

			exchangeDiff.AH_ExchangeRate = 1.5m;
			Assert(exchangeDiff.ForeignCurrencyGainLoss > 0);
			AssertEquals(testObjectCreator.GLHeader1.PK, exchangeDiff.ExchangeGainLossAccount);

			exchangeDiff.AH_ExchangeRate = 0.1m;
			Assert(exchangeDiff.ForeignCurrencyGainLoss < 0);
			AssertEquals(testObjectCreator.GLHeader2.PK, exchangeDiff.ExchangeGainLossAccount);

			Factory.Save();
			AssertEquals(testObjectCreator.GLHeader2.PK, exchangeDiff.AH_AG);
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesCashbookExchangeDiff()
		{
			var localList = new List<string>
				{
					nameof(ExchangeDiff.LocalAmountBeforeAdjustment),
					nameof(ExchangeDiff.LocalAmountAfterAdjustment),
					nameof(ExchangeDiff.ForeignCurrencyGainLoss)
				};

			var osList = new List<string>
				{
					nameof(ExchangeDiff.BankCurrencyBalance)
				};

			var exList = new List<string>
				{
					nameof(ExchangeDiff.CurrentExchangeRate)
				};

			var tester = new DecimalPlacesAttributeTester(ExchangeDiff);
			tester.CheckLocalCurrency(localList, nameof(ExchangeDiff.LocalCurrencyDecimals));
			tester.CheckNonLocalCurrency(osList, nameof(ExchangeDiff.OSCurrencyDecimals), nameof(ExchangeDiff.AH_RX_NKTransactionCurrency), ExchangeDiff);
			tester.CheckExchangeRate(exList, nameof(ExchangeDiff.ExchangeRateDecimalPlaces));
		}

		public override void TestInternalOSAmountFieldsSetOnLoadCorrectly()
		{
			AccBankAccount bank = TestObjectCreator.AUDBankAccount;
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.AUD, 1M);

			DirectReceipt.DirectReceipt dRC = Factory.NewWithValidTestData<DirectReceipt.DirectReceipt>();
			dRC.AH_AB = bank.PK;
			dRC.Lines.AddNew();
			DirectReceipt.DirectReceiptLine line = (DirectReceipt.DirectReceiptLine)dRC.Lines[0];
			line.AL_OSExTaxAmount = 30;

			Factory.Save();

			ExchangeDiff.AH_AB = bank.PK;
			AssertEquals("BankCurrencyBalance should be 30", 30m, ExchangeDiff.BankCurrencyBalance);

			Factory.Save();

			CashbookExchangeDiff loadedHeader = ReadOnlyFactory.Load<CashbookExchangeDiff>(ExchangeDiff.PK);

			AssertEquals("OS Ex Tax Amount on Load", 0m, loadedHeader.AH_OSExTaxAmount);
			AssertEquals("OS Tax Amount on Load", 0m, loadedHeader.AH_OSTaxAmount);
		}

		public void TestDebitCredit()
		{
			ExchangeDiff.AH_OSTotal = -70m;
			AssertEquals("Debit should be 0", 0m, ExchangeDiff.Debit);
			AssertEquals("Credit should be 70", 70m, ExchangeDiff.Credit);

			ExchangeDiff.AH_OSTotal = 70m;
			AssertEquals("Debit should be 70", 70m, ExchangeDiff.Debit);
			AssertEquals("Credit should be 0", 0m, ExchangeDiff.Credit);
		}

		protected override void AssertReversedAmountsCorrectlyNegated(TransactionHeader reversingHeader)
		{
			AssertEquals("Should be 0", 0m, ExchangeDiff.AH_OutstandingAmount);
		}

		public override void TestLocalCredit()
		{
			ExchangeDiff.AH_OSTotal = -40m;
			ExchangeDiff.AH_ExchangeRate = 2m;
			AssertEquals("LocalCredit should be 0", 0m, ExchangeDiff.LocalCredit);

			ExchangeDiff.AH_InvoiceAmount = -40m;
			ExchangeDiff.AH_ExchangeRate = 2m;
			AssertEquals("LocalCredit should be 40", 40m, ExchangeDiff.LocalCredit);
		}

		public override void TestLocalDebit()
		{
			ExchangeDiff.AH_OSTotal = 30m;
			ExchangeDiff.AH_ExchangeRate = 3m;
			AssertEquals("LocalDebit should be 0", 0m, ExchangeDiff.LocalDebit);

			ExchangeDiff.AH_InvoiceAmount = 30m;
			ExchangeDiff.AH_ExchangeRate = 2m;
			AssertEquals("LocalDebit should be 30", 30m, ExchangeDiff.LocalDebit);
		}

		public override void TestExchangeRateTypeDependsOnLedgerAndType()
		{
			AssertEquals("Exchange Rate type should be Buy", ExchangeRateType.Buy, ExchangeDiff.RateType);
		}

		public void TestBankAccountFiltering()
		{
			AccBankAccount cNYBank = Factory.NewWithValidTestData<AccBankAccount>();
			cNYBank.AB_RX_NKAccountCurrency = "CNY";

			AccBankAccount uSDBank = Factory.NewWithValidTestData<AccBankAccount>();
			uSDBank.AB_RX_NKAccountCurrency = "USD";
			Factory.Save();

			AccBankAccount aUDBank = Factory.NewWithValidTestData<AccBankAccount>();
			aUDBank.AB_RX_NKAccountCurrency = "AUD";
			Factory.Save();

			ZString localCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";
			try
			{
				CashbookExchangeDiff exDiff = Factory.NewWithValidTestData<CashbookExchangeDiff>();
				exDiff.BankAccounts.Load();
				AssertEquals("Should be 2 accounts", 2, exDiff.BankAccounts.Count);
				Assert("Should contain CNYBank", exDiff.BankAccounts.Contains(cNYBank.PK));
				Assert("Should contain USDBank", exDiff.BankAccounts.Contains(uSDBank.PK));
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = localCurrency;
			}
		}

		public void TestBankAccounts_ContainsOnlyActiveBanks()
		{
			ZQuery currencyFilter = new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			RefCurrency foreignCurrency = Factory.LoadTop1<RefCurrency>(currencyFilter);

			AccBankAccount activeBank = Factory.NewWithValidTestData<AccBankAccount>();
			activeBank.AB_RX_NKAccountCurrency = foreignCurrency.RX_Code;

			AccBankAccount inactiveBank = Factory.NewWithValidTestData<AccBankAccount>();
			inactiveBank.AB_RX_NKAccountCurrency = foreignCurrency.RX_Code;
			inactiveBank.AB_IsActive = false;

			CashbookExchangeDiff exDiff = Factory.New<CashbookExchangeDiff>();
			exDiff.BankAccounts.Load();
			AssertEquals("Should contain active bank", true, exDiff.BankAccounts.Contains(activeBank));
			AssertEquals("Should not contain inactive bank", false, exDiff.BankAccounts.Contains(inactiveBank));
		}

		public void TestSettingBankSetsCurrency()
		{
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_RX_NKAccountCurrency = currency.RX_Code;
			Factory.Save();

			ExchangeDiff.AH_AB = bank.PK;
			AssertEquals("Currency should be the test currency", currency.RX_Code, ExchangeDiff.AH_RX_NKTransactionCurrency);
		}

		public void TestSettingBankDefaultsExchangeRate()
		{
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_RX_NKAccountCurrency = "JPY";
			RefExchangeRate exRate = Factory.NewWithValidTestData<RefExchangeRate>();
			exRate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			exRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			exRate.RE_SellRate = 1.234m;
			exRate.RE_RX_NKExCurrency = "JPY";
			exRate.RE_GC = GlbCompany.CurrentCompany.PK;
			exRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.BuyRate;
			Factory.Save();

			ExchangeDiff.AH_AB = bank.PK;
			AssertEquals("Exchange rate should be 1.234", 1.234m, ExchangeDiff.AH_ExchangeRate);
		}

		[TestDate(2000, 1, 21)]
		public void TestSavedRecord()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_RX_NKAccountCurrency = "JPY";
			RefExchangeRate exRate = Factory.NewWithValidTestData<RefExchangeRate>();
			exRate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			exRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			exRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.BuyRate;
			exRate.RE_SellRate = 1.234m;
			exRate.RE_RX_NKExCurrency = "JPY";
			exRate.RE_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			Header = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			Header.AH_InvoiceDate = new ZDateTime(2000, 1, 20);
			Header.AH_PostDate = new ZDateTime(2000, 1, 21);
			Header.AH_AB = testBank.PK;
			Header.AH_Desc = "test bank currency adjustment";
			Header.AH_ExchangeRate = 1.5m;

			ZDecimal exGainLoss = ((CashbookExchangeDiff)Header).ForeignCurrencyGainLoss;
			Factory.Save();

			CashbookExchangeDiff savedExDiff = Factory.Load<CashbookExchangeDiff>(Header.PK);
			CheckSavedValues(savedExDiff, false, exGainLoss, Header.PK, testBank);

			ZQuery filter = new ZQuery(AccTransactionLinesSchema.AL_AH, savedExDiff.PK);
			int result = Factory.GetDatabaseCount(typeof(AccTransactionLines), filter);
			AssertEquals("Line record should NOT be created", 0, result);

			//Check Reversed Record
			savedExDiff.GenerateReverseTransaction(true);
			Factory.Save();

			CashbookExchangeDiff reversedExDiff = Factory.Load<CashbookExchangeDiff>(savedExDiff.FReverseTransaction_ForTestOnly.PK);
			CheckSavedValues(reversedExDiff, true, exGainLoss, ZGuid.Empty, testBank);
		}

		public void TestShouldSetAH_AG()
		{
			var newHeader = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			AssertEquals(true, newHeader.ShouldSetAH_AG);

			Factory.Save();
			AssertEquals(false, newHeader.ShouldSetAH_AG);

			var cancelledHeader = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			cancelledHeader.IsCancelled = true;
			AssertEquals(false, cancelledHeader.ShouldSetAH_AG);

			var headerREA = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			headerREA.AH_TransactionCategory = TransactionCategory.Codes.RealizedExchangeGainLoss;
			AssertEquals(false, cancelledHeader.ShouldSetAH_AG);
		}

		public void TestDocManagerCode()
		{
			AssertEquals("Wrong DocManagerCode. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "BCA", ((IDocManagerSupport)Factory.New<CashbookExchangeDiff>()).DocManagerInfo.DocManagerCode);
		}

		[TestDate(2020, 2, 6)]
		public void TestGetNewExchangeRate()
		{
			var periodManagements = Factory.Load<AccPeriodManagement>(new ZQuery());
			periodManagements.ForEach((x) => { x.Delete(); });
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			var bank = TestObjectCreator.USDBankAccount;
			var cashbookExchangeDiff = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			cashbookExchangeDiff.AH_AB = bank.PK;

			AssertEquals("Pre-condition: registry setting is PER (default)", Constants.ExchangeRateTypes.Code.PeriodEndRate, AccountingConfigurationRegistry.Instance.BankCurrencyAdjustmentExchangeRateType.Value);
			AssertEquals("pre-condition", 0m, cashbookExchangeDiff.GetNewExchangeRate());

			cashbookExchangeDiff.AH_PostDate = ZDateTime.Now;
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, 4.8m, new ZDateTime(2020, 2, 1), new ZDateTime(2020, 2, 29));
			AssertEquals("Use 'BUY' rate when registry setting is 'PER' and 'PER' rate does not exist.", 4.8m, cashbookExchangeDiff.GetNewExchangeRate());

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.PeriodEndRate, 4.7m, new ZDateTime(2020, 2, 29), new ZDateTime(2020, 2, 29));
			AssertEquals("Use 'PER' rate when registry setting is 'PER' and it is has the February PER rate.", 4.7m, cashbookExchangeDiff.GetNewExchangeRate());

			cashbookExchangeDiff.AH_PostDate = ZDateTime.Now.AddMonths(1);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.PeriodEndRate, 4.6m, new ZDateTime(2020, 3, 31), new ZDateTime(2020, 3, 31));
			AssertEquals("Use 'PER' rate when registry setting is 'PER' and it is has the March PER rate.", 4.6m, cashbookExchangeDiff.GetNewExchangeRate());

			cashbookExchangeDiff.AH_PostDate = ZDateTime.Now.AddMonths(1);
			var exchangeRateTypeCodeList = new ExchangeRateTypeListProvider().CodeDescriptionPairList.GetAllCodes();
			for (int i = 0; i < exchangeRateTypeCodeList.Length; i++)
			{
				var rateType = exchangeRateTypeCodeList[i];
				if (rateType == Constants.ExchangeRateTypes.Code.PeriodEndRate)
				{
					continue;
				}
				var rate = 4.8m + 0.1m * i;
				TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, rateType, rate, new ZDateTime(2020, 3, 1), new ZDateTime(2020, 3, 31));
				using (AccountingConfigurationRegistry.Instance.BankCurrencyAdjustmentExchangeRateType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rateType))
				{
					AssertEquals($"Use '{rateType}' rate when registry setting is '{rateType}'.", rate, cashbookExchangeDiff.GetNewExchangeRate());
				}
			}
		}

		public void TestIsRealizedExchangeGainLoss()
		{
			var cashbookExchangeDiff = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			cashbookExchangeDiff.AH_TransactionCategory = "";
			AssertEquals(false, cashbookExchangeDiff.IsRealizedExchangeGainLoss);

			cashbookExchangeDiff.AH_TransactionCategory = Constants.TransactionCategory.Codes.RealizedExchangeGainLoss;
			AssertEquals(true, cashbookExchangeDiff.IsRealizedExchangeGainLoss);
		}

		public void TestIsSavedByFactory()
		{
			AssertNull(ExchangeDiff.BankTransferParent);
			AssertEquals("When has no bank transfer parent", true, ExchangeDiff.IsSavedByFactory);

			var bankTransfer = TestObjectCreator.CreateBankTransfer(ZDateTime.Today, TestObjectCreator.AUDBankAccount2.PK, TestObjectCreator.EURBankAccount.PK, 1000m, 0.5m);
			ExchangeDiff.BankTransferParent = bankTransfer;

			ExchangeDiff.BankTransferParent.ShouldCalculateExchangeVariance = false;
			AssertEquals(false, ExchangeDiff.IsSavedByFactory);

			ExchangeDiff.BankTransferParent.ShouldCalculateExchangeVariance = true;
			ExchangeDiff.BankTransferParent.LocalBuyAmount += 100m;
			AssertEquals(true, ExchangeDiff.IsSavedByFactory);
		}

		#region Implementation

		CashbookExchangeDiff ExchangeDiff => CachedBusinessObject as CashbookExchangeDiff;

		protected override ZDecimal GetExpectedOutstandindAmount(ZDecimal expectedAmount) => 0;

		protected override ZDecimal GetExpectedOutstandindAmountForReversing(ZDecimal expectedAmount) => 0;

		void CheckSavedValues(CashbookExchangeDiff savedExDiff, bool isReveresed, ZDecimal exGainLoss, ZGuid exDiffGuid, AccBankAccount testBank)
		{
			if (isReveresed)
			{
				AssertEquals(new ZDateTime(2000, 1, 21), savedExDiff.AH_DueDate);
				AssertEquals(new ZDateTime(2000, 1, 21), savedExDiff.AH_InvoiceDate);
				AssertEquals(-exGainLoss, savedExDiff.AH_InvoiceAmount);
				AssertEquals(exDiffGuid, savedExDiff.AH_TransactionBelongsToGroup);
			}
			else
			{
				AssertEquals("test bank currency adjustment", savedExDiff.AH_Desc);
				AssertEquals(true, savedExDiff.AH_DueDate.IsEmpty);
				AssertEquals(exGainLoss, savedExDiff.AH_InvoiceAmount);
				AssertEquals(true, savedExDiff.AH_TransactionBelongsToGroup.IsEmpty);
				AssertEquals(new ZDateTime(2000, 1, 20), savedExDiff.AH_InvoiceDate);
			}

			AssertEquals(ZArchitecture.Core.LedgerTypes.CashBook, savedExDiff.AH_Ledger);
			AssertEquals(ZArchitecture.Core.TransactionTypes.ExchangeDifference, savedExDiff.AH_TransactionType);
			AssertEquals(1, (int)savedExDiff.AH_TransactionCount);
			AssertEquals("", savedExDiff.AH_TransactionReference);

			AssertEquals("UNR", savedExDiff.AH_TransactionCategory);
			AssertEquals(0m, savedExDiff.AH_GSTAmount);
			AssertEquals(0m, savedExDiff.AH_WithholdingTax);
			AssertEquals(0m, savedExDiff.AH_OSTotal);
			AssertEquals(testBank.AB_RX_NKAccountCurrency, savedExDiff.AH_RX_NKTransactionCurrency);
			AssertEquals(1.5m, savedExDiff.AH_ExchangeRate);
			AssertEquals(0, savedExDiff.AH_AgePeriod);
			AssertEquals(0, savedExDiff.AH_PostPeriod);
			AssertEquals(new ZDateTime(2000, 1, 21), savedExDiff.AH_PostDate);
			AssertEquals(false, savedExDiff.AH_IsDisbursementCalc);
			AssertEquals("", savedExDiff.AH_ChequeOrReference);
			AssertEquals("", savedExDiff.AH_ReceiptType);
			AssertEquals(false, savedExDiff.AH_CashBasisGSTIndicator);
			AssertEquals(false, savedExDiff.AH_CashBasisGSTRealisedToGL);
			AssertEquals("", savedExDiff.AH_ChequeDrawer);
			AssertEquals("", savedExDiff.AH_DrawerBank);
			AssertEquals("", savedExDiff.AH_DrawerBranch);
			AssertEquals(false, savedExDiff.AH_InvoiceApproved);
			AssertEquals("", savedExDiff.AH_ConsolidatedInvoiceRef);
			AssertEquals(true, savedExDiff.AH_FullyPaidDate.IsEmpty);
			AssertEquals(false, savedExDiff.AH_InvoicePrinted);
			AssertEquals(false, savedExDiff.AH_IsCancelled);
			//AssertEquals(false, SavedExDiff.AH_IsClearedInCashbook);
			AssertEquals(ZDateTime.Empty, savedExDiff.AH_DateClearedInCashbook);
			AssertEquals(false, savedExDiff.AH_NotAllocated);
			AssertEquals(0m, savedExDiff.AH_OutstandingAmount);
			AssertEquals(false, savedExDiff.AH_PostedToEFT);
			AssertEquals("N", savedExDiff.AH_PostToGL);
			AssertEquals("", savedExDiff.AH_ReceiptBatchNo);
			AssertEquals(false, savedExDiff.AH_TransactionCreatedByMatching);
			AssertEquals("", savedExDiff.AH_InvoiceTerm);
			AssertEquals(0, (int)savedExDiff.AH_InvoiceTermDays);
			AssertEquals(false, savedExDiff.AH_POST1);
			AssertEquals(false, savedExDiff.AH_POST2);
			AssertEquals(false, savedExDiff.AH_POST3);
			AssertEquals(false, savedExDiff.AH_POST4);
			AssertEquals(testBank.PK, savedExDiff.AH_AB);
			AssertEquals(true, savedExDiff.AH_OH.IsEmpty);
			AssertEquals(true, savedExDiff.AH_JH.IsEmpty);
			AssertEquals(GlbBranch.CurrentBranch.PK, savedExDiff.AH_GB);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, savedExDiff.AH_GE);
			AssertEquals(AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.Value, savedExDiff.AH_AG);
			AssertEquals(true, savedExDiff.AH_AH_InvoiceStatement.IsEmpty);
		}

		BusinessObjectFactory ReadOnlyFactory
		{
			get
			{
				if (fReadOnlyFactory == null)
				{
					fReadOnlyFactory = new BusinessObjectFactory();
				}
				return fReadOnlyFactory;
			}
		}
		BusinessObjectFactory fReadOnlyFactory;

		#endregion

		protected override Type TypeOfValidation
		{
			get { return typeof(CashbookExchangeDiffValidation); }
		}
	}
}
