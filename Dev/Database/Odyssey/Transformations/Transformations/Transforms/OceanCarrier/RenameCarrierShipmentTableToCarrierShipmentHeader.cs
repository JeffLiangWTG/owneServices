using System.Collections.Generic;
using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.OceanCarrier;

class RenameCarrierShipmentTableToCarrierShipmentHeader : RenameTableTransformation
{
	public override string UserDescription => "Rename table CarrierShipment to CarrierShipmentHeader";

	protected override IEnumerable<IRenameTableTransformationInfo> RenameTableInfoList => new[] { new RenameTableTransformationInfo("CarrierShipment", "CarrierShipmentHeader") };
}
