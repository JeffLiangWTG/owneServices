using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using CargoWise.Common.Testing;

namespace CargoWise.Windows.UI.Interop
{
	public sealed class KNativeWindow
	{
		KNativeWindow(IntPtr handle)
		{
			this.Handle = handle;
		}

		public static KNativeWindow FromHandle(IntPtr handle)
		{ return handle == IntPtr.Zero ? null : new KNativeWindow(handle); }

		public IntPtr Handle { get; private set; }

		#region Factory Methods / Properties

		public static KNativeWindow[] GetAllOutOfProcessTopLevelWindows()
		{
			List<KNativeWindow> result = new List<KNativeWindow>();
			foreach (KNativeWindow window in GetAllTopLevelWindows())
			{
				if (window.GetWindowProcessId() != Process.GetCurrentProcess().Id)
				{
					result.Add(window);
				}
			}
			return result.ToArray();
		}

		public static KNativeWindow[] GetAllTopLevelWindows()
		{
			getAllTopLevelWindowsHandle = new List<IntPtr>();
			UnsafeNativeMethods.EnumWindows(new NativeMethods.EnumCallback(GetAllTopLevelWindowHandlesCallback), IntPtr.Zero);

			List<KNativeWindow> result = new List<KNativeWindow>();
			foreach (IntPtr next in getAllTopLevelWindowsHandle)
			{
				result.Add(KNativeWindow.FromHandle(next));
			}
			return result.ToArray();
		}
		[ThreadStatic]
		static List<IntPtr> getAllTopLevelWindowsHandle;

		static bool GetAllTopLevelWindowHandlesCallback(IntPtr handle, IntPtr lParam)
		{
			getAllTopLevelWindowsHandle.Add(handle);
			return true;
		}

		public static KNativeWindow FindForm(string lpClassName, string lpWindowName)
		{ return KNativeWindow.FromHandle(UnsafeNativeMethods.FindWindow(lpClassName, lpWindowName)); }

#if DEBUG
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
		[SuppressThreadStaticFieldMessage]
		public static IntPtr ActiveTopLevelWindowForTest { get; set; }
#endif
		public static KNativeWindow ActiveTopLevelWindow
		{
			get
			{
				KNativeWindow handle;
#if DEBUG
				if (ActiveTopLevelWindowForTest != IntPtr.Zero)
				{
					handle = KNativeWindow.FromHandle(ActiveTopLevelWindowForTest);
				}
				else
#endif
				{
					handle = KNativeWindow.FromHandle(UnsafeNativeMethods.GetForegroundWindow());
				}
				return handle;
			}
		}

		#endregion

		#region Operator Overloads

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs")]
		public static bool operator ==(KNativeWindow lhs, KNativeWindow rhs)
		{
			return
				((object)lhs == null && (object)rhs == null) ||
				((object)lhs != null && (object)rhs != null && lhs.Handle == rhs.Handle);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs")]
		public static bool operator !=(KNativeWindow lhs, KNativeWindow rhs)
		{
			return !(lhs == rhs);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
		public override bool Equals(object obj)
		{
			KNativeWindow rhs = obj as KNativeWindow;
			return rhs != null && Handle == rhs.Handle;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
		public override int GetHashCode()
		{
			unchecked
			{
				return Handle.ToInt32();
			}
		}

		#endregion

		#region ChildWindows

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public KNativeWindow[] ChildWindows
		{
			get
			{
				UnsafeNativeMethods.EnumChildWindows(new HandleRef(this, Handle), new NativeMethods.EnumCallback(ChildWindowsCallback), new HandleRef(this, IntPtr.Zero));
				List<KNativeWindow> result = new List<KNativeWindow>();
				foreach (IntPtr childHandle in childWindowsHandle)
				{
					result.Add(KNativeWindow.FromHandle(childHandle));
				}
				childWindowsHandle.Clear();
				return result.ToArray();
			}
		}
		readonly List<IntPtr> childWindowsHandle = new List<IntPtr>();

		bool ChildWindowsCallback(IntPtr handle, IntPtr lParam)
		{
			childWindowsHandle.Add(handle);
			return true;
		}

		#endregion

		#region Enabled / Visible

		public bool Enabled
		{ get { return SafeNativeMethods.IsWindowEnabled(new HandleRef(this, Handle)); } }

		public bool Focused
		{ get { return UnsafeNativeMethods.GetFocus() == Handle; } }

		public bool Visible
		{ get { return SafeNativeMethods.IsWindowVisible(new HandleRef(this, Handle)); } }

		#endregion

		public string Text
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "CargoWise.Windows.UI.Interop.UnsafeNativeMethods.GetWindowText(System.IntPtr,System.Text.StringBuilder,System.Int32)")]
			get
			{
				StringBuilder result = new StringBuilder(4000);
				UnsafeNativeMethods.GetWindowText(Handle, result, 4000);
				return result.ToString();
			}
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "CargoWise.Windows.UI.Interop.UnsafeNativeMethods.SetWindowText(System.IntPtr,System.String)")]
			set { UnsafeNativeMethods.SetWindowText(Handle, value); }
		}

		public void Focus()
		{ UnsafeNativeMethods.SetFocus(new HandleRef(this, Handle)); }

		public bool IsActiveTopLevelWindow
		{
			get
			{
				return
#if DEBUG
					(Handle == ActiveTopLevelWindowForTest) ||
#endif
					UnsafeNativeMethods.GetForegroundWindow() == Handle;
			}
		}

		public void Activate()
		{ UnsafeNativeMethods.SetForegroundWindow(new HandleRef(this, Handle)); }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public int GetWindowProcessId()
		{
			IntPtr result = IntPtr.Zero;
			UnsafeNativeMethods.GetWindowThreadProcessId(Handle, ref result);
			unchecked
			{
				return result.ToInt32();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public int GetWindowThreadId()
		{
			IntPtr unused = IntPtr.Zero;
			IntPtr result = UnsafeNativeMethods.GetWindowThreadProcessId(Handle, ref unused);
			unchecked
			{
				return result.ToInt32();
			}
		}

		public Rectangle Bounds
		{
			get
			{
				Rectangle result = new Rectangle();
				NativeMethods.WindowInfo info = new NativeMethods.WindowInfo();
				if (Handle != IntPtr.Zero &&
					UnsafeNativeMethods.GetWindowInfo(new HandleRef(this, Handle), ref info))
				{
					result = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledRectangle(
						info.rcWindow.Left, info.rcWindow.Top,
						info.rcWindow.Right - info.rcWindow.Left, info.rcWindow.Bottom - info.rcWindow.Top,
						false); // Coordinates are already in physical sizes (i.e. scaled)
				}
				return result;
			}
		}
	}
}
