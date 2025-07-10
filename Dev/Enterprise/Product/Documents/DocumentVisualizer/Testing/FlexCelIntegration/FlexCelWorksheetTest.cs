using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration.Testing
{
	sealed class FlexCelWorksheetTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetCellOutOfRange()
		{
			IWorksheet worksheet = TestFiles.CreateWorksheet();

			AssertNotNull("cell (0,0)", worksheet.GetCell(0, 0));
			AssertNotNull("cell (0,1)", worksheet.GetCell(0, 1));
			AssertNotNull("cell (1,0)", worksheet.GetCell(1, 0));
			AssertNotNull("cell (*,1)", worksheet.GetCell(worksheet.Rows.Count + 1, 1));
			AssertNotNull("cell (1,*)", worksheet.GetCell(1, worksheet.Columns.Count + 1));

			AssertEquals("cell (0,0) is cached", worksheet.GetCell(0, 0), worksheet.GetCell(0, 0));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCacheCells()
		{
			IWorksheet worksheet = TestFiles.CreateWorksheet();

			var cell = worksheet.GetCell(1, 1);

			AssertNotNull("cell is not null", cell);
			Assert("flexcel cell", cell is FlexCelCell);

			AssertEquals("cell was cached", cell, worksheet.GetCell(1, 1));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTemplateCellBackgroundColor()
		{
			IWorksheet worksheet = TestFiles.CreateWorksheet(TestFiles.ColorsTestTemplateFilePath);

			var cell1 = worksheet.GetCell(5, 2);
			var cell2 = worksheet.GetCell(5, 3);
			var cell3 = worksheet.GetCell(5, 4);

			AssertNotNull("cell [5,2] is not null", cell1);
			AssertNotNull("cell [5,3] is not null", cell2);
			AssertNotNull("cell [5,4] is not null", cell3);

			AssertEquals("cell [5,2] color", Color.FromArgb(255, 217, 217, 217), cell1.Format.BackgroundColor);
			AssertEquals("cell [5,3] color", Color.FromArgb(255, 255, 192, 0), cell2.Format.BackgroundColor);
			AssertEquals("cell [5,4] color", Color.FromArgb(255, 255, 0, 0), cell3.Format.BackgroundColor);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestResources()
		{
			var worksheet = TestFiles.CreateWorksheet();

			var res = worksheet.Resources;

			AssertContainsExactElementsInAnyOrder("image paths",
				new[]
				{
					"sheet1/cat",
					"sheet1/squirrel",
					"sheet2/squirrel",
					"sheet2/elephant"
				},
				res.Keys);
		}

		#region TestWorksheet

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWorksheet_FromFilePath()
		{
			IWorksheet worksheet = TestFiles.CreateWorksheet();

			AssertWorksheet(worksheet);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWorksheet_FromStream()
		{
			using (var fileStream = File.OpenRead(TestFiles.XlsFilePath))
			{
				IWorksheet worksheet = FlexCelWorksheet.FromStream(fileStream);

				AssertWorksheet(worksheet);
			}
		}

		public void AssertWorksheet(IWorksheet worksheet)
		{
			AssertEquals("Columns", 7, worksheet.Columns.Count);
			AssertEquals("Rows", 9, worksheet.Rows.Count);

			AssertContainsExactElementsInAnyOrder("cells", new[]
			{
				"[1,1] [1,1]",
				"[1,2] [1,2]",
				"[1,3] [1,3]",
				"[1,4] [1,4]",
				"[1,5] [1,5]",
				"[1,6] [1,6]",
				"[1,7] [1,7]",

				"[2,1] [2,1] text:",
				"[2,2] [2,2] cell",
				"[2,3] [4,4] merged cell that needs to wrap nicely",
				"[2,3] [4,4] merged cell that needs to wrap nicely",
				"[2,5] [2,5]",
				"[2,6] [2,6] 90 degrees",
				"[2,7] [2,7] 180 degrees",

				"[3,1] [3,1]",
				"[3,2] [3,2]",
				"[2,3] [4,4] merged cell that needs to wrap nicely",
				"[2,3] [4,4] merged cell that needs to wrap nicely",

				"[3,5] [3,5]",
				"[3,6] [3,6]",
				"[3,7] [3,7]",

				"[4,1] [4,1] formula:",
				"[4,2] [4,2] evaluated if true",
				"[2,3] [4,4] merged cell that needs to wrap nicely",
				"[2,3] [4,4] merged cell that needs to wrap nicely",
				"[4,5] [4,5]",
				"[4,6] [4,6]",
				"[4,7] [4,7]",

				"[5,1] [5,1]",
				"[5,2] [5,2]",
				"[5,3] [5,3]",
				"[5,4] [5,4]",
				"[5,5] [5,5]",
				"[5,6] [5,6]",
				"[5,7] [5,7]",

				"[6,1] [6,1]",
				"[6,2] [6,2]",
				"[6,3] [6,3]",
				"[6,4] [6,4]",
				"[6,5] [6,5]",
				"[6,6] [6,6]",
				"[6,7] [6,7]",

				"[7,1] [7,1]",
				"[7,2] [7,2]",
				"[7,3] [7,3]",
				"[7,4] [7,4]",
				"[7,5] [7,5]",
				"[7,6] [7,6]",
				"[7,7] [7,7]",

				"[8,1] [8,1]",
				"[8,2] [8,2]",
				"[8,3] [8,3]",
				"[8,4] [8,4]",
				"[8,5] [8,5]",
				"[8,6] [8,6]",
				"[8,7] [8,7]",

				"[9,1] [9,1]",
				"[9,2] [9,2]",
				"[9,3] [9,3]",
				"[9,4] [9,4]",
				"[9,5] [9,5]",
				"[9,6] [9,6]",
				"[9,7] [9,7]"
			},
			GetCellValues(worksheet));

			var cell = worksheet.GetCell(2, 1);

			CombineAssertions(
				() =>
					{
						AssertEquals("[2,1] [2,1] Font.Size", 11f, cell.Format.Font.Size);
						AssertEquals("[2,1] [2,1] Font.Color", "Color [A=255, R=0, G=0, B=0]", cell.Format.Font.Color.ToString());
						AssertEquals("[2,1] [2,1] Font.Style", FontStyle.Regular, cell.Format.Font.Style);

						AssertEquals("[2,1] [2,1] Borders", false, cell.HasBorders());
						AssertEquals("[2,1] [2,1] Borders.TopRow", BorderStyle.None, cell.Format.Borders.Top.Style);
						AssertEquals("[2,1] [2,1] Borders.LeftColumn", BorderStyle.None, cell.Format.Borders.Left.Style);
						AssertEquals("[2,1] [2,1] Borders.BottomRow", BorderStyle.None, cell.Format.Borders.Bottom.Style);
						AssertEquals("[2,1] [2,1] Borders.RightColumn", BorderStyle.None, cell.Format.Borders.Right.Style);

						cell = worksheet.GetCell(2, 2);

						AssertEquals("[2,2] [2,2] Font.Size", 22f, cell.Format.Font.Size);
						AssertEquals("[2,2] [2,2] Font.Color", "Color [A=255, R=0, G=0, B=0]", cell.Format.Font.Color.ToString());
						AssertEquals("[2,2] [2,2] Font.Style", FontStyle.Bold, cell.Format.Font.Style);

						AssertEquals("[2,2] [2,2] Borders", false, cell.HasBorders());
						AssertEquals("[2,2] [2,2] Borders.TopRow", BorderStyle.None, cell.Format.Borders.Top.Style);
						AssertEquals("[2,2] [2,2] Borders.LeftColumn", BorderStyle.None, cell.Format.Borders.Left.Style);
						AssertEquals("[2,2] [2,2] Borders.BottomRow", BorderStyle.None, cell.Format.Borders.Bottom.Style);
						AssertEquals("[2,2] [2,2] Borders.RightColumn", BorderStyle.None, cell.Format.Borders.Right.Style);

						cell = worksheet.GetCell(2, 3);

						AssertEquals("[2,3] [4,4] Font.Size", 16f, cell.Format.Font.Size);
						AssertEquals("[2,3] [4,4] Font.Color", "Color [A=255, R=91, G=155, B=213]", cell.Format.Font.Color.ToString());
						AssertEquals("[2,3] [4,4] Font.Style", FontStyle.Regular, cell.Format.Font.Style);

						AssertEquals("[2,3] [4,4] Borders", true, cell.HasBorders());
						AssertEquals("[2,2] [2,2] Borders.TopRow", BorderStyle.Medium, cell.Format.Borders.Top.Style);
						AssertEquals("[2,2] [2,2] Borders.LeftColumn", BorderStyle.Thin, cell.Format.Borders.Left.Style);
						AssertEquals("[2,2] [2,2] Borders.BottomRow", BorderStyle.Thin, cell.Format.Borders.Bottom.Style);
						AssertEquals("[2,2] [2,2] Borders.RightColumn", BorderStyle.Thick, cell.Format.Borders.Right.Style);

						cell = worksheet.GetCell(4, 1);

						AssertEquals("[4,1] [4,1] Font.Size", 11f, cell.Format.Font.Size);
						AssertEquals("[4,1] [4,1] Font.Color", "Color [A=255, R=192, G=0, B=0]", cell.Format.Font.Color.ToString());
						AssertEquals("[4,1] [4,1] Font.Style", FontStyle.Italic, cell.Format.Font.Style);

						AssertEquals("[4,1] [4,1] Borders", false, cell.HasBorders());
						AssertEquals("[4,1] [4,1] Borders.TopRow", BorderStyle.None, cell.Format.Borders.Top.Style);
						AssertEquals("[4,1] [4,1] Borders.LeftColumn", BorderStyle.None, cell.Format.Borders.Left.Style);
						AssertEquals("[4,1] [4,1] Borders.BottomRow", BorderStyle.None, cell.Format.Borders.Bottom.Style);
						AssertEquals("[4,1] [4,1] Borders.RightColumn", BorderStyle.None, cell.Format.Borders.Right.Style);

						cell = worksheet.GetCell(4, 2);

						AssertEquals("[4,2] [4,2] Font.Size", 11f, cell.Format.Font.Size);
						AssertEquals("[4,2] [4,2] Font.Color", "Color [A=255, R=0, G=0, B=0]", cell.Format.Font.Color.ToString());
						AssertEquals("[4,2] [4,2] Font.Style", FontStyle.Regular, cell.Format.Font.Style);

						AssertEquals("[4,2] [4,2] Borders", false, cell.HasBorders());
						AssertEquals("[4,2] [4,2] Borders.TopRow", BorderStyle.None, cell.Format.Borders.Top.Style);
						AssertEquals("[4,2] [4,2] Borders.LeftColumn", BorderStyle.None, cell.Format.Borders.Left.Style);
						AssertEquals("[4,2] [4,2] Borders.BottomRow", BorderStyle.None, cell.Format.Borders.Bottom.Style);
						AssertEquals("[4,2] [4,2] Borders.RightColumn", BorderStyle.None, cell.Format.Borders.Right.Style);

						cell = worksheet.GetCell(6, 2);

						AssertEquals("[6,2] [6,2] Borders", true, cell.HasBorders());
						AssertEquals("[6,2] [6,2] Borders.TopRow", BorderStyle.Thin, cell.Format.Borders.Top.Style);
						AssertEquals("[6,2] [6,2] Borders.LeftColumn", BorderStyle.Thin, cell.Format.Borders.Left.Style);
						AssertEquals("[6,2] [6,2] Borders.BottomRow", BorderStyle.None, cell.Format.Borders.Bottom.Style);
						AssertEquals("[6,2] [6,2] Borders.RightColumn", BorderStyle.None, cell.Format.Borders.Right.Style);

						cell = worksheet.GetCell(7, 2);

						AssertEquals("[7,2] [7,2] Borders", true, cell.HasBorders());
						AssertEquals("[7,2] [7,2] Borders.TopRow", BorderStyle.None, cell.Format.Borders.Top.Style);
						AssertEquals("[7,2] [7,2] Borders.LeftColumn", BorderStyle.None, cell.Format.Borders.Left.Style);
						AssertEquals("[7,2] [7,2] Borders.BottomRow", BorderStyle.Thin, cell.Format.Borders.Bottom.Style);
						AssertEquals("[7,2] [7,2] Borders.RightColumn", BorderStyle.Thin, cell.Format.Borders.Right.Style);
					});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBorders_LeftRightDiagonalDown()
		{
			IWorksheet worksheet = TestFiles.CreateWorksheet(TestFiles.XlsFileForBordersTestPath);

			var cell = worksheet.GetCell(10, 2);

			AssertNotNull($"{cell} Left Border", cell.Format.Borders.Left);
			AssertEquals($"{cell} Left BorderStyle", BorderStyle.Thin, cell.Format.Borders.Left.Style);

			AssertNotNull($"{cell} Right Border", cell.Format.Borders.Right);
			AssertEquals($"{cell} Right BorderStyle", BorderStyle.Thin, cell.Format.Borders.Right.Style);

			AssertNotNull($"{cell} Top Border", cell.Format.Borders.Top);
			AssertEquals($"{cell} Top BorderStyle", BorderStyle.None, cell.Format.Borders.Top.Style);

			AssertNotNull($"{cell} Bottom Border", cell.Format.Borders.Bottom);
			AssertEquals($"{cell} Bottom BorderStyle", BorderStyle.None, cell.Format.Borders.Bottom.Style);

			AssertNotNull($"{cell} DiagonalUp Border", cell.Format.Borders.DiagonalUp);
			AssertEquals($"{cell} DiagonalUp BorderStyle", BorderStyle.None, cell.Format.Borders.DiagonalUp.Style);

			AssertNotNull($"{cell} DiagonalDown Border", cell.Format.Borders.DiagonalDown);
			AssertEquals($"{cell} DiagonalDown BorderStyle", BorderStyle.Thin, cell.Format.Borders.DiagonalDown.Style);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBorders_TopDownDiagonalUp()
		{
			IWorksheet worksheet = TestFiles.CreateWorksheet(TestFiles.XlsFileForBordersTestPath);

			var cell = worksheet.GetCell(12, 2);

			AssertNotNull($"{cell} Left Border", cell.Format.Borders.Left);
			AssertEquals($"{cell} Left BorderStyle", BorderStyle.None, cell.Format.Borders.Left.Style);

			AssertNotNull($"{cell} Right Border", cell.Format.Borders.Right);
			AssertEquals($"{cell} Right BorderStyle", BorderStyle.None, cell.Format.Borders.Right.Style);

			AssertNotNull($"{cell} Top Border", cell.Format.Borders.Top);
			AssertEquals($"{cell} Top BorderStyle", BorderStyle.Thin, cell.Format.Borders.Top.Style);

			AssertNotNull($"{cell} Bottom Border", cell.Format.Borders.Bottom);
			AssertEquals($"{cell} Bottom BorderStyle", BorderStyle.Thin, cell.Format.Borders.Bottom.Style);

			AssertNotNull($"{cell} DiagonalUp Border", cell.Format.Borders.DiagonalUp);
			AssertEquals($"{cell} DiagonalUp BorderStyle", BorderStyle.Thin, cell.Format.Borders.DiagonalUp.Style);

			AssertNotNull($"{cell} DiagonalDown Border", cell.Format.Borders.DiagonalDown);
			AssertEquals($"{cell} DiagonalDown BorderStyle", BorderStyle.None, cell.Format.Borders.DiagonalDown.Style);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBorders_All()
		{
			IWorksheet worksheet = TestFiles.CreateWorksheet(TestFiles.XlsFileForBordersTestPath);

			var cell = worksheet.GetCell(14, 2);

			AssertNotNull($"{cell} Left Border", cell.Format.Borders.Left);
			AssertEquals($"{cell} Left BorderStyle", BorderStyle.Thin, cell.Format.Borders.Left.Style);

			AssertNotNull($"{cell} Right Border", cell.Format.Borders.Right);
			AssertEquals($"{cell} Right BorderStyle", BorderStyle.Thin, cell.Format.Borders.Right.Style);

			AssertNotNull($"{cell} Top Border", cell.Format.Borders.Top);
			AssertEquals($"{cell} Top BorderStyle", BorderStyle.Thin, cell.Format.Borders.Top.Style);

			AssertNotNull($"{cell} Bottom Border", cell.Format.Borders.Bottom);
			AssertEquals($"{cell} Bottom BorderStyle", BorderStyle.Thin, cell.Format.Borders.Bottom.Style);

			AssertNotNull($"{cell} DiagonalUp Border", cell.Format.Borders.DiagonalUp);
			AssertEquals($"{cell} DiagonalUp BorderStyle", BorderStyle.Thin, cell.Format.Borders.DiagonalUp.Style);

			AssertNotNull($"{cell} DiagonalDown Border", cell.Format.Borders.DiagonalDown);
			AssertEquals($"{cell} DiagonalDown BorderStyle", BorderStyle.Thin, cell.Format.Borders.DiagonalDown.Style);
		}

		#endregion

		#region Implementation

		IEnumerable<string> GetCellValues(IWorksheet worksheet)
		{
			for (int rowNumber = 1; rowNumber <= worksheet.Rows.Count; rowNumber++)
			{
				for (int columnNumber = 1; columnNumber <= worksheet.Columns.Count; columnNumber++)
				{
					var cell = worksheet.GetCell(rowNumber, columnNumber);
					var cellValue = cell.Value;

					yield return cellValue == null
						? string.Format("[{0},{1}] [{2},{3}]", cell.TopRow, cell.LeftColumn, cell.BottomRow, cell.RightColumn)
						: string.Format("[{0},{1}] [{2},{3}] {4}", cell.TopRow, cell.LeftColumn, cell.BottomRow, cell.RightColumn, cellValue);
				}
			}
		}

		#endregion
	}
}
