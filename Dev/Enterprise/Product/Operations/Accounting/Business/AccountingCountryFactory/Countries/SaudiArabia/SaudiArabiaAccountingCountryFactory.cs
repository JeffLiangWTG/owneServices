using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class SaudiArabiaAccountingCountryFactory : IAccountingCountryFactory,
		IQRCodeDataProvider,
		IInstanceProvider<IEInvoicingEligibilityDecider>,
		IInstanceProvider<IEInvoicingPivotStatusProvider>
	{
		string IQRCodeDataProvider.GetTransactionQRCodeString(InvoicingBase invoice)
		{
			IQRCodeDataProvider qrCodeDataProvider = new SaudiArabiaQRCodeDataProvider();
			return qrCodeDataProvider.GetTransactionQRCodeString(invoice);
		}

		IEInvoicingEligibilityDecider IInstanceProvider<IEInvoicingEligibilityDecider>.Get() => new SaudiArabiaEInvoicingEligibilityDecider();

		IEInvoicingPivotStatusProvider IInstanceProvider<IEInvoicingPivotStatusProvider>.Get() => new SaudiArabiaEInvoicingPivotStatusProvider();
	}
}
