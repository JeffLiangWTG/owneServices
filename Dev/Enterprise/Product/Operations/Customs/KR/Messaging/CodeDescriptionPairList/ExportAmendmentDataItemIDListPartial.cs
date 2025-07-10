namespace Enterprise.Customs.KR.Messaging
{
	partial class ExportAmendmentDataItemIDList
	{
		public static bool IsDateTimeField(string code)
		{
			return code == Codes.AB01
				|| code == Codes.A904
				|| code == Codes.A905
				|| code == Codes.A606
				|| code == Codes.A608
				|| code == Codes.H103
				|| code == Codes.H104
				|| code == Codes.G105;
		}

		public static bool IsHeaderDataItem(string code) => code.StartsWith(firstCharacterHeaderDataItem);
		const string firstCharacterHeaderDataItem = "A";
	}
}
