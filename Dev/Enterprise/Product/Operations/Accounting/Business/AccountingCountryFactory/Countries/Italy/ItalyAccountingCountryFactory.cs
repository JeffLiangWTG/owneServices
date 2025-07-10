using Enterprise.Accounting.Business.AccountingCountryFactory.Italy;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class ItalyAccountingCountryFactory : IAccountingCountryFactory,
		IInstanceProvider<IComplianceNumberResetStatus>,
		IInstanceProvider<IEInvoicingPreEligibilityProvider>
	{
		IComplianceNumberResetStatus IInstanceProvider<IComplianceNumberResetStatus>.Get() => new ComplianceNumberResetStatus();

		IEInvoicingPreEligibilityProvider IInstanceProvider<IEInvoicingPreEligibilityProvider>.Get() => new ItalyEInvoicingPreEligibilityProvider();
	}
}
