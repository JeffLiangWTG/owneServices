using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.EInvoicing
{
	class EInvoicingConfigurationChecksTest : TestCaseWithFactory
	{
		public void TestIsEInvoicingSupportedForCurrentCountry_ResultForDifferentConfiguration()
		{
			var eInvoicingMock = new Mock<IGlobalEInvoicingObjectFactory>();
			ObjectFactory.Substitute(eInvoicingMock.Object);

			var currentCompany = GlbCompany.CurrentCompany;
			var currentCompanyPK = currentCompany.PK.ToGuid();
			var registry = AccountingMasterFilesRegistry.Instance;
			IEInvoicingConfigurationChecks configurationChecks = new EInvoicingConfigurationChecks();

			AssertIsEInvoicingSupportedForCurrentCountry(LedgerTypes.AccountsPayable, true, false, false);
			AssertIsEInvoicingSupportedForCurrentCountry(LedgerTypes.AccountsPayable, false, true, true);
			AssertIsEInvoicingSupportedForCurrentCountry(LedgerTypes.AccountsPayable, true, true, true);
			AssertIsEInvoicingSupportedForCurrentCountry(LedgerTypes.AccountsPayable, false, false, false);
			AssertIsEInvoicingSupportedForCurrentCountry(LedgerTypes.AccountsReceivable, true, false, true);
			AssertIsEInvoicingSupportedForCurrentCountry(LedgerTypes.AccountsReceivable, false, true, false);
			AssertIsEInvoicingSupportedForCurrentCountry(LedgerTypes.AccountsReceivable, true, true, true);
			AssertIsEInvoicingSupportedForCurrentCountry(LedgerTypes.AccountsReceivable, false, false, false);
			AssertIsEInvoicingSupportedForCurrentCountry(ZString.Empty, true, true, false);

			foreach (var ledgerType in typeof(LedgerTypes).GetConstantValues())
			{
				if (ledgerType != LedgerTypes.AccountsReceivable && ledgerType != LedgerTypes.AccountsPayable)
				{
					AssertIsEInvoicingSupportedForCurrentCountry(ledgerType, true, true, false);
				}
			}

			void AssertIsEInvoicingSupportedForCurrentCountry(ZString ledgerType, bool enableRegistryForAR, bool enableRegistryForAP, bool expectedResult)
			{
				using (registry.EnableEInvoicingFunctionality.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, enableRegistryForAR))
				using (registry.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, enableRegistryForAP))
				{
					eInvoicingMock.Setup(x => x.DoesCountrySupportElectronicInvoicing(It.IsAny<ZString>())).Returns(true);
					var actualResult = configurationChecks.IsEInvoicingSupportedForCurrentCountry(ledgerType);
					AssertEquals("Country that does support einvoicing", expectedResult, actualResult);

					eInvoicingMock.Setup(x => x.DoesCountrySupportElectronicInvoicing(It.IsAny<ZString>())).Returns(false);
					actualResult = configurationChecks.IsEInvoicingSupportedForCurrentCountry(ledgerType);
					AssertEquals("Country that does not support einvoicing", false, actualResult);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestIsEInvoicingSupportedForCurrentCountry_CurrentCountryIsUseToCheckEInvoiceSupport()
		{
			var eInvoicingMock = new Mock<IGlobalEInvoicingObjectFactory>();
			ObjectFactory.Substitute(eInvoicingMock.Object);

			var currentCompany = GlbCompany.CurrentCompany;
			IEInvoicingConfigurationChecks configurationChecks = new EInvoicingConfigurationChecks();
			foreach (var country in new[] { CountryCodes.Italy, CountryCodes.Brazil, CountryCodes.India })
			{
				using (currentCompany.TemporarilySetCountry(country))
				{
					eInvoicingMock.Reset();

					var result = configurationChecks.IsEInvoicingSupportedForCurrentCountry(LedgerTypes.AccountsReceivable);

					eInvoicingMock.Verify(x => x.DoesCountrySupportElectronicInvoicing(country), Times.Once);
				}
			}
		}
	}
}
