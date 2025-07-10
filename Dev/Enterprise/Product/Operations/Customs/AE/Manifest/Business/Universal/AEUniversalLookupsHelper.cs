using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AE.Manifest.Business;

static class AEUniversalLookupsHelper
{
	public static ZString GetUN20CodeCustomsUQ(BusinessObjectFactory factory, ZString cw1Value)
	{
		return GetUN20CodeList(factory)
			.FirstOrDefault(x => x.ZZM_CW1orCommercialValue == cw1Value)?.ZZM_CustomsValue ?? ZString.Empty;
	}

	static RefCusMap[] GetUN20CodeList(BusinessObjectFactory factory)
	{
		return factory.GetCachedValue("AE.Manifest.Business.AEUniversalLookupsHelper.GetUN20CodeList",
			() =>
			{
				var dataGrouping = Core.Constants.CountryCodes.UnitedArabEmirates;
				var query = new ZQuery(RefCusMapSchema.ZZM_ZZZ_NKDataGrouping, dataGrouping);
				query.AddToFilter(RefCusMapSchema.ZZM_ZZP_NKMapType, new[] { RefCusMapTypeList.Codes.MUQCO, RefCusMapTypeList.Codes.VUQCO });
				return factory.Load<RefCusMap>(query);
			});
	}
}
