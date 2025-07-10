using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class GermanyAccountingCountryFactory : IAccountingCountryFactory,
		IInstanceProvider<IComplianceReportsProvider>,
		IInstanceProvider<IEInvoicingEligibilityDecider>
	{
		IComplianceReportsProvider IInstanceProvider<IComplianceReportsProvider>.Get() => new GermanyComplianceReportsProvider();

		IEInvoicingEligibilityDecider IInstanceProvider<IEInvoicingEligibilityDecider>.Get() => new GermanyEInvoicingEligibilityDecider();
	}
}
