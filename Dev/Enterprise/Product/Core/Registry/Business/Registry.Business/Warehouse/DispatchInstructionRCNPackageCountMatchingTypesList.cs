using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Warehouse
{
	public partial class DispatchInstructionRCNPackageCountMatchingTypesList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Typical = "TYP";
			public const string Count = "COU";
			public const string HBL = "HBL";
		}

		public static class Descriptions
		{
			public static MultilingualString Typical { get { return ResString.GetMultilingualString("CBE2E598-818A-45A5-A343-1657A88457AA", "RCN Reference + Pack Count + Pack Type"); } }
			public static MultilingualString Count { get { return ResString.GetMultilingualString("2F55396C-9B4C-4F39-A36C-24CEF6E0AF19", "RCN Reference + Pack Count Only (Ignore Pack Type)"); } }
			public static MultilingualString HBL { get { return ResString.GetMultilingualString("EB5DEB64-D223-461B-84BA-863FDD3B0CE9", "RCN Reference (Ignore Pack Count + Pack Type)"); } }
		}

		public DispatchInstructionRCNPackageCountMatchingTypesList()
		{
			AddPair(Codes.Typical, Descriptions.Typical);
			AddPair(Codes.Count, Descriptions.Count);
			AddPair(Codes.HBL, Descriptions.HBL);
			DefaultCode = Codes.Typical;
		}
	}
}
