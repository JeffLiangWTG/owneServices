namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IGovernmentAllocatedIDValidationProvider
	{
		string ValidateGovernmentAllocatedID(IGovernmentAllocatedIDValidationData validationData);
	}
}
