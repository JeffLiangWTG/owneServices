using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.GVMS
{
	public class GVMSRouteCalculator
	{
		public GVMSRouteCalculator(AsycudaManifestHeader manifest)
		{
			this.manifest = Argument.NotNull(manifest, nameof(manifest));
		}

		public ZString CalculateRoute()
		{
			var factory = manifest.Factory;
			var loadPort = new RefUNLOCO.Loader(factory).Load(manifest.AMA_RL_NKPortOfLoading);
			var loadPortGVMMap = loadPort?.RefLocoMaps.FirstOrDefault(x => x.Country.Code == Core.Constants.CountryCodes.UnitedKingdom && x.RY_SystemUsage == GVMSManifestType.Codes.GoodsVehicleMovementSystemGvms)?.RY_LocalPortCode ?? ZString.Empty;

			var dischargePort = new RefUNLOCO.Loader(factory).Load(manifest.AMA_RL_NKPortOfDischarge);
			var dischargePortGVMMap = dischargePort?.RefLocoMaps.FirstOrDefault(x => x.Country.Code == Core.Constants.CountryCodes.UnitedKingdom && x.RY_SystemUsage == GVMSManifestType.Codes.GoodsVehicleMovementSystemGvms)?.RY_LocalPortCode ?? ZString.Empty;

			var carrierCode = manifest.AMA_CarrierCode;

			var calculatedRoute = ZString.Empty;

			var refCusCodeListPorts = new ZZRefCusCodeListCombinedCollection(factory, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
			refCusCodeListPorts.Load();

			if (loadPortGVMMap.IsEmpty)
			{
				loadPortGVMMap = GetPortMapWhenGVMUsageDoesNotExist(loadPort, refCusCodeListPorts);
			}

			if (dischargePortGVMMap.IsEmpty)
			{
				dischargePortGVMMap = GetPortMapWhenGVMUsageDoesNotExist(dischargePort, refCusCodeListPorts);
			}

			if (!loadPortGVMMap.IsEmpty && !dischargePortGVMMap.IsEmpty && !carrierCode.IsEmpty)
			{
				calculatedRoute = GetRouteFromPortsAndCarrier(loadPortGVMMap, dischargePortGVMMap, carrierCode);
			}

			return calculatedRoute;
		}

		ZString GetRouteFromPortsAndCarrier(ZString loadPortGVMMap, ZString dischargePortGVMMap, ZString carrierCode)
		{
			var routeFound = ZString.Empty;
			var descriptionSearch = ZString.Format("Route #{0} from {1} to {2} via {3}", RouteNumberReplaceConstant, loadPortGVMMap, dischargePortGVMMap, carrierCode);
			var routeList = manifest.Lookups.GVMSRoutesList;

			foreach (CodeDescriptionPair route in routeList)
			{
				if (route.Description == descriptionSearch.Replace(RouteNumberReplaceConstant, route.Code))
				{
					routeFound = route.Code;
					break;
				}
			}

			return routeFound;
		}

		ZString GetPortMapWhenGVMUsageDoesNotExist(RefUNLOCO dischargePort, ZZRefCusCodeListCombinedCollection refCusCodeListPorts)
		{
			var dischargePortMap = dischargePort?.RefLocoMaps.FirstOrDefault(x => x.Country.Code == Core.Constants.CountryCodes.UnitedKingdom
																				&& x.RY_SystemUsage != GVMSManifestType.Codes.GoodsVehicleMovementSystemGvms)?.RY_LocalPortCode ?? ZString.Empty;

			if (dischargePortMap.IsEmpty)
			{
				dischargePortMap = dischargePort?.Code.SubstringSafe(2) ?? ZString.Empty;
			}

			var port = refCusCodeListPorts.Cast<ZZRefCusCodeListCombined>().FirstOrDefault(x => x.ZZD_Code == dischargePortMap);
			var portAttr = port?.GetAttribute("GvmsPortId") ?? ZString.Empty;
			return portAttr;
		}

		readonly AsycudaManifestHeader manifest;

		const string RouteNumberReplaceConstant = "<ROUTENUMBER>";
	}
}
