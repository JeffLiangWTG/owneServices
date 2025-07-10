namespace Enterprise.Customs.KR.Messaging
{
	partial class FTAAmendmentDataItemIDList
	{
		public static bool IsDateTimeField(string code)
		{
			return code == Codes._11F
				|| code == Codes._14
				|| code == Codes._16;
		}
	}
}
