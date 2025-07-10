using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using CHRefCusCodeList = Enterprise.Customs.CH.Business.UniversalReferenceConstants.RefCusCodeList;
using CountryCodes = Enterprise.Core.Constants.CountryCodes;

namespace Enterprise.Customs.CH.Business;

public class RestrictionLookups : CusSupportingInfoLookups
{
	public RestrictionLookups(Restriction parent) : base(parent)
	{
	}

	public override ICollection CodeList => RefCusCodeListTypes.GetCachedList(Factory, CountryCodes.Switzerland, CHRefCusCodeList.PassarTypes.Restrictions, EffectiveAssessmentDate);

	public OrganisationsFindBoxCollection PermitOwnerList => new OrganisationsFindBoxCollection(Factory);

	public CodeDescriptionPairList ExceptionReasonList => RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory, CountryCodes.Switzerland,
		CHRefCusCodeList.PassarTypes.N1121, EffectiveAssessmentDate, false, CHRefCusCodeList.Attributes.PermitAuthority, new[] { Parent.CSI_Code });

	new Restriction Parent => (Restriction)base.Parent;

	ZDateTime EffectiveAssessmentDate => Parent.Parent?.EffectiveAssessmentDate ?? ZDateTime.Today;
}
