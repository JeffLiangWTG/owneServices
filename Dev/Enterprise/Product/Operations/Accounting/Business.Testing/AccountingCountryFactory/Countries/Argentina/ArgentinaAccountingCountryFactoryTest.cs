using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.QRCodeData;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Moq;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class ArgentinaAccountingCountryFactoryTest : TestCaseWithFactory
	{
		public void TestFormatProvider()
		{
			var provider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Argentina) as IInstanceProvider<IOrgTaxRateImportFileFormatProvider>)?.Get();
			AssertNotNull("Provider must not be null", provider);

			var formatForCapitalFederal = "S01AR";
			var fileFormatCapitalFederal = provider?.GetFileFormat(formatForCapitalFederal) as CapitalFederalFileFormat;
			AssertType<CapitalFederalFileFormat>(fileFormatCapitalFederal);

			var formatForBuenosAires = "S02AR";
			var fileFormatBuenosAires = provider?.GetFileFormat(formatForBuenosAires) as BuenosAiresFileFormat;
			AssertType<BuenosAiresFileFormat>(fileFormatBuenosAires);
		}

		public void TestFormatProviderExpectedDefaultFormat()
		{
			var testCases = new[] { "", "X1Y2", null };
			var provider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Argentina) as IInstanceProvider<IOrgTaxRateImportFileFormatProvider>)?.Get();

			foreach (var test in testCases)
			{
				var fileFormat = provider?.GetFileFormat(test) as DefaultFileFormat;
				AssertType<DefaultFileFormat>(fileFormat);
			}
		}

		public void TestEachFileFormatIsAssignableFrom()
		{
			var testCases = new[] { typeof(BuenosAiresFileFormat), typeof(CapitalFederalFileFormat), typeof(DefaultFileFormat) };

			foreach (var test in testCases)
			{
				var isAssignableFrom = typeof(IOrgTaxRateImportFileFormat).IsAssignableFrom(test);
				AssertEquals($"{nameof(test)} must implement IOrgTaxRateImportFileFormat interface.", isAssignableFrom, true);
			}
		}

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

				var result = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Argentina) as IQRCodeDataProvider)?.GetTransactionQRCodeString(invoice);

				AssertEquals(expectedreturn, result);

				mockICountryComplianceFactory.Verify(x => x.GetIQRCodeDataProvider(Constants.CountryCodes.Argentina), Times.Once);
				mockIQRCodeDataProvider.Verify(x => x.GetTransactionQRCodeString(transactionQRCodeDataProvider), Times.Once);
				mockIAccountingDependencyFactory.Verify(x => x.GetTransactionQRCodeDataProvider(invoice), Times.Once);
			}
		}

		public void TestGetThresholdAmount()
		{
			var builder = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Argentina) as IThresholdProvider;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Argentina))
			{
				var expectedValue = 10.00m;
				AccountingConfigurationRegistry.Instance.ThresholdValidationFCEElectronicCreditInvoice.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, expectedValue);
				AssertEquals(expectedValue, builder.GetThresholdAmount());

				expectedValue = 87655520.00m;
				AccountingConfigurationRegistry.Instance.ThresholdValidationFCEElectronicCreditInvoice.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, expectedValue);
				AssertEquals(expectedValue, builder.GetThresholdAmount());
			}
		}
	}
}
