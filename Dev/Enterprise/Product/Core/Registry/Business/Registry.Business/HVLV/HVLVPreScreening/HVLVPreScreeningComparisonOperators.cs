using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Freight.Shipment
{
	public class HVLVPreScreeningComparisonOperators : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Contains = nameof(Contains);
			public const string ExactMatch = nameof(ExactMatch);
			public const string StartsWith = nameof(StartsWith);
			public const string EndsWith = nameof(EndsWith);
		}

		public static class Descriptions
		{
			public static MultilingualString Contains { get { return ResString.GetMultilingualString("abcb69eb-b39f-4bec-a3c8-27d83fe01efd", "Contains"); } }
			public static MultilingualString ExactMatch { get { return ResString.GetMultilingualString("53f6c73c-e2f5-4e6e-a12b-752e2c005b6c", "Exact Match"); } }
			public static MultilingualString StartsWith { get { return ResString.GetMultilingualString("d2161a60-b25b-4986-be33-9c51380b5935", "Starts With"); } }
			public static MultilingualString EndsWith { get { return ResString.GetMultilingualString("7cafb817-d440-4f7a-a8ae-07746aeb5f50", "Ends With"); } }
		}

		public HVLVPreScreeningComparisonOperators()
		{
			AddPair(Codes.Contains, Descriptions.Contains);
			AddPair(Codes.ExactMatch, Descriptions.ExactMatch);
			AddPair(Codes.StartsWith, Descriptions.StartsWith);
			AddPair(Codes.EndsWith, Descriptions.EndsWith);
		}
	}
}
