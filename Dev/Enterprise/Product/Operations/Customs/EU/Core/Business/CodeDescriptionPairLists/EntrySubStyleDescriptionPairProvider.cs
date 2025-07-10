using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public class EntrySubStyleDescriptionPairProvider : Integration.Customs.EU.IEntrySubStyleDescriptionPairProvider, DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return new EntrySubStyleList();
		}
	}
}
