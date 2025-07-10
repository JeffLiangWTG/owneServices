using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	class OwnerDrawMenuItem : ZMenuItem
	{
		public OwnerDrawMenuItem(MultilingualString caption)
			: base(caption)
		{
			OwnerDraw = true;
		}

		public bool DrawShortcutFromTag { get; set; }

		#if !WINZOR

		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			e.Graphics.FillRectangle(BrushProvider.FromColor((e.State & (DrawItemState.Focus | DrawItemState.HotLight | DrawItemState.Selected)) != 0 ? e.BackColor : SystemColors.MenuBar), e.Bounds);
			TextRendererHelper.DrawText(e.Graphics, Text, SystemFonts.MenuFont, e.Bounds.X + ControlDpiScalingHelper.ScaleToCurrentDpiX(16), e.Bounds.Y, BrushProvider.FromColor(e.ForeColor));

			if (DrawShortcutFromTag && Tag != null)
			{
				var shortcutString = Tag.ToString();
				var shortcutStringSize = TextRenderer.MeasureText(shortcutString, SystemFonts.MenuFont);
				TextRendererHelper.DrawText(e.Graphics, shortcutString, SystemFonts.MenuFont, e.Bounds.Right - shortcutStringSize.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(16), e.Bounds.Y, BrushProvider.FromColor(e.ForeColor));
			}
		}

		protected override void OnMeasureItem(MeasureItemEventArgs e)
		{
			var stringSize = e.Graphics.MeasureString(Text, SystemFonts.MenuFont);
			e.ItemWidth = (int)stringSize.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(16);
			e.ItemHeight = (int)stringSize.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(2);
		}
		#else
		public override void ShowShortcutString()
		{
			if (DrawShortcutFromTag && Tag != null)
			{
				ShortcutString = Tag.ToString();
			}
		}
		#endif
	}
}
