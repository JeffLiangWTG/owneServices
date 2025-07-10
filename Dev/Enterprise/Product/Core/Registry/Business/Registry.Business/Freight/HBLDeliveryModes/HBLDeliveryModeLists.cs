using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Registry.Business
{
	public class HBLDeliveryModeLists
	{
		public HBLDeliveryModeLists(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, "factory");
		}
		readonly BusinessObjectFactory factory;

		HBLDeliveryModeCollection DefaultHBLDeliveryModeList_FCL
		{
			get
			{
				return factory.GetCachedValue<HBLDeliveryModeCollection>("HBLDeliveryModeLists.DefaultHBLDeliveryModeList_FCL", delegate
				{
					var result = new HBLDeliveryModeCollection();
					result.Add(Core.Constants.HBLDeliveryModes.Codes.CFS_CFS, Core.Constants.HBLDeliveryModes.Descriptions.CFS_CFS);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.CFS_CY, Core.Constants.HBLDeliveryModes.Descriptions.CFS_CY);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR, Core.Constants.HBLDeliveryModes.Descriptions.CFS_DOOR);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.CY_CY, Core.Constants.HBLDeliveryModes.Descriptions.CY_CY);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.CY_CFS, Core.Constants.HBLDeliveryModes.Descriptions.CY_CFS);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.CY_DOOR, Core.Constants.HBLDeliveryModes.Descriptions.CY_DOOR);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR, Core.Constants.HBLDeliveryModes.Descriptions.DOOR_DOOR);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS, Core.Constants.HBLDeliveryModes.Descriptions.DOOR_CFS);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.DOOR_CY, Core.Constants.HBLDeliveryModes.Descriptions.DOOR_CY);
					return result;
				});
			}
		}

		HBLDeliveryModeCollection DefaultHBLDeliveryModeList_LCL
		{
			get
			{
				return factory.GetCachedValue<HBLDeliveryModeCollection>("HBLDeliveryModeLists.DefaultHBLDeliveryModeList_LCL", delegate
				{
					var result = new HBLDeliveryModeCollection();
					result.Add(Core.Constants.HBLDeliveryModes.Codes.CFS_CFS, Core.Constants.HBLDeliveryModes.Descriptions.CFS_CFS);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR, Core.Constants.HBLDeliveryModes.Descriptions.CFS_DOOR);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR, Core.Constants.HBLDeliveryModes.Descriptions.DOOR_DOOR);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS, Core.Constants.HBLDeliveryModes.Descriptions.DOOR_CFS);
					return result;
				});
			}
		}

		HBLDeliveryModeCollection DefaultHBLDeliveryModeList_BCN
		{
			get
			{
				return factory.GetCachedValue<HBLDeliveryModeCollection>("HBLDeliveryModeLists.DefaultHBLDeliveryModeList_BCN", delegate
				{
					var result = new HBLDeliveryModeCollection();
					result.Add(Core.Constants.HBLDeliveryModes.Codes.CFS_CFS, Core.Constants.HBLDeliveryModes.Descriptions.CFS_CFS);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR, Core.Constants.HBLDeliveryModes.Descriptions.CFS_DOOR);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.CFS_CY, Core.Constants.HBLDeliveryModes.Descriptions.CFS_CY);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR, Core.Constants.HBLDeliveryModes.Descriptions.DOOR_DOOR);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS, Core.Constants.HBLDeliveryModes.Descriptions.DOOR_CFS);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.DOOR_CY, Core.Constants.HBLDeliveryModes.Descriptions.DOOR_CY);
					return result;
				});
			}
		}

		HBLDeliveryModeCollection DefaultHBLDeliveryModeList_SCN
		{
			get
			{
				return factory.GetCachedValue<HBLDeliveryModeCollection>("HBLDeliveryModeLists.DefaultHBLDeliveryModeList_SCN", delegate
				{
					var result = new HBLDeliveryModeCollection();
					result.Add(Core.Constants.HBLDeliveryModes.Codes.CFS_CFS, Core.Constants.HBLDeliveryModes.Descriptions.CFS_CFS);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR, Core.Constants.HBLDeliveryModes.Descriptions.CFS_DOOR);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.CY_CFS, Core.Constants.HBLDeliveryModes.Descriptions.CY_CFS);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR, Core.Constants.HBLDeliveryModes.Descriptions.DOOR_DOOR);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS, Core.Constants.HBLDeliveryModes.Descriptions.DOOR_CFS);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.CY_DOOR, Core.Constants.HBLDeliveryModes.Descriptions.CY_DOOR);
					return result;
				});
			}
		}

		HBLDeliveryModeCollection DefaultHBLDeliveryModeList_BBK_ROR_BLK_LQD
		{
			get
			{
				return factory.GetCachedValue<HBLDeliveryModeCollection>("HBLDeliveryModeLists.DefaultHBLDeliveryModeList_BBK_ROR_BLK_LQD", delegate
				{
					var result = new HBLDeliveryModeCollection();
					result.Add(Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR, Core.Constants.HBLDeliveryModes.Descriptions.DOOR_DOOR);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.DOOR_PORT, Core.Constants.HBLDeliveryModes.Descriptions.DOOR_PORT);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.PORT_PORT, Core.Constants.HBLDeliveryModes.Descriptions.PORT_PORT);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.PORT_DOOR, Core.Constants.HBLDeliveryModes.Descriptions.PORT_DOOR);
					return result;
				});
			}
		}

		HBLDeliveryModeCollection DefaultHBLDeliveryModeList_LSE_ULD
		{
			get
			{
				return factory.GetCachedValue<HBLDeliveryModeCollection>("HBLDeliveryModeLists.DefaultHBLDeliveryModeList_LSE_ULD", delegate
				{
					var result = new HBLDeliveryModeCollection();
					result.Add(Core.Constants.HBLDeliveryModes.Codes.CFS_CFS, Core.Constants.HBLDeliveryModes.Descriptions.CFS_CFS);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR, Core.Constants.HBLDeliveryModes.Descriptions.CFS_DOOR);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS, Core.Constants.HBLDeliveryModes.Descriptions.DOOR_CFS);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR, Core.Constants.HBLDeliveryModes.Descriptions.DOOR_DOOR);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.ARPT_DOOR, Core.Constants.HBLDeliveryModes.Descriptions.ARPT_DOOR);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.ARPT_CFS, Core.Constants.HBLDeliveryModes.Descriptions.ARPT_CFS);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.DOOR_ARPT, Core.Constants.HBLDeliveryModes.Descriptions.DOOR_ARPT);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.CFS_ARPT, Core.Constants.HBLDeliveryModes.Descriptions.CFS_ARPT);
					result.Add(Core.Constants.HBLDeliveryModes.Codes.ARPT_ARPT, Core.Constants.HBLDeliveryModes.Descriptions.ARPT_ARPT);
					return result;
				});
			}
		}

		public HBLDeliveryModeCollection GetDefaultHBLDeliveryModeList(ZString shipmentPackingMode)
		{
			var result = new HBLDeliveryModeCollection(shipmentPackingMode);
			if (shipmentPackingMode == Core.Constants.ContainerModes.FCL)
			{
				result.AddRange(DefaultHBLDeliveryModeList_FCL);
			}
			else if (shipmentPackingMode == Core.Constants.ContainerModes.LCL)
			{
				result.AddRange(DefaultHBLDeliveryModeList_LCL);
			}
			else if (shipmentPackingMode == Core.Constants.ContainerModes.BuyersConsol)
			{
				result.AddRange(DefaultHBLDeliveryModeList_BCN);
			}
			else if (shipmentPackingMode == Core.Constants.ContainerModes.ShippersConsol)
			{
				result.AddRange(DefaultHBLDeliveryModeList_SCN);
			}
			else if (shipmentPackingMode == Core.Constants.ContainerModes.Bulk
					|| shipmentPackingMode == Core.Constants.ContainerModes.BreakBulk
					|| shipmentPackingMode == Core.Constants.ContainerModes.RollOnRollOff
					|| shipmentPackingMode == Core.Constants.ContainerModes.Liquid)
			{
				result.AddRange(DefaultHBLDeliveryModeList_BBK_ROR_BLK_LQD);
			}
			else if (shipmentPackingMode == Core.Constants.ContainerModes.Loose
					|| shipmentPackingMode == Core.Constants.ContainerModes.ULD)
			{
				result.AddRange(DefaultHBLDeliveryModeList_LSE_ULD);
			}
			return result;
		}
	}
}
