using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using QuoteStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;

namespace Enterprise.Accounting.Business.Testing
{
	public class EPaymentQuoteAcceptorTest : TestCaseWithFactory
	{
		public void TestQuoteAccepted_PaymentMatchingBaseCreated_ExchangeDifferenceBizOCreated()
		{
			AssertQuoteAcceptingWhenPaymentMatchingBaseIsCreated(true);
		}

		public void TestQuoteAccepted_PaymentMatchingBaseCreated_ExchangeDifferenceBizONotCreated()
		{
			AssertQuoteAcceptingWhenPaymentMatchingBaseIsCreated(false);
		}

		void AssertQuoteAcceptingWhenPaymentMatchingBaseIsCreated(bool isExchangeDifferenceBizOCreated)
		{
			//payment approval -> 1000 USD / 1.4 ex rate = 714.29 AUD
			//invoice -> 1000 USD / 1.6 ex rate = 625 AUD
			//exx journal -> 625 AUD - 714.29 AUD = -89.29 AUD
			var paymentApproval = CreatePaymentApprovalForTest();
			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV123", TestObjectCreator.USD, 1.6m, 1000m, 0m, 625m, 0m, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK);
			Factory.Save();

			var paymentApprovalItem = Factory.New<PaymentApprovalItem>();
			paymentApprovalItem.A2_AH = apInvoice.PK;
			paymentApprovalItem.A2_AV = paymentApproval.PK;
			paymentApprovalItem.A2_PaymentThisRun = -apInvoice.AH_LocalExTaxAmount;
			paymentApproval.AV_ExchangeDifference = -89.29m; //for payment approvals, we do not create database record for exx journal, we store the amount on AV_ExchangeDifference column.
			Factory.Save();

			//quote -> 1000 USD / 1.2 ex rate = 833.33 AUD
			var quoteToAccept = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, paymentApproval);
			quoteToAccept.QU_FromAmount = 833.33m;
			quoteToAccept.QU_ToAmount = 1000m;
			quoteToAccept.QU_ExchangeRate = 1.2m;
			quoteToAccept.QU_ExchangeRateInverted = 0.8333m;
			Factory.Save();

			AssertEquals(QuoteStatusCodes.Received, quoteToAccept.QU_Status);
			Assert(!quoteToAccept.QU_ProviderReference.IsEmpty);
			AssertEquals(quoteToAccept.QU_RX_NKFromCurrency, Env.CurrentCompany.LocalCurrency.Code);
			paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems.Reload(true);
			AssertEquals(1, paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems.Count);
			AssertEquals(apInvoice.PK, paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems[0].A2_AH);
			AssertEquals(-89.29m, paymentApproval.AV_ExchangeDifference);

