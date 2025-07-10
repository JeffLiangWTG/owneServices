using System;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	internal partial class AccountingNumberRangeUserControl : RuntimeOptionUserControl
	{
		public AccountingNumberRangeUserControl()
		{
			InitializeComponent();
		}

		public override Type ExpectedFilterType()
		{
			return typeof(AccountingNumberRangeField);
		}

		public override void SetFilter(FilterField filter)
		{
			base.SetFilter(filter);

			fieldLabel.GetExtension<ILabelCaptionRenderer>().Caption = EscapeMnemonics(filter.DisplayNameLocalized);

			fromEdit.AllowNull = true;
			toEdit.AllowNull = true;

			SetDataBinding(filter, "");
			BindingSource.SetBindingMember(this.fromEdit, "From");
			BindingSource.SetBindingMember(this.toEdit, "To");
		}
	}
}
