using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation.China
{
	class ChinaComplianceNumberProvider :
		IComplianceNumberProvider
	{
		bool IComplianceNumberProvider.CanAllocateComplianceNumber(bool allLinesWithCMTCharge)
		{
			return false;
		}
	}
}
