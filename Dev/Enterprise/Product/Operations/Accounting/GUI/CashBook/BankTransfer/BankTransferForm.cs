using System;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.CashBook.Transfer
{
	public partial class BankTransferForm : AccountingZForm, IButtonPostTextOverride, IButtonApplyTextOverride
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public BankTransferForm()
		{
		}

		public BankTransferForm(BankTransfer bankTransferBizO)
			: base(bankTransferBizO)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			AH_NumberOfSupportingDocumentsCalcEdit.Visible = bankTransferBizO.AH_NumberOfSupportingDocumentsVisible_ReadOnly;

			CalcExVarianceCheckBox.CheckedChanged += (object sender, EventArgs e) =>
			{
				ExRateGainLossCalcFindBox.Visible = CalcExVarianceCheckBox.Checked;
			};

			bankTransferBizO.OnCancelingCalculateExchangeVariance += OnCancelingCalculateExchangeVariance;
		}

		bool OnCancelingCalculateExchangeVariance()
		{
			var caption = Res.GetString("716054B5-9094-4210-95B7-5AF49CB3D0B5", "Confirm Buy Local Amount re-defaulting");
			var message = Res.GetString("6FBC955C-DB22-4CFE-B77E-86FBEA8FA5D9", @"Canceling the Exchange Variance Calculation will result in equal Local Sell and Local Buy Amounts.
Depending on the currency of the To Bank Account, either Buy Amount or Buy exchange rate will be recalculated and should be confirmed as correct before posting the Bank Transfer.
Do you wish to cancel the exchange variance?");

			var dialogResult = Globals.Message.Show(message
				, caption
				, ZMessageBoxButtons.YesNo
				, ZMessageBoxIcon.Question
				, defaultResult: ZDialogResult.Yes
			);

			return dialogResult == ZDialogResult.Yes;
		}

		BankTransfer BankTransfer => (BankTransfer)BusinessEntity;

		#region Overrides

		string IButtonPostTextOverride.PostButtonText
		{
			get { return Res.GetString("Accounting|BankTransferForm|PostAndCloseButton", "P&ost && Close"); }
		}

		string IButtonApplyTextOverride.ApplyButtonText
		{
			get { return Res.GetString("Accounting|BankTransferForm|PostButton", "&Post"); }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			HideTaxFields();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			if (IsReversingMode && !BusinessEntity.Factory.HasContext(BusinessContext.ReverseDateForm))
			{
				BusinessEntity.Factory.SetContext(BusinessContext.ReverseDateForm);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (BankTransfer != null)
			{
				BankTransfer.OnCancelingCalculateExchangeVariance -= OnCancelingCalculateExchangeVariance;
			}

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
		}

		#endregion

		#region Implementation

		void HideTaxFields()
		{
			if (!GlbCompany.CurrentCompany.GC_IsGSTRegistered)
			{
				TaxPanel.Visible = false;
				ControlDpiScalingHelper.SetTop(ref TotalPanel, TaxPanel.Top, false);
				ControlDpiScalingHelper.SetHeight(ref FinanceChargeGroupBox, TotalPanel.Top + TotalPanel.Height + GovtChargeCodePanel.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(2), false);
			}

			if (!AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value)
			{
				GovtChargeCodePanel.Visible = false;
				ControlDpiScalingHelper.SetHeight(ref FinanceChargeGroupBox, FinanceChargeGroupBox.Height - GovtChargeCodePanel.Height, false);
			}

			ControlDpiScalingHelper.SetHeight(ref TabControl, FinanceChargeGroupBox.Top + FinanceChargeGroupBox.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(30), false);
			ControlDpiScalingHelper.SetHeight(this, TabControl.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(110), false);
			ControlDpiScalingHelper.SetTop(ref PostingButtonsUserControl, TabControl.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(10), false);
		}

		#endregion
	}
}

