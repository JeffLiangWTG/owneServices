using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Registry.Business.Testing
{
	sealed class HBLDeliveryModeListsTest : TestCaseWithFactory
	{
		public void TestDefaultHBLDeliveryModeList_FCL()
		{
			var hblDeliveryModeList = new HBLDeliveryModeLists(Factory).GetDefaultHBLDeliveryModeList(Core.Constants.ContainerModes.FCL);

			AssertEquals(9, hblDeliveryModeList.Count);
			AssertContainsCode(hblDeliveryModeList, "CFS/CFS");
			AssertContainsCode(hblDeliveryModeList, "CFS/CY");
			AssertContainsCode(hblDeliveryModeList, "CFS/DOOR");
			AssertContainsCode(hblDeliveryModeList, "CY/CY");
			AssertContainsCode(hblDeliveryModeList, "CY/CFS");
			AssertContainsCode(hblDeliveryModeList, "CY/DOOR");
			AssertContainsCode(hblDeliveryModeList, "DOOR/DOOR");
			AssertContainsCode(hblDeliveryModeList, "DOOR/CFS");
			AssertContainsCode(hblDeliveryModeList, "DOOR/CY");
		}

		public void TestDefaultHBLDeliveryModeList_LCL()
		{
			var hblDeliveryModeList = new HBLDeliveryModeLists(Factory).GetDefaultHBLDeliveryModeList(Core.Constants.ContainerModes.LCL);

			AssertEquals(4, hblDeliveryModeList.Count);
			AssertContainsCode(hblDeliveryModeList, "CFS/CFS");
			AssertContainsCode(hblDeliveryModeList, "CFS/DOOR");
			AssertContainsCode(hblDeliveryModeList, "DOOR/DOOR");
			AssertContainsCode(hblDeliveryModeList, "DOOR/CFS");
		}

		public void TestDefaultHBLDeliveryModeList_BCN()
		{
			var hblDeliveryModeList = new HBLDeliveryModeLists(Factory).GetDefaultHBLDeliveryModeList(Core.Constants.ContainerModes.BuyersConsol);

			AssertEquals(6, hblDeliveryModeList.Count);
			AssertContainsCode(hblDeliveryModeList, "CFS/CFS");
			AssertContainsCode(hblDeliveryModeList, "CFS/DOOR");
			AssertContainsCode(hblDeliveryModeList, "CFS/CY");
			AssertContainsCode(hblDeliveryModeList, "DOOR/DOOR");
			AssertContainsCode(hblDeliveryModeList, "DOOR/CFS");
			AssertContainsCode(hblDeliveryModeList, "DOOR/CY");
		}

		public void TestDefaultHBLDeliveryModeList_SCN()
		{
			var hblDeliveryModeList = new HBLDeliveryModeLists(Factory).GetDefaultHBLDeliveryModeList(Core.Constants.ContainerModes.ShippersConsol);

			AssertEquals(6, hblDeliveryModeList.Count);
			AssertContainsCode(hblDeliveryModeList, "CFS/CFS");
			AssertContainsCode(hblDeliveryModeList, "CFS/DOOR");
			AssertContainsCode(hblDeliveryModeList, "CY/CFS");
			AssertContainsCode(hblDeliveryModeList, "DOOR/DOOR");
			AssertContainsCode(hblDeliveryModeList, "DOOR/CFS");
			AssertContainsCode(hblDeliveryModeList, "CY/DOOR");
		}

		public void TestDefaultHBLDeliveryModeList_BBK_ROR_BLK_LQD()
		{
			var hblDeliveryModeList = new HBLDeliveryModeLists(Factory).GetDefaultHBLDeliveryModeList(Core.Constants.ContainerModes.Bulk);

			AssertEquals(4, hblDeliveryModeList.Count);
			AssertContainsCode(hblDeliveryModeList, "DOOR/DOOR");
			AssertContainsCode(hblDeliveryModeList, "DOOR/PORT");
			AssertContainsCode(hblDeliveryModeList, "PORT/PORT");
			AssertContainsCode(hblDeliveryModeList, "PORT/DOOR");
		}

		public void TestDefaultHBLDeliveryModeList_LSE()
		{
			var hblDeliveryModeList = new HBLDeliveryModeLists(Factory).GetDefaultHBLDeliveryModeList(Core.Constants.ContainerModes.Loose);
			AssertEquals(9, hblDeliveryModeList.Count);
			AssertContainsCode(hblDeliveryModeList, "CFS/DOOR");
			AssertContainsCode(hblDeliveryModeList, "CFS/DOOR");
			AssertContainsCode(hblDeliveryModeList, "DOOR/CFS");
			AssertContainsCode(hblDeliveryModeList, "DOOR/DOOR");
			AssertContainsCode(hblDeliveryModeList, "ARPT/DOOR");
			AssertContainsCode(hblDeliveryModeList, "ARPT/CFS");
			AssertContainsCode(hblDeliveryModeList, "DOOR/ARPT");
			AssertContainsCode(hblDeliveryModeList, "CFS/ARPT");
			AssertContainsCode(hblDeliveryModeList, "ARPT/ARPT");
		}

		public void TestDefaultHBLDeliveryModeList_ULD()
		{
			var hblDeliveryModeList = new HBLDeliveryModeLists(Factory).GetDefaultHBLDeliveryModeList(Core.Constants.ContainerModes.ULD);
			AssertEquals(9, hblDeliveryModeList.Count);
			AssertContainsCode(hblDeliveryModeList, "CFS/DOOR");
			AssertContainsCode(hblDeliveryModeList, "CFS/DOOR");
			AssertContainsCode(hblDeliveryModeList, "DOOR/CFS");
			AssertContainsCode(hblDeliveryModeList, "DOOR/DOOR");
			AssertContainsCode(hblDeliveryModeList, "ARPT/DOOR");
			AssertContainsCode(hblDeliveryModeList, "ARPT/CFS");
			AssertContainsCode(hblDeliveryModeList, "DOOR/ARPT");
			AssertContainsCode(hblDeliveryModeList, "CFS/ARPT");
			AssertContainsCode(hblDeliveryModeList, "ARPT/ARPT");
		}

		void AssertContainsCode(HBLDeliveryModeCollection list, string code)
		{
			AssertEquals(true, list.Cast<HBLDeliveryMode>().Any(x => x.Code == code));
		}
	}
}
