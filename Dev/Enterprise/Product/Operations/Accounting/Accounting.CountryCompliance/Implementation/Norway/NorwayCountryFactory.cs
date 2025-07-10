using Enterprise.Accounting.CountryCompliance.Implementation.Norway;
using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation
{
	public class NorwayCountryFactory :
		IInstanceProvider<IDefaultValuesForCountrySpecificRegistryItems>,
		IInstanceProvider<ISAFTComplianceReport130DataProvider>
	{
		IDefaultValuesForCountrySpecificRegistryItems IInstanceProvider<IDefaultValuesForCountrySpecificRegistryItems>.Get() => new NorwayRegistryItemsDefaultValues();

		ISAFTComplianceReport130DataProvider IInstanceProvider<ISAFTComplianceReport130DataProvider>.Get() => new NorwaySAFTComplianceReport130DataProvider();
	}
}
