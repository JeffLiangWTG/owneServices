using System;

namespace Enterprise.Customs.ES.GUI;
public partial class UCC6EntryLineAdditionalDataUserControl : ImportEntryLineAdditionalDataUserControl
{
	public UCC6EntryLineAdditionalDataUserControl()
	{
		InitializeComponent();
	}

	protected override Type GetDutyAndTaxDetailsUserControlType() => typeof(EntryLineTaxAndConfirmedFeeUserControl);
}
