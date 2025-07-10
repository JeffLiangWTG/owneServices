
using System;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class AccountingPeriodsRangeUserControl : RuntimeOptionUserControl
	{
		public AccountingPeriodsRangeUserControl()
		{
			InitializeComponent();
		}

		public override Type ExpectedFilterType()
		{
			return typeof(AccountingPeriodsRangeField);
		}

		public override void SetFilter(FilterField filter)
		{
			base.SetFilter(filter);
			AccountingPeriodsRangeField rangeField = (AccountingPeriodsRangeField)filter;
			FieldLabel.GetExtension<ILabelCaptionRenderer>().Caption = EscapeMnemonics(rangeField.DisplayNameLocalized);
			SetDataBinding(rangeField, "");
			PeriodRangeEdit.SetSchedules(rangeField.LowSchedule, rangeField.HighSchedule);
		}
	}
}
