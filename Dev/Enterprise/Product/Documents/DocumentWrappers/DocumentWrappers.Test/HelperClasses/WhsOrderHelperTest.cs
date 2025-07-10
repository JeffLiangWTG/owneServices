using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class WhsOrderHelperTest : TestCaseWithFactory
	{
		#region TestGetCartageDropModeFromWhsOrder

		public void TestGetCartageDropModeFromWhsOrder()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_RL_NKClosestPort = "AU";
			consignee.OH_FullName = "My Organisation Name";

			var orgAddress = consignee.Addresses[0];
			orgAddress.OA_LCLEquipmentNeeded = Core.Constants.LCLAIREquipmentNeeded.HandUnloadLoad;
			orgAddress.OA_FCLEquipmentNeeded = Core.Constants.FCLEquipmentNeeded.WaitForUnpack;

			var order = Factory.New<WhsOrder>();
			order.ConsigneePK = consignee.PK;
			order.WD_DropMode = "";

			var helper = new WhsPickableDocketHelper(order);
			AssertEquals("Should have loaded Cartage Drop Mode for LCL freight", "HUL - Hand Unload/Load by Premise", helper.GetCartageDropModeFromWhsOrder());

			order.Containers.AddNew();
			AssertEquals("Should have loaded Cartage Drop Mode for FCL freight", "WUP - Wait for Pack/Unpack", helper.GetCartageDropModeFromWhsOrder());

			order.WD_DropMode = Core.Constants.EquipmentNeeded.Ask;
			AssertEquals("Should be the order drop mode", "ASK - Ask Client", helper.GetCartageDropModeFromWhsOrder());
		}

		#endregion
	}
}
