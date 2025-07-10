namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IGovernmentAllocatedIDValidationData
	{
		string EReportingStatus { get; }
		string GovernmentAllocatedID { get; }
		string Ledger { get; }
		string OrgCountryCode { get; }
	}
}