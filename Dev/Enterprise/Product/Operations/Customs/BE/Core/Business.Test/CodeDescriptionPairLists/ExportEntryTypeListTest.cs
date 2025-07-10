using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing
{
	class ExportEntryTypeListTest : TestCase
	{
		public void TestPresentationNotification()
		{
			CombineAssertions(() =>
			{
				AssertEquals("PreliminaryDeclarationUnderCodeA", true, BEExportEntryTypeList.GetExportEntryTypeList(EntrySubStyleList.Codes.PreliminaryStandardDeclarationUnderCodeA).ContainsCode(BEExportEntryTypeList.Codes.PresentationNotification));
				AssertEquals("PreliminaryDeclarationUnderCodeB", true, BEExportEntryTypeList.GetExportEntryTypeList(EntrySubStyleList.Codes.PreliminarySimplifiedDeclarationUnderCodeB).ContainsCode(BEExportEntryTypeList.Codes.PresentationNotification));
				AssertEquals("PreliminaryDeclarationUnderCodeC", true, BEExportEntryTypeList.GetExportEntryTypeList(EntrySubStyleList.Codes.PreliminarySimplifiedDeclarationUnderCodeC).ContainsCode(BEExportEntryTypeList.Codes.PresentationNotification));
				AssertEquals("NormalDeclaration", false, BEExportEntryTypeList.GetExportEntryTypeList(EntrySubStyleList.Codes.StandardDeclaration).ContainsCode(BEExportEntryTypeList.Codes.PresentationNotification));
			});
		}
	}
}
