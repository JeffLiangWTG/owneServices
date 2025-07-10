namespace Enterprise.Customs.KR.Messaging
{
	public partial class CustomsBrokerInspectionOpinionStatementTypeCodeList
	{
		public static bool IsInspectionRequired(string code)
		{
			return code == Codes.B
				|| code == Codes.C
				|| code == Codes.D
				|| code == Codes.E;
		}
	}
}
