using System;

namespace Enterprise.Customs.EU.GUI
{
	public partial class UCC6EntryLineAdditionalDataUserControl : EntryLineAdditionalDataUserControl
	{
		public UCC6EntryLineAdditionalDataUserControl()
		{
			InitializeComponent();
		}

		protected override Type GetDutyAndTaxDetailsUserControlType() => typeof(EntryLineTaxAndConfirmedFeeUserControl);
	}
}
