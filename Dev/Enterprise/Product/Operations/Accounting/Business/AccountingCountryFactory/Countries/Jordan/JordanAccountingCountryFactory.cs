using Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Jordan;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class JordanAccountingCountryFactory : IAccountingCountryFactory,
		IInstanceProvider<IEInvoicingEligibilityDecider>,
		IInstanceProvider<IEInvoicingPreEligibilityProvider>,
		IQRCodeDataProvider
	{
		IEInvoicingEligibilityDecider IInstanceProvider<IEInvoicingEligibilityDecider>.Get() => new JordanEInvoicingEligibilityDecider();

		IEInvoicingPreEligibilityProvider IInstanceProvider<IEInvoicingPreEligibilityProvider>.Get() => new JordanEInvoicingPreEligibilityProvider();

		string IQRCodeDataProvider.GetTransactionQRCodeString(InvoicingBase invoicing)
		{
			return new JordanQRCodeDataProvider().GetTransactionQRCodeString(invoicing);
		}
	}
}
