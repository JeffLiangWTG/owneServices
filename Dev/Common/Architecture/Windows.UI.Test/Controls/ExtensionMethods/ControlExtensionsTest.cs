using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using Application = System.Windows.Forms.Application;

namespace CargoWise.Windows.UI.Testing
{
	sealed class ControlExtensionsTest : TestCase
	{
		public void TestSuspendDrawing_ResumeDrawing_DoesntCreateHandle()
		{
			using (var form = new Form())
			{
				Assert(!form.IsHandleCreated);
				Assert(!form.Created);
				form.SuspendDrawing();
				Assert(!form.IsHandleCreated);
				Assert(!form.Created);
				form.ResumeDrawing();
				Assert(!form.IsHandleCreated);
				Assert(!form.Created);
				form.Show();
				Assert(form.IsHandleCreated);
				Assert(form.Created);
			}
		}

		public void TestRemoveAndDisposeAll()
		{
			using (var form = new Form())
			{
				form.Show();
				var label1 = new KLabel();
				var label2 = new KLabel();
				form.Controls.AddRange(new[] { label1, label2 });

				form.Controls.RemoveAndDisposeAll();
				AssertEquals(0, form.Controls.Count);
				Assert(label1.IsDisposed);
				Assert(label2.IsDisposed);
			}
		}

		#region TestGetFrontMostActiveControlProvider

		public void TestGetFrontMostActiveControlProvider()
		{
			KUserControl containerControl1 = new KUserControl();
			KUserControl containerControl2 = new KUserControl();
			TestControl testControl = new TestControl();
			containerControl1.Controls.Add(containerControl2);
			containerControl2.Controls.Add(testControl);
			testControl.Controls.Add(Control);
			Form.Controls.Add(containerControl1);

			Form.Show();
			Application.DoEvents();

			Control.Focus();
			AssertEquals("Should use TestFrontMostControlProvider to return testControl, not Control", testControl, Form.GetFrontMostActiveControl());
		}

		[FrontMostControlProvider(typeof(TestFrontMostControlProvider))]
		public class TestControl : KUserControl
		{
		}

		public class TestFrontMostControlProvider : DefaultFrontMostControlProvider
		{
			public override Control GetFrontMostControl(Control control)
			{
				return control;
			}
		}

		#endregion

		#region TestGetFrontMostActiveControl

		public void TestGetFrontMostActiveControl()
		{
			KUserControl containerControl1 = new KUserControl();
			KUserControl containerControl2 = new KUserControl();
			containerControl1.Controls.Add(containerControl2);
			containerControl2.Controls.Add(Control);
			Form.Controls.Add(containerControl1);

			Form.Show();
			Application.DoEvents();

			Control.Focus();
			AssertEquals("Front most active control should be returned", Control, containerControl1.GetFrontMostActiveControl());
		}

		public void TestGetFrontMostActiveControl_WithTabControl()
		{
			KTabControl tabControl = new KTabControl();
			TabPage tabPage = new TabPage("TabPage");
			tabControl.TabPages.Add(tabPage);

			tabPage.Controls.Add(Control);
			Form.Controls.Add(tabControl);
			Form.Show();
			Application.DoEvents();

			Control.Focus();
			AssertEquals("Front most active control should be returned", Control, Form.GetFrontMostActiveControl());
		}

		public void TestGetFrontMostActiveControl_WithNoContainerControl()
		{
			using (TextBox textBox = new TextBox())
			{
				AssertEquals(textBox, textBox.GetFrontMostActiveControl());
			}
		}

		#endregion

		public void TestContainsIncludingChildren()
		{
			KUserControl containerControl1 = new KUserControl();
			KUserControl containerControl2 = new KUserControl();
			containerControl1.Controls.Add(containerControl2);
			containerControl2.Controls.Add(Control);
			Form.Controls.Add(containerControl1);
			AssertEquals("Control is a child control", true, Form.ContainsIncludingChildren(Control));
			AssertEquals("Control is not a child control", false, Control.ContainsIncludingChildren(containerControl1));
		}

