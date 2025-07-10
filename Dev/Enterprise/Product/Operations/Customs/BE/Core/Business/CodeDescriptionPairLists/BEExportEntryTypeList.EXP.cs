using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business
{
	public partial class BEExportEntryTypeList : CodeDescriptionPairList
	{
		public static CodeDescriptionPairList GetExportEntryTypeList(string declarationSubStyle)
		{
			var codeDescriptionPairList = new BEExportEntryTypeList();
			if (!declarationSubStyle.Equals(EntrySubStyleList.Codes.PreliminaryStandardDeclarationUnderCodeA) &&
				!declarationSubStyle.Equals(EntrySubStyleList.Codes.PreliminarySimplifiedDeclarationUnderCodeB) &&
				!declarationSubStyle.Equals(EntrySubStyleList.Codes.PreliminarySimplifiedDeclarationUnderCodeC))
			{
				codeDescriptionPairList.RemoveCode(BEExportEntryTypeList.Codes.PresentationNotification);
			}

			return codeDescriptionPairList;
		}
	}
}
