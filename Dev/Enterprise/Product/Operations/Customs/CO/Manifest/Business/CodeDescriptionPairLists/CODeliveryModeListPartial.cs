using CargoWise.Types;

namespace Enterprise.Customs.CO.Manifest.Business
{
	public partial class CODeliveryModeList
	{
		public static ZString MapDeliveryMode(ZString deliveryMode)
		{
			switch (deliveryMode)
			{
				case Core.Constants.DeliveryModes.Codes.CY_CY:
					return Codes._1;
				case Core.Constants.DeliveryModes.Codes.CY_CFS:
					return Codes._2;
				case Core.Constants.DeliveryModes.Codes.CFS_CY:
					return Codes._3;
				case Core.Constants.DeliveryModes.Codes.CFS_CFS:
					return Codes._4;
				default:
					return ZString.Empty;
			}
		}
	}
}
