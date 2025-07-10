using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using CargoWise.Common;
using CargoWise.Interop;

namespace CargoWise.Windows.UI
{
	public class ONativeWindow
	{
		protected ONativeWindow(IntPtr handle)
		{
			this.Handle = handle;
		}

		#region Factory Methods

		public static ONativeWindow FromHandle(IntPtr handle)
		{
			return handle == IntPtr.Zero ? null : new ONativeWindow(handle);
		}

		public static ONativeWindow ActiveTopLevelWindow
		{
			get
			{
				ONativeWindow result;
				if (ActiveTopLevelWindowForTest != IntPtr.Zero)
				{
					result = ONativeWindow.FromHandle(ActiveTopLevelWindowForTest);
				}
				else
				{
					result = ONativeWindow.FromHandle(GetForegroundWindow());
				}
				return result;
			}
		}

		public static ONativeWindow FindForm(string lpClassName, string lpWindowName)
		{
			return ONativeWindow.FromHandle(FindWindow(lpClassName, lpWindowName));
		}

		[ThreadStatic]
		public static IntPtr ActiveTopLevelWindowForTest;

		#endregion

		#region GetAllTopLevelWindows

		public static ONativeWindow[] GetAllOutOfProcessTopLevelWindows()
		{
			ArrayList result = new ArrayList();
			foreach (ONativeWindow window in GetAllTopLevelWindows())
			{
				if (window.GetWindowProcessId() != Process.GetCurrentProcess().Id)
				{
					result.Add(window);
				}
			}
			return (ONativeWindow[])result.ToArray(typeof(ONativeWindow));
		}

		[ThreadStatic]
		static ArrayList GetAllTopLevelWindowsResult;
		public static ONativeWindow[] GetAllTopLevelWindows()
		{
			GetAllTopLevelWindowsResult = new ArrayList();
			EnumWindows(new EnumDelegate(GetAllTopLevelWindowHandlesCallback), IntPtr.Zero);

			ArrayList result = new ArrayList();
			foreach (IntPtr handle in GetAllTopLevelWindowsResult)
			{
				result.Add(ONativeWindow.FromHandle(handle));
			}
			return (ONativeWindow[])result.ToArray(typeof(ONativeWindow));
		}

		static bool GetAllTopLevelWindowHandlesCallback(IntPtr hWnd, IntPtr lParam)
		{
			GetAllTopLevelWindowsResult.Add(hWnd);
			return true;
		}

		#endregion

		#region Operator Overloads

		public static bool operator ==(ONativeWindow lhs, ONativeWindow rhs)
		{
			return
				((object)lhs == null && (object)rhs == null) ||
				((object)lhs != null && (object)rhs != null && lhs.Handle == rhs.Handle);
		}

		public static bool operator !=(ONativeWindow lhs, ONativeWindow rhs)
		{
			return !(lhs == rhs);
		}

		public override bool Equals(object obj)
		{
			return obj is ONativeWindow && this.Handle == ((ONativeWindow)obj).Handle;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return Handle.ToInt32();
			}
		}

		#endregion

		#region Visible / Enabled / Focus

		public bool Visible
		{
			get { return SafeNativeMethods.IsWindowVisible(new HandleRef(this, Handle)); }
		}

		public bool Enabled
		{
			get { return SafeNativeMethods.IsWindowEnabled(new HandleRef(this, Handle)); }
		}

		public void Focus()
		{
			SetFocus(new HandleRef(this, Handle));
		}

		#endregion

		#region Bounds

		public Rectangle Bounds
		{
			get
			{
				Rectangle result = new Rectangle();
				WindowInfo info = new WindowInfo();
				if (Handle != IntPtr.Zero &&
					GetWindowInfo(new HandleRef(this, Handle), ref info))
				{
					result = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledRectangle(
						info.rcWindow.Left, info.rcWindow.Top,
						info.rcWindow.Right - info.rcWindow.Left, info.rcWindow.Bottom - info.rcWindow.Top,
						false); // Coordinates are already in physical sizes (i.e. scaled)
				}
				return result;
			}
		}

		#endregion

		#region Text

		public string Text
		{
			get
			{
				StringBuilder sb = new StringBuilder(512);

				StringBuilder stringBuilder = new StringBuilder(4000);
				GetWindowText(Handle, stringBuilder, 4000);
				return stringBuilder.ToString();
			}
			set
			{
				SetWindowText(Handle, value);
			}
		}

		#endregion

		#region GetWindowStyle / SetWindowStyle

