using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Business
{
	internal class DeliveryRestrictionTypeHelper
	{
		static MultilingualString NoneMultilingualString => ResString.GetMultilingualString("064D123F-C5E8-4B6E-981A-D6A003DCA8C1", "None");
		static MultilingualString MovementRestrictedOnHoldMultilingualString => ResString.GetMultilingualString("40C13F5E-5106-4783-A2F5-728A3147F98E", "Movement Restricted/Credit on Hold");
		static MultilingualString UserDefinedMultilingualString => ResString.GetMultilingualString("7A694952-E7F8-484C-B992-5D5578B68F6B", "User Defined");

		public static CodeDescriptionPairList DeliveryRestrictionTypeList()
		{
			var deliveryRestrictionList = new CachedCodeDescriptionPairList();
			deliveryRestrictionList.AddPair(nameof(DeliveryRestrictionType.NON), NoneMultilingualString);
			deliveryRestrictionList.AddPair(nameof(DeliveryRestrictionType.CNH), MovementRestrictedOnHoldMultilingualString);
			deliveryRestrictionList.AddPair(nameof(DeliveryRestrictionType.UDF), UserDefinedMultilingualString);
			return deliveryRestrictionList;
		}

		public static CodeDescriptionPairList UserDefinedDeliveryRestrictionTypeList()
		{
			var deliveryRestrictionList = new CachedCodeDescriptionPairList();
			deliveryRestrictionList.AddPair(nameof(DeliveryRestrictionType.NON), NoneMultilingualString);
			deliveryRestrictionList.AddPair(nameof(DeliveryRestrictionType.UDF), UserDefinedMultilingualString);
			return deliveryRestrictionList;
		}
	}
}
