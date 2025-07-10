using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Reports;

public class EntryInstructionStyleListProvider : Integration.Customs.IT.IEntryInstructionStyleListProvider, DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
{
	public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
	{
		var list = new SADDeclarationTypeList();
		list.Sort();
		return list;
	}
}
