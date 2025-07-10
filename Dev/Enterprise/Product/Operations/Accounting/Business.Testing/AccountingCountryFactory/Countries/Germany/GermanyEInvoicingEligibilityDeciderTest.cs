using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class GermanyEInvoicingEligibilityDeciderTest : EInvoicingEligibilityDeciderTestBase
	{
		protected override string CountryCode => CountryCodes.Germany;

		protected override string ExpectedAdditionalTraceLog => "Transaction category: GOV, Debtor country: DE, Enabled features: ";

		protected override FakeEligibilityLiteTransaction CreateEligibleTransaction()
			=> new()
			{
				CountryCode = CountryCodes.Germany,
				Ledger = LedgerTypes.AccountsReceivable,
				TransactionType = TransactionTypes.Invoice,
				InvoiceOrgAddressOverride = new FakeEligibilityLiteOrgAddress() { CountryCode = CountryCodes.Germany },
				OrgHeader = new FakeEligibilityLiteOrgHeader().WithCategory(OrgConstants.Category.Government),
				Lines = new FakeEligibilityLiteTransactionLine[] { new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.Rated, TaxRate = 10 } },
				InvoiceDate = new ZDateTime(2024, 6, 1),
				InvoiceAmount = 26000
			};

		public void TestIsTransactionEligibleForB2G()
		{
			var testCases = new IsTransactionEligibleTestCase[]
			{
				new IsTransactionEligibleTestCase(true, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, OrgConstants.Category.Government, CountryCodes.Germany),
				new IsTransactionEligibleTestCase(true, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, OrgConstants.Category.Government, CountryCodes.Germany),

				new IsTransactionEligibleTestCase(false, LedgerTypes.AccountsPayable,    TransactionTypes.Invoice,        OrgConstants.Category.Government,                CountryCodes.Germany),
				new IsTransactionEligibleTestCase(false, LedgerTypes.AccountsReceivable, TransactionTypes.AdjustmentNote, OrgConstants.Category.Government,                CountryCodes.Germany),
				new IsTransactionEligibleTestCase(false, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice,        OrgConstants.Category.NonGovernmentOrganisation, CountryCodes.Germany),
				new IsTransactionEligibleTestCase(false, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice,        OrgConstants.Category.Government,                CountryCodes.Australia),
			};
			RunIsTransactionEligibleTestCases("B2G disabled with B2B disabled", testCases, enableB2B: false, enableB2GinXT: false);
			RunIsTransactionEligibleTestCases("B2G disabled with B2B enabled", testCases, enableB2B: true, enableB2GinXT: false);
			RunIsTransactionEligibleTestCases("B2G enabled with B2B disabled", testCases, enableB2B: false, enableB2GinXT: true);
			RunIsTransactionEligibleTestCases("B2G enabled with B2B enabled", testCases, enableB2B: true, enableB2GinXT: true);
		}

		public void TestIsTransactionEligibleForB2BWithB2BEnabled()
		{
			var testCases = new IsTransactionEligibleTestCase[]
			{
				new IsTransactionEligibleTestCase(true, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, OrgConstants.Category.Business, CountryCodes.Germany),
				new IsTransactionEligibleTestCase(true, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, OrgConstants.Category.Business, CountryCodes.Germany),
				new IsTransactionEligibleTestCase(true, LedgerTypes.AccountsReceivable, TransactionTypes.AdjustmentNote, OrgConstants.Category.Business, CountryCodes.Germany),

				new IsTransactionEligibleTestCase(false, LedgerTypes.AccountsPayable,    TransactionTypes.Invoice, OrgConstants.Category.Business,                  CountryCodes.Germany),
				new IsTransactionEligibleTestCase(false, LedgerTypes.AccountsReceivable, TransactionTypes.Payment, OrgConstants.Category.Business,                  CountryCodes.Germany),
				new IsTransactionEligibleTestCase(false, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, OrgConstants.Category.NonGovernmentOrganisation, CountryCodes.Germany),
				new IsTransactionEligibleTestCase(false, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, OrgConstants.Category.Business,                  CountryCodes.Australia),
			};
			RunIsTransactionEligibleTestCases("B2B with B2B enabled", testCases, enableB2B: true);
		}

		public void TestIsTransactionEligibleForB2BWithB2BDisabled()
		{
			var testCases = new IsTransactionEligibleTestCase[]
			{
				new IsTransactionEligibleTestCase(false, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, OrgConstants.Category.Business, CountryCodes.Germany),
				new IsTransactionEligibleTestCase(false, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, OrgConstants.Category.Business, CountryCodes.Germany),
				new IsTransactionEligibleTestCase(false, LedgerTypes.AccountsReceivable, TransactionTypes.AdjustmentNote, OrgConstants.Category.Business, CountryCodes.Germany),

				new IsTransactionEligibleTestCase(false, LedgerTypes.AccountsPayable,    TransactionTypes.Invoice, OrgConstants.Category.Business,                  CountryCodes.Germany),
				new IsTransactionEligibleTestCase(false, LedgerTypes.AccountsReceivable, TransactionTypes.Payment, OrgConstants.Category.Business,                  CountryCodes.Germany),
				new IsTransactionEligibleTestCase(false, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, OrgConstants.Category.NonGovernmentOrganisation, CountryCodes.Germany),
				new IsTransactionEligibleTestCase(false, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, OrgConstants.Category.Business,                  CountryCodes.Australia),
			};
			RunIsTransactionEligibleTestCases("B2B with B2B disabled", testCases, enableB2B: false);
		}

		public void TestGetAdditionalTraceLogAdditionalCasesWithoutB2B()
		{
			var testCases = new IsTransactionEligibleTestCase[]
			{
				new IsTransactionEligibleTestCase(false, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, OrgConstants.Category.Business, CountryCodes.Germany,
					expectedTraceLog: "Transaction category: BUS, Debtor country: DE, Enabled features: "),
				new IsTransactionEligibleTestCase(false, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, OrgConstants.Category.Government, CountryCodes.Austria,
					expectedTraceLog: "Transaction category: GOV, Debtor country: AT, Enabled features: "),
			};
			RunIsTransactionEligibleTestCases("GetAdditionalTraceLog tests, B2B disabled", testCases, enableB2B: false);
		}

		public void TestGetAdditionalTraceLogAdditionalCasesWithB2B()
		{
			var testCases = new IsTransactionEligibleTestCase[]
			{
				new IsTransactionEligibleTestCase(true, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, OrgConstants.Category.Business, CountryCodes.Germany,
					expectedTraceLog: "Transaction category: BUS, Debtor country: DE, Enabled features: B2B"),
				new IsTransactionEligibleTestCase(false, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, OrgConstants.Category.Government, CountryCodes.Austria,
					expectedTraceLog: "Transaction category: GOV, Debtor country: AT, Enabled features: B2B"),
			};
			RunIsTransactionEligibleTestCases("GetAdditionalTraceLog tests, B2B enabled", testCases, enableB2B: true);
		}

		void RunIsTransactionEligibleTestCases(string runName, IEnumerable<IsTransactionEligibleTestCase> testCases, bool enableB2B = false, bool enableB2GinXT = false)
		{
			var featureControlManager = GetFeatureControlManagerMock(enableB2B, enableB2GinXT);
			using (ObjectFactory.Substitute(featureControlManager))
			{
				var eligibilityDecider = GetEligibilityDecider();

				foreach (var testConfigurationEnumerated in Enumerable.Range(1, testCases.Count()).Zip(testCases, (index, testConfiguration) => new { index, testConfiguration }))
				{
					var index = testConfigurationEnumerated.index;
					var testConfiguration = testConfigurationEnumerated.testConfiguration;

					var transaction = CreateEligibleTransaction();
					transaction.Ledger = testConfiguration.LedgerType;
					transaction.TransactionType = testConfiguration.TransactionType;
					transaction.TransactionCategory = testConfiguration.Category;
					transaction.CountryCode = testConfiguration.OrgCountry;
					transaction.InvoiceOrgAddressOverride = new FakeEligibilityLiteOrgAddress() { CountryCode = testConfiguration.OrgCountry };
					transaction.OrgHeader = new FakeEligibilityLiteOrgHeader().WithCategory(testConfiguration.Category);

					var actual = eligibilityDecider.IsTransactionEligible(transaction);
					AssertEquals($"{runName}: REC#{index} {nameof(CountryCodes.Germany)} {testConfiguration.LedgerType} {testConfiguration.TransactionType} {testConfiguration.Category} {testConfiguration.OrgCountry}", testConfiguration.Expected, actual);

					if (!string.IsNullOrEmpty(testConfiguration.ExpectedTraceLog))
					{
						AssertEquals($"{runName}: REC#{index} AdditionalTraceLog", testConfiguration.ExpectedTraceLog, eligibilityDecider.GetAdditionalTraceLog(transaction));
					}
				}
			}
		}

		IFeatureControlManager GetFeatureControlManagerMock(bool isB2bEnabled, bool isB2GinXTEnabled)
		{
			string featureDataParameter = @"
			{
				""DE"": {
					""Features"": [PLACEHOLDER PLACEHOLDER_B2G]
				}
			}"
			.Replace("PLACEHOLDER_B2G", isB2GinXTEnabled ? ((isB2bEnabled ? "," : "") + "\"B2GinXT\"") : "")
			.Replace("PLACEHOLDER", isB2bEnabled ? "\"B2B\"" : "");

			var featureDataMock = new Mock<IFeatureData>();
			featureDataMock.SetupGet(x => x.Parameter).Returns(featureDataParameter);

			var featureControlManagerMock = new Mock<IFeatureControlManager>();
			featureControlManagerMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingEInvoicingConfiguration, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
			return featureControlManagerMock.Object;
		}

		class IsTransactionEligibleTestCase(bool expected, string ledgerType, string transactionType, string category, string country, string expectedTraceLog = null)
		{
			public bool Expected { get; } = expected;
			public string ExpectedTraceLog { get; } = expectedTraceLog;
			public string LedgerType { get; } = ledgerType;
			public string TransactionType { get; } = transactionType;
			public string Category { get; } = category;
			public string OrgCountry { get; } = country;
		}
	}
}
