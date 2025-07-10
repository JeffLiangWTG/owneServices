using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;

namespace CargoWise.Windows.UI
{
	public sealed class LabelVariableLengthCaptionRenderer : ControlTextVariableLengthCaptionRenderer
	{
		public LabelVariableLengthCaptionRenderer(Label label) : base(label)
		{
			Control.MouseHover += Control_MouseHover;
		}

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]

		internal void Control_MouseHover(object sender, EventArgs e)
		{
			var labelCaptionRenderer = Label.GetExtension<ILabelCaptionRenderer>();
			var isInHotCaptionFeedbackMode = labelCaptionRenderer?.InHotCaptionFeedbackMode() ?? false;

			if (isInHotCaptionFeedbackMode && ToolTipService.HasToolTip(Control))
			{
				ToolTipService.ClearTooltip(Control);
			}
			else if (!Label.AutoEllipsis && Control.Visible && isCaptionTruncated && !isInHotCaptionFeedbackMode && !ToolTipService.HasToolTip(Control))
			{
				ToolTipService.SetToolTip(Control, Captions?.OrderLongToShort().Last(), 500);
			}
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				Control.MouseHover -= Control_MouseHover;
			}
			base.Dispose(isDisposing);
		}

		Label Label => (Label)base.Control;

		protected override string MeasureBestFitCaption()
		{
			if (Label.AutoSize || Label.AutoEllipsis)
			{
				return base.MeasureBestFitCaption();
			}

			var proposedSize = ControlDpiScalingHelper.NewScaledSize(Control.Width - Control.Padding.Horizontal, Control.Height - Control.Padding.Vertical, false);

			var result = StringRenderingHelper.MeasureBestFit(
				Captions,
				c => StringRenderingHelper.HasObstruction(proposedSize, StringRenderingHelper.MeasureText(c, Control.Font, proposedSize), Control.Font.Height), out isCaptionTruncated);

			var ext = Label.GetExtension<ILabelCaptionRenderer>();
			if (ext != null)
			{
				ext.IsCaptionTruncated = isCaptionTruncated;
			}

			return result;
		}

		bool isCaptionTruncated;
	}
}
