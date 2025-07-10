using System.Diagnostics;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Visualisation.Testing
{
	sealed class NonVisualisableMacroCleanerTest : TestCase
	{
		[DeveloperOnlyTest]
		public void TestRemoveMacrosUsedForFormattingOnly_Performance()
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			for (var i = 0; i < 1000000; i++)
			{
				NonVisualisableMacroCleaner.RemoveMacrosUsedForFormattingOnly("<ExpandToFit>Hello World");
			}
			AssertLessThan(stopWatch.ElapsedMilliseconds, 2000);
		}

		public void TestCleanupMacros()
		{
			AssertEquals("Hello World", NonVisualisableMacroCleaner.RemoveMacrosUsedForFormattingOnly("<ExpandToFit>Hello World"));
			AssertEquals("Hello World", NonVisualisableMacroCleaner.RemoveMacrosUsedForFormattingOnly("Hello World<DataType(\"System.Decimal\")>"));
			AssertEquals("12345", NonVisualisableMacroCleaner.RemoveMacrosUsedForFormattingOnly("1< Auto Height >2<HideRowIfCellIsEmpty>3<HideRowIf(1==2)>4<HPageBreak>5< Shrink To Fit >"));
			AssertEquals("12345", NonVisualisableMacroCleaner.RemoveMacrosUsedForFormattingOnly("1< Auto Height >2<HideRowIfCellIsEmpty>3<HideRowIf(1==2)>4<HPageBreak>5< ShrinkToFitForBillOfLading >"));
			AssertEquals("1\n2", NonVisualisableMacroCleaner.RemoveMacrosUsedForFormattingOnly("1< Auto Height >\n2\r\n\r\n"));
			AssertEquals("1\n2", NonVisualisableMacroCleaner.RemoveMacrosUsedForFormattingOnly("1<OverFlowToFollowPage(\"Title\")>\n2\r\n\r\n"));
		}
	}
}
