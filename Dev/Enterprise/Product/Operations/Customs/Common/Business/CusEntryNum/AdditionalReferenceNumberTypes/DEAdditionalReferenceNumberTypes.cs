using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common
{
	public class GermanyAdditionalReferenceNumberTypes : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string SZBNumber = "SZB";
		}

		public static class Descriptions
		{
			public static MultilingualString SZBNumber { get { return ResString.GetMultilingualString("GermanyAdditionalReferenceNumberTypes|SZBNumber", "SZB Number"); } }
		}

		public GermanyAdditionalReferenceNumberTypes()
		{
			Add(new CustomsNumberTypeCodeDescription(Codes.SZBNumber, Descriptions.SZBNumber, false));
		}
	}
}
