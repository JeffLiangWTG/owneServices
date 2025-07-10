using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.CH;

namespace Enterprise.Customs.CH.NCTS.Business;

public class RelatedArrivalMovementGenPivot : CustomsGenPivot, INctsRelatedArrivalGenPivot
{
	public RelatedArrivalMovementGenPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		XX_RelationType = GenPivotTypeDecider.Types.NctsRelatedArrivalGenPivot;
		XX_Relation1TableCode = CusInBondMoveHeaderSchema.Constants.Prefix;
		XX_Relation2TableCode = CusInBondMoveHeaderSchema.Constants.Prefix;
	}

	public NctsArrivalMovementHeader ChildMovement => (NctsArrivalMovementHeader)Relation2Object;

	public override ZGuid XX_Relation1ID
	{
		get => base.XX_Relation1ID;
		set
		{
			base.XX_Relation1ID = value;

			if (Relation1Object is NctsArrivalMovementHeader parentMovement && !parentMovement.MultipleMRNIndicator)
			{
				ErrorReporter.ReportOnce("RelatedArrivalMovementGenPivot.XX_Relation1ID.MultipleMRNIndicator", "It's not possible to set parent movement with MultipleMRNIndicator=FALSE");
			}
		}
	}

	public override ZGuid XX_Relation2ID
	{
		get => base.XX_Relation2ID;
		set
		{
			base.XX_Relation2ID = value;

			if (Relation2Object is NctsArrivalMovementHeader childMovement && childMovement.MultipleMRNIndicator)
			{
				ErrorReporter.ReportOnce("RelatedArrivalMovementGenPivot.XX_Relation2ID.MultipleMRNIndicator", "It's not possible to add a child movement with MultipleMRNIndicator=TRUE");
			}
		}
	}
}
