using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IE.Business
{
	public class CusGoodsLocationLookups : EU.Business.CusGoodsLocationLookups
	{
		public CusGoodsLocationLookups(EU.Business.CusGoodsLocation parent) : base(parent)
		{
		}

		string DataGroupingCode => Parent is CusGoodsLocation goodsLocation && goodsLocation.UCCVersionProvider is ICusGoodsLocationProviderWithUCCVersion provider && provider.IsUCC5 ? Core.Constants.Customs.Universal.RefDataGrouping.Codes.IEUcc5 : Core.Constants.CountryCodes.Ireland;

		public override ICollection UnlocodeList => CustomsOfficeList;

		public override ZZRefCusCodeListCombinedCollection CustomsOfficeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, DataGroupingCode, UniversalReferenceConstants.RefCusCodeListTypes.Codes.GoodsLocation, ZDateTime.Today);
	}
}
