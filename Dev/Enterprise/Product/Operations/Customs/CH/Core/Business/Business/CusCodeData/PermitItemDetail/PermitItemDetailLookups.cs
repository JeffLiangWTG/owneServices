using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class PermitItemDetailLookups : CusCodeDataLookups
{
	public PermitItemDetailLookups(PermitItemDetail permitDetails) : base(permitDetails)
	{
	}

	public override CodeDescriptionPairList CY_CodeList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.PermitItemDetailsKey, Parent.EffectiveAssessmentDate);

	public ZZRefCusCodeListCombinedCollection CITESCommodityTypeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.CITESCommodityType, Parent.EffectiveAssessmentDate);

	public ZZRefCusCodeListCombinedCollection CITESScientificNameList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.CITESScientificName, Parent.EffectiveAssessmentDate);

	public ZZRefCusCodeListCombinedCollection CITESCodeList
	{
		get
		{
			switch (Parent.CY_Code)
			{
				case PermitItemDetailKeyList.Codes.Key3:
					return CITESCommodityTypeList;
				case PermitItemDetailKeyList.Codes.Key4:
					return CITESScientificNameList;
				default:
					return null;
			}
		}
	}

	protected new PermitItemDetail Parent => (PermitItemDetail)base.Parent;
}
