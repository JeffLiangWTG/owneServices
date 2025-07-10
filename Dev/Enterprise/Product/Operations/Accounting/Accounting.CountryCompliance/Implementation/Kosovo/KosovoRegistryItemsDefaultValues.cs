using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Kosovo
{
	class KosovoRegistryItemsDefaultValues : DefaultValuesForCountrySpecificRegistryItems
	{
		protected override bool GetShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency() => true;
	}
}
