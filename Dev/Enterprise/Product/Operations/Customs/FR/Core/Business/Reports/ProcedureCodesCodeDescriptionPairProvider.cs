using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Reports
{
	public class ProcedureCodesCodeDescriptionPairProvider : CodeDescriptionPairList,
			Integration.Customs.FR.IProcedureCodesCodeDescriptionPairProvider,
			DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(new DeltaGImportDeclarationTypeList());
			result.AddRangeOverwriteIfExists(new DeltaGExportDeclarationTypeList());

			result.Sort();
			return result;
		}
	}
}
