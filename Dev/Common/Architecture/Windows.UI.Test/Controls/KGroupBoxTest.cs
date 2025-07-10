using System.ComponentModel;
using CargoWise.Common;

namespace CargoWise.Windows.UI.Testing
{
	sealed class KGroupBoxTest : ControlTestCase<KGroupBox>
	{
		#region IVariableLengthCaptionRenderer

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "DontUseApplicationDoEventsRule", Justification = "Testing")]
		public void TestText_ComesFromCaptionRenderer()
		{
			Form.Controls.Add(GroupBox);
			Form.Show();
			System.Windows.Forms.Application.DoEvents();

			((IVariableLengthCaptionRenderer)GroupBox).Captions = new string[] { "Caption" };
			AssertEquals("Text from caption renderer", "Caption", GroupBox.Text);

			GroupBox.Text = "Text";
			AssertEquals("Text when setting Text explicitly", "Text", GroupBox.Text);

			((IVariableLengthCaptionRenderer)GroupBox).Captions = new string[] { "NewCaption" };
			AssertEquals("Text when setting Text explicitly (renderer doesn't override)", "Text", GroupBox.Text);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "DontUseApplicationDoEventsRule", Justification = "Testing")]
		public void TestText_ShouldSerialize()
		{
			using (ComponentExtensions.SwitchToDesignMode())
			{
				Form.Controls.Add(GroupBox);
				Form.Show();
				System.Windows.Forms.Application.DoEvents();

				((IVariableLengthCaptionRenderer)GroupBox).Captions = new string[] { "Caption" };
				AssertEquals("Caption", GroupBox.Text);
				AssertEquals("Don't serialize auto-generated text", false, TypeDescriptor.GetProperties(GroupBox.GetType())["Text"].ShouldSerializeValue(GroupBox));

				GroupBox.Text = "Text";
				AssertEquals("Serialize text that has been explicitly set on the control", true, TypeDescriptor.GetProperties(GroupBox.GetType())["Text"].ShouldSerializeValue(GroupBox));

				GroupBox.Text = "";
				AssertEquals("Don't serialize empty text", false, TypeDescriptor.GetProperties(GroupBox.GetType())["Text"].ShouldSerializeValue(GroupBox));
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
			if (groupBox != null)
			{
				groupBox.Dispose();
			}
		}

		KForm Form
		{
			get { return form ?? (form = new KForm()); }
		}
		KForm form;

		KGroupBox GroupBox
		{
			get { return groupBox ?? (groupBox = new KGroupBox()); }
		}
		KGroupBox groupBox;

		#endregion
	}
}
