using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Drawing;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Forms
{
	class ZMainFormTest : TestCase
	{
		Mock<ZMainForm> MockForm;

		#region Setup

		protected override void SetUp()
		{
			MockForm = GetFormMock();
		}

		static Mock<ZMainForm> GetFormMock()
		{
			var mockForm = new Mock<ZMainForm>();
			mockForm.CallBase = true;
			AssertNotNull(mockForm.Object); // this is required to 'initialise' the mocked object once since lazy initialisation seems to be used
			mockForm.Invocations.Clear();
			return mockForm;
		}

		protected override void TearDown()
		{
			MockForm.Object.Dispose();
		}

		#endregion

		#region Events

		protected internal class FormClosingHook
		{
			public int Count { get; set; }
			public bool ClosedProperly => Count == 2;

			public FormClosingHook(Form f)
			{
				AssertEquals("The current threads apartment state must be STA to run these tests", Thread.CurrentThread.GetApartmentState(), ApartmentState.STA);
				f.FormClosing += (a, b) => Count++;
				f.FormClosed += (a, b) => Count++;

				f.Show();
			}
		}

		public void TestAppCloseClicked()
		{
			using (var form = new ZMainForm())
			{
				// arrange
				var fch = new FormClosingHook(form);

				// act
				form.AppClose_MouseClick(null, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));

				// assert
				Assert(fch.ClosedProperly);
			}
		}

		public void TestAppMinimiseClicked()
		{
			using (var form = new ZMainForm())
			{
				// act
				form.Show();

				// assert
				AssertEquals(FormWindowState.Normal, form.WindowState);

				// act
				form.AppMinimise_MouseClick(null, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));

				// assert
				AssertEquals(FormWindowState.Minimized, form.WindowState);
			}
		}

		public void TestAppMaximiseClicked()
		{
			using (var form = new ZMainForm())
			{
				// act
				form.Show();

				// assert
				AssertEquals(FormWindowState.Normal, form.WindowState);

				// act
				form.AppMaximise_MouseClick(null, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));

				// assert
				AssertEquals(FormWindowState.Maximized, form.WindowState);

				// act
				form.AppMaximise_MouseClick(null, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));

				// assert
				AssertEquals(FormWindowState.Normal, form.WindowState);
			}
		}