		public void TestGetTopLevelNonParentedControl()
		{
			using (KUserControl containerControl = new KUserControl())
			{
				containerControl.Controls.Add(Control);
				AssertEquals("Top level control of form is form", Form, Form.GetTopLevelNonParentedControl());
				AssertEquals("Top level of control on non-parented container control is container control", containerControl, Control.GetTopLevelNonParentedControl());

				Form.Controls.Add(Control);
				AssertEquals("Top level of control on container control on form is form", Form, Control.GetTopLevelNonParentedControl());
			}
		}

		public void TestIsDesignMode()
		{
			using (Control parent = new Control())
			using (Control child = new Control())
			{
				child.Parent = parent;
				parent.Site = new MockSite();
				AssertEquals("Child control is in design mode because parent control is in design mode", true, child.IsDesignMode());
			}
		}

		public void TestIsLayoutSuspended()
		{
			using (var form = new Form())
			{
				AssertEquals(false, form.IsLayoutSuspended());
				form.SuspendLayout();
				AssertEquals(true, form.IsLayoutSuspended());
				form.ResumeLayout();
				AssertEquals(false, form.IsLayoutSuspended());
			}
		}

		#region GetReadOnly / SetReadOnly

		public void TestReadOnly_WithoutReadOnlyProperty()
		{
			ControlWithoutReadOnlyProperty.SetReadOnly(true);
			AssertEquals(true, ControlWithoutReadOnlyProperty.GetReadOnly());
			AssertEquals(false, ControlWithoutReadOnlyProperty.Enabled);

			ControlWithoutReadOnlyProperty.SetReadOnly(false);
			AssertEquals(false, ControlWithoutReadOnlyProperty.GetReadOnly());
			AssertEquals(true, ControlWithoutReadOnlyProperty.Enabled);
		}

		public void TestReadOnly_WithReadOnlyProperty()
		{
			ControlWithReadOnlyProperty.SetReadOnly(true);
			AssertEquals(true, ControlWithReadOnlyProperty.GetReadOnly());
			AssertEquals(true, ControlWithReadOnlyProperty.ReadOnly);

			ControlWithReadOnlyProperty.SetReadOnly(false);
			AssertEquals(false, ControlWithReadOnlyProperty.GetReadOnly());
			AssertEquals(false, ControlWithReadOnlyProperty.ReadOnly);
		}

		public void TestReadOnly_WithExplicitMetaData()
		{
			ControlWithExplicitMetaData.SetReadOnly(true);
			AssertEquals(true, ControlWithExplicitMetaData.GetReadOnly());
			AssertEquals(true, ControlWithExplicitMetaData.ReadOnly);

			ControlWithExplicitMetaData.SetReadOnly(false);
			AssertEquals(false, ControlWithExplicitMetaData.GetReadOnly());
			AssertEquals(false, ControlWithExplicitMetaData.ReadOnly);
		}

		#endregion

		#region SelectNextControlNonTabStopNonReadOnly

		public void TestSelectNextControlNonTabStopNonReadOnly()
		{
			Form.Show();
			Form.Controls.Add(UserControl);
			UserControl.Controls.Add(TextBox1);
			UserControl.Controls.Add(TextBox2);
			UserControl.Controls.Add(TextBox3);
			Application.DoEvents();

			TextBox1.TabIndex = 1;
			TextBox2.TabIndex = 2;
			TextBox3.TabIndex = 3;

			TextBox2.SetReadOnly(true);
			UserControl.SelectNextControlNonTabStopNonReadOnly(TextBox1, true, true, false);
			AssertEquals("Next non-readonly control should be focused", true, TextBox3.Focused);
		}

		public void TestPerformControlValidationMethodInfo_ShouldNotBeNull()
		{
			AssertNotNull("PerformControlValidationMethodInfo should not be null", ControlExtensions.PerformControlValidationMethodInfo);
		}

