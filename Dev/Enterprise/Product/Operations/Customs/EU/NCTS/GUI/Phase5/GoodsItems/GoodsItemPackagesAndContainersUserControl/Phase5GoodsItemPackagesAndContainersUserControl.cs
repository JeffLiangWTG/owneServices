using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5GoodsItemPackagesAndContainersUserControl : ZUserControl
	{
		public Phase5GoodsItemPackagesAndContainersUserControl()
		{
			InitializeComponent();
		}

		protected new NctsHeader DataSource => base.DataSource as NctsHeader;

		protected INctsPhase5LayoutProvider LayoutProvider => layoutProvider ??= NctsPhase5LayoutProvider.GetLayoutProvider(DataSource?.DefaultDataGroupingCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		INctsPhase5LayoutProvider layoutProvider;
	}
}
