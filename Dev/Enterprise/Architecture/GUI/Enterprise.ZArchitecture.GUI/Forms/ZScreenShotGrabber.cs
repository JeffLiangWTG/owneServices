using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	public static class ZScreenShotGrabber
	{
		public static Bitmap Capture(Control ctrl)
		{
			var result = CaptureWindow(ctrl.Handle);
			if (result == null)
			{
				result = new Bitmap(ctrl.Width, ctrl.Height);
				//Not used by default because Control.DrawToBitmap function has rendering bugs
				ctrl.DrawToBitmap(result, ControlDpiScalingHelper.NewScaledRectangle(0, 0, ctrl.Width, ctrl.Height, false));
			}
			return result;
		}

		public static Bitmap CaptureSection(IntPtr windowHandle, Point location, Size size)
		{
			Bitmap result = null;
			try
			{
				result = new Bitmap(size.Width, size.Height);

				using (var g = Graphics.FromImage(result))
				{
					var destDeviceContext = g.GetHdc();
					var srcDeviceContext = User32.GetWindowDC(windowHandle);

					GDI32.BitBlt(destDeviceContext, 0, 0, size.Width, size.Height, srcDeviceContext, location.X, location.Y, GDI32.SRCCOPY);

					User32.ReleaseDC(windowHandle, srcDeviceContext);
					g.ReleaseHdc(destDeviceContext);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException()) { }

			return result;
		}

		/// <summary>
		/// Never throws exceptions - all are suppressed
		/// </summary>
		/// <param name="handle"></param>
		/// <returns></returns>
		public static Bitmap CaptureWindow(IntPtr handle)
		{
			Bitmap result = null;
			try
			{
				result = CaptureWindowRaw(handle);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
			}
			return result;
		}

		/// <summary>
		/// Will throw exceptions
		/// </summary>
		/// <param name="handle"></param>
		/// <returns></returns> 
		internal static Bitmap CaptureWindowRaw(IntPtr handle)
		{
			var hdcSrc = User32.GetWindowDC(handle);

			var windowRect = new User32.RECT();
			User32.GetWindowRect(handle, ref windowRect);
			var width = windowRect.right - windowRect.left;
			var height = windowRect.bottom - windowRect.top;

			var hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
			var hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
			var hOld = GDI32.SelectObject(hdcDest, hBitmap);

			GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
			GDI32.SelectObject(hdcDest, hOld);

			Bitmap bmp = null;
			try
			{
				bmp = Bitmap.FromHbitmap(hBitmap);
			}
			finally
			{
				GDI32.DeleteObject(hBitmap);
				GDI32.DeleteDC(hdcDest);
				User32.ReleaseDC(handle, hdcSrc);
			}

			return bmp;
		}

		static class GDI32
		{
			public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter
			[DllImport("gdi32.dll")]
			public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
				int nWidth, int nHeight, IntPtr hObjectSource,
				int nXSrc, int nYSrc, int dwRop);
			[DllImport("gdi32.dll")]
			public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
				int nHeight);
			[DllImport("gdi32.dll")]
			public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

			internal static class SafeNativeMethods
			{
				[return: MarshalAs(UnmanagedType.Bool)]
				[DllImport("gdi32.dll")]
				internal static extern bool DeleteDC(IntPtr hDC);
			}

			public static void DeleteDC(IntPtr hDC)
			{
				var result = SafeNativeMethods.DeleteDC(hDC);
				if (!result)
				{
					var e = new ErrorWrapper(result);
					var ex = new Win32Exception("ZScreenGrabber.cs and DeleteDC(IntPtr, IntPtr) returned an error. \n" + e.ToString());
					ErrorReporter.ReportOnce("", ex);
				}
			}

			[DllImport("gdi32.dll")]
			public static extern bool DeleteObject(IntPtr hObject);
			[DllImport("gdi32.dll")]
			public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
		}

		static class User32
		{
			[StructLayout(LayoutKind.Sequential)]
			public struct RECT
			{
				public int left;
				public int top;
				public int right;
				public int bottom;
			}
			[DllImport("user32.dll")]
			public static extern IntPtr GetDesktopWindow();
			[DllImport("user32.dll")]
			public static extern IntPtr GetWindowDC(IntPtr hWnd);

			internal static class SafeNativeMethods
			{
				[DllImport("user32.dll")]
				internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

				[DllImport("user32.dll")]
				internal static extern int GetWindowRect(IntPtr hWnd, ref RECT rect);
			}

			public static void ReleaseDC(IntPtr hWnd, IntPtr hDC)
			{
				var result = SafeNativeMethods.ReleaseDC(hWnd, hDC);
				if (result == 0)
				{
					var e = new ErrorWrapper(result);
					var ex = new Win32Exception("ZScreenGrabber.cs and ReleaseDC(IntPtr, IntPtr) returned an error. \n" + e.ToString());
					ErrorReporter.ReportOnce("", ex);
				}
			}

			public static void GetWindowRect(IntPtr hWnd, ref RECT rect)
			{
				var result = SafeNativeMethods.GetWindowRect(hWnd, ref rect);
				if (result == 0)
				{
					var e = new ErrorWrapper(result);
					var ex = new Win32Exception("ZScreenGrabber.cs and GetWindowRect(IntPtr, ref RECT) returned an error. \n" + e.ToString());
					ErrorReporter.ReportOnce("", ex);
				}
			}
		}
	}
}
