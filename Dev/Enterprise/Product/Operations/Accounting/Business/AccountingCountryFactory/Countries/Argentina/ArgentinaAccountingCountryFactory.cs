using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Argentina;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class ArgentinaAccountingCountryFactory : IAccountingCountryFactory,
		IQRCodeDataProvider,
		IThresholdProvider,
		ICountrySpecificLabelTranslator,
		IInstanceProvider<IOrgTaxRateImportFileFormatProvider>,
		IInstanceProvider<ITaxFrameworkConfigurationDefaults>
	{
		string IQRCodeDataProvider.GetTransactionQRCodeString(InvoicingBase invoice)
		{
			//The QR implementation in MasterFiles solution is obsolete, use Accounting solution instead. Check Mexico as an example.
			return ObjectFactory.Get<ICountryComplianceFactory>()
				.GetIQRCodeDataProvider(Constants.CountryCodes.Argentina)?.GetTransactionQRCodeString(ObjectFactory.Get<IAccountingDependencyFactory>().GetTransactionQRCodeDataProvider(invoice));
		}

		ZDecimal IThresholdProvider.GetThresholdAmount()
		{
			return AccountingConfigurationRegistry.Instance.ThresholdValidationFCEElectronicCreditInvoice.Value;
		}

		ZString? ICountrySpecificLabelTranslator.GetTranslation(LabelsEnum? label, params object[] parameters)
		{
			return ArgentinaSpecificTranslactions.TranslateLabel(label, parameters);
		}

		IOrgTaxRateImportFileFormatProvider IInstanceProvider<IOrgTaxRateImportFileFormatProvider>.Get() => new ArgentinaFileFormatProvider();

		ITaxFrameworkConfigurationDefaults IInstanceProvider<ITaxFrameworkConfigurationDefaults>.Get() => new TaxFrameworkConfigurationDefaults();
	}
}
