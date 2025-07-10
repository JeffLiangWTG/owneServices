using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.Interop;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Notifications;

namespace Enterprise.ZArchitecture.GUI
{
	public enum ZContentAlignment
	{
		Left, Right
	}

	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[ToolboxItem(true)]
	public partial class ZCheckBox : KCheckBox, IBindTo, IExtendedControl, IBackColorMutable, IResCaptionedControl, IDisposeStackProvider, IEditableInViewMode
	{
		#region Constructors

		static ZCheckBox()
		{
			NotificationAdornmentFactory.RegisterCustomLayout(new ZCheckBoxAdornmentLayout());
		}

		public ZCheckBox()
		{
			UserEventTracker.Instance.AddUserEventToControl(this);
			ColorChanger = new ActiveControlColorChanger(this);
			FlatStyle = FlatStyle.System;
#if !WINZOR
			CurrentMouseState = MouseState.Normal;
#endif

			DisposableLeakListener.Instance.RegisterDisposable(this);
			Extensions = new DefaultControlExtensionCollection(this);

			translationFeedbackManager = new TranslationFeedbackManager(this, TranslationFeedbackManager.ClickMode.None);
			devInfoPopupManager = new DevInfoPopupManager(this, TranslationFeedbackManager.ClickMode.None);
		}

		#endregion

		#region Properties

