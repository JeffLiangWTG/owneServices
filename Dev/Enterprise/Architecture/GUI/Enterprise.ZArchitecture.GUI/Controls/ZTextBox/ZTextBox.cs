using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using CargoWise.Interop;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using static CargoWise.Windows.UI.ControlDpiScalingHelper;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[ToolboxItem(true)]
	public class ZTextBox : KTextBox,
		IBindTo,
		IExtendedControl,
		IDataBoundControl,
		IIsOnGrid,
		IGridControl,
		IBackColorMutable,
		IIsVisibleForBindingControl,
		IResCaptionedControl,
		IHotkeyProvider,
		IEditableInViewMode,
		IMacroBox,
		ITextFindControl
	{
		#region Bare

		[ToolboxItem(false)]
		public class Bare : ZTextBox
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				return new ControlExtensionCollection(this);
			}
		}

		#endregion

		#region Constructor

		public ZTextBox()
		{
			contextMenuManager = new ZTextBoxBaseContextMenuManager(this, this, this);
			CharacterCasing = CharacterCasing.Upper;
			AllowPopupWhenMultiLine = true;
			Extensions = NewExtensionCollection();
			ColorChanger = new ActiveControlColorChanger(this);

			UserEventTracker.Instance.AddUserEventToControl(this);
			DisposableLeakListener.Instance.RegisterDisposable(this);
			devInfoPopupManager = new DevInfoPopupManager(this);

			TextChanged += Text_TextChanged;
		}

		void Text_TextChanged(object sender, EventArgs e)
		{
			//normally prevented when we try to type, but MSAA can get around this by calling AccessibleObject.Value.set (WI00420735)
			if (ShouldAggressivelyTruncateText
				&&
				this.Enabled && this.Visible && !this.ReadOnly
				&& !string.IsNullOrEmpty(Text) && MaxLength > 0 && MaxLength < 32767 && Text.Length > MaxLength)
			{
				Text = Text.Substring(0, MaxLength);
			}
		}

		[DefaultValue(false)]
		public virtual bool ShouldAggressivelyTruncateText
		{
			get
			{
				return this.GetType() == typeof(ZTextBox) || this.GetType() == typeof(Bare);
			}
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			if (DataBindings.Count > 0 && BindingContext != null) // avoid calling BindingContext if no data binding
			{
				for (var index = 0; index < DataBindings.Count; ++index)
				{
					var binding = DataBindings[index];

					var bindingManagerBase = BindingContext[binding.DataSource, binding.BindingMemberInfo.BindingPath];

					if (bindingManagerBase is CurrencyManager currencyManager)
					{
						if (binding.IsBinding && currencyManager.List == null)
						{
							ErrorReporter.ReportOnce("NewBindingContextWouldThrowNullReferenceException", FormattableString.Invariant($@"
New binding context would throw null reference exception, details:
Control Info: {ControlDescription.GetControlPath(this)}
DataSource: {binding.DataSource?.GetType()}
DataMember： {binding.BindingMemberInfo.BindingPath}"));

							break;
						}
					}
				}
			}

			base.OnBindingContextChanged(e);

			UpdateHotkeysForBindingChanged();
		}

		public void UpdateHotkeysForBindingChanged()
		{
			if (!emailHotkeyInitialized)
			{
				var descriptor = PropertyDescriptor;
				if (descriptor != null)
				{
					emailHotkeyInitialized = true;
					if (descriptor.HasSetter() && descriptor.Attributes.OfType<EmailAddressAttribute>().Any() && !Hotkeys.IsRegistered(Keys.Control | Keys.E))
					{
						Hotkeys.RegisterHotKey(Keys.Control | Keys.E, SetEmail, Res.GetString("b0d46ff0-475a-4a22-8af0-c4211c30b1d1", "Set to your email address"));
					}
				}
			}
		}

		bool emailHotkeyInitialized;

		PropertyDescriptor PropertyDescriptor
		{
			get
			{
				if (BindingManager != null)
				{
					var bindingMember = BindingMemberHelper.BindingMember;
					if (!string.IsNullOrEmpty(bindingMember))
					{
						var periodLocation = bindingMember.LastIndexOf('.');
						if (periodLocation >= 0)
						{
							bindingMember = bindingMember.Substring(periodLocation + 1);
						}
						return BindingManager.GetItemProperties()[bindingMember];
					}
				}
				return null;
			}
		}

		bool SetEmail(object sender, Keys keyData)
		{
			if (this.GetReadOnly() || !Enabled)
			{
				return false;
			}

			var emailAddress = Env.CurrentUser.EmailAddress;
			if (!string.IsNullOrEmpty(emailAddress))
			{
				Text = emailAddress;
				SelectionStart = Text.Length;
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("29c82a85-acc1-4cec-a814-b624a875da70", "Your email address has not been set"));
			}

			return true;
		}

		#endregion

		#region MaxLength

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override int MaxLength
		{
			get => base.MaxLength;
			set
			{
				if (MaxLength != value && value >= 0)
				{
					base.MaxLength = value;
					OnMaxLengthChanged();
				}
			}
		}

		protected virtual void OnMaxLengthChanged()
		{
			MaxLengthChanged?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler MaxLengthChanged;

		#endregion

		#region DefaultSize

		public static int DefaultHeight
		{
			get
			{
				if (defaultHeight == -1)
				{
					using (var textbox = new ZTextBox())
					{
						defaultHeight = textbox.Height;
					}
				}
				return defaultHeight;
			}
		}

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static int defaultHeight = -1;

		#endregion

		#region IsVisibleForBinding

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZBool IsVisibleForBinding
		{
			get => Visible;
			set
			{
				if (!fInVisibleForBinding)
				{
					fInVisibleForBinding = true;
					try
					{
						if (Visible != value)
						{
							Visible = value;
							OnIsVisibleForBindingChanged();
						}
					}
					finally
					{
						fInVisibleForBinding = false;
					}
				}
			}
		}
		bool fInVisibleForBinding;

		public event EventHandler IsVisibleForBindingChanged;

		protected virtual void OnIsVisibleForBindingChanged()
		{
			IsVisibleForBindingChanged?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		#region Tabbing / Popup when Multiline

		public virtual string TypeNameForDisplay => Res.GetString("0bbb5cd7-88a4-4d1f-bd40-6767bb1b75ad", "Text Box");
		public HotkeyRegister Hotkeys { get; } = new HotkeyRegister();

		void RegisterPopupKey()
		{
			if (!haveRegisteredPopupHotkey)
			{
				haveRegisteredPopupHotkey = true;
				Hotkeys.RegisterHotKey(Keys.F3, PopupFormHotkey, Res.GetString("27bf496e-8eb9-42ba-92b8-e9d614689426", "Pop out (if multi-line)"));
			}
		}
		bool haveRegisteredPopupHotkey;

		protected override void OnMultilineChanged(EventArgs e)
		{
			base.OnMultilineChanged(e);

			if (Multiline)
			{
				RegisterPopupKey();
			}
		}

		void PopupFormHotkey()
		{
			if (IsDynamicMultiline)
			{
				ShowDynamicMultilineTextBoxForm();
			}
			else if (AllowPopupWhenMultiLine && Multiline && !ReadOnly)
			{
				var popupForm = new ZTextBoxPopupForm(this);
				ZFormModaliser.Show(popupForm, FindForm());
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			var result = false;
			if (!IsOnGrid && keyData == Keys.Enter && !Multiline && !AcceptsReturn)
			{
				var args = new CancelEventArgs();
				EnterPressedLeavingControl?.Invoke(this, args);
				result = args.Cancel || this.GetTopLevelNonParentedControl().SelectNextControlNonTabStopNonReadOnly(this, true, true, true);
			}

			#if !WINZOR
			if (keyData == (Keys.Back | Keys.Control) || keyData == (Keys.Back | Keys.Control | Keys.Shift))
			{
				var lastCaretPos = SelectionStart;

				if (lastCaretPos == 0)
				{
					return true;
				}

				var scanCode = InputSimulatorUnsafeNativeMethods.MapVirtualKey((uint)Keys.Left, 0);
				// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-keydown
				var lParam = (0x01000001 | (scanCode << 16));

				UnsafeNativeMethods.SendMessage(new HandleRef(this, Handle), WindowsMessage.WM_KEYDOWN, new IntPtr((uint)Keys.Left), new IntPtr(lParam));
				Select(SelectionStart, lastCaretPos - SelectionStart);
				var strPtr = Marshal.StringToCoTaskMemUni(string.Empty);
				const int EM_REPLACESEL = 0x00C2;

				using (new DisposableAction(() => Marshal.FreeHGlobal(strPtr)))
				{
					UnsafeNativeMethods.SendMessage(new HandleRef(this, Handle), EM_REPLACESEL, new IntPtr(Convert.ToInt32(true)), strPtr);
				}

				return true;
			}
			else if (EnableFindDialog && keyData == (Keys.Control | Keys.F))
			{
				ShowFindDialog();
				return true;
			}
			#endif

			return result || Hotkeys.ProcessCmdKey(this, keyData) || base.ProcessCmdKey(ref msg, keyData);
		}

		[DefaultValue(true)]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool AllowPopupWhenMultiLine { get; set; }

		public event EventHandler EnterPressedLeavingControl;

		#endregion

		#region ColorChanger

		public ActiveControlColorChanger ColorChanger { get; private set; }

		[DefaultValue(false), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool EnableValidStateColor { get; set; } = false;

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			if (!this.IsDesignMode())
			{
				ColorChanger.SetReadonlyColor(ReadOnly);
			}
		}

		protected override void OnReadOnlyChanged(EventArgs e)
		{
			base.OnReadOnlyChanged(e);
			if (!this.IsDesignMode())
			{
				ColorChanger.SetReadonlyColor(ReadOnly);
			}
		}

		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZTextBox>()
				.Property("Text", "") // This is property name
				.Property("ReadOnly", false)
				.Property("ReadOnlyForBinding", false, false)
				.Property("ReadOnlyForBindingIsNull", false, false)
				.Property("MaxLength", 0)
				.Property("IsVisibleForBinding", ZBool.True)
				.Result;
		}

		#endregion

		#region ResetPosition

		/// <summary>
		/// Reset SelectionStart property to 0 in OnLeave method if no text is selected.
		/// </summary>
		[DefaultValue(true)]
		public bool ResetPosition { get; set; } = true;

		#endregion

		#region OnPaintEventExtension

		int previousSelectionLength;
		int previousSelectionStart;

		protected override void OnLeave(EventArgs e)
		{
#if !WINZOR
			PaintExtension.Skip = false;
#endif
			base.OnLeave(e);
			if (IsHandleCreated && ResetPosition)
			{
				if (HideSelection)
				{
					previousSelectionLength = SelectionLength;
					previousSelectionStart = SelectionStart;
					SelectionLength = 0;
				}
#if !WINZOR
				if (string.Equals(InputLanguage.CurrentInputLanguage.Culture.Name,"ko-KR", StringComparison.OrdinalIgnoreCase))
				{
					return;
				}
#endif
				if (SelectionLength == 0)
				{
					SelectionStart = 0;
				}
			}
		}

		protected override void OnEnter(EventArgs e)
		{
#if !WINZOR
			PaintExtension.Skip = true;
#else
			var icon = (WinzorFramework.NotificationIcon)Parent?.WinzorSpecificControls.FirstOrDefault(c => c is WinzorFramework.NotificationIcon icon && icon.NotificationAnchorControl == this);
			Parent?.WinzorSpecificControls.Remove(icon);
#endif
			base.OnEnter(e);

			if (IsOnGrid || IsOnFilterStrip)
			{
				SelectAll();
			}
			else if (HideSelection && previousSelectionLength != 0 && SelectionLength == 0)
			{
				SelectionStart = previousSelectionStart;
				SelectionLength = previousSelectionLength;
			}

			if (IsPasswordField())
			{
				if (IsCapsLockOn())
				{
					ShowCapsLockIsOnMessage();
				}
				else
				{
					DisposeToolTip(capsLockOnToolTip);
				}
			}
		}

#if !WINZOR

		internal OnPaintEventExtension PaintExtension => paintExtension ?? (paintExtension = new OnPaintEventExtension(this));
		OnPaintEventExtension paintExtension;

		protected override void OnResize(EventArgs e)
		{
			PaintExtension.OnResize(e);
			base.OnResize(e);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			if (IsPasswordField())
			{
				if ((e.Control) && e.KeyCode == Keys.C)
				{
					DisposeToolTip(capsLockOnToolTip);
					ShowCannotCopyMessage();
					return;
				}
				if (IsCapsLockOn())
				{
					DisposeToolTip(cannotCopyPasswordToolTip);
					ShowCapsLockIsOnMessage();
				}
				else
				{
					DisposeToolTip(capsLockOnToolTip);
				}
			}
			base.OnKeyUp(e);
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "We are working with raw pixel values, to work around a bug in TextRenderer")]
		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			DisposeToolTip(capsLockOnToolTip);
			DisposeToolTip(cannotCopyPasswordToolTip);
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			DisposeToolTip(capsLockOnToolTip);
			DisposeToolTip(cannotCopyPasswordToolTip);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "See Issue 01146273.")]
		protected override void WndProc(ref Message message)
		{
			const int WM_HSCROLL = 0x114;
			const int WM_VSCROLL = 0x115;
			const int WM_MOUSEWHEEL = 0x20a;
			const int WM_SHOWBALLOONTIP = 0x1503;
			const int WM_KEYDOWN = 0x100;
			const int VK_UP = 0x26;
			const int VK_DOWN = 0x28;

			if (message.Msg == WM_HSCROLL || message.Msg == WM_VSCROLL || message.Msg == WM_MOUSEWHEEL || (message.Msg == WM_KEYDOWN && (message.WParam.ToInt32() == VK_UP || message.WParam.ToInt32() == VK_DOWN)))
			{
				var oldCharIndex = TopCharIndex;
				base.WndProc(ref message);

				if (oldCharIndex != TopCharIndex)
				{
					Invalidate();
				}

				return;
			}

			if (message.Msg == WindowsMessage.WM_PASTE)
			{
				if (WmPaste())
				{
					return;
				}
			}
			else if (message.Msg == WM_SHOWBALLOONTIP)
			{
				if (IsPasswordField())
				{
#if NETFRAMEWORK
					message.Result = (IntPtr)0;
#else
					message.Result = 0;
#endif
					return;
				}
			}

			try
			{
				PaintExtension.WndProc(ref message);
			}
			catch (ArgumentException ex)
			{
				if (ex.Message.Contains("Parameter is not valid."))
				{
					Globals.Message.ShowError(Res.GetString("1BBAD6EE-280F-4043-AD48-CADA8D2A4109",
						@"Out Of Memory Exception occurred.
If problems persist, please contact your system administrator and show them this information:

{0}", TopLevelExceptionHandler.ResourcesMessage()));
				}
			}

			base.WndProc(ref message);
			PaintDynamicMultilineExpandIcon(message.Msg);
		}

		void PaintDynamicMultilineExpandIcon(int winMsg)
		{
			if (IsDynamicMultiline && winMsg == WindowsMessage.WM_PAINT)
			{
				Point[] polygonPoints =
				{
					NewScaledPoint(ClientRectangle.Width, ClientRectangle.Height, false),
					NewScaledPoint(ClientRectangle.Width - ScaleToCurrentDpiX(4), ClientRectangle.Height, false),
					NewScaledPoint(ClientRectangle.Width, ClientRectangle.Height - ScaleToCurrentDpiY(4), false)
				};
				var graphics = CreateGraphics();
				graphics.FillPolygon(Brushes.Black, polygonPoints);
			}
		}

		int TopCharIndex => GetCharIndexFromPosition(NewScaledPoint(2, 2));

#endif

#endregion

#region WmPaste

#if !WINZOR

		static readonly char[] InvisibleControlChars = { '\x0', '\x1', '\x1c', '\x1d', '\x1e', '\x1f' };

		bool WmPaste()
		{
			var ok = false;
			if (!SafeClipboard.Debounce())
			{
				return true;
			}
			var text = SafeClipboard.GetText();
			if (!string.IsNullOrEmpty(text) && 0 <= text.IndexOfAny(InvisibleControlChars))
			{
				var filteredText = new System.Text.StringBuilder(text.Length - 1);
				foreach (var c in text)
				{
					if (c > '\x1f' || -1 == Array.IndexOf(InvisibleControlChars, c))
					{
						filteredText.Append(c);
					}
				}
				var result = filteredText.ToString();
				if (MaxLength > 0)
				{
					var maxLengthLeft = Math.Max(MaxLength - Text.Length + SelectedText.Length, 0);
					if (result.Length > maxLengthLeft)
					{
						result = result.Substring(0, maxLengthLeft);
					}
				}
				SelectedText = result;
				ok = true;
			}

			return ok;
		}

#endif

#endregion

#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing && !this.IsDisposed && !this.Disposing)
			{
				if (ColorChanger != null)
				{
					ColorChanger.Dispose();
				}
				Extensions.Dispose();
#if !WINZOR
				if (paintExtension != null)
				{
					paintExtension.Dispose();
				}
#endif
				contextMenuManager.Dispose();
				DisposeDynamicMultilineItems();
				if (devInfoPopupManager != null)
				{
					devInfoPopupManager.Dispose();
				}
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}

			base.Dispose(disposing);
		}

#endregion

#region Obsolete

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool AllowNull
		{
			get { return true; }
			set { }
		}

#endregion

#region Multiline

		public override bool Multiline
		{
			get => base.Multiline;
			set
			{
				base.Multiline = value;
				var labelCaptionRenderer = Extensions.Get<ILabelCaptionRenderer>();
				if (labelCaptionRenderer != null && labelCaptionRenderer.Alignment != LabelCaptionAlignment.Top)
				{
					labelCaptionRenderer.Options = value ? StringRenderingOptions.Wrap : StringRenderingOptions.Truncate;
					labelCaptionRenderer.Refresh();
				}
			}
		}

#endregion

#region Dynamic Multiline

		/// <summary>
		/// The TextBox is able to be converted to multiline (resizable) at runtime by pressing F3 or clicking the expand button
		/// </summary>
		[Browsable(true), DefaultValue(false)]
		public bool IsDynamicMultiline
		{
			get => isDynamicMultiline;
			set
			{
				isDynamicMultiline = value;
				if (isDynamicMultiline)
				{
					RegisterPopupKey();
				}
			}
		}
		bool isDynamicMultiline;

#if !WINZOR

		protected override void OnMouseUp(MouseEventArgs mevent)
		{
			base.OnMouseUp(mevent);
			if (IsDynamicMultiline && IsWithinExpandButtonRectangle(mevent.Location))
			{
				ShowDynamicMultilineTextBoxForm();
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (IsDynamicMultiline)
			{
				if (IsWithinExpandButtonRectangle(e.Location))
				{
					if (originalCursor == null)
					{
						originalCursor = Cursor;
					}
					Cursor = Cursors.Hand;
				}
				else
				{
					Cursor = originalCursor;
				}
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (!Focused && !IsOnGrid)
			{
				Focus();
			}
		}

		bool IsWithinExpandButtonRectangle(Point pt)
		{
			var referenceRectangle = ControlDpiScalingHelper.NewScaledRectangle(ClientRectangle.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(4),
				ClientRectangle.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(4),
				ControlDpiScalingHelper.ScaleToCurrentDpiX(4),
				ControlDpiScalingHelper.ScaleToCurrentDpiY(4),
				false);
			return referenceRectangle.Contains(pt);
		}

		void DisposeDynamicMultilineItems()
		{
			HideDynamicMultilineTextBoxForm();
			RemoveMouseHook();
		}

#endif

		#region Mouse Hooks

#if !WINZOR

		SafeNativeMethods.MouseHookProc mouseHookProcEventHandler;
		IntPtr mouseHook;

		void SetMouseHook()
		{
			if (mouseHook == IntPtr.Zero)
			{
				if (mouseHookProcEventHandler == null)
				{
					mouseHookProcEventHandler = MouseHookProc;
				}

				mouseHook = UnsafeNativeMethods.SetWindowsHookEx(WindowsMessage.WH_MOUSE, mouseHookProcEventHandler, IntPtr.Zero, SafeNativeMethods.GetCurrentThreadId());
			}
		}

		IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
		{
			var handled = false;

			if (nCode >= 0 && !this.Disposing)
			{
				switch (wParam.ToInt64())
				{
					case SafeNativeMethods.WM_NCLBUTTONDOWN:
					case SafeNativeMethods.WM_NCRBUTTONDOWN:
					case SafeNativeMethods.WM_LBUTTONDOWN:
						var mouseHookInfoEx = (SafeNativeMethods.MouseHookInfoEx)Marshal.PtrToStructure(lParam, typeof(SafeNativeMethods.MouseHookInfoEx));
						var mouseHookInfo = mouseHookInfoEx.MouseHookInfo;

						var location = ControlDpiScalingHelper.NewScaledPoint(mouseHookInfo.Location.X, mouseHookInfo.Location.Y, false);
						handled = HandleClick(location);
						break;
				}
			}

			return (!handled)
				? UnsafeNativeMethods.CallNextHookEx(new HandleRef(this, mouseHook), nCode, wParam, lParam)
				: new IntPtr(1);
		}

		void RemoveMouseHook()
		{
			if (mouseHook != IntPtr.Zero)
			{
				UnsafeNativeMethods.UnhookWindowsHookEx(new HandleRef(this, mouseHook));
				mouseHook = IntPtr.Zero;
			}
		}

		bool HandleClick(Point screenLocation)
		{
			if (dynamicMultilineTextBoxForm != null && !dynamicMultilineTextBoxForm.IsDisposed)
			{
				var locationForDropForm = dynamicMultilineTextBoxForm.PointToClient(screenLocation);
				if (!dynamicMultilineTextBoxForm.ClientRectangle.Contains(locationForDropForm))
				{
					HideDynamicMultilineTextBoxForm();
				}
			}

			return false;
		}

#endif

		#endregion

		#region Show / Hide Dynamic Multiline TextBox Form

		internal Size lastMultilineTextBoxSize;

		ZDynamicMultilineTextBoxForm dynamicMultilineTextBoxForm;
#if !WINZOR

		void ShowDynamicMultilineTextBoxForm()
		{
			dynamicMultilineTextBoxForm = new ZDynamicMultilineTextBoxForm(this, lastMultilineTextBoxSize);
			SetMouseHook();
			var location = Parent?.PointToScreen(Location) ?? PointToScreen(Location);
			dynamicMultilineTextBoxForm.Location = location;

			SafeNativeMethods.ShowWindow(dynamicMultilineTextBoxForm, ShowWindowsOptions.SW_SHOWNORMAL);
			SafeNativeMethods.SetWindowPos(dynamicMultilineTextBoxForm, -1, dynamicMultilineTextBoxForm.Location.X, dynamicMultilineTextBoxForm.Location.Y, dynamicMultilineTextBoxForm.Width, dynamicMultilineTextBoxForm.Height, 0x0010);
		}

		void HideDynamicMultilineTextBoxForm()
		{
			dynamicMultilineTextBoxForm?.Close();
		}

#if DEBUG
		internal bool IsDynamicMultilineTextBoxFormVisible => dynamicMultilineTextBoxForm != null && dynamicMultilineTextBoxForm.Visible;

		internal ZDynamicMultilineTextBoxForm TestDynamicMultilineTextBoxForm => dynamicMultilineTextBoxForm;
#endif

#else
		public override bool IsDynamicMultilineTextBoxFormVisible => dynamicMultilineTextBoxForm != null && dynamicMultilineTextBoxForm.Visible;

		protected override bool HasDynamicExpandArrow => isDynamicMultiline && !IsOnGrid;

		protected override void ShowDynamicMultilineTextBoxForm()
		{
			if (!IsDynamicMultilineTextBoxFormVisible)
			{
				dynamicMultilineTextBoxForm = new ZDynamicMultilineTextBoxForm(this, lastMultilineTextBoxSize);
				dynamicMultilineTextBoxForm.ShowDynamicTextBox();
				var location = Parent?.PointToScreen(Location) ?? PointToScreen(Location);
				dynamicMultilineTextBoxForm.Location = location;
				Enabled = false;
			}
		}

		protected override void DisposeDynamicMultilineItems()
		{
			dynamicMultilineTextBoxForm?.HideDynamicTextBox();
			dynamicMultilineTextBoxForm?.Dispose();
		}

#endif

#endregion
#if !WINZOR
		Cursor originalCursor;
#endif

#endregion

#region IExtendedControl Members

		Control IExtendedControl.Host => this;

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; }

		protected virtual IControlExtensionCollection NewExtensionCollection()
		{
			return new DefaultControlExtensionCollection(this);
		}

#endregion

#region IDataBoundControl Members

		string IDataBoundControl.DataMember => DataMember;

		protected string DataMember => DataBoundControl.GetDefaultImplementation(this).DataMember;

		object IDataBoundControl.DataSource => DataSource;

		protected object DataSource => DataBoundControl.GetDefaultImplementation(this).DataSource;

		Type IDataBoundControl.DataSourceType => DataSourceType;

		protected virtual Type DataSourceType => DataBoundControl.GetDefaultImplementation(this).DataSourceType;

		public virtual void SetDataBinding(object dataSource, string dataMember)
		{
			DataBoundControl.GetDefaultImplementation(this).SetDataBinding(dataSource, dataMember);
		}

#endregion

#region IBindTo Members

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string BindTo
		{
			get => BindingMemberHelper.BindingMember;
			set => BindingMemberHelper.BindingMember = value;
		}

		ControlBindingMemberHelper BindingMemberHelper => bindingMemberHelper ?? (bindingMemberHelper = ControlBindingMemberHelper.Get(this));
		ControlBindingMemberHelper bindingMemberHelper;

		protected Type BindingSourceDataSourceType => KBindingSource.GetBindingSource(this) == null ? null : KBindingSource.GetBindingSource(this).DataSourceType;

#endregion

#region IIsOnGrid Members

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual bool IsOnGrid { get; set; }

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual bool IsOnFilterStrip { get; set; }

#endregion

#region IGridControl Members

		int IGridControl.ButtonWidth => 0;

		void IGridControl.ActivateEditControl()
		{
			Focus();
		}

		bool IGridControl.ShouldHandleKey(Keys keyData)
		{
			return ShouldHandleKey(keyData);
		}

		protected virtual bool ShouldHandleKey(Keys keyData)
		{
			return Multiline && (keyData == Keys.Enter || keyData == Keys.Down || keyData == Keys.Up || keyData == Keys.Left || keyData == Keys.Right);
		}

		bool IGridControl.ShownForReadOnly => false;

#endregion

#region Implementation

		[SmartTagVisible]
		[DefaultValue(CharacterCasing.Upper)]
		public new CharacterCasing CharacterCasing
		{
			get => base.CharacterCasing;
			set => base.CharacterCasing = value;
		}

		protected override void SetVisibleCore(bool value)
		{
			base.SetVisibleCore(value);
			NotificationBroadcaster.Instance.BroadcastVisibilityChange(this);
		}

		protected virtual BindingManagerBase BindingManager => DataBindings["Text"]?.BindingManagerBase;

		internal ZTextBoxBaseContextMenuManager contextMenuManager;

		[DefaultValue(false)]
		public bool EditableInViewMode { get; set; }

#endregion

#region CaptionResourceString

		[Browsable(true)]
		public ResourceStringData CaptionResourceString
		{
			get { return captionResourceString ?? ResourceStringData.Empty; }
			set
			{
#if DEBUG
				if (DesignMode && value == null)
				{
					return;
				}
#endif
				captionResourceString = value;
				this.RefreshCaptionLabel();
			}
		}
		ResourceStringData captionResourceString;

		bool ShouldSerializeCaptionResourceString()
		{
			return !CaptionResourceString.IsEmpty();
		}

		#endregion

		#region Warnings

		[SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
		[DllImport("user32.dll")]
		static extern short GetKeyState(int nVirtKey);

#if DEBUG
		internal static Overridable<bool> EnableMockCapsLockIsOnForTesting = new Overridable<bool>();
		internal static Overridable<bool> MockCapsLockIsOn = new Overridable<bool>();

		internal
#endif
		static bool IsCapsLockOn()
		{
			var keyState = GetKeyState(0x14);

#if DEBUG
			if (EnableMockCapsLockIsOnForTesting.Value)
			{
				keyState = MockCapsLockIsOn.Value ? (short)1 : (short)2;
			}
#endif
			if (keyState == 1 || keyState == -127)
			{
				return true;
			}

			return false;
		}

		bool IsPasswordField()
		{
			return IsHandleCreated && PasswordChar != '\0';
		}

#if DEBUG
		internal
#endif
		KToolTip capsLockOnToolTip;
#if DEBUG
		internal
#endif
		KToolTip cannotCopyPasswordToolTip;

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		KToolTip ShowToolTip(KToolTip toolTip, ToolTipIcon iconType, string toolTipTitle, string toolTipText)
		{
			DisposeToolTip(toolTip);
			toolTip = new KToolTip
			{
				InitialDelay = 500,
				ToolTipTitle = toolTipTitle,
				ToolTipIcon = iconType,
				IsBalloon = true
			};
			toolTip.SetToolTip(this, toolTipText);
			toolTip.Show(toolTipText, this, ControlDpiScalingHelper.ScaleToCurrentDpiX(5), Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(5), 5000);
			return toolTip;
		}

		void DisposeToolTip(IDisposable toolTip)
		{
			toolTip?.Dispose();
		}

		void ShowCapsLockIsOnMessage()
		{
			capsLockOnToolTip = ShowToolTip(capsLockOnToolTip, ToolTipIcon.Warning, Res.GetString("E93EF597-C9F5-4604-B7E3-D7708CA1F697", "Caps Lock is On"), Res.GetString("7E30441E-FC81-4FD1-84AB-2BAE4210ED9E", "Having Caps Lock on may cause you to enter your password incorrectly.\r\n\r\nYou should press Caps Lock to turn it off before entering your password."));
		}

#if !WINZOR
		void ShowCannotCopyMessage()
		{
			cannotCopyPasswordToolTip = ShowToolTip(cannotCopyPasswordToolTip, ToolTipIcon.Error, Res.GetString("002A7FFE-D513-4E73-812F-16152E28F4E2", "Not Allowed"), Res.GetString("A2C5FD0D-7C10-45CB-A5D8-671686DEA2A4", "You cannot copy text from a password field."));
		}
#endif

#endregion

#region Macros

		[DefaultValue(false)]
		public bool SupportsMacroTemplates { get; set; }

		[DefaultValue(true)]
		public bool AllowMultipleMacros { get; set; } = true;

		[DefaultValue(false)]
		public bool DataFieldsOnly { get; set; }

		[DefaultValue(false)]
		public bool MacroFieldsOnly { get; set; }

		[DefaultValue(false)]
		public bool HideMacroFields { get; set; }

		[DefaultValue(false)]
		public bool HideDataFields { get; set; }

		[DefaultValue(false)]
		public bool ShowXmlFields { get; set; }

		[DefaultValue(1)]
		public int DefaultCollectionIndex { get; set; } = 1;

		[DefaultValue(null)]
		public Type XmlType { get; set; }

		[DefaultValue(false)]
		public bool UseMcrEvaluator { get; set; }

		[DefaultValue(MacroConstants.DefaultMacroOpeningBracket)]
		public string MacroOpeningBracket { get; set; } = MacroConstants.DefaultMacroOpeningBracket;

		[DefaultValue(MacroConstants.DefaultMacroClosingBracket)]
		public string MacroClosingBracket { get; set; } = MacroConstants.DefaultMacroClosingBracket;

		[DefaultValue(false)]
		public bool EnableFindDialog { get; set; }

		TextBox IMacroBox.TextBox => this;
		object IMacroBox.DataSource => DataSource;
		IMapTreePresentationManager IMacroBox.MacroManager { get; set; }
		EventHandler IMacroBox.InsertMacroHandler => this.GetInsertMacroHandler();
		EventHandler IMacroBox.PreviewMacroHandler => this.GetPreviewMacroHandler();
		bool IMacroBox.IsMacroControl => SupportsMacroTemplates;
		ResourceStringData IMacroBox.InsertMacroCaption => null;
		void IMacroBox.StartedEditing(TextBox t) { }

		[DefaultValue(false)]
		public bool ShouldEscapeAllSpecialCharacters { get; set; }

		#endregion

		internal readonly DevInfoPopupManager devInfoPopupManager;

#if DEBUG

		public void InitializeContextMenu_ForTest()
		{
			contextMenuManager.InitializeContextMenu();
		}

#endif

		public void ShowFindDialog()
		{
#if !WINZOR
			if (!FindDialogIsOpen)
			{
				currentFindForm = new TextFindForm(this);
				currentFindForm.Owner = FindForm();
				currentFindForm.Show();
			}
			else
			{
				currentFindForm.BringToFront();
				currentFindForm.Focus();
			}
		}
		TextFindForm currentFindForm;

		[DefaultValue(false)]
		public bool FindDialogIsOpen
		{
			get { return currentFindForm != null && !currentFindForm.IsDisposed; }
		}
#else
		}
#endif

		public void HighlightText(ZInt startPosition, ZInt length)
		{
			Select(startPosition, length);
			ScrollToCaret();
		}

		public string GetText() => this.Text;
	}
}
