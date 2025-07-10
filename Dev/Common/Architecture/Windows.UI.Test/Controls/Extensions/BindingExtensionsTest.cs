using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class BindingExtensionsTest : TestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1104:DoNotUseSystemWindowsTabControl", Justification = "Testing")]
		public void TestForceBinding()
		{
			using (Form form = new Form())
			{
				TabControl tabControl = new TabControl();
				TabPage tabPage1 = new TabPage();
				tabPage1.Text = "TabPage1";
				tabControl.TabPages.Add(tabPage1);
				TabPage tabPage2 = new TabPage();
				tabPage2.Text = "TabPage2";
				tabControl.TabPages.Add(tabPage2);

				TextBox textBox = new TextBox();
				tabPage2.Controls.Add(textBox);
				form.Controls.Add(tabControl);

				form.Show();

				Binding binding = new Binding("Text", this, "StringValue");
				textBox.DataBindings.Add(binding);
				binding.ForceBinding(textBox);
				AssertEquals("Control not visible for the test", false, textBox.Visible);
				AssertEquals("Control handle not created", false, textBox.IsHandleCreated);
				AssertEquals("Binding should be happening even though the control is not visible", true, binding.IsBinding);
			}
		}

		public string StringValue
		{
			get { return ""; }
			set { }
		}
	}
}
