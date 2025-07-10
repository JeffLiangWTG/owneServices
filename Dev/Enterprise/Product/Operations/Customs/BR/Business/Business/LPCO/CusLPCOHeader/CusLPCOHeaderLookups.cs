using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public class CusLPCOHeaderLookups : CusPermitHeaderLookups
	{
		public CusLPCOHeaderLookups(CusLPCOHeader parent) : base(parent)
		{
		}

		public CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<BRMessageStatusList>();
	}
}
