using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Accounting.Business.AccountingPresentationProviders.Test
{
	public class TransactionModuleStripPresentationProviderTest : TestCaseWithFactory
	{
		public void TestIsEInvoicingColumnsAvailable()
		{
			var electronicInvoicingAccountingObjectFactoryMock = new Mock<IElectronicInvoicingAccountingObjectFactory>();
			var eInvoicingConfigurationChecksMock = new Mock<IEInvoicingConfigurationChecks>();

			electronicInvoicingAccountingObjectFactoryMock.Setup(x => x.GetEInvoicingConfigurationChecks()).Returns(eInvoicingConfigurationChecksMock.Object);
			ObjectFactory.Substitute(electronicInvoicingAccountingObjectFactoryMock.Object);

			ITransactionModuleStripPresentationProvider provider = new TransactionModuleStripPresentationProvider();

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
	}
}
