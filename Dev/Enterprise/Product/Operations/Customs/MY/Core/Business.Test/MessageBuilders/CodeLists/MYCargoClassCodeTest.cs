using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.MY.Business.Testing
{
	class MYCargoClassCodeTest : TestCaseWithFactory
	{
		public void TestGetCodeFromConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "vessel";
			consol.Transports[0].JW_Vessel = "vessel";

			vessel.RV_VesselType = Core.Constants.VesselType.LiquidNaturalGasTanker;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals("Liquid Bulk", "0", MYCargoClassCodes.GetCodeFromConsol(consol));

			consol.JK_ConsolMode = Core.Constants.ContainerModes.Liquid;
			AssertEquals("Liquid Bulk", "0", MYCargoClassCodes.GetCodeFromConsol(consol));

			vessel.RV_VesselType = Core.Constants.VesselType.DryCargoVessel;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals("Dry Bulk", "D", MYCargoClassCodes.GetCodeFromConsol(consol));

			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			vessel.RV_VesselType = Core.Constants.VesselType.LiquidNaturalGasTanker;
			AssertEquals("Container", "2", MYCargoClassCodes.GetCodeFromConsol(consol));
			vessel.RV_VesselType = Core.Constants.VesselType.LiveStockVessel;
			AssertEquals("Container", "2", MYCargoClassCodes.GetCodeFromConsol(consol));
			vessel.RV_VesselType = Core.Constants.VesselType.PassengerVessel;
			AssertEquals("Container", "2", MYCargoClassCodes.GetCodeFromConsol(consol));
			vessel.RV_VesselType = Core.Constants.VesselType.CarCarringVessel;
			AssertEquals("Container", "2", MYCargoClassCodes.GetCodeFromConsol(consol));

			vessel.RV_VesselType = Core.Constants.VesselType.PassengerVessel;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals("Passengers", "6", MYCargoClassCodes.GetCodeFromConsol(consol));

			consol.JK_ConsolMode = Core.Constants.ContainerModes.Liquid;
			AssertEquals("Passengers", "6", MYCargoClassCodes.GetCodeFromConsol(consol));

			vessel.RV_VesselType = Core.Constants.VesselType.LiveStockVessel;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals("Livestock", "L", MYCargoClassCodes.GetCodeFromConsol(consol));

			consol.JK_ConsolMode = Core.Constants.ContainerModes.Liquid;
			AssertEquals("Livestock", "L", MYCargoClassCodes.GetCodeFromConsol(consol));

			vessel.RV_VesselType = Core.Constants.VesselType.CarCarringVessel;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals("Vehicles", "7", MYCargoClassCodes.GetCodeFromConsol(consol));

			consol.JK_ConsolMode = Core.Constants.ContainerModes.Liquid;
			AssertEquals("Vehicles", "7", MYCargoClassCodes.GetCodeFromConsol(consol));

			vessel.RV_VesselType = Core.Constants.VesselType.RollOnRollOff;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals("Bulk", "1", MYCargoClassCodes.GetCodeFromConsol(consol));

			vessel.RV_VesselType = Core.Constants.VesselType.RollOnRollOff;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("BreakBulk", "1", MYCargoClassCodes.GetCodeFromConsol(consol));
		}
	}
}
