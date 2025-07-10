using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace CargoWise.Windows.UI.Interop
{
	/// <summary>
	/// Enables hooking of windows messages using DUnsafeNativeMethods.SetWindowsHookEx.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	internal sealed class WindowsHookManager : IDisposable
	{
		public WindowsHookManager(int windowsHookCode, HookProc hookProc)
		{
			this.windowsHookCode = windowsHookCode;
			this.hookProc = hookProc;
		}

		~WindowsHookManager()
		{ Dispose(false); }

		public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

		public IntPtr CurrentHookId
		{ get { return this.currentHookId; } }

		public bool Enabled
		{
			get { return this.currentHookId != IntPtr.Zero; }
			set
			{
				if (Enabled != value)
				{
					if (value)
					{
						currentHookId = SetWindowsHookEx(
							windowsHookCode, this.hookProc, new HandleRef(this, UnsafeNativeMethods.GetModuleHandle(null)), SafeNativeMethods.GetCurrentThreadId());
						if (currentHookId == IntPtr.Zero)
						{
							throw new Win32Exception();
						}
					}
					else
					{
						UnhookWindowsHookEx(new HandleRef(this, currentHookId));
						currentHookId = IntPtr.Zero;
					}
				}
			}
		}

		public IntPtr CallNextHookEx(int nCode, IntPtr wParam, IntPtr lParam)
		{ return CallNextHookEx(new HandleRef(this, CurrentHookId), nCode, wParam, lParam); }

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "disposing")]
		void Dispose(bool disposing)
		{
			Enabled = false;
		}

		#endregion

		#region PInvoke

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		static extern IntPtr SetWindowsHookEx(int hookid, HookProc pfnhook, HandleRef hinst, int threadid);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
		[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool UnhookWindowsHookEx(HandleRef hhook);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
		[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
		static extern IntPtr CallNextHookEx(HandleRef hhook, int code, IntPtr wparam, IntPtr lparam);

		#endregion

		#region Implementation

		readonly int windowsHookCode;
		readonly HookProc hookProc;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
		IntPtr currentHookId;

		#endregion
	}
}
