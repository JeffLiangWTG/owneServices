using System;
using System.ComponentModel;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	sealed class BoundControlWithChildBoundControlSameDataSourceTestCase : TestCase
	{
		public void TestBinding()
		{
			TestDataSource data = new TestDataSource();
			using (KForm form = new KForm())
			{
				BoundControlWithChildBoundControlSameDataSourceTestCaseControl control = new BoundControlWithChildBoundControlSameDataSourceTestCaseControl();
				form.Controls.Add(control);
				control.SetDataBinding(data, "");
				form.Show();

				TextBox dummyControl = new TextBox();
				form.Controls.Add(dummyControl);

				dummyControl.Focus();
				control.txtText1.Focus();
				control.txtText1.Text = "x";
				control.txtText2.Focus();
				control.txtText2.Text = "y";

				dummyControl.Focus();
				AssertEquals("x", data.Property1);
				AssertEquals("y", data.Property2);
			}
		}

		#region TestDataSource

		public class TestDataSource : ComponentModel.Testing.KComponentWithPropertyChange
		{
			public string Property1
			{
				get { return property1; }
				set
				{
					if (property1 != value)
					{
						property1 = value;
						if (Property1Changed != null)
						{
							Property1Changed(this, EventArgs.Empty);
						}
					}
				}
			}
			string property1;
			public event EventHandler Property1Changed;

			public string Property2
			{
				get { return property2; }
				set
				{
					if (property2 != value)
					{
						property2 = value;
						if (Property2Changed != null)
						{
							Property2Changed(this, EventArgs.Empty);
						}
					}
				}
			}
			string property2;
			public event EventHandler Property2Changed;
		}

		#endregion
	}
}
