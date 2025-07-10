using System;
using System.Diagnostics;
using System.Drawing;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration
{
	[DebuggerDisplay("[{mergedRange.TopRow},{mergedRange.LeftColumn}] : [{mergedRange.BottomRow},{mergedRange.RightColumn}] {Value}")]
	public class FlexCelCell : ICell
	{
		public FlexCelCell(FlexCelWorksheet worksheet, IRange mergedRange)
		{
			this.worksheet = Argument.NotNull(worksheet, "worksheet");
			this.mergedRange = Argument.NotNull(mergedRange, "mergedRange");

			this.lazyDrawing = new Lazy<IDrawing>(GetDrawing);
		}

		readonly FlexCelWorksheet worksheet;
		readonly IRange mergedRange;

		#region ICell members

		public object Value
		{
			get
			{
				if (value == null)
				{
					value = worksheet.GetCellValueEx(mergedRange.TopRow, mergedRange.LeftColumn);
				}

				return value;
			}
		}

		object value;

		public bool HasDynamicContent => false;

		public double Height
		{
			get
			{
				if (height < 0)
				{
					height = 0;

					for (int row = mergedRange.TopRow; row <= mergedRange.BottomRow; row++)
					{
						height += worksheet.Rows.GetAt(row).Height;
					}
				}

				return height;
			}
		}

		double height = -1d;

		public double Width
		{
			get
			{
				if (width < 0)
				{
					width = 0;

					for (int column = mergedRange.LeftColumn; column <= mergedRange.RightColumn; column++)
					{
						width += worksheet.Columns.GetAt(column).Width;
					}
				}

				return width;
			}
		}

		double width = -1d;

		public RectangleF Padding => padding ?? (padding = new RectangleF(0.72f, 0.72f, 0.72f, 0)).Value;
		RectangleF? padding;
		
		public IFormat Format => format ?? (format = worksheet.GetCellFormatEx(this.TopRow, this.LeftColumn));
		IFormat format;

		public IDrawing Drawing => lazyDrawing.Value;
		readonly Lazy<IDrawing> lazyDrawing;

		IDrawing GetDrawing()
		{
			foreach (var drawing in worksheet.Drawings)
			{
				if (drawing.LeftColumn == LeftColumn && drawing.TopRow == TopRow)
				{
					return drawing;
				}
			}

			return null;
		}

		#endregion

		#region IRange members
		
		public int TopRow => mergedRange.TopRow;
		public int BottomRow => mergedRange.BottomRow;
		public int LeftColumn => mergedRange.LeftColumn;
		public int RightColumn => mergedRange.RightColumn;

		#endregion
	}
}