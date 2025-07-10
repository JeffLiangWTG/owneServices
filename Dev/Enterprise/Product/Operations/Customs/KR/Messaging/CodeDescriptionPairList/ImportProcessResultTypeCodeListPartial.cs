namespace Enterprise.Customs.KR.Messaging
{
	partial class ImportProcessResultTypeCodeList
	{
		public static bool IsReleasedFromCustomsControl(string code)
		{
			return code == Codes._13
				|| code == Codes._14
				|| code == Codes._15;
		}
	}
}
