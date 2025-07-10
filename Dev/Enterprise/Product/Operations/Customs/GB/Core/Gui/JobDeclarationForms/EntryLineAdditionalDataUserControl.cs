using System;

namespace Enterprise.Customs.GB.GUI
{
	public partial class EntryLineAdditionalDataUserControl : EU.GUI.EntryLineAdditionalDataUserControl
	{
		public EntryLineAdditionalDataUserControl()
		{
			InitializeComponent();
		}

		protected override Type GetDutyAndTaxDetailsUserControlType() => typeof(EntryLineTaxAndConfirmedFeeUserControl);
	}
}
