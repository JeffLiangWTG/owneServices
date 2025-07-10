using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.CashBook.DepositBatch
{
	public partial class DepositBatchForm : AccountingZForm, IButtonPostTextOverride, IButtonApplyTextOverride, IButtonDeleteTextOverride
	{
		public DepositBatchForm(DepositBatchParent depositBatchParent)
			: base(depositBatchParent)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			if (depositBatchParent.IsExistingBatch)
			{
				PlugIns.Add(ControllerIDs.eDocsPlugIn);
			}
		}
		ZDateEdit DepositPostDateDateEdit;
		ZPanel BottomPanel;
		ZPostingButtonsUserControl PostingButtonsUserControl;

		DepositBatchParent BatchParent
		{
			get { return (DepositBatchParent)base.BusinessEntity; }
		}

		static string NoTransactionSelectedMessage
		{
			get { return Res.GetString("c5f9ecdb-ae8c-490b-a125-97cfa17c9d43", "There are no receipt transaction selected for this batch."); }
		}

		#region Select / Deselect All

		void SelectAllButton_Click(object sender, EventArgs e)
		{
			if (BatchParent != null)
			{
				BatchParent.SelectAllTransactionsForBankAccount(BankCodeLabel.Text, true);
			}
		}

		void UnSelectAllButton_Click(object sender, EventArgs e)
		{
			if (BatchParent != null)
			{
				BatchParent.SelectAllTransactionsForBankAccount(BankCodeLabel.Text, false);
			}
		}

		void BankSelectAllButton_Click(object sender, EventArgs e)
		{
			if (BatchParent != null)
			{
				BatchParent.SelectAllTransactions(true);
			}
		}

		void BankUnSelectAllButton_Click(object sender, EventArgs e)
		{
			if (BatchParent != null)
			{
				BatchParent.SelectAllTransactions(false);
			}
		}

		void DisableSelectUnSelectButtons()
		{
			BankSelectAllButton.Enabled = false;
			BankUnSelectAllButton.Enabled = false;
			SelectAllButton.Enabled = false;
			UnSelectAllButton.Enabled = false;
		}

		#endregion

		#region Form Overrides

		protected override IBusiness GetTopLevelBusinessEntityForPlugIn()
		{
			return BatchParent.DepositBatch;
		}

		string IButtonPostTextOverride.PostButtonText
		{
			get { return IsThisSavingAndNotPosting ? Res.GetString("DepositBatchForm|6EADAB04-C39E-46AA-AC11-306862A87EE3", "S&ave && Close") : Res.GetString("DepositBatchForm|23DEAF16-B42F-4193-87E9-3BF4389118DD", "P&ost && Close"); }
		}

		string IButtonApplyTextOverride.ApplyButtonText
		{
			get { return IsThisSavingAndNotPosting ? Res.GetString("DepositBatchForm|A326DC91-F3A1-4D4F-9BD4-277BE8FCA1C9", "&Save") : Res.GetString("DepositBatchForm|948277E9-AF8F-479d-957B-921C8CB6BA5B", "&Post"); }
		}

		string IButtonDeleteTextOverride.DeleteButtonText
		{
			get { return Res.GetString("DepositBatchForm|752DDB7F-CDB0-41f3-9824-A0F33BEC3C6C", "&Cancel"); }
		}

		protected override string ReversingReasonInputBoxText
		{
			get { return Res.GetString("DepositBatchForm|4CD1C638-2751-4522-A2FD-FCE4A7C4ECEB", "Please enter the reason for canceling this transaction"); }
		}

		protected override string ReversingReasonCaptionText
		{
			get { return Res.GetString("DepositBatchForm|99792B03-8548-4174-884D-171D8314E640", "Cancel Reason"); }
		}

		bool IsThisSavingAndNotPosting => BatchParent != null && BatchParent.DepositBatch != null && BatchParent.DepositBatch.IsInDatabase;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			if (((DepositBatchParent)DataSource).IsExistingBatch)
			{
				DisableSelectUnSelectButtons();
			}
		}

		public override Guid IdentifierForPersistingForm
		{
			get
			{
				return BatchParent.DepositBatchLines.FirstOrDefault()?.PK.ToGuid() ?? base.IdentifierForPersistingForm;
			}
		}

		#endregion

		#region Loading

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			BatchNumberTextBox.Visible = BatchParent.IsInDatabase;
			CancelReasonLabel.Visible = ((IReversing)BatchParent).IsReversed;
			CancelReasonLabel.Text = ((IReversing)BatchParent).ReversingReason;

			if (DisplayMode == ODisplayMode.Delete)
			{
				ZFormMenuStrategy.SetMenuItemEnabled(this, ZFormMenuStrategy.FileReloadMenuItemName, false);
			}
		}

		#endregion

		#region Saving and Printing

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave result = base.ValidateAndSave();

			if (result == ContinueWithSave.Yes)
			{
				if (BatchParent.TotalNoOfTransactionsSelected == 0)
				{
					result = ContinueWithSave.No;
					Globals.Message.Show(NoTransactionSelectedMessage, Res.GetString("3326e4ab-5519-4a74-874b-4f1790aace9c", "Deposit Batch"), MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				else
				{
					if (DisplayMode == ODisplayMode.New)
					{
						if (Globals.Message.Show(Res.GetString("0857c195-b3da-4270-833f-cc68afff9732", "Do you want to print the Deposit Slip now?"), Res.GetString("3326e4ab-5519-4a74-874b-4f1790aace9c", "Deposit Batch"),
							MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
						{
							PrintDepositSlip();
						}
					}

					DisableSelectUnSelectButtons();
				}
			}

			return result;
		}

		void PrintDepositSlip()
		{
			DepositSlipPrintHelper printHelper = new DepositSlipPrintHelper();
			foreach (Business.CashBook.DepositBatch.DepositBatch line in BatchParent.DepositBatchLines)
			{
				if (line.IsSelected)
				{
					printHelper.PrintDepositSlip(line.AH_ReceiptBatchNo);
				}
			}
		}

		#endregion

		#region Dispose

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

		#endregion
	}
}

