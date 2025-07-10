using Enterprise.Accounting.CountryCompliance.Implementation.Spain;
using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation
{
	public class SpainCountryFactory :
		IInstanceProvider<IDefaultValuesForCountrySpecificRegistryItems>
	{
		IDefaultValuesForCountrySpecificRegistryItems IInstanceProvider<IDefaultValuesForCountrySpecificRegistryItems>.Get() => new SpainRegistryItemsDefaultValues();
	}
}
