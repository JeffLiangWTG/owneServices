using Enterprise.Accounting.CountryCompliance.Implementation.Turkey;
using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation
{
	public class TurkeyCountryFactory :
		IInstanceProvider<IDefaultValuesForCountrySpecificRegistryItems>
	{
		IDefaultValuesForCountrySpecificRegistryItems IInstanceProvider<IDefaultValuesForCountrySpecificRegistryItems>.Get() => new TurkeyRegistryItemsDefaultValues();
	}
}
