using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ServiceManager.GUI
{
	public partial class StmServiceTaskRecurrenceControl : ZUserControl, IBindTo
	{
		public StmServiceTaskRecurrenceControl()
		{
			InitializeComponent();
		}

		IStmServiceTaskRecurrenceControlDataProvider RecurrenceControlDataProvider => (IStmServiceTaskRecurrenceControlDataProvider)CurrentDataItem;
		StmServiceTaskAdapter Recurrence => RecurrenceControlDataProvider?.Recurrence;

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

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (Recurrence != null)
			{
				Recurrence.NextRunTimeCalculatorInfo.ValueChanged -= UpdatePanels;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (Recurrence != null)
			{
				Recurrence.NextRunTimeCalculatorInfo.ValueChanged += UpdatePanels;
				UpdatePanels();
			}
		}

		void UpdatePanels(object sender, EventArgs e)
		{
			UpdatePanels();
		}

		void UpdatePanels()
		{
			SecondPanel.Visible = Recurrence.SecondsRange;
			MinutePanel.Visible = Recurrence.MinutesRange;
			HourlyPanel.Visible = Recurrence.HoursRange;
			DailyPanel.Visible = Recurrence.DaysRange;
			WeeklyPanel.Visible = Recurrence.WeeksRange;
			MonthlyPanel.Visible = Recurrence.MonthsRange;
			YearlyPanel.Visible = Recurrence.YearsRange;
		}

		#endregion
	}
}
