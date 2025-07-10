using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class EditableControlTest : TestCaseWithDummy
	{
		public void TestHasModifiedDataBindingValue()
		{
			using (var form = new ZTestForm(Dummy))
			{
				form.Show();
				Application.DoEvents();

				form.TextBox.Focus();
				AssertEquals("Not true until the control is edited", false, EditableControl.HasModifiedDataBindingValue(form.TextBox.DataBindings));

				form.TextBox.Text = "NewValue";
				AssertEquals("True after the control is focused and it's value changed", true, EditableControl.HasModifiedDataBindingValue(form.TextBox.DataBindings));
			}
		}

		public void TestDefaultImplementation()
		{
			using (var form = new ZTestForm(Dummy))
			{
				form.Show();
				Application.DoEvents();

				form.TextBox.Focus();
				AssertEquals("Not true until the control is edited", false, EditableControl.Get(form.TextBox).IsEditing);

				form.TextBox.Text = "NewValue";
				AssertEquals("True after the control is focused and it's value changed", true, EditableControl.Get(form.TextBox).IsEditing);

				form.CalcEdit.Focus();
				AssertEquals("False after the control has lost focused", false, EditableControl.Get(form.TextBox).IsEditing);

				var childControl = new TestEditableChildControl();
				childControl.IsEditing = true;
				form.TextBox.Controls.Add(childControl);
				AssertEquals("True if child control is being edited", true, EditableControl.Get(form.TextBox).IsEditing);
			}
		}

		#region Implementation

		class TestEditableChildControl : TextBox, IEditableControl
		{
			public bool IsEditing;

			bool IEditableControl.IsEditing
			{
				get { return IsEditing; }
			}
		}

		#endregion
	}
}
