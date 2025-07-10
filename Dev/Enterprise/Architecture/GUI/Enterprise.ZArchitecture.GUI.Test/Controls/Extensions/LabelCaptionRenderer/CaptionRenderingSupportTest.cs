using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class CaptionRenderingSupportTest : TestCase
	{
		public void TestIsCaptionRenderingEnabled_ForForm()
		{
			Form.CaptionRenderingEnabled = true;
			AssertEquals("Caption rendering enabled for form caption", true, CaptionRenderingSupport.IsCaptionRenderingEnabled(Form));
			Form.CaptionRenderingEnabled = false;
			AssertEquals("Caption rendering not enabled for form caption", false, CaptionRenderingSupport.IsCaptionRenderingEnabled(Form));
		}

		public void TestIsCaptionRenderingEnabled_ForControl()
		{
			Form.Controls.Add(UserControl);
			UserControl.CaptionRenderingEnabled = true;
			AssertEquals("Caption rendering enabled only affected by parent control", false, CaptionRenderingSupport.IsCaptionRenderingEnabled(UserControl));

			Form.CaptionRenderingEnabled = true;
			AssertEquals("Caption rendering enabled for form caption", true, CaptionRenderingSupport.IsCaptionRenderingEnabled(UserControl));
			Form.CaptionRenderingEnabled = false;
			AssertEquals("Caption rendering not enabled for form caption", false, CaptionRenderingSupport.IsCaptionRenderingEnabled(UserControl));
		}

		#region Implementation

		ZForm Form
		{
			get { return form ?? (form = new ZForm()); }
		}
		ZForm form;

		ZUserControl UserControl
		{
			get { return userControl ?? (userControl = new ZUserControl()); }
		}
		ZUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
			if (userControl != null)
			{
				userControl.Dispose();
			}
		}

		#endregion
	}
}
