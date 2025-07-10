using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public partial class ZSearchButton : ZDropButton
	{
		protected override ZDropForm NewDropForm()
		{
			return new ZSearchForm(ParentDropEdit);
		}

		public ZSearchForm DropDownSeacrhForm
		{
			get { return (ZSearchForm)DropDown; }
		}

		#if !WINZOR

		protected override void OnPaintCore(PaintEventArgs e)
		{
			var buttonRectangle = ControlDpiScalingHelper.NewScaledRectangle(ButtonPoint.X, ButtonPoint.Y, ButtonSize.Width, ButtonSize.Height, false);
			var readOnly = ParentDropEdit == null || ParentDropEdit.ReadOnly;

			var borderSize = ShouldDrawBorder ? ControlDpiScalingHelper.OnePixel * 2 : 0;
			var buttonImage = IsDroppedDown ? Properties.Resources.Cross : Properties.Resources.Search;

			var imageOffset = ControlDpiScalingHelper.NewScaledPoint(
				Math.Max(0, (buttonRectangle.Width - buttonImage.Width) / 2),
				Math.Max(0, (buttonRectangle.Height - buttonImage.Height) / 2),
				false);

			var imageSection = ControlDpiScalingHelper.NewScaledRectangle(
				buttonRectangle.X + imageOffset.X + borderSize,
				buttonRectangle.Y + imageOffset.Y + borderSize,
				Math.Min(buttonImage.Width, buttonRectangle.Width - 2 * (imageOffset.X + borderSize)),
				Math.Min(buttonImage.Height, buttonRectangle.Height - 2 * (imageOffset.Y + borderSize)),
				false
			);

			if (readOnly)
			{
				e.Graphics.FillRectangle(SystemBrushes.Control, ClientRectangle);
			}
			else
			{
				e.Graphics.FillRectangle(Brushes.LightGray, buttonRectangle);
			}
			e.Graphics.DrawImage(buttonImage, imageSection);

			if (ShouldDrawBorder)
			{
				ControlPaint.DrawBorder3D(e.Graphics, buttonRectangle, Border3DStyle.Flat);
				ControlPaint.DrawBorder(e.Graphics, ClientRectangle, Color.Black, ButtonBorderStyle.Solid);
			}
		}

		#endif
	}
}
