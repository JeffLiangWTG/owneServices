using System;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Excel.Testing
{
	sealed class ExcelExportColumnGeographyTest : ExcelExportColumnBaseTestCase
	{
		public override void TestGetValueForExport()
		{
			Dummy.Z0_Geography = ZGeography.Invalid;
			AssertFormattedResult(Dummy, ZString.Empty);

			Dummy.Z0_Geography = ZGeography.Empty;
			AssertFormattedResult(Dummy, ZString.Empty);

			var exportColumn = new ExcelExportGeographyColumn(DummyBizoSchema.Z0_Geography);

			Dummy.Z0_Geography = new ZGeography("POINT (-121 48)");

			AssertFormattedResult(Dummy, (ZString)new ZGeography("POINT (-121 48)").AsText(), exportColumn);
		}

		public void TestWithNullZGeographyValue()
		{
			ErrorReporter.Clear();
			AssertEquals("Precondition - ErrorReporter.LastMessageReported should be empty.", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			var invalidColumn = new SchemaGeographyColumn(CargoWise.Schema.Schema.GenericTableSchema, "Z0_Geography+Invalid", 1, DBNull.Value, true);
			var result = new ExcelExportGeographyColumn(invalidColumn).GetValueForExport(Dummy);

			AssertEquals(ZString.Empty, result);
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		}

		public void TestWithNonZGeographyValue()
		{
			ErrorReporter.Clear();
			AssertEquals("Precondition - ErrorReporter.LastMessageReported should be empty.", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			Dummy.Z0_Description = "non-date";
			AssertEquals("", GetNewExcelExportColumn().GetValueForExport(Dummy));

			ErrorReporter.Clear();
		}

		protected override ExcelExportColumnBase GetNewExcelExportColumn(IExcelExportCustomFunction baseColumn)
		{
			if ((baseColumn != null) && (baseColumn is IExcelExportCellComment))
			{
				return new ExcelExportGeographyColumn(DummyBizoSchema.Z0_Geography, baseColumn as IExcelExportCellComment, baseColumn as IExcelExportCellColor);
			}
			else
			{
				return new ExcelExportGeographyColumn(DummyBizoSchema.Z0_Geography);
			}
		}

		protected override ZString ExpectedFormatPattern
		{
			get { return ""; }
		}
	}
}
