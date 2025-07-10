using System.Diagnostics;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using FlexCel.Core;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration
{
	[DebuggerDisplay("Height={Height}")]
	public class FlexCelRow : IRow
	{
		public FlexCelRow(FlexCelWorksheet worksheet, int number)
		{
			this.worksheet = Argument.NotNull(worksheet, "worksheet");
			this.Number = number;
		}

		readonly FlexCelWorksheet worksheet;

		public int Number { get; private set; }
		
		public double Height
		{
			get
			{
				if (height < 0)
				{
					var flxCelRowHeight = worksheet.GetRowHeight(Number, true);

					//http://www.tmssoftware.com/flexcel/doc/RowMult.html"
					//http://www.tmssoftware.com/site/flexcelnet.asp
					height = flxCelRowHeight / ExcelMetrics.RowMultDisplay(worksheet);
				}

				return height;
			}
		}

		double height = -1d;
	}
}