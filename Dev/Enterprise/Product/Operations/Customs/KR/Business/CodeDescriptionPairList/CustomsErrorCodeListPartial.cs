namespace Enterprise.Customs.KR.Business
{
	partial class CustomsErrorCodeList
	{
		public static bool IsInvalidGlbExternalPassword(string code)
		{
			return code == Codes.C401
				|| code == Codes.C402
				|| code == Codes.C450;
		}
	}
}
