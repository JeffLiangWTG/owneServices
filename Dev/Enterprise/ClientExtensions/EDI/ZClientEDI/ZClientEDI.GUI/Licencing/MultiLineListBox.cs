using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Licencing.GUI
{
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public class MultiLineListBox : ListBox
	{
		public MultiLineListBox()
		{
			this.DrawMode = DrawMode.OwnerDrawVariable;
			this.ScrollAlwaysVisible = true;
		}

		protected override void OnMeasureItem(MeasureItemEventArgs e)
		{
			if (Site != null)
			{
				return;
			}

			if (e.Index > -1)
			{
				string text = Items[e.Index].ToString();
				SizeF size = e.Graphics.MeasureString(text, Font, Width);
				int margin = (e.Index == 0) ? 15 : 10;
				e.ItemHeight = (int)size.Height + margin;
				e.ItemWidth = Width;
			}
		}

#if !WINZOR
		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			if (Site != null)
			{
				return;
			}

			if (e.Index > -1)
			{
				string text = Items[e.Index].ToString();

				var g = e.Graphics;
				if ((e.State & DrawItemState.Focus) == 0)
				{
					g.FillRectangle(SystemBrushes.Window, e.Bounds);
					TextRendererHelper.DrawText(g, text, Font, e.Bounds, SystemBrushes.WindowText);
					int y = e.Bounds.Bottom - 1;
					g.DrawLine(SystemPens.Highlight,
						ControlDpiScalingHelper.NewScaledPoint(
							ControlDpiScalingHelper.UnscaleFromCurrentDpiX(e.Bounds.Left),
							ControlDpiScalingHelper.UnscaleFromCurrentDpiY(y)),
						ControlDpiScalingHelper.NewScaledPoint(
							ControlDpiScalingHelper.UnscaleFromCurrentDpiX(e.Bounds.Right),
							ControlDpiScalingHelper.UnscaleFromCurrentDpiY(y)));
				}
				else
				{
					g.FillRectangle(SystemBrushes.Highlight, e.Bounds);
					TextRendererHelper.DrawText(g, text, Font, e.Bounds, SystemBrushes.HighlightText);
				}
			}
		}
#endif

	}
}
