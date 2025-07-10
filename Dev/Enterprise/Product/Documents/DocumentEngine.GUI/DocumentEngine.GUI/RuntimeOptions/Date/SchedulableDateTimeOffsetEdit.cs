
using System;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.GUI.Scheduler;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	public partial class SchedulableDateTimeOffsetEdit : ZUserControl, IBindTo
	{
		public SchedulableDateTimeOffsetEdit()
		{
			InitializeComponent();
			ValueDateEdit.AllowOverlap(EditButton);
		}

		[AttributeProvider("Enterprise.ZArchitecture.BindToPropertyAttributes, Enterprise.ZArchitecture.GUI", nameof(BindTo))]
		public string BindTo
		{
			get { return ValueDateEdit.BindTo; }
			set { ValueDateEdit.BindTo = value; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			UpdateLayout(EditButton.Visible);
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			UpdateLayout(EditButton.Visible);
		}

		bool? shouldShowHourMinute;

		public bool ShouldShowHourMinute
		{
			get { return shouldShowHourMinute ?? false; }
			set
			{
				if (value)
				{
					ValueDateEdit.KeepLengthAlwaysLong = true;
				}

				shouldShowHourMinute = value;
			}
		}

		public ZDateTimePickerFormat DateTimeFormat
		{
			get { return ValueDateEdit.DateTimeFormat; }
			set
			{
				if (DateTimeFormat != value && schedule == null)
				{
					ValueDateEdit.DateTimeFormat = value;

					if (shouldShowHourMinute == null)
					{
						ShouldShowHourMinute = (value == ZDateTimePickerFormat.Long);
					}
				}
			}
		}

		public DateSchedule Schedule
		{
			get { return schedule; }
		}

		public void SetSchedule(DateSchedule value)
		{
			bool hasSchedule = (value != null);
			try
			{
				if (hasSchedule)
				{
					EditButton.Visible = true;
				}
				else
				{
					EditButton.Visible = false;
				}

				schedule = value;

				UpdateLayout(hasSchedule);
			}
			catch (NullReferenceException ex)
			{
				ErrorReporter.ReportOnce(string.Format("Error Setting Schedule, ValueDateEdit is {0}, EditButton is {1}.", ValueDateEdit == null ? "null" : (NoResString)"not null", EditButton == null ? "null" : (NoResString)"not null"), ex);
			}
		}

		void EditButton_Click(object sender, EventArgs e)
		{
			var formToShow = new DateScheduleForm(schedule);
			formToShow.HourMinuteRadioButton.Visible = ShouldShowHourMinute;
			formToShow.HourMinuteRadioButton.Enabled = ShouldShowHourMinute;
			formToShow.ActionOnClosed = b =>
			{
				ValueDateEdit.DateTimeFormat = b ? ZDateTimePickerFormat.Long : ZDateTimePickerFormat.Short;
				ValueDateEdit.DateTimeValue = Schedule.GetScheduleDate();
			};

			ZFormModaliser.ShowDialogAndDispose(formToShow);
		}

		const int ShortWidthOn100Scaling = 140;
		const int LongOffset = 30;
		const int LongWidthOn100Scaling = ShortWidthOn100Scaling + LongOffset;

		void UpdateLayout(bool hasSchedule)
		{
			if (!suspendResizing)
			{
				suspendResizing = true;
				try
				{
					var widthToSet = ShouldShowHourMinute ? LongWidthOn100Scaling : ShortWidthOn100Scaling;
					ControlDpiScalingHelper.SetHeight(this, hasSchedule ? EditButton.Bottom : ValueDateEdit.Bottom + ControlDpiScalingHelper.MarkAsScaled(2), false);
					ControlDpiScalingHelper.SetWidth(this, widthToSet, true);
					EditButton.Location = ControlDpiScalingHelper.NewScaledPoint(Width - EditButton.Width, EditButton.Location.Y, false);

					if (Schedule != null)
					{
						ValueDateEdit.DateTimeFormat = Schedule.ByHourAndMinute ? ZDateTimePickerFormat.Long : ZDateTimePickerFormat.Short;
					}
				}
				finally
				{
					suspendResizing = false;
				}
			}
		}

		DateSchedule schedule;
		bool suspendResizing;

		internal class ZDateTimeOffsetEditWithShortFormatAndLongBox : ZDateTimeOffsetEdit
		{
			protected override int GetDateFormatWidth()
			{
				if (keepLengthAlwaysLong)
				{
					return Width;
				}
				return base.GetDateFormatWidth();
			}
			bool keepLengthAlwaysLong;
			public bool KeepLengthAlwaysLong { get => keepLengthAlwaysLong; set => keepLengthAlwaysLong = value; }
		}

		#region Test
#if DEBUG
		internal ZDateTimeOffsetEditWithShortFormatAndLongBox ValueDateEdit_Exposed
		{
			get { return ValueDateEdit; }
			set { ValueDateEdit = value; }
		}

		public ZButton EditButton_Exposed
		{
			get { return EditButton; }
			set { EditButton = value; }
		}

#endif
		#endregion
	}
}
