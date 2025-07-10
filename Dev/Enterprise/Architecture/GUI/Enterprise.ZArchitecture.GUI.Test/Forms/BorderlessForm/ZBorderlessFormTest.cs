using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI.Forms;
using Enterprise.ZArchitecture.GUI.Forms.Native;
using NUnit.Framework;
using static Enterprise.Core.Forms.CachedScreenInfo;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZBorderlessFormTest : TestCase
	{
		public void TestWindowStateChangePropagatedCorrectly()
		{
			using (var form = new ZBorderlessForm())
			{
#if WINZOR
				form.Show();  // Winzor need form.Show to pass CargowiseClientServices to the form
#endif

				// arrange
				var onLayout = FormWindowState.Normal;
				var onResize = FormWindowState.Normal;
				form.Layout += (s, e) => { onLayout = form.WindowState; };
				form.SizeChanged += (s, e) => { onResize = form.WindowState; };
				form.Visible = true;

				// pre-assert
				AssertEquals(FormWindowState.Normal, form.WindowState);
				AssertEquals(FormWindowState.Normal, onLayout);
				AssertEquals(FormWindowState.Normal, onResize);

				//Currently, Winzor setting WindowState to Minimized does not trigger Layout and Resize
#if !WINZOR
				// act
				form.WindowState = FormWindowState.Minimized;
				// assert
				AssertEquals(FormWindowState.Minimized, form.WindowState);
				AssertEquals(FormWindowState.Minimized, onLayout);
				AssertEquals(FormWindowState.Minimized, onResize);
#endif

				// act
				form.WindowState = FormWindowState.Maximized;
				// assert
				AssertEquals(FormWindowState.Maximized, form.WindowState);
				AssertEquals(FormWindowState.Maximized, onLayout);
				AssertEquals(FormWindowState.Maximized, onResize);

				// act
				form.WindowState = FormWindowState.Normal;
				// assert
				AssertEquals(FormWindowState.Normal, form.WindowState);
				AssertEquals(FormWindowState.Normal, onLayout);
				AssertEquals(FormWindowState.Normal, onResize);
			}
		}

		public void TestSetBorderColour()
		{
			using (var form = new ZBorderlessForm())
			{
				// arrange
				var color = Color.Chartreuse;
				// act
				form.SetBorderColour(color);

				// assert
				AssertEquals(color, form.TopLeftCornerPanel.BackColor);
				AssertEquals(color, form.TopBorderPanel.BackColor);
				AssertEquals(color, form.TopRightCornerPanel.BackColor);
				AssertEquals(color, form.LeftBorderPanel.BackColor);
				AssertEquals(color, form.RightBorderPanel.BackColor);
				AssertEquals(color, form.BottomLeftCornerPanel.BackColor);
				AssertEquals(color, form.BottomBorderPanel.BackColor);
				AssertEquals(color, form.BottomRightCornerPanel.BackColor);
			}
		}

		public void TestResizeBorders()
		{
			using (var form = new ZBorderlessForm())
			{
				// arrange
				form.Size = ControlDpiScalingHelper.NewScaledSize(1366, 768);
				form.BorderSize = 7;

				// act
				form.ResizeBorders();

				// assert size
				var expectedCornerPanelSize = ControlDpiScalingHelper.NewScaledSize(7, 7);
				AssertEquals(expectedCornerPanelSize, form.TopLeftCornerPanel.Size);
				AssertEquals(expectedCornerPanelSize, form.TopRightCornerPanel.Size);
				AssertEquals(expectedCornerPanelSize, form.BottomLeftCornerPanel.Size);
				AssertEquals(expectedCornerPanelSize, form.BottomRightCornerPanel.Size);

				var expectedLeftRightBorderSize = ControlDpiScalingHelper.NewScaledSize(7, 754);
				var expectedTopBottomBorderSize = ControlDpiScalingHelper.NewScaledSize(1352, 7);
				AssertEquals(expectedLeftRightBorderSize, form.LeftBorderPanel.Size);
				AssertEquals(expectedLeftRightBorderSize, form.RightBorderPanel.Size);
				AssertEquals(expectedTopBottomBorderSize, form.TopBorderPanel.Size);
				AssertEquals(expectedTopBottomBorderSize, form.BottomBorderPanel.Size);

				// assert location
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(0, 0), form.TopLeftCornerPanel.Location);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(1359, 0), form.TopRightCornerPanel.Location);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(0, 761), form.BottomLeftCornerPanel.Location);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(1359, 761), form.BottomRightCornerPanel.Location);

				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(7, 0), form.TopBorderPanel.Location);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(0, 7), form.LeftBorderPanel.Location);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(1359, 7), form.RightBorderPanel.Location);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(7, 761), form.BottomBorderPanel.Location);
			}
		}

		public void TestMakeLong()
		{
			using (var form = new ZBorderlessForm())
			{
				// assert
				AssertNoExceptionThrown(() => ZBorderlessForm.PointToLParam(new Point(0, 0)));
				AssertNoExceptionThrown(() => ZBorderlessForm.PointToLParam(new Point(1, 2)));
				AssertNoExceptionThrown(() => ZBorderlessForm.PointToLParam(new Point(-2, -3)));
				AssertNoExceptionThrown(() => ZBorderlessForm.PointToLParam(new Point(-2000000, 300000)));
			}
		}

