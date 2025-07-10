using Enterprise.Accounting.CountryCompliance.Implementation.Singapore;
using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation
{
	public class SingaporeCountryFactory :
		IInstanceProvider<IDefaultValuesForCountrySpecificRegistryItems>
	{
		IDefaultValuesForCountrySpecificRegistryItems IInstanceProvider<IDefaultValuesForCountrySpecificRegistryItems>.Get() => new SingaporeRegistryItemsDefaultValues();
	}
}
