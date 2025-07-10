using Enterprise.Accounting.CountryCompliance.Implementation.Testing.CountrySpecificRegistryDefaultValues;

namespace Enterprise.Accounting.CountryCompliance.Implementation.CountrySpecificRegistryDefaultValues.Testing
{
	class ChinaRegistryItemsDefaultValuesTestData : DefaultTestDataForCountrySpecificRegistryItems
	{
		public override int EInvoicingRequeueDelayTime => 30;
	}
}
