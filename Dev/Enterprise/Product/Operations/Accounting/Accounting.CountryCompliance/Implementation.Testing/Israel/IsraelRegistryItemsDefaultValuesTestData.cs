using Enterprise.Accounting.CountryCompliance.Implementation.Testing.CountrySpecificRegistryDefaultValues;

namespace Enterprise.Accounting.CountryCompliance.Implementation.CountrySpecificRegistryDefaultValues.Testing
{
	class IsraelRegistryItemsDefaultValuesTestData : DefaultTestDataForCountrySpecificRegistryItems
	{
		public override string PrintWatermarkForTransactionAwaitingApproval => "Invalid.\r\nNo Govt. ID";

		public override bool EnableGovernmentAllocatedNumberBehavior => true;
	}
}
