using System.Threading;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.ZA;

public class MoveJobComInvoiceLineVehicleAddInfosToCusVehicle : DataTransformation
{
	public override string UserDescription => "Move ZA JI vehicle AddInfos to CusVehicle";

	protected override void OnlinePostUpgradeTransform(CancellationToken token)
	{
		// Intend to remove logic from this transformation (moved to MoveJobComInvoiceLineVehicleAddInfosToCusVehicle2), but keep this override to prevent failure of ODT service task
	}
}
