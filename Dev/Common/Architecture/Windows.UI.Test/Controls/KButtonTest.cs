using System.ComponentModel;
using CargoWise.Common;

namespace CargoWise.Windows.UI.Testing
{
	sealed class KButtonTest : ControlTestCase<KButton>
	{
		#region IVariableLengthCaptionRenderer

		public void TestText_ComesFromCaptionRenderer()
		{
			Form.Controls.Add(Button);
			Form.Show();

			((IVariableLengthCaptionRenderer)Button).Captions = new string[] { "Caption" };
			AssertEquals("Text from caption renderer", "Caption", Button.Text);

			Button.Text = "Text";
			AssertEquals("Text when setting Text explicitly", "Text", Button.Text);

			((IVariableLengthCaptionRenderer)Button).Captions = new string[] { "NewCaption" };
			AssertEquals("Text when setting Text explicitly (renderer doesn't override)", "Text", Button.Text);
		}

		public void TestText_ShouldSerialize()
		{
			using (ComponentExtensions.SwitchToDesignMode())
			{
				Form.Controls.Add(Button);
				Form.Show();

				((IVariableLengthCaptionRenderer)Button).Captions = new string[] { "Caption" };
				AssertEquals("Caption", Button.Text);
				AssertEquals("Don't serialize auto-generated text", false, TypeDescriptor.GetProperties(Button.GetType())["Text"].ShouldSerializeValue(Button));

				Button.Text = "Text";
				AssertEquals("Serialize text that has been explicitly set on the control", true, TypeDescriptor.GetProperties(Button.GetType())["Text"].ShouldSerializeValue(Button));

				Button.Text = "";
				AssertEquals("Don't serialize empty text", false, TypeDescriptor.GetProperties(Button.GetType())["Text"].ShouldSerializeValue(Button));
			}
		}

		#endregion

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
			if (button != null)
			{
				button.Dispose();
			}
		}

		KForm Form
		{
			get { return form ?? (form = new KForm()); }
		}
		KForm form;

		KButton Button
		{
			get { return button ?? (button = new KButton()); }
		}
		KButton button;

		#endregion
	}
}
