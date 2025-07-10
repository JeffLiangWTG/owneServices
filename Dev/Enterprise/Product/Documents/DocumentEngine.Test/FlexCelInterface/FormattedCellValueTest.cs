using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.FlexCelInterface.Testing
{
	sealed class FormattedCellValueTest : TestCaseWithFactory
	{
		public void TestToString()
		{
			FormattedCellValue cellValue = new FormattedCellValue(1234, "#,##0");
			AssertEquals("1,234", cellValue.ToString());

			cellValue = new FormattedCellValue(1234, "#,##0.00");
			AssertEquals("1,234.00", cellValue.ToString());

			cellValue = new FormattedCellValue(1234, "#,##0.0000");
			AssertEquals("1,234.0000", cellValue.ToString());

			cellValue = new FormattedCellValue(null, "#,##0");
			AssertEquals(String.Empty, cellValue.ToString());

			cellValue = new FormattedCellValue("=B25", "#,##0");
			AssertEquals("=ROUND(B25,0)", cellValue.ToString());

			cellValue = new FormattedCellValue("=B25+A14", "#,##0.00");
			AssertEquals("=ROUND(B25+A14,2)", cellValue.ToString());

			cellValue = new FormattedCellValue("=B25+A14", null);
			AssertEquals("=B25+A14", cellValue.ToString());
		}
	}
}
