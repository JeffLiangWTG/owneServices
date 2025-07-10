using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ArchiveManager
{
	class ResetBiIsRequiredDeleteOrphanSubscriberRegistryItem : RegistryDataTransformation
	{
		public override string UserDescription
			=> "Reset default value for registry item BiIsRequiredDeleteOrphanSubscriber";

		protected override void OfflinePostUpgradeTransform()
			=> DeleteRegistryItemRows("BiIsRequiredDeleteOrphanSubscriber");
	}
}
