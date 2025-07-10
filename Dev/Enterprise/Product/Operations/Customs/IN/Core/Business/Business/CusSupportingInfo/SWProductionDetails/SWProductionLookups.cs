using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IN.Business;

public class SWProductionLookups : CusSupportingInfoLookups
{
	public SWProductionLookups(AutoCusSupportingInfo parent) : base(parent)
	{
	}

	public new SWProduction Parent => (SWProduction)base.Parent;

	public override CodeDescriptionPairList UnitOfQuantityList
		=> UniversalReferenceDataHelper.GetCustomsUnitOfQuantityList(Factory, Parent.Parent?.EffectiveAssessmentDate ?? ZDateTime.Today);
}

