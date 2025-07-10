using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Core;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Spain
{
	class SpainRegistryItemsDefaultValues : DefaultValuesForCountrySpecificRegistryItems
	{
		protected override string TaxMessageIsMandatoryReceivables => Constants.TaxMessageMandatoryOptionConstants.RequiredAlways;
		protected override string TaxMessageIsMandatoryPayables => Constants.TaxMessageMandatoryOptionConstants.RequiredAlways;
	}
}
