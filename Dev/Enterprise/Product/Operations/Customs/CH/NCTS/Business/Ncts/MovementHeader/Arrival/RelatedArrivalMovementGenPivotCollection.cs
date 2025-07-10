using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class RelatedArrivalMovementGenPivotCollection : CustomsGenPivotCollection<RelatedArrivalMovementGenPivot, NctsArrivalMovementHeader, NctsArrivalMovementHeader>
{
	public RelatedArrivalMovementGenPivotCollection(NctsArrivalMovementHeader master) : base(master)
	{
	}

	protected override string RelationType => GenPivotTypeDecider.Types.NctsRelatedArrivalGenPivot;
}
