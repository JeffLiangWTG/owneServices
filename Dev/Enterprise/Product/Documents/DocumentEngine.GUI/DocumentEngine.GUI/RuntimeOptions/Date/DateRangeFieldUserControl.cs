
using System;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class DateRangeFieldUserControl : RuntimeOptionUserControl
	{
		public DateRangeFieldUserControl()
		{
			InitializeComponent();
		}

		public override Type ExpectedFilterType()
		{
			return typeof(DateRangeField);
		}

		public override void SetFilter(FilterField filter)
		{
			base.SetFilter(filter);
			DateRangeField dateRangeField = (DateRangeField)filter;
			FieldLabel.GetExtension<ILabelCaptionRenderer>().Caption = EscapeMnemonics(dateRangeField.DisplayNameLocalized);
			SetDataBinding(dateRangeField, "");
			dateControl.FromDateEdit.DateTimeFormat = dateRangeField.ZPickerFormat;
			dateControl.ToDateEdit.DateTimeFormat = dateRangeField.ZPickerFormat;
			dateControl.FromDateEdit.SetSchedule(dateRangeField.LowSchedule);
			dateControl.ToDateEdit.SetSchedule(dateRangeField.HighSchedule);
		}
	}
}
