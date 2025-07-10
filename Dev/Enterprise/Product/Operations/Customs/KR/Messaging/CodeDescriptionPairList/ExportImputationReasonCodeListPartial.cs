namespace Enterprise.Customs.KR.Messaging
{
	partial class ExportImputationReasonCodeList
	{
		public static bool IsGoodForSelfDeclaringSupplier(string code)
		{
			return code == ExportImputationReasonCodeList.Codes.C
				|| code == ExportImputationReasonCodeList.Codes.D
				|| code == ExportImputationReasonCodeList.Codes.G
				|| code == ExportImputationReasonCodeList.Codes.Z
				|| code == ExportImputationReasonCodeList.Codes.E;
		}
	}
}
