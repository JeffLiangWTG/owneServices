using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	static class IB3HeaderHelper
	{
		internal static ZString GetCustomsRegNo(OrgHeader orgHeader, params ZString[] codeTypes)
		{
			var result = ZString.Empty;
			if (orgHeader != null)
			{
				var orgCusCode = orgHeader.CustomsCodes.GetOrgCusCodeObjectMatchingCountryAndCodes(Core.Constants.CountryCodes.Canada, codeTypes);
				if (orgCusCode != null)
				{
					result = orgCusCode.OK_CustomsRegNo;
				}
			}
			return result;
		}

		internal static ZString GetBusinessNumber(OrgHeader orgHeader)
		{
			return GetCustomsRegNo(orgHeader, OrgCusCode.CACodeTypes.BusinessNumberForImportExport, OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial);
		}

		internal static ZString GetLVSBusinessNumber(OrgHeader orgHeader)
		{
			return GetCustomsRegNo(orgHeader, OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments, OrgCusCode.CACodeTypes.BusinessNumberForImportExport);
		}

		internal static ZString GetBusinessNumber(bool isCasual, params OrgHeader[] orgHeaders)
		{
			var result = ZString.Empty;
			foreach (var orgHeader in orgHeaders)
			{
				if (result.IsEmpty)
				{
					result = GetCustomsRegNo(orgHeader,
						isCasual ? OrgCusCode.CACodeTypes.BusinessNumberImporterNonCommercial : OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial,
						OrgCusCode.CACodeTypes.BusinessNumberForImportExport);
				}
			}
			return result;
		}

		internal static ZString GetBrokerBusinessNumber(OrgHeader orgHeader)
		{
			return GetCustomsRegNo(orgHeader, OrgCusCode.CACodeTypes.BusinessNumberCustomsBroker, OrgCusCode.CACodeTypes.BusinessNumberForImportExport);
		}

		internal static ZString GetGSTNumber(OrgHeader orgHeader)
		{
			var result = GetCustomsRegNo(orgHeader, OrgCusCode.CACodeTypes.BusinessNumberForGoodsServicesHarmonizedSalesTax);
			if (result.Length != 10)
			{
				result = ZString.Empty;
			}
			return result;
		}
	}
}
