using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Excel.Testing
{
	sealed class ExcelExportColumnDateTest : ExcelExportColumnBaseTestCase
	{
		public override void TestGetValueForExport()
		{
			Dummy.Z0_DateOnly = ZDate.Invalid;
			AssertFormattedResult(Dummy, ZString.Empty);

			Dummy.Z0_DateOnly = ZDate.Empty;
			AssertFormattedResult(Dummy, ZString.Empty);

			Dummy.Z0_DateOnly = new ZDate(2005, 1, 14);
			AssertFormattedResult(Dummy, new ZDateTime(2005, 1, 14));
		}

		public void TestWithNullZDateValue()
		{
			ErrorReporter.Clear();
			AssertEquals("Precondition - ErrorReporter.LastMessageReported should be empty.", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			var invalidColumn = new SchemaDateColumn(CargoWise.Schema.Schema.GenericTableSchema, "Z0_Date+Invalid", 1, SqlDbType.Date, DBNull.Value, true);
			var result = new ExcelExportDateColumn(invalidColumn, null, null).GetValueForExport(Dummy);

			AssertEquals(ZString.Empty, result);
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		}

		public void TestWithNonZDateTimeValue()
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
				return new ExcelExportDateColumn(DummyBizoSchema.Z0_DateOnly, baseColumn as IExcelExportCellComment, baseColumn as IExcelExportCellColor);
			}
			else
			{
				return new ExcelExportDateColumn(DummyBizoSchema.Z0_DateOnly, null, null);
			}
		}

		protected override ZString ExpectedFormatPattern
		{
			get { return ZDateTime.ShortDateFormat; }
		}

		protected override DummyBusinessObject GetBizObjForTest()
		{
			return Factory.New<DummyBusinessObject>();
		}
	}
}
