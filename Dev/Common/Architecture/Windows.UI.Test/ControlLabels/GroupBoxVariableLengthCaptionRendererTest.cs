using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class GroupBoxVariableLengthCaptionRendererTest : TestCase
	{
		public void TestCaptions()
		{
			using (Form)
			{
				Form.Controls.Add(GroupBox);
				CaptionRenderer.Captions = new string[] { "Caption 1", "Caption Longer 2", "Caption Even Longer 3" };

				// we should show form as captions won't be updated until handle is created
				// this ensures we will not interfere with LabelCaptionRenderer
				Form.Show();

				GroupBox.Width = 110;
				AssertEquals("Caption Longer 2", GroupBox.Text);
			}
		}

		#region Implementation

		GroupBoxVariableLengthCaptionRenderer CaptionRenderer
		{
			get { return captionRenderer ?? (captionRenderer = new GroupBoxVariableLengthCaptionRenderer(GroupBox)); }
		}
		GroupBoxVariableLengthCaptionRenderer captionRenderer;

		KForm Form
		{
			get { return form ?? (form = new KForm()); }
		}
		KForm form;

		GroupBox GroupBox
		{
			get { return groupBox ?? (groupBox = new GroupBox()); }
		}
		GroupBox groupBox;

		#endregion
	}
}
