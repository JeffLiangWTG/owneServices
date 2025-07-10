using System;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	internal partial class NumberNotInRangeUserControl : RuntimeOptionUserControl
	{
		public NumberNotInRangeUserControl()
		{
			InitializeComponent();
		}

		public override Type ExpectedFilterType()
		{
			return typeof(NumberNotInRangeField);
		}

		public override void SetFilter(FilterField filter)
		{
			base.SetFilter(filter);

			FieldLabel.GetExtension<ILabelCaptionRenderer>().Caption = EscapeMnemonics(filter.DisplayNameLocalized);

			NumberRangeEditControl.editFrom.AllowNull = true;
			NumberRangeEditControl.editTo.AllowNull = true;

			KBindingSource bindingSource = new KBindingSource();
			bindingSource.SetBindingMember(NumberRangeEditControl.editFrom, "From");
			bindingSource.SetBindingMember(NumberRangeEditControl.editTo, "To");
			bindingSource.SetDataBinding(filter, "");
		}
	}
}
