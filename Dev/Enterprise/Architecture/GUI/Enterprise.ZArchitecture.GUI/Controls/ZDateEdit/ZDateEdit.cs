using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Core.Forms;
using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Balloons;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Notifications;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressFormDesignerAnalysis]
	[DefaultDataSourceBindingMember(null)]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[Designer(typeof(Designer))]
	[ToolboxItem(true)]
	[DefaultBindingProperty("DateTimeValue")]
	[FrontMostControlProvider(typeof(ZDateEditFrontMostControlProvider))]
	public partial class ZDateEdit : ZUserControl, IDateInputControl, IExtendedControl, IGridControl, IBindTo, IBackColorMutable, IPastableControl
	{
		#region Bare

		[ToolboxItem(false)]
		public class Bare : ZDateEdit
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				return new ControlExtensionCollection(this);
			}
		}

		#endregion

		#region Core

		protected ZDateEditCore Core;

		protected virtual internal ZDateEditCore GetNewCore()
		{
			return new ZDateEditCore(this);
		}

		#endregion

		#region Constructors

		static ZDateEdit()
		{
			NotificationAdornmentFactory.RegisterCustomLayout(new ZDateEditAdornmentLayout());
		}

		public ZDateEdit()
		{
			InitializeComponent();
			DateTimeFormat = ZDateTimePickerFormat.Short;

			AutoCompleteYear = true;
			AutoCompleteMonthThreshold = 1;
			Core = GetNewCore();
			Extensions = NewExtensionCollection();

			DateTextBox.Validating += (sender, e) => OnValidating(e);
			DateTextBox.TextChanged += DateTextBox_TextChanged;
			DateTextBox.ReadOnlyChanged += (sender, args) => OnReadOnlyChanged();
			DateTextBox.Resize += DateTextBox_Resize;
			InitializeComponentWhenNotDesignMode();

			DisposableLeakListener.Instance.RegisterDisposable(this);
			DateTextBox.KeyDown += DateTextBox_KeyDown;

			RegisterHotkeys();
		}

		protected virtual IControlExtensionCollection NewExtensionCollection()
		{
			return new DefaultControlExtensionCollection(this);
		}

		#endregion

		#region Properties

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new ControlBindingsCollection DataBindings
		{
			// Hiding the base property prevents serialisation in the InitializeComponents
			get { return base.DataBindings; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string Text
		{
			get { return DateTextBox.Text; }
			set { DateTextBox.Text = value; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsFixedReadOnly { get; set; }

		#region Format String

		public string FormatString
		{
			get { return formatString; }
		}

		public static string GetFormatStringFromZDateTimePickerFormat(ZDateTimePickerFormat dateTimeFormat)
		{
			switch (dateTimeFormat)
			{
				case ZDateTimePickerFormat.Long:
					return DateTimeFormatStrings.LongTimeFormat;
				case ZDateTimePickerFormat.Time:
					return DateTimeFormatStrings.ShortTimeFormat;
				case ZDateTimePickerFormat.LongIncludingSeconds:
					return DateTimeFormatStrings.LongTimeIncludingSecondsFormat;
				case ZDateTimePickerFormat.TimeIncludingSeconds:
					return DateTimeFormatStrings.ShortTimeIncludingSecondsFormat;
				default:
					return DateTimeFormatStrings.ShortDateFormat;
			}
		}

		[DefaultValue(ZDateTimePickerFormat.Short)]
		public ZDateTimePickerFormat DateTimeFormat
		{
			get { return dateTimeFormat; }
			set
			{
				dateTimeFormat = value;
				formatString = GetFormatStringFromZDateTimePickerFormat(value);

				switch (value)
				{
					case ZDateTimePickerFormat.Long:
						CalendarButton.Enabled = true;
						if (Text.Length == 9)
						{
							Text += " 00:00";           // Append blank time
							ResetTimeValueIfNeeded();
						}
						SetWidthIfNotOnGrid(ControlDpiScalingHelper.ScaleToCurrentDpiX(DateTimeFormatWidth));
						break;
					case ZDateTimePickerFormat.LongIncludingSeconds:
						CalendarButton.Enabled = true;
						if (Text.Length.Equals(9))
						{
							Text += " 00:00:00";
						}
						SetWidthIfNotOnGrid(ControlDpiScalingHelper.ScaleToCurrentDpiX(DateTimeFormatIncludingSecondsWidth));
						break;
					case ZDateTimePickerFormat.Time:
						CalendarButton.Enabled = false;
						SetWidthIfNotOnGrid(ControlDpiScalingHelper.ScaleToCurrentDpiX(TimeFormatWidth));
						break;
					case ZDateTimePickerFormat.TimeIncludingSeconds:
						CalendarButton.Enabled = false;
						SetWidthIfNotOnGrid(ControlDpiScalingHelper.ScaleToCurrentDpiX(TimeFormatIncludingSecondsWidth));
						break;
					default:
						CalendarButton.Enabled = true;
						if (Text.Length >= 10 && Text[9] == ' ')
						{
							Text = Text.Substring(0, 9);    // Remove existing time
						}
						SetWidthIfNotOnGrid(GetDateFormatWidth());
						break;
				}
			}
		}

		protected virtual void ResetTimeValueIfNeeded()
		{
		}

		#endregion

		#region Size

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new int Width
		{
			get { return base.Width; }
			[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Inside the implementation of the setter")]
			set { base.Width = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new int Height
		{
			get { return base.Height; }
			[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Inside the implementation of the setter")]
			set { base.Height = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new Size Size
		{
			get { return base.Size; }
			set { base.Size = value; }
		}

		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore(x, y, width, height, specified);

			if (IsOnGrid)
			{
				var proposedTextBoxWidth = Width - CalendarButton.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(3);
				if (proposedTextBoxWidth > 0 && DateTextBox.Width > proposedTextBoxWidth)
				{
					ControlDpiScalingHelper.SetWidth(DateTextBox, Width - CalendarButton.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(3), false);
				}
			}
		}

		#endregion

		#region ReadOnly

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnly
		{
			get { return DateTextBox.ReadOnly; }
			set
			{
				if (ReadOnly != value)
				{
					DateTextBox.ReadOnly = value;
					CalendarButton.Visible = !DateTextBox.ReadOnly;
				}
			}
		}

		public event EventHandler ReadOnlyChanged
		{
			add { DateTextBox.ReadOnlyChanged += value; }
			remove { DateTextBox.ReadOnlyChanged -= value; }
		}

		protected void OnReadOnlyChanged()
		{
			ReadOnlyForBindingProperty.OnReadOnlyChanged();
		}

		#endregion

		#region Binding ReadOnly

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This property is intended for data binding only")]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnlyForBinding
		{
			get { return ReadOnlyForBindingProperty.ReadOnlyForBinding; }
			set { ReadOnlyForBindingProperty.ReadOnlyForBinding = value; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This property is intended for data binding only")]
		public event EventHandler ReadOnlyForBindingChanged
		{
			add { ReadOnlyForBindingProperty.ReadOnlyForBindingChanged += value; }
			remove { ReadOnlyForBindingProperty.ReadOnlyForBindingChanged -= value; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This property is intended for data binding only")]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnlyForBindingIsNull
		{
			get { return false; }
			set
			{
				if (value)
				{
					ReadOnlyForBinding = true;
				}
			}
		}

		protected virtual ControlReadOnlyPropertyHelper ReadOnlyForBindingProperty
		{
			get
			{
				if (readOnlyForBindingProperty == null)
				{
					readOnlyForBindingProperty = new ControlReadOnlyPropertyHelper(
						this,
						() => ReadOnly,
						value => ReadOnly = value);
				}
				return readOnlyForBindingProperty;
			}
		}
		ControlReadOnlyPropertyHelper readOnlyForBindingProperty;

		#endregion

		#region IsOnGrid

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override bool IsOnGrid
		{
			get { return base.IsOnGrid; }
			set
			{
				base.IsOnGrid = value;
				if (IsOnGrid)
				{
					DateTextBox.BorderStyle = BorderStyle.None;
					DateTextBox.Dock = DockStyle.None;
					DateTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
					ControlDpiScalingHelper.SetLeft(DateTextBox, 2, true);
					ControlDpiScalingHelper.SetTop(DateTextBox, 2, true);
					ControlDpiScalingHelper.SetWidth(DateTextBox, Width - CalendarButton.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(3), false);
				}
			}
		}

		#endregion

		#region DateTimeValue

		[DefaultValue("")]
		[Bindable(true)]
		[BindingMetaDataProperty(MetaDataTypes.ReadOnly, "ReadOnlyForBinding")]
		public virtual ZDateTime DateTimeValue
		{
			get
			{
				if (pushValueRequired)
				{
					PushValue();
				}
				return dateTimeValue;
			}
			set
			{
				Text = Core.DateToString(value, Text);
				lastParsedValue = Text;
				dateTimeValue = value;
			}
		}
		ZDateTime dateTimeValue;

		public event EventHandler DateTimeValueChanged;

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			pushValueRequired = true;
			if (DateTimeValueChanged != null)
			{
				DateTimeValueChanged(this, e);
			}
		}

		#endregion

		#region AutoCompleteYear

		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue(true)]
		public virtual bool AutoCompleteYear { get; set; }

		[DefaultValue(0)]
		public virtual int AutoCompleteMonthThreshold
		{
			get { return fAutoCompleteMonthThreshold; }
			set
			{
				if (value < 0 || value > 6)
				{
					throw new ArgumentOutOfRangeException(nameof(value), "Please enter a value within the range 1 - 6.");
				}

				fAutoCompleteMonthThreshold = value;
			}
		}
		private int fAutoCompleteMonthThreshold;

		#endregion

		#region CalendarButtonWidth

		protected virtual int GetCalendarButtonWidth()
		{
			return ControlDpiScalingHelper.ScaleToCurrentDpiX(CalendarButtonWidthZ);
		}

		public const int CalendarButtonWidth = 21;
		public const int CalendarButtonWidthZ = 23;

		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZDateEdit>()
				.Property("DateTimeValue", ZDateTime.Empty)
				.Property("ReadOnly", true)
				.Property("IsVisibleForBinding", ZBool.True)
				.Property("ReadOnlyForBinding", true)
				.Property("ReadOnlyForBindingIsNull", true, false)
				.Result;
		}

		#endregion

		#endregion

		#region Designer Support

		internal sealed class Designer : GenericControlDesignerWithTextBoxSnapLine<ZDateEdit>
		{
			protected override TextBox GetTextBox(ZDateEdit userControl)
			{
				return userControl.DateTextBox;
			}
		}

		#endregion

		#region Custom Adornment Layout

		class ZDateEditAdornmentLayout : AdornmentLayout<ZDateEdit>
		{
			public override IEnumerable<Control> GetBackroundAdornmentTargets(ZDateEdit source)
			{
				yield return source.DateTextBox;
			}

			public override IEnumerable<IIconLayout> GetIconAdornmentTargets(ZDateEdit source)
			{
				yield return
					source.CalendarButton.Visible
						? new IconLayout(source.CalendarButton, IconAlignment.Center)
						: new IconLayout(source.DateTextBox, IconAlignment.Right);
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Extensions.Dispose();
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
				DataBindings.Clear();
				if (calendar != null)
				{
					calendar.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		#endregion

		#region IBindTo Interface

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

		#region IDataBoundControl

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			DataBoundControl.GetDefaultImplementation(this).SetDataBinding(dataSource, dataMember);
			this.dataSource = dataSource;
			this.dataMember = dataMember;
		}

		protected override object DataSourceCore
		{
			get { return dataSource; }
		}
		object dataSource;

		protected override string DataMemberCore
		{
			get { return dataMember; }
		}
		string dataMember;

		#endregion

		#region IGridControl Members

		int IGridControl.SelectionStart
		{
			get { return DateTextBox.SelectionStart; }
			set { DateTextBox.SelectionStart = value; }
		}

		int IGridControl.SelectionLength
		{
			get { return DateTextBox.SelectionLength; }
			set { DateTextBox.SelectionLength = value; }
		}

		string IGridControl.Text
		{
			get { return DateTextBox.Text; }
			set { DateTextBox.Text = value; }
		}

		int IGridControl.ButtonWidth
		{
			get { return ControlDpiScalingHelper.ScaleToCurrentDpiX(CalendarButtonWidth); }
		}

		event KeyEventHandler IGridControl.KeyDown
		{
			add { DateTextBox.KeyDown += value; }
			remove { DateTextBox.KeyDown -= value; }
		}

		int IGridControl.MaxLength
		{
			get { return 0; }
			set { }
		}

		void IGridControl.ActivateEditControl()
		{
			DateTextBox.Focus();
		}

		bool IGridControl.ShouldHandleKey(Keys keyData)
		{
			return false;
		}

		bool IGridControl.ShownForReadOnly
		{
			get { return false; }
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

		#region Events

		public event EventHandler SelectedDateChanged;
		public event EventHandler SelectFromPopup;

		#endregion

#pragma warning disable IDE0001 // Simplify Names
		#region Component Designer generated code

		private void InitializeComponent()
		{
			this.DateTextBox = new Enterprise.ZArchitecture.ZTextBox.Bare();
			this.CalendarButton = GetNewCalendarButton();
			this.SuspendLayout();
			// 
			// DateTextBox
			// 
			this.DateTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DateTextBox.Name = "DateTextBox";
			this.DateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.DateTextBox.TabIndex = 0;
			this.DateTextBox.Text = "";
			this.DateTextBox.TrackDisposedAccess = true;
			// 
			// CalendarButton
			// 
			this.CalendarButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.CalendarButton.Name = "CalendarButton";
			this.CalendarButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 0, true);
			this.CalendarButton.TabIndex = 1;
			this.CalendarButton.TabStop = false;
			this.CalendarButton.Click += new System.EventHandler(this.CalendarButton_Click);
			// 
			// ODateEdit
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DateTextBox);
			try
			{
				this.Controls.Add(this.CalendarButton);
			}
			catch (ArgumentException)
			{
				//Parameter is not valid. Possibly caused by OOM/GDI object leak/faulty .NET installation/bad codec/video driver/???.
				this.CalendarButton.Image = null;
				this.Controls.Remove(this.CalendarButton);
				this.Controls.Add(this.CalendarButton);
			}
			this.Name = "ODateEdit";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.Resize += new System.EventHandler(this.ODateEdit_Resize);
			this.ResumeLayout(false);
#if WINZOR
			monthCalendar = new MonthCalendar();
			monthCalendar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1, 20, true);
			monthCalendar.TabStop = false;
			monthCalendar.DateChanged += OnDateChanged;
			this.DateTextBox.AllowOverlap(monthCalendar);
			this.WinzorSpecificControls.Add(monthCalendar);
#endif
		}

#if WINZOR
		internal MonthCalendar monthCalendar;
		void OnDateChanged(object sender, DateRangeEventArgs value)
		{
			if (!ReadOnly)
			{
				Calendar.OnDateChanged(value.End);
				DateTimeValue = value.End;
				if(monthCalendar.DateTime == default)
				{
					DateTimeValue = ZDateTime.Empty;
				}
			}
		}
#endif

		protected void InitializeComponentWhenNotDesignMode()
		{
			ControlDpiScalingHelper.SetWidth(ref CalendarButton, GetCalendarButtonWidth(), false);
			DateTextBox.Enter += new EventHandler(DateTextBox_Enter);
			Extensions.SetDataBinding(null, "");

			if (calendar != null)
			{
				calendar.Dispose();
				calendar = null;
			}
		}

		#endregion
#pragma warning restore IDE0001 // Simplify Names

		#region Implementation

		internal const int DateFormatWidth = 85;
		internal const int DateTimeFormatWidth = 116;
		internal const int TimeFormatWidth = 65;
		internal const int DateTimeFormatIncludingSecondsWidth = 130;
		internal const int TimeFormatIncludingSecondsWidth = 80;

		protected string lastParsedValue = string.Empty;
		protected bool pushValueRequired;

		ZDateTimePickerFormat dateTimeFormat;
		string formatString;
		ZPopupCalendar calendar;

		public ZButton CalendarButton;

		ActiveControlColorChanger IBackColorMutable.ColorChanger
		{
			get { return DateTextBox != null ? DateTextBox.ColorChanger : null; }
		}

		[DefaultValue(false), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool EnableValidStateColor { get; set; } = false;

		public ZTextBox DateTextBox { get; private set; }

		protected virtual int GetDateFormatWidth()
		{
			return ControlDpiScalingHelper.ScaleToCurrentDpiX(DateFormatWidth);
		}

		protected ZPopupCalendar Calendar
		{
			get
			{
				if (calendar == null)
				{
					calendar = new ZPopupCalendar { Name = (Name + ".CalendarPopup") };
					calendar.DateTimeSelected += DateTimeSelectedFromPopup;
				}
				return calendar;
			}
		}

		protected static Image ButtonImage
		{
			get
			{
				if (buttonImage == null)
				{
					var resources = new System.Resources.ResourceManager(typeof(ZDateEdit));
					buttonImage = ((Image)(resources.GetObject("CalendarButton.Image"))); // This is architecture GUI control
				}
				return buttonImage;
			}
		}
		[ThreadStatic]
		static Image buttonImage;

		#region Popup

		protected virtual void OnSelectedDateChanged()
		{
			if (SelectedDateChanged != null)
			{
				SelectedDateChanged(this, new EventArgs());
			}
		}

		protected virtual void OnSelectFromPopup()
		{
			if (SelectFromPopup != null)
			{
				SelectFromPopup(this, new EventArgs());
			}
		}

		protected internal void DateTimeSelectedFromPopup(object sender, ZPopupCalendar.DateTimeSelectedEventArgs e)
		{
			var parentOWinForm = FindForm() as ZForm;
			try
			{
				if (parentOWinForm != null)
				{
					parentOWinForm.IsChangingWithoutFocus = true;
				}

				SetDateTimeSelected(e);
				OnSelectFromPopup();
				ValidateBoundData();
			}
			finally
			{
				if (parentOWinForm != null)
				{
					parentOWinForm.IsChangingWithoutFocus = false;
				}

#if !WINZOR
				if (ObjectFactory.Get<TerminalService>().IsRemoteAppSession)
				{
					parentOWinForm?.Activate();
				}
#endif
			}
		}

		protected bool IsValidating
		{
			get { return validating; }
		}
		bool validating;

		protected void ValidateBoundData()
		{
			validating = true;
			try
			{
				Validate();
				OnValidating(new CancelEventArgs());
			}
			finally
			{
				validating = false;
			}
		}

		protected virtual void OnBeforeShowingPopupCalendar()
		{
		}

		protected void ShowPopupCalendar()
		{
			if (Parent != null)
			{
				OnBeforeShowingPopupCalendar();

				var initialDateTime = GetDateTimeForPopupCalendar();
				DateTextBox.Focus();
#if !WINZOR
				var initialLocation = Parent.PointToScreen(ControlDpiScalingHelper.NewScaledPoint(Left, Bottom, false));
				Calendar.Popup(initialLocation, initialDateTime, (KForm)FindForm(), DateTimeFormat, GetOffsetForPopupCalendar());
				Calendar.Location = GetPopupLocation();
#else
				//Here we set the timezone value in Winzor regardless of GetOffsetForPopupCalendar
				//sightly different to CW1 because we use two way bindings for values in calendar razor
				//the value will set when we show the calendar
				if (!ReadOnly && DateTimeValue.IsEmpty)
				{
					DateTimeValue = initialDateTime;
				}
				monthCalendar.ShowTime = IsShowingTime;
				monthCalendar.SetDate(initialDateTime);
				Calendar.SetDateTimeOffset(GetOffsetForPopupCalendar());
				if (!ReadOnly)
				{
					monthCalendar.ShowCalendar();
				}
#endif
			}
		}

#if !WINZOR
		[return: DpiState(DpiState.ScaledVariant)]
		Point GetPopupLocation()
		{
			var popupLocation = Parent.PointToScreen(ControlDpiScalingHelper.NewScaledPoint(Left, Bottom, false));

			var currentScreenRect = CachedScreenInfo.Instance.FromControl(this);
			var horizontalStateRight = CachedScreenInfo.Instance.GetHorizontalState(ControlDpiScalingHelper.NewScaledPoint(popupLocation.X + Calendar.Width, popupLocation.Y, false));

			if (horizontalStateRight == HorizontalState.OffScreenRight)
			{
				ControlDpiScalingHelper.SetX(ref popupLocation, currentScreenRect.Right - Calendar.Width, false);
			}

			var verticalStateBottom = CachedScreenInfo.Instance.GetVerticalState(popupLocation, Calendar.Height);

			if (verticalStateBottom == VerticalState.OffScreenBottom)
			{
				ControlDpiScalingHelper.SetY(ref popupLocation, popupLocation.Y - Calendar.Height - Height, false);
			}

			return popupLocation;
		}
#endif

		protected internal virtual TimeSpan? GetOffsetForPopupCalendar()
		{
			return null;
		}

		internal DateTime GetDateTimeForPopupCalendar()
		{
			var initialDateTime = GetDateTimeForPopupCalendarCore();

			if (initialDateTime == null || initialDateTime == DateTime.MinValue)
			{
				initialDateTime = GetFormattedCurrentDateTime();
			}
			return (DateTime)initialDateTime;
		}

		DateTime GetFormattedCurrentDateTime()
		{
			var env = EnvProxy.Instance;
			return DateTimeFormat == ZDateTimePickerFormat.Short ? env.Time.CurrentLocalDate : env.Time.CurrentLocalDateTime;
		}

		protected virtual void SetDateTimeSelected(ZPopupCalendar.DateTimeSelectedEventArgs e)
		{
			if (popupUpdate && valueBeforePopup != DateTimeValue)
			{
				popupUpdate = false;
				return;
			}

			DateTimeValue = new ZDateTime(e.Value);

			if (DateTimeValue.IsEmpty)
			{
				DateTimeValue = ZDateTime.Empty;
			}
			else if (!DateTimeValue.IsValid || !DateTimeValue.IsValidSmallDateTime)
			{
				DateTimeValue = ZDateTime.Invalid;
			}
		}

		protected virtual internal DateTime? GetDateTimeForPopupCalendarCore()
		{
			return DateTimeValue.IsValid ? DateTimeValue.ToDateTime() : null;
		}

		protected void CalendarButton_Click(object sender, EventArgs e)
		{
			valueBeforePopup = DateTimeValue;
			popupUpdate = true;
			Balloon.Instance.Hide();
#if WINZOR
			OnEnter(e);
#endif
			ShowPopupCalendar();
		}

		ZDateTime valueBeforePopup;
		bool popupUpdate;

		#endregion

		#region Handling Special Keys

		public override string TypeNameForDisplay => Res.GetString("79a5715a-0b23-4eaa-b658-e5fb702f7fed", "Date Field");

		void RegisterHotkeys()
		{
			Hotkeys.RegisterHotKey(Keys.F4, WrapReadOnly(ShowPopupCalendar), Res.GetString("79804f55-b1c9-4c82-8380-e48195fe16cf", "Show calendar"));
			Hotkeys.RegisterHotKey(Keys.F5, WrapReadOnly(() => Text = EnvProxy.Instance.Time.CurrentLocalDateTime.ToString(FormatString, CultureInfo.CurrentCulture)), Res.GetString("58a11633-abab-4729-b143-49c09769e7e0", "Set to current time"));

			Hotkeys.RegisterHotKey(Keys.Control | Keys.Up, (o, k) => IncrementTextAsDateTime(0, 0, 1, 0), Res.GetString("158e5d4c-eddb-4c01-b76a-685692b17bf9", "Add one day"));
			Hotkeys.RegisterHotKey(Keys.Control | Keys.Down, (o, k) => IncrementTextAsDateTime(0, 0, -1, 0), Res.GetString("4611a35d-a68a-4d93-8f12-4e6990b7fffd", "Subtract one day"));

			Hotkeys.RegisterHotKey(Keys.Control | Keys.Right, (o, k) => TryIncrementHour(1), Res.GetString("eacef838-1b13-4b18-9ccf-dd3322719dd5", "Add one hour"));
			Hotkeys.RegisterHotKey(Keys.Control | Keys.Left, (o, k) => TryIncrementHour(-1), Res.GetString("9e45099f-e7dd-4823-ba1e-c63dd4e04a99", "Subtract one hour"));

			Hotkeys.RegisterHotKey(Keys.Alt | Keys.Up, (o, k) => IncrementTextAsDateTime(0, 1, 0, 0), Res.GetString("a5181fa3-22bd-47d5-83de-457181b6f275", "Add one month"));
			Hotkeys.RegisterHotKey(Keys.Alt | Keys.Down, (o, k) => IncrementTextAsDateTime(0, -1, 0, 0), Res.GetString("34818e35-1d91-4fcd-823d-3793e540ad7b", "Subtract one month"));

			Hotkeys.RegisterHotKey(Keys.Control | Keys.Alt | Keys.Up, (o, k) => IncrementTextAsDateTime(1, 0, 0, 0), Res.GetString("58644b5f-6575-44ea-9021-eaedf66320c6", "Add one year"));
			Hotkeys.RegisterHotKey(Keys.Control | Keys.Alt | Keys.Down, (o, k) => IncrementTextAsDateTime(-1, 0, 0, 0), Res.GetString("95ccb0ca-d83c-46b3-b6a2-aa672c6dcd9f", "Subtract one year"));

			Hotkeys.AddDescription(nameof(Keys.T), Res.GetString("c6893529-5b9b-4319-9cea-689857581eab", "(Macro) Inserts current date and time."));
			Hotkeys.AddDescription(nameof(Keys.Y), Res.GetString("ffde194a-bfb8-4c76-8a93-913a3f107a17", "(Macro) Inserts yesterday's date at current time."));

			Hotkeys.AddDescription(nameof(Keys.S), Res.GetString("062cdec2-bff6-4f97-af2d-741a233f2caf", "(Macro) Inserts today's starting time for the current logged in user."));
			Hotkeys.AddDescription(nameof(Keys.E), Res.GetString("254179bc-e2c5-4a41-9fa9-7913cdf24b1c", "(Macro) Inserts today's ending time for the current logged in user."));

			Hotkeys.AddDescription(nameof(Keys.D) + nameof(Keys.S), Res.GetString("512f2a80-b26e-4981-afcc-f0acfc14ef04", "(Macro) Inserts today's starting time for the current logged in department."));
			Hotkeys.AddDescription(nameof(Keys.D) + nameof(Keys.E), Res.GetString("df854f32-f81a-49a4-adfd-b143ccbca89c", "(Macro) Inserts today's ending time for the current logged in department."));
		}

		HotKeyPressedProcessor WrapReadOnly(Action action)
		{
			return (o, k) =>
			{
				if (!ReadOnly)
				{
					action();
					return true;
				}
				return false;
			};
		}

		bool TryIncrementHour(int hours)
		{
			if (!ReadOnly && IsShowingTime)
			{
				IncrementTextAsDateTime(0, 0, 0, hours);
				return true;
			}

			return false;
		}

		protected bool IncrementTextAsDateTime(int years, int months, int days, int hours)
		{
			if (!ReadOnly && StringToDateTime(Text) != DateTime.MinValue)
			{
				Text = StringToDateTime(Text)
					.AddMonths(12 * years + months)
					.Add(new TimeSpan(days, hours, 0, 0))
					.ToString(FormatString, CultureInfo.InvariantCulture);

				return true;
			}

			return false;
		}

		DateTime StringToDateTime(string text)
		{
			var result = Core.StringToDate(text);

			if (result.IsValid)
			{
				return result.ToDateTime();
			}
			else
			{
				return DateTime.MinValue;
			}
		}

		#endregion

		#region Parse & Format

		protected internal string InputText
		{
			get { return inputText; }
			set { inputText = value; }
		}
		string inputText = "";

		protected virtual void DateToString(object sender, ConvertEventArgs e)
		{
			e.Value = e.Value != null ? Core.DateToString(e.Value, InputText) : "";
		}

		protected virtual void StringToDate(object sender, ConvertEventArgs e)
		{
			try
			{
				InputText = e.Value.ToString();
				e.Value = Core.StringToDate(InputText.Trim());
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ex.Data["InputText"] = InputText;
				throw;
			}
		}

		#endregion

		void DateTextBox_TextChanged(object sender, EventArgs e)
		{
			OnTextChanged(e);
		}

		void DateTextBox_Enter(object sender, EventArgs e)
		{
			DateTextBox.SelectAll();
		}

		void DateTextBox_Resize(object sender, EventArgs e)
		{
			SynchroniseControlSize();
		}

		void ODateEdit_Resize(object sender, EventArgs e)
		{
			SynchroniseControlSize();
		}

		void SynchroniseControlSize()
		{
			if (!IsOnGrid && Height != DateTextBox.Height)
			{
				ControlDpiScalingHelper.SetHeight(this, DateTextBox.Height, false);
			}

			if (DesignMode && !IsOnGrid && Width != ControlDpiScalingHelper.ScaleToCurrentDpiX(DateTimeFormatWidth))
			{
				ControlDpiScalingHelper.SetWidth(this, DateTimeFormatWidth, true);
			}
		}

		bool IsShowingTime
		{
			get { return (DateTimeFormat == ZDateTimePickerFormat.Time) || (DateTimeFormat == ZDateTimePickerFormat.Long); }
		}

		void SetWidthIfNotOnGrid(int newWidth)
		{
			if (!IsOnGrid)
			{
				ControlDpiScalingHelper.SetWidth(this, newWidth, false);
			}
		}

		void DateTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (IsOnGrid && e.KeyData == Keys.F2)
			{
				DateTextBox.SelectionLength = 0;
				DateTextBox.SelectionStart = DateTextBox.Text.Length;
			}
		}

		protected override void SetVisibleCore(bool value)
		{
			base.SetVisibleCore(value);
			NotificationBroadcaster.Instance.BroadcastVisibilityChange(this);
		}

		protected internal virtual void PushValue()
		{
			if (!DateTextBox.Text.Equals(lastParsedValue, StringComparison.OrdinalIgnoreCase))
			{
				dateTimeValue = Core.StringToDate(DateTextBox.Text); //We don't want to set Text when we just read the value from Text, so access field directly.
				lastParsedValue = Text;
			}
		}

		protected virtual ZButton GetNewCalendarButton()
		{
			return new ZDateEditCalendarButton { Size = ControlDpiScalingHelper.NewScaledSize(CalendarButtonWidthZ, 24) };
		}
#if DEBUG
		internal bool ProcessCmdKeyExposed(ref Message msg, Keys keyData)
		{
			return ProcessCmdKey(ref msg, keyData);
		}
#endif
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if ((keyData == (Keys.Control | Keys.X) || keyData == (Keys.Control | Keys.C)) && !string.IsNullOrEmpty(Text))
			{
				SafeClipboard.SetText(Text);
				if (keyData == (Keys.Control | Keys.X))
				{
					Text = ZString.Empty;
				}
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		#endregion

#if !WINZOR

		bool IPastableControl.TryPaste()
		{
			var dataObject = SafeClipboard.GetDataObject();
			var value = (string)dataObject?.GetData(typeof(string));

			if (value == null || !ZDateTime.TryParseISO8601Date(value, out _) && !ZDate.TryParseJulianDate(value, out _))
			{
				return false;
			}

			Text = value;
			return true;
		}

#endif
	}

	#region ZDateEditFrontMostControlProvider

	public class ZDateEditFrontMostControlProvider : DefaultFrontMostControlProvider
	{
		public override Control GetFrontMostControl(Control control)
		{
			return control;
		}
	}

	#endregion
}
