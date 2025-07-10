using System;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class LabelVariableLengthCaptionRendererTest : TestCase
	{
		public void TestCaptions()
		{
			using (var form = new KForm())
			using (var label = new KLabel())
			{
				form.Controls.Add(label);
				var captionRenderer = new LabelVariableLengthCaptionRenderer(label);
				captionRenderer.Captions = new string[] { "Caption 1", "Caption Longer 2", "Caption Even Longer 3" };
				form.Show();

				label.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
				AssertEquals("Caption Longer 2", label.Text);
				captionRenderer.Control_MouseHover(label, EventArgs.Empty);
				Assert(!ToolTipService.HasToolTip(label));

				label.AutoEllipsis = true;
				captionRenderer.Captions = new string[] { "Caption Even Longer" };
				AssertEquals("Caption Even Longer", label.Text);
				captionRenderer.Control_MouseHover(label, EventArgs.Empty);
				Assert(!ToolTipService.HasToolTip(label));

				label.AutoEllipsis = false;
				captionRenderer.Captions = new string[] { "Caption Even Longer 1" };
				captionRenderer.Control_MouseHover(label, EventArgs.Empty);
				Assert(label.Text.StartsWith("Caption Even L") && label.Text.EndsWith("..."));
				Assert(ToolTipService.HasToolTip(label));
				AssertEquals("Caption Even Longer 1", ToolTipService.GetToolTip(label));

				label.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(12);
				captionRenderer.Captions = new string[] { "Caption" };
				AssertNotEquals("C...", label.Text);
				AssertEquals("Caption", label.Text);

				label.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(30);
				captionRenderer.Captions = new string[] { "Caption 1", "Caption Longer 2", "Caption Even Longer 3" };
				AssertEquals("Caption Even Longer 3", label.Text);

				label.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(12);
				label.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
				captionRenderer.Captions = new string[] { "Caption0" };
				AssertEquals("Caption0", label.Text);
				label.Padding = ControlDpiScalingHelper.NewScaledPadding(6);
				label.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(56);
				Assert(label.Text.Contains("..."));
			}
		}
	}
}
