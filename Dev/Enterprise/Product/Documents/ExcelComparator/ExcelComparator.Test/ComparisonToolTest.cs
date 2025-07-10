using System.IO;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.ExcelComparator.Testing
{
	sealed class ComparisonToolTest : TestCase
	{
		public void TestConstruction()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var file1XlsPath = resourceRetriever.SaveResourceToFile("Enterprise.ExcelComparator.Testing.TestFiles.File1.xls", "File1.xls");
				var tool = new ComparisonTool(file1XlsPath);
				AssertEquals("tool.IsInstalled() for a File That Exists", true, tool.IsInstalled());

				tool = new ComparisonTool(Path.Combine(TempForTest.TempPath, "NonExistant.xls"));
				AssertEquals("tool.IsInstalled() for a File The Does Not Exist", false, tool.IsInstalled());
			}
		}

		public void TestMockTool()
		{
			var tool = new MockComparisonTool();
			AssertEquals("tool.IsInstalled()", true, tool.IsInstalled());
			AssertEquals(@"C:\Program Files\Mock Comparison Tool\Compare.exe", tool.Path);
			AssertEquals(null, tool.LastComparisonFilePath1);
			AssertEquals(null, tool.LastComparisonFilePath2);

			var tempPath = ComparableTextGenerator.GetTempFilePath();
			var tempFilePath1 = Path.Combine(tempPath, "File1");
			var tempFilePath2 = Path.Combine(tempPath, "File2");
			var tempFilePath3 = Path.Combine(tempPath, "File3");
			var tempFilePath4 = Path.Combine(tempPath, "File4");
			File.WriteAllText(tempFilePath1, @"
Text A
Text B");
			File.WriteAllText(tempFilePath2, @"
Text A
Text B");
			File.WriteAllText(tempFilePath3, @"
Text B
Text A");
			File.WriteAllText(tempFilePath4, @"
Text B
Text B
Text C");

			tool.RunComparison(tempFilePath1, tempFilePath2);
			AssertEquals("tool.LastComparisonFilePath1", tempFilePath1, tool.LastComparisonFilePath1);
			AssertEquals("tool.LastComparisonFilePath2", tempFilePath2, tool.LastComparisonFilePath2);
			AssertMultilineASCIIEquals("tool.Differences", @"
".Trim(), tool.Differences);

			tool.RunComparison(tempFilePath3, tempFilePath4);
			AssertEquals("tool.LastComparisonFilePath1", tempFilePath3, tool.LastComparisonFilePath1);
			AssertEquals("tool.LastComparisonFilePath2", tempFilePath4, tool.LastComparisonFilePath2);
			AssertMultilineASCIIEquals("tool.Differences", @"
line:[2]   file1:[Text A]   file2:[Text B]
line:[3]   file1:[]   file2:[Text C]
".Trim(), tool.Differences);

			tool.SetInstalled(false);
			AssertEquals("tool.IsInstalled()", false, tool.IsInstalled());
		}
	}
}
