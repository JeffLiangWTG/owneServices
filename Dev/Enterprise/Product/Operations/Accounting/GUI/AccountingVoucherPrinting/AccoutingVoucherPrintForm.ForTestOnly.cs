#if DEBUG

using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.AccountingVoucherPrinting
{
	public partial class AccoutingVoucherPrintForm
	{
		public void OnLoad_ForTestOnly(EventArgs e)
		{
			OnLoad(e);
		}

		public void PrintAccountingVouchers_ForTestOnly()
		{
			PrintAccountingVouchers();
		}

		public void SetCheckedListBoxItems_ForTestOnly(Business.AccountingVoucherPrint.OptionalFilterCriteriaList list, CheckedListBox listBox)
		{
			SetCheckedListBoxItems(list, listBox);
		}

		public void SynchroniseListCheckedStatus_ForTestOnly(Business.AccountingVoucherPrint.OptionalFilterCriteriaList list, CheckedListBox listBox)
		{
			SynchroniseListCheckedStatus(list, listBox);
		}

		public ZModuleButtonGrid ZModuleButtonGrid1_ForTestOnly
		{
			get { return zModuleButtonGrid1; }
			set { zModuleButtonGrid1 = value; }
		}

		public void GenerateButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			GenerateButton_Click(sender, e);
		}

		public void CloseButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			CloseButton_Click(sender, e);
		}

		public CargoWise.Windows.UI.KCheckedListBox LedgerCheckedListBox_ForTestOnly
		{
			get { return LedgerCheckedListBox; }
			set { LedgerCheckedListBox = value; }
		}

		public void ClearCheckedListBox_ForTestOnly(CheckedListBox listBox)
		{
			ClearCheckedListBox(listBox);
		}

		public void SelectAllList_ForTestOnly(CheckedListBox listBox)
		{
			SelectAllList(listBox);
		}

		public ZPanel MainPanel_ForTestOnly
		{
			get { return MainPanel; }
			set { MainPanel = value; }
		}

		public ZGroupBox PrintOptionsGroupBox_ForTestOnly
		{
			get { return PrintOptionsGroupBox; }
			set { PrintOptionsGroupBox = value; }
		}
	}
}

#endif
