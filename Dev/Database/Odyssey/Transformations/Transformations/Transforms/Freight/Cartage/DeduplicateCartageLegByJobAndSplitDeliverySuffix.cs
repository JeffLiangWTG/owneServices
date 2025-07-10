using System.Threading;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Cartage
{
	sealed class DeduplicateCartageLegByJobAndSplitDeliverySuffix : DataTransformation
	{
		public override string UserDescription => "De-duplicate JU_SplitDeliverySuffix on JobContainerLegs belonging to the same JobCartage.JJ_PK (via JobBookedCtgMove.EW_PK) [obsolete]";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			base.OnlinePostUpgradeTransform(token);
		}
	}
}
