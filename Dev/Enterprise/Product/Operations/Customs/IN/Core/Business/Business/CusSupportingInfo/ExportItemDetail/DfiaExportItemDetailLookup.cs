using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IN.Business;

public sealed class DfiaExportItemDetailLookup : CusSupportingInfoLookups
{
	public DfiaExportItemDetailLookup(DfiaExportItemDetail parent) : base(parent)
	{
	}

	new DfiaExportItemDetail Parent => (DfiaExportItemDetail)base.Parent;

	public override CodeDescriptionPairList UnitOfQuantityList
		=> UniversalReferenceDataHelper.GetCustomsUnitOfQuantityList(Factory, Parent.Parent?.EffectiveAssessmentDate ?? ZDateTime.Today);
}
