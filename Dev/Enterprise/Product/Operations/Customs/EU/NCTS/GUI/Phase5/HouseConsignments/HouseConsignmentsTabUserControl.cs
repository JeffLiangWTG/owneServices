using System;
using System.Windows.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class HouseConsignmentsTabUserControl : ZUserControl
	{
		public HouseConsignmentsTabUserControl()
		{
			InitializeComponent();

			HouseConsignmentDetailsTabPage.RunWhenBindingOrFirstShown((s, args) => SetOrRemoveTransportDeparturePanelLayout());
			GoodsItemsTabPage.RunWhenBindingOrFirstShown((s, args) => SetGoodsItemsTabUserControl());
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			var layoutProvider = LayoutProvider;

			var houseConsignmentDetailsPanelLayoutWithGrid = layoutProvider.HouseConsignmentDetailsPanelLayoutWithGrid;
			HouseConsignmentDetailsDynamicLayoutPanel.UpdateLayout(houseConsignmentDetailsPanelLayoutWithGrid);
			SetupHouseConsignmentsGrid(houseConsignmentDetailsPanelLayoutWithGrid);
		}

		void SetOrRemoveTransportDeparturePanelLayout()
		{
			var houseConsignmentTransportDeparturePanelLayout = LayoutProvider.HouseConsignmentTransportDeparturePanelLayout;
			if (houseConsignmentTransportDeparturePanelLayout != null)
			{
				TransportDepartureDynamicLayoutPanel.UpdateLayout(houseConsignmentTransportDeparturePanelLayout);
			}
			else
			{
				TransportDepartureGroupBox.Visible = false;
			}
		}

		void SetGoodsItemsTabUserControl()
		{
			var userControl = (ZUserControl)Activator.CreateInstance(LayoutProvider.GoodsItemsTabUserControlType);
			userControl.Dock = System.Windows.Forms.DockStyle.Fill;
			BindingSource.SetBindingMember(userControl, nameof(NctsBill.GoodsItems));
			GoodsItemsTabPage.Controls.Add(userControl);
		}

		void SetupHouseConsignmentsGrid(IPanelLayoutWithGridProvider panelLayoutWithGridProvider)
		{
			var userControl = (ZUserControl)Activator.CreateInstance(panelLayoutWithGridProvider.GridUserControlType);
			GridContainer.Controls.Add(userControl);
			BindingSource.SetBindingMember(userControl, ".");
			userControl.Dock = DockStyle.Fill;
		}

		Control GridContainer => HouseConsignmentsSplitContainer.Panel1;

		protected new NctsHeader DataSource => base.DataSource as NctsHeader;

		INctsPhase5LayoutProvider LayoutProvider => layoutProvider ?? (layoutProvider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource?.DefaultDataGroupingCode));
		INctsPhase5LayoutProvider layoutProvider;
	}
}
