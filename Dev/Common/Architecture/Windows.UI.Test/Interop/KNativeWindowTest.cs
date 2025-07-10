using System;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Interop.Testing
{
	sealed class KNativeWindowTest : TestCase
	{
		public void TestIsActiveTopLevelWindow()
		{
			using (Form form = new Form())
			{
				form.Show();
				Application.DoEvents();
				KNativeWindow.ActiveTopLevelWindowForTest = form.Handle;

				KNativeWindow nativeWindow;
				try
				{
					nativeWindow = KNativeWindow.FromHandle(form.Handle);
					AssertEquals("The active form is the one showing now", form.Handle, nativeWindow.Handle);
					AssertEquals("IsActiveTopLevelWindow should be true", true, nativeWindow.IsActiveTopLevelWindow);
				}
				finally
				{
					KNativeWindow.ActiveTopLevelWindowForTest = IntPtr.Zero;
				}

				using (Form decoy = new Form())
				{
					decoy.Show();
					Application.DoEvents();
					KNativeWindow.ActiveTopLevelWindowForTest = decoy.Handle;
					try
					{
						AssertEquals("IsActive should be false", false, nativeWindow.IsActiveTopLevelWindow);
					}
					finally
					{
						KNativeWindow.ActiveTopLevelWindowForTest = IntPtr.Zero;
					}
				}
			}
		}

		public void TestActiveTopLevelWindowForTest()
		{
			using (Form form = new Form())
			{
				form.Show();
				Application.DoEvents();
				form.Text = "TestForm";

				KNativeWindow.ActiveTopLevelWindowForTest = form.Handle;
				try
				{
					KNativeWindow window = KNativeWindow.ActiveTopLevelWindow;
					AssertEquals("Should find the correct window", "TestForm", window.Text);
				}
				finally
				{
					KNativeWindow.ActiveTopLevelWindowForTest = IntPtr.Zero;
				}
			}
		}

		public void TestGetAllTopLevelWindows()
		{
			AssertEquals("All forms in the current window station should be returned", true, KNativeWindow.GetAllTopLevelWindows().Length > 0);
		}

		public void TestText()
		{
			using (Form form = new Form())
			{
				form.Text = "caption";
				form.Show();
				Application.DoEvents();
				KNativeWindow nativeWindow = KNativeWindow.FromHandle(form.Handle);
				AssertEquals("caption", nativeWindow.Text);
			}
		}

		public void TestEnabled()
		{
			using (Form form = new Form())
			{
				form.Show();
				Application.DoEvents();
				KNativeWindow nativeWindow = KNativeWindow.FromHandle(form.Handle);

				AssertEquals("Enabled true", true, nativeWindow.Enabled);
				form.Enabled = false;
				AssertEquals("Enabled false", false, nativeWindow.Enabled);
			}
		}

		public void TestVisible()
		{
			using (Form form = new Form())
			{
				form.Show();
				Application.DoEvents();
				KNativeWindow nativeWindow = KNativeWindow.FromHandle(form.Handle);

				AssertEquals("Visible true", true, nativeWindow.Visible);
				form.Visible = false;
				AssertEquals("Visible false", false, nativeWindow.Visible);
			}
		}

		public void TestDimensions()
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

				KNativeWindow nativeWindow = KNativeWindow.FromHandle(form.Handle);
				AssertEquals(form.Left, nativeWindow.Bounds.Left);
				AssertEquals(form.Top, nativeWindow.Bounds.Top);
				AssertEquals(form.Right, nativeWindow.Bounds.Right);
				AssertEquals(form.Bottom, nativeWindow.Bounds.Bottom);
			}
		}

		public void TestEquals()
		{
			using (Form form = new Form())
			{
				form.Show();
				Application.DoEvents();

				KNativeWindow nativeWindow1 = KNativeWindow.FromHandle(form.Handle);
				KNativeWindow nativeWindow2 = KNativeWindow.FromHandle(form.Handle);
				AssertEquals(true, nativeWindow1 == nativeWindow2);
				AssertEquals(false, nativeWindow1 != nativeWindow2);
				AssertEquals(true, nativeWindow1.Equals(nativeWindow2));
			}
		}
	}
}
