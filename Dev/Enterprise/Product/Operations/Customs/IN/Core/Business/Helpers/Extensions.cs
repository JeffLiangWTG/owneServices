using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IN.Business;

public static class Extensions
{
	public static ZString GetAEONumber(this OrgHeader organisation)
	{
		return organisation.GetCustomsRegNo(IndiaOrgCusCodeInfo.OrgCusCodes.AEO, organisation?.MainAddress?.OA_RN_NKCountryCode ?? ZString.Empty);
	}

	public static ZString GetCustomsRegNo(this OrgHeader organisation, ZString code, ZString countryCode)
	{
		return organisation?.CustomsCodes.GetCustomsRegNo(code, countryCode) ?? ZString.Empty;
	}

	public static ZString GetCustomsRegNo(this OrgAddress address, ZString code, ZString countryCode)
	{
		return address?.CustomsCodes.GetCustomsRegNo(code, countryCode) ?? ZString.Empty;
	}
}
