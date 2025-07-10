using System.Windows.Forms;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class ContainerControlWithNoChildrenTabSkipperTest : TestCase
	{
		public void TestContainerControlWithNoChildrenSkipped()
		{
			TextBox textBox1 = new TextBox();
			TextBox textBox2 = new TextBox();
			textBox1.TabIndex = 1;
			textBox2.TabIndex = 2;
			ContainerControl.TabIndex = 3;
			Form.Controls.Add(textBox1);
			Form.Controls.Add(textBox2);
			Form.Controls.Add(ContainerControl);

			Form.Show();
			Form.SelectNextControl(textBox1, true, true, true, false);
			AssertEquals("Tab to next control", textBox2, Form.ActiveControl);
			Form.SelectNextControl(textBox2, true, true, true, false);
			AssertEquals("Tab past the UserControl, back to the original control", textBox1, Form.ActiveControl);
		}

		public void TestContainerControlWithNoChildrenOnTabPageNotSkipped()
		{
			KTabControl tabControl = new KTabControl();
			TabPage tabPage1 = new TabPage();
			TabPage tabPage2 = new TabPage();
			tabPage2.Controls.Add(ContainerControl);
			tabControl.TabPages.Add(tabPage1);
			tabControl.TabPages.Add(tabPage2);
			tabControl.Dock = DockStyle.Fill;
			Form.Controls.Add(tabControl);

			Form.Show();
			tabControl.SelectedTab = tabPage1;
			tabControl.SelectedTab = tabPage2;
			ContainerControl.Focus();
			AssertEquals(@"
When the container control is on a tab by itself, the ContainerControl should remain selected.
This helps with the tab page tab extender because it means an empty tab page will work correctly.", ContainerControl, Form.ActiveControl);
		}

		public void TestNoNullReferenceExceptionThrown()
		{
			var internalControl1 = new KUserControl();
			internalControl1.Name = "internalControl1";

			var internalControl2 = new KUserControl();
			internalControl2.Name = internalControl2Name;

			var internalControl3 = new KUserControl();
			internalControl3.Name = "internalControl3";

			ContainerControl.Controls.Add(internalControl1);

			var anotherUserControl = new TestContainerControl();
			anotherUserControl.Controls.Add(internalControl2);
			anotherUserControl.Controls.Add(internalControl3);

			using (var form = new TestContainerForm())
			using (var form2 = new KForm())
			{
				form.Controls.Add(ContainerControl);
				form.Controls.Add(anotherUserControl);
				form.Show();

				form2.Show();
				anotherUserControl.ActiveControl = internalControl2;

				form.RefreshForm();

				AssertEquals("The NullReferenceException should not be reported.", "", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}
		const string internalControl2Name = "internalControl2";

		#region Test Classes

		class TestContainerControl : KUserControl
		{
			protected override void SelectCore(bool directed, bool forward)
			{
				base.SelectCore(directed, forward);
				ActiveControl = null;
				UserControlWithNoItemsTabSkipper.AfterSelect(forward);
			}

			ContainerControlWithNoChildrenTabSkipper UserControlWithNoItemsTabSkipper
			{
				get { return userControlWithNoItemsTabSkipper ?? (userControlWithNoItemsTabSkipper = new ContainerControlWithNoChildrenTabSkipper(this)); }
			}
			ContainerControlWithNoChildrenTabSkipper userControlWithNoItemsTabSkipper;

			public void RefreshControl()
			{
				var findControls = Controls.Find(internalControl2Name, false);
				foreach (var control in findControls)
				{
					control.Dispose();
				}
				KUserControl internalControl = new KUserControl();
				internalControl.Name = internalControl2Name;
				Controls.Add(internalControl);
			}
		}

		class TestContainerForm : KForm
		{
			public void RefreshForm()
			{
				foreach (var control in Controls)
				{
					var containerControl = control as TestContainerControl;
					containerControl?.RefreshControl();
				}
			}
		}

		#endregion

		#region Implementation

		KForm Form
		{
			get { return form ?? (form = new KForm()); }
		}
		KForm form;

		TestContainerControl ContainerControl
		{
			get { return userControl ?? (userControl = new TestContainerControl()); }
		}
		TestContainerControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			if (userControl != null)
			{
				userControl.Dispose();
			}
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
