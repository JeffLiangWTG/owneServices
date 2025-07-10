using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.Excel.Testing;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class ExcelExportColumnTest : ExcelExportColumnBaseTestCase
	{
		protected override ZString ExpectedFormatPattern => "";

		public override void TestGetValueForExport()
		{
			var lineExportData = new NFeInvoiceLineExportObject(Factory.New<JobComInvoiceLine>());
			lineExportData.GoodsDescription = "Test Value";
			AssertEquals("Comment should be as expected", "Test Value", GetNewExcelExportColumn().GetValueForExport(lineExportData));
		}

		protected override ExcelExportColumnBase GetNewExcelExportColumn(IExcelExportCustomFunction baseColumn)
		{
			return new ExcelExportColumn<NFeInvoiceLineExportObject>(NFeInvoiceLineExportObject.Schema.GoodsDescription, "Goods Description", 80);
		}

		public void TestPropertyName()
		{
			AssertEquals("PropertyName", NFeInvoiceLineExportObject.Schema.GoodsDescription, (GetNewExcelExportColumn() as ExcelExportColumn<NFeInvoiceLineExportObject>).PropertyName);
		}

		public override void TestComment()
		{
			var column = GetNewExcelExportColumn();
			AssertEquals("Comment should be empty", "", column.GetComment(Dummy));
		}

		public override void TestColor()
		{
			ExcelExportColumnBase column = GetNewExcelExportColumn();
			AssertNull("Color should be empty", column.GetColor(Dummy));
		}

		public void TestCreateFromColumn()
		{
			var columnInfo = new ZTextBoxColumnStyleInfo();
			columnInfo.ColumnName = NFeInvoiceLineExportObject.Schema.GoodsDescription;
			columnInfo.Width = 80;

			var column = new ZGridColumn();
			using (var columnStyle = new ZTextBoxColumnStyle(columnInfo))
			{
				columnStyle.HeaderText = "Goods Description";
				column.ColumnStyle = columnStyle;

				var exportColumn = ExcelExportColumn<NFeInvoiceLineExportObject>.CreateFromColumn(column);

				CombineAssertions(() =>
				{
					AssertEquals("PropertyName", "GoodsDescription", exportColumn.PropertyName);
					AssertEquals("Description", "Goods Description", exportColumn.Description);
					AssertEquals("Width", 80 * 48, exportColumn.Width);
				});
			}
		}
	}
}
