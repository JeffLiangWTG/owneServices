using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Layout;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZCollapsiblePanel : ZPanel, ISplitterLayoutSaveProvider, IAcceptNegativeSplitterPosition
	{
		public ZCollapsiblePanel()
		{
			Padding = new Padding(0, ControlDpiScalingHelper.ScaleToCurrentDpiY(CaptionHeight), 0, 0);
		}

		public const int CaptionHeight = 20;

		public bool IsCollapsed
		{
			get => isCollapsed;
			set
			{
				if (isCollapsed != value)
				{
					isCollapsed = value;
					UpdateSizeAndRepaint();
				}
			}
		}
		bool isCollapsed;

		bool IsVertical => Dock == DockStyle.Left || Dock == DockStyle.Right;

		protected virtual bool IsHorizontal => Dock == DockStyle.Top || Dock == DockStyle.Bottom;

		void UpdateSizeAndRepaint()
		{
			if (IsCollapsed)
			{
				if (linkedSplitter != null)
				{
					linkedSplitter.Visible = false;
				}

				foreach (Control control in Controls)
				{
					control.Visible = false;
				}

				if (IsVertical)
				{
					previousSize = Width;
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(CaptionHeight);
				}
				else if (IsHorizontal)
				{
					previousSize = Height;
					Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(CaptionHeight);
				}
			}
			else
			{
				if (IsVertical)
				{
					Width = ControlDpiScalingHelper.MarkAsScaled(previousSize);
				}
				else if (IsHorizontal)
				{
					Height = ControlDpiScalingHelper.MarkAsScaled(previousSize);
				}

				foreach (Control control in Controls)
				{
					control.Visible = true;
				}

				if (linkedSplitter != null)
				{
					linkedSplitter.Visible = true;
				}
			}

			Invalidate();
			Update();
		}

		int previousSize = 200;

		public void LinkSplitter(KSplitter splitter)
		{
			linkedSplitter = splitter;
			if (linkedSplitter != null)
			{
				linkedSplitter.Visible = !IsCollapsed;
				linkedSplitter.DoNotSaveSplitterLayout = true;
			}
		}

		KSplitter linkedSplitter;

		#region Paint

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			e.Graphics.FillRectangle(BrushProvider.FromColor(BackColor), e.ClipRectangle);
			DrawCollapseButton(e);
			DrawCaption(e);
		}

		void DrawCollapseButton(PaintEventArgs e)
		{
			var buttonSize = ControlDpiScalingHelper.NewScaledSize(CaptionHeight, CaptionHeight);
			var rectangle = ControlDpiScalingHelper.NewScaledRectangle(ClientRectangle.Location, buttonSize, false);
			var thickness = ControlDpiScalingHelper.ScaleToCurrentDpiX(2);

			if (IsHovering)
			{
				e.Graphics.FillRectangle(BrushProvider.FromColor(SystemColors.MenuHighlight), rectangle);
			}

			using (var pen = new Pen(ForeColor, thickness))
			{
				if (IsCollapsed && Dock == DockStyle.Right || !IsCollapsed && Dock != DockStyle.Right)
				{
					DrawLeftTriangle(e.Graphics, pen, thickness, rectangle);
				}
				else
				{
					DrawRightTriangle(e.Graphics, pen, thickness, rectangle);
				}
			}
		}

		void DrawLeftTriangle(Graphics g, Pen pen, int thickness, Rectangle r)
		{
			var x1 = r.Left + 2 * thickness;
			var x2 = r.Right - 3 * thickness - 1;
			var y1 = r.Top + 2 * thickness;
			var y2 = r.Top + r.Height / 2 - 1;
			var y3 = r.Bottom - 2 * thickness - 1;

			DrawTriangle(g, pen, ControlDpiScalingHelper.NewScaledPoint(x2, y1, false), ControlDpiScalingHelper.NewScaledPoint(x1, y2, false), ControlDpiScalingHelper.NewScaledPoint(x2, y3, false));
		}

		void DrawRightTriangle(Graphics g, Pen pen, int thickness, Rectangle r)
		{
			var x1 = r.Right - 2 * thickness - 1;
			var x2 = r.Left + 3 * thickness;
			var y1 = r.Top + 2 * thickness;
			var y2 = r.Top + r.Height / 2 - 1;
			var y3 = r.Bottom - 2 * thickness - 1;

			DrawTriangle(g, pen, ControlDpiScalingHelper.NewScaledPoint(x2, y1, false), ControlDpiScalingHelper.NewScaledPoint(x1, y2, false), ControlDpiScalingHelper.NewScaledPoint(x2, y3, false));
		}

		void DrawTriangle(Graphics g, Pen pen, Point a, Point b, Point c)
		{
			if (IsHovering && IsPressed)
			{
				g.FillPolygon(BrushProvider.FromColor(ForeColor), new[] { a, b, c });
			}
			else
			{
				g.DrawPolygon(pen, new[] { a, b, c });
			}
		}

		void DrawCaption(PaintEventArgs e)
		{
			var textBrush = BrushProvider.FromColor(ForeColor);

			if (IsCollapsed && IsVertical)
			{
				var shiftX = ControlDpiScalingHelper.ScaleToCurrentDpiX(2);
				var shiftY = ControlDpiScalingHelper.ScaleToCurrentDpiY(CaptionHeight + 8);
				var rectangle = ControlDpiScalingHelper.NewScaledRectangle(ClientRectangle.X + shiftX, ClientRectangle.Y + shiftY, ClientRectangle.Width, ClientRectangle.Height + shiftY, false);

				using (var stringFormat = new StringFormat
				{
					Alignment = StringAlignment.Near,
					Trimming = StringTrimming.None,
					FormatFlags = StringFormatFlags.DirectionVertical,
				})
				{
					e.Graphics.DrawString(Text, HeaderFont, textBrush, rectangle, stringFormat);
				}
			}
			else
			{
				var shiftX = ControlDpiScalingHelper.ScaleToCurrentDpiX(CaptionHeight + 8);
				var shiftY = ControlDpiScalingHelper.ScaleToCurrentDpiY(2);
				var sizeY = ControlDpiScalingHelper.ScaleToCurrentDpiY(CaptionHeight);
				var rectangle = ControlDpiScalingHelper.NewScaledRectangle(ClientRectangle.X + shiftX, ClientRectangle.Y + shiftY, ClientRectangle.Width - shiftX, sizeY, false);

				using (var stringFormat = new StringFormat
				{
					Alignment = StringAlignment.Near,
					Trimming = StringTrimming.None,
				})
				{
					e.Graphics.DrawString(Text, HeaderFont, textBrush, rectangle, stringFormat);
				}
			}
		}

		Font HeaderFont => headerFont ?? (headerFont = OFont.GetFontBold());
		Font headerFont;

		#endregion

		#region Mouse

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (e.Button == MouseButtons.Left)
			{
				IsPressed = IsHovering;
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (e.Button == MouseButtons.Left)
			{
				if (IsHovering && IsPressed)
				{
					IsCollapsed = !IsCollapsed;
				}
				IsHovering = false;
				IsPressed = false;
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			var buttonSize = ControlDpiScalingHelper.NewScaledSize(CaptionHeight, CaptionHeight);
			var rectangle = ControlDpiScalingHelper.NewScaledRectangle(ClientRectangle.Location, buttonSize, false);
			IsHovering = rectangle.Contains(e.Location);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			IsHovering = false;
			IsPressed = false;
		}

		bool IsHovering
		{
			get => isHovering;
			set
			{
				if (isHovering != value)
				{
					isHovering = value;
					Invalidate();
					Update();
				}
			}
		}
		bool isHovering;

		bool IsPressed
		{
			get => isPressed;
			set
			{
				if (isPressed != value)
				{
					isPressed = value;
					Invalidate();
					Update();
				}
			}
		}
		bool isPressed;

		#endregion

		#region ISplitterLayoutSaveProvider

		int ISplitterLayoutSaveProvider.SplitterPosition
		{
			get
			{
				var scaledPosition = IsCollapsed ? -previousSize : (IsVertical ? Width : Height);
				var unscaledPosition = IsVertical
					? ControlDpiScalingHelper.UnscaleFromCurrentDpiX(scaledPosition)
					: ControlDpiScalingHelper.UnscaleFromCurrentDpiY(scaledPosition);
				return unscaledPosition;
			}
			set
			{
				if (value == 0)
				{
					return; // Not initialized value
				}

				if (value < 0)
				{
					IsCollapsed = true;
					previousSize = ControlDpiScalingHelper.ScaleToCurrentDpiX(-value);
				}
				else
				{
					var scaledPosition = IsVertical
						? ControlDpiScalingHelper.ScaleToCurrentDpiX(value)
						: ControlDpiScalingHelper.ScaleToCurrentDpiY(value);

					if (IsCollapsed)
					{
						previousSize = scaledPosition;
						IsCollapsed = false;
					}
					else
					{
						if (IsVertical)
						{
							Width = ControlDpiScalingHelper.MarkAsScaled(scaledPosition);
						}
						else
						{
							Height = ControlDpiScalingHelper.MarkAsScaled(scaledPosition);
						}
					}
				}
			}
		}

		int ISplitterLayoutSaveProvider.ContainerSize => ControlDpiScalingHelper.ScaleToCurrentDpiX(100); // Keep it same size (not related to parent form size)

		bool ISplitterLayoutSaveProvider.IsSplitterFixed => false;

		bool ISplitterLayoutSaveProvider.IsLayoutRestored { get; set; }

		#endregion
	}
}
