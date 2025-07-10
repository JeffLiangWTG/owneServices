using System;
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Interop;

namespace CargoWise.Windows.UI
{
	public class ExternalAppDialogButtonClicker : IDisposable
	{
		public ExternalAppDialogButtonClicker(string buttonText)
			: this(buttonText, new TimeSpan(0, 0, 10))
		{
		}

		public ExternalAppDialogButtonClicker(string buttonText, TimeSpan timeout)
		{
			this.ButtonText = buttonText.Trim().ToUpperInvariant();
			this.Timeout = timeout;
		}

		public readonly string ButtonText;
		public readonly TimeSpan Timeout;

		public bool AllowClickingOnInitialForegroundWindow;

		public bool IsSearchingForButtonToClick
		{
			get { return searchingForButtonToClick; }
		}

		public bool ButtonClicked
		{
			get { return buttonClicked; }
		}

		bool searchingForButtonToClick;
		bool buttonClicked;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public void PressButtonOnNextDialogs()
		{
			if (thread != null)
			{
				terminating = true;
				thread.Join();
			}
			thread = new Thread(new ThreadStart(OnThreadStart));
			thread.Start();
			while (!startedThread)
			{
				Thread.Sleep(0);
			}
		}

		protected void MoveMouseAndClick(uint x, uint y)
		{
			Point oldCursorPos = Form.MousePosition;
			SetCursorPosInternal(x, y);
			try
			{
				NativeMethods.mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, UIntPtr.Zero);
				NativeMethods.mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, UIntPtr.Zero);
			}
			finally
			{
				SetCursorPosInternal((uint)oldCursorPos.X, (uint)oldCursorPos.Y);
			}
		}

		protected virtual void SetCursorPosInternal(uint x, uint y)
		{
			NativeMethods.SetCursorPos(x, y);
		}

		#region IDisposable Members

		public void Dispose()
		{
			this.terminating = true;
			if (thread != null)
			{
				thread.Join();
			}
		}

		#endregion

		#region Implementation

		Thread thread;
		bool terminating;
		protected ONativeWindow initialForegroundWindow;
		protected ONativeWindow[] allWindowsInitially;
		bool startedThread;

		#region Native Methods

		internal static class NativeMethods
		{
			[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
			[return: MarshalAs(UnmanagedType.Bool)]
			internal static extern bool SetCursorPos(uint x, uint y);

			[DllImport("user32")]
			internal static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, UIntPtr dwExtraInfo);
		}

		const int MOUSEEVENTF_LEFTDOWN = 0x2;
		const int MOUSEEVENTF_LEFTUP = 0x4;

		#endregion

		#region LocateButtonWindow

		ONativeWindow LocateButtonWindow(ONativeWindow window)
		{
			ONativeWindow result = null;
			foreach (ONativeWindow childWindow in window.ChildWindows)
			{
				if (childWindow.Text.Replace("&", "").Trim().ToUpperInvariant() == ButtonText)
				{
					result = childWindow;
					break;
				}
			}
			return result;
		}

		#endregion

		void OnThreadStart()
		{
			initialForegroundWindow = ONativeWindow.ActiveTopLevelWindow; // this window should never be touched
			allWindowsInitially = ONativeWindow.GetAllTopLevelWindows();
			startedThread = true;

			int sleepBetween = 50;
			int loopCount = (int)Timeout.TotalMilliseconds / sleepBetween;
			try
			{
				searchingForButtonToClick = true;
				for (int i = 0; i < loopCount; i++)
				{
					ONativeWindow[] allTopWindows = ONativeWindow.GetAllTopLevelWindows();
					foreach (ONativeWindow window in allTopWindows)
					{
						// only do this if the window was just created, we don't want to click stuff on Word or Enterprise or something
						if (!terminating &&
							(AllowClickingOnInitialForegroundWindow || window != initialForegroundWindow) &&
							!((IList)allWindowsInitially).Contains(window))
						{
							TryClickingOnButton(window);
						}
					}
					Thread.Sleep(sleepBetween);
					if (terminating)
					{
						break;
					}
				}
			}
			finally
			{
				searchingForButtonToClick = false;
			}
			thread = null;
		}

		protected virtual bool TryClickingOnButton(ONativeWindow window)
		{
			bool result = false;
			ONativeWindow buttonWindow = LocateButtonWindow(window);

			if (buttonWindow != null && buttonWindow.Enabled)
			{
				result = true;
				Rectangle bounds = buttonWindow.Bounds;
				uint x = (uint)(bounds.Left + 5);
				uint y = (uint)(bounds.Top + 5);

				// this will work if the button is a CargoWise.Windows.UI.KButton (be it in or out of process)
				int oCM_COMMAND = 8465; //0x2111
				UnsafeNativeMethods.SendMessage(new HandleRef(buttonWindow, buttonWindow.Handle), oCM_COMMAND, IntPtr.Zero, IntPtr.Zero);

				window.IsActiveTopLevelWindow = true;
				if (window.IsActiveTopLevelWindow)
				{
					MoveMouseAndClick(x, y);
				}
				buttonClicked = true;
			}
			return result;
		}

		#endregion
	}
}
