using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Registry
{
	public class RenameGlowUseIndexingServiceForSearchRegistryItem : RegistryDataTransformation
	{
		public override string UserDescription => "Rename registry from GlowUseIndexingServiceForSearch to GlowUseIndexingServiceForGlobalSearch";

		protected override void OfflinePostUpgradeTransform()
		{
			UpdateRegistryItemName(registryItemOldName, registryItemNewName);
		}

		const string registryItemOldName = "GlowUseIndexingServiceForSearch";
		const string registryItemNewName = "GlowUseIndexingServiceForGlobalSearch";
	}
}
