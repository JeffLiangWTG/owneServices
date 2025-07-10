using System;
using System.Data;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Excel.Testing
{
	sealed class ExcelExportColumnDateTimeOffsetTest : ExcelExportColumnBaseTestCase
	{
		public override void TestGetValueForExport()
		{
			Dummy.Z0_DateTimeOffset = ZDateTimeOffset.Invalid;
			AssertFormattedResult(Dummy, ZString.Empty);

			Dummy.Z0_DateTimeOffset = ZDateTimeOffset.Empty;
			AssertFormattedResult(Dummy, ZString.Empty);

			var exportColumn = new ExcelExportDateTimeOffsetColumn(DummyBizoSchema.Z0_DateTimeOffset);

			Dummy.Z0_DateTimeOffset = new ZDateTimeOffset(2005, 1, 14, 9, 30, 7, TimeSpan.FromHours(11));

			AssertFormattedResult(Dummy, new ZDateTime(2005, 1, 14, 9, 30, 7), exportColumn);
		}

		public void TestWithNullZDateTimeOffsetValue()
		{
			ErrorReporter.Clear();
			AssertEquals("Precondition - ErrorReporter.LastMessageReported should be empty.", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			var invalidColumn = new SchemaDateTimeOffsetColumn(CargoWise.Schema.Schema.GenericTableSchema, "Z0_DateTimeOffset+Invalid", 1, SqlDbType.DateTimeOffset, DBNull.Value, true, 7);
			var result = new ExcelExportDateTimeOffsetColumn(invalidColumn).GetValueForExport(Dummy);

			AssertEquals(ZString.Empty, result);
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		}

		public void TestWithNonZDateTimeOffsetValue()
		{
			ErrorReporter.Clear();
			AssertEquals("Precondition - ErrorReporter.LastMessageReported should be empty.", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			Dummy.Z0_Description = "non-date";
			AssertEquals("", GetNewExcelExportColumn().GetValueForExport(Dummy));
			//AssertEquals("The value 'non-date' is not a valid type for formatting by an ExcelExportDateTimeColumn.", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		protected override ExcelExportColumnBase GetNewExcelExportColumn(IExcelExportCustomFunction baseColumn)
		{
			if ((baseColumn != null) && (baseColumn is IExcelExportCellComment))
			{
				return new ExcelExportDateTimeOffsetColumn(DummyBizoSchema.Z0_DateTimeOffset, baseColumn as IExcelExportCellComment, baseColumn as IExcelExportCellColor);
			}
			else
			{
				return new ExcelExportDateTimeOffsetColumn(DummyBizoSchema.Z0_DateTimeOffset);
			}
		}

		protected override ZString ExpectedFormatPattern
		{
			get { return ZDateTime.LongTimeFormat; }
		}
	}
}
