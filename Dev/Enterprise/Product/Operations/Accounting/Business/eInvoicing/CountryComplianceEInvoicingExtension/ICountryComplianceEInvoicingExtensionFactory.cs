using CargoWise.Types;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public interface ICountryComplianceEInvoicingExtensionFactory
	{
		IEInvoicingTransactionValidation GetIEInvoicingTransactionValidation(ZString countryCode);
		IMostRecentPivotProvider GetIMostRecentPivotProvider(ZString countryCode);
		IEReportingStatusMessageProvider GetIEReportingStatusMessageProvider(ZString countryCode);
		IExistPivotCheckProvider GetExistPivotCheckProvider(ZString countryCode);
	}
}
