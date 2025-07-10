namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUCChapterTariffTest : TariffTestCase
	{
		public void TestGetHierarchyForChapterOnly()
		{
			var chapterHierarchy = chapter.GetHierarchy();
			AssertEquals("Hierarchy depth", 2, chapterHierarchy.Length);
			AssertEquals("Chapter", chapter, chapterHierarchy[0]);
			AssertEquals("Section", section, chapterHierarchy[1]);
		}
	}
}
