using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class CusCAeMHAddressesUserControl : ZUserControl
	{
		public CusCAeMHAddressesUserControl()
		{
			InitializeComponent();
		}

		internal void AddressesGrid_CurrentCellChanged(object sender, System.EventArgs e)
		{
			var row = this.AddressesGrid.CurrentRowIndex;
			if (row > -1)
			{
				var wrapper = (CAeMHDocAddress)this.AddressesGrid.List[row];
				this.AddressDocAddressControl.Text = wrapper.AddressDescription;
				var masterBill = this.DataSource as CusCAeMHMaster;
				if (masterBill != null)
				{
					masterBill.Lookups.CurrentAddressType = wrapper.E2_AddressType;
				}
			}
		}
	}
}
