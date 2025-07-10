using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingPresentationProviders;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Presentation;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Accounting.GUI.Base
{
	public partial class TransactionReasonForm : ZChildForm
	{
		public TransactionReasonForm()
		{ }

		public TransactionReasonForm(TransactionReasonHolder reversingHolder, string reasonTextBoxCaption, string formCaption)
			: base(reversingHolder)
		{
			InitializeComponent();

			this.reversingHolder = reversingHolder;

			if (ShouldDisplaySupportingDocumentNumberTextBox)
			{
				SupportingDocumentNumLabel.Visible = true;
				SupportingDocumentNumberTextBox.Visible = true;
			}
#if DEBUG
			MissingResourceStringChecker.ExcludeFromTest(ReasonTextBox);
			TypeDescriptor.AddAttributes(ReasonTextBox, new SuppressFormsLocalizedTestAttribute());
			MissingResourceStringChecker.ExcludeFromTest(ReasonLabel);
			TypeDescriptor.AddAttributes(ReasonLabel, new SuppressFormsLocalizedTestAttribute());
#endif

			Text = formCaption;
			ReasonLabel.GetExtension<ILabelCaptionRenderer>().Caption = reasonTextBoxCaption + ":";

			if (TransactionReasonFormPresentationProvider.IsAmendInFullVisible(reversingHolder.BusinessEntity as InvoicingBase))
			{
				IsAmendInFullCheckBox.Visible = true;
			}
		}

		readonly TransactionReasonHolder reversingHolder;

		bool ShouldDisplaySupportingDocumentNumberTextBox
		{
			get
			{
				return GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.VietNam
					&& (
						(reversingHolder.Category == TransactionReasonCategory.ReverseReason && reversingHolder.BusinessEntity is ARCreditNote)
						|| (reversingHolder.Category == TransactionReasonCategory.AmendmentReason && (reversingHolder.BusinessEntity is ARCreditNote || reversingHolder.BusinessEntity is ARInvoice))
						);
			}
		}

		bool ShouldPopulateSupportingDocumentNumber
		{
			get
			{
				return ShouldPopulateSupportingDocumentNumber_ReverseReason || ShouldPopulateSupportingDocumentNumber_AmendmentReason;
			}
		}

		bool ShouldPopulateSupportingDocumentNumber_ReverseReason
		{
			get
			{
				return AccountingUtils.IsVietnamCompanyEInvoicingEnabled
					&& reversingHolder.Category == TransactionReasonCategory.ReverseReason
					&& reversingHolder.BusinessEntity is ARCreditNote arCreditNote
					&& arCreditNote.OriginalTransaction?.EInvoicingTransactionPivotSubmitted != null;
			}
		}

		bool ShouldPopulateSupportingDocumentNumber_AmendmentReason
		{
			get
			{
				return AccountingUtils.IsVietnamCompanyEInvoicingEnabled
					&& reversingHolder.Category == TransactionReasonCategory.AmendmentReason
					&& reversingHolder.BusinessEntity is IAmending amending
					&& amending.OriginalTransaction is TransactionHeader originalTransaction
					&& !string.IsNullOrEmpty(originalTransaction.AH_TransactionReference)
					&& !string.IsNullOrEmpty(originalTransaction.AH_ComplianceSubType)
					&& (
						(AccountingConfigurationRegistry.Instance.VietnamEInvoicingAdjustment.Value && reversingHolder.BusinessEntity is ARCreditNote)
						|| (AccountingConfigurationRegistry.Instance.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.Value && reversingHolder.BusinessEntity is ARInvoice)
					);
			}
		}

		void OKReasonButton_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(ReversingReasonCodeDropEdit.CodeBox.Text) || !reversingHolder.TransactionReasonCodes_List.ContainsCode(ReversingReasonCodeDropEdit.CodeBox.Text))
			{
				Globals.Message.ShowError(Res.GetString("1ee556d4-da9f-4643-8ce0-29196e69eeee", "Enter a valid reason code"));
			}
			else if (string.IsNullOrEmpty(ReasonTextBox.Text))
			{
				Globals.Message.ShowError(Res.GetString("f516f85f-e99e-4edc-8a82-d08ed8f118c5", "Please enter a non-blank reason text!"));
			}
			else if (string.IsNullOrEmpty(SupportingDocumentNumberTextBox.Text) && ShouldPopulateSupportingDocumentNumber)
			{
				Globals.Message.ShowError(AccountingConstants.AccountingSupportingDocumentNumberErrorMessage.EmptySupportingDocumentNumber);
			}
			else
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		void CancelReasonButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (DialogResult != DialogResult.OK)
			{
				reversingHolder.Reason = string.Empty;
				reversingHolder.Code = string.Empty;
				reversingHolder.SupportingDocumentNumber = string.Empty;
			}
			base.OnFormClosing(e);
		}

		ITransactionReasonFormPresentationProvider TransactionReasonFormPresentationProvider => transactionReasonFormPresentationProvider ?? (transactionReasonFormPresentationProvider = ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetTransactionReasonFormPresentationProvider());
		ITransactionReasonFormPresentationProvider transactionReasonFormPresentationProvider;
	}
}
