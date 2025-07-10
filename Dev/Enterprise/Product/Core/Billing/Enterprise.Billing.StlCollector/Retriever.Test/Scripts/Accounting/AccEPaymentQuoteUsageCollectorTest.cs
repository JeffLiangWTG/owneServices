using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Billing.Collectors.Accounting;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Accounting
{
	[TestedType(typeof(AccEPaymentQuoteUsageCollector))]
	sealed class AccEPaymentQuoteUsageCollectorTest : RefStlScriptWithDefaultsTest
	{
		//Update the BaseDate (date/hour) when generating the script to run in UAT - useful when setting up for developer functional testing.
		public static DateTime BaseDate = new DateTime(2023, 10, 9, 21, 0, 0, DateTimeKind.Utc);

		readonly List<QuoteUsageCollectorTestCase> testCases = new List<QuoteUsageCollectorTestCase>
		{
			new TestPaymentWithNoQuote(),
			new TestIndicativeRateQueued(),
			new TestIndicativeRateRequested(),
			new TestIndicativeRateReceived(),
			new TestIndicativeRateMultipleRequests(),
			new TestQuoteQueued(),
			new TestQuoteRequested(),
			new TestQuoteReceived(),
			new TestQuoteError(),
			new TestQuoteExpired(),
			new TestQuoteAccepted()
		};

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2023, 10);

		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			var sqlQuery = testCases.Aggregate(
				new StringBuilder().AppendLine(
					GetConfigurationQuery(TestConfig.Default)
				),
				(queryBuilder, testCase) => queryBuilder.AppendLine(testCase.SetupQuery)
				).ToString();
			TestConnection.ExecuteNonQuery(sqlQuery);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			var transactionsByRef = transactions.ToDictionary(t => t.Reference5);
			foreach (var testCase in testCases)
			{
				var testName = testCase.GetType().UnderlyingSystemType.Name;
				foreach (var expectedTransaction in testCase.ExpectedTransactions)
				{
					AssertEquals($"({testName}) Expecting transaction for reference {expectedTransaction.ReferencePK}", true, transactionsByRef.ContainsKey(expectedTransaction.ReferencePK));
					var collectedTransaction = transactionsByRef[expectedTransaction.ReferencePK];
					AssertEquals($"({testName}) User should be {expectedTransaction.CreatingUserCode}", expectedTransaction.CreatingUserCode, collectedTransaction.ClientStaffCode);
					AssertEquals($"({testName}) Company Code should be {testCase.Config.CompanyCode}", testCase.Config.CompanyCode, collectedTransaction.GetCompanyCode());
					AssertEquals($"({testName}) BranchCode should be {testCase.Config.BranchCode}", testCase.Config.BranchCode, collectedTransaction.GetBranchCode());
					AssertEquals($"({testName}) AdditionalRefs should be as expected", expectedTransaction.ToJson(), collectedTransaction.AdditionalRefs);
				}
			}
		}

		string GetConfigurationQuery(TestConfig config)
		{
			return $@"
DECLARE @AccHeaderPK UNIQUEIDENTIFIER = NEWID();

INSERT OrgHeader (OH_PK, OH_Code) VALUES
('{config.OrgProxyPK}', '{config.OrgProxyCode}');

INSERT GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode, GC_OH_OrgProxy) VALUES
('{config.CompanyPK}', '{config.CompanyCode}', 'AU company', '{config.CompanyCurrency}', '{config.CompanyCountry}', '{config.OrgProxyPK}');

INSERT GlbBranch (GB_PK, GB_Code, GB_GC) VALUES
('{config.BranchPK}', '{config.BranchCode}', '{config.CompanyPK}');

INSERT AccGLHeader (AG_PK, AG_AccountType, AG_AccountNum, AG_DebitCredit) VALUES
(@AccHeaderPK, 'BSH', '{config.BankAccountNumber}', 'DR');

INSERT AccBankAccount (AB_PK, AB_GC, AB_AG, AB_Code, AB_AccountNum) VALUES
('{config.BankAccountPK}', '{config.CompanyPK}', @AccHeaderPK, '{config.BankAccountCode}', '{config.BankAccountNumber}');

