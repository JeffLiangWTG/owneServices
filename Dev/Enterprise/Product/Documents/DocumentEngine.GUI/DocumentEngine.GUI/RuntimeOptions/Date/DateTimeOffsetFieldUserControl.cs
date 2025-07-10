
using System;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class DateTimeOffsetFieldUserControl : RuntimeOptionUserControl
	{
		public DateTimeOffsetFieldUserControl()
		{
			InitializeComponent();
		}

		public override Type ExpectedFilterType()
		{
			return typeof(DateTimeOffsetField);
		}

		public override void SetFilter(FilterField filter)
		{
			base.SetFilter(filter);
			DateTimeOffsetField dateField = (DateTimeOffsetField)filter;
			FieldLabel.GetExtension<ILabelCaptionRenderer>().Caption = EscapeMnemonics(dateField.DisplayNameLocalized);
			FieldDateEdit.DateTimeFormat = dateField.ZPickerFormat;
			SetDataBinding(dateField, "");
			FieldDateEdit.SetSchedule(dateField.Schedule);
		}

		#region Test
#if DEBUG

		public SchedulableDateTimeOffsetEdit FieldDateEdit_Exposed
		{
			get { return FieldDateEdit; }
		}

#endif
		#endregion
	}
}
