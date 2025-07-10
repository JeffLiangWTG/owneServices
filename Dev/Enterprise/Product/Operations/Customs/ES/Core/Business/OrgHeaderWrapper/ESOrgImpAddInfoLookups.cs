using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class ESOrgImpAddInfoLookups : AutoESOrgImpAddInfoLookups
	{
		public ESOrgImpAddInfoLookups(AutoESOrgImpAddInfo parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList MethodOfPaymentList => Factory.GetCachedValue<MethodOfPaymentList>();
	}
}
