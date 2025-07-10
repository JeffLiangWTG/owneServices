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
	public partial class ExportSupplierHeaderUserControl : EUNonLayoutExportSupplierHeaderUserControl
	{
		public ExportSupplierHeaderUserControl()
		{
			InitializeComponent();
		}

		protected override CargoWiseOne.ResourceStrings.ResourceStringData GetAdditionalInfoTabPageCaption(EU.Business.Declaration.JobDeclaration declaration) => CaptionProvider.AdditionalInfoTabPageCaption(false);

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

		protected override Type GetAdditionalInfosUserControlType()
		{
			return typeof(AdditionalInfosUserControl);
		}

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

			var showAgreedPlaceCodeFindBox = JobDeclaration?.IsUCC6 ?? false;
			AgreedPlaceCodeDropEdit.Visible = !showAgreedPlaceCodeFindBox;
			AgreedPlaceCodeFindBox.Visible = showAgreedPlaceCodeFindBox;
			JZ_ValuationCodeDropEdit.Visible = false;
		}
	}
}
