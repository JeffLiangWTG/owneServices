using System.Threading;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Workflow;

public class RemoveProcessHeaderWithG2ParentTableCode : DataTransformation
{
	public override string UserDescription => "Remove ProcessHeader with 'G2' ParentTableCode";

	protected override void OnlinePostUpgradeTransform(CancellationToken token)
	{
		// WI00890102: method body removed to un-stuck ediProd upgrade
	}
}
