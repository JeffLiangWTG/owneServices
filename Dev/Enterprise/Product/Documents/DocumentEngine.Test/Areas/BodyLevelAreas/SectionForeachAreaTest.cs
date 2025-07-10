using System.Linq;
using Enterprise.DocumentEngine.Testing;

namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class SectionForeachAreaTest : TreeDataAreaAbstractTest
	{
		public void TestSectionBody()
		{
			var sectionBody = new SectionBodyArea(1, 10, TestReport, "");
			var foreachArea1 = new SectionForeachArea(2, 7, TestReport, "#BeginLoop:Data=DummyCollection", sectionBody, sectionBody);
			var foreachArea2 = new SectionForeachArea(8, 9, TestReport, "#BeginLoop:Data=DummyCollection", sectionBody, sectionBody);
			var foreachArea3 = new SectionForeachArea(3, 4, TestReport, "#BeginLoop:Data=DummyCollection", foreachArea1, sectionBody);
			var foreachArea4 = new SectionForeachArea(5, 6, TestReport, "#BeginLoop:Data=DummyCollection", foreachArea1, sectionBody);

			AssertEquals(sectionBody, foreachArea1.SectionBody);
			AssertEquals(sectionBody, foreachArea2.SectionBody);
			AssertEquals(sectionBody, foreachArea3.SectionBody);
			AssertEquals(sectionBody, foreachArea4.SectionBody);
		}

		public void TestIsSectionForeachAreaStart()
		{
			Assert(SectionForeachArea.IsSectionForeachAreaBeginStart("#BeginLoop"));
			Assert(SectionForeachArea.IsSectionForeachAreaBeginStart("#BeginLooP:asdf"));
			Assert(SectionForeachArea.IsSectionForeachAreaEndStart("#EndLoop"));
			Assert(SectionForeachArea.IsSectionForeachAreaEndStart("#EndLooP"));
		}

		public void TestProcessSectionForeachAreas()
		{
			var templateContents =
				@"{A}-[#config]
{A}-[#DocumentHeader]
{A}-[#SectionBody]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#EndLoop]
{A}-[#EndLoop]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#EndLoop]
{A}-[#EndLoop]
{A}-[#BeginLoop:Data=DummyCollection]
{A}-[#EndLoop]
{A}-[#EndOfReport]";

			using (var report = new Report(DocumentPack.EmptyPack, DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, templateContents)))
			{
				report.PrepareForRender();
				report.Renderer.Render();
				var sectionBody = report.Analyser.Areas.OfType<SectionBodyArea>().First();

				AssertEquals(2, sectionBody.Children.Count);
				AssertEquals(5, sectionBody.AllChildForeachAreas.Count);

				var node1 = sectionBody.Children[0] as SectionForeachArea;
				AssertEquals(2, node1.Children.Count);

				var node2 = sectionBody.Children[1] as SectionForeachArea;
				AssertEquals(0, node2.Children.Count);

				var node11 = node1.Children[0] as SectionForeachArea;
				AssertEquals(1, node11.Children.Count);
			}
		}

		public void TestHasCorrectParent()
		{
			var treeArea = GetNewAreaToTest() as TreeDataArea;
			AssertNotNull(treeArea.Parent);
		}

		protected override Area GetNewAreaToTest()
		{
			var bodyArea = new SectionBodyArea(1, 10, TestReport, "");
			return new SectionForeachArea(1, 10, TestReport, "#BeginLoop:Data=DummyCollection", bodyArea, bodyArea);
		}
	}
}
