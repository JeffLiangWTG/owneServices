using System;
using System.Windows.Forms;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using Enterprise.DocumentScanning.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	sealed class RulerPanelTest : TestCase
	{
		public void TestHotkeys()
		{
			using (var rulerPanel = new RulerPanel())
			{
				rulerPanel.SetRulerLocation(Oreantation.Horizontal, 50);
				rulerPanel.SetRulerLocation(Oreantation.Vertical, 50);

				KeySender.SendKeyDownToProcessCmdKey(rulerPanel, (int)(Keys.Alt | Keys.Down));
				AssertEquals(51, rulerPanel.GetRulerLocation(Oreantation.Horizontal));

				KeySender.SendKeyDownToProcessCmdKey(rulerPanel, (int)(Keys.Alt | Keys.Up));
				AssertEquals(50, rulerPanel.GetRulerLocation(Oreantation.Horizontal));

				KeySender.SendKeyDownToProcessCmdKey(rulerPanel, (int)(Keys.Alt | Keys.Right));
				AssertEquals(51, rulerPanel.GetRulerLocation(Oreantation.Vertical));

				KeySender.SendKeyDownToProcessCmdKey(rulerPanel, (int)(Keys.Alt | Keys.Left));
				AssertEquals(50, rulerPanel.GetRulerLocation(Oreantation.Vertical));
			}
		}

		public void TestDraggingLine()
		{
			using (var form = new ZForm())
			using (var rulerPanel = new RulerPanel())
			{
				form.Controls.Add(rulerPanel);

				form.Show();

				rulerPanel.SetRulerLocation(Oreantation.Horizontal, 50);

				var message = MouseMessageFilterTest.MakeMouseEvent(WindowsMessage.WM_LBUTTONDOWN, rulerPanel, 50, 50);
				rulerPanel.FireMouseHook(ref message, ControlDpiScalingHelper.NewScaledPoint(50, 50)); // Grabs horizontal line

				message = MouseMessageFilterTest.MakeMouseEvent(WindowsMessage.WM_MOUSEMOVE, rulerPanel, 50, 75);
				rulerPanel.FireMouseHook(ref message, ControlDpiScalingHelper.NewScaledPoint(50, 75)); // Drags line

				AssertEquals("Line should have been dragged with the mouse", 75, rulerPanel.GetRulerLocation(Oreantation.Horizontal));

				message = MouseMessageFilterTest.MakeMouseEvent(WindowsMessage.WM_LBUTTONUP, rulerPanel, 50, 75);
				rulerPanel.FireMouseHook(ref message, ControlDpiScalingHelper.NewScaledPoint(50, 75)); // Release Line

				message = MouseMessageFilterTest.MakeMouseEvent(WindowsMessage.WM_MOUSEMOVE, rulerPanel, 50, 100);
				rulerPanel.FireMouseHook(ref message, ControlDpiScalingHelper.NewScaledPoint(50, 75)); // Move without drag

				AssertEquals("Line should NOT have been dragged with the mouse because it was released", 75, rulerPanel.GetRulerLocation(Oreantation.Horizontal));
			}
		}

		public void TestWhenShowLinesIsFalseRulerIgnoresMouseEvents()
		{
			using (var form = new ZForm())
			using (var rulerPanel = new RulerPanel())
			{
				form.Controls.Add(rulerPanel);

				form.Show();

				rulerPanel.SetRulerLocation(Oreantation.Horizontal, 50);

				rulerPanel.ShowLines = false;

				var message = MouseMessageFilterTest.MakeMouseEvent(WindowsMessage.WM_LBUTTONDOWN, rulerPanel, 50, 50);
				Assert("No mouse events should be enabled when the lines arent visible", !rulerPanel.FireMouseHook(ref message, ControlDpiScalingHelper.NewScaledPoint(50, 50)));

				message.Msg = WindowsMessage.WM_MOUSEMOVE;
				rulerPanel.FireMouseHook(ref message, ControlDpiScalingHelper.NewScaledPoint(50, 50));

				Assert("No mouse events should be enabled when the lines arent visible", !rulerPanel.FireMouseHook(ref message, ControlDpiScalingHelper.NewScaledPoint(50, 50)));
			}
		}

		[RequiresSTA]
		public void TestDraggingLine_MultipleLines()
		{
			using (var form = new ZForm())
			using (var rulerPanel = new RulerPanel())
			{
				form.Controls.Add(rulerPanel);

				form.Show();

				rulerPanel.SetRulerLocation(Oreantation.Horizontal, 50);
				rulerPanel.SetRulerLocation(Oreantation.Vertical, 50);

				var message = MouseMessageFilterTest.MakeMouseEvent(WindowsMessage.WM_LBUTTONDOWN, rulerPanel, 50, 50);
				rulerPanel.FireMouseHook(ref message, ControlDpiScalingHelper.NewScaledPoint(50, 50)); // Grabs botrh lines

				message = MouseMessageFilterTest.MakeMouseEvent(WindowsMessage.WM_MOUSEMOVE, rulerPanel, 75, 75);
				rulerPanel.FireMouseHook(ref message, ControlDpiScalingHelper.NewScaledPoint(75, 75)); // Drags lines

				AssertEquals("Horizontal line should have been dragged with the mouse", 75, rulerPanel.GetRulerLocation(Oreantation.Horizontal));
				AssertEquals("Vertical line should also have been dragged with the mouse", 75, rulerPanel.GetRulerLocation(Oreantation.Vertical));
			}
		}

		public void TestEventsThatDontHitLinesArePassedToCorrectControl()
		{
			using (var form = new ZForm())
			using (var rulerPanel = new RulerPanel())
			using (var child = new ZUserControl())
			{
				child.DockInside(rulerPanel);
				rulerPanel.Dock = DockStyle.Fill;

				form.Size = ControlDpiScalingHelper.NewScaledSize(300, 300);
				form.Controls.Add(rulerPanel);

				form.Show();

				var childMouseDownCount = 0;
				child.MouseClick += (o, e) => childMouseDownCount++;

				rulerPanel.SetRulerLocation(Oreantation.Horizontal, 50);
				rulerPanel.SetRulerLocation(Oreantation.Vertical, 50);

				var message = MouseMessageFilterTest.MakeMouseEvent(WindowsMessage.WM_LBUTTONDOWN, rulerPanel, 50, 50);
				Assert("We clicked on top of a line, so mouse event should not be passed to child", rulerPanel.FireMouseHook(ref message, ControlDpiScalingHelper.NewScaledPoint(50, 50)));

				message = MouseMessageFilterTest.MakeMouseEvent(WindowsMessage.WM_LBUTTONUP, rulerPanel, 50, 50);
				Assert("We clicked on top of a line, so mouse event should not be passed to child", rulerPanel.FireMouseHook(ref message, ControlDpiScalingHelper.NewScaledPoint(50, 50)));

				message = MouseMessageFilterTest.MakeMouseEvent(WindowsMessage.WM_LBUTTONDOWN, rulerPanel, 75, 75);
				Assert("We've dodged the lines so we want this one passed down", !rulerPanel.FireMouseHook(ref message, ControlDpiScalingHelper.NewScaledPoint(75, 75)));
			}
		}

		public void TestUpdatesCursorsWhenOverLines()
		{
			// Fails locally when you move your mouse over something that effects the cursor (like hovering over a text box to get the 'I' typing curser
			// Shouldnt be a problem on DAT

			using (var form = new ZForm())
			using (var rulerPanel = new RulerPanel())
			{
				form.Controls.Add(rulerPanel);

				form.Show();

				rulerPanel.SetRulerLocation(Oreantation.Horizontal, ControlDpiScalingHelper.ScaleToCurrentDpiX(50));
				rulerPanel.SetRulerLocation(Oreantation.Vertical, ControlDpiScalingHelper.ScaleToCurrentDpiY(50));

				CombineAssertions(() =>
				{
					AssertUsesCursor("When we're over a horizontal line, use HSplit Cursor", Cursors.HSplit, rulerPanel, 100, 50);
					AssertUsesCursor("When we're over a vertical line, use VSplit Cursor", Cursors.VSplit, rulerPanel, 50, 100);
					AssertUsesCursor("When we're over both the horizontal and vertical line use a crosshair", Cursors.Cross, rulerPanel, 50, 50);
				});
			}
		}

		void AssertUsesCursor(string failMessage, Cursor expected, RulerPanel rulerPanel, int x, int y)
		{
			var message = MouseMessageFilterTest.MakeMouseEvent(WindowsMessage.WM_MOUSEMOVE, rulerPanel, x, y);
			rulerPanel.FireMouseHook(ref message, ControlDpiScalingHelper.NewScaledPoint(x, y));

			AssertEquals(failMessage, expected, Cursor.Current);
		}

		public void TestEventsArentLeakingObjects()
		{
			using (var form = new ZChildForm())
			using (var child = new ZUserControl())
			{
				form.Show();

				var reference = CreatePanelThenAddAndRemoveChild(form, child);

				GC.Collect(); // Unit test for leaks, need to force a GC
				GC.WaitForPendingFinalizers();
				GC.Collect();

				RulerPanel ignored;
				Assert("We have leaked the ruler panel. This indicates that the panel is not correctly removing its event handlers from its children.", !reference.TryGetTarget(out ignored));
			}
		}

		WeakReference<RulerPanel> CreatePanelThenAddAndRemoveChild(Control parent, Control child)
		{
			var parentForm = parent.FindForm();
			if (parentForm == null || !parentForm.Visible)
			{
				Fail("Parent form needs to be shown or else the handle is never created");
			}

			using (var rulerPanel = new RulerPanel())
			{
				rulerPanel.Controls.Add(child);
				parent.Controls.Add(rulerPanel);

				parent.Refresh();
				while (!rulerPanel.IsHandleCreated)
				{
					Application.DoEvents();
				}

				parent.Controls.Remove(rulerPanel);
				rulerPanel.Controls.Remove(child);

				return new WeakReference<RulerPanel>(rulerPanel);
			}
		}
	}
}
