using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Integration
{
	public static class PrepaidCollectFreightForwardingList
	{
		public static class Codes
		{
			public const string All = PrepaidCollectList.Codes.All;
			public const string PPD = Core.Constants.PaymentType.Prepaid;
			public const string CCX = Core.Constants.PaymentType.Collect;
			public const string CTS = PrepaidCollectList.Codes.CTS;
			public const string FDT = "FDT";
			public const string FOG = "FOG";
			public const string LDT = "LDT";
			public const string LOG = "LOG";
		}

		public static class Descriptions
		{
			public readonly static MultilingualString All = PrepaidCollectList.Descriptions.All;
			public readonly static MultilingualString CTS = PrepaidCollectList.Descriptions.CTS;
			public readonly static MultilingualString FDT = ResString.GetMultilingualString("016ae546-55e8-4031-8488-74aaa93d2646", "Foreign Destination");
			public readonly static MultilingualString FOG = ResString.GetMultilingualString("43f89284-221d-4eb5-ba85-9f8b76f3a8cb", "Foreign Origin");
			public readonly static MultilingualString LDT = ResString.GetMultilingualString("7f5479a5-2ccc-4fb6-b6c7-695799bce57d", "Local Destination");
			public readonly static MultilingualString LOG = ResString.GetMultilingualString("34de0c91-8451-4365-9bbc-4c7978af56e3", "Local Origin");
		}
	}
}
