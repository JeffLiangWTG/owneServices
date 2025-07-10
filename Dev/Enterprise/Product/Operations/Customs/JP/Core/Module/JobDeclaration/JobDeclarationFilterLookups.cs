using Enterprise.Customs.JP.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Module
{
	public class JobDeclarationFilterLookups : Customs.Module.JobDeclarationFilterLookups
	{
		public JobDeclarationFilterLookups(JobDeclarationFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		public override CodeDescriptionPairList MessageStatusList() => Factory.GetCachedValue<JPMessageStatusList>();
	}
}
