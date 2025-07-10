using Enterprise.Customs.Common.KR;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Module
{
	public class JobDeclarationFilterLookups : Customs.Module.JobDeclarationFilterLookups
	{
		public JobDeclarationFilterLookups(JobDeclarationFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		public override CodeDescriptionPairList MessageTypeList => Factory.GetCachedValue<KRJobMessageTypeList>();
	}
}
