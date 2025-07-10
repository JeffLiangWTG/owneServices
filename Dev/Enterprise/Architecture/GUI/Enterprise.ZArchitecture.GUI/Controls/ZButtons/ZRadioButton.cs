#pragma warning disable 0809
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.Interop;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[ToolboxItem(true)]
	public class ZRadioButton : KRadioButton, IBindTo, IExtendedControl, IIsVisibleForBindingControl, IBackColorMutable, IResCaptionedControl
	{
		#region Constructors

		public ZRadioButton()
		{
			UserEventTracker.Instance.AddUserEventToControl(this);
			InitializeProperties();
			InitializeControl();
			InitializeDisposable();
			InitializeExtensions();
			translationFeedbackManager = new TranslationFeedbackManager(this);
			devInfoPopupManager = new DevInfoPopupManager(this);
		}

		#endregion

		#region Initialization

		void InitializeProperties()
		{
			AutoCheck = false;
			FlatStyle = FlatStyle.System;
			ColorChanger = new ActiveControlColorChanger(this);
		}

		void InitializeControl()
		{
			FlatStyle = FlatStyle.System;
			TabStop = false;
#if !WINZOR
			CurrentMouseState = MouseState.Normal;
#endif
		}

		void InitializeDisposable()
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		void InitializeExtensions()
		{
			Extensions = new DefaultControlExtensionCollection(this);
		}

		#endregion

		#region Not supported

		[Obsolete("TextAlign is not supported by ZRadioButton, see Geoff if you require it.", ObsoleteIsCompileError)]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new ContentAlignment TextAlign
		{
			get { return base.TextAlign; }
			set { /* so that we get the obsolete compile error when setting */ }
		}

		[Obsolete("RightToLeft is not supported by ZRadioButton, see Geoff if you require it.", ObsoleteIsCompileError)]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override RightToLeft RightToLeft
		{
			get { return base.RightToLeft; }
			set { /* so that we get the obsolete compile error when setting */ }
		}

		[Obsolete("CheckAlign is not supported by ZRadioButton, see Geoff if you require it.", ObsoleteIsCompileError)]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new ContentAlignment CheckAlign
		{
			get { return base.CheckAlign; }
			set { /* so that we get the obsolete compile error when setting */ }
		}

		const bool ObsoleteIsCompileError = true;

		#endregion

		#region Properties

		[BindingOptions(UseTypeConverters = true, UpdateDataSourceOnPropertyChange = true)]
		[DefaultValue(false)]
		public new bool Checked
		{
			get { return base.Checked; }
			set { base.Checked = value; }
		}

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
					OnReadOnlyChanged(EventArgs.Empty);
				}
			}
		}
		bool readOnly;

		void OnReadOnlyChanged(EventArgs e)
		{
			ColorChanger.SetReadonlyColor(ReadOnly);
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

		#region IsEnabledForBinding

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZBool IsEnabledForBinding
		{
			get { return Enabled; }
			set
			{
				if (!fInEnabledForBinding)
				{
					fInEnabledForBinding = true;
					Enabled = value;
					fInEnabledForBinding = false;
				}
			}
		}

		bool fInEnabledForBinding;

		protected override void SetVisibleCore(bool value)
		{
			base.SetVisibleCore(value);
			NotificationBroadcaster.Instance.BroadcastVisibilityChange(this);
		}

		#endregion

		#endregion

#if !WINZOR

		#region Drawing

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "See Issue 00852474., See Issue 00852474. Imperfectly correlated with low available physical || virtual memory.")]
		protected override void OnPaint(PaintEventArgs e)
		{
			try
			{
				if (DrawXpStyle)
				{
					DrawBackground(e.Graphics, e.ClipRectangle);
					DrawButton(e.Graphics);
					DrawText(e.Graphics);
					DrawImage(e.Graphics);

					if (Focused)
					{
						DrawFocus(e.Graphics);
					}
				}

				base.OnPaint(e);
			}
			catch (InvalidOperationException ex) { if (ex.Message != "Object is currently in use elsewhere.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
			catch (System.Runtime.InteropServices.ExternalException ex) { if (ex.Message != "A generic error occurred in GDI+." && ex.Message != "External component has thrown an exception.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
		}

		protected void DrawBackground(Graphics g, Rectangle rectangle)
		{
			g.FillRectangle(Focused ? SystemBrushes.Info : BrushProvider.FromColor(BackColor), rectangle);
		}

		protected void DrawButton(Graphics g)
		{
			const int RadioButton = 2;

			//RECT BackRect = new RECT(ClientRectangle);
			var radioRect = new RECT(ButtonArea);
			var hDC = g.GetHdc();
			try
			{
				XpThemeAPI.DrawThemeBackground(ButtonThemeData, hDC, RadioButton, (int)CurrentDrawState, ref radioRect, ref radioRect);
			}
			finally
			{
				g.ReleaseHdc(hDC);
			}
		}

		protected void DrawText(Graphics g)
		{
			var brush = BrushProvider.FromColor(Enabled ? ForeColor : SystemColors.GrayText);
			TextRendererHelper.DrawText(g, Text, Font, TextArea, brush, TextFormat);
		}

		protected virtual void DrawImage(Graphics g)
		{
		}

		protected void DrawFocus(Graphics g)
		{
			var area = FocusArea(g);
			ControlPaint.DrawFocusRectangle(g, area);
		}

		StringFormat TextFormat
		{
			get
			{
				var format =
					new StringFormat
					{
						HotkeyPrefix = ((ShowKeyboardCues) ? HotkeyPrefix.Show : HotkeyPrefix.Hide),
						Alignment = StringAlignment.Near,
						LineAlignment = StringAlignment.Center
					};

				return format;
			}
		}

		protected bool DrawXpStyle
		{
			get { return IsSystemFlatStyle && XpUtils.IsAppUsingXpTheme && XpUtils.IsWindowsXpOrGreater && XpUtils.IsWindowsUsingXpTheme; }
		}

		protected Image CurrentImage
		{
			get { return (ImageList != null && ImageIndex != -1) ? ImageList.Images[ImageIndex] : null; }
		}

		const int RadioWidth = 13;

		#region Rectangles

		protected virtual Rectangle ButtonArea
		{
			get { return ControlDpiScalingHelper.NewScaledRectangle(0, 0, ControlDpiScalingHelper.ScaleToCurrentDpiX(RadioWidth), Height, false); }
		}

		protected virtual Rectangle TextArea
		{
			get
			{
				return ControlDpiScalingHelper.NewScaledRectangle(
					ClientRectangle.X + ControlDpiScalingHelper.ScaleToCurrentDpiX(RadioWidth), ClientRectangle.Y, ClientRectangle.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(RadioWidth), ClientRectangle.Height, false);
			}
		}

		protected virtual Rectangle FocusArea(Graphics g)
		{
			var size = g.MeasureString(Text, Font, ClientRectangle.Location, TextFormat).ToSize();

			return ControlDpiScalingHelper.NewScaledRectangle(
				TextArea.X + ControlDpiScalingHelper.ScaleToCurrentDpiX(2), (ClientRectangle.Height - size.Height) / 2, size.Width, size.Height, false);
		}

		#endregion

		#region Button Theme Data

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		protected IntPtr ButtonThemeData
		{
			get
			{
				if (fButtonThemeData == IntPtr.Zero)
				{
					fButtonThemeData = XpThemeAPI.OpenThemeData(Handle, "Button");
				}

				return fButtonThemeData;
			}
		}

		IntPtr fButtonThemeData = IntPtr.Zero;

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
			base.OnMouseDown(e);

			if (!ReadOnly)
			{
				CurrentMouseState = MouseState.Pressed;
				Invalidate();
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (!ReadOnly)
			{
				IsMouseDown = false;
				CurrentMouseState = MouseState.Hot;
				Invalidate();
			}
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);

			if (!ReadOnly)
			{
				CurrentMouseState = MouseState.Hot;
				Invalidate();
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			if (!ReadOnly)
			{
				CurrentMouseState = MouseState.Normal;
				Invalidate();
			}
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

		#endregion

		#region WndProc

		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);

			if (m.Msg == WindowsMessage.WM_THEMECHANGED)
			{
				if (fButtonThemeData != IntPtr.Zero)
				{
					XpThemeAPI.NativeMethods.CloseThemeData(fButtonThemeData);
					fButtonThemeData = IntPtr.Zero;
				}

				Invalidate();
			}
		}

		#endregion

		#endregion

#endif

		#region Dispose

		protected override void Dispose(bool disposing)
		{
#if !WINZOR

			if (fButtonThemeData != IntPtr.Zero)
			{
				XpThemeAPI.NativeMethods.CloseThemeData(fButtonThemeData);
				fButtonThemeData = IntPtr.Zero;
			}

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
			}
			base.Dispose(disposing);
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

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZRadioButton>()
				.Property("Text", "") // Property name
								.Property("ReadOnly", false, false)
								.Property("ReadOnlyForBinding", false, false)
								.Property("ReadOnlyForBindingIsNull", false, false)
				.Property("Checked", false) // Property name
				.Result;
		}

		#endregion

		#region Implementation

		public ActiveControlColorChanger ColorChanger { get; private set; }

		[DefaultValue(false), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool EnableValidStateColor { get; set; } = false;

		protected override void OnClick(EventArgs e)
		{
			if (!translationFeedbackManager.HandleClick() && !ReadOnly)
			{
				foreach (Control currentControl in Parent.Controls)
				{
					if (currentControl is ZRadioButton && currentControl != this)
					{
						((ZRadioButton)currentControl).Checked = false;
						((ZRadioButton)currentControl).OnValidating(new CancelEventArgs());
					}
				}

				Checked = true;
				OnValidating(new CancelEventArgs());

				base.OnClick(e);
			}
		}

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

		protected override void OnCheckedChanged(EventArgs e)
		{
			//Only one radio button in group should have TabStop true. This is how the standard .Net radio button behaves.
			//Can use arrow keys to select a radio button within a group
			//but <tab> should tab to the next group or non-radio button control
			base.OnCheckedChanged(e);
			TabStop = Checked;
		}

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
#pragma warning restore 0809
