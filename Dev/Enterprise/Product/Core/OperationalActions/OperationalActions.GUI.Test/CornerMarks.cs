using System.Drawing;
using System.Windows.Forms;

namespace Enterprise.Services.OperationalActions.GUI.Testing
{
	sealed class CornerMarks : Control
	{
#if !WINZOR

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			Size minSize;

			if (Parent != null && !(minSize = Parent.MinimumSize).IsEmpty)
			{
				Render(e.Graphics, Pens.Blue, CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledRectangle(-Location.X, -Location.Y, minSize.Width - 1, minSize.Height - 1));
			}

			Render(e.Graphics, Pens.Red, CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledRectangle(0, 0, Size.Width - 1, Size.Height - 1));
		}

		static void Render(Graphics g, Pen pen, Rectangle r)
		{
			for (int i = 0; i < 4; i++)
			{
				int x0;
				int x1;
				int y0;
				int y1;

				if ((i & 1) == 0)
				{
					x0 = r.Left;
					x1 = r.Left + 10;
				}
				else
				{
					x0 = r.Right;
					x1 = r.Right - 10;
				}

				if ((i & 2) == 0)
				{
					y0 = r.Top;
					y1 = r.Top + 10;
				}
				else
				{
					y0 = r.Bottom;
					y1 = r.Bottom - 10;
				}

				g.DrawLine(pen, x0, y0, x0, y1);
				g.DrawLine(pen, x0, y0, x1, y1);
				g.DrawLine(pen, x0, y0, x1, y0);
			}
		}
#endif
	}
}
