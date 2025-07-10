using System;
using System.Drawing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngine.Visualisation.Testing
{
	sealed class GUILocationsAndSizesProviderTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestGetCellSizeForGUI()
		{
			using (var xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(TestXlsFilePath);
				var sizeProvider = new GUILocationsAndSizesProvider(xlInterface.WorkSheets[0]);

				AssertEquals(ControlDpiScalingHelper.NewScaledSize(new Size(114, 21)), sizeProvider.GetCellSizeForGUI(2, 2, 0, 0));
				AssertEquals(ControlDpiScalingHelper.NewScaledSize(new Size(268, 21)), sizeProvider.GetCellSizeForGUI(2, 2, 3, 3));
				AssertEquals(ControlDpiScalingHelper.NewScaledSize(new Size(416, 63)), sizeProvider.GetCellSizeForGUI(2, 4, 0, 3));
				AssertEquals(ControlDpiScalingHelper.NewScaledSize(new Size(416, 63)), sizeProvider.GetCellSizeForGUI(2, 4, 0, 3, true));
				AssertEquals(ControlDpiScalingHelper.NewScaledSize(new Size(502, 21)), sizeProvider.GetCellSizeForGUI(2, 2, 3, 8));
				AssertEquals(ControlDpiScalingHelper.NewScaledSize(new Size(502, 21)), sizeProvider.GetCellSizeForGUI(2, 2, 3, 8, true));
				AssertEquals(ControlDpiScalingHelper.NewScaledSize(new Size(502, 21)), sizeProvider.GetCellSizeForGUI(2, 2, 4, 8));
				AssertEquals(ControlDpiScalingHelper.NewScaledSize(new Size(410, 21)), sizeProvider.GetCellSizeForGUI(2, 2, 4, 8, true));
			}
		}

		public void TestGetCellSizeForGUIWithHiddenColumns()
		{
			using (var xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(TestXlsWithHiddenColumnFilePath);
				var sizeProvider = new GUILocationsAndSizesProvider(xlInterface.WorkSheets[0]);

				AssertEquals(ControlDpiScalingHelper.NewScaledSize(new Size(6, 120)), sizeProvider.GetCellSizeForGUI(7, 7, 1, 1));
				AssertEquals(ControlDpiScalingHelper.NewScaledSize(new Size(325, 17)), sizeProvider.GetCellSizeForGUI(7, 7, 2, 24));
				AssertEquals(ControlDpiScalingHelper.NewScaledSize(new Size(3, 17)), sizeProvider.GetCellSizeForGUI(7, 7, 25, 25));
				AssertEquals(ControlDpiScalingHelper.NewScaledSize(new Size(516, 17)), sizeProvider.GetCellSizeForGUI(7, 7, 26, 48));
			}
		}

		public void TestGetUpperLeftCornerOfCellForGUI()
		{
			using (var xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(TestXlsFilePath);
				var sizeProvider = new GUILocationsAndSizesProvider(xlInterface.WorkSheets[0]);
				var helper = new GetUpperLeftForGUITestHelper(sizeProvider);

				helper.AppendLocation(new Point(0, 100), 1, 1, 1, 1200);
				helper.AppendBlankLine();
				helper.AppendLocation(new Point(107, 100), 1, 1, 2, 1200);
				helper.AppendLocation(new Point(210, 100), 1, 1, 3, 1200);
				helper.AppendLocation(new Point(302, 100), 1, 1, 4, 1200);
				helper.AppendBlankLine();
				helper.AppendLocation(new Point(210, 131), 1, 1, 3, 1580);
				helper.AppendBlankLine();
				helper.AppendLocation(new Point(0, 121), 1, 2, 1, 1200);
				helper.AppendLocation(new Point(0, 142), 1, 3, 1, 1200);
				helper.AppendLocation(new Point(0, 163), 1, 4, 1, 1200);
				helper.AppendBlankLine();
				helper.AppendLocation(new Point(210, 131), 1, 1, 3, 1580);

				helper.AppendBlankLine();
				helper.AppendBlankLine();

				helper.AppendLocation(new Point(0, 100), 1, 1, 1, 1200);
				helper.AppendBlankLine();
				helper.AppendLocation(new Point(107, 100), 1, 1, 2, 1200);
				helper.AppendLocation(new Point(210, 100), 1, 1, 3, 1200);
				helper.AppendLocation(new Point(302, 100), 1, 1, 4, 1200);
				helper.AppendBlankLine();
				helper.AppendLocation(new Point(210, 131), 1, 1, 3, 1580);
				helper.AppendBlankLine();
				helper.AppendLocation(new Point(0, 121), 1, 2, 1, 1200);
				helper.AppendLocation(new Point(0, 142), 1, 3, 1, 1200);
				helper.AppendLocation(new Point(0, 163), 1, 4, 1, 1200);
				helper.AppendBlankLine();
				helper.AppendLocation(new Point(210, 131), 1, 1, 3, 1580);

				helper.AssertResults();
			}
		}

		public void TestAutoHeightCellForGui()
		{
			using (var xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.AutoHeightInPageHeader.xls"));
				var worksheet = xlInterface.WorkSheets[0];
				var sizeProvider = new GUILocationsAndSizesProvider(worksheet);

				AssertContains("worksheet[19, 4] should contain <AutoHeight>.", "<AutoHeight>", worksheet[19, 4].ToString());
				AssertEquals("workSheet.GetCellSizeForGUI(19, 19, 4, 4).Height", ControlDpiScalingHelper.ScaleToCurrentDpiY(60), sizeProvider.GetCellSizeForGUI(19, 19, 4, 4).Height);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		string testXlsFilePath;
		string TestXlsFilePath
		{
			get
			{
				if (string.IsNullOrEmpty(testXlsFilePath))
				{
					testXlsFilePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.test.xls");
				}
				return testXlsFilePath;
			}
		}

		string testXlsWithHiddenColumnFilePath;
		string TestXlsWithHiddenColumnFilePath
		{
			get
			{
				if (string.IsNullOrEmpty(testXlsWithHiddenColumnFilePath))
				{
					testXlsWithHiddenColumnFilePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.TestTemplateWithHiddenColumn.xls");
				}
				return testXlsWithHiddenColumnFilePath;
			}
		}
		sealed class GetUpperLeftForGUITestHelper
		{
			public GetUpperLeftForGUITestHelper(GUILocationsAndSizesProvider sizeProvider)
			{
				this.sizeProvider = sizeProvider;
			}
			readonly GUILocationsAndSizesProvider sizeProvider;

			readonly ZStringBuilder expected = new ZStringBuilder();
			readonly ZStringBuilder actual = new ZStringBuilder();

			internal void AppendLocation(Point expectedResult, int startOfArea, int rowNumber, int columnNumber, int yOffset)
			{
				var idString = "StartOfArea: " + startOfArea.ToString() + "   RowNumber: " + rowNumber.ToString() + "   ColumnNumber: " + columnNumber.ToString() + "   YOffset: " + yOffset.ToString();
				expected.Append(idString + "   --- Result: " + ControlDpiScalingHelper.NewScaledPoint(expectedResult.X, expectedResult.Y).ToString());
				actual.Append(idString + "   --- Result: " + sizeProvider.GetUpperLeftCornerOfCellForGUI(startOfArea, rowNumber, columnNumber, yOffset).ToString());
			}

			internal void AppendBlankLine()
			{
				expected.Append("");
				actual.Append("");
			}

			public void AssertResults()
			{
				AssertMultilineASCIIEquals("All values for all cells should match.", expected.ToStringWithNewLineBetweenAppends(), actual.ToStringWithNewLineBetweenAppends());
			}
		}
	}
}
