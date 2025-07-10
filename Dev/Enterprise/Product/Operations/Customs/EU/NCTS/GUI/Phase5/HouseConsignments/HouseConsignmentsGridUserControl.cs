using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed partial class HouseConsignmentsGridUserControl : ZUserControl
	{
		public HouseConsignmentsGridUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			UpdateGridColumnLayout();
		}

		new NctsHeader DataSource => base.DataSource as NctsHeader;

		void UpdateGridColumnLayout()
		{
			var header = DataSource;
			var layoutProvider = NctsPhase5LayoutProvider.GetLayoutProvider(header?.DefaultDataGroupingCode);
			var gridColumnLayout = header != null && header.IsInPhase5TransitionPeriod && layoutProvider is INctsPhase5TransitionPeriodLayoutProvider transitionPeriodLayoutProvider
				? transitionPeriodLayoutProvider.GetHouseConsignmentDetailsGridColumnLayout()
				: layoutProvider.GetHouseConsignmentDetailsGridColumnLayout();
			HouseConsignmentsGrid.ApplyGridColumnLayout(gridColumnLayout);
		}
	}
}
