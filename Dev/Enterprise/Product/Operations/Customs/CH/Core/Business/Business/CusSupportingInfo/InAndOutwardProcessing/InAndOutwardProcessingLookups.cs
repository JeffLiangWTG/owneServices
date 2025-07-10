using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public sealed class InAndOutwardProcessingLookups : CusSupportingInfoLookups
{
	public InAndOutwardProcessingLookups(AutoCusSupportingInfo parent) : base(parent)
	{
	}

	new InAndOutwardProcessing Parent => (InAndOutwardProcessing)base.Parent;

	public CodeDescriptionPairList DirectionList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.Direction, Parent.EffectiveAssessmentDate);

	public CodeDescriptionPairList RefinementTypeList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.RefinementType, Parent.EffectiveAssessmentDate);

	public CodeDescriptionPairList ProcessTypeList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.ProcessType, Parent.EffectiveAssessmentDate);

	public CodeDescriptionPairList BillingTypeList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.BillingType, Parent.EffectiveAssessmentDate);

	public ZZRefCusCodeListCombinedCollection NotifyCustomsOfficeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.CustomsOffice, Parent.EffectiveAssessmentDate);
}
