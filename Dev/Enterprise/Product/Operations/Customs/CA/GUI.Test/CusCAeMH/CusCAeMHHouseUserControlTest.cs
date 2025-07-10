using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CusCAeMHHouseUserControlTest : TestCaseWithFactory
	{
		public void TestDecimalOfBW_Weight()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			var message1 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Now);
			message1.EM_LinkedObject = house;
			Factory.Save();
			using (var form = new ZForm(master))
			using (var userControl = new CusCAeMHMasterUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				var houseUserControl = userControl.cusCAeMHHouseUserControl;
				var grid = houseUserControl.Controls.Find("HouseBillsGrid", true)[0] as ZGrid;
				var gridColumnInfo = grid.ColumnStyles.OfType<ZCalcEditColumnStyleInfo>().First(x => x.ColumnName == CusCAeMHHouse.Schema.BW_Weight);
				AssertEquals(3, gridColumnInfo.Decimals);
				var calcDropEdit = houseUserControl.Controls.Find("WeightCalcDropEdit", true)[0] as ZCalcDropEdit;
				AssertEquals(3, calcDropEdit.Decimals);
			}
		}

		public void TestDelete()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			var message1 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Now);
			message1.EM_LinkedObject = house;
			Factory.Save();
			using (var form = new ZForm(master))
			using (var userControl = new CusCAeMHMasterUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				var houseUserControl = userControl.cusCAeMHHouseUserControl;
				var grid = houseUserControl.Controls.Find("HouseBillsGrid", true)[0] as ZGrid;
				grid.Select(1);
				AssertEquals(1, master.HouseBills.Count);
				grid.DeleteMenuItem.PerformClick();
				AssertEquals("This Bill Of Lading can not be deleted since it has attached messages.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, master.HouseBills.Count);
			}
		}

		public void TestVisibilityOfControl()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			using (var form = new ZForm(master))
			using (var userControl = new CusCAeMHMasterUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				var houseUserControl = userControl.cusCAeMHHouseUserControl;
				Assert(!houseUserControl.HouseBillOverrideCheckBox.Visible);
				Assert(!houseUserControl.HouseBillsGrid.Columns.Contains(CusCAeMHHouse.Schema.BW_OverrideFreightDefaults));
			}

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			master.BP_ParentID = consol.PK;
			house.BW_ParentID = shipment.PK;
			using (var form = new ZForm(master))
			using (var userControl = new CusCAeMHMasterUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				var houseUserControl = userControl.cusCAeMHHouseUserControl;
				Assert(houseUserControl.HouseBillOverrideCheckBox.Visible);
				Assert(houseUserControl.HouseBillsGrid.Columns.Contains(CusCAeMHHouse.Schema.BW_OverrideFreightDefaults));
			}
		}
	}
}
