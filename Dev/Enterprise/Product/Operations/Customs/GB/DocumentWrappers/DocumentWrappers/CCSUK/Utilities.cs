using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.DocumentWrappers.Ccsuk
{
	static class Utilities
	{
		internal static ZString GetAgentNameFromDatabase(ICcsukCusAwb iCcsukCusAwb)
		{
			return iCcsukCusAwb.Factory.GetCachedValue<ZString>("GB.CcsukWrappers.Agent_" + iCcsukCusAwb.AgentBadge,
				delegate
				{
					var agentsList = CusMAWBLookups.GetAllAgents(iCcsukCusAwb.Factory);
					var foundPair = agentsList.ToArray().Where(c => c.Code == iCcsukCusAwb.AgentBadge).FirstOrDefault();
					return foundPair != null ? foundPair.Description : string.Empty;
				});
		}

		internal static ZString GetShedNameFromDatabase(ICcsukCusAwb iCcsukCusAwb, ZString shedCode, ZString airportCode)
		{
			return shedCode.IsEmpty || airportCode.IsEmpty ? ZString.Empty : iCcsukCusAwb.Factory.GetCachedValue("GB.CcsukWrappers.Shed_" + shedCode + airportCode,
			delegate
			{
				return Shed.LoadByCode(iCcsukCusAwb.Factory, Core.Constants.CountryCodes.UnitedKingdom, airportCode + shedCode)?.Name ?? ZString.Empty;
			});
		}

		internal static ZString GetAirlineNameFromCode(ICcsukCusAwb iCcsukCusAwb, ZString airlineTwoCharCode)
		{
			return iCcsukCusAwb.Factory.GetCachedValue("GB.CcsukWrappers.Airline_" + airlineTwoCharCode,
			delegate
			{
				var airline = iCcsukCusAwb.Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, airlineTwoCharCode));
				return airline != null
							? ZString.Format("{0} {1}", airlineTwoCharCode, airline.RM_AirlineName1)
							: ZString.Empty;
			});
		}
	}
}
