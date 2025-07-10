using Enterprise.Accounting.CountryCompliance.Implementation.Testing.CountrySpecificRegistryDefaultValues;
using Enterprise.Core;

namespace Enterprise.Accounting.CountryCompliance.Implementation.CountrySpecificRegistryDefaultValues.Testing
{
	class SpainRegistryItemsDefaultValuesTestData : DefaultTestDataForCountrySpecificRegistryItems
	{
		public override string TaxMessageIsMandatoryReceivables => Constants.TaxMessageMandatoryOptionConstants.RequiredAlways;

		public override string TaxMessageIsMandatoryPayables => Constants.TaxMessageMandatoryOptionConstants.RequiredAlways;
	}
}
