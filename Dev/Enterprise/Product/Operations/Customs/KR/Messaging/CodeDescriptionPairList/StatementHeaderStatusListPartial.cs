namespace Enterprise.Customs.KR.Messaging
{
	partial class StatementHeaderStatusList
	{
		public static bool IsResultingFromAmendment(string code)
		{
			return code == Codes.A
				|| code == Codes.B
				|| code == Codes.O;
		}
	}
}
