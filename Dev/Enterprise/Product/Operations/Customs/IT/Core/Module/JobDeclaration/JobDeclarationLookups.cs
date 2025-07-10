using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Module;

sealed class JobDeclarationFilterLookups : EU.Module.JobDeclarationFilterLookups
{
	public JobDeclarationFilterLookups(JobDeclarationFilterBusinessObject filterBizObj)
		: base(filterBizObj)
	{
	}

	public CodeDescriptionPairList MessageVersionList => Factory.GetCachedValue<MessageVersionList>();
}
