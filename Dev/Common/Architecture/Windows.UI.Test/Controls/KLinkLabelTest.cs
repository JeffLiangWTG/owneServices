using System.ComponentModel;
using CargoWise.Common;

namespace CargoWise.Windows.UI.Testing
{
	sealed class KLinkLabelTest : ControlTestCase<KLabel>
	{
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

		public void TestClick()
		{
			var wasHit = false;
			using (var lnk = new KLinkLabel())
			{
				lnk.Click += (o, e) => wasHit = true;

				lnk.PerformClick_ForTest();
				Assert("Should have fired the event", wasHit);
			}
		}

		#region Implementation

		KForm Form
		{
			get { return form ?? (form = new KForm()); }
		}
		KForm form;

		KLinkLabel Label
		{
			get { return label ?? (label = new KLinkLabel()); }
		}
		KLinkLabel label;

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
