using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public static class ChargeCodeForPricingPageSectionsHelper
	{
		public static class PricingPage
		{
			public static class Code
			{
				public const string ForwardingConcise = "FCO";
			}

			public static CodeDescriptionPair ForwardingConcise =>
				new (Code.ForwardingConcise, ResString.GetMultilingualString("88F45713-B9B6-4CF5-B494-797300DCEE30", "Forwarding Concise"));
		}

		public static class Section
		{
			public static class Code
			{
				public const string OriginPickupCharges = "OPC";
				public const string DestinationDeliveryCharges = "DDC";
			}

			public static CodeDescriptionPair OriginPickupCharges =>
				new (Code.OriginPickupCharges, ResString.GetMultilingualString("DF7D89CA-A27B-4364-BF9C-E40F7F6D61D7", "Origin Pickup Charges"));

			public static CodeDescriptionPair DestinationDeliveryCharges =>
				new (Code.DestinationDeliveryCharges, ResString.GetMultilingualString("D931BDC3-BC24-4C42-BFDF-0DA1B84F1313", "Destination Delivery Charges"));
		}

		#region ChargeCodeGroupList

		public static class ChargeCodeGroupList
		{
			public const string Origin = "ORG";
			public const string Loading = "LOD";
			public const string OriginBrokerage = "OBR";
			public const string OriginBrokerageOnly = "OBO";
			public const string Destination = "DST";
			public const string Unloading = "UNL";
			public const string Brokerage = "BRK";
			public const string BrokerageOnly = "BON";
			public const string CustomsDuty = "CDS";
		}

		#endregion
	}
}