INSERT INTO AccEPaymentStaffToken (TK_PK, TK_AccountName, TK_GS_NKStaffCode, TK_Scope, TK_AB, TK_RequestedUtc, TK_GC, TK_SystemCreateTimeUtc, TK_SystemCreateUser, TK_SystemLastEditTimeUtc, TK_SystemLastEditUser, TK_Status, TK_ExpiryUtc) VALUES
(NEWID(), 'TSTACC', '{config.UserCode}', 'payments', '{config.BankAccountPK}', '2023-09-28 05:12', '{config.CompanyPK}', '2023-09-28 05:12', '{config.UserCode}', '2023-10-03 07:51', '~BP', 'ATH', '2023-12-27 06:33');
			";
		}
	}

	#region Test Cases

	class TestPaymentWithNoQuote : QuoteUsageCollectorTestCase
	{
		public override string SetupQuery => new TestCaseQueryBuilder()
			.Login(Config.UserCode, Config.CompanyPK, Config.BranchPK, Config.BranchCode, Config.OrgProxyPK)
			.CreatePayment(
				pk: "6118AE27-CA9F-469A-A124-92AF74F60DE3",
				timestampUtc: BaseDate,
				bankAccountPK: Config.BankAccountPK,
				paymentType: "EFT",
				amount: 100m,
				currency: "EUR",
				reference: "p0")
			.Build();

		public override TransactionDetails[] ExpectedTransactions => Array.Empty<TransactionDetails>();
	}

	class TestIndicativeRateQueued : QuoteUsageCollectorTestCase
	{
		const string PaymentPK = "6178C685-2663-4E6D-AD4C-B056B3DD501B";
		const string QuotePK = "BECA2549-1971-435B-9404-F364A8D93B49";

		public override string SetupQuery => new TestCaseQueryBuilder()
			.Login(Config.UserCode, Config.CompanyPK, Config.BranchPK, Config.BranchCode, Config.OrgProxyPK)
			.CreatePayment(PaymentPK,
				timestampUtc: BaseDate.With(minutes: 57),
				bankAccountPK: Config.BankAccountPK,
				paymentType: "EFT",
				amount: 100m,
				currency: "EUR",
				reference: "p1")
			.CreateQuote(QuotePK,
				timestampUtc: BaseDate.With(minutes: 57),
				currency: "AUD",
				reference: "p1q1")
			.Build();

		public override TransactionDetails[] ExpectedTransactions => new[]
		{
			new TransactionDetails(QuotePK)
			{
				PaymentPK = PaymentPK,
				CreatingUserCode = Config.UserCode,
				QuoteType = "Rate",
				Country = "AU",
				Currency = "AUD",
				QuoteStatus = "QUE",
				FromCurrency = "AUD",
				ToCurrency = "EUR",
				ToAmount = "100.0000",
				PaymentCreatedDate = "2023-10-09T21:57:00",
				QuoteCreatedDate = "2023-10-09T21:57:00",
				LastEditDate = "2023-10-09T21:57:00"
			}
		};
	}

	class TestIndicativeRateRequested : QuoteUsageCollectorTestCase
	{
		const string PaymentPK = "A8C09C0E-1011-4589-9354-C74123B50E50";
		const string QuotePK = "36814A32-8A95-4427-88AF-456F1A701975";

		public override string SetupQuery => new TestCaseQueryBuilder()
			.Login(Config.UserCode, Config.CompanyPK, Config.BranchPK, Config.BranchCode, Config.OrgProxyPK)
			.CreatePayment(PaymentPK,
				timestampUtc: BaseDate.With(minutes: 49),
				bankAccountPK: Config.BankAccountPK,
				paymentType: "EFT",
				amount: 110m,
				currency: "EUR",
				reference: "p2")
			.CreateQuote(QuotePK,
				timestampUtc: BaseDate.With(minutes: 49),
				currency: "AUD",
				reference: "p2q1")
			.SendRequestToProvider(
				timestampUtc: BaseDate.With(minutes: 50, seconds: 36, milliseconds: 583))
			.Build();

		public override TransactionDetails[] ExpectedTransactions => new[]
		{
			new TransactionDetails(QuotePK)
			{
				PaymentPK = PaymentPK,
				CreatingUserCode = Config.UserCode,
				QuoteType = "Rate",
				Country = "AU",
				Currency = "AUD",
				QuoteStatus = "REQ",
				FromCurrency = "AUD",
				ToCurrency = "EUR",
				ToAmount = "110.0000",
				PaymentCreatedDate = "2023-10-09T21:49:00",
				QuoteCreatedDate = "2023-10-09T21:49:00",
				LastEditDate = "2023-10-09T21:50:00"
			}
		};
	}

	class TestIndicativeRateReceived : QuoteUsageCollectorTestCase
	{
		const string PaymentPK = "3037C778-92D8-4E7D-AB3A-A04CC0612CA9";
		const string QuotePK = "2AFF3249-F725-4735-B118-456C45F64082";

		public override string SetupQuery => new TestCaseQueryBuilder()
			.Login(Config.UserCode, Config.CompanyPK, Config.BranchPK, Config.BranchCode, Config.OrgProxyPK)
			.CreatePayment(PaymentPK,
				timestampUtc: BaseDate.With(minutes: 10),
				bankAccountPK: Config.BankAccountPK,
				paymentType: "EFT",
				amount: 120m,
				currency: "EUR",
				reference: "p3")
			.CreateQuote(QuotePK,
				timestampUtc: BaseDate.With(minutes: 10),
				currency: "AUD",
				reference: "p3q1")
			.SendRequestToProvider(
				timestampUtc: BaseDate.With(minutes: 11, seconds: 3, milliseconds: 520))
			.ProcessResponseFromProvider(
				timestampUtc: BaseDate.With(minutes: 11, seconds: 13),
				exRate: 0.6512m,
				amount: 184.28m)
			.PostResponseInCargoWise(
				timestampUtc: BaseDate.With(minutes: 13, seconds: 10, milliseconds: 430))
			.Build();

		public override TransactionDetails[] ExpectedTransactions => new[]
		{
			new TransactionDetails(QuotePK)
			{
				PaymentPK = PaymentPK,
				QuoteType = "Rate",
				CreatingUserCode = Config.UserCode,
				Country = "AU",
				Currency = "AUD",
				QuoteStatus = "RCV",
				FromCurrency = "AUD",
				ToCurrency = "EUR",
				ToAmount = "120.0000",
				PaymentCreatedDate = "2023-10-09T21:10:00",
				QuoteCreatedDate = "2023-10-09T21:10:00",
				LastEditDate = "2023-10-09T21:13:00",
				RequestTimeSeconds = "10",
				ResponseProcessingTimeSeconds = "117",
				TotalTimeSeconds = "127"
			}
		};
	}

	class TestIndicativeRateMultipleRequests : QuoteUsageCollectorTestCase
	{
		const string PaymentPK = "9F669BCE-A59A-4EF0-94A0-A87E0A7D7D72";
		const string QuoteDiscardedPK = "DEF19AE5-9CDA-4D6C-BFCD-B4B16A029009";
		const string QuoteReceivedPK = "EE6345E5-6036-46E3-A752-8B202893436A";

		public override string SetupQuery => new TestCaseQueryBuilder()
			.Login(Config.UserCode, Config.CompanyPK, Config.BranchPK, Config.BranchCode, Config.OrgProxyPK)
			.CreatePayment(PaymentPK,
				timestampUtc: BaseDate.With(minutes: 10),
				bankAccountPK: Config.BankAccountPK,
				paymentType: "EFT",
				amount: 120m,
				currency: "EUR",
				reference: "p4")
			.CreateQuote(QuoteDiscardedPK,
				timestampUtc: BaseDate.With(minutes: 10),
				currency: "AUD",
				reference: "p4q1")
			.SendRequestToProvider(
				timestampUtc: BaseDate.With(minutes: 11, seconds: 3, milliseconds: 520))
			.ProcessResponseFromProvider(
				timestampUtc: BaseDate.With(minutes: 11, seconds: 13),
				exRate: 0.6512m,
				amount: 184.28m)
			.PostResponseInCargoWise(
				timestampUtc: BaseDate.With(minutes: 13, seconds: 10, milliseconds: 430))
			.ReplaceQuote(QuoteReceivedPK,
				timestampUtc: BaseDate.With(minutes: 30),
				reference: "p4q2")
			.SendRequestToProvider(
				timestampUtc: BaseDate.With(minutes: 31, seconds: 28, milliseconds: 53))
			.ProcessResponseFromProvider(
				timestampUtc: BaseDate.With(minutes: 32, seconds: 49),
				exRate: 0.6512m,
				amount: 185.81m)
			.PostResponseInCargoWise(
				timestampUtc: BaseDate.With(minutes: 33, seconds: 6, milliseconds: 510))
			.Build();

		public override TransactionDetails[] ExpectedTransactions => new[]
		{
			new TransactionDetails(QuoteDiscardedPK)
			{
				PaymentPK = PaymentPK,
				CreatingUserCode = Config.UserCode,
				QuoteType = "Rate",
				Country = "AU",
				Currency = "AUD",
				QuoteStatus = "DCD",
				FromCurrency = "AUD",
				ToCurrency = "EUR",
				ToAmount = "120.0000",
				PaymentCreatedDate = "2023-10-09T21:10:00",
				QuoteCreatedDate = "2023-10-09T21:10:00",
				LastEditDate = "2023-10-09T21:30:00",
				RequestTimeSeconds = "10",
				ResponseProcessingTimeSeconds = "117",
				TotalTimeSeconds = "127"
			},
			new TransactionDetails(QuoteReceivedPK)
			{
				PaymentPK = PaymentPK,
				CreatingUserCode = Config.UserCode,
				QuoteType = "Rate",
				Country = "AU",
				Currency = "AUD",
				QuoteStatus = "RCV",
				FromCurrency = "AUD",
				ToCurrency = "EUR",
				ToAmount = "120.0000",
				PaymentCreatedDate = "2023-10-09T21:10:00",
				QuoteCreatedDate = "2023-10-09T21:30:00",
				LastEditDate = "2023-10-09T21:33:00",
				RequestTimeSeconds = "81",
				ResponseProcessingTimeSeconds = "17",
				TotalTimeSeconds = "98"
			}
		};
	}

	class TestQuoteQueued : QuoteUsageCollectorTestCase
	{
		const string PaymentPK = "F5839E54-95F0-4A8D-BA66-52463A985A8A";
		const string QuotePK = "7D4683F2-6CDC-4DE6-B437-0978D0B2A657";

		public override string SetupQuery => new TestCaseQueryBuilder()
			.Login(Config.UserCode, Config.CompanyPK, Config.BranchPK, Config.BranchCode, Config.OrgProxyPK)
			.CreatePayment(PaymentPK,
				timestampUtc: BaseDate.With(minutes: 48),
				bankAccountPK: Config.BankAccountPK,
				paymentType: "EPA",
				amount: 130m,
				currency: "EUR",
				reference: "p5")
			.CreateQuote(QuotePK,
				timestampUtc: BaseDate.With(minutes: 48),
				currency: "AUD",
				reference: "p5q1")
			.Build();

		public override TransactionDetails[] ExpectedTransactions => new[]
		{
			new TransactionDetails(QuotePK)
			{
				PaymentPK = PaymentPK,
				CreatingUserCode = Config.UserCode,
				QuoteType = "Quote",
				Country = "AU",
				Currency = "AUD",
				QuoteStatus = "QUE",
				FromCurrency = "AUD",
				ToCurrency = "EUR",
				ToAmount = "130.0000",
				PaymentCreatedDate = "2023-10-09T21:48:00",
				QuoteCreatedDate = "2023-10-09T21:48:00",
				LastEditDate = "2023-10-09T21:48:00"
			}
		};
	}

	class TestQuoteRequested : QuoteUsageCollectorTestCase
	{
		const string PaymentPK = "17AFF550-54F9-488E-9219-507F87673C90";
		const string QuotePK = "5586E3CB-ADD0-4FFC-817C-36D8966E4C83";

		public override string SetupQuery => new TestCaseQueryBuilder()
			.Login(Config.UserCode, Config.CompanyPK, Config.BranchPK, Config.BranchCode, Config.OrgProxyPK)
			.CreatePayment(PaymentPK,
				timestampUtc: BaseDate.With(minutes: 1),
				bankAccountPK: Config.BankAccountPK,
				paymentType: "EPA",
				amount: 130m,
				currency: "EUR",
				reference: "p6")
			.CreateQuote(QuotePK,
				timestampUtc: BaseDate.With(minutes: 1),
				currency: "AUD",
				reference: "p6q1")
			.SendRequestToProvider(
				timestampUtc: BaseDate.With(minutes: 25, seconds: 24, milliseconds: 700))
			.Build();

		public override TransactionDetails[] ExpectedTransactions => new[]
		{
			new TransactionDetails(QuotePK)
			{
				PaymentPK = PaymentPK,
				CreatingUserCode = Config.UserCode,
				QuoteType = "Quote",
				Country = "AU",
				Currency = "AUD",
				QuoteStatus = "REQ",
				FromCurrency = "AUD",
				ToCurrency = "EUR",
				ToAmount = "130.0000",
				PaymentCreatedDate = "2023-10-09T21:01:00",
				QuoteCreatedDate = "2023-10-09T21:01:00",
				LastEditDate = "2023-10-09T21:25:00"
			}
		};
	}

	class TestQuoteReceived : QuoteUsageCollectorTestCase
	{
		const string PaymentPK = "4B74FE36-2568-4C35-BCBB-1A7D6BA60A9B";
		const string QuotePK = "4DEC1442-ACC4-48B8-962E-C36E6C14C34B";

		public override string SetupQuery => new TestCaseQueryBuilder()
			.Login(Config.UserCode, Config.CompanyPK, Config.BranchPK, Config.BranchCode, Config.OrgProxyPK)
			.CreatePayment(PaymentPK,
				timestampUtc: BaseDate.With(minutes: 39),
				bankAccountPK: Config.BankAccountPK,
				paymentType: "EPA",
				amount: 250m,
				currency: "EUR",
				reference: "p7")
			.CreateQuote(QuotePK,
				timestampUtc: BaseDate.With(minutes: 39),
				currency: "AUD",
				reference: "p7q1")
			.SendRequestToProvider(
				timestampUtc: BaseDate.With(minutes: 39, seconds: 40, milliseconds: 873))
			.ProcessResponseFromProvider(
				timestampUtc: BaseDate.With(minutes: 40, seconds: 7),
				exRate: 0.6626m,
				amount: 377.3m,
				feeAmount: 15m)
			.PostResponseInCargoWise(
				timestampUtc: BaseDate.With(minutes: 41, seconds: 17, milliseconds: 60))
			.Build();

		public override TransactionDetails[] ExpectedTransactions => new[]
		{
			new TransactionDetails(QuotePK)
			{
				PaymentPK = PaymentPK,
				CreatingUserCode = Config.UserCode,
				QuoteType = "Quote",
				Country = "AU",
				Currency = "AUD",
				QuoteStatus = "RCV",
				FromCurrency = "AUD",
				ToCurrency = "EUR",
				ToAmount = "250.0000",
				FeeAmount = "15.0000",
				PaymentCreatedDate = "2023-10-09T21:39:00",
				QuoteCreatedDate = "2023-10-09T21:39:00",
				LastEditDate = "2023-10-09T21:41:00",
				RequestTimeSeconds = "27",
				ResponseProcessingTimeSeconds = "70",
				TotalTimeSeconds = "97"
			}
		};
	}

	class TestQuoteError : QuoteUsageCollectorTestCase
	{
		const string PaymentPK = "1CBF84D6-B741-463B-8DC1-47334FF33BFE";
		const string QuotePK = "C44A80CB-9877-46E1-851A-6933A8C97685";

		public override string SetupQuery => new TestCaseQueryBuilder()
			.Login(Config.UserCode, Config.CompanyPK, Config.BranchPK, Config.BranchCode, Config.OrgProxyPK)
			.CreatePayment(PaymentPK,
				timestampUtc: BaseDate.With(minutes: 8),
				bankAccountPK: Config.BankAccountPK,
				paymentType: "EPA",
				amount: 130m,
				currency: "EUR",
				reference: "p8")
			.CreateQuote(QuotePK,
				timestampUtc: BaseDate.With(minutes: 8),
				currency: "AUD",
				reference: "p8q1")
			.SendRequestToProvider(
				timestampUtc: BaseDate.With(minutes: 32, seconds: 24, milliseconds: 700))
			.ProcessErrorResponseFromProvider(
				timestampUtc: BaseDate.With(minutes: 41, seconds: 30),
				errorMessage: "Please retry your last action or re-authorise your OFX user account from your Bank Account before proceeding.")
			.PostResponseInCargoWise(
				timestampUtc: BaseDate.With(minutes: 45, seconds: 57, milliseconds: 433))
			.Build();

		public override TransactionDetails[] ExpectedTransactions => new[]
		{
			new TransactionDetails(QuotePK)
			{
				PaymentPK = PaymentPK,
				CreatingUserCode = Config.UserCode,
				QuoteType = "Quote",
				Country = "AU",
				Currency = "AUD",
				QuoteStatus = "ERR",
				QuoteMessage = "Please retry your last action or re-authorise your OFX user account from your Bank Account before proceeding.",
				FromCurrency = "AUD",
				ToCurrency = "EUR",
				ToAmount = "130.0000",
				PaymentCreatedDate = "2023-10-09T21:08:00",
				QuoteCreatedDate = "2023-10-09T21:08:00",
				LastEditDate = "2023-10-09T21:45:00"
			}
		};
	}

	class TestQuoteExpired : QuoteUsageCollectorTestCase
	{
		const string PaymentPK = "B5979C7F-0A88-4C5E-A229-E2AF8C0CAB09";
		const string QuoteExpiredPK = "FDB7BAFE-D928-4EB9-859F-969DBEFABBB5";
		const string QuoteAcceptedPK = "D4DA241F-FD56-40F6-8D66-3D83B2EA6EB9";

		public override string SetupQuery => new TestCaseQueryBuilder()
			.Login(Config.UserCode, Config.CompanyPK, Config.BranchPK, Config.BranchCode, Config.OrgProxyPK)
			.CreatePayment(PaymentPK,
				timestampUtc: BaseDate.With(minutes: 3),
				bankAccountPK: Config.BankAccountPK,
				paymentType: "EPA",
				amount: 510m,
				currency: "EUR",
				reference: "p9")
			.CreateQuote(QuoteExpiredPK,
				timestampUtc: BaseDate.With(minutes: 3),
				currency: "AUD",
				reference: "p9q1")
			.SendRequestToProvider(
				timestampUtc: BaseDate.With(minutes: 9, seconds: 45, milliseconds: 177))
			.ProcessResponseFromProvider(
				timestampUtc: BaseDate.With(minutes: 13, seconds: 15),
				exRate: 0.6626m,
				amount: 769.7m,
				feeAmount: 10m)
			.PostResponseInCargoWise(
				timestampUtc: BaseDate.With(minutes: 17, seconds: 4, milliseconds: 190))
			.AcceptExpiredQuote(
				timestampUtc: BaseDate.With(minutes: 45),
				newQuotePK: QuoteAcceptedPK,
				newQuoteReference: "p9q2")
			.Build();

		public override TransactionDetails[] ExpectedTransactions => new[]
		{
			new TransactionDetails(QuoteExpiredPK)
			{
				PaymentPK = PaymentPK,
				CreatingUserCode = Config.UserCode,
				QuoteType = "Quote",
				Country = "AU",
				Currency = "AUD",
				QuoteStatus = "DCD",
				QuoteMessage = "Quote Expired",
				FromCurrency = "AUD",
				ToCurrency = "EUR",
				ToAmount = "510.0000",
				FeeAmount = "10.0000",
				PaymentCreatedDate = "2023-10-09T21:03:00",
				QuoteCreatedDate = "2023-10-09T21:03:00",
				LastEditDate = "2023-10-09T21:45:00",
				RequestTimeSeconds = "210",
				ResponseProcessingTimeSeconds = "229",
				TotalTimeSeconds = "439"
			},
			new TransactionDetails(QuoteAcceptedPK)
			{
				PaymentPK = PaymentPK,
				CreatingUserCode = "~AD",
				QuoteType = "Quote",
				Country = "AU",
				Currency = "AUD",
				QuoteStatus = "ACP",
				QuoteMessage = "",
				FromCurrency = "AUD",
				ToCurrency = "EUR",
				ToAmount = "510.0000",
				FeeAmount = "10.0000",
				PaymentCreatedDate = "2023-10-09T21:03:00",
				QuoteCreatedDate = "2023-10-09T21:45:00",
				LastEditDate = "2023-10-09T21:45:00"
			}
		};
	}

	class TestQuoteAccepted : QuoteUsageCollectorTestCase
	{
		const string PaymentPK = "CBC8F175-7253-482A-99E4-4636E92E9977";
		const string QuotePK = "A8E4A783-C506-4AA4-B9D0-AED045E2D186";

		public override string SetupQuery => new TestCaseQueryBuilder()
			.Login(Config.UserCode, Config.CompanyPK, Config.BranchPK, Config.BranchCode, Config.OrgProxyPK)
			.CreatePayment(PaymentPK,
				timestampUtc: BaseDate.With(minutes: 54),
				bankAccountPK: Config.BankAccountPK,
				paymentType: "EPA",
				amount: 550m,
				currency: "EUR",
				reference: "p10")
			.CreateQuote(QuotePK,
				timestampUtc: BaseDate.With(minutes: 54),
				currency: "AUD",
				reference: "p10q1")
			.SendRequestToProvider(
				timestampUtc: BaseDate.With(minutes: 54, seconds: 45, milliseconds: 380))
			.ProcessResponseFromProvider(
				timestampUtc: BaseDate.With(minutes: 57, seconds: 51),
				exRate: 0.6512m,
				amount: 830.06m,
				feeAmount: 15m)
			.PostResponseInCargoWise(
				timestampUtc: BaseDate.With(minutes: 58, seconds: 45, milliseconds: 243))
			.AcceptQuote(
				timestampUtc: BaseDate.With(minutes: 59))
			.Build();

		public override TransactionDetails[] ExpectedTransactions => new[]
		{
			new TransactionDetails(QuotePK)
			{
				PaymentPK = PaymentPK,
				CreatingUserCode = Config.UserCode,
				QuoteType = "Quote",
				Country = "AU",
				Currency = "AUD",
				QuoteStatus = "ACP",
				FromCurrency = "AUD",
				ToCurrency = "EUR",
				ToAmount = "550.0000",
				FeeAmount = "15.0000",
				PaymentCreatedDate = "2023-10-09T21:54:00",
				QuoteCreatedDate = "2023-10-09T21:54:00",
				LastEditDate = "2023-10-09T21:59:00",
				RequestTimeSeconds = "186",
				ResponseProcessingTimeSeconds = "54",
				TotalTimeSeconds = "240"
			}
		};
	}

	#endregion

	#region Supporting Classes

	abstract class QuoteUsageCollectorTestCase
	{
		protected DateTime BaseDate = AccEPaymentQuoteUsageCollectorTest.BaseDate;
		public virtual TestConfig Config => TestConfig.Default;
		public abstract string SetupQuery { get; }
		public abstract TransactionDetails[] ExpectedTransactions { get; }
	}

	class TestConfig
	{
		public string CompanyPK { get; set; }
		public string CompanyCode { get; set; }
		public string CompanyCountry { get; set; }
		public string CompanyCurrency { get; set; }
		public string OrgProxyPK { get; set; }
		public string OrgProxyCode { get; set; }
		public string BranchPK { get; set; }
		public string BranchCode { get; set; }
		public string BankAccountPK { get; set; }
		public string BankAccountCode { get; set; }
		public string BankAccountNumber { get; set; }
		public string UserCode { get; set; }

		public static TestConfig Default = new TestConfig
		{
			CompanyPK = "A0D5B451-E634-4E71-95E6-1DB52849420C",
			CompanyCode = "XAU",
			CompanyCountry = "AU",
			CompanyCurrency = "AUD",
			OrgProxyPK = "217E60D3-2964-4BC9-8998-4A2E981CBC25",
			OrgProxyCode = "SYDNEY",
			BranchPK = "05D3C471-A601-4DA7-9370-1FF35EF746E0",
			BranchCode = "XAU",
			BankAccountPK = "1C56130F-8030-4078-A0A4-F4BA8581EF3F",
			BankAccountCode = "XAU1",
			BankAccountNumber = "123456",
			UserCode = "TST"
		};
	}

	class TestCaseQueryBuilder
	{
		const string SystemUserCode = "~AD";
		string userCode, companyPK, branchPK, branchCode, orgProxyPK, bankAccountPK,
			paymentPK, paymentType, toCurrency, fromCurrency, paymentReference,
			quotePK, quoteReference, quoteStatus, quoteMessage = "", feeCurrency = "", providerReference = "";
		DateTime paymentTimestampUtc, quoteTimestampUtc, quoteLastEditDate, responseTimestampUtc;
		decimal fromAmount, toAmount, providerExRate, providerInvertedExRate, feeAmount;
		bool isQuote, isError, responseReceived;
		StringBuilder insertQuoteValuesSqlBuilder = new StringBuilder(), insertLogValuesSqlBuilder = new StringBuilder();

		public TestCaseQueryBuilder Login(string userCode, string companyPK, string branchPK, string branchCode, string orgProxyPK)
		{
			this.userCode = userCode;
			this.companyPK = companyPK;
			this.branchPK = branchPK;
			this.branchCode = branchCode;
			this.orgProxyPK = orgProxyPK;

			return this;
		}

		public TestCaseQueryBuilder CreatePayment(string pk, DateTime timestampUtc, string bankAccountPK, string paymentType, decimal amount, string currency, string reference)
		{
			paymentPK = pk;
			paymentTimestampUtc = timestampUtc;
			this.bankAccountPK = bankAccountPK;
			this.paymentType = paymentType;
			toAmount = amount;
			toCurrency = currency;
			paymentReference = reference;
			isQuote = paymentType == "EPA";

			return this;
		}

		public TestCaseQueryBuilder CreateQuote(string pk, DateTime timestampUtc, string currency, string reference)
		{
			quotePK = pk;
			quoteTimestampUtc = timestampUtc;
			quoteLastEditDate = quoteTimestampUtc;
			fromCurrency = currency;
			quoteReference = reference;
			quoteStatus = "QUE";
			quoteMessage = string.Empty;

			return this;
		}

		public TestCaseQueryBuilder ReplaceQuote(string quotePK, DateTime timestampUtc, string reference)
		{
			DiscardQuote(timestampUtc);

			return CreateQuote(quotePK, timestampUtc, fromCurrency, reference);
		}

		public TestCaseQueryBuilder SendRequestToProvider(DateTime timestampUtc)
		{
			quoteLastEditDate = timestampUtc;
			quoteStatus = "REQ";
			AddEventToQuery("DEX", $"Purpose: FX Quote Request {quoteReference} Sent to OFX", "~BP", timestampUtc, timestampUtc);

			return this;
		}

		public TestCaseQueryBuilder ProcessResponseFromProvider(DateTime timestampUtc, decimal exRate, decimal amount, decimal feeAmount = 0m)
		{
			responseReceived = true;
			responseTimestampUtc = timestampUtc;
			providerExRate = exRate;
			providerInvertedExRate = 1 / exRate;
			fromAmount = amount;
			this.feeAmount = feeAmount;
			feeCurrency = fromCurrency;
			providerReference = isQuote ? quoteReference : string.Empty;

			return this;
		}

		public TestCaseQueryBuilder ProcessErrorResponseFromProvider(DateTime timestampUtc, string errorMessage)
		{
			responseReceived = true;
			isError = true;
			responseTimestampUtc = timestampUtc;
			quoteMessage = errorMessage;
			providerReference = isQuote ? quoteReference : string.Empty;

			return this;
		}

		public TestCaseQueryBuilder PostResponseInCargoWise(DateTime timestampUtc)
		{
			quoteLastEditDate = timestampUtc;
			quoteStatus = isError ? "ERR" : "RCV";
			var eventType = isError ? "IRJ" : "IAK";
			var reference = isQuote ? "E-Quote Received from OFX." : "Indicative Rate Received from OFX.";
			AddEventToQuery(eventType, reference, SystemUserCode, timestampUtc, responseTimestampUtc);

			return this;
		}

		public TestCaseQueryBuilder AcceptQuote(DateTime timestampUtc)
		{
			quoteLastEditDate = timestampUtc;
			quoteStatus = "ACP";

			return this;
		}

		public TestCaseQueryBuilder AcceptExpiredQuote(DateTime timestampUtc, string newQuotePK, string newQuoteReference)
		{
			quoteMessage = "Quote Expired";
			DiscardQuote(timestampUtc);

			userCode = SystemUserCode;
			return CreateQuote(newQuotePK, timestampUtc, fromCurrency, newQuoteReference)
				.AcceptQuote(timestampUtc);
		}

		void DiscardQuote(DateTime timestampUtc)
		{
			quoteLastEditDate = timestampUtc;
			quoteStatus = "DCD";
			AddCurrentQuoteToQuery();
		}

		public string Build()
		{
			var sqlQueryBuilder = new StringBuilder();
			if (paymentPK != null)
			{
				sqlQueryBuilder = sqlQueryBuilder.AppendLine($@"
INSERT INTO AccPaymentApproval (AV_PK,AV_PayRunNo,AV_Status,AV_PaymentType,AV_PayExRate,AV_Amount,AV_GB,AV_OH,AV_AB,AV_Ledger,AV_ExchangeDifference,AV_Discount,AV_PaymentDate,AV_PostDate,AV_GS_NKApproval1st,AV_GS_NKApproval2nd,AV_GS_NKApproval3rd,AV_RX_NKPaymentCurrency,AV_ChequeOrReference,AV_PaymentComment,AV_RejectionReasonCode,AV_RejectionReasonDetails,AV_APB_PaymentBatch,AV_GC,AV_PaymentApprovalReference,AV_SystemCreateTimeUtc,AV_SystemCreateUser,AV_SystemLastEditTimeUtc,AV_SystemLastEditUser,AV_EPaymentReasonCode) VALUES
('{paymentPK}', 0, 'DFT', '{paymentType}', 0.000000000, {toAmount.To2Dp()}, '{branchPK}', '{orgProxyPK}', '{bankAccountPK}', 'AP', 0.00, 0.00, '2023-10-10 00:00', '2023-10-10 00:00', '', '', '', '{toCurrency}', '', 'AP PAYMENT', '', '', NULL, '{companyPK}', '{paymentReference}', '{paymentTimestampUtc.ToSmallDate()}', '{userCode}', '{paymentTimestampUtc.ToSmallDate()}', '{userCode}', '')");
			}

			if (quotePK != null)
			{
				AddCurrentQuoteToQuery();
				sqlQueryBuilder = sqlQueryBuilder.AppendLine($@"
INSERT INTO AccEPaymentQuote (QU_PK,QU_InternalReference,QU_AV,QU_GC,QU_Status,QU_ProviderCode,QU_ProviderReference,QU_FromAmount,QU_RX_NKFromCurrency,QU_ToAmount,QU_RX_NKToCurrency,QU_FeeAmount,QU_RX_NKFeeCurrency,QU_ExchangeRate,QU_ExchangeRateInverted,QU_LastResponseReceivedUtc,QU_ErrorDescription,QU_SystemCreateTimeUtc,QU_SystemCreateUser,QU_SystemLastEditTimeUtc,QU_SystemLastEditUser) VALUES {insertQuoteValuesSqlBuilder}");
			}

			if (insertLogValuesSqlBuilder.Length != 0)
			{
				sqlQueryBuilder = sqlQueryBuilder.AppendLine($@"
INSERT INTO StmALog (SL_PK,SL_Table,SL_Parent,SL_IsEstimate,SL_IsCancelled,SL_Reference,SL_PostedTimeUtc,SL_EventTime,SL_GS_NKUser,SL_SE_NKEvent,SL_GB_NKBranch,SL_GE_NKDepartment,SL_FireWorkflow,SL_DataSource,SL_EventTimeUtc) VALUES {insertLogValuesSqlBuilder}");
			}

			return sqlQueryBuilder.ToString();
		}

		void AddCurrentQuoteToQuery()
		{
			insertQuoteValuesSqlBuilder = insertQuoteValuesSqlBuilder.Append($@"{(insertQuoteValuesSqlBuilder.Length != 0 ? "," : "")}
('{quotePK}', '{quoteReference}', '{paymentPK}', '{companyPK}', '{quoteStatus}', 'OFX', '{providerReference}', {fromAmount.To2Dp()}, '{fromCurrency}', {toAmount.To2Dp()}, '{toCurrency}', {feeAmount}, '{feeCurrency}', {providerExRate.To9Dp()}, {providerInvertedExRate.To9Dp()}, {(responseReceived ? $"'{responseTimestampUtc.ToLongDate()}'" : "NULL")}, '{quoteMessage}', '{quoteTimestampUtc.ToSmallDate()}', '{userCode}', '{quoteLastEditDate.ToSmallDate()}', '~BP')");
		}

		void AddEventToQuery(string eventType, string reference, string userCode, DateTime postedTimeUtc, DateTime eventTimeUtc)
		{
			insertLogValuesSqlBuilder = insertLogValuesSqlBuilder.Append($@"{(insertLogValuesSqlBuilder.Length != 0 ? "," : "")}
(NEWID(), 'AccPaymentApproval', '{paymentPK}', 'N', 'N', '{reference}', '{postedTimeUtc.ToLongDate()}', '{eventTimeUtc.ToLongDate()}', '{userCode}', '{eventType}', '{branchCode}', 'BRN', 0, 'C', '{eventTimeUtc.ToLongDate()}'),
(NEWID(), 'AccEPaymentQuote', '{quotePK}', 'N', 'N', '{reference}', '{postedTimeUtc.ToLongDate()}', '{eventTimeUtc.ToLongDate()}', '{userCode}', '{eventType}', '{branchCode}', 'BRN', 0, 'C', '{eventTimeUtc.ToLongDate()}')");
		}
	}

	class TransactionDetails
	{
		public TransactionDetails(string quotePK)
		{
			ReferencePK = quotePK;
		}

		public string ReferencePK { get; }
		public string PaymentPK { get; set; }
		public string CreatingUserCode { get; set; }
		public string QuoteType { get; set; }
		public string Country { get; set; }
		public string Currency { get; set; }
		public string QuoteStatus { get; set; }
		public string QuoteMessage { get; set; } = "";
		public string FromCurrency { get; set; }
		public string ToAmount { get; set; }
		public string ToCurrency { get; set; }
		public string FeeAmount { get; set; } = "0.0000";
		public string PaymentCreatedDate { get; set; }
		public string QuoteCreatedDate { get; set; }
		public string LastEditDate { get; set; }
		public string RequestTimeSeconds { get; set; }
		public string ResponseProcessingTimeSeconds { get; set; }
		public string TotalTimeSeconds { get; set; }

		public string ToJson() =>
			$"{{\"PaymentPK\":\"{PaymentPK}\"" +
			$",\"QuoteType\":\"{QuoteType}\"" +
			$",\"Country\":\"{Country}\"" +
			$",\"Currency\":\"{Currency}\"" +
			$",\"QuoteStatus\":\"{QuoteStatus}\"" +
			$",\"QuoteMessage\":\"{QuoteMessage}\"" +
			$",\"FromCurrency\":\"{FromCurrency}\"" +
			$",\"ToCurrency\":\"{ToCurrency}\"" +
			$",\"ToAmount\":{ToAmount}" +
			$",\"FeeAmount\":{FeeAmount}" +
			$",\"PaymentCreatedDate\":\"{PaymentCreatedDate}\"" +
			$",\"QuoteCreatedDate\":\"{QuoteCreatedDate}\"" +
			$",\"LastEditDate\":\"{LastEditDate}\"" +
			(RequestTimeSeconds != null ? $",\"RequestTimeSeconds\":{RequestTimeSeconds}" : "") +
			(ResponseProcessingTimeSeconds != null ? $",\"ResponseProcessingTimeSeconds\":{ResponseProcessingTimeSeconds}" : "") +
			(TotalTimeSeconds != null ? $",\"TotalTimeSeconds\":{TotalTimeSeconds}" : "") +
			"}";
	}

	static class TestExtensions
	{
#pragma warning disable CW1050
		public static DateTime With(this DateTime date, int minutes = 0, int seconds = 0, int milliseconds = 0) =>
			date.Add(new TimeSpan(0, 0, minutes, seconds, milliseconds));
#pragma warning restore CW1050

		public static string To2Dp(this decimal value)
		{
			return value.ToString("##.00");
		}
		public static string To9Dp(this decimal value)
		{
			return value.ToString("##.000000000");
		}

		public static string ToSmallDate(this DateTime date)
		{
			return date.ToString("yyyy-MM-dd HH:mm");
		}

		public static string ToLongDate(this DateTime date)
		{
			return date.ToString("yyyy-MM-dd HH:mm:ss.fff");
		}
	}
	#endregion
}
