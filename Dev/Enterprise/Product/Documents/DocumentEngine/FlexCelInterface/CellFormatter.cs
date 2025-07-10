using Enterprise.ZArchitecture.Core;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.FlexCelInterface
{
	public class CellFormatter : ICellFormatter
	{
		public virtual string Format(ExcelWorkSheet cellSource, int row, int column)
		{
			object cellContent = cellSource[row, column];

			if (cellContent is TFormula)
			{
				TFormula cellFormula = (TFormula)cellContent;
				cellContent = cellFormula.Result;
			}

			if (cellContent == null)
			{
				return "";
			}

			if (cellContent is string || cellContent is int || cellContent is double)
			{
				return cellContent.ToString().Replace("\r\n", "|>").Replace("\r", "|>").Replace("\n", "|>");
			}

			return (NoResString)"Unhandled CellContent of Type: " + cellContent.GetType();
		}
	}
}