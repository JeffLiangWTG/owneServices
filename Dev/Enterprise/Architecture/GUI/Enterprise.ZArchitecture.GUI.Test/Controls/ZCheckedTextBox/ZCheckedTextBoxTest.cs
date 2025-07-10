using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZCheckedTextBoxTest : ZControlBaseTestCase<ZCheckedTextBox>
	{
		public void TestSize()
		{
			using (var box = new ZCheckedTextBox())
			{
				box.Width = 200;
				box.Gap = 5;
				AssertEquals(box.Width - box.CheckBox.Width - box.Gap, box.TextBox.Width);
			}
		}

		public void TestCheckBoxDoesNotExpandVertically_WhenHasLongCaption()
		{
			using (var box = new ZCheckedTextBox())
			{
				box.Width = 100;
				box.CheckBox.Text = "Very very very very very very very very very very very very long caption";
				AssertEquals(box.TextBox.Height, box.CheckBox.Height);
			}
		}

		public void TestDefaultValues()
		{
			using (var box = new ZCheckedTextBox())
			{
				AssertEquals("Default value for Checked should be...", false, box.Checked);
				AssertEquals("Default value for Text should be...", string.Empty, box.Text);
				AssertEquals("Default value for CharacterCasing should be...", CharacterCasing.Upper, box.CharacterCasing);
				AssertEquals("Default value for MaxLength should be...", 32767, box.MaxLength);
			}
		}

		public void TestTextChanged()
		{
			using (var box = new ZCheckedTextBox())
			{
				AssertNoExceptionThrown("Text assignment should be safe even when event handlers are not assigned", () => { box.Text = "Some text"; });

				var textChangedEventFired = false;
				box.TextChanged += delegate(object sender, System.EventArgs e)
				{
					AssertEquals(box, sender);
					textChangedEventFired = true;
				};
				box.Text = "Another text";
				AssertEquals("TextChanged event should be fired when the handler is assigned", true, textChangedEventFired);

				textChangedEventFired = false;
				box.TextBox.Text = "Some other text";
				AssertEquals("TextChanged event should be fired when the handler is assigned", true, textChangedEventFired);
			}
		}

		public void TestCheckedChanged()
		{
			using (var box = new ZCheckedTextBox())
			{
				AssertNoExceptionThrown("Checked assignment should be safe even when event handlers are not assigned", () => { box.Checked = false; });

				var checkedChangedEventFired = false;
				box.CheckedChanged += delegate(object sender, System.EventArgs e)
				{
					AssertEquals(box, sender);
					checkedChangedEventFired = true;
				};
				box.Checked = true;
				AssertEquals("CheckedChanged event should be fired when the handler is assigned", true, checkedChangedEventFired);

				checkedChangedEventFired = false;
				box.CheckBox.Checked = false;
				AssertEquals("CheckedChanged event should be fired when the handler is assigned", true, checkedChangedEventFired);
			}
		}

		public void TestCheckedUpdatesTextBoxEnabled()
		{
			using (var box = new ZCheckedTextBox())
			{
				box.Checked = true;
				AssertEquals("TextBox should be enabled when the control is checked", true, box.TextBox.Enabled);

				box.Checked = false;
				AssertEquals("TextBox should be disabled when the control is not checked", false, box.TextBox.Enabled);
			}
		}

		public void TestMaxLength()
		{
			using (var form = new ZForm())
			{
				var box = new ZCheckedTextBox();
				box.CharacterCasing = CharacterCasing.Normal;
				form.Controls.Add(box);
				form.Show();

				box.MaxLength = 200;
				box.Text = "Some Text";
				AssertEquals("Text should not be truncated when it is short enough", "Some Text", box.Text);

				box.Text = "Some Text";
				box.MaxLength = 200;
				AssertEquals("Text should not be truncated when it is short enough", "Some Text", box.Text);

				box.MaxLength = "Some Text".Length;
				box.Text = "Some Text";
				AssertEquals("Text should not be truncated when it is short enough", "Some Text", box.Text);

				box.Text = "Some Text";
				box.MaxLength = "Some Text".Length;
				AssertEquals("Text should not be truncated when it is short enough", "Some Text", box.Text);

				box.MaxLength = 4;
				box.Text = "Some Text";
				AssertEquals("Text should be truncated when it is too long", "Some", box.Text);

				box.Text = "Some Text";
				box.MaxLength = 4;
				AssertEquals("Text should be truncated when it is too long", "Some", box.Text);

				box.MaxLength = 0;
				box.Text = "Some Text";
				AssertEquals("Text should not be truncated when text length is not limited", "Some Text", box.Text);

				box.Text = "Some Text";
				box.MaxLength = 0;
				AssertEquals("Text should not be truncated when text length is not limited", "Some Text", box.Text);
			}
		}

		public void TestTextAndCheckedDoubleBinding()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = string.Empty;
			dummy.Z0_Bool = false;
			using (var form = new ZForm(dummy))
			{
				var box = new ZCheckedTextBox();
				box.BindTo = DummyBizoSchema.Z0_Description.Name;
				box.BindToForChecked = DummyBizoSchema.Z0_Bool.Name;

				var controlTextChangedFired = false;
				var controlCheckedChangedFired = false;
				box.TextChanged += delegate
				{ controlTextChangedFired = true; };
				box.CheckedChanged += delegate
				{ controlCheckedChangedFired = true; };

				form.Controls.Add(box);
				form.Show();

				//CASE A
				controlTextChangedFired = false;
				controlCheckedChangedFired = false;
				dummy.Z0_Description = "bound";
				AssertEquals("CASE A: TextChanged should be fired", true, controlTextChangedFired);
				AssertEquals("CASE A: Text should reflect the property bound to Text", "BOUND", box.Text);

				//CASE B1
				controlTextChangedFired = false;
				controlCheckedChangedFired = false;
				dummy.Z0_Bool = true;
				AssertEquals("CASE B1: CheckedChanged should be fired", true, controlCheckedChangedFired);
				AssertEquals("CASE B1: Checked should reflect the property bound to Checked", true, box.Checked);
				AssertEquals("CASE B1: TextBox should be enabled when checked", true, box.TextBox.Enabled);

				//CASE B2
				controlTextChangedFired = false;
				controlCheckedChangedFired = false;
				dummy.Z0_Bool = false;
				AssertEquals("CASE B2: CheckedChanged should be fired", true, controlCheckedChangedFired);
				AssertEquals("CASE B2: Checked should reflect the property bound to Checked", false, box.Checked);
				AssertEquals("CASE B2: TextBox should be disabled when unchecked", false, box.TextBox.Enabled);

				var bizoTextChanged = false;
				var bizoCheckedChanged = false;
				dummy.Z0_DescriptionInfo.ValueChanged += delegate
				{ bizoTextChanged = true; };
				dummy.Z0_BoolInfo.ValueChanged += delegate
				{ bizoCheckedChanged = true; };

				//CASE C
				bizoTextChanged = false;
				bizoCheckedChanged = false;
				box.Text = "Some new text";
				AssertEquals("CASE C: Dummy TextChanged event should be fired", true, bizoTextChanged);
				AssertEquals("CASE C: Text should reflect the property bound to Text", "SOME NEW TEXT", dummy.Z0_Description);
				AssertEquals("CASE C: Dummy CheckedChanged event should not be fired", false, bizoCheckedChanged);

				//CASE D
				box.Checked = false;
				bizoTextChanged = false;
				bizoCheckedChanged = false;
				box.Checked = true;
				AssertEquals("CASE D: Dummy CheckedChanged event should be fired", true, bizoCheckedChanged);
				AssertEquals("CASE D: Checked should reflect the property bound to Checked", true, dummy.Z0_Bool);
				AssertEquals("CASE D: Dummy TextChanged event should not be fired", false, bizoTextChanged);
			}
		}

		public void TestCharacterCasing()
		{
			using (var form = new ZForm())
			{
				var box = new ZCheckedTextBox();
				form.Controls.Add(box);
				form.Show();

				box.CharacterCasing = CharacterCasing.Normal;
				box.Text = "Some Text";
				AssertEquals("Some Text", box.Text);

				box.CharacterCasing = CharacterCasing.Upper;
				box.Text = "Some Text";
				AssertEquals("SOME TEXT", box.Text);

				box.CharacterCasing = CharacterCasing.Lower;
				box.Text = "Some Text";
				AssertEquals("some text", box.Text);
			}
		}

		public void TestNotifications()
		{
			var dummy = Factory.New<DummyWithValidationErrors>();
			using (var form = new ZForm(dummy))
			{
				var box = new ZCheckedTextBox();
				box.BindTo = nameof(dummy.PropertyWithErrors);
				form.Controls.Add(box);
				form.Show();

				form.Validate();
				AssertEquals("TextBox should render a notification error", 1, box.TextBox.GetExtension<NotificationExtension>().Notifications.Count());
			}
		}

		#region Implementation

		protected override void BindControl()
		{
			base.BindControl();
			Control.BindTo = DummyBizoSchema.Z0_Description.Name;
			DataBoundControl.Get(Control).SetDataBinding(Dummy, Control.BindTo);
		}

		#endregion
	}
}
