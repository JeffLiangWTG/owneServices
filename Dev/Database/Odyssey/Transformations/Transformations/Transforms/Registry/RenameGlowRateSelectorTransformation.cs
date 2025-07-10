using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Registry;

public class RenameGlowRateSelectorTransformation : RegistryDataTransformation
{
	public override string UserDescription => "Rename registry from GlowRateSelector to CargoWiseCarrierConnectForJobAutorating";

	protected override void OfflinePostUpgradeTransform()
	{
		UpdateRegistryItemName(cargoWiseCarrierConnectForJobAutoratingOldName, cargoWiseCarrierConnectForJobAutoratingNewName);
	}

	const string cargoWiseCarrierConnectForJobAutoratingOldName = "GlowRateSelector";
	const string cargoWiseCarrierConnectForJobAutoratingNewName = "CargoWiseCarrierConnectForJobAutorating";
}
