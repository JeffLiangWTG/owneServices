using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class RelatedExportEntryHeaderGenPivotCollection : CustomsGenPivotCollection<RelatedExportEntryHeaderGenPivot, NctsDepartureMovementHeader, CusEntryHeader>
{
	public RelatedExportEntryHeaderGenPivotCollection(NctsDepartureMovementHeader master) : base(master)
	{
	}

	protected override string RelationType => GenPivotTypeDecider.Types.NctsRelatedExportGenPivot;
}
