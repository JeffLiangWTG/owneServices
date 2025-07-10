using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class AustraliaAccountingCountryFactory : IAccountingCountryFactory, IInstanceProvider<IComplianceFinancialYear>
	{
		public IComplianceFinancialYear Get() => new AustraliaComplianceFinancialYear();
	}
}
