using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Excel.Testing
{
	sealed class ExcelExportColumnDecimalTest : ExcelExportColumnBaseTestCase
	{
		public override void TestGetValueForExport()
		{
			Dummy.Z0_AnotherDecimal = 0.1m;
			AssertFormattedResult(Dummy, (ZDecimal)0.10m);

			Dummy.Z0_AnotherDecimal = 1m;
			AssertFormattedResult(Dummy, (ZDecimal)1.00m);
		}

		public void TestWithNonZDecimalValue()
		{
			ErrorReporter.Clear();
			AssertEquals("Precondition - ErrorReporter.LastMessageReported should be empty.", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			Dummy.Z0_Description = "non-decimal";
			AssertEquals(new ZDecimal(0), GetNewExcelExportColumn().GetValueForExport(Dummy));
			//AssertEquals("The value 'non-decimal' is not a valid type for formatting by an ExcelExportDecimalColumn.", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestFormatWithNoDecimals()
		{
			var exportColumn = (ExcelExportDecimalColumn)GetNewExcelExportColumn();
			exportColumn.Decimals = 0;

			AssertEquals("#,#", exportColumn.Format.FormatPattern);
		}

		protected override ExcelExportColumnBase GetNewExcelExportColumn()
		{
			return new ExcelExportDecimalColumn(DummyBizoSchema.Z0_AnotherDecimal);
		}

		protected override ZString ExpectedFormatPattern
		{
			get { return "#,##0.00"; }
		}

		protected override ExcelExportColumnBase GetNewExcelExportColumn(IExcelExportCustomFunction baseColumn)
		{
			if ((baseColumn != null) && (baseColumn is IExcelExportCellComment))
			{
				return new ExcelExportDecimalColumn(DummyBizoSchema.Z0_Decimal, 2, (IExcelExportCellComment)baseColumn, (IExcelExportCellColor)baseColumn);
			}
			else
			{
				return new ExcelExportDecimalColumn(DummyBizoSchema.Z0_Decimal);
			}
		}
	}
}
