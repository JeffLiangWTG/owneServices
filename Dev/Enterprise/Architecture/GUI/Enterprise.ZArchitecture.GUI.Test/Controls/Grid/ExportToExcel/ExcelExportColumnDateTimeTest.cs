using System;
using System.Data;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Excel.Testing
{
	sealed class ExcelExportColumnDateTimeTest : ExcelExportColumnBaseTestCase
	{
		public override void TestGetValueForExport()
		{
			Dummy.Z0_Date = ZDateTime.Invalid;
			AssertFormattedResult(Dummy, ZString.Empty);

			Dummy.Z0_Date = ZDateTime.Empty;
			AssertFormattedResult(Dummy, ZString.Empty);

			var longFormatExportColumn = new ExcelExportDateTimeColumn(DummyBizoSchema.Z0_Date, ZDateTimePickerFormat.Long);
			var customFormatExportColumn = new ExcelExportDateTimeColumn(DummyBizoSchema.Z0_Date, ZDateTimePickerFormat.Custom);
			var shortFormatExportColumn = new ExcelExportDateTimeColumn(DummyBizoSchema.Z0_Date, ZDateTimePickerFormat.Short);
			var timeFormatExportColumn = new ExcelExportDateTimeColumn(DummyBizoSchema.Z0_Date, ZDateTimePickerFormat.Time);
			var upTo999FormatExportColumn = new ExcelExportDateTimeColumn(DummyBizoSchema.Z0_Date, ZDateTimePickerFormat.TimeUpTo999HoursAnd45Minutes);

			Dummy.Z0_Date = new ZDateTime(2005, 1, 14, 9, 30, 7);

			AssertFormattedResult(Dummy, new ZDateTime(2005, 1, 14, 9, 30, 7), longFormatExportColumn);
			AssertFormattedResult(Dummy, new ZDateTime(2005, 1, 14, 9, 30, 7), customFormatExportColumn);
			AssertFormattedResult(Dummy, new ZDateTime(2005, 1, 14, 9, 30, 7), shortFormatExportColumn);
			AssertFormattedResult(Dummy, new ZDateTime(2005, 1, 14, 9, 30, 7), timeFormatExportColumn);
			AssertFormattedResult(Dummy, (ZString)"321:30", upTo999FormatExportColumn);
		}

		public void TestWithNullZDateTimeValue()
		{
			ErrorReporter.Clear();
			AssertEquals("Precondition - ErrorReporter.LastMessageReported should be empty.", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			var invalidColumn = new SchemaDateTimeColumn(CargoWise.Schema.Schema.GenericTableSchema, "Z0_Date+Invalid", 1, SqlDbType.DateTime, DBNull.Value, true);
			var result = new ExcelExportDateTimeColumn(invalidColumn).GetValueForExport(Dummy);

			AssertEquals(ZString.Empty, result);
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		}

		public void TestWithNonZDateTimeValue()
		{
			ErrorReporter.Clear();
			AssertEquals("Precondition - ErrorReporter.LastMessageReported should be empty.", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			Dummy.Z0_Description = "non-date";
			AssertEquals("", GetNewExcelExportColumn().GetValueForExport(Dummy));
			//AssertEquals("The value 'non-date' is not a valid type for formatting by an ExcelExportDateTimeColumn.", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestFormatWithNonLongTimeFormat()
		{
			var exportColumn = (ExcelExportDateTimeColumn)GetNewExcelExportColumn();

			exportColumn.DateTimeFormat = ZDateTimePickerFormat.Short;
			AssertEquals(ZDateTime.ShortDateFormat, exportColumn.Format.FormatPattern);

			exportColumn.DateTimeFormat = ZDateTimePickerFormat.Time;
			AssertEquals(ZDateTime.ShortTimeFormat, exportColumn.Format.FormatPattern);

			exportColumn.DateTimeFormat = ZDateTimePickerFormat.TimeUpTo999HoursAnd45Minutes;
			AssertEquals("", exportColumn.Format.FormatPattern);
		}

		protected override ExcelExportColumnBase GetNewExcelExportColumn(IExcelExportCustomFunction baseColumn)
		{
			if ((baseColumn != null) && (baseColumn is IExcelExportCellComment))
			{
				return new ExcelExportDateTimeColumn(DummyBizoSchema.Z0_Date, baseColumn as IExcelExportCellComment, baseColumn as IExcelExportCellColor);
			}
			else
			{
				return new ExcelExportDateTimeColumn(DummyBizoSchema.Z0_Date);
			}
		}

		protected override ZString ExpectedFormatPattern
		{
			get { return ZDateTime.LongTimeFormat; }
		}
	}
}
