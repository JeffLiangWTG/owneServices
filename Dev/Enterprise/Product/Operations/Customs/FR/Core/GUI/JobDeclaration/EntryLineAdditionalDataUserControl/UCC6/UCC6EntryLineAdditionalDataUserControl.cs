using System;

namespace Enterprise.Customs.FR.GUI
{
	public partial class UCC6EntryLineAdditionalDataUserControl : EU.GUI.UCC6EntryLineAdditionalDataUserControl
	{
		public UCC6EntryLineAdditionalDataUserControl()
		{
			InitializeComponent();
		}

		protected override Type GetDutyAndTaxDetailsUserControlType() => typeof(UCC6EntryLineTaxAndConfirmedFeeUserControl);
	}
}
