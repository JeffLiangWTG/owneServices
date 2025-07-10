using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.GUI.PlugIn;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.FR.GUI
{
	public partial class ImportSupplierHeaderUserControl : EUNonLayoutImportSupplierHeaderUserControl
	{
		public ImportSupplierHeaderUserControl()
		{
			InitializeComponent();
			ChangeAdditionalInfoTabPageCaption();
		}

		protected override void ChangeGridColumnsVisibility()
		{
			JobComInvoiceHeadersBoundGrid.InnerGrid.SetAvailability(false, JobComInvoiceHeader.Schema.JZ_OH_Supplier);
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			using (InvoiceChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				InvoiceChargesGrid.RemoveFromAvailableColumns(BaseJobComInvHeaderCharge.Schema.ChargeCodeDescription);
				InvoiceChargesGrid.ColumnStyles.Insert(1, new ZCheckBoxColumnStyleInfo
				{
					ColumnName = InvoiceCharge.Schema.IsSystemCalculated,
					IsMandatory = true,
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
				});
			}

			using (BaseGroupChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				BaseGroupChargesGrid.ColumnStyles.Insert(1, new ZCheckBoxColumnStyleInfo
				{
					ColumnName = GroupInvoiceCharge.Schema.IsSystemCalculated,
					IsMandatory = true,
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
				});
			}

			JobComInvoiceHeadersBoundGrid.InnerGrid.SetAvailability(false, JobComInvoiceHeader.Schema.JZ_ValuationCode);
			JobComInvoiceHeadersBoundGrid.InnerGrid.SetColumnVisible(true, JobComInvoiceHeader.Schema.ConsigneeOrgPK);
			JobComInvoiceHeadersBoundGrid.InnerGrid.SetColumnVisible(true, JobComInvoiceHeader.Schema.JZ_OA_ConsigneeAddress);
		}

		protected override Type GetPreviousDocumentsUserControlType()
		{
			return typeof(PreviousDocumentsUserControl);
		}

		protected override Type GetAdditionalInfosUserControlType() => IsUCC6
			? typeof(EU.GUI.PlugIn.AdditionalInfosUserControlWithGrid)
			: typeof(AdditionalInfosUserControl);

		protected override Type GetSupportingDocumentsUserControlType()
		{
			return typeof(SupportingDocumentsUserControl);
		}

		public new JobDeclaration JobDeclaration
		{
			get => (JobDeclaration)base.JobDeclaration;
			set => base.JobDeclaration = value;
		}

		protected override EU.GUI.CalculateFreightForm GetCalculateFreightForm(IJobComInvChargeCollection<JobComInvCharge> charges)
		{
			var bizObj = CalculateFreightBizObj.New(charges, JobDeclaration);
			if (bizObj != null)
			{
				return new CalculateFreightForm(bizObj);
			}
			return null;
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();

			var isUCC6 = IsUCC6;
			AgreedPlaceCodeDropEdit.Visible = !isUCC6;
			AgreedPlaceCodeFindBox.Visible = isUCC6;
			JZ_AdditionalTermsTextBox.Visible = isUCC6;
			ZG_IncotermCountryCodeFindBox.Visible = isUCC6;

			if (isUCC6)
			{
				JZ_IncoTermPlaceTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(118, 175, true);
				JZ_IncoTermPlaceTextBox.TabIndex = 9;
				JZ_ValuationCodeDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(118, 223, true);
				ValuationMethodDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(118, 247, true);
				GrossWeightCalcDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(118, 271, true);
				NetWeightCalcDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(118, 295, true);
				JZ_InvoiceCurrLandedCostExRateCalcEdit.Location = ControlDpiScalingHelper.NewScaledPoint(118, 319, true);
				NoOfPacksCalcDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(118, 343, true);
			}
			else
			{
				JZ_IncoTermPlaceTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(118, 151, true);
				JZ_IncoTermPlaceTextBox.TabIndex = 7;
				JZ_ValuationCodeDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(118, 175, true);
				ValuationMethodDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(118, 199, true);
				GrossWeightCalcDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(118, 223, true);
				NetWeightCalcDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(118, 247, true);
				JZ_InvoiceCurrLandedCostExRateCalcEdit.Location = ControlDpiScalingHelper.NewScaledPoint(118, 271, true);
				NoOfPacksCalcDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(118, 295, true);
			}

			ChangeAdditionalInfoTabPageCaption();
			JZ_ValuationCodeDropEdit.Visible = false;
		}

		void ChangeAdditionalInfoTabPageCaption()
		{
			var captionResourceString = CaptionProvider.AdditionalInfoTabPageCaption(IsUCC6);
			AdditionalInfoTabPage.Text = captionResourceString.Caption;
		}

		protected override CargoWiseOne.ResourceStrings.ResourceStringData GetAdditionalInfoTabPageCaption(EU.Business.Declaration.JobDeclaration declaration) => CaptionProvider.AdditionalInfoTabPageCaption(declaration?.IsUCC6 ?? false);

		bool IsUCC6 => JobDeclaration is JobDeclaration jobDeclaration && jobDeclaration.IsUCC6;
	}
}
