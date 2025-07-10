using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class Phase5ContainersAndSealsUserControl : ZUserControl
	{
		public Phase5ContainersAndSealsUserControl()
		{
			InitializeComponent();

			ContainerTabPage.RunWhenBindingOrFirstShown((s, args) => UpdateGridColumnLayout());
		}

		protected new NctsHeader DataSource => (NctsHeader)base.DataSource;

		void UpdateGridColumnLayout()
		{
			var layoutProvider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource?.DefaultDataGroupingCode);
			ContainersGrid.ApplyGridColumnLayout(layoutProvider.ContainersGridColumnLayout);
			AdditionalSealsGrid.ApplyGridColumnLayout(layoutProvider.AdditionalSealsGridColumnLayout);
		}
	}
}
