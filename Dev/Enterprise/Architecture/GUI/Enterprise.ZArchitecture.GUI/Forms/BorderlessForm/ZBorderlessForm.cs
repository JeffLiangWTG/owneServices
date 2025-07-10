using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI.Forms.Native;

namespace Enterprise.ZArchitecture.GUI.Forms
{
	// Implements a borderless window (ie no system-drawn borders, we draw our own with panels)
	public partial class ZBorderlessForm : KForm
	{
		public ZBorderlessForm()
		{
			InitializeComponent();

			TopLeftCornerPanel.MouseDown += (s, e) => FormBorderMouseDown(e, HitTestValues.HTTOPLEFT);
			TopRightCornerPanel.MouseDown += (s, e) => FormBorderMouseDown(e, HitTestValues.HTTOPRIGHT);
			BottomLeftCornerPanel.MouseDown += (s, e) => FormBorderMouseDown(e, HitTestValues.HTBOTTOMLEFT);
			BottomRightCornerPanel.MouseDown += (s, e) => FormBorderMouseDown(e, HitTestValues.HTBOTTOMRIGHT);

			TopBorderPanel.MouseDown += (s, e) => FormBorderMouseDown(e, HitTestValues.HTTOP);
			LeftBorderPanel.MouseDown += (s, e) => FormBorderMouseDown(e, HitTestValues.HTLEFT);
			RightBorderPanel.MouseDown += (s, e) => FormBorderMouseDown(e, HitTestValues.HTRIGHT);
			BottomBorderPanel.MouseDown += (s, e) => FormBorderMouseDown(e, HitTestValues.HTBOTTOM);

			ResizeBorders();
		}

#if !WINZOR
		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			if (!DesignMode)
			{
				SetWindowRegion(Handle, 0, 0, Width, Height);
			}
		}
#endif