		[BindingOptions(UseTypeConverters = true, UpdateDataSourceOnPropertyChange = true)]
		[DefaultValue(false)]
		public new bool Checked
		{
			get { return base.Checked; }
			set { base.Checked = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public XPButtonState XPButtonState { get; set; }

		#region ZContentAlignment

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(ZContentAlignment.Left)]
		public new ZContentAlignment CheckAlign
		{
			get { return fCheckAlign; }
			set
			{
				if (fCheckAlign != value)
				{
					fCheckAlign = value;
					base.CheckAlign = (value == ZContentAlignment.Left ? ContentAlignment.MiddleLeft : ContentAlignment.MiddleRight);
					Invalidate();
				}
			}
		}
		ZContentAlignment fCheckAlign = ZContentAlignment.Left;

		#endregion

		#region FlatStyle

		[DefaultValue(FlatStyle.System)]
		public new FlatStyle FlatStyle
		{
			get { return IsSystemFlatStyle ? FlatStyle.System : base.FlatStyle; }

			// if we set this to System, drawing is handled by Win32 API, and will not call OnPaint()
			set
			{
				IsSystemFlatStyle = (value == FlatStyle.System);
				base.FlatStyle = IsSystemFlatStyle ? FlatStyle.Standard : value;
			}
		}
		bool IsSystemFlatStyle;

		#endregion

		#region ReadOnly

		[DefaultValue(false)]
		public bool ReadOnly
		{
			get { return readOnly; }
			set
			{
				if (readOnly != value)
				{
					readOnly = value;

					Color newForeColor;
					if (value)
					{
						previousForeColor = ForeColor;
						newForeColor = ReadOnlyColor;
					}
					else
					{
						newForeColor = previousForeColor ?? DefaultForeColor;
					}

					ForeColor = newForeColor;
					OnReadOnlyChanged(EventArgs.Empty);
				}
			}
		}
		bool readOnly;
		Color? previousForeColor;

		protected virtual Color ReadOnlyColor { get { return SystemColors.GrayText; } }

		void OnReadOnlyChanged(EventArgs e)
		{
			Invalidate();
			ReadOnlyForBindingProperty.OnReadOnlyChanged();

			if (ReadOnlyChanged != null)
			{
				ReadOnlyChanged(this, e);
			}
		}

		public event EventHandler ReadOnlyChanged;

		protected override ControlReadOnlyPropertyHelper ReadOnlyForBindingProperty
		{
			get
			{
				if (readOnlyForBindingProperty == null)
				{
					readOnlyForBindingProperty = new ControlReadOnlyPropertyHelper(
							this,
							delegate
							{ return ReadOnly; },
							delegate(bool value)
							{ ReadOnly = value; });
				}
				return readOnlyForBindingProperty;
			}
		}
		ControlReadOnlyPropertyHelper readOnlyForBindingProperty;

		#endregion

		#endregion

		#region Custom Adornment Layout

		class ZCheckBoxAdornmentLayout : AdornmentLayout<ZCheckBox>
		{
			public override IEnumerable<IIconLayout> GetIconAdornmentTargets(ZCheckBox source)
			{
				yield return new ZCheckBoxIconLayout { Target = source };
			}
		}

		class ZCheckBoxIconLayout : PreciseIconLayout
		{
			internal const int CheckBoxWidth = 14;

			protected override int GetAlignmentDirection()
			{
				return ControlDpiScalingHelper.ScaleToCurrentDpiX(CheckBox.CheckAlign == ZContentAlignment.Left ? 1 : -1);
			}

			protected override int GetPreciseWidth()
			{
				return ControlDpiScalingHelper.ScaleToCurrentDpiX(CheckBoxWidth);
			}

			protected override void AddWidth(int width)
			{
				CheckBox.preferredWidthModifier = width;
			}

			protected override bool ShouldResize
			{
				get { return true; }
			}

			ZCheckBox CheckBox
			{
				get { return ((ZCheckBox)Target); }
			}
		}

		#endregion

		#region Not Supported

#pragma warning disable 0809
		[Obsolete("RightToLeft is not supported by ZCheckBox, see Geoff if you require it.", true)]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override RightToLeft RightToLeft
		{
			get { return base.RightToLeft; }
			set { /* so that we get the obsolete compile error when setting */ }
		}
#pragma warning restore 0809

		#endregion

		#region Drawing

		/// <summary>
		/// Called by the GridColumn to get the image of XP-style checkbox
		/// </summary>
		/// <param name="g">Graphics</param>
		/// <param name="bounds">Rectangle to fill</param>
		public void Draw(Graphics g, Rectangle bounds)
		{
#if !WINZOR
			OnPaint(new PaintEventArgs(g, bounds));
#endif
		}

#if !WINZOR
		const int buttonType = (int)ButtonType.CheckBox;
#pragma warning disable IDE0044 //Object hTheme is getting modified, conflicting readonly property
		IntPtr hTheme = IntPtr.Zero;
#pragma warning restore IDE0044
#endif
		int preferredWidthModifier;

		protected void BasePaint(PaintEventArgs pevent)
		{
			base.OnPaint(pevent);
		}

		///<summary>
		/// Retrieves the size of a rectangular area into which a control can be fitted.
		///</summary>
		///
		///<returns>
		/// An ordered pair of type <see cref="T:System.Drawing.Size"></see> representing the width and height of a rectangle.
		///</returns>
		///
		///<param name="proposedSize">The custom-sized area for a control.</param>
		public override Size GetPreferredSize(Size proposedSize)
		{
			var preferredSize = base.GetPreferredSize(proposedSize);
			return ControlDpiScalingHelper.NewScaledSize(preferredSize.Width + preferredWidthModifier, preferredSize.Height, false);
		}

#if !WINZOR

		bool paintHasFailedBefore;

		protected override void OnPaint(PaintEventArgs e)
		{
			if (paintHasFailedBefore)
			{
				PaintException(e);
				return;
			}

			try
			{
				if (DrawXpStyle)
				{
					DrawBackground(e.Graphics, e.ClipRectangle);
					DrawButton(e.Graphics);
					DrawText(e.Graphics);

					if (Focused)
					{
						DrawFocus(e.Graphics);
					}
				}

				BasePaint(e);
			}
			catch (ArgumentException)
			{
				//Parameter is not valid. at System.Drawing.Graphics.GetHdc()
				paintHasFailedBefore = true;
				PaintException(e);
			}
			catch (InvalidOperationException)
			{
				//Object is currently in use elsewhere. at System.Drawing.Graphics.CheckErrorStatus(Int32 status)
				paintHasFailedBefore = true;
				PaintException(e);
			}
		}

		//copied from System.Windows.Forms.Control.PaintException and modified
		void PaintException(PaintEventArgs e)
		{
			try
			{
				using (var pen = new Pen(Color.Red, 2.0f))
				{
					var clientRectangle = ClientRectangle;
					var rect = clientRectangle;
					ControlDpiScalingHelper.SetX(ref rect, rect.X + ControlDpiScalingHelper.ScaleToCurrentDpiX(1), false);
					ControlDpiScalingHelper.SetY(ref rect, rect.Y + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
					ControlDpiScalingHelper.SetWidth(ref rect, rect.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(1), false);
					ControlDpiScalingHelper.SetHeight(ref rect, rect.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
					e.Graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
					rect.Inflate(-1, -1);
					e.Graphics.FillRectangle(Brushes.White, rect);
					e.Graphics.DrawLine(pen, clientRectangle.Left, clientRectangle.Top, clientRectangle.Right, clientRectangle.Bottom);
					e.Graphics.DrawLine(pen, clientRectangle.Left, clientRectangle.Bottom, clientRectangle.Right, clientRectangle.Top);
				}
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{ throw; }
			}
		}

		/// <summary>
		/// Draws a Windows XP style button.
		/// </summary>
		/// <param name="hDC">A managed pointer containing the handle to a control's device context.</param>
		/// <param name="bounds">A Rectangle structure containing the bounds of the control to draw on.</param>
		protected virtual void DrawXpButton(IntPtr hDC, Rectangle bounds)
		{
			var rect = new RECT(bounds);

			var code = NativeMethods.DrawThemeParentBackground(Handle, hDC, ref rect);
			if (code != 0)
			{
				var error = new Win32Exception(code, "ZCheckBox.cs and NativeMethods.DrawThemeParentBackground() have returned an error.");
				ErrorReporter.ReportOnce("", error);
			}

			if (!Enabled)
			{
				var error = NativeMethods.DrawThemeBackground(hTheme, hDC, buttonType, (int)XPButtonState.Disabled, ref rect, ref rect);
				if (error != 0)
				{
					var exception = new Win32Exception(error, "ZCheckBox.cs and NativeMethods.DrawThemeBackground() have returned an error."); // developer use only
					ErrorReporter.ReportOnce("", exception);
				}
			}
			else
			{
				int stateId;

				switch (XPButtonState)
				{
					case XPButtonState.Hot:
						stateId = Checked ? 6 : (int)XPButtonState.Hot;
						NativeMethods.DrawThemeBackground(hTheme, hDC, buttonType, stateId, ref rect, ref rect);
						break;

					case XPButtonState.Pressed:
						NativeMethods.DrawThemeBackground(hTheme, hDC, buttonType, (int)XPButtonState.Pressed, ref rect, ref rect);
						break;

					default:
						stateId = (Checked) ? 5 : (int)XPButtonState.Normal;
						NativeMethods.DrawThemeBackground(hTheme, hDC, buttonType, stateId, ref rect, ref rect);
						break;
				}
			}
		}

		protected void DrawBackground(Graphics g, Rectangle rect)
		{
			if (Focused)
			{
				g.FillRectangle(SystemBrushes.Info, rect);
			}
			else
			{
				var brush = BrushProvider.FromColor(BackColor);
				g.FillRectangle(brush, rect);
			}
		}

		protected void DrawButton(Graphics g)
		{
			const int checkBoxButton = 3;
			var checkRect = new RECT(ButtonArea);

			var hDC = g.GetHdc();
			try
			{
				XpThemeAPI.DrawThemeBackground(ButtonThemeData, hDC, checkBoxButton, (int)CurrentDrawState, ref checkRect, ref checkRect);
			}
			finally
			{
				g.ReleaseHdc(hDC);
			}
		}

		protected void DrawText(Graphics g)
		{
			var brush = BrushProvider.FromColor(Enabled ? ForeColor : SystemColors.GrayText);
			using (var textFormat = CreateTextFormat())
			{
				TextRendererHelper.DrawText(g, Text, Font, TextArea, brush, textFormat);
			}
		}

		protected void DrawFocus(Graphics g)
		{
			Size focusSize;
			using (var textFormat = CreateTextFormat())
			{
				focusSize = g.MeasureString(Text, Font, ClientRectangle.Location, textFormat).ToSize();
			}

			var focusArea = ControlDpiScalingHelper.NewScaledRectangle(
				TextArea.X + ControlDpiScalingHelper.ScaleToCurrentDpiX(2),
				(ClientRectangle.Height - focusSize.Height) / 2,
				focusSize.Width,
				focusSize.Height, false);

			ControlDpiScalingHelper.SetHeight(ref focusArea, focusArea.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
			ControlDpiScalingHelper.SetY(ref focusArea, focusArea.Y - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);

			ControlPaint.DrawFocusRectangle(g, focusArea);
		}

		protected Rectangle ButtonArea
		{
			get
			{
				var result =
					CheckAlign == ZContentAlignment.Right
						? ControlDpiScalingHelper.NewScaledRectangle(ClientRectangle.Left + Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(BoxWidth), 0, ControlDpiScalingHelper.ScaleToCurrentDpiX(BoxWidth), Height, false)
						: ControlDpiScalingHelper.NewScaledRectangle(0, 0, ControlDpiScalingHelper.ScaleToCurrentDpiX(BoxWidth), Height, false);

				return result;
			}
		}

		Rectangle TextArea
		{
			get
			{
				var area =
					CheckAlign == ZContentAlignment.Right
						? ControlDpiScalingHelper.NewScaledRectangle(
							ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(BoxWidth + SpaceBetweenCheckAndText), ClientRectangle.Height, false)
						: ControlDpiScalingHelper.NewScaledRectangle(
								ClientRectangle.X + ControlDpiScalingHelper.ScaleToCurrentDpiX(BoxWidth + SpaceBetweenCheckAndText), ClientRectangle.Y, ClientRectangle.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(BoxWidth + SpaceBetweenCheckAndText), ClientRectangle.Height, false);
				return area;
			}
		}

		protected StringFormat CreateTextFormat()
		{
			var format =
				new StringFormat
				{
					HotkeyPrefix = (ShowKeyboardCues ? HotkeyPrefix.Show : HotkeyPrefix.Hide),
					Alignment = StringAlignment.Near,
					LineAlignment = StringAlignment.Center
				};

			return format;
		}

		protected bool DrawXpStyle
		{
			get { return IsSystemFlatStyle && XpUtils.IsAppUsingXpTheme && XpUtils.IsWindowsXpOrGreater && XpUtils.IsWindowsUsingXpTheme; }
		}

		public const int BoxWidth = 13;
		public const int SpaceBetweenCheckAndText = 4;

		#region Button Theme Data

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		IntPtr ButtonThemeData
		{
			get
			{
				if (buttonThemeData == IntPtr.Zero)
				{
					buttonThemeData = XpThemeAPI.OpenThemeData(Handle, "Button");
				}

				return buttonThemeData;
			}
		}

		IntPtr buttonThemeData = IntPtr.Zero;

		#endregion

		#region Mouse & Draw States

		enum DrawState
		{
			Normal = 1,
			Hot,
			Pressed,
			Disabled,
			Checked,
			CheckedHot,
			CheckedPressed,
			CheckedDisabled
		}

		enum MouseState
		{
			Normal,
			Hot,
			Pressed
		}

		DrawState CurrentDrawState
		{
			get
			{
				DrawState result;

				if (CurrentMouseState == MouseState.Hot)
				{
					result = Checked ? DrawState.CheckedHot : DrawState.Hot;
				}
				else if (CurrentMouseState == MouseState.Pressed)
				{
					result = Checked ? DrawState.CheckedPressed : DrawState.Pressed;
				}
				else if (Enabled && !ReadOnly)
				{
					result = Checked ? DrawState.Checked : DrawState.Normal;
				}
				else
				{
					result = Checked ? DrawState.CheckedDisabled : DrawState.Disabled;
				}

				return result;
			}
		}

		MouseState CurrentMouseState;

		#endregion

		#region Mouse Events

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (!ReadOnly)
			{
				base.OnMouseDown(e);
				XPButtonState = XPButtonState.Pressed;
				CurrentMouseState = MouseState.Pressed;
			}
			Invalidate();
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (translationFeedbackManager.HandleClick() || devInfoPopupManager.HandleClick())
			{
				XPButtonState = XPButtonState.Normal;
				IsMouseDown = false;
				CurrentMouseState = MouseState.Hot;
			}
			else if (!ReadOnly)
			{
				base.OnMouseUp(e);
				XPButtonState = XPButtonState.Normal;
				IsMouseDown = false;
				CurrentMouseState = MouseState.Hot;
			}
			Invalidate();
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			if (!ReadOnly)
			{
				base.OnMouseEnter(e);
				XPButtonState = XPButtonState.Hot;
				CurrentMouseState = MouseState.Hot;
			}
			Invalidate();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			if (!ReadOnly)
			{
				base.OnMouseLeave(e);
			}
			XPButtonState = XPButtonState.Normal;
			CurrentMouseState = MouseState.Normal;
			Invalidate();
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (!ReadOnly)
			{
				if (!ClientRectangle.Contains(e.X, e.Y))
				{
					if (CurrentMouseState == MouseState.Pressed) // user has left button while holding mouse
					{
						IsMouseDown = true;
						CurrentMouseState = MouseState.Hot;
					}
				}
				else if (IsMouseDown && CurrentMouseState != MouseState.Pressed)
				{
					CurrentMouseState = MouseState.Pressed; // user has held mouse, left button and come back
				}
			}
		}

		bool IsMouseDown;

		protected override void OnClick(EventArgs e)
		{
			if (!ReadOnly && !TranslationFeedbackManager.InTranslationFeedbackMode())
			{
				base.OnClick(e);
			}
		}

		#endregion

		#region WndProc

		bool fullyPainted;
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "exception text, See Issue 01579103. Imperfectly correlated with low available physical || virtual memory.")]
		protected override void WndProc(ref Message m)
		{
			try
			{
				base.WndProc(ref m);

				if (m.Msg == WindowsMessage.WM_THEMECHANGED)
				{
					if (hTheme != IntPtr.Zero)
					{
						var code = NativeMethods.CloseThemeData(hTheme);
						if (code != 0)
						{
							var error = new Win32Exception(code, "ZCheckBox.cs and WndProc(ref Message) have thown an exception.");
							ErrorReporter.ReportOnce("", error);
						}

						hTheme = IntPtr.Zero;
					}
					CloseThemeData();
					Invalidate();
				}

				if (m.Msg == WindowsMessage.WM_PAINT && !fullyPainted)
				{
					fullyPainted = true;
					var form = FindForm();
					currentControlInformation = FormattableString.Invariant($@"Name: {this.Name}
ControlPath: {ControlDescription.GetControlPath(this)}
Form: {form?.Text}, with typeof {form?.GetType().FullName}");
				}
			}
			catch (ExternalException ex)
			{
				if (ex.Message != "A generic error occurred in GDI+." && ex.Message != "External component has thrown an exception.")
				{
					throw;
				}
				ZUserControl.ReportGraphicsDisplayFailure(ex);
			}
		}

		#endregion

#endif

		#endregion

		#region Visibility

		protected override void SetVisibleCore(bool value)
		{
			base.SetVisibleCore(value);
			NotificationBroadcaster.Instance.BroadcastVisibilityChange(this);
		}

		#endregion

		#region ProcessCmdKey

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			var result = false;
			if (keyData == Keys.Enter)
			{
				result = this.GetTopLevelNonParentedControl().SelectNextControlNonTabStopNonReadOnly(this, true, true, true);
			}
			if (!result)
			{
				result = base.ProcessCmdKey(ref msg, keyData);
			}
			return result;
		}

		#endregion

		#region Dispose

		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "safe to suppress since we never thow the exception.")]
		protected override void Dispose(bool disposing)
		{
#if !WINZOR
			CloseThemeData();
#endif

			if (disposing)
			{
				if (translationFeedbackManager != null)
				{
					translationFeedbackManager.Dispose();
				}
				if (devInfoPopupManager != null)
				{
					devInfoPopupManager.Dispose();
				}
				Extensions.Dispose();
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
				ColorChanger.Dispose();
#if !WINZOR
				if (hTheme != IntPtr.Zero)
				{
					var code = NativeMethods.CloseThemeData(hTheme);
					if (code != 0)
					{
						var error = new Win32Exception(code, "ZCheckBox.cs and NativeMethods.CloseThemeData() have returned an error");
						ErrorReporter.ReportOnce("", error);
					}
				}
#endif
			}

			if (TrackDisposedAccess)
			{
				disposeStack = new StackTrace();
				disposeControlPath = ControlDescription.GetControlPath(this);
			}

			base.Dispose(disposing);
		}

#if !WINZOR

