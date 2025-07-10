using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation.CzechRepublic
{
	class CzechRepublicRegistryItemsDefaultValues : DefaultValuesForCountrySpecificRegistryItems
	{
		protected override bool GetShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency() => true;
	}
}
