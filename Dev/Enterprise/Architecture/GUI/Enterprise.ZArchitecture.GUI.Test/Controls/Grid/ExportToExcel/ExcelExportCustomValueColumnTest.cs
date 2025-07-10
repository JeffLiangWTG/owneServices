using System.Drawing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Excel.Testing
{
	sealed class ExcelExportCustomValueColumnTest : ExcelExportColumnBaseTestCase
	{
		protected override void TestDefaultDescription()
		{
			AssertEquals("Test Description", DummyCustomValueImplementation.DescriptionForTest, GetNewExcelExportColumn().Description);
		}

		public override void TestComment()
		{
			var columnWitCommentAndValue = GetNewExcelExportColumn(DummyGridColumn);
			var columnWithOutComment = GetNewExcelExportColumn();

			AssertEquals("Comment should be as expected", "Test Comment", columnWitCommentAndValue.GetComment(BizObj));
			AssertEquals("Comment should be as expected", "", columnWithOutComment.GetComment(BizObj));
		}

		public override void TestGetValueForExport()
		{
			var columnWitCommentAndValue = GetNewExcelExportColumn(DummyGridColumn);

			AssertEquals("Comment should be as expected", "Test Value", columnWitCommentAndValue.GetValueForExport(BizObj));
		}

		protected override void TestDefaultFormat()
		{
			AssertNotNull("Should return not null as the CellFormat", ExcelExportColumn.GetFormat(new ZDecimal(6)));

			AssertEquals("Format Pattern should be as #,##0.00 ", ExpectedFormatPattern, ExcelExportColumn.GetFormat(new ZDecimal(6)).FormatPattern);

			AssertEquals("Format Pattern should be as Short Date Format", ZDateTime.ShortDateFormat, ExcelExportColumn.GetFormat(new ZDateTime()).FormatPattern);
			AssertEquals("Format Pattern should be as Short Date Format", ZDateTime.ShortDateFormat, ExcelExportColumn.GetFormat(new ZDateTimeOffset()).FormatPattern);

			AssertEquals("Format Pattern should be as empty string", "", ExcelExportColumn.GetFormat(new ZString()).FormatPattern);

			AssertEquals("Format Pattern should be as empty string", "", ExcelExportColumn.GetFormat(new ZGuid()).FormatPattern);
		}

		public override void TestColor()
		{
			var columnWitCommentAndValue = GetNewExcelExportColumn(DummyGridColumn);
			var columnWithOutColor = GetNewExcelExportColumn();

			AssertEquals("Color should be as expected", Color.Red, columnWitCommentAndValue.GetColor(BizObj));
			AssertEquals("Color should be DefaultCellColor", null, columnWithOutColor.GetColor(BizObj));
		}

		protected override ExcelExportColumnBase GetNewExcelExportColumn()
		{
			return ExcelExportCustomValueColumn.New(DummyGridColumn);
		}

		protected override ZString ExpectedFormatPattern
		{
			get { return "#,##0.00"; }
		}

		protected override ExcelExportColumnBase GetNewExcelExportColumn(IExcelExportCustomFunction baseColumn)
		{
			var value = baseColumn as IExcelExportCustomValue;
			if ((baseColumn != null) && (baseColumn is IExcelExportCellComment))
			{
				return ExcelExportCustomValueColumn.New(value, (IExcelExportCellComment)baseColumn);
			}
			else
			{
				return ExcelExportCustomValueColumn.New(value);
			}
		}

		DummyBusinessObject BizObj
		{
			get
			{
				if (fBizObj == null)
				{
					fBizObj = Factory.New<DummyBusinessObject>();
				}
				return fBizObj;
			}
		}

		DummyBusinessObject fBizObj;

		DummyCustomValueImplementation DummyGridColumn
		{
			get
			{
				if (fDummyGridColumn == null)
				{
					fDummyGridColumn = new DummyCustomValueImplementation();
				}
				return fDummyGridColumn;
			}
		}

		DummyCustomValueImplementation fDummyGridColumn;
	}
}
