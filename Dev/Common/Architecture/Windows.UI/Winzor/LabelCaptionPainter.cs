using System;
using System.Windows.Forms;
using WinzorFramework.Extensions;

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
				if (!renderer.isDisposed && renderer.Control != null && !renderer.Control.IsDisposed)
				{
					renderer.InvalidateCore();
				}
			}

			public void HookRenderSurfaceEvents()
			{
			}

			public void UnhookRenderSurfaceEvents()
			{
				if (label != null)
				{
					label.Click -= new EventHandler(renderSurface_Click);
					renderer.renderSurface.WinzorSpecificControls.Remove(label);
					label = null;
				}
			}

			void renderSurface_Click(object sender, EventArgs e)
			{
				if (renderer.InHotCaptionFeedbackMode())
				{
					renderer.OpenCaptionFeedback();
				}
			}

			public void Paint(PaintEventArgs e, LabelCaptionMeasurement captionMeasurement)
			{
				if (label == null)
				{
					label = new CaptionLabel();
					label.Click += new EventHandler(renderSurface_Click);
					label.BindingControl = renderer.Control;
					renderer.renderSurface.WinzorSpecificControls.Add(label);
				}
				SetLabelProperties(label, captionMeasurement);
			}

			void SetLabelProperties(Label label, LabelCaptionMeasurement captionMeasurement)
			{
				label.Text = captionMeasurement.Caption;
				label.Bounds = captionMeasurement.CaptionBounds;
			}

#if DEBUG
			internal void renderSurface_MouseMove(object sender, MouseEventArgs e)
			{
			}
#endif

			public void Dispose()
			{
				label?.Dispose();
			}

			readonly LabelCaptionRenderer renderer;
			CaptionLabel label;
		}

		public class CaptionLabel : Label
		{
			protected override string ControlStyleString => base.ControlStyleString + CalibrationFontColorStyleString;

			string CalibrationFontColorStyleString => ForeColor == DefaultForeColor ? $"color:{ForeColor.GetColorStyleValue()};" : string.Empty;

			internal Control BindingControl { get; set; }

			protected override bool ShouldRender => base.ShouldRender && ((BindingControl is null) || (BindingControl.Visible && BindingControl.Width > 0 && BindingControl.Height > 0));

			protected override void OnMouseDoubleClick(MouseEventArgs e)
			{
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				BindingControl = null;
			}
		}
	}
}
