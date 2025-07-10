using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.PeriodApportionment
{
	public class PeriodApportionmentMethodsTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			testObjectCreator = new TestObjectCreator(Factory);
		}

		TestObjectCreator testObjectCreator;

		[TestDate(2020, 2, 15)]
		public void TestManualPeriodApportionment()
		{
			var clearingAccPK = testObjectCreator.GLJournalClearingAccount.PK;

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(2021, GlbCompany.CurrentCompany.PK);
			Factory.Save();

			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.AUD, 1m, testObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			Assert("Precondition", invoiceLine.PeriodApportionment.Lines.IsNullOrEmpty());
			invoiceLine.AL_AG = testObjectCreator.GLHeader1.PK;
			invoiceLine.AL_ExchangeRate = 0.64m;
			invoiceLine.AL_LocalExTaxAmount = 909.09090909;
			invoiceLine.AL_AT = testObjectCreator.GST1.PK;
			invoiceLine.AL_RX_NKTransactionCurrency = "USD";
			Assert(invoiceLine.PeriodApportionment.Lines.IsNullOrEmpty());

			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.EquallyOverPeriods;
			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 7, 27);
			invoiceLine.PeriodClearingGLAccountPK = clearingAccPK;

			var expected = new[]
			{
				(202008, 96.97m, 151.52m, "USD", 0.64m, clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
				(202009, 96.97m, 151.52m, "USD", 0.64m, clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
				(202010, 96.97m, 151.52m, "USD", 0.64m, clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
				(202011, 96.97m, 151.52m, "USD", 0.64m, clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
				(202012, 96.97m, 151.52m, "USD", 0.64m, clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
				(202101, 96.97m, 151.49m, "USD", 0.640108m, clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
			};

			string lineSelector(PeriodApportionmentLine x)
			{
				return (x.Period, x.OSAmount, x.LocalAmount, x.CurrencyCode, x.ExchangeRate, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK, x.OSAmountInfo.ReadOnly).ToString();
			}

			IEnumerable<string> actual()
			{
				return invoiceLine.PeriodApportionment.Lines
					.Cast<PeriodApportionmentLine>()
					.Select(lineSelector);
			}

			AssertContainsExactElementsInAnyOrder(expected, actual());
			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.Manual;

			expected = new[]
			{
				(202008, 96.97m, 151.52m, "USD", 0.64m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202009, 96.97m, 151.52m, "USD", 0.64m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202010, 96.97m, 151.52m, "USD", 0.64m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202011, 96.97m, 151.52m, "USD", 0.64m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202012, 96.97m, 151.52m, "USD", 0.64m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202101, 96.97m, 151.49m, "USD", 0.640108m, clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
			};

			AssertContainsExactElementsInAnyOrder(expected, actual());

			invoiceLine.PeriodApportionment.Lines[0].OSAmount = 100m;
			invoiceLine.PeriodApportionment.Lines[1].OSAmount = 90m;
			invoiceLine.PeriodApportionment.Lines[2].OSAmount = 80m;

			expected = new[]
			{
				(202008, 100m,      156.25m,    "USD", 0.64m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202009, 90m,       140.63m,    "USD", 0.64m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202010, 80m,       125m,       "USD", 0.64m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202011, 96.97m,    151.52m,    "USD", 0.64m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202012, 96.97m,    151.52m,    "USD", 0.64m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202101, 117.88m,   184.17m,    "USD", 0.640061m, clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
			};

			AssertContainsExactElementsInAnyOrder(expected, actual());

			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 7, 28);

			AssertContainsExactElementsInAnyOrder("even dates changed, periods count hasn't so everything stays same", expected, actual());

			invoiceLine.PeriodEndDate = new ZDate(2020, 8, 28);

			expected = new[]
			{
				(202008, 83.12m, 129.88m, "USD", 0.64m,     clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202009, 83.12m, 129.88m, "USD", 0.64m,     clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202010, 83.12m, 129.88m, "USD", 0.64m,     clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202011, 83.12m, 129.88m, "USD", 0.64m,     clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202012, 83.12m, 129.88m, "USD", 0.64m,     clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202101, 83.12m, 129.88m, "USD", 0.64m,     clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202102, 83.10m, 129.81m, "USD", 0.640166m, clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
			};

			AssertContainsExactElementsInAnyOrder("More periods should trigger recalculation using last non-manual non-default method", expected, actual());

			Factory.Save();
		}

		[TestDate(2019, 12, 21)]
		public void TestEquallyOverPeriodPeriodsApportionment()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			Factory.Save();

			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.AUD, 1m, testObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			Assert("Precondition", invoiceLine.PeriodApportionmentLines.IsNullOrEmpty());
			invoiceLine.AL_AG = testObjectCreator.GLHeader1.PK;
			invoiceLine.AL_ExchangeRate = 0.64m;
			invoiceLine.AL_OSExTaxAmount = 100m;
			invoiceLine.AL_AT = testObjectCreator.GST1.PK;
			invoiceLine.AL_RX_NKTransactionCurrency = "USD";
			Assert(invoiceLine.PeriodApportionmentLines.IsNullOrEmpty());

			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.EquallyOverPeriods;
			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 4, 29);
			invoiceLine.PeriodClearingGLAccountPK = testObjectCreator.GLJournalClearingAccount.PK;

			var expected = new[]
			{
				(202008, 33.33m, 52.08m, "USD", 0.64m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
				(202009, 33.33m, 52.08m, "USD", 0.64m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
				(202010, 33.34m, 52.09m, "USD", 0.640046m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
			};

			var actual = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Select(x => (x.Period, x.OSAmount, x.LocalAmount, x.CurrencyCode, x.ExchangeRate, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK).ToString());
			AssertContainsExactElementsInAnyOrder(expected, actual);

			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.Default;
			Assert(invoiceLine.PeriodApportionmentLines.IsNullOrEmpty());
		}

		[TestDate(2019, 12, 21)]
		public void TestTestEquallyOverPeriodPeriodsApportionment2()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(2021, GlbCompany.CurrentCompany.PK);
			Factory.Save();

			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.AUD, 1m, testObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			Assert("Precondition", invoiceLine.PeriodApportionmentLines.IsNullOrEmpty());
			invoiceLine.AL_AG = testObjectCreator.GLHeader1.PK;
			invoiceLine.AL_ExchangeRate = 0.64m;
			invoiceLine.AL_LocalExTaxAmount = 909.09090909;
			invoiceLine.AL_AT = testObjectCreator.GST1.PK;
			invoiceLine.AL_RX_NKTransactionCurrency = "USD";
			Assert(invoiceLine.PeriodApportionmentLines.IsNullOrEmpty());

			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 7, 29);
			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.EquallyOverPeriods;
			invoiceLine.PeriodClearingGLAccountPK = testObjectCreator.GLJournalClearingAccount.PK;

			var expected = new[]
			{
				(202008, 96.97m, 151.52m, "USD", 0.64m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
				(202009, 96.97m, 151.52m, "USD", 0.64m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
				(202010, 96.97m, 151.52m, "USD", 0.64m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
				(202011, 96.97m, 151.52m, "USD", 0.64m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
				(202012, 96.97m, 151.52m, "USD", 0.64m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
				(202101, 96.97m, 151.49m, "USD", 0.640108m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
			};

			var actual = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Select(x => (x.Period, x.OSAmount, x.LocalAmount, x.CurrencyCode, x.ExchangeRate, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK).ToString());
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		[TestDate(2019, 12, 21)]
		public void TestSettingMANFirstTriggersPeriodsCalculated()
		{
			var clearingAccPK = testObjectCreator.GLJournalClearingAccount.PK;

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(2021, GlbCompany.CurrentCompany.PK);
			Factory.Save();

			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.AUD, 1m, testObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.Manual;
			invoiceLine.AL_AG = testObjectCreator.GLHeader1.PK;
			invoiceLine.AL_ExchangeRate = 0.64m;
			invoiceLine.AL_LocalExTaxAmount = 909.09090909;
			invoiceLine.AL_AT = testObjectCreator.GST1.PK;
			invoiceLine.AL_RX_NKTransactionCurrency = "USD";
			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 7, 27);
			invoiceLine.PeriodClearingGLAccountPK = clearingAccPK;

			var expected = new[]
			{
				(202008, 96.97m, 151.52m, "USD", 0.64m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202009, 96.97m, 151.52m, "USD", 0.64m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202010, 96.97m, 151.52m, "USD", 0.64m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202011, 96.97m, 151.52m, "USD", 0.64m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202012, 96.97m, 151.52m, "USD", 0.64m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202101, 96.97m, 151.49m, "USD", 0.640108m, clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
			};

			string lineSelector(PeriodApportionmentLine x)
			{
				return (x.Period, x.OSAmount, x.LocalAmount, x.CurrencyCode, x.ExchangeRate, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK, x.OSAmountInfo.ReadOnly).ToString();
			}

			IEnumerable<string> actual()
			{
				return invoiceLine.PeriodApportionment.Lines
					.Cast<PeriodApportionmentLine>()
					.Select(lineSelector);
			}

			AssertContainsExactElementsInAnyOrder(expected, actual());
		}

		[TestDate(2019, 12, 21)]
		public void TestRecalculateApportionmentsAfterChanges()
		{
			var clearingAccPK = testObjectCreator.GLJournalClearingAccount.PK;

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(2021, GlbCompany.CurrentCompany.PK);
			Factory.Save();

			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.USD, 0.64m, testObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.Manual;
			invoiceLine.AL_AG = testObjectCreator.GLHeader1.PK;
			invoiceLine.AL_ExchangeRate = 0.64m;
			invoiceLine.AL_LocalExTaxAmount = 909.09090909;
			invoiceLine.AL_AT = testObjectCreator.GST1.PK;
			invoiceLine.AL_RX_NKTransactionCurrency = "USD";
			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 7, 27);
			invoiceLine.PeriodClearingGLAccountPK = clearingAccPK;

			var expected = new[]
			{
				(202008, 96.97m, 151.52m, "USD",    0.64m,          clearingAccPK,      testObjectCreator.GLHeader1.PK, false).ToString(),
				(202009, 96.97m, 151.52m, "USD",    0.64m,          clearingAccPK,      testObjectCreator.GLHeader1.PK, false).ToString(),
				(202010, 96.97m, 151.52m, "USD",    0.64m,          clearingAccPK,      testObjectCreator.GLHeader1.PK, false).ToString(),
				(202011, 96.97m, 151.52m, "USD",    0.64m,          clearingAccPK,      testObjectCreator.GLHeader1.PK, false).ToString(),
				(202012, 96.97m, 151.52m, "USD",    0.64m,          clearingAccPK,      testObjectCreator.GLHeader1.PK, false).ToString(),
				(202101, 96.97m, 151.49m, "USD",    0.640108m,      clearingAccPK,      testObjectCreator.GLHeader1.PK, true).ToString(),
			};

			string lineSelector(PeriodApportionmentLine x)
			{
				return (x.Period, x.OSAmount, x.LocalAmount, x.CurrencyCode, x.ExchangeRate, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK, x.OSAmountInfo.ReadOnly).ToString();
			}

			IEnumerable<string> actual()
			{
				return invoiceLine.PeriodApportionment.Lines
					.Cast<PeriodApportionmentLine>()
					.Select(lineSelector);
			}

			AssertContainsExactElementsInAnyOrder(expected, actual());

			invoiceLine.PeriodApportionment.Lines[0].OSAmount = 100m;
			invoiceLine.PeriodApportionment.Lines[1].OSAmount = 90m;
			invoiceLine.PeriodApportionment.Lines[2].OSAmount = 80m;

			invoice.AH_ExchangeRate = 0.8m;

			expected = new[]
			{
				(202008, 100m,      125m,       "USD",      0.8m,       clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202009, 90m,       112.5m,     "USD",      0.8m,       clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202010, 80m,       100m,       "USD",      0.8m,       clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202011, 96.97m,    121.21m,    "USD",      0.8m,       clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202012, 96.97m,    121.21m,    "USD",      0.8m,       clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202101, 117.88m,   147.36m,    "USD",      0.799946m,  clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
			};

			AssertContainsExactElementsInAnyOrder("Changing exchange rate should preserve os amounts but update local ones", expected, actual());

			invoiceLine.AL_OSExTaxAmount += 50;

			expected = new[]
			{
				(202008, 100m,      125m,       "USD",      0.8m,       clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202009, 90m,       112.5m,     "USD",      0.8m,       clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202010, 80m,       100m,       "USD",      0.8m,       clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202011, 96.97m,    121.21m,    "USD",      0.8m,       clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202012, 96.97m,    121.21m,    "USD",      0.8m,       clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202101, 167.88m,   209.86m,    "USD",      0.799962m,  clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
			};

			AssertContainsExactElementsInAnyOrder("Changing amount recalculates last line only", expected, actual());
		}

		[TestDate(2019, 12, 21)]
		public void TestNegativeApportionmentsNotAllowed()
		{
			var clearingAccPK = testObjectCreator.GLJournalClearingAccount.PK;
			var chargeCMT = testObjectCreator.CreateChargeCode("CMT1", "CMT Test", Core.Constants.ChargeType.Comment, 1m, null, null);

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(2021, GlbCompany.CurrentCompany.PK);
			Factory.Save();

			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.USD, 0.64m, testObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.Manual;
			invoiceLine.GenericCharge = chargeCMT.PK;
			invoiceLine.AL_AG = testObjectCreator.GLHeader1.PK;
			invoiceLine.AL_ExchangeRate = 0.64m;
			invoiceLine.AL_LocalExTaxAmount = 909.09090909;
			invoiceLine.AL_AT = testObjectCreator.GST1.PK;
			invoiceLine.AL_RX_NKTransactionCurrency = "USD";
			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 7, 27);
			invoiceLine.PeriodClearingGLAccountPK = clearingAccPK;

			IEnumerable<string> actualInvoiceLineErrors() => invoiceLine.GetErrors().Select(x => x.Message);
			AssertContainsExactElementsInAnyOrder(new[] { "Error - AL_OSExTaxAmount: Line amount cannot be set if there is a comment charge entered" }, actualInvoiceLineErrors());

			var lastLine = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Single(x => x.IsLastLine);

			string lineSelector(PeriodApportionmentLine x)
			{
				return (x.Period, x.OSAmount, x.LocalAmount, x.CurrencyCode, x.ExchangeRate, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK, x.OSAmountInfo.ReadOnly).ToString();
			}

			IEnumerable<string> actual()
			{
				return invoiceLine.PeriodApportionment.Lines
					.Cast<PeriodApportionmentLine>()
					.Select(lineSelector);
			}

			var expected = new[]
			{
				(202008, 96.97m, 151.52m, "USD", 0.64m, clearingAccPK,      testObjectCreator.GLHeader1.PK, false).ToString(),
				(202009, 96.97m, 151.52m, "USD", 0.64m, clearingAccPK,      testObjectCreator.GLHeader1.PK, false).ToString(),
				(202010, 96.97m, 151.52m, "USD", 0.64m, clearingAccPK,      testObjectCreator.GLHeader1.PK, false).ToString(),
				(202011, 96.97m, 151.52m, "USD", 0.64m, clearingAccPK,      testObjectCreator.GLHeader1.PK, false).ToString(),
				(202012, 96.97m, 151.52m, "USD", 0.64m, clearingAccPK,      testObjectCreator.GLHeader1.PK, false).ToString(),
				(202101, 96.97m, 151.49m, "USD", 0.640108m, clearingAccPK,  testObjectCreator.GLHeader1.PK, true).ToString(),
			};

			AssertContainsExactElementsInAnyOrder(expected, actual());
			AssertNoErrors(lastLine.OSAmountInfo);
			AssertContainsExactElementsInAnyOrder(new[] { "Error - AL_OSExTaxAmount: Line amount cannot be set if there is a comment charge entered" }, actualInvoiceLineErrors());

			invoiceLine.PeriodApportionment.Lines[0].OSAmount = 1000m;

			expected = new[]
			{
				(202008, 1000m,     1562.5m,    "USD", 0.64m,       clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202009, 96.97m,    151.52m,    "USD", 0.64m,       clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202010, 96.97m,    151.52m,    "USD", 0.64m,       clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202011, 96.97m,    151.52m,    "USD", 0.64m,       clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202012, 96.97m,    151.52m,    "USD", 0.64m,       clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202101, -806.06m,  -1259.49m,  "USD", 0.639989m,   clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
			};

			AssertContainsExactElementsInAnyOrder(expected, actual());
			AssertHasError(lastLine.OSAmountInfo, "Negative apportionments not allowed");
			AssertContainsExactElementsInAnyOrder(new[] { "Error - OSAmount: Negative apportionments not allowed", "Error - AL_OSExTaxAmount: Line amount cannot be set if there is a comment charge entered" }, actualInvoiceLineErrors());
		}

		public void TestInvoiceAmountsIsSplitCorrectly_ByNumberOfDays_OnSingleFinalPeriod()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			Factory.Save();
			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.AUD, 0.64m, testObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();

			invoiceLine.AL_AG = testObjectCreator.GLHeader1.PK;
			invoiceLine.AL_OSExTaxAmount = 581.82;
			invoiceLine.AL_AT = testObjectCreator.GST1.PK;
			invoiceLine.AL_RX_NKTransactionCurrency = "USD";

			invoiceLine.PeriodApportionmentMethod = "DAY";
			invoiceLine.PeriodStartDate = new ZDate(2020, 12, 01);
			AssertNoExceptionThrown("Expect no exception being thrown when set the PeriodEndDate ", () => invoiceLine.PeriodEndDate = new ZDate(2020, 12, 31));
			invoiceLine.PeriodClearingGLAccountPK = testObjectCreator.GLJournalClearingAccount.PK;

			var expected = new[]
			{
					(202012, 581.82m, 909.09m, 31, "USD", 0.640003m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
			};

			var actual = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Select(x => (x.Period, x.OSAmount, x.LocalAmount, x.NumberOfDaysInPeriod, x.CurrencyCode, x.ExchangeRate, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK).ToString());

			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		[TestDate(2019, 12, 21)]
		public void TestInvoiceAmountsIsSplitCorrectlyByNumberOfDays()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(2021, GlbCompany.CurrentCompany.PK);
			Factory.Save();
			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.AUD, 0.64m, testObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();

			invoiceLine.AL_AG = testObjectCreator.GLHeader1.PK;
			invoiceLine.AL_OSExTaxAmount = 581.82;
			invoiceLine.AL_AT = testObjectCreator.GST1.PK;
			invoiceLine.AL_RX_NKTransactionCurrency = "USD";

			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 4, 29);
			invoiceLine.PeriodApportionmentMethod = "DAY";
			invoiceLine.PeriodClearingGLAccountPK = testObjectCreator.GLJournalClearingAccount.PK;

			var expected = new[]
			{
					(202008, 116.36m, 181.81m, 15, "USD", 0.64m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
					(202009, 240.49m, 375.77m, 31, "USD", 0.64m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
					(202010, 224.97m, 351.51m, 29, "USD", 0.640010m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString()
			};

			var actual = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Select(x => (x.Period, x.OSAmount, x.LocalAmount, x.NumberOfDaysInPeriod, x.CurrencyCode, x.ExchangeRate, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK).ToString());

			AssertContainsExactElementsInAnyOrder(expected, actual);

			invoiceLine.PeriodEndDate = new ZDate(2021, 2, 12);
			invoiceLine.PeriodStartDate = new ZDate(2020, 12, 10);

			expected = new[]
			{
					(202106, 196.92m, 307.69m, 22, "USD", 0.64m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
					(202107, 277.48m, 433.56m, 31, "USD", 0.64m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
					(202108, 107.42m, 167.84m, 12, "USD", 0.640014m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString()
			};

			actual = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Select(x => (x.Period, x.OSAmount, x.LocalAmount, x.NumberOfDaysInPeriod, x.CurrencyCode, x.ExchangeRate, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK).ToString());

			AssertContainsExactElementsInAnyOrder(expected, actual);

			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 2, 29);

			Assert("There should only be one period apportionment line.", invoiceLine.PeriodApportionmentLines.Count == 1);

			expected = new[]
			{
				(202008, 581.82m, 909.09m, 15, "USD", 0.640003m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString()
			};

			actual = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Select(x => (x.Period, x.OSAmount, x.LocalAmount, x.NumberOfDaysInPeriod, x.CurrencyCode, x.ExchangeRate, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK).ToString());

			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		[TestDate(2019, 12, 21)]
		public void TestInvoiceAmountsAndDaysCorrectlyUpdatesWhenMethodIsManual()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(2021, GlbCompany.CurrentCompany.PK);
			Factory.Save();
			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.AUD, 0.64m, testObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();

			invoiceLine.AL_AG = testObjectCreator.GLHeader1.PK;
			invoiceLine.AL_OSExTaxAmount = 581.82;
			invoiceLine.AL_AT = testObjectCreator.GST1.PK;
			invoiceLine.AL_RX_NKTransactionCurrency = "USD";

			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 4, 29);

			invoiceLine.PeriodApportionmentMethod = "DAY";
			invoiceLine.PeriodClearingGLAccountPK = testObjectCreator.GLJournalClearingAccount.PK;
			invoiceLine.PeriodApportionmentMethod = "MAN";

			invoiceLine.PeriodApportionmentLines[0].OSAmount = 110.40m;
			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 2);

			var originalExpected = new[]
			{
					(202008, 110.40m, 172.5m, 28, "USD", 0.64m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
					(202009, 240.49m, 375.77m, 31, "USD", 0.64m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
					(202010, 230.93m, 360.82m, 29, "USD", 0.640014m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString()
			};

			var actual = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Select(x => (x.Period, x.OSAmount, x.LocalAmount, x.NumberOfDaysInPeriod, x.CurrencyCode, x.ExchangeRate, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK).ToString());

			AssertContainsExactElementsInAnyOrder(originalExpected, actual);

			invoiceLine.PeriodStartDate = new ZDate(2020, 1, 2);
			invoiceLine.PeriodEndDate = new ZDate(2020, 4, 29);

			var newExpected = new[]
			{
					(202007, 146.68m, 229.19m, 30, "USD", 0.64m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
					(202008, 141.79m, 221.55m, 29, "USD", 0.64m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
					(202009, 151.57m, 236.83m, 31, "USD", 0.64m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
					(202010, 141.78m, 221.52m, 29, "USD", 0.640033m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString()
			};

			actual = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Select(x => (x.Period, x.OSAmount, x.LocalAmount, x.NumberOfDaysInPeriod, x.CurrencyCode, x.ExchangeRate, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK).ToString());

			AssertContainsExactElementsInAnyOrder(newExpected, actual);
		}

		[TestDate(2019, 12, 21)]
		public void TestWhenChangingInvoiceLineDate_DaysAndAmountsAdjust()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(2021, GlbCompany.CurrentCompany.PK);
			Factory.Save();
			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.AUD, 0.64m, testObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();

			invoiceLine.AL_AG = testObjectCreator.GLHeader1.PK;
			invoiceLine.AL_OSExTaxAmount = 581.82;
			invoiceLine.AL_AT = testObjectCreator.GST1.PK;
			invoiceLine.AL_RX_NKTransactionCurrency = "USD";

			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 4, 29);
			invoiceLine.PeriodApportionmentMethod = "DAY";
			invoiceLine.PeriodClearingGLAccountPK = testObjectCreator.GLJournalClearingAccount.PK;

			var originalExpected = new[]
			{
					(202008, 116.36m, 181.81m, 15, "USD", 0.64m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
					(202009, 240.49m, 375.77m, 31, "USD", 0.64m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
					(202010, 224.97m, 351.51m, 29, "USD", 0.640010m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString()
			};

			var actual = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Select(x => (x.Period, x.OSAmount, x.LocalAmount, x.NumberOfDaysInPeriod, x.CurrencyCode, x.ExchangeRate, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK).ToString());

			AssertContainsExactElementsInAnyOrder(originalExpected, actual);

			invoiceLine.PeriodEndDate = new ZDate(2020, 3, 29);

			var newExpected = new[]
			{
					(202008, 198.35m, 309.92m, 15, "USD", 0.64m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
					(202009, 383.47m, 599.17m, 29, "USD", 0.640002m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString()
			};

			actual = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Select(x => (x.Period, x.OSAmount, x.LocalAmount, x.NumberOfDaysInPeriod, x.CurrencyCode, x.ExchangeRate, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK).ToString());

			AssertContainsExactElementsInAnyOrder(newExpected, actual);
		}

		[TestDate(2019, 12, 21)]
		public void TestAppportionmentLineAmountsBalance()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(2021, GlbCompany.CurrentCompany.PK);
			Factory.Save();
			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.AUD, 0.64m, testObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();

			invoiceLine.AL_AG = testObjectCreator.GLHeader1.PK;
			invoiceLine.AL_OSExTaxAmount = 581.82;
			invoiceLine.AL_AT = testObjectCreator.GST1.PK;
			invoiceLine.AL_RX_NKTransactionCurrency = "USD";

			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 4, 29);
			invoiceLine.PeriodClearingGLAccountPK = testObjectCreator.GLJournalClearingAccount.PK;

			Assert(invoiceLine.PeriodApportionmentMethodsList.OfType<CodeDescriptionPair>().Any(x => x.Code != PeriodApportionmentMethods.Codes.Default));

			foreach (CodeDescriptionPair method in invoiceLine.PeriodApportionmentMethodsList)
			{
				if (method.Code != "DEF")
				{
					invoiceLine.PeriodApportionmentMethod = method.Code;
					var totalLocalLineAmount = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Sum(x => x.LocalAmount);
					var totalOSLineAmount = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Sum(x => x.OSAmount);

					CombineAssertions(() =>
					{
						AssertEquals("When apportionment method equals " + method.Code + ", sum of apportionment line OS amounts must = invoice line OS amounts", invoiceLine.AL_OSExTaxAmount, totalOSLineAmount);
						AssertEquals("When apportionment method equals " + method.Code + ", sum of apportionment line local amounts must = invoice line local amounts", invoiceLine.AL_LocalExTaxAmount, totalLocalLineAmount);
					});
				}
			}
		}

		// Test for Apportionment Tax Not Recoverable 
		[TestDate(2020, 3, 27)]
		public void TestManualPeriodApportionmentEUR()
		{
			var clearingAccPK = testObjectCreator.GLJournalClearingAccount.PK;

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(2021, GlbCompany.CurrentCompany.PK);
			Factory.Save();

			var creditor1 = testObjectCreator.Creditor1;
			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.EUR, 1m, creditor1);

			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();

			Assert("Precondition", invoiceLine.PeriodApportionment.Lines.IsNullOrEmpty());

			AccGLHeader testObjectAccGLHeader = testObjectCreator.GLHeader1;

			invoiceLine.AL_AG = testObjectAccGLHeader.PK;
			invoiceLine.AL_ExchangeRate = 1.00m;
			invoiceLine.AL_LocalExTaxAmount = 909.09090909;

			AccTaxRate testObjectAccTaxRate = testObjectCreator.GST1;
			testObjectAccTaxRate.SetRate_ForTestOnly(2200, 100);
			invoiceLine.AL_AT = testObjectAccTaxRate.PK;
			invoiceLine.AL_RX_NKTransactionCurrency = "EUR";
			Assert(invoiceLine.PeriodApportionment.Lines.IsNullOrEmpty());
			invoiceLine.AL_Calc_InputGSTVATRecoverablePercentage = 60.00m;

			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.EquallyOverPeriods;
			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 7, 27);

			invoiceLine.PeriodClearingGLAccountPK = clearingAccPK;

			var expected = new[]
			{
				(202008, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
				(202009, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
				(202010, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
				(202011, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
				(202012, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
				(202101, 151.49m, 151.49m, "EUR", 1, 13.35m, 13.35m, clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
			};

			string lineSelector(PeriodApportionmentLine x)
			{
				return (x.Period, x.OSAmount, x.LocalAmount, x.CurrencyCode, x.ExchangeRate, x.OSTaxNotRecoverable, x.LocalTaxNotRecoverable, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK, x.OSAmountInfo.ReadOnly).ToString();
			}

			IEnumerable<string> actual()
			{
				return invoiceLine.PeriodApportionment.Lines
					.Cast<PeriodApportionmentLine>()
					.Select(lineSelector);
			}

			AssertContainsExactElementsInAnyOrder("Test message", expected, actual());
			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.Manual;

			expected = new[]
			{
				(202008, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202009, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202010, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202011, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202012, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202101, 151.49m, 151.49m, "EUR", 1, 13.35m, 13.35m, clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
			};

			AssertContainsExactElementsInAnyOrder(expected, actual());

			invoiceLine.PeriodApportionment.Lines[0].OSAmount = 100m;
			invoiceLine.PeriodApportionment.Lines[1].OSAmount = 90m;
			invoiceLine.PeriodApportionment.Lines[2].OSAmount = 80m;
			invoiceLine.PeriodApportionment.Lines[0].OSTaxNotRecoverable = 9.80m;

			expected = new[]
			{
				(202008, 100m,     100m,    "EUR", 1,  9.80m,  9.80m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202009,  90m,      90m,    "EUR", 1,  7.92m,  7.92m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202010,  80m,      80m,    "EUR", 1,  7.04m,  7.04m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202011, 151.52m,  151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202012, 151.52m,  151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202101, 336.05m,  336.05m, "EUR", 1, 28.58m, 28.58m, clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
			};

			AssertContainsExactElementsInAnyOrder(expected, actual());

			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 7, 28);

			AssertContainsExactElementsInAnyOrder("even dates changed, periods count hasn't so everything stays same", expected, actual());

			invoiceLine.PeriodEndDate = new ZDate(2020, 8, 28);

			expected = new[]
			{
				(202008, 129.87m, 129.87m, "EUR", 1, 11.43m, 11.43m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202009, 129.87m, 129.87m, "EUR", 1, 11.43m, 11.43m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202010, 129.87m, 129.87m, "EUR", 1, 11.43m, 11.43m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202011, 129.87m, 129.87m, "EUR", 1, 11.43m, 11.43m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202012, 129.87m, 129.87m, "EUR", 1, 11.43m, 11.43m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202101, 129.87m, 129.87m, "EUR", 1, 11.43m, 11.43m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202102, 129.87m, 129.87m, "EUR", 1, 11.42m, 11.42m, clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
			};

			AssertContainsExactElementsInAnyOrder("More periods should trigger recalculation using last non-manual non-default method", expected, actual());

			invoiceLine.AL_Calc_InputGSTVATRecoverablePercentage = 50.00m;

			expected = new[]
			{
				(202008, 129.87m, 129.87m, "EUR", 1, 11.43m, 11.43m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202009, 129.87m, 129.87m, "EUR", 1, 11.43m, 11.43m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202010, 129.87m, 129.87m, "EUR", 1, 11.43m, 11.43m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202011, 129.87m, 129.87m, "EUR", 1, 11.43m, 11.43m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202012, 129.87m, 129.87m, "EUR", 1, 11.43m, 11.43m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202101, 129.87m, 129.87m, "EUR", 1, 11.43m, 11.43m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202102, 129.87m, 129.87m, "EUR", 1, 31.42m, 31.42m, clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
			};

			AssertContainsExactElementsInAnyOrder("More periods should trigger recalculation using last non-manual non-default method", expected, actual());

			Factory.Save();
		}

		[TestDate(2020, 03, 30)]
		public void TestEquallyOverPeriodPeriodsApportionmentEUR()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			Factory.Save();

			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.EUR, 1m, testObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			Assert("Precondition", invoiceLine.PeriodApportionmentLines.IsNullOrEmpty());
			invoiceLine.AL_AG = testObjectCreator.GLHeader1.PK;
			invoiceLine.AL_OSExTaxAmount = 100m;
			invoiceLine.AL_ExchangeRate = 1.00m;
			AccTaxRate testObjectAccTaxRate = testObjectCreator.GST1;
			testObjectAccTaxRate.SetRate_ForTestOnly(2200, 100);
			invoiceLine.AL_AT = testObjectAccTaxRate.PK;
			invoiceLine.AL_RX_NKTransactionCurrency = "EUR";
			Assert(invoiceLine.PeriodApportionment.Lines.IsNullOrEmpty());
			invoiceLine.AL_Calc_InputGSTVATRecoverablePercentage = 60.00m;

			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.EquallyOverPeriods;
			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 4, 29);
			invoiceLine.PeriodClearingGLAccountPK = testObjectCreator.GLJournalClearingAccount.PK;

			var expected = new[]
			{
				(202008, 33.33m, 33.33m, "EUR", 1, 2.93m, 2.93m , testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
				(202009, 33.33m, 33.33m, "EUR", 1, 2.93m, 2.93m , testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
				(202010, 33.34m, 33.34m, "EUR", 1, 2.94m, 2.94m , testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
			};

			var actual = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Select(x => (x.Period, x.OSAmount, x.LocalAmount, x.CurrencyCode, x.ExchangeRate, x.OSTaxNotRecoverable, x.LocalTaxNotRecoverable, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK).ToString());
			AssertContainsExactElementsInAnyOrder(expected, actual);

			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.Default;
			Assert(invoiceLine.PeriodApportionmentLines.IsNullOrEmpty());
		}

		[TestDate(2020, 03, 30)]
		public void TestTestEquallyOverPeriodPeriodsApportionment2EUR()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(2021, GlbCompany.CurrentCompany.PK);
			Factory.Save();

			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.EUR, 1m, testObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			Assert("Precondition", invoiceLine.PeriodApportionmentLines.IsNullOrEmpty());
			invoiceLine.AL_AG = testObjectCreator.GLHeader1.PK;
			invoiceLine.AL_LocalExTaxAmount = 909.09090909;
			invoiceLine.AL_ExchangeRate = 1.00m;
			AccTaxRate testObjectAccTaxRate = testObjectCreator.GST1;
			testObjectAccTaxRate.SetRate_ForTestOnly(2200, 100);
			invoiceLine.AL_AT = testObjectAccTaxRate.PK;
			invoiceLine.AL_RX_NKTransactionCurrency = "EUR";
			Assert(invoiceLine.PeriodApportionment.Lines.IsNullOrEmpty());
			invoiceLine.AL_Calc_InputGSTVATRecoverablePercentage = 60.00m;

			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 7, 29);
			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.EquallyOverPeriods;
			invoiceLine.PeriodClearingGLAccountPK = testObjectCreator.GLJournalClearingAccount.PK;

			var expected = new[]
			{
				(202008, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
				(202009, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
				(202010, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
				(202011, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
				(202012, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
				(202101, 151.49m, 151.49m, "EUR", 1, 13.35m, 13.35m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
			};

			var actual = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Select(x => (x.Period, x.OSAmount, x.LocalAmount, x.CurrencyCode, x.ExchangeRate, x.OSTaxNotRecoverable, x.LocalTaxNotRecoverable, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK).ToString());
			AssertContainsExactElementsInAnyOrder(expected, actual);

			invoiceLine.AL_Calc_InputGSTVATRecoverablePercentage = 50.00m;

			expected = new[]
			{
				(202008, 151.52m, 151.52m, "EUR", 1, 16.67m, 16.67m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
				(202009, 151.52m, 151.52m, "EUR", 1, 16.67m, 16.67m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
				(202010, 151.52m, 151.52m, "EUR", 1, 16.67m, 16.67m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
				(202011, 151.52m, 151.52m, "EUR", 1, 16.67m, 16.67m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
				(202012, 151.52m, 151.52m, "EUR", 1, 16.67m, 16.67m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
				(202101, 151.49m, 151.49m, "EUR", 1, 16.65m, 16.65m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
			};

			actual = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Select(x => (x.Period, x.OSAmount, x.LocalAmount, x.CurrencyCode, x.ExchangeRate, x.OSTaxNotRecoverable, x.LocalTaxNotRecoverable, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK).ToString());
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		[TestDate(2020, 03, 30)]
		public void TestSettingMANFirstTriggersPeriodsCalculatedEUR()
		{
			var clearingAccPK = testObjectCreator.GLJournalClearingAccount.PK;

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(2021, GlbCompany.CurrentCompany.PK);
			Factory.Save();

			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.EUR, 1m, testObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.Manual;
			invoiceLine.AL_AG = testObjectCreator.GLHeader1.PK;
			invoiceLine.AL_LocalExTaxAmount = 909.09090909;
			invoiceLine.AL_ExchangeRate = 1.00m;
			AccTaxRate testObjectAccTaxRate = testObjectCreator.GST1;
			testObjectAccTaxRate.SetRate_ForTestOnly(2200, 100);
			invoiceLine.AL_AT = testObjectAccTaxRate.PK;
			invoiceLine.AL_RX_NKTransactionCurrency = "EUR";

			Assert(invoiceLine.PeriodApportionment.Lines.IsNullOrEmpty());

			invoiceLine.AL_Calc_InputGSTVATRecoverablePercentage = 60.00m;

			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 7, 27);
			invoiceLine.PeriodClearingGLAccountPK = clearingAccPK;

			var expected = new[]
			{
				(202008, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202009, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202010, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202011, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202012, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202101, 151.49m, 151.49m, "EUR", 1, 13.35m, 13.35m, clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
			};

			string lineSelector(PeriodApportionmentLine x)
			{
				return (x.Period, x.OSAmount, x.LocalAmount, x.CurrencyCode, x.ExchangeRate, x.OSTaxNotRecoverable, x.LocalTaxNotRecoverable, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK, x.OSAmountInfo.ReadOnly).ToString();
			}

			IEnumerable<string> actual()
			{
				return invoiceLine.PeriodApportionment.Lines
					.Cast<PeriodApportionmentLine>()
					.Select(lineSelector);
			}

			AssertContainsExactElementsInAnyOrder(expected, actual());
		}

		[TestDate(2020, 03, 31)]
		public void TestNegativeApportionmentsNotAllowedEUR()
		{
			var clearingAccPK = testObjectCreator.GLJournalClearingAccount.PK;

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(2021, GlbCompany.CurrentCompany.PK);
			Factory.Save();

			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.EUR, 1m, testObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.Manual;

			invoiceLine.AL_AG = testObjectCreator.GLHeader1.PK;
			invoiceLine.AL_LocalExTaxAmount = 909.09090909;
			invoiceLine.AL_ExchangeRate = 1.00m;
			AccTaxRate testObjectAccTaxRate = testObjectCreator.GST1;
			testObjectAccTaxRate.SetRate_ForTestOnly(2200, 100);
			invoiceLine.AL_AT = testObjectAccTaxRate.PK;
			invoiceLine.AL_RX_NKTransactionCurrency = "EUR";
			invoiceLine.AL_Calc_InputGSTVATRecoverablePercentage = 60.00m;

			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 7, 27);
			invoiceLine.PeriodClearingGLAccountPK = clearingAccPK;

			var expected = new[]
			{
				(202008, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202009, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202010, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202011, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202012, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202101, 151.49m, 151.49m, "EUR", 1, 13.35m, 13.35m, clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
			};

			var lastLine = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Single(x => x.IsLastLine);

			string lineSelector(PeriodApportionmentLine x)
			{
				return (x.Period, x.OSAmount, x.LocalAmount, x.CurrencyCode, x.ExchangeRate, x.OSTaxNotRecoverable, x.LocalTaxNotRecoverable, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK, x.OSAmountInfo.ReadOnly).ToString();
			}

			IEnumerable<string> actual()
			{
				return invoiceLine.PeriodApportionment.Lines
					.Cast<PeriodApportionmentLine>()
					.Select(lineSelector);
			}

			AssertContainsExactElementsInAnyOrder(expected, actual());

			invoiceLine.PeriodApportionment.Lines[0].OSTaxNotRecoverable = 33.33m;

			expected = new[]
			{
				(202008, 151.52m, 151.52m, "EUR", 1, 33.33m, 33.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202009, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202010, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202011, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202012, 151.52m, 151.52m, "EUR", 1, 13.33m, 13.33m, clearingAccPK, testObjectCreator.GLHeader1.PK, false).ToString(),
				(202101, 151.49m, 151.49m, "EUR", 1, -6.65m, -6.65m, clearingAccPK, testObjectCreator.GLHeader1.PK, true).ToString(),
			};

			AssertContainsExactElementsInAnyOrder(expected, actual());
			AssertHasError(lastLine.OSTaxNotRecoverableInfo, "Negative apportionments of the taxes not allowed");
		}

		[TestDate(2020, 03, 31)]
		public void TestInvoiceAmountsIsSplitCorrectlyByNumberOfDaysEUR()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(2021, GlbCompany.CurrentCompany.PK);
			Factory.Save();
			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.EUR, 1m, testObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();

			invoiceLine.AL_AG = testObjectCreator.GLHeader1.PK;
			invoiceLine.AL_OSExTaxAmount = 581.82;
			AccTaxRate testObjectAccTaxRate = testObjectCreator.GST1;
			testObjectAccTaxRate.SetRate_ForTestOnly(2200, 100);
			invoiceLine.AL_AT = testObjectAccTaxRate.PK;
			invoiceLine.AL_RX_NKTransactionCurrency = "EUR";
			invoiceLine.AL_Calc_InputGSTVATRecoverablePercentage = 60.00m;

			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 4, 29);
			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.Day;
			invoiceLine.PeriodClearingGLAccountPK = testObjectCreator.GLJournalClearingAccount.PK;

			var expected = new[]
			{
					(202008, 116.36m, 116.36m, 15, "EUR", 1, 10.24m, 10.24m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
					(202009, 240.49m, 240.49m, 31, "EUR", 1, 21.16m, 21.16m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
					(202010, 224.97m, 224.97m, 29, "EUR", 1, 19.80m, 19.80m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString()
			};

			var actual = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Select(x => (x.Period, x.OSAmount, x.LocalAmount, x.NumberOfDaysInPeriod, x.CurrencyCode, x.ExchangeRate, x.OSTaxNotRecoverable, x.LocalTaxNotRecoverable, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK).ToString());

			AssertContainsExactElementsInAnyOrder(expected, actual);

			invoiceLine.PeriodStartDate = new ZDate(2020, 12, 10);
			invoiceLine.PeriodEndDate = new ZDate(2021, 2, 12);

			expected = new[]
			{
					(202106, 196.92m, 196.92m, 22, "EUR", 1, 17.33m, 17.33m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
					(202107, 277.48m, 277.48m, 31, "EUR", 1, 24.42m, 24.42m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
					(202108, 107.42m, 107.42m, 12, "EUR", 1, 9.45m, 9.45m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString()
			};

			actual = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Select(x => (x.Period, x.OSAmount, x.LocalAmount, x.NumberOfDaysInPeriod, x.CurrencyCode, x.ExchangeRate, x.OSTaxNotRecoverable, x.LocalTaxNotRecoverable, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK).ToString());

			AssertContainsExactElementsInAnyOrder(expected, actual);

			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 2, 29);

			Assert("There should only be one period apportionment line.", invoiceLine.PeriodApportionmentLines.Count == 1);

			expected = new[]
			{
				(202008, 581.82m, 581.82m, 15, "EUR", 1, 51.20m, 51.20m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString()
			};

			actual = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Select(x => (x.Period, x.OSAmount, x.LocalAmount, x.NumberOfDaysInPeriod, x.CurrencyCode, x.ExchangeRate, x.OSTaxNotRecoverable, x.LocalTaxNotRecoverable, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK).ToString());

			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		[TestDate(2020, 03, 31)]
		public void TestInvoiceAmountsAndDaysCorrectlyUpdatesWhenMethodIsManualEUR()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(2021, GlbCompany.CurrentCompany.PK);
			Factory.Save();
			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.EUR, 1m, testObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();

			invoiceLine.AL_AG = testObjectCreator.GLHeader1.PK;
			invoiceLine.AL_OSExTaxAmount = 581.82;
			invoiceLine.AL_ExchangeRate = 1.00m;
			AccTaxRate testObjectAccTaxRate = testObjectCreator.GST1;
			testObjectAccTaxRate.SetRate_ForTestOnly(2200, 100);
			invoiceLine.AL_AT = testObjectAccTaxRate.PK;
			invoiceLine.AL_RX_NKTransactionCurrency = "EUR";
			invoiceLine.AL_Calc_InputGSTVATRecoverablePercentage = 60.00m;

			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 4, 29);

			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.Day;
			invoiceLine.PeriodClearingGLAccountPK = testObjectCreator.GLJournalClearingAccount.PK;
			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.Manual;

			invoiceLine.PeriodApportionmentLines[0].OSAmount = 110.40m;
			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 2);

			var originalExpected = new[]
			{
					(202008, 110.40m, 110.40m, 28, "EUR", 1,  9.72m,  9.72m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
					(202009, 240.49m, 240.49m, 31, "EUR", 1, 21.16m, 21.16m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
					(202010, 230.93m, 230.93m, 29, "EUR", 1, 20.32m, 20.32m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString()
			};

			var actual = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Select(x => (x.Period, x.OSAmount, x.LocalAmount, x.NumberOfDaysInPeriod, x.CurrencyCode, x.ExchangeRate, x.OSTaxNotRecoverable, x.LocalTaxNotRecoverable, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK).ToString());

			AssertContainsExactElementsInAnyOrder(originalExpected, actual);

			invoiceLine.PeriodStartDate = new ZDate(2020, 1, 2);
			invoiceLine.PeriodEndDate = new ZDate(2020, 4, 29);

			var newExpected = new[]
			{
					(202007, 146.68m, 146.68m, 30, "EUR", 1,  12.91m,  12.91m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
					(202008, 141.79m, 141.79m, 29, "EUR", 1,  12.48m,  12.48m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
					(202009, 151.57m, 151.57m, 31, "EUR", 1,  13.34m,  13.34m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
					(202010, 141.78m, 141.78m, 29, "EUR", 1,  12.47m,  12.47m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString()
			};

			actual = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Select(x => (x.Period, x.OSAmount, x.LocalAmount, x.NumberOfDaysInPeriod, x.CurrencyCode, x.ExchangeRate, x.OSTaxNotRecoverable, x.LocalTaxNotRecoverable, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK).ToString());

			AssertContainsExactElementsInAnyOrder(newExpected, actual);
		}

		[TestDate(2020, 03, 31)]
		public void TestWhenChangingInvoiceLineDate_DaysAndAmountsAdjustEUR()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(2021, GlbCompany.CurrentCompany.PK);
			Factory.Save();
			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.EUR, 1m, testObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();

			invoiceLine.AL_AG = testObjectCreator.GLHeader1.PK;
			invoiceLine.AL_OSExTaxAmount = 581.82;
			invoiceLine.AL_ExchangeRate = 1.00m;
			AccTaxRate testObjectAccTaxRate = testObjectCreator.GST1;
			testObjectAccTaxRate.SetRate_ForTestOnly(2200, 100);
			invoiceLine.AL_AT = testObjectAccTaxRate.PK;
			invoiceLine.AL_RX_NKTransactionCurrency = "EUR";
			invoiceLine.AL_Calc_InputGSTVATRecoverablePercentage = 60.00m;

			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 4, 29);
			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.Day;
			invoiceLine.PeriodClearingGLAccountPK = testObjectCreator.GLJournalClearingAccount.PK;

			var originalExpected = new[]
			{
					(202008, 116.36m, 116.36m, 15, "EUR", 1, 10.24m, 10.24m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
					(202009, 240.49m, 240.49m, 31, "EUR", 1, 21.16m, 21.16m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
					(202010, 224.97m, 224.97m, 29, "EUR", 1, 19.80m, 19.80m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString()
			};

			var actual = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Select(x => (x.Period, x.OSAmount, x.LocalAmount, x.NumberOfDaysInPeriod, x.CurrencyCode, x.ExchangeRate, x.OSTaxNotRecoverable, x.LocalTaxNotRecoverable, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK).ToString());

			AssertContainsExactElementsInAnyOrder(originalExpected, actual);

			invoiceLine.PeriodEndDate = new ZDate(2020, 3, 29);

			var newExpected = new[]
			{
					(202008, 198.35m, 198.35m, 15, "EUR", 1, 17.45m, 17.45m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString(),
					(202009, 383.47m, 383.47m, 29, "EUR", 1, 33.75m, 33.75m, testObjectCreator.GLJournalClearingAccount.PK, testObjectCreator.GLHeader1.PK).ToString()
			};

			actual = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Select(x => (x.Period, x.OSAmount, x.LocalAmount, x.NumberOfDaysInPeriod, x.CurrencyCode, x.ExchangeRate, x.OSTaxNotRecoverable, x.LocalTaxNotRecoverable, x.PeriodClearingGLAccount.PK, x.ControlAccount.PK).ToString());

			AssertContainsExactElementsInAnyOrder(newExpected, actual);
		}

		[TestDate(2020, 03, 31)]
		public void TestAppportionmentLineAmountsBalanceEUR()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(2021, GlbCompany.CurrentCompany.PK);
			Factory.Save();
			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.EUR, 1m, testObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();

			invoiceLine.AL_AG = testObjectCreator.GLHeader1.PK;
			invoiceLine.AL_OSExTaxAmount = 581.82;
			invoiceLine.AL_ExchangeRate = 1.00m;
			AccTaxRate testObjectAccTaxRate = testObjectCreator.GST1;
			testObjectAccTaxRate.SetRate_ForTestOnly(2200, 100);
			invoiceLine.AL_AT = testObjectAccTaxRate.PK;
			invoiceLine.AL_RX_NKTransactionCurrency = "EUR";
			invoiceLine.AL_Calc_InputGSTVATRecoverablePercentage = 60.00m;

			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 4, 29);
			invoiceLine.PeriodClearingGLAccountPK = testObjectCreator.GLJournalClearingAccount.PK;

			Assert(invoiceLine.PeriodApportionmentMethodsList.OfType<CodeDescriptionPair>().Any(x => x.Code != PeriodApportionmentMethods.Codes.Default));

			foreach (CodeDescriptionPair method in invoiceLine.PeriodApportionmentMethodsList)
			{
				if (method.Code != PeriodApportionmentMethods.Codes.Default)
				{
					invoiceLine.PeriodApportionmentMethod = method.Code;
					var totalLocalLineAmount = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Sum(x => x.LocalAmount);
					var totalOSLineAmount = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Sum(x => x.OSAmount);
					var totaLocalTaxLineAmount = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Sum(x => x.LocalTaxNotRecoverable);
					var totalOSTaxLineAmount = invoiceLine.PeriodApportionmentLines.Cast<PeriodApportionmentLine>().Sum(x => x.OSTaxNotRecoverable);

					CombineAssertions(() =>
					{
						AssertEquals("When apportionment method equals " + method.Code + ", sum of apportionment line OS amounts must = invoice line OS amounts", invoiceLine.AL_OSExTaxAmount, totalOSLineAmount);
						AssertEquals("When apportionment method equals " + method.Code + ", sum of apportionment line local amounts must = invoice line local amounts", invoiceLine.AL_LocalExTaxAmount, totalLocalLineAmount);
						AssertEquals("When apportionment method equals " + method.Code + ", sum of apportionment line OS TAX amounts must = invoice line OS TAX amounts", invoiceLine.AL_OSTaxAmount_NotRecoverable, totalOSTaxLineAmount);
						AssertEquals("When apportionment method equals " + method.Code + ", sum of apportionment line local TAX amounts must = invoice line local TAX amounts", invoiceLine.AL_LocalTaxAmount_NotRecoverable, totaLocalTaxLineAmount);
					});
				}
			}
		}
	}
}
