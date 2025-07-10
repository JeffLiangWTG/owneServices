using System;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class AccountingPeriodFieldUserControl : RuntimeOptionUserControl
	{
		public AccountingPeriodFieldUserControl()
		{
			InitializeComponent();
		}

		public override Type ExpectedFilterType()
		{
			return typeof(AccountingPeriodField);
		}

		protected override int DesiredCaptionWidthCore => 0; // Contains many controls and some Radiobuttons, that shouldn't be taken into consideration.

		protected override void ChangeLabelSizeForAlignmentCore(int descriptionSize)
		{
			// Don't do anything in here, because control is very different from what we expect
		}

		public override void SetFilter(FilterField filter)
		{
			base.SetFilter(filter);

			if (currentFilter != null)
			{
				currentFilter.UseSinglePeriodInfo.ValueChanged -= new EventHandler(UseSinglePeriodInfo_ValueChanged);
				currentFilter.UsePeriodRangeInfo.ValueChanged -= new EventHandler(UsePeriodRangeInfo_ValueChanged);
				currentFilter.UseYearToPeriodInfo.ValueChanged -= new EventHandler(UseYearToPeriodInfo_ValueChanged);
			}

			currentFilter = (AccountingPeriodField)filter;
			FieldGroupBox.Text = EscapeMnemonics(currentFilter.DisplayNameLocalized);
			SetDataBinding(currentFilter, "");
			SinglePeriodEdit.SetSchedule(currentFilter.SinglePeriodSchedule);
			PeriodRangeEdit.SetSchedules(currentFilter.FromPeriodSchedule, currentFilter.ToPeriodSchedule);
			YearToPeriodEdit.SetSchedule(currentFilter.YearToPeriodSchedule);

			UpdateSinglePeriodEdit();
			UpdatePeriodRangeEdit();
			UpdateYearToPeriodEdit();

			currentFilter.UseSinglePeriodInfo.ValueChanged += new EventHandler(UseSinglePeriodInfo_ValueChanged);
			currentFilter.UsePeriodRangeInfo.ValueChanged += new EventHandler(UsePeriodRangeInfo_ValueChanged);
			currentFilter.UseYearToPeriodInfo.ValueChanged += new EventHandler(UseYearToPeriodInfo_ValueChanged);
		}

		void UseSinglePeriodInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateSinglePeriodEdit();
		}

		void UsePeriodRangeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdatePeriodRangeEdit();
		}

		void UseYearToPeriodInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateYearToPeriodEdit();
		}

		void UpdateSinglePeriodEdit()
		{
			SinglePeriodEdit.ReadOnly = !currentFilter.UseSinglePeriod;
		}

		void UpdatePeriodRangeEdit()
		{
			PeriodRangeEdit.ReadOnly = !currentFilter.UsePeriodRange;
		}

		void UpdateYearToPeriodEdit()
		{
			YearToPeriodEdit.ReadOnly = !currentFilter.UseYearToPeriod;
		}

		AccountingPeriodField currentFilter;
	}
}
