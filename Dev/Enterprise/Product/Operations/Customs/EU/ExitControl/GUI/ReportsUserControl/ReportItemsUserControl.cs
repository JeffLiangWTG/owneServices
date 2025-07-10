using System;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public sealed partial class ReportItemsUserControl : ZUserControl, IReportItemsUserControl
	{
		public ReportItemsUserControl()
		{
			InitializeComponent();
			IsPackageRelatedToConsignmentItemCheckBox.CheckedChanged += IsPackageRelatedToConsignmentItemCheckBox_CheckedChanged;
		}

		new CusExitHeader DataSource => (CusExitHeader)base.DataSource;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var authorizationIsActive = false;
			if (ExitControlLayoutProvider.GetLayoutProvider(DataSource?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode) is { } provider)
			{
				if (provider.ReportItemAdditionalDocumentsGridLayout is { } additionalDocumentsGridLayout)
				{
					AdditionalDocumentGrid.ApplyGridColumnLayout(additionalDocumentsGridLayout);
				}

				if (provider.AuthorizationGridColumnLayout is { } authorizationsGridLayout)
				{
					AuthorizationsGrid.ApplyGridColumnLayout(authorizationsGridLayout);
				}

				authorizationIsActive = provider.AuthorizationIsActive;
			}
			AdditionalPanelsSplitContainer.Panel1Collapsed = !authorizationIsActive;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				var listManager = ReportItemsGrid?.ListManager;
				if (listManager != null)
				{
					listManager.CurrentChanged -= ListManager_CurrentChanged;
				}

				if (IsPackageRelatedToConsignmentItemCheckBox != null)
				{
					IsPackageRelatedToConsignmentItemCheckBox.CheckedChanged -= IsPackageRelatedToConsignmentItemCheckBox_CheckedChanged;
				}
			}
			base.Dispose(disposing);
		}

		void IsPackageRelatedToConsignmentItemCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			RefreshBindingForReportItemPackagesGrid();
		}

		void ReportItemsGrid_AfterBind(object sender, EventArgs e)
		{
			var listManager = ReportItemsGrid.ListManager;
			if (listManager != null)
			{
				listManager.CurrentChanged += ListManager_CurrentChanged;
			}
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			if (IsPackageRelatedToConsignmentItemCheckBoxChecked)
			{
				RefreshBindingForReportItemPackagesGrid();
			}
		}

		void RefreshBindingForReportItemPackagesGrid()
		{
			var listManager = ReportItemsGrid.ListManager;
			if (listManager != null)
			{
				var exitReportItem = (CusExitReportItem)listManager.GetCurrent();
				if (exitReportItem != null)
				{
					var report = exitReportItem.Report;
					if (IsPackageRelatedToConsignmentItemCheckBoxChecked)
					{
						report.UpdateAdditionalFilterForCusExitReportItemPackages(exitReportItem.ConsignmentItem.PK);
					}
					else
					{
						report.ClearAdditionalFilterForCusExitReportItemPackages();
					}
				}
			}
		}

		bool IsPackageRelatedToConsignmentItemCheckBoxChecked => IsPackageRelatedToConsignmentItemCheckBox.Checked;

		void IReportItemsUserControl.OnExitReportChanged(CusExitReport cusExitReport)
		{
			RefreshReportItemAdditionalInfosGroupBoxVisibility(cusExitReport);
		}

		void RefreshReportItemAdditionalInfosGroupBoxVisibility(CusExitReport report)
		{
			if (report != null && PackagesAndAdditionalDocumentSplitContainer != null)
			{
				PackagesAndAdditionalDocumentSplitContainer.Panel2Collapsed = !report.IsAdditionalInfosRequiredForReportAndItems;
			}
		}
	}
}
