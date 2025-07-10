using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class ControlCustomVariableLengthCaptionRendererTest : TestCase
	{
		public void TestUpdateCaptionsWhenControlHasNoText()
		{
			Form.Controls.Add(CaptionRenderedGroupBox);

			ControlCaptionRenderer.Control = CaptionRenderedGroupBox;
			ControlCaptionRenderer.Captions = new string[] { "Caption" };
			AssertEquals("Caption not rendered until control is visible", "", CaptionRenderedGroupBox.Text);

			Form.Show();
			AssertEquals("Caption", CaptionRenderedGroupBox.Text);
			AssertEquals("Caption rendered when control is visible", "Caption", CaptionRenderedGroupBox.Text);

			CaptionRenderedGroupBox.Text = "";
			AssertEquals("Text should be empty", "", CaptionRenderedGroupBox.Text);

			ControlCaptionRenderer.Renderer.IsCaptionOverridden = false;
			ControlCaptionRenderer.Captions = new string[] { "Caption" };
			AssertEquals("Text should also be updated if it's empty", "Caption", CaptionRenderedGroupBox.Text);
		}

		public void TestCaptionsRenderedOnlyWhenControlVisible()
		{
			Form.Controls.Add(CaptionRenderedGroupBox);

			ControlCaptionRenderer.Control = CaptionRenderedGroupBox;
			ControlCaptionRenderer.Captions = new string[] { "Caption" };
			AssertEquals("Caption not rendered until control is visible", "", CaptionRenderedGroupBox.Text);

			Form.Show();
			AssertEquals("Caption rendered when control is visible", "Caption", CaptionRenderedGroupBox.Text);

			ControlCaptionRenderer.Captions = new string[] { "UpdatedCaption" };
			AssertEquals("Caption updated when control is visible", "UpdatedCaption", CaptionRenderedGroupBox.Text);
		}

		public void TestCaptionsDelegate()
		{
			CaptionGetter = delegate
			{ return new string[] { "Caption" }; };

			Form.Controls.Add(CaptionRenderedGroupBox);
			Form.Show();
			AssertEquals("Caption from delegate rendered", "Caption", CaptionRenderedGroupBox.Text);
		}

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			if (captionRenderedGroupBox != null)
			{
				captionRenderedGroupBox.Dispose();
			}
			if (form != null)
			{
				form.Dispose();
			}
		}

		GetCaptionsDelegate CaptionGetter { get; set; }

		KForm Form
		{
			get { return form ?? (form = new KForm()); }
		}
		KForm form;

		GroupBox CaptionRenderedGroupBox
		{
			get
			{
				if (captionRenderedGroupBox == null)
				{
					captionRenderedGroupBox = new GroupBox();
					captionRenderedGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(200, 100);
					ControlCaptionRenderer.Control = captionRenderedGroupBox;
					ControlCaptionRenderer.Renderer = GroupBoxCaptionRenderer;
				}
				return captionRenderedGroupBox;
			}
		}
		GroupBox captionRenderedGroupBox;

		ControlCustomVariableLengthCaptionRenderer ControlCaptionRenderer
		{
			get { return controlCaptionRenderer ?? (controlCaptionRenderer = new ControlCustomVariableLengthCaptionRenderer(CaptionGetter)); }
		}
		ControlCustomVariableLengthCaptionRenderer controlCaptionRenderer;

		GroupBoxVariableLengthCaptionRenderer GroupBoxCaptionRenderer
		{
			get { return groupBoxCaptionRenderer ?? (groupBoxCaptionRenderer = new GroupBoxVariableLengthCaptionRenderer(CaptionRenderedGroupBox)); }
		}
		GroupBoxVariableLengthCaptionRenderer groupBoxCaptionRenderer;

		#endregion
	}
}
