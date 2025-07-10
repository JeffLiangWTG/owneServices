using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.Presentation;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Accounting.Business.AccountingPresentationProviders.Test
{
	public class JobInvoicePrintingControlPresentationProviderTest : TestCaseWithFactory
	{
		public void TestIsEInvoicingColumnsAvailable()
		{
			var electronicInvoicingAccountingObjectFactoryMock = new Mock<IElectronicInvoicingAccountingObjectFactory>();
			var eInvoicingConfigurationChecksMock = new Mock<IEInvoicingConfigurationChecks>();

			electronicInvoicingAccountingObjectFactoryMock.Setup(x => x.GetEInvoicingConfigurationChecks()).Returns(eInvoicingConfigurationChecksMock.Object);
			ObjectFactory.Substitute(electronicInvoicingAccountingObjectFactoryMock.Object);

			IJobInvoicePrintingControlPresentationProvider provider = new JobInvoicePrintingControlPresentationProvider();

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

		public void TestIsTaxBranchColumnAvailable_DependsOnEnableTaxBranchReportingRegistry()
		{
			var jobInvoicePrintingControlPresentationProvider = ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetJobInvoicePrintingControlPresentationProvider();

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Precondition:EnableTaxBranchReporting registry value is false", false, jobInvoicePrintingControlPresentationProvider.IsTaxBranchColumnAvailable());

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("EnableTaxBranchReporting registry value is true", true, jobInvoicePrintingControlPresentationProvider.IsTaxBranchColumnAvailable());
		}
	}
}
