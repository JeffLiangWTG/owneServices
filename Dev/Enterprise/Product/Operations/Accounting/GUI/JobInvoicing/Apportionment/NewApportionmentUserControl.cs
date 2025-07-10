using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting
{
	public partial class NewApportionmentUserControl : ZUserControl
	{
		public NewApportionmentUserControl(ApportionmentListing apportionments, bool isSpecifiedToHideColumnsForGenericConsolCosting = false)
		{
			this.Apportionments = apportionments;
			IsSpecifiedToHideColumnsForGenericConsolCosting = isSpecifiedToHideColumnsForGenericConsolCosting;
			apportionments.OnJobCreationError += (_, eventArgs) => Globals.Message.Show(eventArgs.ErrorMessage);
			InitializeComponent();

			TaxDateEdit.AllowOutsideOfParent();
		}

		public NewApportionmentUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			HideNonApplicableControls();
			Form parent = FindForm();
			parent.FormClosed += new FormClosedEventHandler(Parent_FormClosed);
			if (Apportionments != null)
			{
				new QuickCalculateMenuItemManager(CostSummaryGrid, Apportionments.CostsCollection.GenericJobCostPlugIn as IRatingSupporter).AddMenuItem();
				new NewApportionmentMenuItemManager(CostSummaryGrid, Apportionments).AddMenuItem();
				if (Apportionments.CostsCollection.GenericJobCostPlugIn != null)
				{
					((BusinessObject)Apportionments.CostsCollection.GenericJobCostPlugIn).SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
					CostRateAuditTabPage.RunWhenBindingOrFirstShown(delegate { AutoRatingUIHelper.SetNoNoteExistsErrorText(AutoRatingNotePopupButton); });
				}

				this.CostSummaryGrid.GetColumnStyle(CostExchangeRateColumnName).IsReadOnly = !Apportionments.IsAllowOverrideBaseExchangeRate;
			}
			this.DetailTabPage.Text = Res.GetString("NewApportionmentUserControl|7ed803f8-2158-4129-8578-356f9c2b9b05", "Detail");
			if (Apportionments.CostsCollection.Count > Apportionments.CostsFilteredCollection.Count)
			{
				this.CostHidingMessage.Visible = true;
			}

			if (!this.Apportionments.CostsCollection.ConsolJobInvoicingEnterOrModifyCheckPoint.IsAllowed)
			{
				this.SetReadOnlyIncludingChildren();
				this.DetailTabPage.Enabled = false;
				this.CostRateAuditTabPage.Enabled = false;
			}

			CostRateAuditTabPage.RunWhenBindingOrFirstShown(delegate
			{
				WiseRatesRawDataUserControl.Visible = DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.Value;
			});

			this.RatingBehaviourDropEdit.AllowDrop = true;
		}

		void Parent_FormClosed(object sender, FormClosedEventArgs e)
		{
			Apportionments.ReleaseMutexes();
			if (Apportionments.CostsCollection.GenericJobCostPlugIn != null)
			{
				((BusinessObject)Apportionments.CostsCollection.GenericJobCostPlugIn).RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
		}

		public void HideNonApplicableControls()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				if (GlbCompany.CurrentCompany.IsExtraTaxApplicable())
				{
					ExtraTaxAmountCalcFindBox.CaptionResourceString = AccountingCaptionHelper.OSExtraTaxAmountCaption;
					CostSummaryGrid.GetColumnStyle(JobConsolCost.Schema.E6_OSExtraTaxAmount).CaptionResourceString = AccountingCaptionHelper.OSExtraTaxAmountCaption;
					CostSummaryGrid.GetColumnStyle(JobConsolCost.Schema.E6_OSGSTRealAmount).CaptionResourceString = AccountingCaptionHelper.OSTaxAmountCaption;
				}
				else
				{
					ExtraTaxPanel.Visible = false;
					CostSummaryGrid.RemoveFromAvailableColumns(JobConsolCost.Schema.E6_OSExtraTaxAmount, JobConsolCost.Schema.E6_OSGSTRealAmount);

					if (!AutoJRJRegistryStatusHelper.IsAutoJRJEnabled())
					{
						ApportionedChargesGrid.RemoveFromAvailableColumns(
							JobChargeSchema.Constants.JR_GB_InternalBranch,
							JobChargeSchema.Constants.JR_GE_InternalDept,
							JobChargeSchema.Constants.JR_JH_InternalJob);
					}
				}

				var isParentGateway = Apportionments?.CostsCollection?.IsUsedForGateway ?? false;

				if (isParentGateway || IsSpecifiedToHideColumnsForGenericConsolCosting)
				{
					CostSummaryGrid.RemoveFromAvailableColumns(JobConsolCost.Schema.E6_IsForCollectInvoice);
					IncludeOnCollectCheckBox.Visible = false;
				}
				else
				{
					ApportionedChargesGrid.RemoveFromAvailableColumns(nameof(ApportionSplitCharge.JR_Calc_RelatedJobNumber));
				}

				if (Apportionments == null || isParentGateway || IsSpecifiedToHideColumnsForGenericConsolCosting || !AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value)
				{
					ApportionedChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.Constants.JR_CostGovtChargeCode);
					ApportionedChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.Constants.JR_SellGovtChargeCode);
					CostSummaryGrid.RemoveFromAvailableColumns(JobConsolCost.Schema.E6_CostGovtChargeCode);
					CostSummaryGrid.RemoveFromAvailableColumns(JobConsolCost.Schema.E6_SellGovtChargeCode);
				}

				if (AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.Value)
				{
					CostSummaryGrid.RemoveFromAvailableColumns(JobConsolCost.Schema.IsApproved);
				}

				if (!GlbCompany.CurrentCompany.GC_IsWHTRegistered)
				{
					WithholdingTaxPanel.Visible = false;
					CostSummaryGrid.RemoveFromAvailableColumns(JobConsolCost.Schema.E6_AW, JobConsolCost.Schema.E6_OSWHTAmount);
					ApportionedChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.Constants.JR_AW_CostWHTRate, JobChargeSchema.Constants.JR_OSCostWHTAmt);
				}

				if (!PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany))
				{
					CostSummaryGrid.RemoveFromAvailableColumns(JobConsolCost.Schema.E6_PlaceOfSupply);
					PlaceOfSupplyDropEdit.Visible = false;
				}

				if (!AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
				{
					CostSupplyTypeDropEdit.Visible = false;
					CostSummaryGrid.RemoveFromAvailableColumns(JobConsolCost.Schema.E6_SupplyType);

					ApportionedChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.JR_CostSupplyType.Name);
					ApportionedChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.JR_SellSupplyType.Name);
				}

				if (!AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.Value)
				{
					CostTaxBranchGuidFindBox.Visible = false;
					CostSummaryGrid.RemoveFromAvailableColumns(JobConsolCost.Schema.E6_GB_CostTaxBranch);

					ApportionedChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.JR_GB_CostTaxBranch.Name);
					ApportionedChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.JR_GB_SellTaxBranch.Name);
				}
			}
		}

		public ZGrid GetApportionedChargesGrid() => ApportionedChargesGrid;

		readonly ApportionmentListing Apportionments;

		readonly bool IsSpecifiedToHideColumnsForGenericConsolCosting;

		const string CostExchangeRateColumnName = "CostExchangeRate+Rate";
	}
}