#if !WINZOR

		public void TestAppIconMouseUp()
		{
			using (var form = new ZMainFormTestWndProcLogger())
			using (var timer = new System.Threading.Timer((a) => { SendKeys.SendWait("{ESC}"); }, null, 1000, 0))
			{
				// act
				form.AppIcon_MouseUp(null, new MouseEventArgs(MouseButtons.Right, 1, 0, 0, 0));

				// assert
				Assert("Sysmenu should have opened", form.WndProcMsgLog.Contains(Native.WindowMessages.WM_SYSMENU));
			}
		}

		public void TestShowSystemMenuDoesntOverflow()
		{
			using (var form = new ZMainFormTestWndProcLogger())
			{
				using (var timer = new System.Threading.Timer((a) => { SendKeys.SendWait("{ESC}"); }, null, 1000, 0))
				{
					// assert
					AssertNoExceptionThrown(() => form.ShowSystemMenu(new Point(2, 3)));
				}

				using (var timer = new System.Threading.Timer((a) => { SendKeys.SendWait("{ESC}"); }, null, 1000, 0))
				{
					// assert
					AssertNoExceptionThrown(() => form.ShowSystemMenu(new Point(-1, -2)));
				}

				using (var timer = new System.Threading.Timer((a) => { SendKeys.SendWait("{ESC}"); }, null, 1000, 0))
				{
					// assert
					AssertNoExceptionThrown(() => form.ShowSystemMenu(new Point(1280000, 10240000)));
				}
			}
		}

		public void TestAppTitleTextMouseUp()
		{
			using (var form = new ZMainFormTestWndProcLogger())
			using (var timer = new System.Threading.Timer((a) => { SendKeys.SendWait("{ESC}"); }, null, 1000, 0))
			{
				// act
				form.AppTitleText_MouseUp(null, new MouseEventArgs(MouseButtons.Right, 1, 0, 0, 0));

				// assert
				Assert("Sysmenu should have opened", form.WndProcMsgLog.Contains(Native.WindowMessages.WM_SYSMENU));
			}
		}

		public void TestAppTitleTextMouseDownDoubleClick()
		{
			using (var form = new ZMainForm())
			{
				// arrange
				form.Show();

				// act
				var message = new Message { Msg = (int)Native.WindowMessages.WM_LBUTTONDBLCLK, HWnd = form.AppTitleText.Handle };
				form.messageFilter.PreFilterMessage(ref message);
				Application.DoEvents();

				// assert
				AssertEquals("Form should be maximised", FormWindowState.Maximized, form.WindowState);

				// act
				Thread.Sleep(SystemInformation.DoubleClickTime * 2);

				// assert
				form.messageFilter.PreFilterMessage(ref message);
				Application.DoEvents();
				AssertEquals("Form should be normal", FormWindowState.Normal, form.WindowState);
			}
		}

		public void TestAppTitleTextMouseDownSingleClick()
		{
			using (var form = new ZMainFormTestWndProcLogger())
			{
				// arrange
				form.Show();

				// act
				form.AppTitleText_MouseDown(null, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));

				// assert
				Assert("Mouse down on title bar", form.WndProcMsgLog.Contains(Native.WindowMessages.WM_NCLBUTTONDOWN)); // can't check for HT_CAPTION with current test setup
			}
		}

		public void TestFormIconChanged()
		{
			using (var form = new ZMainForm())
			{
				// arrange
				var image = new Bitmap(5, 5);

				using (var i = Icon.FromHandle(image.GetHicon()))
				{
					// act
					form.Icon = i;
					form.Visible = true; // this isn't required in the real code but somehow in the test the icon isn't changed properly, and Application.DoEvents() does nothing

					// assert
					AssertNotNull(form.AppIcon.DisplayedImage);
					AssertBitmapsArePixelEqual(image, new Bitmap(form.AppIcon.DisplayedImage));
				}
			}
		}

		public void TestSetIconFromImage()
		{
			using (var form = new ZMainForm())
			{
				// arrange
				var image = new Bitmap(5, 5);

				// act
				form.SetIconFromImage(image);
				form.Visible = true; // this isn't required in the real code but somehow in the test the icon isn't changed properly, and Application.DoEvents() does nothing

				// assert
				AssertNotNull(form.AppIcon.DisplayedImage);
				AssertBitmapsArePixelEqual(image, new Bitmap(form.AppIcon.DisplayedImage));
			}
		}

		public void TestFormActivatedDeactivated()
		{
			// arrange
			var mockForm2 = GetFormMock();

			// act
			MockForm.Object.Show();
			MockForm.Object.Activate();

			// assert
			MockForm.Verify(x => x.RecalculateCurrentTheme(true), Times.Once);

			// act
			mockForm2.Object.Show();

			// assert
			MockForm.Verify(x => x.RecalculateCurrentTheme(false), Times.Once);
			mockForm2.Verify(x => x.RecalculateCurrentTheme(true), Times.Once);

			mockForm2.Object.Dispose();
		}

