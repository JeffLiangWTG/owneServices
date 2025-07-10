#if DEBUG

using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.CashBook.DirectDebitBatch
{
	public partial class DirectDebitBatchForm
	{
		public void OnLoad_ForTestOnly(EventArgs e)
		{
			OnLoad(e);
		}

		public string GetSaveDialogFilter_ForTestOnly(string dDRFileFormat)
		{
			return GetSaveDialogFilter(dDRFileFormat);
		}

		public ZOrganisationsForm ShowOrganisationFormToEdit_ForTestOnly()
		{
			return ShowOrganisationFormToEdit();
		}

		public ZArchitecture.ZLabel CancelledBatchLabel_ForTestOnly
		{
			get { return CancelledBatchLabel; }
			set { CancelledBatchLabel = value; }
		}

		public ZDateEdit PostDateDateEdit_ForTestOnly
		{
			get { return PostDateDateEdit; }
			set { PostDateDateEdit = value; }
		}

		public ZArchitecture.ZGrid DepositBatchLineGrid_ForTestOnly
		{
			get { return DepositBatchLineGrid; }
			set { DepositBatchLineGrid = value; }
		}

		public MultilingualString ViewTransactionMenuItemText_ForTestOnly
		{
			get { return ViewTransactionMenuItemText; }
			set { ViewTransactionMenuItemText = value; }
		}

		public MultilingualString OrgEditMenuItemText_ForTestOnly
		{
			get { return OrgEditMenuItemText; }
			set { OrgEditMenuItemText = value; }
		}

		public void SetEditOrgMenuItemReadOnly_ForTestOnly(Business.Base.Transaction.TransactionHeader transaction)
		{
			SetEditOrgMenuItemReadOnly(transaction);
		}

		public MenuItem OrgEditMenuItem_ForTestOnly
		{
			get { return OrgEditMenuItem; }
			set { OrgEditMenuItem = value; }
		}

		public MenuItem ViewTransactionMenu_ForTestOnly
		{
			get { return ViewTransactionMenu; }
			set { ViewTransactionMenu = value; }
		}

		public void ContextMenu_Popup_ForTestOnly(object sender, EventArgs e)
		{
			ContextMenu_Popup(sender, e);
		}

		public void SaveDDRFile_ForTestOnly(Business.CashBook.DirectDebitBatch.DirectDebitBatchHeader header)
		{
			SaveDDRFile(header);
		}

		public ZButton GenerateDDRFileButton_ForTestOnly
		{
			get { return GenerateDDRFileButton; }
			set { GenerateDDRFileButton = value; }
		}
	}
}

#endif