		[Browsable(true)]
		[Description("Occurs after the icon is changed")]
		[SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Justification = "CA rule is incorrect")]
		public event EventHandler<IconType> IconChanged;

		[Description("Sets the thickness of the borders for this form")]
		[DpiState(DpiState.ScaleX)]
		public int BorderSize { get; set; } = ControlDpiScalingHelper.ScaleToCurrentDpiX(4);

		protected internal virtual void SetBorderColour(Color color)
		{
			TopLeftCornerPanel.BackColor = color;
			TopBorderPanel.BackColor = color;
			TopRightCornerPanel.BackColor = color;
			LeftBorderPanel.BackColor = color;
			RightBorderPanel.BackColor = color;
			BottomLeftCornerPanel.BackColor = color;
			BottomBorderPanel.BackColor = color;
			BottomRightCornerPanel.BackColor = color;
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "All values already scaled")]
		protected internal void ResizeBorders()
		{
			TopLeftCornerPanel.Size = ControlDpiScalingHelper.NewScaledSize(BorderSize, BorderSize, false);
			TopRightCornerPanel.Size = ControlDpiScalingHelper.NewScaledSize(BorderSize, BorderSize, false);
			BottomLeftCornerPanel.Size = ControlDpiScalingHelper.NewScaledSize(BorderSize, BorderSize, false);
			BottomRightCornerPanel.Size = ControlDpiScalingHelper.NewScaledSize(BorderSize, BorderSize, false);
			TopBorderPanel.Size = ControlDpiScalingHelper.NewScaledSize(Width - BorderSize * 2, BorderSize, false);
			LeftBorderPanel.Size = ControlDpiScalingHelper.NewScaledSize(BorderSize, Height - BorderSize * 2, false);
			RightBorderPanel.Size = ControlDpiScalingHelper.NewScaledSize(BorderSize, Height - BorderSize * 2, false);
			BottomBorderPanel.Size = ControlDpiScalingHelper.NewScaledSize(Width - BorderSize * 2, BorderSize, false);

			TopLeftCornerPanel.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0, false);
			TopRightCornerPanel.Location = ControlDpiScalingHelper.NewScaledPoint(Width - BorderSize, 0, false);
			BottomLeftCornerPanel.Location = ControlDpiScalingHelper.NewScaledPoint(0, Height - BorderSize, false);
			BottomRightCornerPanel.Location = ControlDpiScalingHelper.NewScaledPoint(Width - BorderSize, Height - BorderSize, false);
			TopBorderPanel.Location = ControlDpiScalingHelper.NewScaledPoint(BorderSize, 0, false);
			LeftBorderPanel.Location = ControlDpiScalingHelper.NewScaledPoint(0, BorderSize, false);
			RightBorderPanel.Location = ControlDpiScalingHelper.NewScaledPoint(Width - BorderSize, BorderSize, false);
			BottomBorderPanel.Location = ControlDpiScalingHelper.NewScaledPoint(BorderSize, Height - BorderSize, false);
		}

		protected internal void ShowSystemMenu()
		{
			ShowSystemMenu(MousePosition);
		}

		protected internal void ShowSystemMenu(Point pos)
		{
			SafeNativeMethods.SendMessage(Handle, (int)WindowMessages.WM_SYSMENU, 0, PointToLParam(pos));
		}

		protected internal static int PointToLParam(Point pos)
		{
			try
			{
				var lowPart = (short)pos.X;
				var highPart = (short)pos.Y;
				return (int)(((ushort)lowPart) | (uint)(highPart << 16));
			}
			catch (ArithmeticException)
			{
				return 0;
			}
		}

		#region Form Border Events

		void ZBorderlessForm_SizeChanged(object sender, EventArgs e)
		{
			ResizeBorders();
		}

		protected void FormTitleMouseDown(MouseEventArgs e)
		{
			FormBorderMouseDown(e, HitTestValues.HTCAPTION);
		}

		void FormBorderMouseDown(MouseEventArgs e, HitTestValues h)
		{
			if (e.Button == MouseButtons.Left)
			{
				FormBorderMouseDown(h);
			}
		}

		void FormBorderMouseDown(HitTestValues hit, Point p)
		{
			UnsafeNativeMethods.ReleaseCapture();
			var pt = new POINTS { X = (short)p.X, Y = (short)p.Y };
			SafeNativeMethods.SendMessage(Handle, (int)WindowMessages.WM_NCLBUTTONDOWN, (int)hit, pt);
		}

		void FormBorderMouseDown(HitTestValues hit)
		{
			FormBorderMouseDown(hit, MousePosition);
		}

		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "This should probably be connected, see WI00619404.")]
		void FormBorderMouseUp(HitTestValues hit, Point p)
		{
			UnsafeNativeMethods.ReleaseCapture();
			var pt = new POINTS { X = (short)p.X, Y = (short)p.Y };
			SafeNativeMethods.SendMessage(Handle, (int)WindowMessages.WM_NCLBUTTONUP, (int)hit, pt);
		}

		#endregion

		#region Windows Message Handling

