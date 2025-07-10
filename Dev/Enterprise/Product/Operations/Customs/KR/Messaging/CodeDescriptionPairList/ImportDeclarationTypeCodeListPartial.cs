namespace Enterprise.Customs.KR.Messaging
{
	partial class ImportDeclarationTypeCodeList
	{
		public static bool IsSimpleDeclarationType(string code)
		{
			return code == Codes.C || code == Codes.D || code == Codes.E;
		}
	}
}
