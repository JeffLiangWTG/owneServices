using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class GoodsItemPackagesUserControl : ZUserControl
	{
		public GoodsItemPackagesUserControl()
		{
			InitializeComponent();
			SetPackagePanelLayout();
		}

		protected new NctsHeader DataSource => (NctsHeader)base.DataSource;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			SetPackagePanelLayout();
		}

		void SetPackagePanelLayout()
		{
			var provider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource?.DefaultDataGroupingCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			PackageDynamicLayoutPanel.UpdateLayout(provider.GoodsItemPackagePanelLayout);
			PackagesGrid.ApplyGridColumnLayout(provider.GoodsItemPackageGridPanelLayout);
		}
	}
}
