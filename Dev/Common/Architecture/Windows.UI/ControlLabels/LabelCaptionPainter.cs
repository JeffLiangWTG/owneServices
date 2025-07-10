using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;

namespace CargoWise.Windows.UI
{
	public partial class LabelCaptionRenderer
	{
		protected class LabelCaptionPainter : IDisposable
		{
			public LabelCaptionPainter(LabelCaptionRenderer renderer)
			{
				this.renderer = renderer;
			}

			public void Invalidate()
			{
				if (!renderer.isDisposed && !isInvalidated)
				{
					Application.Idle += ApplicationIdle_Invalidate;
					isInvalidated = true;
				}
			}

			bool isInvalidated;

			void ApplicationIdle_Invalidate(object sender, EventArgs e)
			{
				Application.Idle -= ApplicationIdle_Invalidate;
				isInvalidated = false;
				if (renderer.Control != null && !renderer.Control.IsDisposed)
				{
					renderer.InvalidateCore();
				}
			}

			public void HookRenderSurfaceEvents()
			{
				renderer.renderSurface.MouseMove += new MouseEventHandler(renderSurface_MouseMove);
				renderer.renderSurface.MouseLeave += new EventHandler(renderSurface_MouseLeave);
				renderer.renderSurface.Click += new EventHandler(renderSurface_Click);
			}

			public void UnhookRenderSurfaceEvents()
			{
				renderer.renderSurface.MouseMove -= new MouseEventHandler(renderSurface_MouseMove);
				renderer.renderSurface.MouseLeave -= new EventHandler(renderSurface_MouseLeave);
				renderer.renderSurface.Click -= new EventHandler(renderSurface_Click);
			}

			internal void renderSurface_MouseMove(object sender, MouseEventArgs e)
			{
				if (renderer.lastPaintedRectangle.Contains((e.Location)))
				{
					if (renderer.InHotCaptionFeedbackMode())
					{
						hotCaptionFeedbackOn = true;
						renderer.renderSurface.Invalidate(Rectangle.Inflate(renderer.lastPaintedRectangle, 2, 2));
						renderer.renderSurface.Update();
					}
					else if (renderer.control.Visible && !isRenderSurfaceToolTipShown)
					{
						if (!string.IsNullOrEmpty(renderer.manuallyToolTip))
						{
							isRenderSurfaceToolTipShown = true;
							ToolTipService.ShowToolTip(renderer.renderSurface, renderer.manuallyToolTip, e.Location, 2000);
						}
						else if (renderer.IsCaptionTruncated)
						{
							isRenderSurfaceToolTipShown = true;
							ToolTipService.ShowToolTip(renderer.renderSurface, renderer.Captions.OrderLongToShort().Last(), e.Location, 2000);
						}
					}
				}
				else if (hotCaptionFeedbackOn)
				{
					renderSurface_MouseLeave(sender, e);
				}
				else if (isRenderSurfaceToolTipShown)
				{
					ToolTipService.HideToolTip(renderer.renderSurface);
					isRenderSurfaceToolTipShown = false;
				}
			}

			void renderSurface_MouseLeave(object sender, EventArgs e)
			{
				if (hotCaptionFeedbackOn)
				{
					hotCaptionFeedbackOn = false;
					renderer.renderSurface.Invalidate(Rectangle.Inflate(renderer.lastPaintedRectangle, 2, 2));
					renderer.renderSurface.Update();
				}
			}

			void renderSurface_Click(object sender, EventArgs e)
			{
				if (hotCaptionFeedbackOn)
				{
					renderer.OpenCaptionFeedback();
				}
			}

			public void Paint(PaintEventArgs e, LabelCaptionMeasurement captionMeasurement)
			{
				DrawString(e.Graphics, captionMeasurement.Caption, captionMeasurement.CaptionBounds);
				if (hotCaptionFeedbackOn)
				{
					renderer.PaintHotCaptionHighlight(e.Graphics, Rectangle.Inflate(captionMeasurement.CaptionBounds, 2, 2));
				}
			}

			void DrawString(Graphics graphics, string str, Rectangle rectangle)
			{
				TextRenderer.DrawText(graphics, str.Replace("&", "&&"), renderer.Font, rectangle,
						!renderer.ForeColor.IsEmpty ? renderer.ForeColor : (renderer.RenderSurface?.ForeColor ?? Color.Empty),
						TextFormatFlags.WordBreak);
			}

			public void Dispose()
			{
				if (isInvalidated)
				{
					Application.Idle -= ApplicationIdle_Invalidate;
					isInvalidated = false;
				}
			}

			readonly LabelCaptionRenderer renderer;
			bool hotCaptionFeedbackOn;
			bool isRenderSurfaceToolTipShown;
		}
	}
}
