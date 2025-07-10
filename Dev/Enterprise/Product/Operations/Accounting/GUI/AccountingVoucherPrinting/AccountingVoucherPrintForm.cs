using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business.AccountingVoucherPrint;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.AccountingVoucherPrinting
{
	public partial class AccoutingVoucherPrintForm : ZChildForm
	{
		ZButton GenerateButton;
		ZButton CloseButton;
		ZLabel LedgerLabel;
		ZLabel TransactionTypeLabel;
		ZGroupBox BatchVoucherReportGroupBox;
		ZPanel VoucherPanel;
		ZPanel MainPanel;
		ZDateEdit FromDateEdit;
		ZDateEdit EndDateEdit;
		ZPanel DatePanel;
		CargoWise.Windows.UI.KCheckedListBox LedgerCheckedListBox;
		CargoWise.Windows.UI.KCheckedListBox TransactionCheckedListBox;
		ZPeriodEdit VoucherPeriodEdit;
		ZLabel HelpTextLabel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		readonly System.ComponentModel.Container components;
		ZGroupBox BranchGroupBox;
		ZModuleButtonGrid zModuleButtonGrid1;
		ZGroupBox PrintOptionsGroupBox;
		ZCheckBox IncludeOrganisationCodeCheckBox;
		ZCheckBox IncludeJobNumberCheckBox;
		readonly AccountingVoucherPrintWrapper VoucherPrintWrapper;

		public AccoutingVoucherPrintForm(AccountingVoucherPrintWrapper voucherPrintWrapper)
			: base(voucherPrintWrapper)
		{
			this.VoucherPrintWrapper = voucherPrintWrapper;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			this.zModuleButtonGrid1.AttachButtonText = Res.GetData("AccoutingVoucherPrintForm|2176B450-246E-429b-BA1B-3C1C0CAB20DC", "Add");
			this.zModuleButtonGrid1.DetachButtonText = Res.GetData("AccoutingVoucherPrintForm|E9AE91E0-4179-4c4e-95B4-9ED86C7A5A18", "Remove");
			this.zModuleButtonGrid1.DetachMessage = Res.GetData("AccoutingVoucherPrintForm|19C27268-EC26-425a-A0D7-EA972DB98E68", "Are you sure you want to remove the selected record?");

			if (DocumentsDataRegistry.Instance.UseNewDocBuilderAccountingVoucher.Value)
			{
				MainPanel.Controls.Remove(PrintOptionsGroupBox);
			}
		}

		#region Implementation

#if DEBUG
		virtual
#endif
 protected bool CanPrintVoucherProceed()
		{
			if (VoucherPrintWrapper.GetWrapperCount() == 0)
			{
				Globals.Message.ShowInformation(Res.GetString("62b53124-1ba0-4f8a-94fa-edec2a146222", "There are no accounting vouchers within given selection criteria."));
				return false;
			}

			if (Globals.Message.Show(Res.GetString("76534475-7aab-48d7-81b5-ebada83b2525", "There are {0} accounting vouchers to print. Do you want to proceed?", VoucherPrintWrapper.GetWrapperCount()),
				Res.GetString("fe1d1d03-13d2-49c0-8b19-8f0485a18c9b", "Print Account Voucher"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) != DialogResult.Yes)
			{
				return false;
			}

			return true;
		}

#if DEBUG
		virtual
#endif
 protected void PrintAccountingVouchers()
		{
			SynchroniseListCheckedStatus(VoucherPrintWrapper.LedgerTypeList, LedgerCheckedListBox);
			SynchroniseListCheckedStatus(VoucherPrintWrapper.TransactionTypeList, TransactionCheckedListBox);

			GenerateVoucherDocument();

			if (CanPrintVoucherProceed())
			{
				PrintVoucherDocument();
			}
		}

#if DEBUG
		virtual
#endif
 protected void GenerateVoucherDocument()
		{
			VoucherPrintWrapper.GenerateVoucherDocWrapper();
		}

#if DEBUG
		virtual
#endif
 protected void PrintVoucherDocument()
		{
			VoucherPrintWrapper.PrintVoucherDocument();
		}

#if DEBUG
		virtual
#endif
 protected void SetCheckedListBoxItems(OptionalFilterCriteriaList list, CheckedListBox listBox)
		{
			foreach (OptionalFilterCriteria item in list)
			{
				int index = listBox.Items.Add(item);
				listBox.SetItemChecked(index, item.Enabled);
			}
		}

#if DEBUG
		virtual
#endif
 protected void SynchroniseListCheckedStatus(OptionalFilterCriteriaList list, CheckedListBox listBox)
		{
			list.ClearAllSelection();
			foreach (int index in listBox.CheckedIndices)
			{
				list[index].Enabled = true;
			}
		}

		void ClearCheckedListBox(CheckedListBox listBox)
		{
			listBox.Items.Clear();
		}

		void SelectAllList(CheckedListBox listBox)
		{
			for (int i = 0; i < listBox.Items.Count; i++)
			{
				listBox.SetItemChecked(i, true);
			}
		}

		#endregion

		#region Event Handler

		void GenerateButton_Click(object sender, EventArgs e)
		{
			if (HasWrapperValidationError)
			{
				ShowErrorsDialog();
			}
			else
			{
				PrintAccountingVouchers();
			}
		}

		protected virtual bool HasWrapperValidationError
		{
			get
			{
				VoucherPrintWrapper.RunPreSaveValidation();
				return VoucherPrintWrapper.HasErrors;
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void LedgerCheckedListBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			SynchroniseListCheckedStatus(VoucherPrintWrapper.LedgerTypeList, LedgerCheckedListBox);

			ClearCheckedListBox(TransactionCheckedListBox);
			SynchroniseListCheckedStatus(VoucherPrintWrapper.LedgerTypeList, LedgerCheckedListBox);
			VoucherPrintWrapper.ReloadTransactionList();

			SetCheckedListBoxItems(VoucherPrintWrapper.TransactionTypeList, TransactionCheckedListBox);
		}

		void TransactionCheckedListBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			SynchroniseListCheckedStatus(VoucherPrintWrapper.TransactionTypeList, TransactionCheckedListBox);
		}

		#endregion

		#region On* Event Overrides

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetCheckedListBoxItems(VoucherPrintWrapper.LedgerTypeList, LedgerCheckedListBox);
			SetCheckedListBoxItems(VoucherPrintWrapper.TransactionTypeList, TransactionCheckedListBox);
		}

		#endregion
	}
}

