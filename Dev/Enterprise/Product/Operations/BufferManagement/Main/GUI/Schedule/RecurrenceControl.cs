using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Scheduler.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

#if DEBUG
using Enterprise.ZArchitecture.GUI.Testing;
#endif

namespace Enterprise.BufferManagement.GUI
{
	public partial class RecurrenceControl : ZUserControl, IBindTo
	{
		public RecurrenceControl()
		{
			InitializeComponent();

#if DEBUG
			MissingResourceStringChecker.ExcludeFromTest(DailyDaysNumber);
			MissingResourceStringChecker.ExcludeFromTest(MonthlyDate);
			MissingResourceStringChecker.ExcludeFromTest(MonthlyNumWeek);
			MissingResourceStringChecker.ExcludeFromTest(MonthlyWeekDayDropEdit);
			MissingResourceStringChecker.ExcludeFromTest(YearlyMonth);
			MissingResourceStringChecker.ExcludeFromTest(YearlyWeekNum);
			MissingResourceStringChecker.ExcludeFromTest(YearlyWeekDayDropEdit);
#endif
		}

		protected IRecurrenceControlDataProvider RecurrenceControlDataProvider => (IRecurrenceControlDataProvider)CurrentDataItem;
		protected virtual StmScheduleTaskRecurrence Recurrence => RecurrenceControlDataProvider?.Recurrence;

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
				UpdateControlBindTos(this);
			}
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (Recurrence != null)
			{
				Recurrence.TaskPeriodInfo.ValueChanged -= UpdatePanels;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (Recurrence != null)
			{
				Recurrence.TaskPeriodInfo.ValueChanged += UpdatePanels;
				UpdatePanels();
			}
		}

		void UpdateControlBindTos(Control control)
		{
			foreach (Control childControl in control.Controls)
			{
				if (!string.IsNullOrEmpty(childControl.GetBindingMember()))
				{
					childControl.SetBindingMember(BindToWithDot + childControl.GetBindingMember());
				}

				if (childControl is IBindToList bindToList && !string.IsNullOrEmpty(bindToList.BindToList))
				{
					bindToList.BindToList = BindToWithDot + bindToList.BindToList;
				}

				UpdateControlBindTos(childControl);
			}
		}

		string BindToWithDot
		{
			get
			{
				var result = BindTo;
				if (result != null && !result.EndsWith("."))
				{
					result += ".";
				}
				return result;
			}
		}

		void UpdatePanels(object sender, EventArgs e)
		{
			UpdatePanels();
		}

		private protected virtual void UpdatePanels()
		{
			WeeklyPanel.Visible = Recurrence.WeeklyRange;
			YearlyPanel.Visible = Recurrence.YearlyRange;
			MonthlyPanel.Visible = Recurrence.MonthlyRange;
			DailyPanel.Visible = Recurrence.DailyRange;
			HourlyPanel.Visible = Recurrence.HourlyRange;
			MinutePanel.Visible = Recurrence.MinuteRange;
			SecondPanel.Visible = Recurrence.SecondRange;
		}

		#endregion
	}
}
