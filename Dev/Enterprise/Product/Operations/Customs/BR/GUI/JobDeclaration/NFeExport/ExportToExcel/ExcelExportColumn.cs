using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Excel;

namespace Enterprise.Customs.BR.GUI
{
	public class ExcelExportColumn<T> : ExcelExportColumnBase where T : BusinessObject
	{
		public ExcelExportColumn(string propertyName, string description, int width)
		{
			PropertyName = propertyName;
			Description = description;
			Width = width;
		}

		public readonly string PropertyName;

		public override DocumentEngineIntegration.CellFormat GetFormat(IZType value) => new DocumentEngineIntegration.CellFormat();

		protected override string GetDescription() => Description;

		protected override IZType GetValueForExportCore(BusinessObject bizObj) => GetValueForExport(bizObj, PropertyName);

		public static IZType GetValueForExport(BusinessObject bizObj, string propertyName) => (IZType)bizObj[propertyName];

		const int ExcelColumnWidthMultiplyFactor = 48;

		public static ExcelExportColumn<T> CreateFromColumn(ZGridColumn column)
		{
			return new ExcelExportColumn<T>(column.ColumnName, column.ColumnStyle.HeaderText, column.ColumnStyle.Width * ExcelColumnWidthMultiplyFactor);
		}
	}
}
