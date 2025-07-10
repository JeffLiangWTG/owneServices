using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CusCAeMHAddressesUserControlTest : TestCaseWithFactory
	{
		public void TestAddressDocAddressControlCaption()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			var address1 = house.DocAddresses.AddNew();
			address1.E2_AddressType = DocAddressTypes.Codes.Carrier;
			var address2 = house.DocAddresses.AddNew();
			address2.E2_AddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;
			using (var form = new CusCAeMHMasterForm(master))
			{
				form.Show();
				var houseTabPage = (ZTabPage)form.Controls.Find("eMHHouseTabPage", true)[0];
				var addressTabPage = (ZTabPage)houseTabPage.Controls.Find("AddressesTabPage", true)[0];
				var houseTabControl = (ZTabControl)houseTabPage.Controls.Find("HouseTabControl", true)[0];
				var adressesUserControl = (CusCAeMHAddressesUserControl)houseTabPage.Controls.Find("cusCAeMHAddressesUserControl", true)[0];
				houseTabControl.SelectedTab = addressTabPage;
				var addressGrid = (ZGrid)addressTabPage.Controls.Find("AddressesGrid", true)[0];
				var addressControl = (ZDocAddressControl)addressTabPage.Controls.Find("AddressDocAddressControl", true)[0];
				addressGrid.CurrentRowIndex = 0;
				adressesUserControl.AddressesGrid_CurrentCellChanged(null, null);
				AssertEquals("Carrier", addressControl.Text);
				AssertType<ShippingProviderCollection>(master.Lookups.ThirdParties);
				addressGrid.CurrentRowIndex = 1;
				adressesUserControl.AddressesGrid_CurrentCellChanged(null, null);
				AssertEquals("Shipper", addressControl.Text);
				AssertType<ConsignorCollection>(master.Lookups.ThirdParties);
			}
		}
	}
}
