using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Business
{
	public class UPEOtherLocoMapSystemUsageList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Ups = "UPS";
		}

		public static class Descriptions
		{
			public const string Ups = "UPS";
		}

		public UPEOtherLocoMapSystemUsageList()
		{
			AddPair(Codes.Ups, Descriptions.Ups);
		}
	}
}

