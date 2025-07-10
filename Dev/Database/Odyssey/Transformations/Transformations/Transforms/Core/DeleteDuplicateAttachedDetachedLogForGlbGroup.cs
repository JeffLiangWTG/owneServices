using System.Threading;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core;

public class DeleteDuplicateAttachedDetachedLogForGlbGroup : DataTransformation
{
	public override string UserDescription => "Empty Transformation";

	protected override void OnlinePostUpgradeTransform(CancellationToken token)
	{
		// This Transformation cannot be completed on EDIProd and we have decided to remove it.
		//
		// 	However, if a removed transformation is stuck in a pending state, UPG service task will fail,
		//
		// 	Thus, it is now an empty transformation: to allow any old pending transformation to complete (if such exists).
	}
}