#if DEBUG

using System;

namespace Enterprise.Accounting.GUI.AccountingVoucherPrinting
{
	public partial class ChinaJournalListingPrintForm
	{
		public void GenerateButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			GenerateButton_Click(sender, e);
		}

		public void CloseButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			CloseButton_Click(sender, e);
		}
	}
}

#endif
