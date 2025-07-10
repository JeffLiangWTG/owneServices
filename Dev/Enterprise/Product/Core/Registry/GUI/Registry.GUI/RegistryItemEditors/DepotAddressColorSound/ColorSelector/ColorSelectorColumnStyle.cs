using System;
using System.Drawing;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public class ColorSelectorColumnStyle : ZCodeFindBoxColumnStyle
	{
		public ColorSelectorColumnStyle(ColorSelectorColumnStyleInfo info)
			: base(() => new ColorSelectorFindBox(), info)
		{
		}

#if !WINZOR

		protected override void Paint(Graphics graphics, Rectangle bounds, System.Windows.Forms.CurrencyManager source, int paintingRowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			var color = ColorManager.ColorFromString(new ZString(GetColumnValueAtRow(source, paintingRowNum)));
			if (color != Color.Empty)
			{
				var brush = new SolidBrush(color);
				base.Paint(graphics, bounds, source, paintingRowNum, brush, brush, alignToRight);
			}
			else
			{
				base.Paint(graphics, bounds, source, paintingRowNum, backBrush, foreBrush, alignToRight);
			}
		}

#endif
	}

	public class ColorSelectorColumnStyleInfo : ZCodeFindBoxColumnStyleInfo
	{
		public override Type ColumnStyleType
		{
			get
			{
				return typeof(ColorSelectorColumnStyle);
			}
		}
	}
}
