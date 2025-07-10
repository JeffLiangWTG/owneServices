using System;
using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.CashBook
{
	public partial class DirectCashBookBaseForm : AccountingZForm
	{
		public static class CashBookLineGridContext
		{
			public const string Payment = nameof(Payment);
			public const string Receipt = nameof(Receipt);
			public const string Direct = nameof(Direct);
		}

		#region Schema

		public abstract class Schema
		{
			public const string AL_OSTaxAmount = "AL_OSTaxAmount";
			public const string AL_LocalTaxAmount = "AL_LocalTaxAmount";
		}

		#endregion

		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public DirectCashBookBaseForm()
		{
		}

		public DirectCashBookBaseForm(DirectTransactionHeaderBase directTransactionBizO)
			: base(directTransactionBizO)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);

			AH_NumberOfSupportingDocumentsCalcEdit.Visible = directTransactionBizO.AH_NumberOfSupportingDocumentsVisible_ReadOnly;

			Direct.AH_RX_NKTransactionCurrencyInfo.ValueChanged += AH_RX_NKTransactionCurrencyInfo_ValueChanged;
			DisplayModeChanged += DirectCashBookBaseForm_DisplayModeChanged;
			PlugIns.Add(ControllerIDs.eDocsPlugIn);

			directTransactionBizO.Lines.ShowGLAccountsForImportAction = ShowGLAccountsForImportAction;
		}

		void ShowGLAccountsForImportAction(AccGLHeaderCollection collection, List<AccGLHeader> glHeaderList)
		{
			ZFormModaliser.ShowDialogAndDispose(new GLAccountSelectionForm(collection, glHeaderList));
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!this.IsDesignMode())
			{
				zDropEditPlaceOfSupply.Visible = Direct.NeedPlaceOfSupplyAtHeaderLevel;
				InitTaxBranchSetting();

				if (!AccountingMasterFilesUtils.HasGLAccountSelectionAndEntry)
				{
					CashBookLineBoundGrid.RemoveFromAvailableColumns("AlternateGLAccountNumber");
					CashBookLineBoundGrid.RemoveFromAvailableColumns("AlternateGLAccountDescription");
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Direct.AH_RX_NKTransactionCurrencyInfo.ValueChanged -= AH_RX_NKTransactionCurrencyInfo_ValueChanged;
				DisplayModeChanged -= DirectCashBookBaseForm_DisplayModeChanged;
			}

			base.Dispose(disposing);
		}

		protected DirectTransactionHeaderBase Direct
		{
			get
			{
				if (fDirect == null)
				{
					fDirect = ((DirectTransactionHeaderBase)BusinessEntity);
				}
				return fDirect;
			}
		}

		DirectTransactionHeaderBase fDirect;
		ZPanel BottomPanel;
		ZPostingButtonsUserControl PostingButtonsUserControl;
		ZPanel BottomDetailsPanel;
		protected ZCalcFindBox AH_LocalExTaxAmountCalcFindBox;
		protected ZCalcFindBox AH_OSTotalAmountCalcFindBox;
		protected ZPanel TopPanel;
		protected ZDropEdit AH_ReceiptTypeDropEdit;
		protected ZDateEdit AH_PostDateDateEdit;
		protected ZTextBox AH_ChequeOrReferenceTextBox;
		protected ZTextBox AH_TransactionNumTextBox;
		protected ZTextBox AH_DescTextBox;
		protected ZExchangeRateControl ExchangeRateControl;
		protected ZGuidFindBox ChequeBookFindBox;
		protected ZGuidFindBox BankAccountsFindBox;
		protected ZDateEdit AH_InvoiceDateDateEdit;
		protected ZLabel ChequeOrReferenceLabel;
		protected ZDropEdit zDropEditPlaceOfSupply;
		ZPanel AdditionalDetailsPanel;
		ZGroupBox DirectCashBookSubAccountsGroupBox;
		DirectCashBookSubAccountsControl DirectCashBookSubAccountsControl;
		ZGuidFindBox AH_GB_TaxBranchGuidFindBox;

		void AH_RX_NKTransactionCurrencyInfo_ValueChanged(object sender, EventArgs e)
		{
			this.AH_LocalExTaxAmountCalcFindBox.Visible = Direct.AH_RX_NKTransactionCurrency != Direct.AH_Calc_LocalRXCode;
		}

		#region DisplayModeChanged

		void DirectCashBookBaseForm_DisplayModeChanged(object sender, DisplayModeChangedEventArgs e)
		{
			DocManagerReadOnlyOverrideHelper.TrySetReadOnlyOverride(BusinessEntity, e.ToMode);
		}

		#endregion

		#region override

		protected override bool ShowAuditTab => true;

		#endregion
	}
}
