using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Interop;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static CargoWise.Interop.SafeNativeMethods;
using static CargoWise.Windows.UI.ControlDpiScalingHelper;

namespace Enterprise.DocumentScanning.Testing
{
	[GuiTest]
	sealed class MouseMessageFilterTest : TestCase
	{
		public static Message MakeMouseEvent(int mouseMessage, Control relativeTo, int x, int y, bool isOnStandardDpi = true)
		{
			if (isOnStandardDpi)
			{
				x = ScaleToCurrentDpiX(x);
				y = ScaleToCurrentDpiX(y);
			}

			// The X & Y coords of the mouse event are squashed into the LPARAM as per the windows spec
			// https://msdn.microsoft.com/en-us/library/windows/desktop/ms645616(v=vs.85).aspx
			var lparam = unchecked(new IntPtr((x & 0xFFFF) | ((y << 16) & (int)0xFFFF0000)));
#pragma warning disable WFDEV001
			return new Message { HWnd = relativeTo.Handle, Msg = mouseMessage, LParam = lparam };
#pragma warning restore WFDEV001
		}

		[GuiTest]
		[RequiresSTA]
		public void TestOnlyWorksWithinBounds()
		{
			using (var form = new ZChildForm())
			using (var control = new ZUserControl())
			{
				control.Location = NewScaledPoint(100, 100);
				control.Size = NewScaledSize(100, 100);

				form.Size = NewScaledSize(300, 300);
				form.Show();

				CombineAssertions(() =>
				{
					var msg = MakeMouseEvent(WM_MOUSEMOVE, control, -10, 50);
					AssertIsCalled("It is above the bounds and should be missed", control, ref msg, false);

					msg = MakeMouseEvent(WM_MOUSEMOVE, control, 50, -10);
					AssertIsCalled("It is left of the bounds and should be missed", control, ref msg, false);

					msg = MakeMouseEvent(WM_MOUSEMOVE, control, 250, 50);
					AssertIsCalled("It is right of the bounds and should be missed", control, ref msg, false);

					msg = MakeMouseEvent(WM_MOUSEMOVE, control, 50, 250);
					AssertIsCalled("It is under the bounds and should be missed", control, ref msg, false);

					msg = MakeMouseEvent(WM_MOUSEMOVE, control, 50, 50);
					AssertIsCalled("Goldilocks, not too high or too low. Should be hit", control, ref msg, true);
				});
			}
		}

		[GuiTest]
		public void TestOnlyWorksWithMouseMethods()
		{
			using (var form = new ZChildForm())
			{
				form.Size = NewScaledSize(300, 300);
				form.Show();

				var msg = MakeMouseEvent(WM_MOUSEFIRST, form, 150, 150);

				CombineAssertions(() =>
				{
					for (msg.Msg = WindowsMessage.WM_MOUSEFIRST; msg.Msg <= WindowsMessage.WM_MOUSELAST; msg.Msg++)
					{
						AssertIsCalled("Should work for " + FindNameOfWindowsMessage(msg.Msg), form, ref msg);
					}

					msg.Msg = WindowsMessage.WM_MOUSELAST + 1;
					AssertIsCalled("Should not work for " + FindNameOfWindowsMessage(msg.Msg, false), form, ref msg, false);

					msg.Msg = WindowsMessage.WM_MOUSEFIRST - 1;
					AssertIsCalled("Should not work for " + FindNameOfWindowsMessage(msg.Msg, false), form, ref msg, false);
				});
			}
		}

		[GuiTest]
		public void TestFindsForCorrectChild()
		{
			using (var form = new ZChildForm())
			using (var outerControl = new ZUserControl())
			using (var innerControl = new ZUserControl())
			{
				outerControl.Location = NewScaledPoint(100, 100);
				innerControl.Location = NewScaledPoint(100, 100);

				outerControl.Size = NewScaledSize(300, 300);
				innerControl.Size = NewScaledSize(100, 100);
				form.Size = NewScaledSize(500, 500);

				outerControl.Controls.Add(innerControl);
				form.Controls.Add(outerControl);

				form.Show();

				Point lastPoint = Point.Empty;
				var filter = new MouseMessageFilter(outerControl, delegate(ref Message inner, Point mousePosition)
				{
					lastPoint = mousePosition;
					return true;
				});

				var message = MakeMouseEvent(WM_MOUSEMOVE, innerControl, 50, 50, false);
				filter.PreFilterMessage(ref message);

				var expectedPosition = NewScaledPoint(innerControl.Left + MarkAsScaled(50), innerControl.Top + MarkAsScaled(50), false);
				AssertEquals("Even though the event was targeted at the child control, we should recieve the actual location", expectedPosition, lastPoint);
			}
		}

		string FindNameOfWindowsMessage(int msg, bool failIfNotFound = true)
		{
			var fields = typeof(WindowsMessage).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
			foreach (var field in fields)
			{
				if (field.IsLiteral && field.FieldType == typeof(int) && (int)field.GetValue(null) == msg)
				{
					return field.Name;
				}
			}

			if (failIfNotFound)
			{
				Fail("Cant find the name for a Windows Message - " + msg);
			}

			return "UNKNOWN - " + msg;
		}

		void AssertIsCalled(string erMsg, Control c, ref Message m, bool shouldBeCalled = true)
		{
			var wasCalled = false;
			var filter = new MouseMessageFilter(c, delegate(ref Message inner, Point mousePosition)
			{
				wasCalled = true;
				return true;
			});

			filter.PreFilterMessage(ref m);

			Assert(erMsg, wasCalled == shouldBeCalled);
		}
	}
}
