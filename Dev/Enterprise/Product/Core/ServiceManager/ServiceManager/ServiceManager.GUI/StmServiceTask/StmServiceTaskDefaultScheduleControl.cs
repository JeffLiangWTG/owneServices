using System;
using System.ComponentModel;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ServiceManager.GUI
{
	public partial class StmServiceTaskDefaultScheduleControl : ZUserControl
	{
		public StmServiceTaskDefaultScheduleControl()
		{
			InitializeComponent();
		}

		IStmServiceTaskDefaultScheduleControlDataProvider RecurrenceControlDataProvider => (IStmServiceTaskDefaultScheduleControlDataProvider)CurrentDataItem;
		StmServiceTaskAdapter DefaultSchedule => RecurrenceControlDataProvider?.DefaultSchedule;

		#region Binding

		[Browsable(true)]
		[Category(ZGUIConstants.DesignerCategory)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue("")]
		public string BindTo { get; set; }

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			if (!string.IsNullOrEmpty(BindTo))
			{
				BindingHelper.UpdateControlBindTos(this, BindTo);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (DefaultSchedule != null)
			{
				UpdatePanels();
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdatePanels();
		}

		private protected virtual void UpdatePanels()
		{
			SecondPanel.Visible = DefaultSchedule.SecondsRange;
			MinutePanel.Visible = DefaultSchedule.MinutesRange;
			HourlyPanel.Visible = DefaultSchedule.HoursRange;
			DailyPanel.Visible = DefaultSchedule.DaysRange;
			WeeklyPanel.Visible = DefaultSchedule.WeeksRange;
			MonthlyPanel.Visible = DefaultSchedule.MonthsRange;
			YearlyPanel.Visible = DefaultSchedule.YearsRange;

			HideEmptyRandomOffsetControls();
		}

		void HideEmptyRandomOffsetControls()
		{
			var randomOffset = DefaultSchedule?.RandomStartOffset;

			if ((randomOffset?.IsEmpty ?? false) || randomOffset.ToString().Equals("00:00"))
			{
				DefaultScheduleSecondlyRandomStartOffset.Visible = false;
				DefaultScheduleMinuteRandomStartOffset.Visible = false;
				DefaultScheduleHourlyRandomStartOffset.Visible = false;
				DefaultScheduleDailyRandomStartOffset.Visible = false;
				DefaultScheduleWeeklyRandomStartOffset.Visible = false;
				DefaultScheduleMonthlyRandomStartOffset.Visible = false;
				DefaultScheduleYearlyRandomStartOffset.Visible = false;
			}
		}

		#endregion
	}
}