		#endregion

		#region GetUserData

		public void TestGetUserData()
		{
			int key = ControlExtensions.CreateUserDataKey();
			object value = new object();
			Control.SetUserData(key, value);
			AssertEquals(value, Control.GetUserData(key));
		}

		[ExpectException(typeof(ObjectDisposedException))]
		public void TestGetUserData_DisposedObject()
		{
			int key = ControlExtensions.CreateUserDataKey();
			Control.Dispose();
			Control.GetUserData(key);
		}

		#endregion

		#region TestGetParent

		public void TestGetParent()
		{
			var outerGroupBox = new KGroupBox();
			var innerGroupBox = new KGroupBox();
			var button = new KButton();
			outerGroupBox.Controls.Add(innerGroupBox);
			innerGroupBox.Controls.Add(button);

			AssertEquals(innerGroupBox, button.GetParent<KGroupBox>());
			AssertEquals(outerGroupBox, innerGroupBox.GetParent<KGroupBox>());
			AssertNull(button.GetParent<KPanel>());
			AssertNull(ControlExtensions.GetParent<KPanel>(null));
		}

		#endregion

		#region Test Classes

		class MockSite : ISite
		{
			public bool DesignMode
			{
				get { return true; }
			}

			public IContainer Container
			{
				get { return container ?? (container = new Container()); }
			}
			IContainer container;

			#region ISite Members

			public IComponent Component
			{
				get { throw new NotImplementedException(); }
			}

			public string Name
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			#endregion

			#region IServiceProvider Members

			public object GetService(Type serviceType)
			{
				return null;
			}

			#endregion
		}

		#endregion

		#region Invoke

		public void TestForceInvokeCalls()
		{
			var i = 0;
			using (var form = new Form())
			using (var control = new Panel())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				control.BeginInvokeSafe(() => { ++i; });
				AssertEquals(1, i);

				using (ControlExtensions.ForceInvokeCalls())
				{
					control.BeginInvokeSafe(() => { ++i; });
					AssertEquals("Should not be called yet", 1, i);
				}
				AssertEquals("Still should not be called yet", 1, i);
				Application.DoEvents();
				AssertEquals("Now should be called", 2, i);

				control.BeginInvokeSafe(() => { ++i; });
				AssertEquals(3, i);
			}
		}

		public void TestBeginInvokeSafe_MainThread()
		{
			var i = 0;

			Control control;
			using (control = new Form())
			{
				control.Show();

				control.BeginInvokeSafe(() => { ++i; });
			}

			AssertEquals("No Appliction.DoEvents means this does *not* behave in the same way as BeginInvoke when on main thread.", 1, i);
		}

		public void TestInvokeSafe_MainThread()
		{
			var i = 0;

			Control control;
			using (control = new Form())
			{
				control.Show();

				control.InvokeSafe(() => { ++i; });
			}

			AssertEquals("Behaves same as Invoke.", 1, i);
		}

		public void TestBeginInvokeSafe_MainThread_AfterDispose()
		{
			var i = 0;

			Control control;
			using (control = new Form())
			{
				control.Show();

				// Nothing
			}
			control.BeginInvokeSafe(() => { ++i; });

			AssertEquals("Do nothing. Control is disposed", 0, i);
		}

		public void TestInvokeSafe_MainThread_AfterDispose()
		{
			var i = 0;

			Control control;
			using (control = new Form())
			{
				control.Show();

				// Nothing
			}
			control.InvokeSafe(() => { ++i; });

			AssertEquals("Do nothing. Control is disposed", 0, i);
		}

		public void TestBeginInvokeSafe_BackgroundThread()
		{
			var i = 0;

			Control control;
			using (control = new Form())
			{
				control.Show();

				var thread = new Thread(() => control.BeginInvokeSafe(() => { ++i; }));
				thread.Start();
				thread.Join();
				AssertEquals("Nothing yet", 0, i);
				Application.DoEvents();
			}

			AssertEquals("How about now? Yes. Because the do event happened before dispose.", 1, i);
		}

