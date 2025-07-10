using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Israel
{
	class IsraelRegistryItemsDefaultValues : DefaultValuesForCountrySpecificRegistryItems
	{
		protected override MultilingualString PrintWatermarkForTransactionAwaitingApproval => ResString.GetMultilingualString("ee91df58-639e-4c53-a9c7-08720115ada6", "Invalid.\r\nNo Govt. ID");

		protected override bool EnableGovernmentAllocatedNumberBehavior => true;
	}
}
