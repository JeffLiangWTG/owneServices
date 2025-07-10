using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Messaging
{
	partial class ExportTypeCodeList :
		Integration.Customs.KR.IKRExportTypeCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		ReadOnlyCodeDescriptionPairList DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider.GetCodeDescriptionPairList() => new ExportTypeCodeList();

		public static bool IsUnderbondPeriodRequired(string code)
		{
			return code == Codes.B
				|| code == Codes.D
				|| code == Codes.E;
		}
	}
}
