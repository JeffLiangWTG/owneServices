using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Testing;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.FlexCelInterface.Testing
{
	sealed class ExcelCellTest : TestCaseWithFactory
	{
		public void TestFormulaCellToText()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);

				var workSheet = excelInterface.WorkSheets[0];
				var cell = workSheet.GetCell(0, 0);

				cell.Value = new TFormula("=ABS(-1)");
				workSheet.ReCalc();

				AssertEquals("Pre-condition: cell.IsFormula", true, cell.IsFormula);
				AssertEquals("Pre-condition: cell.Value", "1", cell.Value.ToString());
				AssertEquals("Pre-condition: cell.Text", "=ABS(-1)", cell.ValueSourceText);

				cell.SetValueDetectingFormulaeFromLeadingEqualsSign("Hello World");
				workSheet.ReCalc();
				AssertEquals("cell.IsFormula", false, cell.IsFormula);
				AssertEquals("cell.Value", "Hello World", cell.Value.ToString());
				AssertEquals("cell.Text", "Hello World", cell.ValueSourceText);

				cell.SetValueDetectingFormulaeFromLeadingEqualsSign("=ABS(-1)");
				workSheet.ReCalc();
				AssertEquals("cell.IsFormula", true, cell.IsFormula);
				AssertEquals("cell.Value", "1", cell.Value.ToString());
				AssertEquals("cell.Text", "=ABS(-1)", cell.ValueSourceText);
			}
		}

		public void TestStringCell()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);

				var workSheet = excelInterface.WorkSheets[0];
				var cell = workSheet.GetCell(0, 0);
				AssertEquals("cell.Row", 0, cell.Row);
				AssertEquals("cell.Column", 0, cell.Column);
				AssertEquals("cell.Reference", "A1", cell.Reference);

				AssertEquals("cell.IsEmpty", true, cell.IsEmpty);
				AssertEquals("cell.IsFormula", false, cell.IsFormula);

				cell.Value = "Hello World";

				AssertEquals("cell.IsEmpty", false, cell.IsEmpty);
				AssertEquals("cell.IsFormula", false, cell.IsFormula);
				AssertEquals("cell.Value", "Hello World", cell.Value.ToString());
				AssertEquals("cell.Text", "Hello World", cell.ValueSourceText);
			}
		}

		public void TestIntegerCell()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);

				var workSheet = excelInterface.WorkSheets[0];
				var cell = workSheet.GetCell(0, 0);
				AssertEquals("cell.Row", 0, cell.Row);
				AssertEquals("cell.Column", 0, cell.Column);
				AssertEquals("cell.Reference", "A1", cell.Reference);

				AssertEquals("cell.IsEmpty", true, cell.IsEmpty);
				AssertEquals("cell.IsFormula", false, cell.IsFormula);

				cell.Value = 88;

				AssertEquals("cell.IsEmpty", false, cell.IsEmpty);
				AssertEquals("cell.IsFormula", false, cell.IsFormula);
				AssertEquals("cell.Value", "88", cell.Value.ToString());
				AssertEquals("cell.Text", "88", cell.ValueSourceText);
			}
		}

		public void TestFormulaCell()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);

				var workSheet = excelInterface.WorkSheets[0];
				var cell = workSheet.GetCell(0, 0);
				AssertEquals("cell.Row", 0, cell.Row);
				AssertEquals("cell.Column", 0, cell.Column);
				AssertEquals("cell.Reference", "A1", cell.Reference);

				AssertEquals("cell.IsEmpty", true, cell.IsEmpty);
				AssertEquals("cell.IsFormula", false, cell.IsFormula);

				cell.Value = new TFormula("=ABS(-1)");
				workSheet.ReCalc();

				AssertEquals("cell.IsEmpty", false, cell.IsEmpty);
				AssertEquals("cell.IsFormula", true, cell.IsFormula);
				AssertEquals("cell.Value", "1", cell.Value.ToString());
				AssertEquals("cell.Text", "=ABS(-1)", cell.ValueSourceText);
			}
		}

		public void TestSetValueDetectingFormulaeFromLeadingEqualsSign_DoesntChangeCellTypeValueHasntChanged()
		{
			var workSheet = DocumentEngineTestHelper.CreateExcelWorkSheetFromString("{A}-[placeholder]");
			var excelFile = workSheet.ParentExcelInterface.Xls;
			var richString = new TRichString();
			richString.SetFromHtml("<b>Kelvin</b> beats 孫 悟空", excelFile.GetDefaultFormat, excelFile);
			workSheet[0, 0] = richString;

			var cell = workSheet.GetCell(0, 0);
			AssertEquals("Precondition: cell content", "Kelvin beats 孫 悟空", cell.ValueSourceText);
			Assert("Precondition: cell should be a RichString", cell.Value is TRichString);

			cell.SetValueDetectingFormulaeFromLeadingEqualsSign(cell.ValueSourceText);

			AssertEquals("Cell content should be the same", "Kelvin beats 孫 悟空", cell.ValueSourceText);
			Assert("RichString should survive as its value hasn't changed. But it didn't. Ouch!!", cell.Value is TRichString);
		}
	}
}
