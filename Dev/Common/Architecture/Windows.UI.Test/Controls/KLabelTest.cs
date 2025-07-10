using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;

namespace CargoWise.Windows.UI.Testing
{
	sealed class KLabelTest : ControlTestCase<KLabel>
	{
		public void TestUseMnemonic()
		{
			Assert(!Label.UseMnemonic);
		}

		#region AutoSize=true while setting Text in HandleCreated problem fix

		public void TestSettingTextInHandleCreatedWhileAutoSizeTrue()
		{
			Label.AutoSize = true;
			Label.HandleCreated += delegate
			{ Label.Text = "12345678901234567890"; };
			Form.Controls.Add(Label);

			var expectedWidth = TextRenderer.MeasureText("12345678901234567890", Label.Font).Width;
			int tolerance = 15; // Allow minor width variations due to rendering differences

			Form.Show();
			Assert(Math.Abs(Label.Width - expectedWidth) <= tolerance || (ControlDpiScalingHelper.DpiX > ControlDpiScalingHelper.BaseDpiX && Label.Width > expectedWidth));
		}

		#endregion

		#region IVariableLengthCaptionRenderer

		public void TestText_ComesFromCaptionRenderer()
		{
			Form.Controls.Add(Label);
			Form.Show();

			((IVariableLengthCaptionRenderer)Label).Captions = new string[] { "Caption" };
			AssertEquals("Text from caption renderer", "Caption", Label.Text);

			Label.Text = "Text";
			AssertEquals("Text when setting Text explicitly", "Text", Label.Text);

			((IVariableLengthCaptionRenderer)Label).Captions = new string[] { "NewCaption" };
			AssertEquals("Text when setting Text explicitly (renderer doesn't override)", "Text", Label.Text);
		}

		public void TestText_ShouldSerialize()
		{
			using (ComponentExtensions.SwitchToDesignMode())
			{
				Form.Controls.Add(Label);
				Form.Show();

				((IVariableLengthCaptionRenderer)Label).Captions = new string[] { "Caption" };
				AssertEquals("Caption", Label.Text);
				AssertEquals("Don't serialize auto-generated text", false, TypeDescriptor.GetProperties(Label.GetType())["Text"].ShouldSerializeValue(Label));

				Label.Text = "Text";
				AssertEquals("Serialize text that has been explicitly set on the control", true, TypeDescriptor.GetProperties(Label.GetType())["Text"].ShouldSerializeValue(Label));

				Label.Text = "";
				AssertEquals("Don't serialize empty text", false, TypeDescriptor.GetProperties(Label.GetType())["Text"].ShouldSerializeValue(Label));
			}
		}

		#endregion

		#region Implementation

		KForm Form
		{
			get { return form ?? (form = new KForm()); }
		}
		KForm form;

		KLabel Label
		{
			get { return label ?? (label = new KLabel()); }
		}
		KLabel label;

		protected override void TearDown()
		{
			base.TearDown();
			if (label != null)
			{
				label.Dispose();
			}
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
