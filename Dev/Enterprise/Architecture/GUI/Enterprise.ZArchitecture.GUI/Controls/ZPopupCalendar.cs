using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI
{
#if DEBUG
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis] // the calendar size changes on different versions of windows :)
#endif
	public partial class ZPopupCalendar : KForm
	{
		public ZPopupCalendar()
		{
			InitializeComponent();
			this.timeZoneFindBox.TimeZoneComboBox.DataSource = this.timeZoneFindBox.TimeZoneOffsets;

			Calendar.DateSelected += HandleDateSelected;
			Calendar.KeyDown += ChildControl_KeyDown;
			TimePicker.KeyDown += ChildControl_KeyDown;

			TimePicker.CustomFormat = DateTimeFormatStrings.ShortTimeFormat;
		}

		[Browsable(false)]
		[DefaultValue(false)]
		public bool HasTimeZoneFindBox { get; set; }

		[Browsable(false)]
		[DefaultValue(false)]
		public bool ReturnsDateTimeOffset { get; set; }

		public event OnDateTimeSelected DateTimeSelected;
		public delegate void OnDateTimeSelected(object sender, DateTimeSelectedEventArgs e);

		public class DateTimeSelectedEventArgs : EventArgs
		{
			public DateTimeSelectedEventArgs(DateTime selectedDateTime)
			{
				Value = selectedDateTime;
			}

			public DateTime Value { get; set; }
		}

		public class DateTimeOffsetSelectedEventArgs : DateTimeSelectedEventArgs
		{
			public DateTimeOffsetSelectedEventArgs(DateTime selectedDateTime, TimeSpan offset) : base(selectedDateTime)
			{
				OffsetValue = offset;
			}

			public TimeSpan OffsetValue { get; set; }
		}

		public void Popup([DpiState(DpiState.ScaledVariant)] Point location, DateTime initialDateTime, KForm parentWinForm, ZDateTimePickerFormat dateTimeFormat, TimeSpan? offset = null)
		{
			if (initialDateTime < TimePicker.MinDate || initialDateTime > TimePicker.MaxDate)
			{
				initialDateTime = EnvProxy.Instance.Time.CurrentLocalDate;
			}

			if (offset.HasValue)
			{
				timeZoneFindBox.OffsetValue = offset.Value;
			}

			Location = location;
			ParentWinForm = parentWinForm;

			Calendar.SelectionStart = initialDateTime;
			Calendar.SelectionEnd = initialDateTime;
			Calendar.TodayDate = EnvProxy.Instance.Time.CurrentLocalDate;
			Calendar.FirstDayOfWeek = DayOfWeekToDay(Culture.CurrentCompanyCountryCulture.DateTimeFormat.FirstDayOfWeek);

			labelToday.Text = Res.GetString("86e4f7a6-11ce-4bc9-84b9-704f0f108a80", "Today: {0}", Calendar.TodayDate.ToString(Culture.CurrentCompanyCountryCulture.DateTimeFormat.ShortDatePattern));
			ControlDpiScalingHelper.SetLeft(ref labelToday, (panelToday.Width - labelToday.Width) / 2, false);

			TimePicker.Value = initialDateTime;

			Show();

			var isShowingTime = (dateTimeFormat == ZDateTimePickerFormat.Long);

			var height = !isShowingTime ? panelToday.Bottom : zTimePanel.Bottom;
			TimePicker.TabStop = isShowingTime;
			TimePicker.Visible = isShowingTime;

			ResizeControlsForTimeZoneFindBox();

			SuspendLayout();
			try
			{
				ClientSize = ControlDpiScalingHelper.NewScaledSize(Calendar.Width, height, false);
			}
			finally
			{
				ResumeLayout();
			}

			Activate();
			Calendar.Focus();
		}

		Day DayOfWeekToDay(DayOfWeek dayOfWeek)
		{
			switch (dayOfWeek)
			{
				case DayOfWeek.Monday:
					return Day.Monday;
				case DayOfWeek.Tuesday:
					return Day.Tuesday;
				case DayOfWeek.Wednesday:
					return Day.Wednesday;
				case DayOfWeek.Thursday:
					return Day.Thursday;
				case DayOfWeek.Friday:
					return Day.Friday;
				case DayOfWeek.Saturday:
					return Day.Saturday;
				case DayOfWeek.Sunday:
					return Day.Sunday;
				default:
					return Day.Default;
			}
		}

		public override string ToString()
		{
			return Res.GetString("8ca21eb7-a5d9-40bd-af34-8e5cfc1193f3", "{0} Name: {1}", base.ToString(), Name);
		}

		#region Implementation

		#region TimeZone Offsets

		void ResizeControlsForTimeZoneFindBox()
		{
			zTimePanel.SuspendLayout();
			timeZoneFindBox.SuspendLayout();

			if (!HasTimeZoneFindBox)
			{
				TimePicker.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 20, true);
				timeZoneFindBox.Visible = false;
			}
			timeZoneFindBox.ResumeLayout();
			zTimePanel.ResumeLayout(true);
		}

		#endregion

		protected override void OnDeactivate(EventArgs e)
		{
			if (!isDeactivating)
			{
				isDeactivating = true;
				try
				{
					Hide();

					if (!IsCancelled && DateTimeSelected != null)
					{
						var selectedDate = Calendar.SelectionStart.Date.
							AddHours(TimePicker.Value.Hour).
							AddMinutes(TimePicker.Value.Minute);

						ParentWinForm.Activate();
						if (ReturnsDateTimeOffset)
						{
							var offset = timeZoneFindBox.OffsetValue;
							DateTimeSelected(this, new DateTimeOffsetSelectedEventArgs(selectedDate, offset));
						}
						else
						{
							DateTimeSelected(this, new DateTimeSelectedEventArgs(selectedDate));
						} 
					}

					IsCancelled = false;
					if (!(!IsCancelled && DateTimeSelected != null))
					{
						ParentWinForm.Activate();
					}
				}
				finally
				{
					isDeactivating = false;
				}
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			IsCancelled = true;
			e.Cancel = true;
			base.OnClosing(e);
			Hide();
		}

		protected void HandleDateSelected(object sender, DateRangeEventArgs e)
		{
			Hide();
		}

		private void ChildControl_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				Hide();
			}
			else if (e.KeyCode == Keys.Escape)
			{
				IsCancelled = true;
				Hide();
			}
		}

		#region labelToday events

		private void labelToday_Click(object sender, EventArgs e)
		{
			Calendar.SelectionStart = Calendar.TodayDate;
			Calendar.SelectionEnd = Calendar.TodayDate;
			HandleDateSelected(sender, new DateRangeEventArgs(Calendar.SelectionStart, Calendar.SelectionEnd));
		}

		private void labelToday_MouseEnter(object sender, EventArgs e)
		{
			labelToday.ForeColor = SystemColors.HotTrack;
		}

		private void labelToday_MouseLeave(object sender, EventArgs e)
		{
			labelToday.ForeColor = SystemColors.WindowText;
		}

		#endregion

		#endregion

		#region Windows Form Designer generated code

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Editing designer window autogenerates unsimiplified names.")]
		private void InitializeComponent()
		{
			this.TimePicker = new CargoWise.Windows.UI.KDateTimePicker();
			this.Calendar = new CargoWise.Windows.UI.KMonthCalendar();
			this.labelToday = new Enterprise.ZArchitecture.ZLabel();
			this.panelToday = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.timeZoneFindBox = new Enterprise.ZArchitecture.GUI.ZTimeZoneFindBox();
			this.zTimePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.panelToday.SuspendLayout();
			this.timeZoneFindBox.SuspendLayout();
			this.zTimePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// TimePicker
			// 
			this.TimePicker.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.TimePicker.DropDownAlign = System.Windows.Forms.LeftRightAlignment.Right;
			this.TimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.TimePicker.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 0, true);
			this.TimePicker.Name = "TimePicker";
			this.TimePicker.ShowUpDown = true;
			this.TimePicker.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 19, true);
			this.TimePicker.TabIndex = 0;
			// 
			// Calendar
			// 
			this.Calendar.BackColor = System.Drawing.Color.White;
			this.Calendar.Dock = System.Windows.Forms.DockStyle.Top;
			this.Calendar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Calendar.MaxSelectionCount = 1;
			this.Calendar.Name = "Calendar";
			this.Calendar.ShowToday = false;
			this.Calendar.ShowTodayCircle = false;
			this.Calendar.TabIndex = 1;
			// 
			// labelToday
			// 
			this.labelToday.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.labelToday.AutoSize = true;
			this.labelToday.BackColor = System.Drawing.Color.Transparent;
			this.labelToday.FontType = (Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif);
			this.labelToday.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 2, true);
			this.labelToday.Name = "labelToday";
			this.labelToday.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.labelToday.TabIndex = 0;
			this.labelToday.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.labelToday.UseMnemonic = false;
			this.labelToday.Click += new System.EventHandler(this.labelToday_Click);
			this.labelToday.MouseEnter += new System.EventHandler(this.labelToday_MouseEnter);
			this.labelToday.MouseLeave += new System.EventHandler(this.labelToday_MouseLeave);
			// 
			// panelToday
			// 
			this.panelToday.Controls.Add(this.labelToday);
			this.panelToday.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelToday.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 159, true);
			this.panelToday.Name = "panelToday";
			this.panelToday.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 20, true);
			this.panelToday.TabIndex = 2;
			// 
			// timeZoneFindBox
			// 
			this.timeZoneFindBox.AllowDrop = true;
			this.timeZoneFindBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.timeZoneFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 0, true);
			this.timeZoneFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.timeZoneFindBox.Name = "timeZoneFindBox";
			this.timeZoneFindBox.OffsetValue = System.TimeSpan.Parse("00:00:00");
			this.timeZoneFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.timeZoneFindBox.ParentType = null;
			this.timeZoneFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.timeZoneFindBox.TabIndex = 3;
			// 
			// zTimePanel
			// 
			this.zTimePanel.Controls.Add(this.timeZoneFindBox);
			this.zTimePanel.Controls.Add(this.TimePicker);
			this.zTimePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.zTimePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 179, true);
			this.zTimePanel.Name = "zTimePanel";
			this.zTimePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 20, true);
			this.zTimePanel.TabIndex = 4;
			// 
			// ZPopupCalendar
			// 
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 205, true);
			this.Controls.Add(this.zTimePanel);
			this.Controls.Add(this.panelToday);
			this.Controls.Add(this.Calendar);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ZPopupCalendar";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.panelToday.ResumeLayout(false);
			this.panelToday.PerformLayout();
			this.timeZoneFindBox.ResumeLayout(true);
			this.timeZoneFindBox.PerformLayout();
			this.zTimePanel.ResumeLayout(false);
			this.zTimePanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected bool IsCancelled;
		protected KForm ParentWinForm;
		protected KMonthCalendar Calendar;
		private ZLabel labelToday;
		private ZPanel panelToday;
		bool isDeactivating;
#if DEBUG
		public
#else
		protected
#endif
		KDateTimePicker TimePicker;

#if DEBUG
		public
#else
		private
#endif
		ZTimeZoneFindBox timeZoneFindBox;
		private ZPanel zTimePanel;
	}
}
