using Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Malaysia;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class MalaysiaAccountingCountryFactory : IAccountingCountryFactory,
		IInstanceProvider<IEInvoicingEligibilityDecider>,
		IInstanceProvider<IEInvoicingPreEligibilityProvider>,
		IInstanceProvider<IEInvoicingPivotsToRequeueFilterProvider>,
		IInstanceProvider<IEInvoicingDiscardAdditionalPiviotActionTypesProvider>,
		IInstanceProvider<IEInvoicingPivotStatusProvider>,
		IQRCodeDataProvider
	{
		IEInvoicingEligibilityDecider IInstanceProvider<IEInvoicingEligibilityDecider>.Get() =>
			new MalaysiaEInvoicingEligibilityDecider();
		IEInvoicingPreEligibilityProvider IInstanceProvider<IEInvoicingPreEligibilityProvider>.Get() => new MalaysiaEInvoicingPreEligibilityProvider();
		public IEInvoicingPivotsToRequeueFilterProvider Get() => new MalaysiaEInvoicingPivotsToRequeueInfo();

		IEInvoicingDiscardAdditionalPiviotActionTypesProvider IInstanceProvider<IEInvoicingDiscardAdditionalPiviotActionTypesProvider>.Get() => new MalaysiaIeInvoicingDiscardAdditionalPiviotActionTypesProvider();

		IEInvoicingPivotStatusProvider IInstanceProvider<IEInvoicingPivotStatusProvider>.Get() => new MalaysiaEInvoicingPivotStatusProvider();

		public string GetTransactionQRCodeString(InvoicingBase invoicing)
		{
			return new MalaysiaQRCodeDataProvider().GetTransactionQRCodeString(invoicing);
		}
	}
}
