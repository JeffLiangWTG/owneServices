namespace Enterprise.Customs.KR.Messaging
{
	partial class ImportAmendmentDataItemIDList
	{
		public static bool IsDateTimeField(string code)
		{
			return code == Codes.A604
				|| code == Codes.A605
				|| code == Codes.A704
				|| code == Codes.D107
				|| code == Codes.F104;
		}

		public static bool IsDutyTaxItemField(string code)
		{
			return code == Codes.A815
				|| code == Codes.A816
				|| code == Codes.A817
				|| code == Codes.A818
				|| code == Codes.A819
				|| code == Codes.A820
				|| code == Codes.A821
				|| code == Codes.A822
				|| code == Codes.A823;
		}

		public static bool IsHeaderDataItem(string code) => code.StartsWith(firstCharacterHeaderDataItem);
		const string firstCharacterHeaderDataItem = "A";
	}
}