		public bool GetWindowStyle(int windowStyleFlag)
		{
			int windowLong = (int)((long)UnsafeNativeMethods.GetWindowLongPtr(new HandleRef(this, this.Handle), -16));
			return (windowLong & windowStyleFlag) == windowStyleFlag;
		}

		public void SetWindowStyle(int windowStyleFlag, bool value)
		{
			int windowLong = (int)((long)UnsafeNativeMethods.GetWindowLongPtr(new HandleRef(this, this.Handle), -16));
#if NET8_0_OR_GREATER
			nint handle = value ? (windowLong | windowStyleFlag) : (windowLong & ~windowStyleFlag);
#else // .NET Framework
			IntPtr handle = value ? ((IntPtr)(windowLong | windowStyleFlag)) : ((IntPtr)(windowLong & ~windowStyleFlag));
#endif
			int code = UnsafeNativeMethods.SetWindowLongPtr(new HandleRef(this, this.Handle), -16, (int)(IntPtr)new HandleRef(null, handle));
			if (code == 0)
			{
				var wrapp = new ErrorWrapper(code);
				var error = new Win32Exception("Native method SetWindowLongPtr returned an error: " + wrapp.ErrorCode);
				ErrorReporter.ReportOnce("", error);
			}
		}

		#endregion

		#region IsActiveTopLevelWindow / GetWindowProcessId

		public bool IsActiveTopLevelWindow
		{
			get { return (Handle == ActiveTopLevelWindowForTest) || GetForegroundWindow() == Handle; }
			set { SetForegroundWindow(new HandleRef(this, Handle)); }
		}

		public uint GetWindowProcessId()
		{
			UIntPtr result = UIntPtr.Zero;
			GetWindowThreadProcessId(Handle, result);
			unchecked
			{
				return result.ToUInt32();
			}
		}

		#endregion

		#region ChildWindows

		readonly ArrayList ChildWindowsResult = new ArrayList();
		public ONativeWindow[] ChildWindows
		{
			get
			{
				EnumChildWindows(Handle, new EnumDelegate(ChildWindowsCallback), IntPtr.Zero);
				ArrayList result = new ArrayList();
				foreach (IntPtr childHandle in ChildWindowsResult)
				{
					result.Add(ONativeWindow.FromHandle(childHandle));
				}
				ChildWindowsResult.Clear();
				return (ONativeWindow[])result.ToArray(typeof(ONativeWindow));
			}
		}

		bool ChildWindowsCallback(IntPtr hWnd, IntPtr lParam)
		{
			ChildWindowsResult.Add(hWnd);
			return true;
		}

		#endregion

		#region Implementation

		public readonly IntPtr Handle;

		#region Unmanaged Methods

		internal static class NativeMethods
		{
			[DllImport("user32.dll")]
			public static extern int GetWindowThreadProcessId(IntPtr hWnd, UIntPtr lpdwProcessId);
		}

		public static void GetWindowThreadProcessId(IntPtr hWnd, UIntPtr lpdwProcessId)
		{
			int result = NativeMethods.GetWindowThreadProcessId(hWnd, lpdwProcessId);
			if (result != 0)
			{
				var exception = new ErrorWrapper(result).ToString();
				var e = new Win32Exception(exception);
				ErrorReporter.ReportOnce("", e);
			}
		}

		[DllImport("user32.dll")]
		static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
		static extern bool SetForegroundWindow(HandleRef hWnd);

		[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
		static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int GetWindowTextLength(HandleRef hWnd);

		[DllImport("user32.dll")]
		static extern int GetWindowText(IntPtr hWndParent, StringBuilder sb, int maxCount);

		[DllImport("user32.dll")]
		static extern int SetWindowText(IntPtr hWndParent, string str);

		[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
		static extern bool GetWindowInfo(HandleRef hWnd, ref WindowInfo info);

		[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
		public static extern IntPtr SetFocus(HandleRef hWnd);

		[DllImport("user32.dll")]
		static extern bool EnumWindows(EnumDelegate lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll")]
		static extern bool EnumChildWindows(IntPtr hWndParent, EnumDelegate lpEnumFunc, IntPtr lParam);

		delegate bool EnumDelegate(IntPtr hWnd, IntPtr lParam);

		public struct WindowInfo
		{
			public int cbSize;
			public RECT rcWindow;
			public RECT rcClient;
			public int dwStyle;
			public int dwExStyle;
			public int dwWindowStatus;
			public int cxWindowBorders;
			public int cyWindowBorders;
			public short atomWindowType;
			public short wCreatorVersion;
		}

		#endregion

		#endregion
	}
}
