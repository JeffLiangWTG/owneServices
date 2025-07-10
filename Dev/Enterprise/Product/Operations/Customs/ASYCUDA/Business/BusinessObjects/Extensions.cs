using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business;

public static class Extensions
{
	public static bool IsSupportedCountries(this ZString countryCode, BusinessObjectFactory factory)
	{
		var countryCodes = ZZDatabaseValidationHelper.GetNVCApplicationBusinessProviders(factory)
			.Union(ZZDatabaseValidationHelper.GetVOCApplicationBusinessProviders(factory))
			.Union(ZZDatabaseValidationHelper.GetBCDApplicationBusinessProviders(factory))
			.Union(ZZDatabaseValidationHelper.GetLVCApplicationBusinessProviders(factory))
			.SelectMany(x => x.CountryCodes);
		var result = countryCodes.Contains(countryCode);
		return result;
	}

	public static ZString ToProperCase(this ZString value)
	{
		var lower = value.ToLower();
		var firstletter = value.SubstringSafe(0, 1).ToUpperInvariant();
		return firstletter + lower.SubstringSafe(1);
	}

	public static ZString GetExstingIMPaymentMethod(this OrgHeader orgHeader)
	{
		var query = new ZQuery(OrgCompanyDataSchema.OB_OH, orgHeader.PK);
		query.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
		query.FetchOnlyFromLocalCache = false;
		query.ReLoadExistingRows = true;
		var companyData = orgHeader.Factory.LoadTop1<OrgCompanyData>(query);
		return companyData?.OB_AREftCustomsPaymentMethod ?? ZString.Empty;
	}
}