#endif

		public void TestFormBackColorChanged()
		{
			using (var form = new ZMainForm())
			{
				// act
				form.BackColor = Color.Chartreuse;

				// assert
				AssertEquals(Color.Chartreuse, form.Main.BackColor);
				AssertEquals(Color.Chartreuse, form.Workspace.BackColor);
			}
		}

		public void TestFormSizeChanged()
		{
			// arrange
			var mockForm = MockForm.Object;
			var panels = new[] { mockForm.TopLeftCornerPanel, mockForm.TopRightCornerPanel, mockForm.BottomLeftCornerPanel, mockForm.BottomRightCornerPanel,
			mockForm.TopBorderPanel, mockForm.LeftBorderPanel, mockForm.RightBorderPanel, mockForm.BottomBorderPanel };
			mockForm.Show();
			MockForm.Invocations.Clear();

			// act
			mockForm.Size = ControlDpiScalingHelper.NewScaledSize(1367, 769);

			// assert
			AssertEquals("maximise button doesn't show maximise icon", "1", mockForm.AppMaximise.Text);
			Assert("some panels weren't visible", panels.Select(p => p.Visible).Aggregate((a, b) => a && b));
			MockForm.Verify(x => x.ResizeMainWorkPanel(), Times.Once);
			MockForm.Verify(x => x.ResizeTitleBar(), Times.Once);

			MockForm.Invocations.Clear();

			// act
			mockForm.WindowState = FormWindowState.Maximized;

			// assert
			AssertEquals("maximise button doesn't show restore icon", "2", mockForm.AppMaximise.Text);
			Assert("some panels were visible", panels.Select(p => !p.Visible).Aggregate((a, b) => a && b));
			// sadly these are called twice, optimising it is going to be tough, we'll have play around
			// with the windows messages and intercept/not send the second message
#if WINZOR
			MockForm.Verify(x => x.ResizeMainWorkPanel(), Times.Once);
			MockForm.Verify(x => x.ResizeTitleBar(), Times.Once);
#else
			MockForm.Verify(x => x.ResizeMainWorkPanel(), Times.Exactly(2));
			MockForm.Verify(x => x.ResizeTitleBar(), Times.Exactly(2));
#endif
		}

		#endregion

		#region Themes

		public void TestApplyButtonTheme()
		{
			using (var button = new ZButton())
			{
				// arrange
				var theme = new ButtonTheme(Color.White, Color.Black, Color.Green, Color.Red, Color.Yellow, Color.Blue, 3);

				// act
				button.ApplyTheme(theme);

				// assert
				ButtonThemeTestHelpers.AssertButtonHasThemeApplied(button, theme);
			}
		}

		public void TestApplyWindowTheme()
		{
			// arrange
			var theme = new WindowTheme(Color.Blue, Color.Green, Color.Black, Color.White, Color.Red);

			// act
			MockForm.Object.ApplyWindowTheme(theme);

			// assert
			MockForm.Verify(x => x.SetBorderColour(Color.Green), Times.Once);
			MockForm.Verify(x => x.SetTextColor(Color.White), Times.Once);
			MockForm.Verify(x => x.SetTitleBarColor(Color.Red), Times.Once);
			Assert(true); // nunit thinks this is an empty test since it doesn't think Moq is a valid unit test
		}

		public void TestInitialiseWindowTheme()
		{
			using (var form = new ZMainForm())
			{
				// act
				form.InitialiseWindowTheme();

				// assert
				AssertColorEquals(Color.White, form.Main.BackColor);
				AssertColorEquals(Color.White, form.Workspace.BackColor);
				AssertEquals(ZMainForm.DefaultWindowTheme, form.WindowTheme);
			}
		}

		public void TestRefreshCurrentTheme()
		{
			// arrange
			var mockForm = MockForm.Object;

			// act
			mockForm.RecalculateCurrentTheme(true);

			// assert (active)
			MockForm.Verify(x => x.ApplyWindowTheme(mockForm.WindowTheme), Times.Once);
			ButtonThemeTestHelpers.AssertButtonHasThemeApplied(mockForm.AppMinimise, mockForm.StandardButtonTheme);
			ButtonThemeTestHelpers.AssertButtonHasThemeApplied(mockForm.AppMaximise, mockForm.StandardButtonTheme);
			ButtonThemeTestHelpers.AssertButtonHasThemeApplied(mockForm.AppClose, mockForm.CloseButtonTheme);

			// assert (inactive)
			mockForm.RecalculateCurrentTheme(false);
			MockForm.Verify(x => x.ApplyWindowTheme(mockForm.WindowTheme), Times.Once);
			ButtonThemeTestHelpers.AssertButtonHasThemeApplied(mockForm.AppMinimise, mockForm.StandardButtonTheme.AsInactive());
			ButtonThemeTestHelpers.AssertButtonHasThemeApplied(mockForm.AppMaximise, mockForm.StandardButtonTheme.AsInactive());
			ButtonThemeTestHelpers.AssertButtonHasThemeApplied(mockForm.AppClose, mockForm.CloseButtonTheme.AsInactive());
		}

		public void TestSetTextColor()
		{
			using (var form = new ZMainForm())
			{
				// act
				form.SetTextColor(Color.White);

				// assert
				AssertEquals(Color.White, form.AppTitleText.ForeColor);
			}
		}

		public void TestSetTitleBarColor()
		{
			using (var form = new ZMainForm())
			{
				// act
				form.SetTitleBarColor(Color.White);

				// assert
				AssertEquals(Color.White, form.AppTitleText.BackColor);
				AssertEquals(Color.White, form.AppIcon.BackColor);
				AssertEquals(Color.White, form.TitleBar.BackColor);
			}
		}

		public void TestWindowThemeSetter()
		{
			// arrange
			var theme = new WindowTheme(Color.Blue, Color.Green, Color.Black, Color.White, Color.Red);

			// act
			MockForm.Object.WindowTheme = theme;

			// assert
			AssertEquals(theme, MockForm.Object.WindowTheme);
			ButtonThemeTestHelpers.AssertButtonThemesEqual(new ButtonTheme(Color.White, Color.Red), MockForm.Object.StandardButtonTheme);
			ButtonThemeTestHelpers.AssertButtonThemesEqual(new ButtonTheme(Color.White, Color.Black), MockForm.Object.CloseButtonTheme);
			MockForm.Verify(x => x.RecalculateCurrentTheme(true), Times.Once);
		}

		#endregion

		#region GUI

		public void TestButtonFlatStyleIsCorrect()
		{
			using (var form = new ZMainForm())
			{
				AssertEquals(FlatStyle.Flat, form.AppMinimise.FlatStyle);
				AssertEquals(FlatStyle.Flat, form.AppMaximise.FlatStyle);
				AssertEquals(FlatStyle.Flat, form.AppClose.FlatStyle);
			}
		}

		public void TestAppTitleBarTextAlignIsCorrect()
		{
			using (var form = new ZMainForm())
			{
#if WINZOR
				AssertEquals(ContentAlignment.TopCenter, form.AppTitleText.TextAlign);
#else
				AssertEquals(ContentAlignment.MiddleCenter, form.AppTitleText.TextAlign);
#endif
			}
		}

		#endregion

		#region Size

		public void TestResizeMainWorkPanelMaximised()
		{
			using (var form = new ZMainForm())
			{
				// arrange
				form.WindowState = FormWindowState.Maximized;

				// act
				form.ResizeMainWorkPanel();

				// assert
				AssertEquals(DockStyle.Fill, form.Main.Dock);
			}
		}

		public void TestResizeMainWorkPanelMinimised()
		{
			using (var form = new ZMainForm())
			{
				// arrange
				form.WindowState = FormWindowState.Minimized;

				// act
				form.ResizeMainWorkPanel();

				// assert
				AssertEquals(DockStyle.None, form.Main.Dock);
				AssertEquals(ControlDpiScalingHelper.NewScaledSize(792, 472, true), form.Main.Size);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(4, 4, true), form.Main.Location); // (4, 4) is the border offset
			}
		}

		public void TestResizeMainWorkPanelNormal()
		{
			using (var form = new ZMainForm())
			{
				// arrange
				form.WindowState = FormWindowState.Normal;

				// act
				form.ResizeMainWorkPanel();

				// assert
				AssertEquals(DockStyle.None, form.Main.Dock);
				AssertEquals(ControlDpiScalingHelper.NewScaledSize(792, 472, true), form.Main.Size);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(4, 4, true), form.Main.Location); // (4, 4) is the border offset
			}
		}

		public void TestResizeTitleBar()
		{
			using (var form = new ZMainForm())
			{
				// act
				form.ResizeTitleBar();

				// assert
				AssertEquals(form.AppMinimise.Width, form.AppMinimise.Height);
				AssertEquals(form.AppMaximise.Width, form.AppMaximise.Height);
				AssertEquals(form.AppClose.Width, form.AppClose.Height);
				AssertEquals(form.AppIcon.Width, form.AppIcon.Height);
				AssertEquals(ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 4, true), form.AppIcon.Padding);
			}
		}

		#endregion

		#region Functionality

		public void AssertBitmapsArePixelEqual(Bitmap a, Bitmap b)
		{
			Assert("Images aren't equal", CompareBitmaps(a, b));
		}

		[DllImport("msvcrt.dll")]
		static extern int memcmp(IntPtr b1, IntPtr b2, long count);

		// https://stackoverflow.com/questions/2031217/what-is-the-fastest-way-i-can-compare-two-equal-size-bitmaps-to-determine-whethe
		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "image comparison, nothing to do with DPI scaling")]
		static bool CompareBitmaps(Bitmap b1, Bitmap b2)
		{
			if ((b1 == null) != (b2 == null))
			{
				return false;
			}
			if (b1.Size != b2.Size)
			{
				return false;
			}

			var bd1 = b1.LockBits(new Rectangle(new Point(0, 0), b1.Size), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			var bd2 = b2.LockBits(new Rectangle(new Point(0, 0), b2.Size), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			try
			{
				var bd1scan0 = bd1.Scan0;
				var bd2scan0 = bd2.Scan0;

				var stride = bd1.Stride;
				var len = stride * b1.Height;

				return memcmp(bd1scan0, bd2scan0, len) == 0;
			}
			finally
			{
				b1.UnlockBits(bd1);
				b2.UnlockBits(bd2);
			}
		}

		#endregion
	}
}