			var newFactory = new BusinessObjectFactory();
			var quoteInNewFactory = newFactory.LoadTop1<EPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quoteToAccept.PK));
			var paymentApprovalInNewFactory = newFactory.LoadTop1<PaymentApprovalBase>(new ZQuery(AccPaymentApprovalSchema.PK, paymentApproval.PK));
			var matchingBase = paymentApprovalInNewFactory.PaymentMatchingBaseObject;
			AssertNotNull("PaymentMatchingBaseObject should be created", matchingBase);
			if (isExchangeDifferenceBizOCreated)
			{
				matchingBase.CreateTemporaryTransactions();
				AssertNotNull("ExchangeDifferenceBizO should be created", matchingBase.ExchangeDiffCurrent);
			}
			else
			{
				AssertNull("ExchangeDifferenceBizO should not be created", matchingBase.ExchangeDiffCurrent);
			}
			var (isQuoteAccepted, userMessage) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(quoteInNewFactory, paymentApprovalInNewFactory);

			AssertEquals(true, isQuoteAccepted);
			AssertEquals($"Quote {quoteInNewFactory.QU_InternalReference} has been accepted.", userMessage);
			AssertEquals(QuoteStatusCodes.Accepted, quoteInNewFactory.QU_Status);

			//new payment approval values -> 1000 USD / 1.2 ex rate = 833.33 AUD
			//invoice -> 1000 USD / 1.6 ex rate = 625 AUD
			//new exx journal -> 625 AUD - 833.33 AUD = -208.33 AUD
			AssertEquals(833.33m, paymentApprovalInNewFactory.AV_Calc_LocalAmount);
			AssertEquals(1000m, paymentApprovalInNewFactory.AV_Amount);
			AssertEquals(1.2000m, paymentApprovalInNewFactory.AV_PayExRate.Round(4));
			AssertEquals(-208.33m, paymentApprovalInNewFactory.AV_ExchangeDifference);
		}

		public void TestQuoteAccepted_QuoteExchangeRateIsSameAsPaymentExchangeRate_ExisitingEXXJournalIsNotUpdated()
		{
			//payment approval -> 1000 USD / 1.4 ex rate = 714.29 AUD
			//invoice -> 1000 USD / 1.6 ex rate = 625 AUD
			//exx journal -> 625 AUD - 714.29 AUD = -89.29 AUD
			var paymentApproval = CreatePaymentApprovalForTest();
			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV123", TestObjectCreator.USD, 1.6m, 1000m, 0m, 625m, 0m, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK);
			Factory.Save();

			var paymentApprovalItem = Factory.New<PaymentApprovalItem>();
			paymentApprovalItem.A2_AH = apInvoice.PK;
			paymentApprovalItem.A2_AV = paymentApproval.PK;
			paymentApprovalItem.A2_PaymentThisRun = -apInvoice.AH_LocalExTaxAmount;
			paymentApproval.AV_ExchangeDifference = -89.29m; //for payment approvals, we do not create database record for exx journal, we store the amount on AV_ExchangeDifference column.
			Factory.Save();

			//quote -> 1000 USD / 1.4 ex rate = 714.29 AUD
			var quoteToAccept = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, paymentApproval);
			quoteToAccept.QU_FromAmount = 714.29m;
			quoteToAccept.QU_ToAmount = 1000m;
			quoteToAccept.QU_ExchangeRate = 1.4m;
			quoteToAccept.QU_ExchangeRateInverted = 0.7142m;
			Factory.Save();

			AssertEquals(QuoteStatusCodes.Received, quoteToAccept.QU_Status);
			Assert(!quoteToAccept.QU_ProviderReference.IsEmpty);
			AssertEquals(quoteToAccept.QU_RX_NKFromCurrency, Env.CurrentCompany.LocalCurrency.Code);
			paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems.Reload(true);
			AssertEquals(1, paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems.Count);
			AssertEquals(apInvoice.PK, paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems[0].A2_AH);
			AssertEquals(-89.29m, paymentApproval.AV_ExchangeDifference);

			var newFactory = new BusinessObjectFactory();
			var quoteInNewFactory = newFactory.LoadTop1<EPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quoteToAccept.PK));
			var paymentApprovalInNewFactory = newFactory.LoadTop1<PaymentApprovalBase>(new ZQuery(AccPaymentApprovalSchema.PK, paymentApproval.PK));
			var (isQuoteAccepted, userMessage) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(quoteInNewFactory, paymentApprovalInNewFactory);

			AssertEquals(true, isQuoteAccepted);
			AssertEquals($"Quote {quoteInNewFactory.QU_InternalReference} has been accepted.", userMessage);
			AssertEquals(QuoteStatusCodes.Accepted, quoteInNewFactory.QU_Status);

			//new payment approval values -> 1000 USD / 1.4 ex rate = 714.29 AUD (same as old payment approval values)
			//invoice -> 1000 USD / 1.6 ex rate = 625 AUD
			//new exx journal -> 625 AUD - 714.29 AUD = -89.29 AUD (same as old exx journal)
			AssertEquals(714.29m, paymentApprovalInNewFactory.AV_Calc_LocalAmount);
			AssertEquals(1000m, paymentApprovalInNewFactory.AV_Amount);
			AssertEquals(1.4000m, paymentApprovalInNewFactory.AV_PayExRate.Round(4));
			AssertEquals(-89.29m, paymentApprovalInNewFactory.AV_ExchangeDifference);
		}

		public void TestQuoteAcceptedForAlreadyAcceptedDeal()
		{
			var paymentApproval = CreatePaymentApprovalForTest();
			var quoteToAccept = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, paymentApproval);
			quoteToAccept.QU_RX_NKFromCurrency = Core.Constants.CurrencyCodes.NewZealand;

			AssertEquals(QuoteStatusCodes.Received, quoteToAccept.QU_Status);
			Assert(!quoteToAccept.QU_ProviderReference.IsEmpty);
			var message = EPaymentQuoteAcceptor.AcceptEPaymentQuoteForAcceptedDeal(quoteToAccept, paymentApproval);

			AssertEquals("Funding Currency does not match quote currency.", message);
			AssertEquals(QuoteStatusCodes.Accepted, quoteToAccept.QU_Status);
		}

		public void TestQuoteNotAccepted_WhenQuoteIsNotInReceivedStatus()
		{
			var paymentApproval = CreatePaymentApprovalForTest();
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Queued, paymentApproval);
			Factory.Save();

			AssertNotEquals(QuoteStatusCodes.Received, quote.QU_Status);

			var (isQuoteAccepted, userMessage) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(quote, paymentApproval);
			AssertEquals(false, isQuoteAccepted);
			AssertEquals("This quote is not in Received status.", userMessage);
			AssertNotEquals(QuoteStatusCodes.Accepted, quote.QU_Status);
		}

		public void TestQuoteNotAccepted_WhenQuoteProviderReferenceIsEmpty()
		{
			var paymentApproval = CreatePaymentApprovalForTest();
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, paymentApproval);
			quote.QU_ProviderReference = ZString.Empty;
			Factory.Save();

			AssertEquals(QuoteStatusCodes.Received, quote.QU_Status);
			AssertEquals(ZString.Empty, quote.QU_ProviderReference);

			var (isQuoteAccepted, userMessage) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(quote, paymentApproval);
			AssertEquals(false, isQuoteAccepted);
			AssertEquals("This is indicative rate only. To request a formal quote, please select an OFX E-Payment Account as the Bank Account for this payment, and ensure you have authorized your OFX User Account.", userMessage);
			AssertNotEquals(QuoteStatusCodes.Accepted, quote.QU_Status);
		}

		public void TestQuoteNotAccepted_WhenQuoteFromCurrencyIsDifferentToLoginCompanyLocalCurrency()
		{
			var paymentApproval = CreatePaymentApprovalForTest();
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, paymentApproval);
			quote.QU_RX_NKFromCurrency = Core.Constants.CurrencyCodes.NewZealand;
			Factory.Save();

			AssertEquals(QuoteStatusCodes.Received, quote.QU_Status);
			Assert(!quote.QU_ProviderReference.IsEmpty);
			AssertNotEquals(quote.QU_RX_NKFromCurrency, Env.CurrentCompany.LocalCurrency.Code);

			var (isQuoteAccepted, userMessage) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(quote, paymentApproval);
			AssertEquals(false, isQuoteAccepted);
			AssertEquals("This quote is no longer valid. Funding Currency of the payment does not match quote currency.", userMessage);
			AssertNotEquals(QuoteStatusCodes.Accepted, quote.QU_Status);
		}

		public void TestQuoteAccepted_PaymentHasNoMatchingTransactions()
		{
			var paymentApproval = CreatePaymentApprovalForTest();
			var quoteToAccept = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, paymentApproval);
			var quoteToDiscard = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Requested, paymentApproval);
			Factory.Save();

			AssertEquals(QuoteStatusCodes.Received, quoteToAccept.QU_Status);
			Assert(!quoteToAccept.QU_ProviderReference.IsEmpty);
			AssertEquals(quoteToAccept.QU_RX_NKFromCurrency, Env.CurrentCompany.LocalCurrency.Code);
			AssertEquals(0, paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems.Count);

			var newFactory = new BusinessObjectFactory();
			var quoteInNewFactory = newFactory.LoadTop1<EPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quoteToAccept.PK));
			var paymentApprovalInNewFactory = newFactory.LoadTop1<PaymentApprovalBase>(new ZQuery(AccPaymentApprovalSchema.PK, paymentApproval.PK));
			var (isQuoteAccepted, userMessage) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(quoteInNewFactory, paymentApprovalInNewFactory);

			AssertEquals(true, isQuoteAccepted);
			AssertEquals($"Quote {quoteInNewFactory.QU_InternalReference} has been accepted.", userMessage);
			AssertEquals(QuoteStatusCodes.Accepted, quoteInNewFactory.QU_Status);
			var quoteToDiscardInNewFactory = newFactory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quoteToDiscard.PK));
			AssertEquals(QuoteStatusCodes.Discarded, quoteToDiscardInNewFactory.QU_Status);
		}

		public void TestQuoteAccepted_PaymentHasMatchingTransactions_MatchingBalanceIsZero()
		{
			var paymentApproval = CreatePaymentApprovalForTest();
			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV123", TestObjectCreator.USD, 1.4m, 1000m, 0m, 714.29m, 0m, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK);
			Factory.Save();

			var paymentApprovalItem = Factory.New<PaymentApprovalItem>();
			paymentApprovalItem.A2_AH = apInvoice.PK;
			paymentApprovalItem.A2_AV = paymentApproval.PK;
			paymentApprovalItem.A2_PaymentThisRun = -apInvoice.AH_LocalExTaxAmount;
			Factory.Save();

			AssertEquals(0m, paymentApproval.AV_ExchangeDifference); //for payment approvals, we do not create database record for exx journal, we store the amount on AV_ExchangeDifference column.

			var quoteToAccept = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, paymentApproval);
			quoteToAccept.QU_FromAmount = 714.29m;
			quoteToAccept.QU_ToAmount = 1000m;
			quoteToAccept.QU_ExchangeRate = 1.4m;
			quoteToAccept.QU_ExchangeRateInverted = 0.71m;
			var quoteToDiscard = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Requested, paymentApproval);
			Factory.Save();

			AssertEquals(QuoteStatusCodes.Received, quoteToAccept.QU_Status);
			Assert(!quoteToAccept.QU_ProviderReference.IsEmpty);
			AssertEquals(quoteToAccept.QU_RX_NKFromCurrency, Env.CurrentCompany.LocalCurrency.Code);
			paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems.Reload(true);
			AssertEquals(1, paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems.Count);
			AssertEquals(apInvoice.PK, paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems[0].A2_AH);
			AssertEquals(0m, paymentApproval.AV_ExchangeDifference);
			var exxJournals = Factory.Load<ExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference));
			AssertEquals(0, exxJournals.Length);

			var newFactory = new BusinessObjectFactory();
			var quoteInNewFactory = newFactory.LoadTop1<EPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quoteToAccept.PK));
			var paymentApprovalInNewFactory = newFactory.LoadTop1<PaymentApprovalBase>(new ZQuery(AccPaymentApprovalSchema.PK, paymentApproval.PK));
			var (isQuoteAccepted, userMessage) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(quoteInNewFactory, paymentApprovalInNewFactory);

			AssertEquals(true, isQuoteAccepted);
			AssertEquals($"Quote {quoteInNewFactory.QU_InternalReference} has been accepted.", userMessage);
			AssertEquals(QuoteStatusCodes.Accepted, quoteInNewFactory.QU_Status);
			AssertEquals(714.29m, paymentApprovalInNewFactory.AV_Calc_LocalAmount);
			AssertEquals(1000m, paymentApprovalInNewFactory.AV_Amount);
			AssertEquals(1.4000m, paymentApprovalInNewFactory.AV_PayExRate.Round(4));
			AssertEquals(0m, paymentApprovalInNewFactory.AV_ExchangeDifference);
			var exxJournalsInNewFactory = newFactory.Load<ExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference));
			AssertEquals(0, exxJournalsInNewFactory.Length);
			var quoteToDiscardInNewFactory = newFactory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quoteToDiscard.PK));
			AssertEquals(QuoteStatusCodes.Discarded, quoteToDiscardInNewFactory.QU_Status);
		}

		public void TestQuoteAccepted_PaymentHasMatchingTransactions_MatchingBalanceIsPositive_ExisitingEXXJournal()
		{
			//payment approval -> 1000 USD / 1.4 ex rate = 714.29 AUD
			//invoice -> 1000 USD / 1.6 ex rate = 625 AUD
			//exx journal -> 625 AUD - 714.29 AUD = -89.29 AUD
			var paymentApproval = CreatePaymentApprovalForTest();
			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV123", TestObjectCreator.USD, 1.6m, 1000m, 0m, 625m, 0m, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK);
			Factory.Save();

			var paymentApprovalItem = Factory.New<PaymentApprovalItem>();
			paymentApprovalItem.A2_AH = apInvoice.PK;
			paymentApprovalItem.A2_AV = paymentApproval.PK;
			paymentApprovalItem.A2_PaymentThisRun = -apInvoice.AH_LocalExTaxAmount;
			paymentApproval.AV_ExchangeDifference = -89.29m; //for payment approvals, we do not create database record for exx journal, we store the amount on AV_ExchangeDifference column.
			Factory.Save();

			//quote -> 1000 USD / 1.2 ex rate = 833.33 AUD
			var quoteToAccept = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, paymentApproval);
			quoteToAccept.QU_FromAmount = 833.33m;
			quoteToAccept.QU_ToAmount = 1000m;
			quoteToAccept.QU_ExchangeRate = 1.2m;
			quoteToAccept.QU_ExchangeRateInverted = 0.8333m;
			var quoteToDiscard = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Requested, paymentApproval);
			Factory.Save();

			AssertEquals(QuoteStatusCodes.Received, quoteToAccept.QU_Status);
			Assert(!quoteToAccept.QU_ProviderReference.IsEmpty);
			AssertEquals(quoteToAccept.QU_RX_NKFromCurrency, Env.CurrentCompany.LocalCurrency.Code);
			paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems.Reload(true);
			AssertEquals(1, paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems.Count);
			AssertEquals(apInvoice.PK, paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems[0].A2_AH);
			AssertEquals(-89.29m, paymentApproval.AV_ExchangeDifference);
			var exxJournals = Factory.Load<ExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference));
			AssertEquals(0, exxJournals.Length);

			var newFactory = new BusinessObjectFactory();
			var quoteInNewFactory = newFactory.LoadTop1<EPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quoteToAccept.PK));
			var paymentApprovalInNewFactory = newFactory.LoadTop1<PaymentApprovalBase>(new ZQuery(AccPaymentApprovalSchema.PK, paymentApproval.PK));
			var (isQuoteAccepted, userMessage) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(quoteInNewFactory, paymentApprovalInNewFactory);

			AssertEquals(true, isQuoteAccepted);
			AssertEquals($"Quote {quoteInNewFactory.QU_InternalReference} has been accepted.", userMessage);
			AssertEquals(QuoteStatusCodes.Accepted, quoteInNewFactory.QU_Status);

			//new payment approval values -> 1000 USD / 1.2 ex rate = 833.33 AUD
			//invoice -> 1000 USD / 1.6 ex rate = 625 AUD
			//new exx journal -> 625 AUD - 833.33 AUD = -208.33 AUD
			AssertEquals(833.33m, paymentApprovalInNewFactory.AV_Calc_LocalAmount);
			AssertEquals(1000m, paymentApprovalInNewFactory.AV_Amount);
			AssertEquals(1.2000m, paymentApprovalInNewFactory.AV_PayExRate.Round(4));
			AssertEquals(-208.33m, paymentApprovalInNewFactory.AV_ExchangeDifference);
			var exxJournalsInNewFactory = newFactory.Load<ExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference));
			AssertEquals(0, exxJournalsInNewFactory.Length);
			var quoteToDiscardInNewFactory = newFactory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quoteToDiscard.PK));
			AssertEquals(QuoteStatusCodes.Discarded, quoteToDiscardInNewFactory.QU_Status);
		}

		public void TestQuoteAccepted_PaymentHasMatchingTransactions_MatchingBalanceIsNegative_ExisitingEXXJournal()
		{
			//payment approval -> 1000 USD / 1.4 ex rate = 714.29 AUD
			//invoice -> 1000 USD / 1.2 ex rate = 833.33 AUD
			//exx journal ->  833.33 AUD - 714.29 AUD = 119.04 AUD
			var paymentApproval = CreatePaymentApprovalForTest();
			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV123", TestObjectCreator.USD, 1.2m, 1000m, 0m, 833.33m, 0m, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK);
			Factory.Save();

			var paymentApprovalItem = Factory.New<PaymentApprovalItem>();
			paymentApprovalItem.A2_AH = apInvoice.PK;
			paymentApprovalItem.A2_AV = paymentApproval.PK;
			paymentApprovalItem.A2_PaymentThisRun = -apInvoice.AH_LocalExTaxAmount;
			paymentApproval.AV_ExchangeDifference = 119.04m; //for payment approvals, we do not create database record for exx journal, we store the amount on AV_ExchangeDifference column.
			Factory.Save();

			//quote -> 1000 USD / 2.5 ex rate = 400 AUD
			var quoteToAccept = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, paymentApproval);
			quoteToAccept.QU_FromAmount = 400m;
			quoteToAccept.QU_ToAmount = 1000m;
			quoteToAccept.QU_ExchangeRate = 2.5m;
			quoteToAccept.QU_ExchangeRateInverted = 0.4m;
			var quoteToDiscard = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Requested, paymentApproval);
			Factory.Save();

			AssertEquals(QuoteStatusCodes.Received, quoteToAccept.QU_Status);
			Assert(!quoteToAccept.QU_ProviderReference.IsEmpty);
			AssertEquals(quoteToAccept.QU_RX_NKFromCurrency, Env.CurrentCompany.LocalCurrency.Code);
			paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems.Reload(true);
			AssertEquals(1, paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems.Count);
			AssertEquals(apInvoice.PK, paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems[0].A2_AH);
			AssertEquals(119.04m, paymentApproval.AV_ExchangeDifference);
			var exxJournals = Factory.Load<ExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference));
			AssertEquals(0, exxJournals.Length);

			var newFactory = new BusinessObjectFactory();
			var quoteInNewFactory = newFactory.LoadTop1<EPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quoteToAccept.PK));
			var paymentApprovalInNewFactory = newFactory.LoadTop1<PaymentApprovalBase>(new ZQuery(AccPaymentApprovalSchema.PK, paymentApproval.PK));
			var (isQuoteAccepted, userMessage) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(quoteInNewFactory, paymentApprovalInNewFactory);

			AssertEquals(true, isQuoteAccepted);
			AssertEquals($"Quote {quoteInNewFactory.QU_InternalReference} has been accepted.", userMessage);
			AssertEquals(QuoteStatusCodes.Accepted, quoteInNewFactory.QU_Status);

			//new payment approval values -> 1000 USD / 2.5 ex rate = 400 AUD
			//invoice -> 1000 USD / 1.2 ex rate = 833.33 AUD
			//new exx journal -> 833.33 AUD - 400 AUD = 433.33 AUD
			AssertEquals(400m, paymentApprovalInNewFactory.AV_Calc_LocalAmount);
			AssertEquals(1000m, paymentApprovalInNewFactory.AV_Amount);
			AssertEquals(2.5000m, paymentApprovalInNewFactory.AV_PayExRate.Round(4));
			AssertEquals(433.33m, paymentApprovalInNewFactory.AV_ExchangeDifference);
			var exxJournalsInNewFactory = newFactory.Load<ExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference));
			AssertEquals(0, exxJournalsInNewFactory.Length);
			var quoteToDiscardInNewFactory = newFactory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quoteToDiscard.PK));
			AssertEquals(QuoteStatusCodes.Discarded, quoteToDiscardInNewFactory.QU_Status);
		}

		public void TestQuoteAccepted_PaymentHasMatchingTransactions_MatchingBalanceIsPositive_NoExisitingEXXJournal()
		{
			//payment approval -> 1000 USD / 1.4 ex rate = 714.29 AUD
			//invoice -> 1000 USD / 1.4 ex rate = 714.29 AUD
			var paymentApproval = CreatePaymentApprovalForTest();
			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV123", TestObjectCreator.USD, 1.4m, 1000m, 0m, 714.29m, 0m, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK);
			Factory.Save();

			var paymentApprovalItem = Factory.New<PaymentApprovalItem>();
			paymentApprovalItem.A2_AH = apInvoice.PK;
			paymentApprovalItem.A2_AV = paymentApproval.PK;
			paymentApprovalItem.A2_PaymentThisRun = -apInvoice.AH_LocalExTaxAmount;
			Factory.Save();

			//quote -> 1000 USD / 1.2 ex rate = 833.33 AUD
			var quoteToAccept = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, paymentApproval);
			quoteToAccept.QU_FromAmount = 833.33m;
			quoteToAccept.QU_ToAmount = 1000m;
			quoteToAccept.QU_ExchangeRate = 1.2m;
			quoteToAccept.QU_ExchangeRateInverted = 0.8333m;
			var quoteToDiscard = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Requested, paymentApproval);
			Factory.Save();

			AssertEquals(QuoteStatusCodes.Received, quoteToAccept.QU_Status);
			Assert(!quoteToAccept.QU_ProviderReference.IsEmpty);
			AssertEquals(quoteToAccept.QU_RX_NKFromCurrency, Env.CurrentCompany.LocalCurrency.Code);
			paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems.Reload(true);
			AssertEquals(1, paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems.Count);
			AssertEquals(apInvoice.PK, paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems[0].A2_AH);
			AssertEquals(0m, paymentApproval.AV_ExchangeDifference);
			var exxJournals = Factory.Load<ExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference));
			AssertEquals(0, exxJournals.Length);

			var newFactory = new BusinessObjectFactory();
			var quoteInNewFactory = newFactory.LoadTop1<EPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quoteToAccept.PK));
			var paymentApprovalInNewFactory = newFactory.LoadTop1<PaymentApprovalBase>(new ZQuery(AccPaymentApprovalSchema.PK, paymentApproval.PK));
			var (isQuoteAccepted, userMessage) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(quoteInNewFactory, paymentApprovalInNewFactory);

			AssertEquals(true, isQuoteAccepted);
			AssertEquals($"Quote {quoteInNewFactory.QU_InternalReference} has been accepted.", userMessage);
			AssertEquals(QuoteStatusCodes.Accepted, quoteInNewFactory.QU_Status);

			//new payment approval values -> 1000 USD / 1.2 ex rate = 833.33 AUD
			//invoice -> 1000 USD / 1.4 ex rate = 714.29 AUD
			//new exx journal -> 714.29 AUD - 833.33 AUD = -119.04 AUD
			AssertEquals(833.33m, paymentApprovalInNewFactory.AV_Calc_LocalAmount);
			AssertEquals(1000m, paymentApprovalInNewFactory.AV_Amount);
			AssertEquals(1.2000m, paymentApprovalInNewFactory.AV_PayExRate.Round(4));
			AssertEquals(-119.04m, paymentApprovalInNewFactory.AV_ExchangeDifference);
			var exxJournalsInNewFactory = newFactory.Load<ExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference));
			AssertEquals(0, exxJournalsInNewFactory.Length);
			var quoteToDiscardInNewFactory = newFactory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quoteToDiscard.PK));
			AssertEquals(QuoteStatusCodes.Discarded, quoteToDiscardInNewFactory.QU_Status);
		}

		public void TestQuoteAccepted_PaymentHasMatchingTransactions_MatchingBalanceIsNegative_NoExisitingEXXJournal()
		{
			//payment approval -> 1000 USD / 1.4 ex rate = 714.29 AUD
			//invoice -> 1000 USD / 1.4 ex rate = 714.29 AUD
			var paymentApproval = CreatePaymentApprovalForTest();
			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV123", TestObjectCreator.USD, 1.4m, 1000m, 0m, 714.29m, 0m, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK);
			Factory.Save();

			var paymentApprovalItem = Factory.New<PaymentApprovalItem>();
			paymentApprovalItem.A2_AH = apInvoice.PK;
			paymentApprovalItem.A2_AV = paymentApproval.PK;
			paymentApprovalItem.A2_PaymentThisRun = -apInvoice.AH_LocalExTaxAmount;
			Factory.Save();

			//quote -> 1000 USD / 2.5 ex rate = 400 AUD
			var quoteToAccept = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, paymentApproval);
			quoteToAccept.QU_FromAmount = 400m;
			quoteToAccept.QU_ToAmount = 1000m;
			quoteToAccept.QU_ExchangeRate = 2.5m;
			quoteToAccept.QU_ExchangeRateInverted = 0.4m;
			var quoteToDiscard = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Requested, paymentApproval);
			Factory.Save();

			AssertEquals(QuoteStatusCodes.Received, quoteToAccept.QU_Status);
			Assert(!quoteToAccept.QU_ProviderReference.IsEmpty);
			AssertEquals(quoteToAccept.QU_RX_NKFromCurrency, Env.CurrentCompany.LocalCurrency.Code);
			paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems.Reload(true);
			AssertEquals(1, paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems.Count);
			AssertEquals(apInvoice.PK, paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems[0].A2_AH);
			AssertEquals(0m, paymentApproval.AV_ExchangeDifference);
			var exxJournals = Factory.Load<ExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference));
			AssertEquals(0, exxJournals.Length);

			var newFactory = new BusinessObjectFactory();
			var quoteInNewFactory = newFactory.LoadTop1<EPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quoteToAccept.PK));
			var paymentApprovalInNewFactory = newFactory.LoadTop1<PaymentApprovalBase>(new ZQuery(AccPaymentApprovalSchema.PK, paymentApproval.PK));
			var (isQuoteAccepted, userMessage) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(quoteInNewFactory, paymentApprovalInNewFactory);

			AssertEquals(true, isQuoteAccepted);
			AssertEquals($"Quote {quoteInNewFactory.QU_InternalReference} has been accepted.", userMessage);
			AssertEquals(QuoteStatusCodes.Accepted, quoteInNewFactory.QU_Status);

			//new payment approval values -> 1000 USD / 2.5 ex rate = 400 AUD
			//invoice -> 1000 USD / 1.4 ex rate = 714.29 AUD
			//new exx journal -> 714.29 AUD - 400 AUD = 314.29 AUD
			AssertEquals(400m, paymentApprovalInNewFactory.AV_Calc_LocalAmount);
			AssertEquals(1000m, paymentApprovalInNewFactory.AV_Amount);
			AssertEquals(2.5000m, paymentApprovalInNewFactory.AV_PayExRate.Round(4));
			AssertEquals(314.29m, paymentApprovalInNewFactory.AV_ExchangeDifference);
			var exxJournalsInNewFactory = newFactory.Load<ExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference));
			AssertEquals(0, exxJournalsInNewFactory.Length);
			var quoteToDiscardInNewFactory = newFactory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quoteToDiscard.PK));
			AssertEquals(QuoteStatusCodes.Discarded, quoteToDiscardInNewFactory.QU_Status);
		}

		public void TestQuoteAccepted_WithFundingCurrencyFromPaymentBatch()
		{
			var payment = CreatePaymentFromPaymentBatch();

			var quote = payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			quote.QU_Status = QuoteStatusCodes.Received;
			quote.QU_ProviderReference = "testreference";
			var (isQuoteAccepted, userMessage) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(quote, payment, false);

			AssertEquals(true, isQuoteAccepted);
			AssertEquals($"Quote {quote.QU_InternalReference} has been accepted.", userMessage);
			AssertEquals(QuoteStatusCodes.Accepted, quote.QU_Status);
		}

		public void TestQuoteAccepted_WithFundingCurrencyFromPaymentBatch_CurrencyInconsistent()
		{
			var payment = CreatePaymentFromPaymentBatch();

			var quote = payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			quote.QU_Status = QuoteStatusCodes.Received;
			quote.QU_ProviderReference = "testreference";
			quote.QU_RX_NKFromCurrency = Core.Constants.CurrencyCodes.Australia;

			var (isQuoteAccepted, userMessage) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(quote, payment, false);

			AssertEquals(false, isQuoteAccepted);
			AssertEquals("This quote is no longer valid. Funding Currency of the payment does not match quote currency.", userMessage);
			AssertEquals(QuoteStatusCodes.Discarded, quote.QU_Status);
		}

		public void TestAcceptEPaymentQuoteForAcceptedDeal_WithFundingCurrencyFromPaymentBatch_CurrencyInconsistent()
		{
			var payment = CreatePaymentFromPaymentBatch();

			var quoteToAccept = payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			quoteToAccept.QU_Status = QuoteStatusCodes.Received;
			quoteToAccept.QU_ProviderReference = "testreference";
			quoteToAccept.QU_RX_NKFromCurrency = Core.Constants.CurrencyCodes.Australia;

			var message = EPaymentQuoteAcceptor.AcceptEPaymentQuoteForAcceptedDeal(quoteToAccept, payment);

			AssertEquals("Funding Currency does not match quote currency.", message);
			AssertEquals(QuoteStatusCodes.Accepted, quoteToAccept.QU_Status);
		}

		public void TestUpdatePaymentLocalAmountAfterAcceptQuote_WithFundingCurrencyFromPaymentBatch_CurrencyInconsistent()
		{
			var payment = CreatePaymentFromPaymentBatch();
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Amount = 10m;
			payment.AV_PayExRate = 0.1;

			var quote = payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			quote.QU_Status = QuoteStatusCodes.Received;
			quote.QU_ProviderReference = "testreference";
			AssertEquals(payment.AV_Amount, quote.QU_ToAmount);
			quote.QU_ToAmount = 5m;

			EPaymentQuoteAcceptor.AcceptEPaymentQuote(quote, payment, false);

			ZDecimal expectedLocalAmount = 5 / 0.1;
			AssertEquals(quote.QU_ToAmount, payment.AV_Amount);
			AssertEquals(quote.QU_RX_NKToCurrency, payment.AV_RX_NKPaymentCurrency);
			AssertEquals(0.1m, payment.AV_PayExRate);
			AssertEquals(expectedLocalAmount, payment.AV_Calc_LocalAmount);
		}

		public void TestUpdatePaymentLocalAmountAfterAcceptQuote_WithFundingCurrencyFromPaymentBatch_AmountInconsistent()
		{
			var payment = CreatePaymentFromPaymentBatch();
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Amount = 10m;
			payment.AV_PayExRate = 0.1;

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.EUR, ExchangeRateTypes.Code.BuyRate, 0.2m, ZDateTime.Today, ZDateTime.Today);

			var quote = payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			quote.QU_Status = QuoteStatusCodes.Received;
			quote.QU_ProviderReference = "testreference";
			AssertEquals(payment.AV_Amount, quote.QU_ToAmount);
			quote.QU_RX_NKToCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			EPaymentQuoteAcceptor.AcceptEPaymentQuote(quote, payment, false);

			ZDecimal expectedLocalAmount = 10 / 0.2;
			AssertEquals(quote.QU_ToAmount, payment.AV_Amount);
			AssertEquals(quote.QU_RX_NKToCurrency, payment.AV_RX_NKPaymentCurrency);
			AssertEquals(0.2m, payment.AV_PayExRate);
			AssertEquals(expectedLocalAmount, payment.AV_Calc_LocalAmount);
		}

		PaymentApprovalBase CreatePaymentFromPaymentBatch()
		{
			var fundingBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var accBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			fundingBankAccount.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.China;
			accBankAccount.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			Factory.Save();

			var aPInvoice = Factory.NewWithValidTestData<APInvoice>();
			var transactions = new TransactionHeaderCollection(Factory);
			transactions.Add(aPInvoice);

			var batchPoster = Factory.NewWithValidTestData<APPaymentBatchPoster>();
			batchPoster.SetDefaultValuesByTransactions(transactions);
			var payment = batchPoster.PaymentApprovalCollection[0];
			batchPoster.SetPaymentDetails(payment);
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			payment.AV_OH = orgHeader.PK;
			batchPoster.APB_AB = accBankAccount.PK;
			batchPoster.APB_AB_FundingBankAccount = fundingBankAccount.PK;
			Factory.Save();

			AssertEquals(true, batchPoster.IsInDatabase);
			AssertEquals(true, payment.IsInDatabase);
			AssertEquals("There should be 1 payment in the collection", 1, batchPoster.PaymentApprovalCollection.Count);
			AssertEquals("There should be 1 approval linked to AccPaymentBatch", 1, batchPoster.PaymentApprovalCollection.Count);

			return payment;
		}

		public void TestQuoteAccepted_WhenQuoteToCurrencyIsDifferentToPaymentCurrency()
		{
			//payment approval -> 1000 USD / 1.4 ex rate = 714.29 AUD
			//invoice -> 1000 USD / 1.4 ex rate = 714.29 AUD
			var paymentApproval = CreatePaymentApprovalForTest();
			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV123", TestObjectCreator.USD, 1.4m, 1000m, 0m, 714.29m, 0m, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK);
			Factory.Save();

			var paymentApprovalItem = Factory.New<PaymentApprovalItem>();
			paymentApprovalItem.A2_AH = apInvoice.PK;
			paymentApprovalItem.A2_AV = paymentApproval.PK;
			paymentApprovalItem.A2_PaymentThisRun = -apInvoice.AH_LocalExTaxAmount;
			Factory.Save();

			//quote -> 1000 USD / 2.5 ex rate = 400 AUD
			var quoteToAccept = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, paymentApproval);
			quoteToAccept.QU_FromAmount = 400m;
			quoteToAccept.QU_ToAmount = 1000m;
			quoteToAccept.QU_ExchangeRate = 2.5m;
			quoteToAccept.QU_ExchangeRateInverted = 0.4m;
			var quoteToDiscard = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Requested, paymentApproval);
			Factory.Save();

			AssertEquals(QuoteStatusCodes.Received, quoteToAccept.QU_Status);
			Assert(!quoteToAccept.QU_ProviderReference.IsEmpty);
			AssertEquals(quoteToAccept.QU_RX_NKFromCurrency, Env.CurrentCompany.LocalCurrency.Code);
			paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems.Reload(true);
			AssertEquals(1, paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems.Count);
			AssertEquals(apInvoice.PK, paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems[0].A2_AH);
			AssertEquals(0m, paymentApproval.AV_ExchangeDifference);
			var exxJournals = Factory.Load<ExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference));
			AssertEquals(0, exxJournals.Length);

			var newFactory = new BusinessObjectFactory();
			var quoteInNewFactory = newFactory.LoadTop1<EPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quoteToAccept.PK));
			var paymentApprovalInNewFactory = newFactory.LoadTop1<PaymentApprovalBase>(new ZQuery(AccPaymentApprovalSchema.PK, paymentApproval.PK));
			paymentApprovalInNewFactory.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.NewZealand; //user change currency before clicking accpet quote button
			var (isQuoteAccepted, userMessage) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(quoteInNewFactory, paymentApprovalInNewFactory);

			AssertEquals(true, isQuoteAccepted);
			AssertEquals($"Quote {quoteInNewFactory.QU_InternalReference} has been accepted.", userMessage);
			AssertEquals(QuoteStatusCodes.Accepted, quoteInNewFactory.QU_Status);

			//new payment approval values -> 1000 USD / 2.5 ex rate = 400 AUD
			//invoice -> 1000 USD / 1.4 ex rate = 714.29 AUD
			//new exx journal -> 714.29 AUD - 400 AUD = 314.19 AUD
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, paymentApprovalInNewFactory.AV_RX_NKPaymentCurrency);
			AssertEquals(400m, paymentApprovalInNewFactory.AV_Calc_LocalAmount);
			AssertEquals(1000m, paymentApprovalInNewFactory.AV_Amount);
			AssertEquals(2.5000m, paymentApprovalInNewFactory.AV_PayExRate.Round(4));
			AssertEquals(314.29m, paymentApprovalInNewFactory.AV_ExchangeDifference);
			var exxJournalsInNewFactory = newFactory.Load<ExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference));
			AssertEquals(0, exxJournalsInNewFactory.Length);
			var quoteToDiscardInNewFactory = newFactory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quoteToDiscard.PK));
			AssertEquals(QuoteStatusCodes.Discarded, quoteToDiscardInNewFactory.QU_Status);
		}

		#region When Funding Currency is Foreign Currency

		public void TestQuoteNotAccepted_WhenQuoteFromCurrencyIsDifferentToFundingCurrency_WhenFundingCurrencyEnabled()
		{
			var paymentApproval = CreatePaymentApprovalForTest();
			CreateBatchPosterWithForeignFundingCurrencyForPaymentApproval(paymentApproval);
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, paymentApproval);
			quote.QU_RX_NKFromCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			Factory.Save();

			AssertEquals(QuoteStatusCodes.Received, quote.QU_Status);
			Assert(!quote.QU_ProviderReference.IsEmpty);
			AssertNotEquals(quote.QU_RX_NKFromCurrency, Env.CurrentCompany.LocalCurrency.Code);

			var (isQuoteAccepted, userMessage) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(quote, paymentApproval);
			AssertEquals(false, isQuoteAccepted);
			AssertEquals("This quote is no longer valid. Funding Currency of the payment does not match quote currency.", userMessage);
			AssertNotEquals(QuoteStatusCodes.Accepted, quote.QU_Status);
		}

		public void TestQuoteAccepted_PaymentHasNoMatchingTransactions_WithForeignFundingCurrency()
		{
			var paymentApproval = CreatePaymentApprovalForTest();
			CreateBatchPosterWithForeignFundingCurrencyForPaymentApproval(paymentApproval);
			var quoteToAccept = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, paymentApproval);
			var quoteToDiscard = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Requested, paymentApproval);
			quoteToAccept.QU_RX_NKFromCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			quoteToDiscard.QU_RX_NKFromCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			Factory.Save();

			AssertEquals(QuoteStatusCodes.Received, quoteToAccept.QU_Status);
			Assert(!quoteToAccept.QU_ProviderReference.IsEmpty);
			AssertEquals(0, paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems.Count);
			AssertNotEquals(quoteToAccept.QU_RX_NKFromCurrency, Env.CurrentCompany.LocalCurrency.Code);

			var newFactory = new BusinessObjectFactory();
			var quoteToAcceptInNewFactory = newFactory.LoadTop1<EPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quoteToAccept.PK));
			var paymentApprovalInNewFactory = newFactory.LoadTop1<PaymentApprovalBase>(new ZQuery(AccPaymentApprovalSchema.PK, paymentApproval.PK));
			var (isQuoteAccepted, userMessage) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(quoteToAcceptInNewFactory, paymentApprovalInNewFactory);

			Assert(isQuoteAccepted);
			AssertEquals($"Quote {quoteToAcceptInNewFactory.QU_InternalReference} has been accepted.", userMessage);
			AssertEquals(QuoteStatusCodes.Accepted, quoteToAcceptInNewFactory.QU_Status);

			var quoteToDiscardInNewFactory = newFactory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quoteToDiscard.PK));
			AssertEquals(QuoteStatusCodes.Discarded, quoteToDiscardInNewFactory.QU_Status);
		}

		public void TestQuoteAccepted_PaymentHasMatchingTransactions_NoExisitingEXXJournal_WithForeignFundingCurrency()
		{
			//payment approval -> 1000 USD / 1.4 ex rate = 714.29 AUD
			//invoice -> 1000 USD / 1.4 ex rate = 714.29 AUD
			var paymentApproval = CreatePaymentApprovalForTest();
			CreateBatchPosterWithForeignFundingCurrencyForPaymentApproval(paymentApproval);
			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV123", TestObjectCreator.USD, 1.4m, 1000m, 0m, 714.29m, 0m, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK);
			Factory.Save();

			var paymentApprovalItem = Factory.New<PaymentApprovalItem>();
			paymentApprovalItem.A2_AH = apInvoice.PK;
			paymentApprovalItem.A2_AV = paymentApproval.PK;
			paymentApprovalItem.A2_PaymentThisRun = -apInvoice.AH_LocalExTaxAmount;
			Factory.Save();

			//quote -> 1000 USD / 1.1 ex rate = 909.09 EUR
			var quoteToAccept = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, paymentApproval);
			quoteToAccept.QU_FromAmount = 909.09m;
			quoteToAccept.QU_ToAmount = 1000m;
			quoteToAccept.QU_ExchangeRate = 1.1m;
			quoteToAccept.QU_ExchangeRateInverted = 0.9091m;
			quoteToAccept.QU_RX_NKFromCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			var quoteToDiscard = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Requested, paymentApproval);
			quoteToDiscard.QU_RX_NKFromCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			Factory.Save();

			AssertEquals(QuoteStatusCodes.Received, quoteToAccept.QU_Status);
			Assert(!quoteToAccept.QU_ProviderReference.IsEmpty);
			AssertNotEquals(quoteToAccept.QU_RX_NKFromCurrency, Env.CurrentCompany.LocalCurrency.Code);
			paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems.Reload(true);
			AssertEquals(1, paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems.Count);
			AssertEquals(apInvoice.PK, paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems[0].A2_AH);
			AssertEquals(0m, paymentApproval.AV_ExchangeDifference);
			var exxJournals = Factory.Load<ExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference));
			AssertEquals(0, exxJournals.Length);

			var newFactory = new BusinessObjectFactory();
			var quoteInNewFactory = newFactory.LoadTop1<EPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quoteToAccept.PK));
			var paymentApprovalInNewFactory = newFactory.LoadTop1<PaymentApprovalBase>(new ZQuery(AccPaymentApprovalSchema.PK, paymentApproval.PK));
			var (isQuoteAccepted, userMessage) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(quoteInNewFactory, paymentApprovalInNewFactory);

			AssertEquals(true, isQuoteAccepted);
			AssertEquals($"Quote {quoteInNewFactory.QU_InternalReference} has been accepted.", userMessage);
			AssertEquals(QuoteStatusCodes.Accepted, quoteInNewFactory.QU_Status);

			//local payment approval values -> 1000 USD / 1.4 ex rate = 714.29 AUD (not updated)
			//invoice -> 1000 USD / 1.4 ex rate = 714.29 AUD (not updated)
			//exx journal -> 0 AUD (not updated)
			AssertEquals(714.29m, paymentApprovalInNewFactory.AV_Calc_LocalAmount);
			AssertEquals(1000m, paymentApprovalInNewFactory.AV_Amount);
			AssertEquals(1.4m, paymentApprovalInNewFactory.AV_PayExRate.Round(4));
			AssertEquals(0m, paymentApprovalInNewFactory.AV_ExchangeDifference);
			var exxJournalsInNewFactory = newFactory.Load<ExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference));
			AssertEquals(0, exxJournalsInNewFactory.Length);
			var quoteToDiscardInNewFactory = newFactory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quoteToDiscard.PK));
			AssertEquals(QuoteStatusCodes.Discarded, quoteToDiscardInNewFactory.QU_Status);
		}

		public void TestQuoteAccepted_PaymentHasMatchingTransactions_ExisitingEXXJournal_WithForeignFundingCurrency()
		{
			//payment approval -> 1000 USD / 1.4 ex rate = 714.29 AUD
			//invoice -> 1000 USD / 1.6 ex rate = 625 AUD
			//exx journal -> 625 AUD - 714.29 AUD = -89.29 AUD
			var paymentApproval = CreatePaymentApprovalForTest();
			CreateBatchPosterWithForeignFundingCurrencyForPaymentApproval(paymentApproval);
			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV123", TestObjectCreator.USD, 1.6m, 1000m, 0m, 625m, 0m, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK);
			Factory.Save();

			var paymentApprovalItem = Factory.New<PaymentApprovalItem>();
			paymentApprovalItem.A2_AH = apInvoice.PK;
			paymentApprovalItem.A2_AV = paymentApproval.PK;
			paymentApprovalItem.A2_PaymentThisRun = -apInvoice.AH_LocalExTaxAmount;
			paymentApproval.AV_ExchangeDifference = -89.29m; //for payment approvals, we do not create database record for exx journal, we store the amount on AV_ExchangeDifference column.
			Factory.Save();

			//quote -> 1000 USD / 1.1 ex rate = 909.09 EUR
			var quoteToAccept = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, paymentApproval);
			quoteToAccept.QU_FromAmount = 909.09m;
			quoteToAccept.QU_ToAmount = 1000m;
			quoteToAccept.QU_ExchangeRate = 1.1m;
			quoteToAccept.QU_ExchangeRateInverted = 0.9091m;
			quoteToAccept.QU_RX_NKFromCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			var quoteToDiscard = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Requested, paymentApproval);
			quoteToDiscard.QU_RX_NKFromCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			Factory.Save();

			AssertEquals(QuoteStatusCodes.Received, quoteToAccept.QU_Status);
			Assert(!quoteToAccept.QU_ProviderReference.IsEmpty);
			AssertNotEquals(quoteToAccept.QU_RX_NKFromCurrency, Env.CurrentCompany.LocalCurrency.Code);
			paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems.Reload(true);
			AssertEquals(1, paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems.Count);
			AssertEquals(apInvoice.PK, paymentApproval.PaymentMatchingBaseObject_ForTestOnly.PaymentApprovalItems[0].A2_AH);
			AssertEquals(-89.29m, paymentApproval.AV_ExchangeDifference);
			var exxJournals = Factory.Load<ExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference));
			AssertEquals(0, exxJournals.Length);

			var newFactory = new BusinessObjectFactory();
			var quoteInNewFactory = newFactory.LoadTop1<EPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quoteToAccept.PK));
			var paymentApprovalInNewFactory = newFactory.LoadTop1<PaymentApprovalBase>(new ZQuery(AccPaymentApprovalSchema.PK, paymentApproval.PK));
			var (isQuoteAccepted, userMessage) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(quoteInNewFactory, paymentApprovalInNewFactory);

			AssertEquals(true, isQuoteAccepted);
			AssertEquals($"Quote {quoteInNewFactory.QU_InternalReference} has been accepted.", userMessage);
			AssertEquals(QuoteStatusCodes.Accepted, quoteInNewFactory.QU_Status);

			//local payment approval values -> 1000 USD / 1.4 ex rate = 714.29 AUD (not updated)
			//invoice -> 1000 USD / 1.6 ex rate = 625 AUD (not updated)
			//exx journal -> 625 AUD - 714.29 AUD = -89.29 AUD (not updated)
			AssertEquals(714.29m, paymentApprovalInNewFactory.AV_Calc_LocalAmount);
			AssertEquals(1000m, paymentApprovalInNewFactory.AV_Amount);
			AssertEquals(1.4000m, paymentApprovalInNewFactory.AV_PayExRate.Round(4));
			AssertEquals(-89.29m, paymentApprovalInNewFactory.AV_ExchangeDifference);
			var exxJournalsInNewFactory = newFactory.Load<ExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference));
			AssertEquals(0, exxJournalsInNewFactory.Length);
			var quoteToDiscardInNewFactory = newFactory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quoteToDiscard.PK));
			AssertEquals(QuoteStatusCodes.Discarded, quoteToDiscardInNewFactory.QU_Status);
		}

		public void TestAcceptEPaymentQuote_FromSinglePaymentWithForeignCurrency_WhenFromCurrencyEqualsFundingCurrency()
		{
			var paymentApproval = CreatePaymentApprovalForTest();
			paymentApproval.AV_AB_FundingBankAccount = TestObjectCreator.GBPBankAccount.PK;
			Factory.Save();

			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(EPaymentStatusCodes.Quote.Received, paymentApproval);
			quote.QU_RX_NKFromCurrency = "GBP";
			quote.QU_FromAmount = 900m;
			quote.QU_ExchangeRate = 0.9m;
			quote.QU_ExchangeRateInverted = 1.1m;
			quote.QU_ToAmount = 810m;
			Factory.Save();
			paymentApproval.RefreshQuotes();
			AssertEquals(1.4m, paymentApproval.AV_PayExRate);
			var (isQuoteAccepted, _) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(quote, paymentApproval);
			AssertEquals(true, isQuoteAccepted);
			AssertEquals("Exchange rate should not update.", 1.4m, paymentApproval.AV_PayExRate);
		}

		public void TestAcceptEPaymentQuote_FromSinglePaymentWithLocalCurrency_WhenFromCurrencyEqualsFundingCurrency()
		{
			var paymentApproval = CreatePaymentApprovalForTest();
			Factory.Save();

			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(EPaymentStatusCodes.Quote.Received, paymentApproval);
			quote.QU_RX_NKFromCurrency = "AUD";
			quote.QU_FromAmount = 900m;
			quote.QU_ExchangeRate = 0.9m;
			quote.QU_ExchangeRateInverted = 1.1m;
			quote.QU_ToAmount = 810m;
			Factory.Save();
			paymentApproval.RefreshQuotes();
			AssertEquals(1.4m, paymentApproval.AV_PayExRate);
			var (isQuoteAccepted, _) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(quote, paymentApproval);
			AssertEquals(true, isQuoteAccepted);
			AssertEquals("Exchange rate should update.", 0.9m, paymentApproval.AV_PayExRate);
		}

		public void TestAcceptEPaymentQuote_FromSinglePayment_WhenFromCurrencyDiffersFundingCurrency()
		{
			var paymentApproval = CreatePaymentApprovalForTest();
			Factory.Save();

			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(EPaymentStatusCodes.Quote.Received, paymentApproval);
			quote.QU_RX_NKFromCurrency = "GBP";
			var (isQuoteAccepted, message) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(quote, paymentApproval);
			AssertEquals(false, isQuoteAccepted);
			AssertEquals("This quote is no longer valid. Funding Currency of the payment does not match quote currency.", message);
		}

		public void TestAcceptEPaymentQuoteForAcceptedDeal_FromSinglePayment_WhenFromCurrencyEqualsFundingCurrency()
		{
			var paymentApproval = CreatePaymentApprovalForTest();
			paymentApproval.AV_AB_FundingBankAccount = TestObjectCreator.USDBankAccount.PK;
			Factory.Save();

			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(EPaymentStatusCodes.Quote.Accepted, paymentApproval);
			quote.QU_RX_NKFromCurrency = "USD";

			var message = EPaymentQuoteAcceptor.AcceptEPaymentQuoteForAcceptedDeal(quote, paymentApproval);
			AssertNullOrEmptyOrWhitespace(message);
		}

		public void TestAcceptEPaymentQuoteForAcceptedDeal_FromSinglePayment_WhenFromCurrencyDiffersFundingCurrency()
		{
			var paymentApproval = CreatePaymentApprovalForTest();
			Factory.Save();

			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(EPaymentStatusCodes.Quote.Accepted, paymentApproval);
			quote.QU_RX_NKFromCurrency = "GBP";

			var message = EPaymentQuoteAcceptor.AcceptEPaymentQuoteForAcceptedDeal(quote, paymentApproval);
			AssertEquals("Funding Currency does not match quote currency.", message);
		}

		#endregion

		PaymentApprovalBase CreatePaymentApprovalForTest()
		{
			var bankAccount = TestObjectCreator.CreateBankAccount("EPA", "E-Payment Account", TestObjectCreator.USD, TestObjectCreator.GLHeader1);
			bankAccount.AB_GC = Env.CurrentCompanyPK;
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
			bankAccount.AB_PaymentProvider = EPaymentProviderCodes.Codes.OFX;
			Factory.Save();

			var paymentApproval = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.Cheque, bankAccount, TestObjectCreator.USDChequeBook);
			paymentApproval.AV_OH = TestObjectCreator.Creditor1.PK;
			paymentApproval.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			paymentApproval.AV_PayExRate = 1.4m;
			paymentApproval.AV_Amount = 1000m;
			paymentApproval.AV_PostDate = ZDateTime.Today.AddDays(-1);
			paymentApproval.AV_PaymentDate = ZDateTime.Today.AddDays(1);
			paymentApproval.AV_PaymentComment = "Paying FreightQuota Invoice 83942";
			paymentApproval.AV_ChequeOrReference = "00009283";
			paymentApproval.AV_GB = Env.CurrentBranchPK;
			paymentApproval.AV_GC = Env.CurrentCompanyPK;
			paymentApproval.AV_Status = PaymentApprovalStatus.Draft;

			var matchingCollection = new IMatchingCollection(Factory);
			matchingCollection.Add(paymentApproval);
			Factory.Save();

			return paymentApproval;
		}

		void CreateBatchPosterWithForeignFundingCurrencyForPaymentApproval(PaymentApprovalBase paymentApproval)
		{
			var batchPoster = Factory.New<APPaymentBatchPoster>();
			batchPoster.APB_GC = Env.CurrentCompanyPK;
			batchPoster.APB_PaymentType = ReceiptTypes.EPayment;
			batchPoster.APB_AB = TestObjectCreator.AUDBankAccount.PK;
			batchPoster.APB_AB_FundingBankAccount = TestObjectCreator.EURBankAccount.PK;
			Factory.Save();

			paymentApproval.AV_APB_PaymentBatch = batchPoster.PK;
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
