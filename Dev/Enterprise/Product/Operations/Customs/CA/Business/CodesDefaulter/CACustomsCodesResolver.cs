using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public enum CACustomsCodeType { Office, SubLocation }

	public static class CACustomsCodesResolver
	{
		public static List<RefLocoMap> GetMatchesForCodeType(CACustomsCodeType codeType, ZString unLoco, ZString transportMode, BusinessObjectFactory factory)
		{
			var port = new RefUNLOCO.Loader(factory).Load(unLoco);
			return GetMatchesForCodeType(codeType, port, transportMode);
		}

		public static List<RefLocoMap> GetMatchesForCodeType(CACustomsCodeType codeType, RefUNLOCO port, ZString transportMode)
		{
			var result = new List<RefLocoMap>();
			if (port != null)
			{
				if (codeType == CACustomsCodeType.Office)
				{
					if (transportMode == TransportTypeList.Codes.Air || transportMode == TransportTypeList.Codes.Sea)
					{
						result = GetMatchingRefLocoMaps(port, new[] { transportMode.ToString() });
					}

					if (result.Count == 0)
					{
						result = GetMatchingRefLocoMaps(port, new[] { CALocoMapSystemUsageList.Codes.All });
					}

					if (result.Count == 0)
					{
						result = GetMatchingRefLocoMaps(port, new[] { CALocoMapSystemUsageList.Codes.Oth });
					}
				}
				else if (codeType == CACustomsCodeType.SubLocation)
				{
					result = GetMatchingRefLocoMaps(port, new[] { CALocoMapSystemUsageList.Codes.Sub });
				}
			}
			return result;
		}

		public static ZString MatchingUNLOCO(ZString officePortCode, BusinessObjectFactory factory)
		{
			var locoMapQuery = new ZQuery(RefLocoMapSchema.RY_RN, Core.Constants.CountryGuids.Canada);
			locoMapQuery.AddToFilter(RefLocoMapSchema.RY_LocalPortCode, officePortCode);

			string[] systemUsage = new string[]
			{
				CALocoMapSystemUsageList.Codes.Oth,
				CALocoMapSystemUsageList.Codes.All,
				CALocoMapSystemUsageList.Codes.Sea,
				CALocoMapSystemUsageList.Codes.Air
			};

			locoMapQuery.AddToFilter(RefLocoMapSchema.RY_SystemUsage, systemUsage);

			var locoMap = factory.LoadTop1<RefLocoMap>(locoMapQuery);
			return locoMap != null ? locoMap.RY_RL_NKLocoPort : ZString.Empty;
		}

		static List<RefLocoMap> GetMatchingRefLocoMaps(RefUNLOCO port, string[] systemUsage)
		{
			var result = GetMatchingRefLocoMaps(port, systemUsage, false);
			if (result.Count == 0)
			{
				result = GetMatchingRefLocoMaps(port, systemUsage, true);
			}

			return result;
		}

		static List<RefLocoMap> GetMatchingRefLocoMaps(RefUNLOCO port, string[] systemUsage, bool isSystem)
		{
			ZQuery loclMapQuery = new ZQuery(RefLocoMapSchema.RY_RN, Core.Constants.CountryGuids.Canada);
			loclMapQuery.AddToFilter(RefLocoMapSchema.RY_SystemUsage, systemUsage);
			loclMapQuery.AddToFilter(RefLocoMapSchema.RY_LocalPortCode, SQLComparisonOperator.IsNotBlank, ZString.Empty);
			loclMapQuery.AddToFilter(RefLocoMapSchema.RY_IsSystem, isSystem);

			return new List<RefLocoMap>(port.RefLocoMaps.Find(loclMapQuery));
		}

		public static ZString MatchingCustomsCode(CACustomsCodeType codeType, ZString unLoco, ZString transportMode, BusinessObjectFactory factory)
		{
			var matches = GetMatchesForCodeType(codeType, unLoco, transportMode, factory);
			return matches.Count == 1 ? matches[0].RY_LocalPortCode : ZString.Empty;
		}
	}
}
