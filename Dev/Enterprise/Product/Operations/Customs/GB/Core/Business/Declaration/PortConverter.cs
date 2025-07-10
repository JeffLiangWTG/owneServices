using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business.Declaration
{
	// This lives in Biz, not in Chief, because JobDeclaration needs to see it. 

	public static class PortConverter
	{
		static RefUNLOCO ChiefToUnloco(ZString chiefPortCode, ZString transportMode, BusinessObjectFactory factory)
		{
			var result = chiefPortCode;
			RefUNLOCO port = null;
			var query = new ZQuery(RefLocoMapSchema.RY_LocalPortCode, chiefPortCode);
			query.AddToFilter(RefLocoMapSchema.RY_SystemUsage, transportMode);
			query.AddToFilter(RefLocoMapSchema.RY_RN, Core.Constants.CountryGuids.UnitedKingdom);
			var refLocoMap = factory.LoadTop1<RefLocoMap>(query);
			if (refLocoMap != null)
			{
				if (refLocoMap.LocoPort != null)
				{
					port = refLocoMap.LocoPort;
				}
			}
			return port;
		}

		public static ZString ChiefToIata(ZString chiefPortCode, ZString transportMode, BusinessObjectFactory factory)
		{
			var port = ChiefToUnloco(chiefPortCode, transportMode, factory);
			return (port != null && !port.RL_IATA.IsEmpty) ? port.RL_IATA : chiefPortCode;
		}

		public static ZString ChiefToUnlocoString(ZString chiefPortCode, ZString transportMode, BusinessObjectFactory factory)
		{
			var port = ChiefToUnloco(chiefPortCode, transportMode, factory);
			return (port != null) ? port.RL_Code : ZString.Empty;
		}

		public static ZString IataToChief(ZString iata, BusinessObjectFactory factory, string transportMode = "AIR")
		{
			return UnlocoToChief(IataToUnloco(iata, factory), factory, transportMode);
		}

		public static ZString UnlocoToIata(ZString unlocoCode, BusinessObjectFactory factory)
		{
			var result = unlocoCode;
			if (unlocoCode.Length == 5)
			{
				var unloco = new RefUNLOCO.Loader(factory).Load(unlocoCode);
				result = unloco != null && !unloco.RL_IATA.IsEmpty ? unloco.RL_IATA : unlocoCode.Right(3);
			}
			return result;
		}

		public static ZString IataToUnloco(ZString iata, BusinessObjectFactory factory)
		{
			var unloco = RefUNLOCO.LoadFromIATA(factory, iata);
			return unloco != null ? unloco.RL_Code : iata;
		}

		public static ZString UnlocoToChief(ZString sourceUnlocoPort, BusinessObjectFactory businessObjectFactory, string refLocoMapUsage = "AIR")
		{
			var chiefPort = sourceUnlocoPort.Right(3);
			var refLocoMap = RefLocoMap.Load(businessObjectFactory, sourceUnlocoPort, Core.Constants.CountryGuids.UnitedKingdom, refLocoMapUsage);
			if (refLocoMap == null)
			{
				if (refLocoMapUsage == TransportTypeList.Codes.Air)
				{
					var unloco = new RefUNLOCO.Loader(businessObjectFactory).Load(sourceUnlocoPort);
					if (unloco != null)
					{
						chiefPort = unloco.RL_IATA;
					}
				}
				else if (refLocoMapUsage == TransportTypeList.Codes.Road || refLocoMapUsage == GBTransportTypeList.Codes.ROR)
				{
					refLocoMap = RefLocoMap.Load(businessObjectFactory, sourceUnlocoPort, Core.Constants.CountryGuids.UnitedKingdom, TransportTypeList.Codes.Sea);
					if (refLocoMap != null)
					{
						chiefPort = refLocoMap.RY_LocalPortCode;
					}
				}
			}
			else
			{
				chiefPort = refLocoMap.RY_LocalPortCode;
			}
			return chiefPort;
		}

		public static CodeDescriptionPairList ConvertChiefPortsToIataCDPL(ShedCollection chiefAirportSheds, string transportMode = TransportTypeList.Codes.Air)
		{
			var list = new CodeDescriptionPairList();
			foreach (Shed shed in chiefAirportSheds)
			{
				var iataPortCode = ChiefToIata(shed.ChiefPort.IsEmpty ? shed.Code.Left(3) : shed.ChiefPort, transportMode, chiefAirportSheds.Factory);
				list.AddPairIfNotExist(iataPortCode, shed.AirportName);
			}
			list.SortByDescription();
			return list;
		}

		public static CodeDescriptionPairList ConvertChiefPortsToIataCDPL(PortCollection chiefAirports, string transportMode = TransportTypeList.Codes.Air)
		{
			var list = new CodeDescriptionPairList();
			foreach (Port chiefPort in chiefAirports)
			{
				var iataPortCode = ChiefToIata(chiefPort.Code, transportMode, chiefAirports.Factory);
				list.AddPairIfNotExist(iataPortCode, chiefPort.Name);
			}
			list.SortByDescription();
			return list;
		}
	}
}
