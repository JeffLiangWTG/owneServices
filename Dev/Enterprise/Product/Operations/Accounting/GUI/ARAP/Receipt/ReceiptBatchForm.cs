using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class ReceiptBatchForm : AccountingZForm, IButtonPostTextOverride, IButtonApplyTextOverride
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public ReceiptBatchForm()
		{
		}

		public ReceiptBatchForm(ARReceiptBatchPoster receiptBatchPosterBizObj)
			: base(receiptBatchPosterBizObj)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, null, CancelPostingButton, SaveAndCloseButton);
			ReceiptBatchGrid.ContextMenu.Popup += new EventHandler(ReceiptBatchGridMenu_Popup);
			ReceiptBatchPoster.OnPrintDepositSlip += new ARReceiptBatchPoster.DefaultBankSelectionEventHandler(ReceiptBatchPoster_OnPrintDepositSlip);
			ReceiptBatchPoster.OnAskBeforeUpdateDescription += new ARReceiptBatchPoster.OnEnquireUserHandler(ReceiptBatchPoster_OnAskBeforeUpdateDescription);
			ReceiptBatchPoster.OnAskBeforeUpdateReceiptType += new ARReceiptBatchPoster.OnEnquireUserHandler(ReceiptBatchPoster_OnAskBeforeUpdateReceiptType);
		}

		#region Overrides

		string IButtonPostTextOverride.PostButtonText
		{
			get { return Res.GetString("ReceiptBatchForm|2E759CDD-5461-4a57-B4E7-00A6FD685E12", "P&ost"); }
		}

		string IButtonApplyTextOverride.ApplyButtonText
		{
			get { return Res.GetString("ReceiptBatchForm|F903C46F-FA59-4fe6-B122-C33E713432E1", "&Post"); }
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave result = ContinueWithSave.No;
			ReceiptBatchPoster.RunPreSaveValidation();
			if (ReceiptBatchPoster.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				if (ReceiptBatchPoster.ReceiptBatch.Count != 0)
				{
					Cursor.Current = Cursors.WaitCursor;
					result = ContinueWithSave.Yes;

					ContinueWithSave formResult = base.ValidateAndSave();

					if (formResult == ContinueWithSave.Yes)
					{
						SaveAndCloseButton.Enabled = false;

						Cursor.Current = Cursors.Arrow;

						if (ReceiptBatchPoster.MatchAfterPosting)
						{
							MatchReceipts();
							Close();
						}
						else
						{
							Close();
						}
					}
				}
				else
				{
					Globals.Message.Show(Res.GetString("770174c8-8cd5-47d6-bab3-08c1a1476522", "There is nothing to post. Fill in the receipts first."), Res.GetString("9e0b4bcf-78ef-45a6-af47-ee812f26b58d", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}

			return result;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#endregion

		#region Implementation

		#region Envent Handlers

		void ReceiptBatchPoster_OnPrintDepositSlip(DepositBatch depositSlipToPrint)
		{
			if (Globals.Message.Show(Res.GetString("2393f50f-ce03-492c-999d-733d437e0aab", "Do you want to print the Deposit Slip now?"), Res.GetString("c9d550de-c2d6-42b8-8071-c5e516094ca0", "Deposit Batch"),
					MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				DepositSlipPrintHelper printHelper = new DepositSlipPrintHelper();
				printHelper.PrintDepositSlip(depositSlipToPrint.AH_ReceiptBatchNo);

#if DEBUG
				if (Globals.IsTest)
				{
					DepositSlipPrintedForTest = ZBool.True;
				}
#endif
			}
		}

		ZBool ReceiptBatchPoster_OnAskBeforeUpdateDescription()
		{
			return Globals.Message.Show(Res.GetString("9d126fa0-4eab-455c-a82e-5f45bc1838a6", "Are you sure you want to set this Description for all receipts?"), Res.GetString("cffd486c-0d03-4971-8d59-05de6468ccf5", "Description Update"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
		}

		ZBool ReceiptBatchPoster_OnAskBeforeUpdateReceiptType()
		{
			return Globals.Message.Show(Res.GetString("4202a3e4-093e-402d-b856-0b8e0916c71f", "Are you sure you want to set this Receipt Type for all receipts?"), Res.GetString("9881b7c6-ecac-434c-854c-2170b18b5837", "Receipt Type Update"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
		}

		void ReceiptBatchGridMenu_Popup(object sender, EventArgs e)
		{
			// Delete Menu Item
			if (ReceiptBatchGrid.SelectedElements.Length == 1)
			{
				if (!ReceiptBatchGrid.ContextMenu.MenuItems.Contains(DeleteMenuItem))
				{
					ReceiptBatchGrid.ContextMenu.MenuItems.Add(DeleteMenuItem);
				}
			}
			else
			{
				if (ReceiptBatchGrid.ContextMenu.MenuItems.Contains(DeleteMenuItem))
				{
					ReceiptBatchGrid.ContextMenu.MenuItems.Remove(DeleteMenuItem);
				}
			}
		}

		#endregion

		void MatchReceipts()
		{
			ReceiptBatchPoster.RemoveNewlyCreatedReceiptsFromUnmatchedTransactions();

			foreach (ARReceipt receipt in ReceiptBatchPoster.ReceiptBatch)
			{
				ARReceipt receiptForMatching = (new BusinessObjectFactory()).Load<ARReceipt>(receipt.PK);
				if (receiptForMatching == null)
				{
					Globals.Message.Show(Res.GetString("A450BF9D-8A22-4303-ACC9-6AF8B4036490", "The receipt you are trying to match does not exist."));
				}
				else
				{
					using (NewMatchGroupForm matchingForm = new NewMatchGroupForm(receiptForMatching.MatchingBaseObject))
					{
						matchingForm.HideMatchAndContinueButtonForReceiptPayment();
						ZFormModaliser.ShowDialogWithoutDispose(matchingForm);

#if DEBUG
						if (Globals.IsTest)
						{
							MainFormWasVisibleDuringMatching = this.Visible;
							OpenedMatchFormsForTest++;
							if (AutoCloseMatchingForms)
							{
								matchingForm.Close();
							}
						}
#endif
					}
					GCWrapper.ReclaimMemory(ref receiptForMatching);
				}
			}
		}

		void DeleteTransaction(object sender, EventArgs e)
		{
			if (ReceiptBatchGrid.SelectedElements.Length == 1)
			{
				if (ReceiptBatchGrid.SelectedElements[0] is ARReceipt)
				{
					ReceiptBatchPoster.RemoveReceiptFromBatch((ARReceipt)ReceiptBatchGrid.SelectedElements[0]);
				}
			}
		}

		MenuItem DeleteMenuItem
		{
			get
			{
				if (fDeleteMenuItem == null)
				{
					fDeleteMenuItem = new ZMenuItem(ResString.GetMultilingualString("MenuItem.Remove", "Remove"), new EventHandler(DeleteTransaction));
				}

				return fDeleteMenuItem;
			}
		}

		MenuItem fDeleteMenuItem;

		ARReceiptBatchPoster ReceiptBatchPoster
		{
			get
			{
				return BusinessEntity as ARReceiptBatchPoster;
			}
		}

		#endregion

		#region Test
#if DEBUG

		internal ZBool DepositSlipPrintedForTest;
		internal ZInt OpenedMatchFormsForTest;
		internal ZBool MainFormWasVisibleDuringMatching;
		internal ZBool AutoCloseMatchingForms;

#endif
		#endregion
	}
}

