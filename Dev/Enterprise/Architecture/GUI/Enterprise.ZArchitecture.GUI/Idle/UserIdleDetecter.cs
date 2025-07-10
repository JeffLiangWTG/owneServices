using System;
using System.Runtime.InteropServices;
using CargoWise.Interop;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	/// <summary>
	/// Detects user mouse and keyboard inactivity for the current application thread.
	/// </summary>
	internal sealed class UserIdleDetecter
	{
		public static bool IsActive
		{
			get { return instance != null; }
		}

		/// <summary>
		/// WARNING: This event fires for every user mouse or keyboard operation.
		/// It is recommended you use very short running methods for the handler of this event.
		/// 
		/// WARNING: Putting a breakpoint within an event handler of this event may crash your computer.
		/// </summary>
		public static event EventHandler UserActivity
		{
			add
			{
				if (Instance.userActivity == null)
				{
					Instance.Hook();
				}
				Instance.userActivity += value;
			}
			remove
			{
				Instance.userActivity -= value;
				if (Instance.userActivity == null)
				{
					Instance.Unhook();
					instance = null;
				}
			}
		}
		EventHandler userActivity;

#if DEBUG
		internal static void FireUserActivity()
		{
			if (instance != null)
			{
				instance.OnUserActivity(EventArgs.Empty);
			}
		}
#endif

		#region Instance

		UserIdleDetecter()
		{
		}

		static UserIdleDetecter Instance
		{
			get { return instance ?? (instance = new UserIdleDetecter()); }
		}
		[ThreadStatic]
		static UserIdleDetecter instance;

		#endregion

		#region Implementation

		const int WM_MOUSEMOVE = 0x200;
		IntPtr mouseHook;
		IntPtr keyboardHook;

		void OnUserActivity(EventArgs e)
		{
			if (userActivity != null)
			{
				userActivity(null, e);
			}
		}

		void Hook()
		{
			//For some reason, whenever UserIdleDetecter.cs mouseHook is hooked, mouse hook in ZDropForm.cs stops working. (I don't know why.)
			//Usually mouse hook is hooked only briefly though, so it's OK so long as that remains true.
			mouseHook = UnsafeNativeMethods.SetWindowsHookEx(WindowsMessage.WH_MOUSE, MouseHookProcDelegate, IntPtr.Zero, SafeNativeMethods.GetCurrentThreadId());
			keyboardHook = UnsafeNativeMethods.SetWindowsHookEx(WindowsMessage.WH_KEYBOARD, KeyboardHookProcDelegate, IntPtr.Zero, SafeNativeMethods.GetCurrentThreadId());
		}

		void Unhook()
		{
			UnsafeNativeMethods.UnhookWindowsHookEx(new HandleRef(this, mouseHook));
			UnsafeNativeMethods.UnhookWindowsHookEx(new HandleRef(this, keyboardHook));
		}

		NativeMethods.HookProc MouseHookProcDelegate
		{
			get { return mouseHookProcDelegate ?? (mouseHookProcDelegate = MouseHookProc); }
		}
		NativeMethods.HookProc mouseHookProcDelegate;

		NativeMethods.HookProc KeyboardHookProcDelegate
		{
			get { return keyboardHookProcDelegate ?? (keyboardHookProcDelegate = KeyboardHookProc); }
		}
		NativeMethods.HookProc keyboardHookProcDelegate;

		IntPtr MouseHookProc(int nCode, int wParam, int lParam)
		{
			if (wParam != WM_MOUSEMOVE && nCode >= 0)
			{
				OnUserActivity(EventArgs.Empty);
			}
			return UnsafeNativeMethods.CallNextHookEx(new HandleRef(this, mouseHook), nCode, new IntPtr(wParam), new IntPtr(lParam));
		}

		IntPtr KeyboardHookProc(int nCode, int wParam, int lParam)
		{
			OnUserActivity(EventArgs.Empty);
			return UnsafeNativeMethods.CallNextHookEx(new HandleRef(this, keyboardHook), nCode, new IntPtr(wParam), new IntPtr(lParam));
		}

		#endregion
	}
}
