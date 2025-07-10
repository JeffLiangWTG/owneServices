using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Windows.UI.Interop;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// An active manager that makes a TextBoxBase auto-list enabled.
	/// </summary>
	public class AutoListManager : IDisposable
	{
		// Things to UAT when modifying this class -
		//
		// - TextBox embedded in a PropertyGrid.
		// - Changing the selected object in a PropertyGrid then moving back again to show the auto-list.

		public AutoListManager()
		{ CargoWise.Common.Testing.DisposableLeakListener.Instance.RegisterDisposable(this); }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
		~AutoListManager()
		{ Dispose(false); }

		/// <summary>
		/// Get or set the text box that is auto-listed.
		/// </summary>
		public TextBoxBase TextBox
		{
			get { return textBox; }
			set
			{
				if (textBox != null)
				{
					textBox.TextChanged -= new EventHandler(TextBox_TextChanged);
					textBox.Leave -= new EventHandler(TextBox_LeaveOrLostFocus);
					textBox.LostFocus -= new EventHandler(TextBox_LeaveOrLostFocus);
					textBox.KeyDown -= new KeyEventHandler(TextBox_KeyDown);
					textBox.KeyUp -= new KeyEventHandler(TextBox_KeyUp);
					textBox.Disposed -= new EventHandler(TextBox_Disposed);
				}
				textBox = value;
				if (textBox != null)
				{
					HideAutoListForm();

					textBox.TextChanged += new EventHandler(TextBox_TextChanged);
					textBox.Leave += new EventHandler(TextBox_LeaveOrLostFocus);
					textBox.LostFocus += new EventHandler(TextBox_LeaveOrLostFocus);
					textBox.KeyDown += new KeyEventHandler(TextBox_KeyDown);
					textBox.KeyUp += new KeyEventHandler(TextBox_KeyUp);
					textBox.Disposed += new EventHandler(TextBox_Disposed);
				}
			}
		}

		/// <summary>
		/// The source of auto-list members.
		/// </summary>
		public IAutoListSource AutoListSource
		{
			get { return autoListSource; }
			set
			{
				if (autoListSource != null)
				{
					autoListSource.ValueCommitRequired -= new EventHandler(OnValueCommitRequired);
				}
				autoListSource = value;
				if (autoListSource != null)
				{
					autoListSource.ValueCommitRequired += new EventHandler(OnValueCommitRequired);
				}
			}
		}
		IAutoListSource autoListSource;

		public bool IsAutoListVisible
		{ get { return frmAutoList != null; } }

		protected Form AutoListForm
		{ get { return frmAutoList; } }

		#region IDisposable

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "wndProcRetHooker")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "mouseHooker")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "keyboardHooker")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "cbtHooker")]
		protected virtual void Dispose(bool disposing)
		{
			if (disposing && !isDisposed)
			{
				TextBox = null;
				isDisposed = true;
				AutoListSource = null;

				if (controlAutoList != null)
				{
					controlAutoList.Dispose();
					controlAutoList = null;
				}
				if (frmAutoList != null)
				{
					frmAutoList.Dispose();
					frmAutoList = null;
				}
				CargoWise.Common.Testing.DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
			WindowsHooksEnabled = false;
		}

		public bool IsDisposed
		{ get { return isDisposed; } }
		bool isDisposed;

		#endregion

		#region Implementation

		TextBoxBase textBox;
		internal AutoListControlHostForm frmAutoList;
		Control controlAutoList;
		bool textChangedSuspended;

		int savedSelectionStart;
		int savedSelectionLength;

		// Paste the text in the list box selection to the text box, if there is a selection. Returns true
		// if a selection was found and it was pasted.
		bool PasteListBoxSelection()
		{
			bool result = false;
			textChangedSuspended = true;
			try
			{
				if (controlAutoList != null &&
					AutoListSource != null &&
					AutoListSource.HasSelection(controlAutoList))
				{
					AutoListSource.ReplaceText(controlAutoList, TextBox);
					result = true;
				}
			}
			finally
			{
				textChangedSuspended = false;
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Allow for debugging")]
		void ShowOrRelocateAutoListForm()
		{
			Application.DoEvents(); // in some rare situations (such as when debugging) the form is created from an invoke
			if (TextBox != null && TextBox.Focused) // focus may have been lost by the above Application.DoEvents
			{
				if (frmAutoList != null)
				{
					frmAutoList.UpdateAutoListFormLocation();
				}
				else
				{
					frmAutoList = new AutoListControlHostForm(this);
					frmAutoList.HostedControl = EnsureListControlCreated();
					WindowsHooksEnabled = true;
					frmAutoList.Show();
				}
			}
			GetRidOfPropertyGridTextBoxMouseProc();
		}

		void GetRidOfPropertyGridTextBoxMouseProc()
		{
			// get out of it you mother fucker
			PropertyInfo textboxHookedProperty = textBox.GetType().GetProperty("HookMouseDown");
			if (textboxHookedProperty != null)
			{
				textboxHookedProperty.SetValue(textBox, false, Array.Empty<object>());
			}
		}

		// Hide the list box after removing all its items. Removing its items saves memory and
		// ensures the list will be re-populated later.
		void HideAutoListForm()
		{
			WindowsHooksEnabled = false;
			if (frmAutoList != null)
			{
				frmAutoList.HostedControl = null;
				frmAutoList.Dispose();
				frmAutoList = null;
			}
		}

		Control EnsureListControlCreated()
		{
			if (controlAutoList == null || controlAutoList.IsDisposed)
			{
				controlAutoList = AutoListSource.NewListControl();
			}
			return controlAutoList;
		}

		void OnValueCommitRequired(object sender, EventArgs e)
		{
			CommitValue(false);
		}

		bool CommitValue(bool hideForm)
		{
			bool result = PasteListBoxSelection();
			if (hideForm)
			{
				HideAutoListForm();
			}
			return result;
		}

		void RestoreSelection()
		{
			textBox.SelectionStart = savedSelectionStart;
			textBox.SelectionLength = savedSelectionLength;
		}

		#region Windows Message Hooks

		WindowsHookManager cbtHooker;
		WindowsHookManager keyboardHooker;
		WindowsHookManager mouseHooker;
		WindowsHookManager wndProcRetHooker;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		bool WindowsHooksEnabled
		{
			set
			{
				if (cbtHooker == null)
				{
					cbtHooker = new WindowsHookManager(
						WindowsHookCodes.WH_CBT, new WindowsHookManager.HookProc(CbtHookProc));
					keyboardHooker = new WindowsHookManager(
						WindowsHookCodes.WH_KEYBOARD, new WindowsHookManager.HookProc(KeyboardHookProc));
					mouseHooker = new WindowsHookManager(
						WindowsHookCodes.WH_MOUSE, new WindowsHookManager.HookProc(MouseHookProc));
					wndProcRetHooker = new WindowsHookManager(
						WindowsHookCodes.WH_CALLWNDPROCRET, new WindowsHookManager.HookProc(WndProcRetHookProc));
				}

				cbtHooker.Enabled = value;
				keyboardHooker.Enabled = value;
				mouseHooker.Enabled = value;
				wndProcRetHooker.Enabled = value;
			}
		}

		IntPtr CbtHookProc(int nCode, IntPtr wParam, IntPtr lParam)
		{
			IntPtr result = IntPtr.Zero;

			if (nCode == CbtHookProcFlags.HCBT_ACTIVATE &&
				wParam == frmAutoList.Handle)
			{
				result = new IntPtr(1);
			}

			if (result == IntPtr.Zero)
			{
				result = cbtHooker.CallNextHookEx(nCode, wParam, lParam);
			}
			return result;
		}

		IntPtr KeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam)
		{
			unchecked
			{
				IntPtr result = IntPtr.Zero;
				if (nCode >= 0 &&
					(Keys)wParam.ToInt32() == Keys.Enter &&
					textBox.Focused)
				{
					if (CommitValue(true))
					{
						result = new IntPtr(1);
					}
				}
				else if (
					nCode >= 0 &&
					(Keys)wParam.ToInt32() == Keys.Escape &&
					textBox.Focused)
				{
					HideAutoListForm();
					result = new IntPtr(1);
				}

				if (result == IntPtr.Zero)
				{
					result = keyboardHooker.CallNextHookEx(nCode, wParam, lParam);
				}
				return result;
			}
		}

		int ignoreNextMouseHookMsg;
		IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
		{
			IntPtr result = IntPtr.Zero;
			int msg;
			unchecked
			{
				msg = wParam.ToInt32();
			}

			NativeMethods.MOUSEHOOKSTRUCT hookData = (NativeMethods.MOUSEHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(NativeMethods.MOUSEHOOKSTRUCT));
			if (!frmAutoList.IsDisposed && !controlAutoList.IsDisposed)
			{
				if (IsParentOf(hookData.hwnd, frmAutoList.Handle))
				{
					if (hookData.wHitTestCode == HitTestCodes.HTCLIENT)
					{
						if (ignoreNextMouseHookMsg == 0)
						{
							Point clientPoint = controlAutoList.PointToClient(ControlDpiScalingHelper.NewScaledPoint(hookData.pt.x, hookData.pt.y));
							NativeMethods.MSG fakeMessage = new NativeMethods.MSG();
							fakeMessage.message = msg;
							fakeMessage.wParam = new IntPtr(hookData.wHitTestCode);
							fakeMessage.lParam = new IntPtr((clientPoint.Y << 16) + clientPoint.X);
							fakeMessage.hwnd = hookData.hwnd;
							fakeMessage.pt_x = hookData.pt.x;
							fakeMessage.pt_y = hookData.pt.y;

							ignoreNextMouseHookMsg++;
							try
							{
								UnsafeNativeMethods.DispatchMessage(ref fakeMessage);
							}
							catch (ObjectDisposedException)
							{
							}
						}
						else
						{
							ignoreNextMouseHookMsg--;
						}
					}
					else
					{
						unchecked
						{
							UnsafeNativeMethods.DefWindowProc(
								hookData.hwnd,
								msg,
								new IntPtr((int)hookData.wHitTestCode),
								new IntPtr((hookData.pt.y << 16) + hookData.pt.x));
						}
					}
				}
			}
			if (result == IntPtr.Zero)
			{
				result = mouseHooker.CallNextHookEx(nCode, wParam, lParam);
			}
			return result;
		}

		IntPtr WndProcRetHookProc(int nCode, IntPtr wParam, IntPtr lParam)
		{
			IntPtr result = IntPtr.Zero;

			if (nCode >= 0)
			{
				NativeMethods.CWPRETSTRUCT hookData = (NativeMethods.CWPRETSTRUCT)Marshal.PtrToStructure(lParam, typeof(NativeMethods.CWPRETSTRUCT));
				if (hookData.message == WindowMessages.WM_MOVE || hookData.message == WindowMessages.WM_SIZE)
				{
					if (IsParentOf(textBox.Handle, hookData.hwnd))
					{
						frmAutoList.UpdateAutoListFormLocation();
					}
				}
				if (hookData.message == WindowMessages.WM_MOUSEWHEEL)
				{
					if (hookData.hwnd == textBox.Parent.Handle)
					{
						ForwardMouseWheelMessageToActiveFormControl(hookData);
						result = new IntPtr(1);
					}
				}
			}

			if (result == IntPtr.Zero)
			{
				result = wndProcRetHooker.CallNextHookEx(nCode, wParam, lParam);
			}
			return result;
		}

		void ForwardMouseWheelMessageToActiveFormControl(NativeMethods.CWPRETSTRUCT hookData)
		{
			Control activeFormCtrl = controlAutoList;
			while (activeFormCtrl is ContainerControl)
			{
				activeFormCtrl = ((ContainerControl)activeFormCtrl).ActiveControl;
			}
			if (activeFormCtrl != null)
			{
				IntPtr msgResult = UnsafeNativeMethods.SendMessage(new HandleRef(this, activeFormCtrl.Handle), hookData.message, hookData.wParam, hookData.lParam);
				if (msgResult != IntPtr.Zero)
				{
					UnsafeNativeMethods.DefWindowProc(activeFormCtrl.Handle, hookData.message, hookData.wParam, hookData.lParam);
				}
			}
		}

		static bool IsParentOf(IntPtr hwnd, IntPtr parent)
		{
			IntPtr current = hwnd;
			while (current != IntPtr.Zero)
			{
				if (current == IntPtr.Zero ||
					current == parent)
				{
					break;
				}
				current = SafeNativeMethods.GetParent(current);
			}
			return (current != IntPtr.Zero);
		}

		#endregion

		#endregion

		#region Event Handlers

		// When the Text property of the TextBox changes, update the list box
		void TextBox_TextChanged(object sender, EventArgs e)
		{
			if (!textChangedSuspended &&
				AutoListSource != null &&
				textBox.Focused &&
				textBox.SelectionStart != -1)
			{
				savedSelectionStart = textBox.SelectionStart;
				savedSelectionLength = textBox.SelectionLength;

				bool showAutoList = false;
				if (savedSelectionStart != -1)
				{
					Control listControl = EnsureListControlCreated();
					if (!listControl.IsHandleCreated)
					{
						listControl.CreateControl();
					}
					showAutoList = AutoListSource.UpdateListControl(listControl, TextBox);
				}

				if (showAutoList)
				{
					ShowOrRelocateAutoListForm();
				}
				else
				{
					HideAutoListForm();
				}
			}
		}

		void TextBox_LeaveOrLostFocus(object sender, EventArgs e)
		{
			if (AutoListSource != null)
			{
				AutoListSource.OnTextBoxLostFocus();
			}
			if (controlAutoList != null)
			{
				textChangedSuspended = true;
				try
				{
					controlAutoList.Dispose();
					controlAutoList = null;
				}
				finally
				{
					textChangedSuspended = false;
				}
			}
			HideAutoListForm();
		}

		void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (IsAutoListVisible)
			{
				AutoListSource.OnTextBoxKeyDown(controlAutoList, e);
				if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
				{
#if NET8_0_OR_GREATER
					textBox.BeginInvoke(new System.Windows.Forms.MethodInvoker(RestoreSelection), Array.Empty<object>());
#else // .Net Framework
					textBox.BeginInvoke(new MethodInvoker(RestoreSelection), Array.Empty<object>());
#endif
				}
			}
		}

		void TextBox_KeyUp(object sender, KeyEventArgs e)
		{
			if (IsAutoListVisible)
			{
				AutoListSource.OnTextBoxKeyUp(controlAutoList, e);
			}
		}

		void TextBox_Disposed(object sender, EventArgs e)
		{
			TextBox = null;
			HideAutoListForm();
			if (AutoListSource != null)
			{
				AutoListSource.OnTextBoxLostFocus();
			}
		}

		#endregion
	}
}
