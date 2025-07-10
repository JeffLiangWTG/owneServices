using System;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	internal partial class MonthYearPeriodUserControl : RuntimeOptionUserControl
	{
		public MonthYearPeriodUserControl()
		{
			InitializeComponent();
		}

		public override void SetFilter(FilterField filter)
		{
			base.SetFilter(filter);
			FieldLabel.GetExtension<ILabelCaptionRenderer>().Caption = EscapeMnemonics(filter.DisplayNameLocalized);
			monthYearPeriodEditControl.PeriodMonthEdit.SetDataBinding(filter, "Month");
			monthYearPeriodEditControl.PeriodYearEdit.SetDataBinding(filter, "Year");
			SetDataBinding(filter, "");
		}

		public override Type ExpectedFilterType()
		{
			return typeof(MonthYearPeriodField);
		}
	}
}
