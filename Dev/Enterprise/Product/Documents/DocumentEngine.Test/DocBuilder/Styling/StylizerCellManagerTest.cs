using System;
using System.Drawing;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineIntegration;
using FlexCel.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocBuilder.Styling.Testing
{
	sealed class StylizerCellManagerTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBackgroundColor()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("StylizerCellManagerTest.xls", TestFilesSubFolder.DocumentTestFiles);

			using (var excelInterface = new ExcelInterface(excelTemplate.GetAsByteArray()))
			{
				var workSheet = excelInterface.WorkSheets[0];
				var manager = new StylizerCellManager(workSheet, 0, 0);

				AssertEquals("Pre-condition: BackgroundColor", Color.Red.ToArgb(), manager.BackgroundColor.ToArgb());

				manager.BackgroundColor = Color.Blue;
				manager.Apply();

				var cellFormat = workSheet.GetCellFormat(0, 0);
				AssertEquals("BackgroundColor", Color.Blue.ToArgb(), cellFormat.BackgroundColor.ToArgb());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBackgroundPattern()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("StylizerCellManagerTest.xls", TestFilesSubFolder.DocumentTestFiles);

			using (var excelInterface = new ExcelInterface(excelTemplate.GetAsByteArray()))
			{
				var workSheet = excelInterface.WorkSheets[0];
				var manager = new StylizerCellManager(workSheet, 0, 0);

				AssertEquals("Pre-condition: BackgroundPattern", FillPatternStyle.Solid, manager.BackgroundPattern);

				manager.BackgroundPattern = FillPatternStyle.None;
				manager.Apply();

				var cellFormat = workSheet.GetCellFormat(0, 0);
				AssertEquals("FillPattern", FillPatternStyle.None, cellFormat.FillPattern);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIsBackgroundColorAutomatic()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("StylizerCellManagerTest.xls", TestFilesSubFolder.DocumentTestFiles);

			using (var excelInterface = new ExcelInterface(excelTemplate.GetAsByteArray()))
			{
				var workSheet = excelInterface.WorkSheets[0];
				var manager = new StylizerCellManager(workSheet, 0, 0);
				AssertEquals("IsBackgroundColorAutomatic", false, manager.IsBackgroundColorAutomatic);

				manager = new StylizerCellManager(workSheet, 1, 0);
				AssertEquals("IsBackgroundColorAutomatic", true, manager.IsBackgroundColorAutomatic);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBorders()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("StylizerCellManagerTest.xls", TestFilesSubFolder.DocumentTestFiles);

			using (var excelInterface = new ExcelInterface(excelTemplate.GetAsByteArray()))
			{
				var workSheet = excelInterface.WorkSheets[0];
				var manager = new StylizerCellManager(workSheet, 0, 0);

				AssertEquals("Borders.Top.Style", TFlxBorderStyle.Thin, manager.Borders.Top.Style);
				AssertEquals("Borders.Bottom.Style", TFlxBorderStyle.Thin, manager.Borders.Bottom.Style);
				AssertEquals("Borders.Left.Style", TFlxBorderStyle.Thin, manager.Borders.Left.Style);
				AssertEquals("Borders.Right.Style", TFlxBorderStyle.Thin, manager.Borders.Right.Style);
				unchecked
				{
					AssertEquals("Borders.Top.Color", 0xff00ff00, (UInt32)manager.Borders.Top.Color.ToColor(excelInterface.Xls).ToArgb());
					AssertEquals("Borders.Bottom.Color", 0xff00ff00, (UInt32)manager.Borders.Bottom.Color.ToColor(excelInterface.Xls).ToArgb());
					AssertEquals("Borders.Left.Color", 0xff00ff00, (UInt32)manager.Borders.Left.Color.ToColor(excelInterface.Xls).ToArgb());
					AssertEquals("Borders.Right.Color", 0xff00ff00, (UInt32)manager.Borders.Right.Color.ToColor(excelInterface.Xls).ToArgb());
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFontName()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("StylizerCellManagerTest.xls", TestFilesSubFolder.DocumentTestFiles);

			using (var excelInterface = new ExcelInterface(excelTemplate.GetAsByteArray()))
			{
				var workSheet = excelInterface.WorkSheets[0];
				var manager = new StylizerCellManager(workSheet, 0, 0);

				AssertEquals("FontName", "Times New Roman", manager.FontName);

				manager.FontName = "Courier New";
				manager.Apply();

				var cellFormat = workSheet.GetCellFormat(0, 0);
				AssertEquals("FontName", "Courier New", cellFormat.FontName);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFontSize()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("StylizerCellManagerTest.xls", TestFilesSubFolder.DocumentTestFiles);

			using (var excelInterface = new ExcelInterface(excelTemplate.GetAsByteArray()))
			{
				var workSheet = excelInterface.WorkSheets[0];
				var manager = new StylizerCellManager(workSheet, 0, 0);

				AssertEquals("FontSize", 20f, manager.FontSize);

				manager.FontSize = 18f;
				manager.Apply();

				var cellFormat = workSheet.GetCellFormat(0, 0);
				AssertEquals("FontSize", 18f, cellFormat.FontSize);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFontStyle()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("StylizerCellManagerTest.xls", TestFilesSubFolder.DocumentTestFiles);

			using (var excelInterface = new ExcelInterface(excelTemplate.GetAsByteArray()))
			{
				var workSheet = excelInterface.WorkSheets[0];
				var manager = new StylizerCellManager(workSheet, 0, 0);

				AssertEquals("FontStyle", FontStyle.Bold | FontStyle.Italic, manager.FontStyle);

				manager.FontStyle = FontStyle.Italic;
				manager.Apply();

				var cellFormat = workSheet.GetCellFormat(0, 0);
				AssertEquals("FontStyle", FontStyle.Italic, cellFormat.FontStyle);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFontColor()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("StylizerCellManagerTest.xls", TestFilesSubFolder.DocumentTestFiles);

			using (var excelInterface = new ExcelInterface(excelTemplate.GetAsByteArray()))
			{
				var workSheet = excelInterface.WorkSheets[0];
				var manager = new StylizerCellManager(workSheet, 0, 0);

				AssertEquals("FontColor", Color.Yellow.ToArgb(), manager.FontColor.ToArgb());

				manager.FontColor = Color.Blue;
				manager.Apply();

				var cellFormat = workSheet.GetCellFormat(0, 0);
				AssertEquals("TextColor", Color.Blue.ToArgb(), cellFormat.TextColor.ToArgb());
			}
		}
	}
}
