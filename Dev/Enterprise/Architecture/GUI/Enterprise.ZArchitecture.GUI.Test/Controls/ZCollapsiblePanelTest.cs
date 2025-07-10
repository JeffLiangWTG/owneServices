using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Layout;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZCollapsiblePanelTest : ZPanelTest
	{
		public void TestIsCollapsed_UpdateSize2()
		{
			using (var panel = new ZCollapsiblePanel { Size = new Size(300, 200) })
			using (var splitter2 = new KSplitter { Size = new Size(300, 200) })
			{
				panel.LinkSplitter(splitter2);
				var dpiScaledPanelCaptionHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(ZCollapsiblePanel.CaptionHeight);

				panel.Dock = DockStyle.Top;
				panel.IsCollapsed = false;
				AssertEquals(300, panel.Width);
				AssertEquals(200, panel.Height);
				Point relativeLoc = new Point(splitter2.Location.X - panel.Location.X, splitter2.Location.Y - panel.Location.Y);

				AssertEquals(0, relativeLoc.X);
				AssertEquals(0, relativeLoc.Y);
			}
		}
		public void TestIsCollapsed_UpdateSize()
		{
			using (var panel = new ZCollapsiblePanel { Size = new Size(300, 200) })
			{
				var dpiScaledPanelCaptionHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(ZCollapsiblePanel.CaptionHeight);

				panel.Dock = DockStyle.Top;
				panel.IsCollapsed = true;
				AssertEquals(300, panel.Width);
				AssertEquals(dpiScaledPanelCaptionHeight, panel.Height);
				panel.IsCollapsed = false;
				AssertEquals(300, panel.Width);
				AssertEquals(200, panel.Height);

				panel.Dock = DockStyle.Bottom;
				panel.IsCollapsed = true;
				AssertEquals(300, panel.Width);
				AssertEquals(dpiScaledPanelCaptionHeight, panel.Height);
				panel.IsCollapsed = false;
				AssertEquals(300, panel.Width);
				AssertEquals(200, panel.Height);

				panel.Dock = DockStyle.Left;
				panel.IsCollapsed = true;
				AssertEquals(dpiScaledPanelCaptionHeight, panel.Width);
				AssertEquals(200, panel.Height);
				panel.IsCollapsed = false;
				AssertEquals(300, panel.Width);
				AssertEquals(200, panel.Height);

				panel.Dock = DockStyle.Right;
				panel.IsCollapsed = true;
				AssertEquals(dpiScaledPanelCaptionHeight, panel.Width);
				AssertEquals(200, panel.Height);
				panel.IsCollapsed = false;
				AssertEquals(300, panel.Width);
				AssertEquals(200, panel.Height);

				panel.Dock = DockStyle.None;
				panel.IsCollapsed = true;
				AssertEquals("Do not change size if not docked to one side", 300, panel.Width);
				AssertEquals("Do not change size if not docked to one side", 200, panel.Height);
				panel.IsCollapsed = false;
				AssertEquals(300, panel.Width);
				AssertEquals(200, panel.Height);
			}
		}

		public void TestIsCollapsed_ControlsVisibility()
		{
			using (var panel = new ZCollapsiblePanel { Size = new Size(300, 200) })
			using (var label = new ZLabel())
			{
				panel.Controls.Add(label);
				Assert(label.Visible);

				panel.IsCollapsed = true;
				Assert(!label.Visible);

				panel.IsCollapsed = false;
				Assert(label.Visible);
			}
		}

		public void TestLinkSplitter()
		{
			using (var panel = new ZCollapsiblePanel { Size = new Size(300, 200) })
			using (var splitter1 = new KSplitter())
			using (var splitter2 = new KSplitter())
			{
				panel.LinkSplitter(splitter2);
				Assert(splitter1.Visible);
				Assert(!splitter1.DoNotSaveSplitterLayout);
				Assert(splitter2.Visible);
				Assert(splitter2.DoNotSaveSplitterLayout);

				panel.IsCollapsed = true;
				Assert(splitter1.Visible);
				Assert(!splitter2.Visible);

				panel.IsCollapsed = false;
				Assert(splitter1.Visible);
				Assert(splitter2.Visible);
			}
		}

		public void TestSplitterPosition()
		{
			using (var panel = new ZCollapsiblePanel { Size = new Size(300, 200) })
			{
				var splitterLayoutSaveProvider = (ISplitterLayoutSaveProvider)panel;
				Assert(!splitterLayoutSaveProvider.IsSplitterFixed);
				AssertEquals(
					"ContainerSize should be constant to restore panel same size every time",
					 ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					 splitterLayoutSaveProvider.ContainerSize);

				panel.Dock = DockStyle.Top;
				AssertEquals(200, ControlDpiScalingHelper.ScaleToCurrentDpiX(splitterLayoutSaveProvider.SplitterPosition), 1d);
				panel.IsCollapsed = true;
				AssertEquals(-200, ControlDpiScalingHelper.ScaleToCurrentDpiX(splitterLayoutSaveProvider.SplitterPosition), 1d);
				panel.IsCollapsed = false;

				panel.Dock = DockStyle.Bottom;
				AssertEquals(200, ControlDpiScalingHelper.ScaleToCurrentDpiX(splitterLayoutSaveProvider.SplitterPosition), 1d);
				panel.IsCollapsed = true;
				AssertEquals(-200, ControlDpiScalingHelper.ScaleToCurrentDpiX(splitterLayoutSaveProvider.SplitterPosition), 1d);
				panel.IsCollapsed = false;

				panel.Dock = DockStyle.Left;
				AssertEquals(300, ControlDpiScalingHelper.ScaleToCurrentDpiX(splitterLayoutSaveProvider.SplitterPosition), 1d);
				panel.IsCollapsed = true;
				AssertEquals(-300, ControlDpiScalingHelper.ScaleToCurrentDpiX(splitterLayoutSaveProvider.SplitterPosition), 1d);
				panel.IsCollapsed = false;

				panel.Dock = DockStyle.Right;
				AssertEquals(300, ControlDpiScalingHelper.ScaleToCurrentDpiX(splitterLayoutSaveProvider.SplitterPosition), 1d);
				panel.IsCollapsed = true;
				AssertEquals(-300, ControlDpiScalingHelper.ScaleToCurrentDpiX(splitterLayoutSaveProvider.SplitterPosition), 1d);
				panel.IsCollapsed = false;
			}
		}
#if !WINZOR
		public void TestMouseClickToCollapse()
		{
			using (var panel = new ClickablePanelForTest { Size = new Size(300, 200) })
			{
				Assert(!panel.IsCollapsed);

				panel.DoClick(100, 100);
				Assert(!panel.IsCollapsed);

				panel.DoClick(ZCollapsiblePanel.CaptionHeight / 2, ZCollapsiblePanel.CaptionHeight / 2);
				Assert(panel.IsCollapsed);

				panel.DoClick(ZCollapsiblePanel.CaptionHeight / 2, ZCollapsiblePanel.CaptionHeight / 2);
				Assert(!panel.IsCollapsed);
			}
		}
#endif
		#region Clickable panel for test

		class ClickablePanelForTest : ZCollapsiblePanel
		{
			public void DoMouseMove(int x, int y)
			{
				OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
			}

			public void DoMouseDown(int x, int y)
			{
				OnMouseDown(new MouseEventArgs(MouseButtons.Left, 0, x, y, 0));
			}

			public void DoMouseUp(int x, int y)
			{
				OnMouseUp(new MouseEventArgs(MouseButtons.Left, 0, x, y, 0));
			}

			public void DoClick(int x, int y)
			{
				DoMouseMove(x, y);
				DoMouseDown(x, y);
				DoMouseUp(x, y);
			}
		}

		#endregion
	}
}
