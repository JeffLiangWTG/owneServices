using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class CusCAeMHHouseUserControl : ZUserControl
	{
		public CusCAeMHHouseUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(System.EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			var masterBill = this.DataSource as CusCAeMHMaster;
			if (masterBill == null || masterBill.Consol == null)
			{
				this.HouseBillOverrideCheckBox.Visible = false;
				this.HouseBillsGrid.RemoveFromAvailableColumns(CusCAeMHHouse.Schema.BW_OverrideFreightDefaults);
			}
		}
	}
}
