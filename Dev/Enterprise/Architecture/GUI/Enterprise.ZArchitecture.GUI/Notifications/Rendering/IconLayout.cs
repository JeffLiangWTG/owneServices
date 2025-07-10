using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.Notifications
{
	public class IconLayout : IconLayoutBase
	{
		public IconAlignment Align { get; set; }

		public IconLayout(Control target, IconAlignment align)
			: this(target, align, false)
		{ }

		public IconLayout(Control target, IconAlignment align, bool semiTransparent)
		{
			if (!Enum.IsDefined(typeof(IconAlignment), align))
			{
				throw new ArgumentOutOfRangeException(nameof(align), align, "Invalid alignment value.");
			}

			Target = target;
			SemiTransparent = semiTransparent;
			Align = align;
		}

		public override Rectangle Compute(Rectangle area, Size clip)
		{
			var xShift = area.X;
			var yShift = area.Y;

			switch (Align)
			{
				case IconAlignment.Left:

					return ControlDpiScalingHelper.NewScaledRectangle(ControlDpiScalingHelper.ScaleToCurrentDpiX(XOffset) + xShift, ControlDpiScalingHelper.ScaleToCurrentDpiY(YOffset) + yShift, clip.Width, clip.Height, false);

				case IconAlignment.Center:

					return ControlDpiScalingHelper.NewScaledRectangle(area.Width / 2 - clip.Width / 2 + xShift, area.Height / 2 - clip.Height / 2 + yShift, clip.Width, clip.Height, false);

				case IconAlignment.Right:

					return ControlDpiScalingHelper.NewScaledRectangle(
						area.Width - clip.Width + xShift - ControlDpiScalingHelper.ScaleToCurrentDpiX(XOffset), ControlDpiScalingHelper.ScaleToCurrentDpiY(YOffset) + yShift, clip.Width, clip.Height, false);

				default:
					return Rectangle.Empty;
			}
		}
	}
}
