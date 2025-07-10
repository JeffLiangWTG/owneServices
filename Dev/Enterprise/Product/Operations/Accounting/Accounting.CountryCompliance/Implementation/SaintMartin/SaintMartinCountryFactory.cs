using Enterprise.Accounting.CountryCompliance.Implementation.SaintMartin;
using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation
{
	public class SaintMartinCountryFactory :
		IInstanceProvider<IDefaultValuesForCountrySpecificRegistryItems>
	{
		IDefaultValuesForCountrySpecificRegistryItems IInstanceProvider<IDefaultValuesForCountrySpecificRegistryItems>.Get() => new SaintMartinRegistryItemsDefaultValues();
	}
}
