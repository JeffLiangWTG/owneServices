using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CH.Business;

public class NonCustomsLawLookups : Customs.Business.CusSupportingInfoLookups
{
	public NonCustomsLawLookups(NonCustomsLaw parent) : base(parent) { }

	protected new NonCustomsLaw Parent => (NonCustomsLaw)base.Parent;

	public override ICollection CodeList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NonCustomsLaw, Parent.Parent?.EffectiveAssessmentDate ?? ZDateTime.Today);
}
