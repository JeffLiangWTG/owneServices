using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Accounting.Business.AccountingPresentationProviders.Test
{
	public class TransactionFilterStripControlPresentationProviderTest : TestCaseWithFactory
	{
		public void TestIsEInvoicingColumnsAvailable()
		{
			var electronicInvoicingAccountingObjectFactoryMock = new Mock<IElectronicInvoicingAccountingObjectFactory>();
			var eInvoicingConfigurationChecksMock = new Mock<IEInvoicingConfigurationChecks>();

			electronicInvoicingAccountingObjectFactoryMock.Setup(x => x.GetEInvoicingConfigurationChecks()).Returns(eInvoicingConfigurationChecksMock.Object);
			ObjectFactory.Substitute(electronicInvoicingAccountingObjectFactoryMock.Object);

			ITransactionFilterStripControlPresentationProvider provider = new TransactionFilterStripControlPresentationProvider();

			AssertEInvoicingColumnsAvailable(LedgerTypes.AccountsPayable, true);
			AssertEInvoicingColumnsAvailable(LedgerTypes.AccountsPayable, false);
			AssertEInvoicingColumnsAvailable(LedgerTypes.AccountsReceivable, true);
			AssertEInvoicingColumnsAvailable(LedgerTypes.AccountsReceivable, false);

			void AssertEInvoicingColumnsAvailable(ZString ledgerType, bool expectedResult)
			{
				eInvoicingConfigurationChecksMock.Setup(x => x.IsEInvoicingSupportedForCurrentCountry(ledgerType)).Returns(expectedResult);

				var actualResult = provider.IsEInvoicingColumnsAvailable(ledgerType);

				AssertEquals(expectedResult, actualResult);
			}
		}

		public void TestIsRelatedDisbursementTransactionFilterAvailable()
		{
			var accountingCountryComplianceGlobalFactoryMock = new Mock<IAccountingCountryComplianceGlobalFactory>();
			var koreaMock = new Mock<IRelatedDisbursementTransaction>();
			var australiaMock = new Mock<IRelatedDisbursementTransaction>();

			accountingCountryComplianceGlobalFactoryMock.Setup(x => x.GetFeatureInterface<IRelatedDisbursementTransaction>(Constants.CountryCodes.KoreaSouth)).Returns(koreaMock.Object);
			accountingCountryComplianceGlobalFactoryMock.Setup(x => x.GetFeatureInterface<IRelatedDisbursementTransaction>(Constants.CountryCodes.Australia)).Returns(australiaMock.Object);
			koreaMock.Setup(x => x.IsEnableRelatedDisbursementTransaction()).Returns(true);

			ObjectFactory.Substitute(accountingCountryComplianceGlobalFactoryMock.Object);

			TestIsRelatedDisbursementTransactionFilterAvailableWithCondition(Constants.CountryCodes.KoreaSouth, true, true);
			TestIsRelatedDisbursementTransactionFilterAvailableWithCondition(Constants.CountryCodes.KoreaSouth, false, false);

			TestIsRelatedDisbursementTransactionFilterAvailableWithCondition(Constants.CountryCodes.Australia, true, false);
			TestIsRelatedDisbursementTransactionFilterAvailableWithCondition(Constants.CountryCodes.Australia, false, false);
		}

		void TestIsRelatedDisbursementTransactionFilterAvailableWithCondition(string countryCode, bool enableEInvoicing, bool expected)
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingAndAutomaticllyAppendingDisbursementFeesSummary(countryCode, enableEInvoicing))
			{
				ITransactionFilterStripControlPresentationProvider provider = new TransactionFilterStripControlPresentationProvider();

				var actual = provider.IsRelatedDisbursementTransactionFilterAvailable();

				AssertEquals($"Country: {countryCode}, EnableEInvoicingFunctionality: {enableEInvoicing}, ExpectedResult should be {expected}", expected, actual);
			}
		}

		#region Test Object Creator

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
			}
		}
		TestObjectCreator fTestObjectCreator;

		#endregion
	}
}
