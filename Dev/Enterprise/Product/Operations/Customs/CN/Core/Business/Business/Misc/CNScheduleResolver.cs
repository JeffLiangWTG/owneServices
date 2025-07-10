using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public static class CNScheduleResolver
	{
		public static ZString GetMatchedUNLOCO(ZString port, BusinessObjectFactory factory)
		{
			var locoMapQuery = new ZQuery(RefLocoMapSchema.RY_RN, Core.Constants.CountryGuids.China);
			locoMapQuery.AddToFilter(RefLocoMapSchema.RY_LocalPortCode, port);
			var systemUsage = new[] { CNLocoMapSystemUsageList.Codes.CustomsPortCodeList };

			locoMapQuery.AddToFilter(RefLocoMapSchema.RY_SystemUsage, systemUsage);
			locoMapQuery.OrderBy = RefLocoMapSchema.RY_IsSystem.Name + ", " + RefLocoMapSchema.RY_RL_NKLocoPort.Name;
			var locoMap = factory.LoadTop1<RefLocoMap>(locoMapQuery);
			return locoMap?.RY_RL_NKLocoPort ?? ZString.Empty;
		}

		public static RefLocoMap GetMatchedLocoMap(ZString unloco, BusinessObjectFactory factory)
		{
			var query = new ZQuery(RefLocoMapSchema.RY_RN, Core.Constants.CountryGuids.China);
			query.AddToFilter(RefLocoMapSchema.RY_RL_NKLocoPort, unloco);
			var systemUsage = new[] { CNLocoMapSystemUsageList.Codes.CustomsPortCodeList };
			query.AddToFilter(RefLocoMapSchema.RY_SystemUsage, systemUsage);
			query.OrderBy = RefLocoMapSchema.RY_IsSystem.Name + ", " + RefLocoMapSchema.RY_LocalPortCode.Name;

			return factory.LoadTop1<RefLocoMap>(query);
		}
	}
}