		void CloseThemeData()
		{
			if (buttonThemeData != IntPtr.Zero)
			{
				XpThemeAPI.NativeMethods.CloseThemeData(buttonThemeData);
				buttonThemeData = IntPtr.Zero;
			}
		}

#endif

		#endregion

		#region Handle

		/// <summary>
		/// Overrides base CreateHandle method with adding information about dispose stack.
		/// </summary>
		protected override void CreateHandle()
		{
			try
			{
				if (!IsDisposed)
				{
					base.CreateHandle();
				}
			}
			catch (ObjectDisposedException ex)
			{
				throw new ObjectDisposedException(this.BuildDisposeInformation(currentControlInformation), ex);
			}
		}

		#endregion

		#region Disposed access tracking

		[DefaultValue(false)]
		[Browsable(false)]
		public bool TrackDisposedAccess { get; set; }

		public StackTrace DisposeStack => disposeStack;
		StackTrace disposeStack;

		public string DisposeControlPath => disposeControlPath;
		string disposeControlPath;

#pragma warning disable IDE0044 //var currentControlInformation is getting modified, conflicting readonly property
		string currentControlInformation = String.Empty;
#pragma warning restore IDE0044

		#endregion

		#region IBindTo Members

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string BindTo
		{
			get { return BindingMemberHelper.BindingMember; }
			set { BindingMemberHelper.BindingMember = value; }
		}

