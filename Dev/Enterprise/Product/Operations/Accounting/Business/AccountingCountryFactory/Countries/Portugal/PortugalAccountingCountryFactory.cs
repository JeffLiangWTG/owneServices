using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class PortugalAccountingCountryFactory :
		IAccountingCountryFactory,
		IQRCodeDataProvider,
		IEnableDocumentSigningServiceProvider,
		ICloudSigningServiceProviderAPIEndpointProvider,
		IInstanceProvider<IReportSAFTWriter>,
		IInstanceProvider<IReportModeAndCreditorSelectorDefault>,
		IInstanceProvider<IComplianceSubTypeEditableProvider>,
		IInstanceProvider<ISourceReferenceEditableProvider>,
		IInstanceProvider<IComplianceReportGUIActionProvider>
	{
		IComplianceReportGUIActionProvider IInstanceProvider<IComplianceReportGUIActionProvider>.Get() => new PortugalComplianceReportGUIActionProvider();

		string IQRCodeDataProvider.GetTransactionQRCodeString(InvoicingBase invoice)
		{
			//The QR implementation in MasterFiles solution is obsolete, use Accounting solution instead. Check Mexico as an example.
			return ObjectFactory.Get<ICountryComplianceFactory>()
				.GetIQRCodeDataProvider(Constants.CountryCodes.Portugal)?.GetTransactionQRCodeString(ObjectFactory.Get<IAccountingDependencyFactory>().GetTransactionQRCodeDataProvider(invoice));
		}

		IReportSAFTWriter IInstanceProvider<IReportSAFTWriter>.Get() => new PortugalReportSAFTWriterProvider();

		IReportModeAndCreditorSelectorDefault IInstanceProvider<IReportModeAndCreditorSelectorDefault>.Get() => new PortugalReportModeAndCreditorSelectorDefault();

		IComplianceSubTypeEditableProvider IInstanceProvider<IComplianceSubTypeEditableProvider>.Get() => new PortugalComplianceSubTypeEditableProvider();

		ISourceReferenceEditableProvider IInstanceProvider<ISourceReferenceEditableProvider>.Get() => new PortugalSourceReferenceEditableProvider();

		bool IEnableDocumentSigningServiceProvider.IsEnableDocumentSigningService() => Env.Instance.IsProductionSystem && ZDateTime.Today >= new ZDateTime(2026, 1, 1);

		string ICloudSigningServiceProviderAPIEndpointProvider.GetCloudSigningServiceProviderAPIEndpoint() => Env.Instance.IsProductionSystem ? "https://qscd.digitalsign.pt" : "https://qscd-dev.digitalsign.pt";
	}
}
