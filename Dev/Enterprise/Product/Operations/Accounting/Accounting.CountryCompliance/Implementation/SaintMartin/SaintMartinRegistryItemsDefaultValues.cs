using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation.SaintMartin
{
	class SaintMartinRegistryItemsDefaultValues : DefaultValuesForCountrySpecificRegistryItems
	{
		protected override bool GetShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency() => true;
	}
}