		ControlBindingMemberHelper BindingMemberHelper
		{
			get { return bindingMemberHelper ?? (bindingMemberHelper = ControlBindingMemberHelper.Get(this)); }
		}
		ControlBindingMemberHelper bindingMemberHelper;

		protected Type BindingSourceDataSourceType
		{
			get { return KBindingSource.GetBindingSource(this) == null ? null : KBindingSource.GetBindingSource(this).DataSourceType; }
		}

		#endregion

		#region IExtendedControl Members

		Control IExtendedControl.Host
		{
			get { return this; }
		}

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; private set; }

		#endregion

		#region IsVisibleForBinding

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZBool IsVisibleForBinding
		{
			get { return Visible; }
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
			if (IsVisibleForBindingChanged != null)
			{
				IsVisibleForBindingChanged(this, EventArgs.Empty);
			}
		}

		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZCheckBox>()
				.Property("Text", "") // Property name
								.Property("ReadOnly", false, false)
								.Property("ReadOnlyForBinding", false, false)
				.Property("ReadOnlyForBindingIsNull", false, false)
				.Property("Checked", false) // Property name
				.Property("IsVisibleForBinding", ZBool.True) // Property name
				.Result;
		}

		#endregion

		#region Implementation

		public ActiveControlColorChanger ColorChanger { get; private set; }

		[DefaultValue(false), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool EnableValidStateColor { get; set; } = false;

		[DefaultValue(false)]
		public bool EditableInViewMode { get; set; }

		protected override bool ShowFocusCues
		{
			// This method is overridden so the Tab and Enter keys behave the same way when navigating to this control from the previous control.
			// The problem was caused because this value would normally be false until the user hits the Tab key for the first time on the Form.
			get
			{
				return true;
			}
		}

#if DEBUG
		internal bool ShowFocusCues_Exposed
		{
			get { return this.ShowFocusCues; }
		}
#endif

		#endregion

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

		readonly internal TranslationFeedbackManager translationFeedbackManager;
		readonly internal DevInfoPopupManager devInfoPopupManager;
	}
}
