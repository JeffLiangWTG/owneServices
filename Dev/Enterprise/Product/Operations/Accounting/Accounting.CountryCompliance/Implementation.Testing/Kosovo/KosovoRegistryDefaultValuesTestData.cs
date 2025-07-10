using Enterprise.Accounting.CountryCompliance.Implementation.Testing.CountrySpecificRegistryDefaultValues;

namespace Enterprise.Accounting.CountryCompliance.Implementation.CountrySpecificRegistryDefaultValues.Testing
{
	class KosovoRegistryItemsDefaultValuesTestData : DefaultTestDataForCountrySpecificRegistryItems
	{
		public override bool ExpectedDefaultShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency => true;
	}
}

