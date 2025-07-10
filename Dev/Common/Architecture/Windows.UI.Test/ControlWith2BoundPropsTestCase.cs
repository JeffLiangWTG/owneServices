using System.ComponentModel;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	sealed class ControlWith2BoundPropsTestCase : TestCase
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void TestDataBindingWith2BoundProps()
		{
			TestControlWith2BoundProperties control;
			Control someOtherControl;
			using (TestDataForm form = NewBoundDataForm(out control, out someOtherControl))
			{
				someOtherControl.Focus();
				control.CurrentDataItem.Property1 = "oldtext1";
				control.CurrentDataItem.Property2 = "oldtext2";

				control.Focus();
				control.Text1 = "newtext1";
				control.Text2 = "newtext2";

				someOtherControl.Focus();
				AssertEquals("newtext1", control.CurrentDataItem.Property1);
				AssertEquals("newtext2", control.CurrentDataItem.Property2);

				PropertyDescriptor property = TypeDescriptor.GetProperties(control.CurrentDataItem)["Property1"];
				property.SetValue(control.CurrentDataItem, "fireevent1");
				AssertEquals(
					"Event should still be picked up by BindToObject",
					"fireevent1", control.Text1);

				control.SetDataBinding(null, "");
				AssertNull("Unhook data bindings on user control", control.DataBindings["Text1"]);
				AssertNull("Unhook data bindings on user control", control.DataBindings["Text2"]);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void TestBindingResumedAfterValidating()
		{
			TestControlWith2BoundProperties ctrl;
			Control someOtherControl;
			using (TestDataForm f = NewBoundDataForm(out ctrl, out someOtherControl))
			{
				someOtherControl.Focus();
				ctrl.CurrentDataItem.Property1 = "oldtext1";
				ctrl.Focus();
				ctrl.Text1 = "newtext1";

				someOtherControl.Focus();
				PropertyDescriptor property = TypeDescriptor.GetProperties(ctrl.CurrentDataItem)["Property1"];
				property.SetValue(ctrl.CurrentDataItem, "fireevent1");
				AssertEquals("Event should still be picked up by BindToObject", "fireevent1", ctrl.Text1);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void TestBindingResumedAfterValidatingCancelled()
		{
			TestControlWith2BoundProperties control;
			Control someOtherControl;
			using (TestDataForm f = NewBoundDataForm(out control, out someOtherControl))
			{
				control.Validating += new CancelEventHandler(Control_ValidatingCancel);

				someOtherControl.Focus();
				control.CurrentDataItem.Property1 = "oldtext1";
				control.Focus();
				control.Text1 = "newtext1";

				someOtherControl.Focus();
				PropertyDescriptor property = TypeDescriptor.GetProperties(control.CurrentDataItem)["Property1"];
				property.SetValue(control.CurrentDataItem, "fireevent1");
				AssertEquals(
					"Event should still be picked up by BindToObject",
					"fireevent1", control.Text1);
			}
		}

		void Control_ValidatingCancel(object sender, CancelEventArgs e)
		{ e.Cancel = true; }

		#region Implementation

		TestDataForm NewBoundDataForm(out TestControlWith2BoundProperties control, out Control someOtherControl)
		{
			TestDataForm form = new TestDataForm();
			form.SetDataBinding(new TestDataSource(), "");
			control = new TestControlWith2BoundProperties();
			someOtherControl = new TextBox();
			someOtherControl.Width = 1;
			someOtherControl.Height = 1;
			form.Controls.Add(control);
			form.Controls.Add(someOtherControl);

			form.BindingSource.SetBindingMember(control, ".");
			form.Show();

			AssertEquals(
				"Hook data bindings on user control",
				"Property1", control.DataBindings["Text1"].BindingMemberInfo.BindingMember);
			AssertEquals(
				"Hook data bindings on user control",
				"Property2", control.DataBindings["Text2"].BindingMemberInfo.BindingMember);

			return form;
		}

		#endregion

		#region TestDataForm and TestDataSource

		public class TestDataSource : ComponentModel.Testing.KComponentWithPropertyChange
		{
			public TestDataSource()
			{
				Property1 = "";
				Property2 = "";
			}

			public string Property1
			{
				get { return property1; }
				set
				{
					if (property1 != value)
					{
						property1 = value;
						FirePropertyChanged(nameof(Property1));
					}
				}
			}
			string property1;

			public string Property2
			{
				get { return property2; }
				set
				{
					if (property2 != value)
					{
						property2 = value;
						FirePropertyChanged(nameof(Property2));
					}
				}
			}
			string property2;

			public TestDataSource Self
			{
				get { return this; }
			}
		}

		internal class TestDataForm : KForm
		{
			public TestDataForm()
			{
				BindingSource.DataSourceType = typeof(object);
				BindingSource.SupportMultipleTwoWayBoundPropertiesOnOneControl = true;
			}

			public new KBindingSource BindingSource
			{ get { return base.BindingSource; } }
		}

		#endregion
	}
}
