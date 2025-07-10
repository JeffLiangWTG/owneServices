using System;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Core.Constants;

#if DEBUG
using Enterprise.ZArchitecture.GUI.Testing;
#endif

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class ClaimChargesUserControl : ZUserControl
	{
		public ClaimChargesUserControl()
		{
			InitializeComponent();

#if DEBUG
			MissingResourceStringChecker.ExcludeFromTest(this.ExtraTaxGroupBox);
#endif
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			base.OnBindingContextChanged(e);

			SetInvoiceHeaderDefaults();
			InvoiceUserControl.CollapseTableWhenBecomeVisible();
			RefreshLocalTotalsBasedOnCurrency();
		}

		#region Implementation

		void SetInvoiceHeaderDefaults()
		{
			SetVisibilityDefaults();

			if (Invoice != null)
			{
				var currCompany = GlbCompany.CurrentCompany;
				var ledger = Invoice.AH_Ledger;

				if ((ledger == LedgerTypes.AccountsPayable || ledger == LedgerTypes.UnapprovedPayableTransactions || ledger == LedgerTypes.IncompleteTransactions)
					&& currCompany.Country.SupportComplianceSubType && currCompany.Country.Code != CountryCodes.China
					&& !AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value)
				{
					var subType = InvoiceUserControl.AH_ComplianceSubTypeDropEdit;
					subType.Visible = true;
					subType.GetExtension<LabelCaptionRenderer>().Visible = true;

					var complNr = InvoiceUserControl.AH_TransactionReferenceTextBox;
					complNr.Visible = true;
					complNr.ReadOnly = true;

					var complSeq = InvoiceUserControl.ComplianceSequenceTextBox;
					complSeq.Visible = true;
					complSeq.ReadOnly = true;
					var sequence = Invoice.ComplianceSequence;
					complSeq.Text = sequence?.XD_Code + " - " + sequence?.XD_Description;
				}

				InvoiceUserControl.SourceReferenceTextBox.Visible = Invoice.IsSourceReferenceEnabled;

				if (ledger == LedgerTypes.AccountsPayable || ledger == LedgerTypes.UnapprovedPayableTransactions)
				{
					InvoiceUserControl.SetInvoiceTypeCaptions(Res.GetString("Accounting|ClaimChargesUserControl|CreditNote", "Credit Note"), Res.GetString("Accounting|ClaimChargesUserControl|CreditNote", "Credit Note"));
					InvoiceUserControl.AddressWithContactControl.CaptionResourceString = Res.GetData("7131edac-6084-41e0-ab4e-ed95455387be", "Creditor Information");
				}

				SetConditionalComponentsInvisible();

				var disbInv = InvoiceUserControl.IsDisbursementInvoiceCheckBox;
				disbInv.Visible = true;
				if (ledger == LedgerTypes.AccountsPayable || ledger == LedgerTypes.UnapprovedPayableTransactions)
				{
					if (!Invoice.IsInDatabase)
					{
						InvoiceUserControl.InvoiceTotalValidationCheckBox.Visible = true;
						InvoiceUserControl.ValidInvoiceTotalCalcEdit.Visible = true;
					}
					disbInv.Text = Res.GetString("ClaimChargesUserControl|IsSelfBillingInvoice", "Is Self Billing Invoice");
					disbInv.BindTo = Invoice.IsSelfBillingInvoiceInfo.Name;
				}
				else
				{
					if (!Invoice.IsInDatabase)
					{
						Invoice.IsDisbursementOrFinal = false;
					}
					InvoiceUserControl.AH_InvoiceTermDropEdit.Visible = true;
					InvoiceUserControl.AH_InvoiceTermDaysCalcEdit.Visible = true;
				}

				if (currCompany.GC_RN_NKCountryCode != CountryCodes.Thailand)
				{
					InvoiceUserControl.CashBasisVATIndicatorCheckbox.Visible = false;
				}
			}
		}

		void SetVisibilityDefaults()
		{
			InvoiceUserControl.AH_ComplianceSubTypeDropEdit.Visible = false;
			InvoiceUserControl.AH_ComplianceSubTypeDropEdit.GetExtension<LabelCaptionRenderer>().Visible = false;
			InvoiceUserControl.AH_TransactionReferenceTextBox.Visible = false;
			InvoiceUserControl.ComplianceSequenceTextBox.Visible = false;

			InvoiceUserControl.SourceReferenceTextBox.Visible = false;

			InvoiceUserControl.LineSummaryTabPage.RunWhenTabInitialized((sender, e) =>
			{
				var transLines = InvoiceUserControl.TransactionLinesGrid;
				transLines.RemoveFromAvailableColumns(InvoicingLineBase.Schema.AL_AW);
				transLines.RemoveFromAvailableColumns(InvoicingLineBase.Schema.AL_OSWHTAmount);
				transLines.RemoveFromAvailableColumns(InvoicingLineBase.Schema.AL_LocalWHTAmount);
			});

			var currCompany = GlbCompany.CurrentCompany;
			if (currCompany.IsExtraTaxApplicable())
			{
				this.ExtraTaxGroupBox.Visible = true;
			}

			var hideLineChargesGridAndShowRestrictionLabel = false;
			var warningMsg = string.Empty;

			if (Invoice != null && Invoice.AH_GC != currCompany.PK)
			{
				switch (Invoice.AH_Ledger)
				{
					case LedgerTypes.AccountsReceivable:
						var security = Env.Security.ReceivablesClaimsAndQueriesClaimChargesLineChgsGrd;
						if (!security.IsAllowed)
						{
							warningMsg = security.ErrorMessageForNotAllowed;
							hideLineChargesGridAndShowRestrictionLabel = true;
						}
						break;
					case LedgerTypes.AccountsPayable:
					case LedgerTypes.UnapprovedPayableTransactions:
						security = Env.Security.PayablesClaimsAndQueriesClaimChargesLineChargesGrd;
						if (!security.IsAllowed)
						{
							warningMsg = security.ErrorMessageForNotAllowed;
							hideLineChargesGridAndShowRestrictionLabel = true;
						}
						break;
				}
			}

			this.LineChargesGrid.Visible = !hideLineChargesGridAndShowRestrictionLabel;
			this.RestrictedLineChargesLabel.Visible = hideLineChargesGridAndShowRestrictionLabel;
			if (hideLineChargesGridAndShowRestrictionLabel)
			{
				this.RestrictedLineChargesLabel.Text = warningMsg;
			}
		}

		InvoicingBase Invoice => DataSource as InvoicingBase;

#if DEBUG
		public InvoicingBase InvoiceForTest => Invoice;

		public InvoiceUserControl InvoiceUserControlForTest => InvoiceUserControl;
#endif

		void RefreshLocalTotalsBasedOnCurrency()
		{
			if (Invoice != null && Invoice.AH_RX_NKTransactionCurrency == Invoice.Company.GC_RX_NKLocalCurrency)
			{
				this.AH_LocalExtraTaxAmountCalcEdit.Visible = false;
				this.AH_LocalTaxAmountCalcEdit.Visible = false;
				this.AH_LocalTotalAmountCalcEdit.Visible = false;
			}
		}

		void SetConditionalComponentsInvisible()
		{
			InvoiceUserControl.ApportionChargesButton.Visible = false;
			InvoiceUserControl.ValidInvoiceTotalCalcEdit.Visible = false;
			InvoiceUserControl.InvoiceTotalValidationCheckBox.Visible = false;
			InvoiceUserControl.TransactionGuidFindBox.Visible = false;
			InvoiceUserControl.AH_InvoiceTermDaysCalcEdit.Visible = false;
			InvoiceUserControl.AH_InvoiceTermDropEdit.Visible = false;
			InvoiceUserControl.GSTInclusiveAmountsCheckBox.Visible = false;
			InvoiceUserControl.BulkChargeImportButton.Visible = false;
		}

		#endregion
	}
}
