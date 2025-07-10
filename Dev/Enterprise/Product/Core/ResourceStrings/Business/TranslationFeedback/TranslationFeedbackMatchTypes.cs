using Enterprise.ZArchitecture.Core;

namespace Enterprise.ResourceStrings.Business
{
	public static class TranslationFeedbackMatchTypes
	{
		public static CodeDescriptionPairList List
		{
			get
			{
				var matchTypes = new CodeDescriptionPairList();
				matchTypes.AddPair(Codes.Exact, "Exact Match");
				matchTypes.AddPair(Codes.RelatedLevel, "Related Level");
				matchTypes.AddPair(Codes.RecentlyUsed, "Recently Used Match");
				matchTypes.AddPair(Codes.FullText, "Full Text Search Match");
				matchTypes.AddPair(Codes.OtherContext, "Other Context");
				return matchTypes;
			}
		}

		public static class Codes
		{
			public const string Exact = "*";
			public const string RelatedLevel = "+";
			public const string RecentlyUsed = "~";
			public const string FullText = "?";
			public const string OtherContext = "O";
			public const string None = "-";
		}
	}
}
