using System;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class SingleAccountingPeriodUserControl : RuntimeOptionUserControl
	{
		public SingleAccountingPeriodUserControl()
		{
			InitializeComponent();
		}

		public override Type ExpectedFilterType()
		{
			return typeof(SingleAccountingPeriodField);
		}

		public override void SetFilter(FilterField filter)
		{
			base.SetFilter(filter);
			FieldLabel.GetExtension<ILabelCaptionRenderer>().Caption = EscapeMnemonics(filter.DisplayNameLocalized);
			SetDataBinding(filter, "");
			FieldPeriodEdit.SetSchedule(((SingleAccountingPeriodField)filter).Schedule);
		}
	}
}
