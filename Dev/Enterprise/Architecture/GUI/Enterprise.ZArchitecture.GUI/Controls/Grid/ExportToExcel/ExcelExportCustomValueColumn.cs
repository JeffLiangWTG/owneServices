using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.ZArchitecture.Excel
{
	#region ExcelExportCustomValueColumn

	public class ExcelExportCustomValueColumn : ExcelExportColumnBase
	{
		#region Static Constructors

		public static ExcelExportCustomValueColumn New(IExcelExportCustomValue customValue)
		{
			return ExcelExportCustomValueColumn.New(customValue, null);
		}

		public static ExcelExportCustomValueColumn New(IExcelExportCustomValue customValue, IExcelExportCustomFunction gridBaseColumn)
		{
			if (customValue == null)
			{
				throw new ArgumentException("Your Grid Column must support IExcelExportCustomValue");
			}

			var supportComment = gridBaseColumn as IExcelExportCellComment;
			var supportColor = gridBaseColumn as IExcelExportCellColor;

			var result = new ExcelExportCustomValueColumn(customValue, supportComment, supportColor);
			return result;
		}

		#endregion

		#region Protected Constructors

		protected ExcelExportCustomValueColumn(IExcelExportCustomValue customValue, IExcelExportCellComment commentSupport, IExcelExportCellColor colorSupport)
			: base(commentSupport, colorSupport)
		{
			CustomValueSupport = customValue;
		}

		#endregion

		protected readonly IExcelExportCustomValue CustomValueSupport;

		protected override IZType GetValueForExportCore(BusinessObject bizObj)
		{
			IZType result;

			if (CustomValueSupport != null)
			{
				result = CustomValueSupport.GetCustomValue(bizObj) ?? ZString.Empty;
			}
			else
			{
				result = ZString.Empty;
			}

			return result;
		}

		public override CellFormat GetFormat(IZType value)
		{
			ZString pattern = ZString.Empty;
			if (CustomValueSupport != null)
			{
				pattern = CustomValueSupport.GetValueFormat(value);
			}

			if (pattern.IsEmpty && value != null)
			{
				if (value.GetType() == typeof(ZDecimal))
				{
					pattern = "#,##0.00";
				}
				else if (value.GetType() == typeof(ZDateTime) || value.GetType() == typeof(ZDateTimeOffset) || value.GetType() == typeof(ZDate))
				{
					pattern = ZDateTime.ShortDateFormat;
				}
				else
				{
					pattern = "";
				}
			}

			var result = new CellFormat();
			result.FormatPattern = pattern;
			result.FillPattern = FillPatternStyle.Solid;

			return result;
		}

		protected override string GetDescription()
		{
			string result;
			if (CustomValueSupport != null)
			{
				result = CustomValueSupport.GetDescription();
			}
			else
			{
				result = ZString.Empty;
			}

			return result;
		}
	}
	#endregion

}
