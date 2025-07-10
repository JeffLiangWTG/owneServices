using Enterprise.Customs.AE.Business;

namespace Enterprise.Customs.AE.Module;

public class JobDeclarationFilterLookups : Customs.Module.JobDeclarationFilterLookups
{
	public JobDeclarationFilterLookups(JobDeclarationFilterStripBusinessObject filterBizObj)
		: base(filterBizObj)
	{
	}

	public override ZArchitecture.Core.CodeDescriptionPairList MessageSubTypeList() => new();

	public override ZArchitecture.Core.CodeDescriptionPairList PaymentPartyList()
	{
		return new PaymentByList();
	}
}
