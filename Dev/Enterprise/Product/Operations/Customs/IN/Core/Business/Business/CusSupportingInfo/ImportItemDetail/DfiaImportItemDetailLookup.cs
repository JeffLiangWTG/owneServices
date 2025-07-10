using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IN.Business;

public sealed class DfiaImportItemDetailLookup : CusSupportingInfoLookups
{
	public DfiaImportItemDetailLookup(DfiaImportItemDetail parent) : base(parent)
	{
	}

	new DfiaImportItemDetail Parent => (DfiaImportItemDetail)base.Parent;

	public override CodeDescriptionPairList UnitOfQuantityList
		=> UniversalReferenceDataHelper.GetCustomsUnitOfQuantityList(Factory, Parent.Parent?.Parent?.EffectiveAssessmentDate ?? ZDateTime.Today);

	public override CodeDescriptionPairList IssuerTypeList => Factory.GetCachedValue<DfiaImportItemDetailsTypeList>();
}
