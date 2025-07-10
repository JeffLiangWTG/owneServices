using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.PAVE.MENT.Business.Test
{
	class MENTAgedScoreExtractorResultTest : TestCaseWithFactory
	{
		public void TestExtractorCategoryMapMergeAndOrder()
		{
			var row1 = MENTTestHelper.CreateMENTDataRow("s1", "10", 10m);
			var row2 = MENTTestHelper.CreateMENTDataRow("s1", "12", 10m);
			var row3 = MENTTestHelper.CreateMENTDataRow("s1", "13", 10m);
			var row4 = MENTTestHelper.CreateMENTDataRow("s2", "15", 10m);
			var row5 = MENTTestHelper.CreateMENTDataRow("s2", "16", 10m);
			var row6 = MENTTestHelper.CreateMENTDataRow("s1", "11", 10m);
			var row7 = MENTTestHelper.CreateMENTDataRow("s1", "13", 10m);
			var row8 = MENTTestHelper.CreateMENTDataRow("s1", "14", 10m);
			var row9 = MENTTestHelper.CreateMENTDataRow("s2", "16", 10m);
			var row10 = MENTTestHelper.CreateMENTDataRow("s2", "17", 10m);

			var items1 = new Collection<MENTDataRow> { row1, row2, row3, row4, row5 };
			var items2 = new Collection<MENTDataRow> { row6, row7, row8, row9, row10 };

			var extractionResult1 = new MENTAgedScoreExtractionResult(items1, "test1");
			var extractionResult2 = new MENTAgedScoreExtractionResult(items2, "test2");

			var extractorResult = new MENTAgedScoreExtractorResult(new[] { extractionResult1, extractionResult2 });

			var map = extractorResult.ResultCategoryIndexMap.ToList();
			AssertEquals(8, map.Count);
			var expectedRows = new[] { "10", "11", "12", "13", "14", "15", "16", "17" };
			var actualRows = map.Select(c => c.CategoryLabel).ToList();
			Assert(expectedRows.SequenceEqual(actualRows));

			var indexs = map.Select(c => c.Index);

			var expectedIndexs = Enumerable.Range(0, map.Count);

			Assert(expectedIndexs.SequenceEqual(indexs));
		}

		public void TestExtractorIntConversion()
		{
			var row1 = MENTTestHelper.CreateMENTDataRow("s1", new IZType[] { new ZInt(1) }, 10m);
			var items1 = new Collection<MENTDataRow> { row1 };
			var extractionResult1 = new MENTAgedScoreExtractionResult(items1, "test1");

			var extractorResult = new MENTAgedScoreExtractorResult(new[] { extractionResult1 });

			var map = extractorResult.ResultCategoryIndexMap.ToList();
			var actualRow = map.Select(c => c.CategoryLabel).First();

			AssertEquals("1", actualRow);
		}
	}
}
