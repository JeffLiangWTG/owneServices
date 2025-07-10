using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business
{
	public static class OrgHeaderExtension
	{
		public static ZBool IsInSupervisionArea(this OrgHeader orgHeader)
		{
			var list = EconomicZoneTypeList.SupervisionAreaCodes;
			var code = orgHeader.GetEconomicZoneType(OrgCusCode.CodeTypes.CustomsClientCode);
			return list.Contains(code);
		}

		public static ZString GetEconomicZoneType(this OrgHeader orgHeader, string regNoType)
		{
			var ccdnumber = orgHeader.GetChinaCustomsRegNo(regNoType);
			return ccdnumber.SubstringSafe(4, 1);
		}

		public static ZString GetChinaCustomsRegNo(this OrgHeader orgHeader, string regNoType)
		{
			return orgHeader?.CustomsCodes?.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.China, regNoType) ?? ZString.Empty;
		}

		public static ZString GetUSCI(this OrgHeader orgHeader) => orgHeader.GetChinaCustomsRegNo(OrgCusCode.ChinaCodeTypes.USC);
		public static ZString GetCCD(this OrgHeader orgHeader) => orgHeader.GetChinaCustomsRegNo(OrgCusCode.CodeTypes.CustomsClientCode);
		public static ZString GetCIQ(this OrgHeader orgHeader) => orgHeader.GetChinaCustomsRegNo(OrgCusCode.ChinaCodeTypes.CIQ);
	}
}
