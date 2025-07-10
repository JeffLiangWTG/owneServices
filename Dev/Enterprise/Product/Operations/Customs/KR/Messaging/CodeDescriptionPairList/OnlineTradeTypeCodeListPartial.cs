namespace Enterprise.Customs.KR.Messaging
{
	partial class OnlineTradeTypeCodeList
	{
		public static bool IsIdentifiableType(string code)
		{
			return code == Codes.A || code == Codes.B || code == Codes.C;
		}
	}
}
