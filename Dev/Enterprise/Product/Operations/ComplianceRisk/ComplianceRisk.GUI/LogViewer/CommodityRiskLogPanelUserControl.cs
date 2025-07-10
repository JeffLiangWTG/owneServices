using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ComplianceRisk.GUI
{
	public partial class CommodityRiskLogPanelUserControl : ZUserControl
	{
		public CommodityRiskLogPanelUserControl()
		{
			InitializeComponent();

			CommodityRiskLogGrid.SnapshotCommoditiesGrid.GetColumnStyle("NomenclatureCondition").IsVisible = false;
			CommodityRiskLogGrid.SnapshotCommoditiesGrid.GetColumnStyle("SpecificCondition").IsVisible = false;
		}
	}
}
