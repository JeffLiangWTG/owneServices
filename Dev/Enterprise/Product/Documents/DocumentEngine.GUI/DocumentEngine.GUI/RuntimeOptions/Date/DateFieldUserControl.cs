
using System;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class DateFieldUserControl : RuntimeOptionUserControl
	{
		public DateFieldUserControl()
		{
			InitializeComponent();
		}

		public override Type ExpectedFilterType()
		{
			return typeof(DateField);
		}

		public override void SetFilter(FilterField filter)
		{
			base.SetFilter(filter);
			DateField dateField = (DateField)filter;
			FieldLabel.GetExtension<ILabelCaptionRenderer>().Caption = EscapeMnemonics(dateField.DisplayNameLocalized);
			FieldDateEdit.DateTimeFormat = dateField.ZPickerFormat;
			SetDataBinding(dateField, "");
			FieldDateEdit.SetSchedule(dateField.Schedule);
		}

		#region Test
#if DEBUG

		public SchedulableDateEdit FieldDateEdit_Exposed
		{
			get { return FieldDateEdit; }
		}

#endif
		#endregion
	}
}
