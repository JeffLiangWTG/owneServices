using System.Collections;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BE.Business;

public sealed class CusGoodsLocationLookups : EU.Business.CusGoodsLocationLookups
{
	public CusGoodsLocationLookups(EU.Business.CusGoodsLocation parent) : base(parent)
	{
	}

	public override ICollection UnlocodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, CargoWise.Types.ZDateTime.Today);
}
