using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public class CusGoodsCatalogLookups : Customs.Business.CusGoodsCatalogLookups
	{
		public CusGoodsCatalogLookups(AutoCusGoodsCatalog parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList StatusTypeList => Factory.GetCachedValue<GoodsCatalogStatusTypeList>();

		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<BRMessageStatusList>();
	}
}
