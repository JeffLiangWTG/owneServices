using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.CashBook
{
	public partial class CashBookExchangeDiffForm : AccountingZForm
	{
		public CashBookExchangeDiffForm(CashbookExchangeDiff cSHExchangeDiff)
			: base(cSHExchangeDiff)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);

			DisplayModeChanged += CashBookExchangeDiffForm_DisplayModeChanged;
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			ChangeControlLayout();
			ZFormMenuStrategy.SetMenuItemText(this, ZFormMenuStrategy.FileSaveAndCloseMenuItemName, Res.GetString("Accounting|CashBookExchangeDiffForm|PostMenuItem", "&Post"));
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override bool AllowNew => !CashbookExchangeDiff.IsRealizedExchangeGainLoss;

		#region Implementation

		#region System-generated stuff

		Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		ZTemplateTabControl zTabControl1;
		ZTabPage zTabPage1;
		ZLogsTabPage zEventTabPage1;
		ZStmNoteTabPage zStmNoteTabPage1;
		ZGuidFindBox AH_ABBoundZGuidFindBox;
		ZCalcFindBox AH_Calc_ExchangeRateBoundCalcFindBox;
		ZCalcFindBox AH_Calc_InvoiceAmountBoundCalcFindBox;
		ZCalcFindBox AH_ExchangeRateBoundAccountingExchangeRateControlBoundCalcFindBox;
		ZDateEdit AH_PostDateBoundDateEdit;
		protected ZArchitecture.ZTextBox AH_TransactionNumBoundTextBox;
		ZArchitecture.ZTextBox AH_DescBoundTextBox;
		ZDateEdit AH_InvoiceDateBoundDateEdit;
		ZGroupBox AfterCurrencyAdjustmentGroupBox;
		ZCalcFindBox AH_Calc_BeforeCurrencyAdjBalanceLocalBoundCalcFindBox;
		ZCalcFindBox AH_Calc_AfterCurrencyAdjBalanceLocal;
		ZCalcFindBox AH_Calc_AfterCurrencyAdjBalanceAmountBoundCalcFindBox;
		ZArchitecture.ZCalcEdit AH_NumberOfSupportingDocumentsCalcEdit;
		ZGuidFindBox AH_AGGuidFindBox;
		IContainer components;

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
		#endregion

		void ChangeControlLayout()
		{
			if (CashbookExchangeDiff?.IsInDatabase == true)
			{
				AH_Calc_ExchangeRateBoundCalcFindBox.Visible = false;
				AfterCurrencyAdjustmentGroupBox.Visible = false;
				ControlDpiScalingHelper.SetTop(ref AfterCurrencyAdjustmentGroupBox, 0, true);
				ControlDpiScalingHelper.SetHeight(ref AfterCurrencyAdjustmentGroupBox, 0, true);

				if (CashbookExchangeDiff.IsRealizedExchangeGainLoss)
				{
					AH_ExchangeRateBoundAccountingExchangeRateControlBoundCalcFindBox.Visible = false;
					AH_AGGuidFindBox.Visible = true;
					ControlDpiScalingHelper.SetTop(ref AH_AGGuidFindBox, AH_Calc_InvoiceAmountBoundCalcFindBox.Top, false);
				}
				ControlDpiScalingHelper.SetTop(ref AH_Calc_InvoiceAmountBoundCalcFindBox, AH_Calc_ExchangeRateBoundCalcFindBox.Top, false);
				ControlDpiScalingHelper.SetHeight(this, 360, true);
			}
		}

		void CashBookExchangeDiffForm_DisplayModeChanged(object sender, DisplayModeChangedEventArgs e)
		{
			DocManagerReadOnlyOverrideHelper.TrySetReadOnlyOverride(BusinessEntity, e.ToMode);
		}

		CashbookExchangeDiff CashbookExchangeDiff => (CashbookExchangeDiff)BusinessEntity;

		#endregion
	}
}

