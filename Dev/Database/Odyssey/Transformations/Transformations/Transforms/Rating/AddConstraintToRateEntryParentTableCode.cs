using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Rating
{
	public class AddConstraintToRateEntryParentTableCode : DataTransformation
	{
		public override string UserDescription => "Cleanup data for new constraint on column TI_ParentTableCode";

		// Intend to remove logic from this transformation (moved to AddConstraintToRateEntryParentTableCode_Online), but keep this override to prevent failure of ODT service task
	}
}