#if !WINZOR

		protected override void WndProc(ref Message m)
		{
			if (DesignMode)
			{
				base.WndProc(ref m);
				return;
			}

			switch (m.Msg)
			{
				case (int)WindowMessages.WM_NCCALCSIZE:
					{
						WmNCCalcSize(ref m);
						break;
					}
				case (int)WindowMessages.WM_NCACTIVATE:
					{
						WmNCActivate(ref m);
						break;
					}
				case (int)WindowMessages.WM_WINDOWPOSCHANGED:
					{
						WmWindowPosChanged(ref m);
						break;
					}
				case (int)WindowMessages.WM_SETICON:
					{
						base.WndProc(ref m);
						WmSetIcon(ref m);
						break;
					}
				default:
					{
						base.WndProc(ref m);
						break;
					}
			}
		}

		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "m")]
		void WmSetIcon(ref Message m)
		{
			IconChanged?.Invoke(m, (IconType)m.WParam);
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "The middle point calculation here doesnt need to consider DPI.")]
		internal void SetWindowRegion(IntPtr hwnd, int left, int top, int right, int bottom)
		{
			if (WindowState == FormWindowState.Maximized)
			{
				// when maximised, clip the window region to remove the window border, only necessary on multi-screen setups
				// NOTE: this only clips the *render* region - form.Bounds will NOT change from this (ie form.Bounds will still extend offscreen whilst maximised)
				var middlePoint = new Point(Location.X + Width / 2, Location.Y + Height / 2);
				var cachedScreenArea = CachedScreenInfo.Instance.getCurrentScreen(middlePoint);
				left = UnsafeNativeMethods.GetSystemMetrics(BFNativeConstants.SM_CXSIZEFRAME);
				right = cachedScreenArea.Width + left;
				top = UnsafeNativeMethods.GetSystemMetrics(BFNativeConstants.SM_CYSIZEFRAME);
				bottom = cachedScreenArea.Height + top;
			}

			var hrg = new HandleRef(this, UnsafeNativeMethods.CreateRectRgn(0, 0, 0, 0));
			// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowrgn
			UnsafeNativeMethods.GetWindowRgn(hwnd, hrg.Handle);
			UnsafeNativeMethods.GetRgnBox(hrg.Handle, out var box); // unpack ptr to rect into rect
			if (box.left != left || box.top != top || box.right != right || box.bottom != bottom)
			{
				var hr = new HandleRef(this, UnsafeNativeMethods.CreateRectRgn(left, top, right, bottom));
				// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowrgn
				UnsafeNativeMethods.SetWindowRgn(hwnd, hr.Handle, UnsafeNativeMethods.IsWindowVisible(hwnd));
			}
			UnsafeNativeMethods.DeleteObject(hrg.Handle);
		}

		// based on https://referencesource.microsoft.com/#system.windows.forms/winforms/Managed/System/WinForms/Form.cs,3f7781372ea29f83
		void WmWindowPosChanged(ref Message m)
		{
			this.UpdateWindowState();
			DefWndProc(ref m);

			var pos = (WINDOWPOS)Marshal.PtrToStructure(m.LParam, typeof(WINDOWPOS));
			SetWindowRegion(m.HWnd, 0, 0, pos.cx, pos.cy);
			UpdateBounds();
		}

		// only necessary to handle when going maximised
		void WmNCCalcSize(ref Message m)
		{
			// https://docs.microsoft.com/en-us/windows/desktop/winmsg/wm-nccalcsize
			var r = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT));

			if (WindowState == FormWindowState.Maximized)
			{
				var x = UnsafeNativeMethods.GetSystemMetrics(BFNativeConstants.SM_CXSIZEFRAME);
				var y = UnsafeNativeMethods.GetSystemMetrics(BFNativeConstants.SM_CYSIZEFRAME);
				var p = UnsafeNativeMethods.GetSystemMetrics(BFNativeConstants.SM_CXPADDEDBORDER);
				var w = x + p;
				var h = y + p;

				r.left += w;
				r.top += h;
				r.right -= w;
				r.bottom -= h;

				var appBarData = new APPBARDATA();
				appBarData.cbSize = Marshal.SizeOf(typeof(APPBARDATA));
				var autohide = (UnsafeNativeMethods.SHAppBarMessage(BFNativeConstants.ABM_GETSTATE, ref appBarData) & BFNativeConstants.ABS_AUTOHIDE) != 0;
				if (autohide)
				{
					r.bottom -= 1;
				}

				Marshal.StructureToPtr(r, m.LParam, true);
			}

			m.Result = IntPtr.Zero;
		}

		void WmNCActivate(ref Message msg)
		{
			//https://docs.microsoft.com/en-us/windows/desktop/winmsg/wm-ncactivate

			if (WindowState == FormWindowState.Minimized)
			{
				DefWndProc(ref msg);
			}
			else
			{
				// allow to deactivate window
				msg.Result = BFNativeConstants.TRUE;
			}
		}

#endif

		#endregion
	}

	// these are necessary because in winforms, Form.cs has a lot of private methods that subclasses need to call
	// but obviously cannot access. this gets around that.
	public static class ZBorderlessFormExtensions
	{
		public static void UpdateWindowState(this ZBorderlessForm form)
		{
			DynamicMethod(form, "UpdateWindowState", Array.Empty<object>());
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static readonly Type formType = new Form().GetType(); // WinForms Form is needed here
		static void DynamicMethod(ZBorderlessForm form, string methodName, object[] @params)
		{
			formType
				.GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				.Invoke(form, @params);
		}
	}
}
