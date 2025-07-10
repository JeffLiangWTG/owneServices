using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation.China
{
	class ChinaRegistryItemsDefaultValues : DefaultValuesForCountrySpecificRegistryItems
	{
		protected override int EInvoicingRequeueDelayTime => 30;
	}
}
