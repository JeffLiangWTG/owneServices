using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public partial class CusEntryHeaderLookups : Customs.Business.CusEntryHeaderLookups
{
	public CusEntryHeaderLookups(CusEntryHeader parent)
		: base(parent)
	{
	}

	public CodeDescriptionPairList SelectionResultList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.SeletionResult, ZDateTime.Today);

	public CodeDescriptionPairList PhaseStatusList => Factory.GetCachedValue<PassarDeclarationPhaseList>();

	public CodeDescriptionPairList EComplaintStatusList => Factory.GetCachedValue<EComplaintStatusList>();
}
