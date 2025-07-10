using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.GUI.Module;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ComplianceRisk.GUI
{
	public partial class CommodityAssessmentPanelUserControl : ZUserControl
	{
		readonly ComplianceRiskPlugInBusinessObject compliancePluginBizObj;

		public CommodityAssessmentPanelUserControl(ComplianceRiskPlugInBusinessObject compliancePluginBizO)
		{
			compliancePluginBizObj = compliancePluginBizO;
			base.SetDataBinding(compliancePluginBizO, "");

			InitializeComponent();
			InitializeHideShowButton();
			AddUserControls(compliancePluginBizO);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is null)
			{
				base.SetDataBinding(dataSource: null, dataMember: "");
			}
		}

		void AddUserControls(ComplianceRiskPlugInBusinessObject compliancePluginBizO)
		{
			var commodityRiskUserControl = ComplianceRiskPlugInUserControl.CreateCommodityRiskUserControl(compliancePluginBizO);
			commodityRiskUserControl.Dock = DockStyle.Fill;
			FilterPanel.Controls.Add(commodityRiskUserControl);
			BindingSource.SetBindingMember(commodityRiskUserControl, "ComplianceRiskStatus");

			CommodityFilterStrip = new ZCommodityFilterStripControl(commodityRiskUserControl.FindSingle<ZGrid>(u => u.Name == "CommodityRiskGrid"), new CommodityDetailFilterBusinessObject());
			CommodityFilterStrip.AllowDrop = true;
			CommodityFilterStrip.Dock = DockStyle.Top;
			CommodityFilterStrip.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			CommodityFilterStrip.Name = "CommodityFilterStrip";
			CommodityFilterStrip.AutoSize = true;
			CommodityFilterStrip.TabIndex = 1;
			CommodityFilterStrip.CaptionRenderingEnabled = true;
			CommodityFilterStrip.PerformSearch += FilterStripControl_PerformSearch;
			FilterPanel.Controls.Add(CommodityFilterStrip);

			var commodityAssessmentRiskUserControl = new CommodityAssessmentRiskUserControl();
			commodityAssessmentRiskUserControl.Dock = DockStyle.Fill;
			commodityAssessmentRiskUserControl.Name = "CommodityAssessmentRiskUserControl";
			if (!ComplianceRiskSecurityRights.IsAllowedEditComplianceAssessmentWithShowError(compliancePluginBizO.HostBusinessEntity, showErrorWhenNotAllowed: false))
			{
				commodityAssessmentRiskUserControl.SetToDisabledWithoutCommodityBorderWiseLinkLabel();
			}

			CommodityAssessmentGroupBox.Controls.Add(commodityAssessmentRiskUserControl);

			BindingSource.SetBindingMember(commodityAssessmentRiskUserControl, "ComplianceRiskStatus.CommodityDetailCollectionView");

			SetCommodityFilterStripVisible(false);
		}

		void InitializeHideShowButton()
		{
			var showHideFilterMenuItem = new ZMenuItem(Res.GetData("317D74BD-32A8-4001-BEA7-0B647BAE2633", "Hide/Show filters"), ShowFilterMenuItem_Click, IconTypes.CollapseButtonActive, IconTypes.CollapseButtonRest);
			HideShowToolStripItem = MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(showHideFilterMenuItem);
			HideShowToolStripItem.ImageScaling = ToolStripItemImageScaling.SizeToFit;
			Toolstrip.Items.Add(HideShowToolStripItem);

			SetFilterCountInfoLabelText(compliancePluginBizObj?.ComplianceRiskStatus.CommodityDetailCollectionView.Count ?? 0
				, compliancePluginBizObj?.ComplianceRiskStatus.CommodityDetailCollection.Count ?? 0);

			if (compliancePluginBizObj != null)
			{
				compliancePluginBizObj.ComplianceRiskStatus.CommodityDetailCollection.CountChanged -= CommodityDetailCollection_CountChanged;
				compliancePluginBizObj.ComplianceRiskStatus.CommodityDetailCollection.CountChanged += CommodityDetailCollection_CountChanged;

				compliancePluginBizObj.ComplianceRiskStatus.CommodityDetailCollectionView.CountChanged -= CommodityDetailCollection_CountChanged;
				compliancePluginBizObj.ComplianceRiskStatus.CommodityDetailCollectionView.CountChanged += CommodityDetailCollection_CountChanged;
			}
		}

		void CommodityDetailCollection_CountChanged(object sender, CargoWise.EntityFramework.CollectionCountChangedEventArgs e)
		{
			SetFilterCountInfoLabelText(compliancePluginBizObj?.ComplianceRiskStatus.CommodityDetailCollectionView.Count ?? 0
				, compliancePluginBizObj?.ComplianceRiskStatus.CommodityDetailCollection.Count ?? 0);
		}

		void ShowFilterMenuItem_Click(object sender, EventArgs e)
		{
			SetCommodityFilterStripVisible(!CommodityFilterStrip.Visible);
		}

		void SetCommodityFilterStripVisible(bool visible)
		{
			CommodityFilterStrip.Visible = visible;
			if (visible)
			{
				HideShowToolStripItem.ImageIndex = Icons.GetImageIndex(IconTypes.CollapseButtonRest);
				HideShowToolStripItem.Image = Icons.ImageList.Images[Icons.GetImageIndex(IconTypes.CollapseButtonActive)];
			}
			else
			{
				HideShowToolStripItem.ImageIndex = Icons.GetImageIndex(IconTypes.ExpandButtonRest);
				HideShowToolStripItem.Image = Icons.ImageList.Images[Icons.GetImageIndex(IconTypes.ExpandButtonActive)];
			}
		}

		internal void FilterStripControl_PerformSearch(object sender, EventArgs e)
		{
			var commodityDetailView = compliancePluginBizObj?.ComplianceRiskStatus.CommodityDetailCollectionView;
			if (commodityDetailView != null)
			{
				commodityDetailView.Filter = CommodityFilterStrip?.FilterBusinessObject?.Filter;
				commodityDetailView.Rebuild();
			}

			var commodityDetailTotalCollection = compliancePluginBizObj?.ComplianceRiskStatus.CommodityDetailCollection;
			SetFilterCountInfoLabelText(commodityDetailView?.Count ?? 0, commodityDetailTotalCollection?.Count ?? 0);
			SetActiveFilterCount();
		}

		internal void SetFilterCountInfoLabelText(int commodityDetailViewCount, int commodityDetailTotalCollectionCount)
		{
			FilterCountInfoLabel.Text = Res.GetString("735CE519-0AEA-469B-9E29-64F3632EE3AF", "{0} of {1} commodities", commodityDetailViewCount, commodityDetailTotalCollectionCount);
		}

		internal void RemoveFiltersButton_Click(object sender, EventArgs e)
		{
			CommodityFilterStrip.ResetFilterStrips();
			FilterStripControl_PerformSearch(sender, e);
		}

		void SetActiveFilterCount()
		{
			if (CommodityFilterStrip != null)
			{
				var filtersCount = CommodityFilterStrip.FilterBusinessObject.ActiveModuleFilters.Count;
				var extraString = filtersCount > 0 ? $"({filtersCount})" : string.Empty;
				HideShowToolStripItem.Text = Res.GetString("8597548a-94ed-44f9-b002-12d6afe3b767", "Hide/Show filters {0}", extraString);
			}
		}

		class ZCommodityFilterStripControl : ZFilterStripBaseControl
		{
			public ZCommodityFilterStripControl(ZGrid grid, ZArchitecture.Business.FilterStripBusinessObject filterBusinessObject) : base(grid, filterBusinessObject)
			{
			}

			protected override int MaxFilterStripPanelHeight => ControlDpiScalingHelper.ScaleToCurrentDpiY(78);
		}
	}
}
