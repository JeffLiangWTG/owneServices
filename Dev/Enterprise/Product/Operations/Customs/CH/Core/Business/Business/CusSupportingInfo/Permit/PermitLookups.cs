using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CH.Business;

public class PermitLookups : Customs.Business.CusSupportingInfoLookups
{
	public PermitLookups(Permit parent) : base(parent)
	{
	}

	public ZZRefCusCodeListCombinedCollection PermitAuthorityCodeList =>
		ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory,
			Core.Constants.CountryCodes.Switzerland,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitAuthority,
			Parent.Parent?.EffectiveAssessmentDate ?? ZDateTime.Today);

	public ZZRefCusCodeListCombinedCollection PermitTypeCodeList =>
		ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory,
			Core.Constants.CountryCodes.Switzerland,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitType,
			Parent.Parent?.EffectiveAssessmentDate ?? ZDateTime.Today);

	protected new Permit Parent => (Permit)base.Parent;
}
