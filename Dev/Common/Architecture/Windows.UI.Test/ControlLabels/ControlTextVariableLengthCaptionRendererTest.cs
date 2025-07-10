using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class ControlTextVariableLengthCaptionRendererTest : TestCase
	{
		public void TestCaptions()
		{
			using (KForm form = new KForm())
			{
				GroupBox groupBox = new GroupBox();

				form.Controls.Add(groupBox);
				GroupBoxVariableLengthCaptionRenderer captionRenderer = new GroupBoxVariableLengthCaptionRenderer(groupBox);
				captionRenderer.Captions = new string[] { "Caption 1", "Caption Longer 2", "Caption Even Longer 3" };

				// we should show form as captions won't be updated until handle is created
				// this ensures we will not interfere with LabelCaptionRenderer
				form.Show();

				groupBox.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
				AssertEquals("Caption Longer 2", groupBox.Text);
			}
		}

		public void TestUpdateTextWhenControlHasNoText()
		{
			using (var form = new KForm())
			{
				var groupBox = new GroupBox();

				form.Controls.Add(groupBox);
				var captionRenderer = new ControlTextVariableLengthCaptionRenderer(groupBox);
				captionRenderer.Captions = new string[] { "Caption" };

				form.Show();

				groupBox.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
				AssertEquals("Should update Text", "Caption", groupBox.Text);

				groupBox.Text = "";
				AssertEquals("Text should be empty", "", groupBox.Text);

				captionRenderer.IsCaptionOverridden = false;
				captionRenderer.Captions = new string[] { "Caption" };
				AssertEquals("Text should also be updated if it's empty", "Caption", groupBox.Text);
			}
		}
	}
}
