namespace Enterprise.Accounting.CountryCompliance.Interfaces
{
	public interface IComplianceNumberProvider
	{
		bool CanAllocateComplianceNumber(bool allLinesWithCMTCharge);
	}
}
