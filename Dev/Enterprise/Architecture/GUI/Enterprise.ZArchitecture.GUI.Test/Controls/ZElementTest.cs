using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZElementTest : TestCaseWithDummy
	{
#if !WINZOR
		public void TestNoLeak()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "Roomba";
			using (var form1 = new ZForm(dummy))
			using (var control = new ZElementHost())
			{
				var textBox = new System.Windows.Controls.TextBox();
				textBox.DataContext = dummy;
				textBox.SetBinding(System.Windows.Controls.TextBox.TextProperty, "Z0_VarCharMax");
				control.Child = textBox;
				form1.Controls.Add(control);
				form1.Show();
				Application.DoEvents();

				System.Windows.Input.Keyboard.Focus(textBox);
				Application.DoEvents();
				textBox.Text = "Boop";

				AssertNoExceptionThrown(() => form1.FireSaveButton());
				Application.DoEvents();

				AssertEquals("Boop", dummy.Z0_VarCharMax);
			}
		}
#endif
	}
}
