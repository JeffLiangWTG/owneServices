using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Singapore
{
	class SingaporeRegistryItemsDefaultValues : DefaultValuesForCountrySpecificRegistryItems
	{
		protected override bool GetShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency() => true;
	}
}
