using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business;

public class TobaccoLookups : Customs.Business.CusSupportingInfoLookups
{
	public TobaccoLookups(Tobacco parent) : base(parent) { }

	protected new Tobacco Parent => (Tobacco)base.Parent;

	public override ICollection CodeList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.TobaccoMainGroup, Parent.Parent?.EffectiveAssessmentDate ?? ZDateTime.Today);

	public override CodeDescriptionPairList SubTypeList
	{
		get
		{
			ZString codeType = ZString.Empty;
			switch (Parent.CSI_Code)
			{
				case TobaccoMainGroupCodes.Cigars:
					codeType = UniversalReferenceConstants.RefCusCodeList.EdecTypes.TobaccoSubGroupCigars;
					break;
				case TobaccoMainGroupCodes.Cigarettes:
					codeType = UniversalReferenceConstants.RefCusCodeList.EdecTypes.TobaccoSubGroupCigarettes;
					break;
				case TobaccoMainGroupCodes.CutTobacco:
					codeType = UniversalReferenceConstants.RefCusCodeList.EdecTypes.TobaccoSubGroupTobacco;
					break;
				case TobaccoMainGroupCodes.Assortment:
					codeType = UniversalReferenceConstants.RefCusCodeList.EdecTypes.TobaccoSubGroupAssortment;
					break;
				case TobaccoMainGroupCodes.ECigarettes:
					codeType = UniversalReferenceConstants.RefCusCodeList.EdecTypes.TobaccoSubGroupECigarettes;
					break;
			}
			return codeType.IsEmpty ? new CodeDescriptionPairList() : RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, codeType, Parent.Parent?.EffectiveAssessmentDate ?? ZDateTime.Today);
		}
	}

	public CodeDescriptionPairList AdditionalDescriptionList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.TobaccoBrand, Parent.Parent?.EffectiveAssessmentDate ?? ZDateTime.Today);

	public CodeDescriptionPairList SpecialUnitOfMeasureList => Factory.GetCachedValue<TobaccoSpecialUnitOfMeasureList>();
}
