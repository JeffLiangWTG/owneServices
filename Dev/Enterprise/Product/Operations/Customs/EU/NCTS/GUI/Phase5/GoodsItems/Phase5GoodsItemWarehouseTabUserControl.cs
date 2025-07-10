using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class Phase5GoodsItemWarehouseTabUserControl : ZUserControl
	{
		public Phase5GoodsItemWarehouseTabUserControl()
		{
			InitializeComponent();
		}

		protected new NctsHeader DataSource => base.DataSource as NctsHeader;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			WarehouseDynamicLayoutPanel.UpdateLayout(new WarehouseLayout());
		}
	}
}
