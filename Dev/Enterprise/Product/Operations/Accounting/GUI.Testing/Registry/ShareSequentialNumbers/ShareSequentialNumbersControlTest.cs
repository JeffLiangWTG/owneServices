using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	class ShareSequentialNumbersControlTest : NUnit.Framework.TestCase
	{
		public void TestConstructor()
		{
			using (ShareSequentialNumbersControl control = new ShareSequentialNumbersControl())
			{
				AssertEquals("OptionGroupBox_ForTestOnly.Text", "Option", control.OptionGroupBox_ForTestOnly.Text);
			}
			using (ShareSequentialNumbersControl control = new ShareSequentialNumbersControl(null))
			{
				AssertEquals("OptionGroupBox_ForTestOnly.Text", "Option", control.OptionGroupBox_ForTestOnly.Text);
			}
			using (ShareSequentialNumbersControl control = new ShareSequentialNumbersControl("x"))
			{
				AssertEquals("OptionGroupBox_ForTestOnly.Text", "x", control.OptionGroupBox_ForTestOnly.Text);
			}
		}

		public void TestAdjustGroupBox()
		{
			using (ZForm form = new ZForm())
			{
				using (ShareSequentialNumbersControl control = new ShareSequentialNumbersControl())
				{
					form.Controls.Add(control);
					form.Show();
					Assert(control.NoShareSequentialNumbers_ForTestOnly.Location.X >
								control.YesShareSequentialNumbers_ForTestOnly.Location.X + control.YesShareSequentialNumbers_ForTestOnly.Width);
					AssertEquals(control.OptionGroupBox_ForTestOnly.Width,
						control.NoShareSequentialNumbers_ForTestOnly.Location.X + control.NoShareSequentialNumbers_ForTestOnly.Width +
						control.YesShareSequentialNumbers_ForTestOnly.Location.X);
				}
			}
		}
	}
}
