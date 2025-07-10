using Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Israel;
using Enterprise.Accounting.Business.AccountingCountryFactory.Israel;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class IsraelAccountingCountryFactory :
		IAccountingCountryFactory,
		IInstanceProvider<IEInvoicingEligibilityDecider>,
		IInstanceProvider<IComplianceReportDefaultValue>,
		IInstanceProvider<IGovernmentAllocatedIDValidationProvider>,
		IInstanceProvider<IEInvoicingPivotStatusProvider>,
		IInstanceProvider<IEInvoicingAuthorizationBehaviourProvider>,
		IInstanceProvider<IEInvoicingPreEligibilityProvider>
	{
		IEInvoicingEligibilityDecider IInstanceProvider<IEInvoicingEligibilityDecider>.Get() => new IsraelEInvoicingEligibilityDecider();

		IComplianceReportDefaultValue IInstanceProvider<IComplianceReportDefaultValue>.Get() => new IsraelComplianceReportDefaultValue();

		IGovernmentAllocatedIDValidationProvider IInstanceProvider<IGovernmentAllocatedIDValidationProvider>.Get() => new IsraelGovernmentAllocatedIDPayablesValidationProvider();

		IEInvoicingPivotStatusProvider IInstanceProvider<IEInvoicingPivotStatusProvider>.Get() => new IsraelEInvoicingPivotStatusProvider();

		IEInvoicingAuthorizationBehaviourProvider IInstanceProvider<IEInvoicingAuthorizationBehaviourProvider>.Get() => new EInvoicingAuthorizationBehaviourProvider();

		IEInvoicingPreEligibilityProvider IInstanceProvider<IEInvoicingPreEligibilityProvider>.Get() => new IsraelEInvoicingPreEligibilityProvider();
	}
}
