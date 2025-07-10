#if DEBUG

extern alias CWAppContext;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Interop;
using CWAppContext::CargoWise.Application;
using static CargoWise.Windows.UI.ControlDpiScalingHelper;

namespace CargoWise.Windows.UI
{
	public class ControlInformationOverlayForm : Form
	{
		public const int OutlineThickness = 2;

		readonly Pen parentPen;
		readonly Pen controlPen;

		internal bool hideInformation;

		public ControlInformationOverlayForm()
		{
			BackColor = Color.SteelBlue;
			TransparencyKey = Color.SteelBlue;
			Opacity = 0.8;

			FormBorderStyle = FormBorderStyle.None;
			ControlBox = false;
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.Manual;
			AutoScaleMode = AutoScaleMode.None;

			parentPen = new Pen(Color.Magenta, OutlineThickness);
			controlPen = new Pen(Color.LimeGreen, OutlineThickness);
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			base.OnPaintBackground(e);
			if (HighlightedControl != null)
			{
				DrawControlBounds(e.Graphics, HighlightedControl.Parent, true, parentPen);
				DrawControlBounds(e.Graphics, HighlightedControl, HighlightedControl is Form, controlPen, Brushes.LightSeaGreen);

				if (!hideInformation)
				{
					var description = BuildInformation(HighlightedControl);

					var format = new StringFormat { FormatFlags = StringFormatFlags.NoWrap };
					var rect = GetInfoboxBounds(e.Graphics, description, Font, format);

					e.Graphics.FillRectangle(Brushes.White, rect);
					e.Graphics.DrawString(description, Font, Brushes.DarkGreen, rect, format);
				}
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left && ModifierKeys.HasFlag(Keys.Control) && HighlightedControl != null)
			{
				ObjectFactory.Get<IInfoDiggerProvider>().ShowInfoDigger(HighlightedControl);
			}
		}

		RectangleF GetInfoboxBounds(Graphics graphics, string content, Font font, StringFormat format)
		{
			var form = HighlightedControl.FindForm();
			var bottomRight = form.PointToScreen(NewScaledPoint(form.ClientSize, false));

			var size = graphics.MeasureString(content, font, int.MaxValue, format).ToSize() + NewScaledSize(5, 5, true);
			var rect = NewScaledRectangle(bottomRight.X - size.Width, bottomRight.Y - size.Height, size.Width, size.Height, false);

			var controlBounds = NewScaledRectangle(HighlightedControl.PointToScreen(Point.Empty), HighlightedControl.Size, false);
			if (rect.IntersectsWith(controlBounds) && rect.Width < controlBounds.X)
			{
				SetX(ref rect, form.PointToScreen(Point.Empty).X, false);
			}

			return NewScaledRectangle(PointToClient(rect.Location), rect.Size, false);
		}

		void DrawControlBounds(Graphics graphics, Control c, bool clientSize, Pen border = null, Brush fill = null)
		{
			if (c != null)
			{
				var screenLocation = c.Parent?.PointToScreen(c.Location) ?? c.PointToScreen(Point.Empty);
				var rect = NewScaledRectangle(PointToClient(screenLocation), clientSize ? c.ClientSize : c.Size, false);

				if (fill != null)
				{
					graphics.FillRectangle(fill, rect);
				}

				if (border != null)
				{
					graphics.DrawRectangle(border, rect);
				}
			}
		}

		string BuildInformation(Control c) => new StringBuilder()
				.AppendLine("Name: " + c.Name)
				.AppendLine("Type: " + c.GetType().FullName)
				.AppendFormat(CultureInfo.CurrentCulture, "Location: {0}, {1}\r\n", c.Left, c.Top)
				.AppendFormat(CultureInfo.CurrentCulture, "Size: {0}, {1}\r\n", c.Width, c.Height)
				.AppendFormat(CultureInfo.CurrentCulture, "Padding (NESW): {0}, {1}, {2}, {3}\r\n", c.Padding.Top, c.Padding.Right, c.Padding.Bottom, c.Padding.Right)
				.AppendFormat(CultureInfo.CurrentCulture, "Margin (NESW): {0}, {1}, {2}, {3}\r\n", c.Margin.Top, c.Margin.Right, c.Margin.Bottom, c.Margin.Right)
				.AppendLine()
				.AppendLine("Click for more details...")
				.AppendLine()
				.ToString();

		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case WindowsMessage.WM_ACTIVATE:
				case WindowsMessage.WM_ACTIVATEAPP:
					return;

				default:
					base.WndProc(ref m);
					return;
			}
		}

		protected override bool ShowWithoutActivation => true;

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			parentPen.Dispose();
			controlPen.Dispose();
		}

		public void CenterOnControl(Control c, Form caller)
		{
			hideInformation = caller.Name == "InfoDiggerForm";
			Bounds = FindMinimumSize(c);
			HighlightedControl = c;
			if (!Visible)
			{
				Show(c.FindForm());
				caller.Focus();
			}
			else
			{
				Invalidate();
			}
		}

		[return:DpiState(DpiState.ScaledVariant)]
		Rectangle FindMinimumSize(Control c)
		{
			var form = c.FindForm();
			var parent = c.Parent;
			var bottomRightPoints = StripNulls(
				NewScaledPoint(form.Right, form.Bottom, false),
				c.PointToScreen(NewScaledPoint(c.Width, c.Height, false)),
				parent?.PointToScreen(NewScaledPoint(parent.ClientSize.Width, parent.ClientSize.Height, false))
			);

			var topLeftPoints = StripNulls(
				form.Location,
				c.PointToScreen(Point.Empty),
				parent?.PointToScreen(Point.Empty)
			);

			var location = NewScaledPoint(topLeftPoints.Min(p => p.X), topLeftPoints.Min(p => p.Y), false);
			var size = NewScaledSize(
				(int)(controlPen.Width + bottomRightPoints.Max(p => p.X) - location.X),
				(int)(controlPen.Width + bottomRightPoints.Max(p => p.Y) - location.Y),
				false
			);

			return NewScaledRectangle(location, size, false);
		}

		static IEnumerable<T> StripNulls<T>(params T?[] nullables) where T : struct
			=> nullables.Where(n => n.HasValue).Select(n => n.Value).ToList();

		public Control HighlightedControl { get; private set; }
	}
}

#endif
