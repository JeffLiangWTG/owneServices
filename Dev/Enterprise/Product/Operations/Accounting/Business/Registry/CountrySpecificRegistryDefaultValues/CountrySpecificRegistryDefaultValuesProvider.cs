using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue;

namespace Enterprise.Accounting.Registry.Business
{
	public class CountrySpecificRegistryDefaultValuesProvider : ICountrySpecificRegistryDefaultValuesProvider
	{
		public IDefaultValuesForCountrySpecificRegistryItems Get(ZString countryCode)
		{
			return (ObjectFactory.Get<IAccountingCountryComplianceGlobalFactory>().GetFeatureInterface<IDefaultValuesForCountrySpecificRegistryItems>(countryCode)) ?? new DefaultValuesForCountrySpecificRegistryItems();
		}
	}
}
