using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.FlexCelInterface
{
	public class ExcelCell
	{
		public ExcelCell(ExcelWorkSheet workSheet, int row, int column)
		{
			this.workSheet = workSheet;
			this.row = row;
			this.column = column;
		}

		readonly ExcelWorkSheet workSheet;
		readonly ZInt row;
		readonly ZInt column;

		internal static readonly Regex CellReferenceRegex = new Regex(@"([a-zA-z]{1,2}[0-9]{1,5})", RegexOptions.Compiled);

		public const int ExcelCellTextPaddingInDeviceIndependentPixels = 5;

		public ExcelWorkSheet WorkSheet => workSheet;

		public ZInt Row => row;

		public ZInt Column => column;

		public ZString Reference => CellReference.GetCellRef(row, column);

		public object Value
		{
			get
			{
				object result = null;

				if (WorkSheet != null && WorkSheet.ParentExcelInterface != null)
				{
					result = WorkSheet.ParentExcelInterface.Xls.GetCellValue(Row + 1, Column + 1);
				}

				return result;
			}
			set
			{
				WorkSheet[Row, Column] = value;
			}
		}

		public string FormattedValue
		{
			get
			{
				string result = null;

				if (WorkSheet != null && WorkSheet.ParentExcelInterface != null)
				{
					result = WorkSheet.ParentExcelInterface.Xls.GetStringFromCell(Row + 1, Column + 1).ToString();
				}

				return result;
			}
		}

		public string ValueSourceText
		{
			get
			{
				var result = string.Empty;

				if (Value != null)
				{
					if (IsFormula)
					{
						result = ((TFormula)Value).Text;
					}
					else
					{
						result = Value.ToString();
					}
				}

				return result;
			}
			set
			{
				if (IsFormula)
				{
					WorkSheet[Row, Column] = new TFormula(value);
				}
				else
				{
					WorkSheet[Row, Column] = value;
				}
			}
		}

		public void SetValueDetectingFormulaeFromLeadingEqualsSign(string value)
		{
			if (ValueSourceText != value)
			{
				if (value.StartsWith("=") || value.StartsWith("{="))
				{
					WorkSheet[Row, Column] = new TFormula(value);
				}
				else
				{
					WorkSheet[Row, Column] = value;
				}
			}
		}

		public bool IsFormula => Value != null && Value is TFormula;

		public bool IsEmpty => string.IsNullOrEmpty(ValueSourceText);

		public CellFormat Format
		{
			get { return WorkSheet.GetCellFormat(Row, Column); }
			set { WorkSheet.SetCellFormat(Row, Column, value); }
		}

		internal int Width => WorkSheet.GetColWidth(Column);

		internal int Height
		{
			get { return WorkSheet.GetRowHeight(Row); }
			set { WorkSheet.SetRowHeight(Row, value); }
		}

		internal bool IsMerged => WorkSheet.IsCellMerged(Row, Column);

		internal ExcelCellRange MergedRange => WorkSheet.GetMergedCellRange(Row, Column);
	}
}
