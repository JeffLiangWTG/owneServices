using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Module;

public class JobDeclarationFilterLookups : Customs.Module.JobDeclarationFilterLookups
{
	public JobDeclarationFilterLookups(JobDeclarationFilterBusinessObject filterBizObj)
		: base(filterBizObj)
	{
	}

	public override CodeDescriptionPairList EntryStatusList() => CommonLookups.CustomsStatusList(Factory);

	protected override CodeDescriptionPairList GetMainMessageStatusList() => Factory.GetCachedValue<CHMessageStatusList>();

	public CodeDescriptionPairList PhaseStatusList => Factory.GetCachedValue<PassarDeclarationPhaseList>();

	public CodeDescriptionPairList SelectionResultList() => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.SeletionResult, ZDateTime.Today);
}
