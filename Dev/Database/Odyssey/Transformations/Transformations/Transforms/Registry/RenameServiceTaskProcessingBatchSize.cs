using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Registry
{
	public class RenameServiceTaskProcessingBatchSize : RegistryDataTransformation
	{
		public override string UserDescription => "Rename registry from ServiceTaskProcessingBatchSize to ServiceTaskProcessingMaximumBatchSize";

		protected override void OfflinePostUpgradeTransform()
		{
			UpdateRegistryItemName(registryItemOldName, registryItemNewName);
		}

		const string registryItemOldName = "ServiceTaskProcessingBatchSize";
		const string registryItemNewName = "ServiceTaskProcessingMaximumBatchSize";
	}
}
