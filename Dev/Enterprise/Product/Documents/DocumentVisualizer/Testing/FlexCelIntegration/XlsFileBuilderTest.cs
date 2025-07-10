using System.Collections.Generic;
using System.Drawing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Testing;
using FlexCel.Core;
using FlexCel.XlsAdapter;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration.Testing
{
	sealed class XlsFileBuilderTest : TestCaseWithFactory
	{
		#region TestBuild

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBuild()
		{
			IWorksheet worksheet = TestFiles.CreateWorksheet();

			var builder = new XlsFileBuilder(worksheet);
			var xls = builder.Build();

			#region Worksheet

			AssertEquals("Sheet1", xls.SheetName);

			CombineAssertions(() =>
				{
					AssertEquals(3214, xls.GetColWidth(1));
					AssertEquals(3271, xls.GetColWidth(2));
					AssertEquals(2340, xls.GetColWidth(3));
					AssertEquals(2340, xls.GetColWidth(4));
					AssertEquals(2340, xls.GetColWidth(5));
					AssertEquals(2340, xls.GetColWidth(6));
					AssertEquals(2340, xls.GetColWidth(7));

					AssertEquals(299, xls.GetRowHeight(1));
					AssertEquals(1163, xls.GetRowHeight(2));
					AssertEquals(287, xls.GetRowHeight(3));
					AssertEquals(287, xls.GetRowHeight(4));
					AssertEquals(287, xls.GetRowHeight(5));
					AssertEquals(287, xls.GetRowHeight(6));
					AssertEquals(287, xls.GetRowHeight(7));
				});

			#endregion

			#region Cells

			const string cellA2 = "Value:text:/Font:Calibri/Color:255 0 0 0/Style:None/" +
				"HAlign:left/VAlign:bottom/Rotation:0/LBorder:None/TBorder:None/RBorder:None/BBorder:None/WrapText:False";
			AssertEquals("Cell A2", cellA2, GetCellDetailsStringForTest(xls, 2, 1));

			const string cellB2 = "Value:cell/Font:Tahoma/Color:255 0 0 0/Style:Bold/" +
				"HAlign:left/VAlign:bottom/Rotation:0/LBorder:None/TBorder:None/RBorder:None/BBorder:None/WrapText:False";
			AssertEquals("Cell B2", cellB2, GetCellDetailsStringForTest(xls, 2, 2));

			const string cellC2D4 = "Value:merged cell that needs to wrap nicely/Font:Calibri/Color:255 91 155 213/Style:None/" +
				"HAlign:center/VAlign:center/Rotation:0/LBorder:Thin/TBorder:Medium/RBorder:Thick/BBorder:Thin/WrapText:True";
			AssertEquals("Cell C2-D4 (merged)", cellC2D4, GetCellDetailsStringForTest(xls, 2, 3));

			const string cellA4 = "Value:formula:/Font:Calibri/Color:255 192 0 0/Style:Italic/" +
				"HAlign:left/VAlign:bottom/Rotation:0/LBorder:None/TBorder:None/RBorder:None/BBorder:None/WrapText:False";
			AssertEquals("Cell A4", cellA4, GetCellDetailsStringForTest(xls, 4, 1));

			const string cellB4 = "Value:evaluated if true/Font:Calibri/Color:255 0 0 0/Style:None/" +
				"HAlign:left/VAlign:bottom/Rotation:0/LBorder:None/TBorder:None/RBorder:None/BBorder:None/WrapText:False";
			AssertEquals("Cell B4", cellB4, GetCellDetailsStringForTest(xls, 4, 2));

			const string cellF2 = "Value:90 degrees/Font:Calibri/Color:255 0 0 0/Style:None/" +
				"HAlign:left/VAlign:bottom/Rotation:90/LBorder:None/TBorder:None/RBorder:None/BBorder:None/WrapText:False";
			AssertEquals("Cell F2", cellF2, GetCellDetailsStringForTest(xls, 2, 6));

			const string cellG2 = "Value:180 degrees/Font:Calibri/Color:255 0 0 0/Style:None/" +
				"HAlign:left/VAlign:bottom/Rotation:180/LBorder:None/TBorder:None/RBorder:None/BBorder:None/WrapText:False";
			AssertEquals("Cell G2", cellG2, GetCellDetailsStringForTest(xls, 2, 7));

			#endregion

			#region Images

			AssertEquals("Contains 1 image", 1, xls.ImageCount);

			var imageType = TXlsImgType.Unknown;
			var imageData = xls.GetImage(1, ref imageType);

			AssertEquals("Image type", TXlsImgType.Png, imageType);
			AssertNotNull("Image data", imageData);

			var imageProperties = xls.GetImageProperties(1);

			AssertEquals("Image TopRow", 9, imageProperties.Anchor.Row1);
			AssertEquals("Image LeftColumn", 1, imageProperties.Anchor.Col1);
			AssertEquals("Image BottomRow", 20, imageProperties.Anchor.Row2);
			AssertEquals("Image RightColumn", 7, imageProperties.Anchor.Col2);

			#endregion
		}

		#endregion

		#region TestExcludeEmptyStyles

		public void TestExcludeEmptyStyles()
		{
			var worksheet = new DummyWorksheet();
			worksheet.Rows.Add(33d);
			worksheet.Columns.Add(33d);
			worksheet.Columns.Add(33d);

			AssertEquals("expected rows", 1, worksheet.Rows.Count);
			AssertEquals("expected columns", 2, worksheet.Columns.Count);

			var cell1 = new DummyCell();
			cell1.TopRow = 1;
			cell1.BottomRow = 1;
			cell1.LeftColumn = 1;
			cell1.RightColumn = 1;
			cell1.Value = "value 1";
			cell1.Format = Format.Empty;

			worksheet.AddCell(cell1);

			var cell2 = new DummyCell();
			cell2.TopRow = 1;
			cell2.BottomRow = 1;
			cell2.LeftColumn = 2;
			cell2.RightColumn = 2;
			cell2.Value = "value 2";
			cell2.Format = new Format
			{
				BackgroundColor = Color.LightSalmon
			};

			worksheet.AddCell(cell2);

			var builder = new XlsFileBuilder(worksheet);
			var xls = builder.Build();

			AssertEquals("expected 2 formats; 1) default one 2) for cell2. Style for cell1 should be ignored",
				2, xls.FormatCount);
		}

		#endregion

		#region TestPaintArea

		public void TestPaintArea()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body
	<Z0_Description>
#End");

			var cell = (DummyCell)worksheet.GetCell(4, 2);
			cell.Width = 174;
			cell.Height = 40.5;
			cell.Padding = new RectangleF(0.72f, 0.72f, 0.72f, 0);
			cell.Format = new Format
			{
				Font = new Core.Font("Arial", 8f, FontStyle.Regular, Color.Empty, 0),
				VAlignment = Alignment.Top,
				HAlignment = Alignment.Left,
				WrapText = true
			};

			IStandardTemplate template = new StandardTemplate(worksheet);
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "Contact: Operations\r\nTel: +61288351212 Fax: +61288351223\r\nEmail: operations.ausyd@client.com";

			var documentBuilder = new StandardDocumentBuilder(
				template.Name,
				"test",
				template,
				new MacroScope(dummy.MakeDynamic()),
				new IMacroLibrary[]
				{
					new StandardLibrary(),
					new DocumentLibrary()
				}.CreateContext(),
				null,
				DefaultLanguageProvider.Instance);

			var document = documentBuilder.Build();
			var builder = new XlsFileBuilder(document);
			var xls = builder.Build();

			var format = xls.GetFormat(xls.GetCellFormat(1, 2));
			AssertEquals(130, format.Font.Size20);
		}

		#endregion

		#region TestPaintAreaShrinkToFitForSingleLineText

		public void TestPaintAreaShrinkToFitForSingleLineText()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body
	<Z0_Description>
#End");

			var cell = (DummyCell)worksheet.GetCell(4, 2);
			cell.Width = 60;
			cell.Height = 38;
			cell.Padding = new RectangleF(0.5f, 0.5f, 0.5f, 0);
			cell.Format = new Format
			{
				Font = new Core.Font("Arial", 8f, FontStyle.Regular, Color.Empty, 0),
				VAlignment = Alignment.Top,
				HAlignment = Alignment.Left,
				WrapText = false
			};

			IStandardTemplate template = new StandardTemplate(worksheet);
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "A short description for cell.";

			var documentBuilder = new StandardDocumentBuilder(
				template.Name,
				"test",
				template,
				new MacroScope(dummy.MakeDynamic()),
				System.Array.Empty<IMacroLibrary>().CreateContext(),
				null,
				DefaultLanguageProvider.Instance);

			var document = documentBuilder.Build();
			var builder = new XlsFileBuilder(document);
			var xls = builder.Build();

			var format = xls.GetFormat(xls.GetCellFormat(1, 2));
			AssertEquals(120, format.Font.Size20);
		}

		#endregion

		#region TestReplaceNewLine

		public void TestReplaceNewLine()
		{
			var worksheet = new DummyWorksheet();
			worksheet.Columns.Add(new Column(1, 100));

			worksheet.Rows.Add(new Row(1, 100));
			worksheet.Rows.Add(new Row(2, 100));
			worksheet.Rows.Add(new Row(3, 100));
			worksheet.Rows.Add(new Row(4, 100));

			worksheet.AddCell(new DummyCell
			{
				TopRow = 1,
				BottomRow = 1,
				LeftColumn = 1,
				RightColumn = 1,
				Value = "aaa\r\nbbb"
			});

			worksheet.AddCell(new DummyCell
			{
				TopRow = 2,
				BottomRow = 2,
				LeftColumn = 1,
				RightColumn = 1,
				Value = "ccc\nddd"
			});

			worksheet.AddCell(new DummyCell
			{
				TopRow = 3,
				BottomRow = 3,
				LeftColumn = 1,
				RightColumn = 1,
				Value = "eee"
			});

			worksheet.AddCell(new DummyCell
			{
				TopRow = 4,
				BottomRow = 4,
				LeftColumn = 1,
				RightColumn = 1,
				Value = null
			});

			var builder = new XlsFileBuilder(worksheet);
			var xls = builder.Build();

			var multilineCellValue1 = xls.GetCellValue(1, 1);
			AssertEquals("multiline with \\r\\n has been replaced with \\n", "aaa\nbbb", multilineCellValue1);

			var multilineCellValue2 = xls.GetCellValue(2, 1);
			AssertEquals("multiline with \\n has not been replaced", "ccc\nddd", multilineCellValue2);

			var singleLineCellValue = xls.GetCellValue(3, 1);
			AssertEquals("single cell value", "eee", singleLineCellValue);

			var emptyCellValue = xls.GetCellValue(4, 1);
			AssertEquals("empty cell value", null, emptyCellValue);
		}

		#endregion

		#region Implementation

		static string GetCellDetailsStringForTest(XlsFile xls, int row, int col)
		{
			var cellInfo = new List<string>();
			cellInfo.Add("Value:" + (string)xls.GetCellValue(row, col));

			var format = xls.GetFormat(xls.GetCellFormat(row, col));
			cellInfo.Add("Font:" + format.Font.Name);
			var fontColor = format.Font.Color.ToColor(xls);
			cellInfo.Add(string.Format("Color:{0} {1} {2} {3}", fontColor.A, fontColor.R, fontColor.G, fontColor.B));

			cellInfo.Add("Style:" + format.Font.Style.ToString());
			cellInfo.Add("HAlign:" + format.HAlignment.ToString());
			cellInfo.Add("VAlign:" + format.VAlignment.ToString());
			cellInfo.Add("Rotation:" + format.Rotation.ToString());

			cellInfo.Add("LBorder:" + format.Borders.Left.Style.ToString());
			cellInfo.Add("TBorder:" + format.Borders.Top.Style.ToString());
			cellInfo.Add("RBorder:" + format.Borders.Right.Style.ToString());
			cellInfo.Add("BBorder:" + format.Borders.Bottom.Style.ToString());

			cellInfo.Add("WrapText:" + format.WrapText.ToString());

			return string.Join("/", cellInfo);
		}

		#endregion
	}
}
