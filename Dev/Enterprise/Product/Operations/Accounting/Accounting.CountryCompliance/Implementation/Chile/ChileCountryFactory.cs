using Enterprise.Accounting.CountryCompliance.Implementation.Chile;
using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation
{
	public class ChileCountryFactory :
		IInstanceProvider<IDefaultValuesForCountrySpecificRegistryItems>
	{
		IDefaultValuesForCountrySpecificRegistryItems IInstanceProvider<IDefaultValuesForCountrySpecificRegistryItems>.Get() => new ChileRegistryItemsDefaultValues();
	}
}
