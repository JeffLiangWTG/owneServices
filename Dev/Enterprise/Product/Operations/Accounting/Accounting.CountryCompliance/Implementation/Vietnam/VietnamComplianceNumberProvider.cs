using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Vietnam
{
	class VietnamComplianceNumberProvider :
		IComplianceNumberProvider
	{
		bool IComplianceNumberProvider.CanAllocateComplianceNumber(bool allLinesWithCMTCharge)
		{
			return !allLinesWithCMTCharge;
		}
	}
}
