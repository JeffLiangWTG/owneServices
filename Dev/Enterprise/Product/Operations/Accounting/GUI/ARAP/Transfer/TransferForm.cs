using System;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class TransferForm : AccountingZForm
	{
		public TransferForm(Transfer bO) : base(bO)
		{
			TransferBO = bO;
			if (TransferBO.AreTransferRowsInDB)
			{
				HideControls();
			}
			SetARAPSpecifics(bO);
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
		}
		ZTemplateTabControl MainTabControl;
		ZTabPage TransferTabPage;
		ZLogsTabPage zEventTabPage1;

		readonly Transfer TransferBO;

		#region Implementation

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			if (IsReversingMode && !BusinessEntity.Factory.HasContext(BusinessContext.ReverseDateForm))
			{
				BusinessEntity.Factory.SetContext(BusinessContext.ReverseDateForm);
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null)
			{
				Transfer bO = dataSource as Transfer;

				string fromToolTip = FormToolTip.GetToolTip(AH_Calc_FromAccountBoundFindBox);
				string toToolTip = FormToolTip.GetToolTip(AH_Calc_ToAccountBoundFindBox);
				if (bO.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					fromToolTip = fromToolTip.Replace("%AccountType%", Res.GetString("TransferForm|6B91ED15-98F0-46c5-9450-A31192E68EE4", "Accounting.Debtor"));
					toToolTip = toToolTip.Replace("%AccountType%", Res.GetString("TransferForm|6B91ED15-98F0-46c5-9450-A31192E68EE4", "Accounting.Debtor"));
					AH_PostDateBoundDateEdit.ReadOnly = !Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed;
				}
				else
				{
					fromToolTip = fromToolTip.Replace("%AccountType%", Res.GetString("TransferForm|510B49AC-31D6-4da7-B86C-16B62F71C310", "Accounting.Creditor"));
					toToolTip = toToolTip.Replace("%AccountType%", Res.GetString("TransferForm|510B49AC-31D6-4da7-B86C-16B62F71C310", "Accounting.Creditor"));
					AH_PostDateBoundDateEdit.ReadOnly = !Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed;
				}
				FormToolTip.SetToolTip(AH_Calc_FromAccountBoundFindBox, fromToolTip);
				FormToolTip.SetToolTip(AH_Calc_ToAccountBoundFindBox, toToolTip);
				AH_NumberOfSupportingDocumentsCalcEdit.Visible = bO.AH_NumberOfSupportingDocumentsVisible_ReadOnly;
				ZFormMenuStrategy.SetMenuItemText(this, ZFormMenuStrategy.FileSaveAndCloseMenuItemName, Res.GetString("TransferForm|59B35ADC-4290-4a19-B0E8-E9DBB1B31201", "S&ave"));
			}
		}

		protected override bool ShowAuditTab => true;

		protected void HideControls()
		{
			ControlDpiScalingHelper.SetHeight(ref FromGroupBox, AH_Calc_FromAgeingDateBoundDateEdit.Top + AH_Calc_FromAgeingDateBoundDateEdit.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(8), false);
			AH_Calc_FromBeforeTransferBoundCurrencyControl.GetExtension<LabelCaptionRenderer>().Visible = false;
			AH_Calc_FromBeforeTransferBoundCurrencyControl.Visible = false;
			AH_Calc_FromAfterTransferBoundCurrencyControl.GetExtension<LabelCaptionRenderer>().Visible = false;
			AH_Calc_FromAfterTransferBoundCurrencyControl.Visible = false;

			ControlDpiScalingHelper.SetHeight(ref ToGroupBox, AH_Calc_ToAgeingDateBoundDateEdit.Top + AH_Calc_ToAgeingDateBoundDateEdit.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(8), false);
			AH_Calc_ToBeforeTransferBoundCurrencyControl.GetExtension<LabelCaptionRenderer>().Visible = false;
			AH_Calc_ToBeforeTransferBoundCurrencyControl.Visible = false;
			AH_Calc_ToAfterTransferBoundCurrencyControl.GetExtension<LabelCaptionRenderer>().Visible = false;
			AH_Calc_ToAfterTransferBoundCurrencyControl.Visible = false;

			ControlDpiScalingHelper.SetTop(ref ToGroupBox, FromGroupBox.Top + FromGroupBox.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(8), false);
			ControlDpiScalingHelper.SetHeight(this, this.Height - (AH_Calc_FromBeforeTransferBoundCurrencyControl.Height * 2 + ControlDpiScalingHelper.ScaleToCurrentDpiY(16)), false);
			//this.Height -= (ButtonsUserControl.Top - ToGroupBox.Bottom) + 8;
		}

		protected void SetARAPSpecifics(Transfer bO)
		{
			Text = Res.GetString("TransferForm|b99b128d-da3b-4d90-b79e-9fac2ea7b845", "{0} Transfer", bO.AH_Ledger);
		}

		Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		ZGroupBox DetailsGroupBox;
		ZGroupBox ToGroupBox;
		ZGroupBox FromGroupBox;
		ZDateEdit AH_InvoiceDateBoundDateEdit;
		ZArchitecture.ZTextBox AH_DescBoundTextEdit;
		ZCalcFindBox AH_Calc_ToAfterTransferBoundCurrencyControl;
		ZCalcFindBox AH_Calc_ToBeforeTransferBoundCurrencyControl;
		ZCalcFindBox AH_Calc_FromAfterTransferBoundCurrencyControl;
		ZCalcFindBox AH_Calc_FromBeforeTransferBoundCurrencyControl;
		ZArchitecture.ZTextBox AH_TransactionNumBoundTextBox;
		ZExchangeRateControl AH_ExchangeRateBoundExchangeRateControl;
		ZCalcFindBox AH_Calc_InvoiceAmountBoundCalcEdit;
		ZCalcFindBox AH_OSTotalBoundCurrencyControl;
		ZGuidFindBox AH_Calc_ToAccountBoundFindBox;
		ZGuidFindBox AH_Calc_FromAccountBoundFindBox;

		ZDateEdit AH_Calc_FromAgeingDateBoundDateEdit;
		ZDateEdit AH_Calc_ToAgeingDateBoundDateEdit;
		ZDateEdit AH_PostDateBoundDateEdit;
		ZArchitecture.ZCalcEdit AH_NumberOfSupportingDocumentsCalcEdit;

		public override string FormCaption
		{
			get
			{
				return TransferBO.AH_Ledger + " " + Res.GetString("TransferForm|Transfer", "Transfer");
			}
		}

		#endregion
	}
}

