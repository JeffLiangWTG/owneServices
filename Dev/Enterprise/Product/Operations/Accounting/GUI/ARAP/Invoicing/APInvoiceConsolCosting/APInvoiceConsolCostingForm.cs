using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public enum APInvoiceConsolCostingFormResult
	{
		Apportion,
		Cancel,
		Close
	}

	public partial class APInvoiceConsolCostingForm : ZChildForm
	{
		ZArchitecture.ZGrid ApportionmentsGrid;
		ZArchitecture.ZGrid ApportionmentChargesGrid;
		ZGroupBox ApportionmentsGroupBox;
		ZGroupBox ApportionSplitChargesGroupBox;
		ZButton ApportionButton;
		ZButton CloseButton;
		ZPanel ApplyButtonPanel;

		public APInvoiceConsolCostingForm(APInvoiceConsolCosting businessObject)
			: base(businessObject)
		{
			ApportionmentList.ConsolCosts.ParentAPInvoice.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			HookEvents();
			BusinessEntity.Factory.SetContext(BusinessContext.APInvoiceApportionToConsol);
			reportingDeletedApportionmentChargesSuspender = businessObject.ParentAPInvoice.GetReportingDeletedApportionmentChargesSuspender();

			UpdateColumnStyle();
		}

		void HookEvents()
		{
			ApportionmentList.ConsolCosts.OnJobCreationError += ConsolCosts_JobCreationError;
			ApportionmentList.ConsolCosts.OnConsolChanged += ConsolCosts_OnConsolChanged;
		}

		void UnHookEvents()
		{
			ApportionmentList.ConsolCosts.OnJobCreationError -= ConsolCosts_JobCreationError;
			ApportionmentList.ConsolCosts.OnConsolChanged -= ConsolCosts_OnConsolChanged;
		}

		readonly IDisposable reportingDeletedApportionmentChargesSuspender;

		void ConsolCosts_OnConsolChanged(object sender, JobConsolCost.ConsolChangedEventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new InvoicingBaseImportApportionmentForm(e.CostImporter));
		}

		void ConsolCosts_JobCreationError(object sender, JobConsolCost.APInvoiceCostingJobCreationErrorEventArgs e)
		{
			Globals.Message.ShowError(e.ErrorMessage);
		}

		public override string FormVerb
		{
			get { return String.Empty; }
		}

		APInvoiceConsolCostingFormResult fFormResult = APInvoiceConsolCostingFormResult.Cancel;

		public APInvoiceConsolCostingFormResult FormResult
		{
			get { return fFormResult; }
		}

		void ApportionButton_Click(object sender, EventArgs e)
		{
			ContinueWithSave validateAndSaveResult = ValidateAndSave();
			if (validateAndSaveResult == ContinueWithSave.Yes)
			{
				Close();
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void BulkConsolCostImportButton_Click(object sender, EventArgs e)
		{
			if (ApportionmentList.ParentAPInvoice != null)
			{
				var isTaxBranchApplicable = AccountingMasterFilesUtils.IsTaxBranchApplicableForTransaction(ApportionmentList.ParentAPInvoice);

				if (ApportionmentList.ParentAPInvoice.AH_OH.IsEmpty || ApportionmentList.ParentAPInvoice.AH_ExchangeRate.IsEmpty || ApportionmentList.ParentAPInvoice.AH_RX_NKTransactionCurrency.IsEmpty || (isTaxBranchApplicable && ApportionmentList.ParentAPInvoice.AH_GB_TaxBranch.IsEmpty))
				{
					var errorMessageForPostToEFT = isTaxBranchApplicable
					? Res.GetString("91B36D96-9B26-4FDE-96FD-2727B61961F7", @"When ""Use Job Exchange Rate"" is ticked this screen can only be accessed when 
- The invoice Creditor, Tax Branch and Currency have been entered; and
- The invoice Currency has a current Buy exchange rate.
Please ensure a Creditor and Transaction Currency have been entered.
Please ensure that the Transaction Currency has a current BUY exchange rate recorded against it.")
					: Res.GetString("cec39cc3-b1ab-4423-9576-653521118f52", @"When ""Use Job Exchange Rate"" is ticked this screen can only be accessed when 
- The invoice Creditor and Currency have been entered; and
- The invoice Currency has a current Buy exchange rate.
Please ensure a Creditor and Transaction Currency have been entered.
Please ensure that the Transaction Currency has a current BUY exchange rate recorded against it.");

					var errorMessageForNotPostToEFT = isTaxBranchApplicable
						? Res.GetString("D3108970-6AE2-4972-BC22-F90F0C877AF5", @"To access this function, you must fill all of the following fields:

- Creditor
- Currency
- Exchange Rate
- Tax Branch")
						: Res.GetString("399fa422-f15c-449d-8e3d-c9366c287e0d", @"To access this function, you must fill all of the following fields:

- Creditor
- Currency
- Exchange Rate");

					Globals.Message.ShowError(ApportionmentList.ParentAPInvoice.AH_PostedToEFT ? errorMessageForPostToEFT : errorMessageForNotPostToEFT);
				}
				else
				{
					InvoicingBaseBulkConsolCostImportForm form = new InvoicingBaseBulkConsolCostImportForm(new InvoicingBaseBulkConsolCostImporter(ApportionmentList));
					ZFormModaliser.Show(form, this);
				}
			}
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			if (FormResult == APInvoiceConsolCostingFormResult.Cancel)
			{
				DialogResult result = Globals.Message.Show(Res.GetString("00cd2337-ef72-4b50-b995-5a34d67c4006", "Do you want to apply changes?"), Res.GetString("5847d098-3869-4fc6-80b5-afeee9763503", "Confirmation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);

				// Dear Developer, You're thinking that this dialog result should have three options, YesNoCancel.
				// Then it would be possible to have a case below where we didn't set e.Cancel to true and close the form without saving.
				// It is not that simple. The transaction window and this popup share the same Factory, so it is not trivial to back out of these changes without losing data.
				// This issue has been around for decades and it needs a fundamental redesign. Please tread carefully.
				// See the latest WI where we considered this "easy fix": WI00816176 - Payables Apportionment Loop
				if (result == DialogResult.Yes)
				{
					ContinueWithSave validateAndSaveResult = ValidateAndSave();
					if (validateAndSaveResult == ContinueWithSave.No)
					{
						e.Cancel = true;
					}
				}
				else
				{
					e.Cancel = true;
				}
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			try
			{
				base.OnClosed(e);
				ApportionmentList.ConsolCosts.ParentAPInvoice.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
				UnHookEvents();

				if (FormResult == APInvoiceConsolCostingFormResult.Apportion)
				{
					ApportionmentList.ParentAPInvoice.AsInvoicingBaseImporterTarget.ImportAllApportionmentsFromCosting_SuspendListChanged();
				}
			}
			finally
			{
				BusinessEntity.Factory.RemoveContext(BusinessContext.APInvoiceApportionToConsol);
				reportingDeletedApportionmentChargesSuspender.Dispose();
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!AutoJRJRegistryStatusHelper.IsAutoJRJEnabled())
			{
				ApportionmentChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.Constants.JR_GB_InternalBranch);
				ApportionmentChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.Constants.JR_GE_InternalDept);
				ApportionmentChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.Constants.JR_JH_InternalJob);
			}

			if (!AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value)
			{
				ApportionmentChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.Constants.JR_CostGovtChargeCode);
				ApportionmentChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.Constants.JR_SellGovtChargeCode);
				ApportionmentsGrid.RemoveFromAvailableColumns(JobConsolCost.Schema.E6_CostGovtChargeCode);
				ApportionmentsGrid.RemoveFromAvailableColumns(JobConsolCost.Schema.E6_SellGovtChargeCode);
			}

			if (!PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany))
			{
				ApportionmentChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.Constants.JR_CostPlaceOfSupply);
				ApportionmentsGrid.RemoveFromAvailableColumns(JobConsolCost.Schema.E6_PlaceOfSupply);
			}

			if (!AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
			{
				ApportionmentsGrid.RemoveFromAvailableColumns(JobConsolCostSchema.Constants.E6_SupplyType);
				ApportionmentChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.Constants.JR_CostSupplyType);
			}

			if (!AccountingMasterFilesUtils.IsTaxBranchApplicable)
			{
				ApportionmentsGrid.RemoveFromAvailableColumns(JobConsolCostSchema.Constants.E6_GB_CostTaxBranch);
				ApportionmentsGrid.RemoveFromAvailableColumns(JobConsolCost.Schema.CostTaxBranchName);
				ApportionmentChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.Constants.JR_GB_CostTaxBranch);
			}
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			if (!GlbCompany.CurrentCompany.GC_IsGSTRegistered)
			{
				ApportionmentsGrid.RemoveFromAvailableColumns(JobConsolCost.Schema.GSTInclusiveAmount);
				ApportionmentsGrid.RemoveFromAvailableColumns(JobConsolCost.Schema.E6_Calc_LocalGSTAmount);
				ApportionmentsGrid.RemoveFromAvailableColumns(JobConsolCost.Schema.E6_Calc_LocalTotalAmount);
				ApportionmentsGrid.RemoveFromAvailableColumns(JobConsolCost.Schema.E6_OSGSTRealAmount);
				ApportionmentsGrid.RemoveFromAvailableColumns(JobConsolCost.Schema.E6_OSGSTAmount_Calc);
				ApportionmentsGrid.RemoveFromAvailableColumns(JobConsolCostSchema.Constants.E6_AT_TaxRate);
				ApportionmentsGrid.RemoveFromAvailableColumns(JobConsolCostSchema.Constants.E6_TaxDate);
				ApportionmentsGrid.RemoveFromAvailableColumns(JobConsolCostSchema.Constants.E6_A9_VATClass);

				ApportionmentChargesGrid.RemoveFromAvailableColumns(JobCharge.Schema.JR_OSCostGSTAmt_Calc);
				ApportionmentChargesGrid.RemoveFromAvailableColumns(BaseCharge.Schema.JR_Calc_OSCostAmtWithGSTName);

				ConsolSummaryGrid.RemoveFromAvailableColumns(APInvoiceConsolCosting.APInvoiceConsolSummary.Schema.InvoiceCurrencyTotalAmountWithTax);
				ConsolSummaryGrid.RemoveFromAvailableColumns(APInvoiceConsolCosting.APInvoiceConsolSummary.Schema.InvoiceCurrencyTotalTaxAmount);
				ConsolSummaryGrid.RemoveFromAvailableColumns(APInvoiceConsolCosting.APInvoiceConsolSummary.Schema.LocalTotalAmountWithTax);
				ConsolSummaryGrid.RemoveFromAvailableColumns(APInvoiceConsolCosting.APInvoiceConsolSummary.Schema.LocalTotalTaxAmount);
			}

			if (!GlbCompany.CurrentCompany.IsExtraTaxApplicable())
			{
				ApportionmentsGrid.RemoveFromAvailableColumns(JobConsolCost.Schema.E6_OSExtraTaxAmount, JobConsolCost.Schema.E6_OSGSTRealAmount);
			}

			if (!GlbCompany.CurrentCompany.GC_IsWHTRegistered)
			{
				ApportionmentsGrid.RemoveFromAvailableColumns(JobConsolCost.Schema.E6_AW, JobConsolCost.Schema.E6_OSWHTAmount);
				ApportionmentChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.Constants.JR_AW_CostWHTRate, JobChargeSchema.Constants.JR_OSCostWHTAmt);
			}

			if (this.DisplayMode == ODisplayMode.ReadOnly)
			{
				BulkConsolCostImportButton.Visible = false;
				ApportionButton.Visible = false;
				CloseButton.Visible = true;

				fFormResult = APInvoiceConsolCostingFormResult.Close;
			}
		}

		APInvoiceConsolCosting ApportionmentList
		{
			get { return (APInvoiceConsolCosting)BusinessEntity; }
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			fFormResult = APInvoiceConsolCostingFormResult.Apportion;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			ConsolSummaryGrid.ContextMenu.MenuItems.Add("-");
			ConsolSummaryGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("APInvoiceConsolCostingForm|NavigateToCosts", "Navigate to Costs"), ConsolSummaryGrid_NavigateToCosts));
			ApportionmentChargesGrid.IsDataVersionLogsMenuItemVisible = false;
		}

		void ConsolSummaryGrid_NavigateToCosts(object sender, EventArgs e)
		{
			if (ConsolSummaryGrid.CurrentRowIndex != -1)
			{
				TabControl.SelectTab(ApportionmentTabPage);
				SelectCosts(ApportionmentList.ConsolSummary[ConsolSummaryGrid.CurrentRowIndex].Consol);
			}
		}

		void SelectCosts(IJobCostingPlugIn consol)
		{
			int i = 0;
			if (consol != null)
			{
				foreach (JobConsolCost consolCost in ApportionmentList.ConsolCosts)
				{
					if (consolCost.E6_ParentID == consol.CostSupporter.PK)
					{
						ApportionmentsGrid.Select(i);
					}
					i++;
				}
			}
		}

		void UpdateColumnStyle()
		{
			if (GlbCompany.CurrentCompany.IsExtraTaxApplicable())
			{
				ApportionmentsGrid.GetColumnStyle(JobConsolCost.Schema.E6_OSGSTRealAmount).CaptionResourceString =
					AccountingCaptionHelper.OSTaxAmountCaption;
				ApportionmentsGrid.GetColumnStyle(JobConsolCost.Schema.E6_OSExtraTaxAmount).CaptionResourceString =
					AccountingCaptionHelper.OSExtraTaxAmountCaption;
			}
		}

		protected override bool ExecuteAllFetchHintsBeforeValidateAll
		{
			get { return false; } // We do it to stop Freight fetch hints from executing because we do not need them
		}
	}
}
