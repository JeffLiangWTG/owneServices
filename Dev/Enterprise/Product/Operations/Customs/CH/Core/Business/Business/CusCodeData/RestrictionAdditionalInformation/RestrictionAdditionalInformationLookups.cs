using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class RestrictionAdditionalInformationLookups : CusCodeDataLookups
{
	public RestrictionAdditionalInformationLookups(RestrictionAdditionalInformation restrictionAdditionalInformation) : base(restrictionAdditionalInformation)
	{
	}

	public override CodeDescriptionPairList CY_CodeList
	{
		get
		{
			if (Restriction != null)
			{
				var restrictionCode = Restriction.CSI_Code;
				var codeList = new CodeDescriptionPairList();
				codeList.AddRange(RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.PassarTypes.N1119, Parent.EffectiveAssessmentDate, false, UniversalReferenceConstants.RefCusCodeList.Attributes.RestrictionCode, new ZString[] { restrictionCode }));
				if (Restriction.CSI_ReferenceNumber.IsEmpty && !IgnoreRemoveCodeB1001(restrictionCode))
				{
					codeList.RemoveCode(UniversalReferenceConstants.RestrictionAdditionalInformationCode.AuthorizationItemNumber);
				}
				return codeList;
			}
			else
			{
				return new CodeDescriptionPairList();
			}
		}
	}

	bool IgnoreRemoveCodeB1001(string code)
	{
		switch (code)
		{
			case UniversalReferenceConstants.RestrictionCode.FOEN_411:
			case UniversalReferenceConstants.RestrictionCode.FOEN_412:
			case UniversalReferenceConstants.RestrictionCode.FOEN_413:
			case UniversalReferenceConstants.RestrictionCode.FOEN_414:
				return true;
			default:
				return false;
		}
	}

	public ZZRefCusCodeListCombinedCollection DataList
	{
		get
		{
			var linkedCodeType = Parent.LinkedCodeType;
			if (linkedCodeType.IsEmpty)
			{
				return new ZZRefCusCodeListCombinedCollection(Factory);
			}
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Switzerland, linkedCodeType, Parent.EffectiveAssessmentDate);
		}
	}

	protected new RestrictionAdditionalInformation Parent => (RestrictionAdditionalInformation)base.Parent;

	Restriction Restriction => Parent.Parent;
}
