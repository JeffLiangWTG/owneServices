namespace Enterprise.Customs.KR.Messaging
{
	partial class DutyTaxCorrectionCodeList
	{
		public static bool Is5UARelevant(string code)
		{
			return code == Codes.A ||
				code == Codes.B;
		}
		public static bool Is5ULRelevant(string code)
		{
			return code == Codes.C;
		}
	}
}
