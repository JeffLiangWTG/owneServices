using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class InwardProcessingProductLookups : CusSupportingInfoLookups
	{
		public InwardProcessingProductLookups(InwardProcessingProduct parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList SubTypeList => Factory.GetCachedValue<YieldTypeList>();
	}
}
