using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Module;

public class EntryHeaderFilterLookups : Customs.Module.EntryHeaderFilterLookups
{
	public EntryHeaderFilterLookups(EntryHeaderFilterBusinessObject filterBizObj)
			: base(filterBizObj)
	{
	}

	protected override CodeDescriptionPairList GetMainMessageStatusList() => Factory.GetCachedValue<CHMessageStatusList>();

	public CodeDescriptionPairList PhaseStatusList => Factory.GetCachedValue<PassarDeclarationPhaseList>();

	public CodeDescriptionPairList EComplaintStatusList => Factory.GetCachedValue<EComplaintStatusWithoutNotSentList>();

	public CodeDescriptionPairList SelectionResultList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.SeletionResult, ZDateTime.Today);
}
