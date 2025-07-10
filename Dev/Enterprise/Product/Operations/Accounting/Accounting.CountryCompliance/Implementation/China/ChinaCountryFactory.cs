using Enterprise.Accounting.CountryCompliance.Implementation.China;
using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation
{
	public class ChinaCountryFactory :
		IInstanceProvider<IComplianceNumberProvider>,
		IInstanceProvider<IDefaultValuesForCountrySpecificRegistryItems>
	{
		IComplianceNumberProvider IInstanceProvider<IComplianceNumberProvider>.Get() => new ChinaComplianceNumberProvider();

		IDefaultValuesForCountrySpecificRegistryItems IInstanceProvider<IDefaultValuesForCountrySpecificRegistryItems>.Get() => new ChinaRegistryItemsDefaultValues();
	}
}
