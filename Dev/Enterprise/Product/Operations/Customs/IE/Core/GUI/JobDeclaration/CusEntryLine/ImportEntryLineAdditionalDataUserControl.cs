using System;

namespace Enterprise.Customs.IE.GUI
{
	public partial class ImportEntryLineAdditionalDataUserControl : EU.GUI.UCC6EntryLineAdditionalDataUserControl
	{
		public ImportEntryLineAdditionalDataUserControl()
		{
			InitializeComponent();
			ReorderTabPages();
		}

		protected override Type GetDutyAndTaxDetailsUserControlType() => typeof(EntryLineTaxAndConfirmedFeeUserControl);

		void ReorderTabPages()
		{
			ExtendInfoTabControl.TabPages.Remove(TaxOrFeeTabPage);
			ExtendInfoTabControl.TabPages.Insert(TaxOrFeeTabPage, 0);
			ExtendInfoTabControl.TabPages.Remove(RefundsTabPage);
			ExtendInfoTabControl.TabPages.Insert(RefundsTabPage, 1);
			ExtendInfoTabControl.TabPages.Remove(ExtendedInfoTabPage);
			ExtendInfoTabControl.TabPages.Insert(ExtendedInfoTabPage, 2);
		}
	}
}
