using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class Extension
	{
		public static ZString GetCustomsClientID(this OrgHeader header, OrgAddress address = null)
		{
			var configCodes = header.CustomsCodes.GetOrgCusCodesForCodeAndCountry(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia);
			var codeWithNoAddress = ZString.Empty;
			foreach (var configCode in configCodes)
			{
				var premisesAddress = configCode.PremisesAddress;
				if (premisesAddress == null)
				{
					if (codeWithNoAddress.IsEmpty)
					{
						codeWithNoAddress = configCode.OK_CustomsRegNo;
					}
				}
				else if (address != null && address.PK == premisesAddress.PK)
				{
					return configCode.OK_CustomsRegNo;
				}
			}
			return codeWithNoAddress;
		}

		public static bool HasCCIDWithMatchedAddress(this OrgHeader header, ZGuid addressPK)
		{
			var configCodes = header.CustomsCodes.GetOrgCusCodesForCodeAndCountry(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia);
			return configCodes.Any(x => x.PremisesAddress == null || (addressPK.IsValid && x.PremisesAddress.PK == addressPK));
		}
	}
}