#if !WINZOR

		public void TestSetWindowRegion()
		{
			using (var form = new ZBorderlessForm())
			{
				// arrange
				form.StartPosition = FormStartPosition.CenterScreen;
				form.Show();
				Application.DoEvents();

				// act
				form.SetWindowRegion(form.Handle, form.Left, form.Top, form.Right, form.Bottom);

				var windowRegion = new HandleRef(this, UnsafeNativeMethods.CreateRectRgn(0, 0, 0, 0));
				_ = UnsafeNativeMethods.GetWindowRgn(form.Handle, windowRegion.Handle);
				UnsafeNativeMethods.GetRgnBox(windowRegion.Handle, out var box); // unpack

				// assert
				// this clip rect should be the same size as the form
				AssertEquals(
					new Rectangle(0, 0, form.Width, form.Height),
					new Rectangle(box.left, box.top, box.right - box.left, box.bottom - box.top));
			}
		}

		public void TestSetWindowRegion_Maximized()
		{
			try
			{
				CachedScreenInfo.Instance.allScreens.Clear();
				var primaryScreen = new Rectangle(0, 0, 1600, 1200);
				var secondaryScreen = new Rectangle(1600, 0, 1920, 1080);
				CachedScreenInfo.Instance.allScreens.Add(new PhysicalScreenInfoImpl { WorkingArea = secondaryScreen });
				CachedScreenInfo.Instance.allScreens.Add(new PhysicalScreenInfoImpl { WorkingArea = primaryScreen });

				using (var form = new ZBorderlessForm())
				{
					form.Visible = true;
					form.WindowState = FormWindowState.Maximized;
					form.Show();
					Application.DoEvents();

					var windowRegion = new HandleRef(this, UnsafeNativeMethods.CreateRectRgn(0, 0, 0, 0));
					_ = UnsafeNativeMethods.GetWindowRgn(form.Handle, windowRegion.Handle);
					UnsafeNativeMethods.GetRgnBox(windowRegion.Handle, out var box); // unpack

					AssertEquals(1600, box.right - box.left);
					AssertEquals(1200, box.bottom - box.top);
				}
			}
			finally
			{
				CachedScreenInfo.Instance.PopulateInfo();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1063:DoNotUseSystemWindowsFormsScreen", Justification = "Testing")]
		public void TestSetWindowRegionFullscreen()
		{
			// run this test for every screen on the system
			foreach (var screen in CachedScreenInfo.Instance.ScreenInfos)
			{
				using (var form = new ZBorderlessForm())
				{
					// arrange
					form.Location = new Point(
						screen.X + (screen.Width / 2),
						screen.Y + (screen.Height / 2));
					form.WindowState = FormWindowState.Maximized;
					form.Show();
					Application.DoEvents();

					// act
					form.SetWindowRegion(form.Handle, form.Left, form.Top, form.Right, form.Bottom);

					// assert
					var windowRegion = new HandleRef(this, UnsafeNativeMethods.CreateRectRgn(0, 0, 0, 0));
					_ = UnsafeNativeMethods.GetWindowRgn(form.Handle, windowRegion.Handle);
					UnsafeNativeMethods.GetRgnBox(windowRegion.Handle, out var box); // unpack

					// the correct position of this clip rect will be inset X and Y pixels from the top left, where X and Y are the size of the system-drawn
					// border, which will usually be 8 pixels
					var leftSystemBorderThickness = UnsafeNativeMethods.GetSystemMetrics(BFNativeConstants.SM_CXSIZEFRAME);
					var topSystemBorderThickness = UnsafeNativeMethods.GetSystemMetrics(BFNativeConstants.SM_CYSIZEFRAME);
					var correctRegion = Screen.FromControl(form).WorkingArea;
					correctRegion.X += leftSystemBorderThickness;
					correctRegion.Y += topSystemBorderThickness;

					AssertEquals(
						correctRegion,
						new Rectangle(
							box.left,
							box.top,
							box.right - box.left,
							box.bottom - box.top));
				}
			}
		}

#endif
	}
}
