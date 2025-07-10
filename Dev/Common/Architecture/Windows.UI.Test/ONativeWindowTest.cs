using System;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class ONativeWindowTest : TestCase
	{
		#region Factory Methods

		public void TestActiveTopLevelWindowForTest()
		{
			using (Form form = new Form())
			{
				form.Show();
				Application.DoEvents();
				form.Text = "TestForm";

				ONativeWindow.ActiveTopLevelWindowForTest = form.Handle;
				try
				{
					ONativeWindow window = ONativeWindow.ActiveTopLevelWindow;
					AssertEquals("Should find the correct window", "TestForm", window.Text);
				}
				finally
				{
					ONativeWindow.ActiveTopLevelWindowForTest = IntPtr.Zero;
				}
			}
		}

		#endregion

		#region GetAllTopLevelWindows

		public void TestGetAllTopLevelWindows()
		{
			AssertEquals("All forms in the current window station should be returned", true, ONativeWindow.GetAllTopLevelWindows().Length > 0);
		}

		#endregion

		#region Operator Overloads

		public void TestEquals()
		{
			using (Form form = new Form())
			{
				form.Show();
				Application.DoEvents();

				ONativeWindow nativeWindow1 = ONativeWindow.FromHandle(form.Handle);
				ONativeWindow nativeWindow2 = ONativeWindow.FromHandle(form.Handle);
				AssertEquals(true, nativeWindow1 == nativeWindow2);
				AssertEquals(false, nativeWindow1 != nativeWindow2);
				AssertEquals(true, nativeWindow1.Equals(nativeWindow2));
			}
		}

		#endregion

		#region Visible / Enabled / Focus

		public void TestVisible()
		{
			using (Form form = new Form())
			{
				form.Show();
				Application.DoEvents();
				ONativeWindow nativeWindow = ONativeWindow.FromHandle(form.Handle);

				AssertEquals("Visible true", true, nativeWindow.Visible);
				form.Visible = false;
				AssertEquals("Visible false", false, nativeWindow.Visible);
			}
		}

		public void TestEnabled()
		{
			using (Form form = new Form())
			{
				form.Show();
				Application.DoEvents();
				ONativeWindow nativeWindow = ONativeWindow.FromHandle(form.Handle);

				AssertEquals("Enabled true", true, nativeWindow.Enabled);
				form.Enabled = false;
				AssertEquals("Enabled false", false, nativeWindow.Enabled);
			}
		}

		#endregion

		#region Bounds

		public void TestBounds()
		{
			using (Form form = new Form())
			{
				form.Text = "caption";
				form.Show();
				form.Left = 10;
				form.Top = 11;
				form.Width = 12;
				form.Height = 13;
				Application.DoEvents();

				ONativeWindow nativeWindow = ONativeWindow.FromHandle(form.Handle);
				AssertEquals(form.Left, nativeWindow.Bounds.Left);
				AssertEquals(form.Top, nativeWindow.Bounds.Top);
				AssertEquals(form.Right, nativeWindow.Bounds.Right);
				AssertEquals(form.Bottom, nativeWindow.Bounds.Bottom);
			}
		}

		#endregion

		#region Text

		public void TestText()
		{
			using (Form form = new Form())
			{
				form.Text = "caption";
				form.Show();
				Application.DoEvents();
				ONativeWindow nativeWindow = ONativeWindow.FromHandle(form.Handle);
				AssertEquals("caption", nativeWindow.Text);
			}
		}

		#endregion

		#region GetWindowStyle / SetWindowStyle

		public void TestGetSetWindowStyle()
		{
			ONativeWindow nativeWindow = ONativeWindow.FromHandle(Form.Handle);
			nativeWindow.SetWindowStyle(CargoWise.Interop.NativeMethods.WindowStyles.WS_DISABLED, true);
			nativeWindow.SetWindowStyle(CargoWise.Interop.NativeMethods.WindowStyles.WS_EX_ACCEPTFILES, true);
			AssertEquals(true, nativeWindow.GetWindowStyle(CargoWise.Interop.NativeMethods.WindowStyles.WS_DISABLED));
			AssertEquals(true, nativeWindow.GetWindowStyle(CargoWise.Interop.NativeMethods.WindowStyles.WS_EX_ACCEPTFILES));

			nativeWindow.SetWindowStyle(CargoWise.Interop.NativeMethods.WindowStyles.WS_DISABLED, false);
			nativeWindow.SetWindowStyle(CargoWise.Interop.NativeMethods.WindowStyles.WS_EX_ACCEPTFILES, true);
			AssertEquals(false, nativeWindow.GetWindowStyle(CargoWise.Interop.NativeMethods.WindowStyles.WS_DISABLED));
			AssertEquals(true, nativeWindow.GetWindowStyle(CargoWise.Interop.NativeMethods.WindowStyles.WS_EX_ACCEPTFILES));

			nativeWindow.SetWindowStyle(CargoWise.Interop.NativeMethods.WindowStyles.WS_DISABLED, true);
			nativeWindow.SetWindowStyle(CargoWise.Interop.NativeMethods.WindowStyles.WS_EX_ACCEPTFILES, false);
			AssertEquals(true, nativeWindow.GetWindowStyle(CargoWise.Interop.NativeMethods.WindowStyles.WS_DISABLED));
			AssertEquals(false, nativeWindow.GetWindowStyle(CargoWise.Interop.NativeMethods.WindowStyles.WS_EX_ACCEPTFILES));
		}

		#endregion

		#region IsActiveTopLevelWindow

		public void TestIsActiveTopLevelWindow()
		{
			using (Form form = new Form())
			{
				form.Show();
				Application.DoEvents();
				ONativeWindow.ActiveTopLevelWindowForTest = form.Handle;

				ONativeWindow nativeWindow;
				try
				{
					nativeWindow = ONativeWindow.FromHandle(form.Handle);
					AssertEquals("The active form is the one showing now", form.Handle, nativeWindow.Handle);
					AssertEquals("IsActiveTopLevelWindow should be true", true, nativeWindow.IsActiveTopLevelWindow);
				}
				finally
				{
					ONativeWindow.ActiveTopLevelWindowForTest = IntPtr.Zero;
				}

				using (Form decoy = new Form())
				{
					decoy.Show();
					Application.DoEvents();
					ONativeWindow.ActiveTopLevelWindowForTest = decoy.Handle;
					try
					{
						AssertEquals("IsActive should be false", false, nativeWindow.IsActiveTopLevelWindow);
					}
					finally
					{
						ONativeWindow.ActiveTopLevelWindowForTest = IntPtr.Zero;
					}
				}
			}
		}

		#endregion

		#region Implementation

		Form Form
		{
			get { return form ?? (form = new Form()); }
		}
		Form form;

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
