using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.QRCodeData;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Moq;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class PortugalAccountingCountryFactoryTest : TestCaseWithFactory
	{
		public void TestAccountingCountryFactory()
		{
			var mockIAccountingDependencyFactory = new Mock<IAccountingDependencyFactory>();
			var mockICountryComplianceFactory = new Mock<ICountryComplianceFactory>();
			var mockIQRCodeDataProvider = new Mock<MasterFiles.Business.CountryCompliance.IQRCodeDataProvider>();

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var transactionQRCodeDataProvider = new TransactionQRCodeDataProvider(invoice);

			var expectedreturn = "XXX";

			using (ObjectFactory.Substitute(mockIAccountingDependencyFactory.Object))
			using (ObjectFactory.Substitute(mockICountryComplianceFactory.Object))
			{
				mockIQRCodeDataProvider.Setup(x => x.GetTransactionQRCodeString(It.IsAny<ITransactionQRCodeDataProvider>())).Returns(expectedreturn);
				mockIAccountingDependencyFactory.Setup(x => x.GetTransactionQRCodeDataProvider(It.IsAny<InvoicingBase>())).Returns(transactionQRCodeDataProvider);
				mockICountryComplianceFactory.Setup(x => x.GetIQRCodeDataProvider(It.IsAny<ZString>())).Returns(mockIQRCodeDataProvider.Object);

				var result = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Portugal) as IQRCodeDataProvider)?.GetTransactionQRCodeString(invoice);

				AssertEquals(expectedreturn, result);

				mockICountryComplianceFactory.Verify(x => x.GetIQRCodeDataProvider(Constants.CountryCodes.Portugal), Times.Once);
				mockIQRCodeDataProvider.Verify(x => x.GetTransactionQRCodeString(transactionQRCodeDataProvider), Times.Once);
				mockIAccountingDependencyFactory.Verify(x => x.GetTransactionQRCodeDataProvider(invoice), Times.Once);
			}
		}

		public void TestIReportSAFTWriter()
		{
			var builder = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Portugal) as IInstanceProvider<IReportSAFTWriter>).Get();

			AssertNotNull(builder);
		}

		public void TestIReportModeAndCreditorSelectorDefault()
		{
			var builder = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Portugal) as IInstanceProvider<IReportModeAndCreditorSelectorDefault>).Get();

			AssertNotNull(builder);
		}

		public void TestIComplianceReportGUIActionProvider()
		{
			var provider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Portugal) as IInstanceProvider<IComplianceReportGUIActionProvider>)?.Get();

			AssertNotNull(provider);
		}
	}
}