		public void TestBeginInvokeSafe_BackgroundThread_EventsAfterDispose()
		{
			var i = 0;

			Control control;
			using (control = new Form())
			{
				control.Show();

				var thread = new Thread(() => control.BeginInvokeSafe(() => { ++i; }));
				thread.Start();
				thread.Join();
			}

			AssertEquals("Nothing yet", 0, i);
			Application.DoEvents();
			AssertEquals("Still nothing, because control was disposed.", 0, i);
		}

		public void TestBeginInvokeSafe_BackgroundThread_QueuedAfterDispose()
		{
			var i = 0;

			Control control;
			using (control = new Form())
			{
				control.Show();
			}

			var thread = new Thread(() => control.BeginInvokeSafe(() => { ++i; }));
			thread.Start();
			thread.Join();

			AssertEquals("Nothing yet", 0, i);
			Application.DoEvents();
			AssertEquals("Still nothing, because control was disposed.", 0, i);
		}

		public void TestInvokeSafe_BackgroundThread_QueuedAfterDispose()
		{
			var i = 0;

			Control control;
			using (control = new Form())
			{
				control.Show();
			}

			var thread = new Thread(() => control.InvokeSafe(() => { ++i; }));
			thread.Start();
			thread.Join();

			AssertEquals("Nothing yet", 0, i);
			Application.DoEvents();
			AssertEquals("Still nothing, because control was disposed.", 0, i);
		}

		public void TestInvokeSafe_BackgroundThread_EventsAfterDispose()
		{
			var i = 0;
			bool hasInvoked = false;
			Thread thread = null;
			Control control;

			try
			{
				using (control = new Form())
				{
					control.Show();

					thread = new Thread(() =>
					{
						control.BeginInvokeSafe(() => { ++i; });
						hasInvoked = true;
					});
					thread.Start();
				}

				AssertEquals("Nothing yet", 0, i);
				Application.DoEvents();
				while (!hasInvoked)
				{
					Application.DoEvents();
				}

				AssertEquals("Still nothing, because control was disposed.", 0, i);
			}
			finally
			{
				if (thread != null)
				{
					try
					{
						thread.Join(); // Don't leak the thread.
					} catch { }
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

		TextBox Control
		{
			get { return control ?? (control = new TextBox()); }
		}
		TextBox control;

		UserControl ControlWithoutReadOnlyProperty
		{
			get { return controlWithoutReadOnlyProperty ?? (controlWithoutReadOnlyProperty = new UserControl()); }
		}
		UserControl controlWithoutReadOnlyProperty;

		TextBox ControlWithReadOnlyProperty
		{
			get { return controlWithReadOnlyProperty ?? (controlWithReadOnlyProperty = new TextBox()); }
		}
		TextBox controlWithReadOnlyProperty;

		KTextBox ControlWithExplicitMetaData
		{
			get { return controlWithExplicitMetaData ?? (controlWithExplicitMetaData = new KTextBox()); }
		}
		KTextBox controlWithExplicitMetaData;

		KTextBox TextBox1
		{
			get { return textBox1 ?? (textBox1 = new KTextBox()); }
		}
		KTextBox textBox1;

		KTextBox TextBox2
		{
			get { return textBox2 ?? (textBox2 = new KTextBox()); }
		}
		KTextBox textBox2;

		KTextBox TextBox3
		{
			get { return textBox3 ?? (textBox3 = new KTextBox()); }
		}
		KTextBox textBox3;

		KUserControl UserControl
		{
			get { return userControl ?? (userControl = new KUserControl()); }
		}
		KUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			if (control != null)
			{
				control.Dispose();
			}
			if (form != null)
			{
				form.Dispose();
			}
			if (controlWithExplicitMetaData != null)
			{
				controlWithExplicitMetaData.Dispose();
			}
		}

		#endregion
	}
}
