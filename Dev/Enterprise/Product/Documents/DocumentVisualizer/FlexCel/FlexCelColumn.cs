using System.Diagnostics;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using FlexCel.Core;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration
{
	[DebuggerDisplay("Width={Width}")]
	public class FlexCelColumn : IColumn
	{
		public FlexCelColumn(FlexCelWorksheet worksheet, int number)
		{
			this.worksheet = Argument.NotNull(worksheet, "worksheet");
			this.Number = number;
		}

		readonly FlexCelWorksheet worksheet;

		public int Number { get; private set; }
		
		public double Width
		{
			get
			{
				if (width < 0)
				{
					var flxCelColumnWidth = worksheet.GetColWidth(this.Number, true);

					//http://www.tmssoftware.com/flexcel/doc/ColMult.html
					width = flxCelColumnWidth / ExcelMetrics.ColMultDisplay(worksheet);
				}

				return width;
			}
		}

		double width = -1d;
	}
}