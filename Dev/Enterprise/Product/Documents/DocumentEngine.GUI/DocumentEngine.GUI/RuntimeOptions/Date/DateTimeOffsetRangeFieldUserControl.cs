using System;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class DateTimeOffsetRangeFieldUserControl : RuntimeOptionUserControl
	{
		public DateTimeOffsetRangeFieldUserControl()
		{
			InitializeComponent();
		}

		public override Type ExpectedFilterType()
		{
			return typeof(DateTimeOffsetRangeField);
		}

		public override void SetFilter(FilterField filter)
		{
			base.SetFilter(filter);
			var dateTimeOffsetRangeField = (DateTimeOffsetRangeField)filter;
			FieldLabel.GetExtension<ILabelCaptionRenderer>().Caption = EscapeMnemonics(dateTimeOffsetRangeField.DisplayNameLocalized);
			SetDataBinding(dateTimeOffsetRangeField, "");
			dateControl.FromDateEdit.DateTimeFormat = dateTimeOffsetRangeField.ZPickerFormat;
			dateControl.ToDateEdit.DateTimeFormat = dateTimeOffsetRangeField.ZPickerFormat;
			dateControl.FromDateEdit.SetSchedule(dateTimeOffsetRangeField.LowSchedule);
			dateControl.ToDateEdit.SetSchedule(dateTimeOffsetRangeField.HighSchedule);
		}
	}
}
