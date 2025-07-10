namespace Enterprise.Customs.IE.PBN.Business
{
	public partial class PBNCustomsStatusList
	{
		public static class EnglishDescriptions
		{
			public const string CheckedIn = "CHECKED_IN";
			public const string Incomplete = "INCOMPLETE";
			public const string Proceed = "PROCEED";
			public const string Rejected = "REJECTED";
		}

		public static string GetCodeFromEnglishDescription(string englishDescription) => englishDescription.Trim() switch
		{
			EnglishDescriptions.CheckedIn => Codes.CheckedIn,
			EnglishDescriptions.Incomplete => Codes.Incomplete,
			EnglishDescriptions.Proceed => Codes.Proceed,
			EnglishDescriptions.Rejected => Codes.Rejected,
			_ => null,
		};
	}
}
