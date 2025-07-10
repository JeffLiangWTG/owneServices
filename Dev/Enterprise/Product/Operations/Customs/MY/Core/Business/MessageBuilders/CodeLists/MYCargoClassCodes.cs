using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.MY.Business
{
	public class MYCargoClassCodes : AutoMYCargoClassCodes
	{
		public static ZString GetCodeFromConsol(ForwardingConsol consol)
		{
			ZString result = MYCargoClassCodes.Codes.General;
			ZString vesselType = (consol.Vessel == null) ? ZString.Empty : consol.Vessel.RV_VesselType;

			if (Core.Constants.ContainerModes.IsContainerised(consol.JK_ConsolMode))
			{
				result = MYCargoClassCodes.Codes.Container;
			}
			else if (Core.Constants.VesselType.IsLiquidVessel(vesselType))
			{
				result = MYCargoClassCodes.Codes.LiquidBulk;
			}
			else if (vesselType == Core.Constants.VesselType.DryCargoVessel)
			{
				result = MYCargoClassCodes.Codes.DryBulk;
			}
			else if (vesselType == Core.Constants.VesselType.PassengerVessel)
			{
				result = MYCargoClassCodes.Codes.Passengers;
			}
			else if (vesselType == Core.Constants.VesselType.LiveStockVessel)
			{
				result = MYCargoClassCodes.Codes.Livestock;
			}
			else if (vesselType == Core.Constants.VesselType.CarCarringVessel)
			{
				result = MYCargoClassCodes.Codes.Vehicles;
			}
			else
			{
				result = MYCargoClassCodes.Codes.BreakBulkGeneralBulk;
			}
			return result;
		}
	}
}
