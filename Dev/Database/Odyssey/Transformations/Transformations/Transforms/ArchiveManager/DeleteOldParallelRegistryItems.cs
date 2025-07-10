using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ArchiveManager
{
	class DeleteOldParallelRegistryItems : RegistryDataTransformation
	{
		public override string UserDescription => "Delete redundant archive manager parallel registry items";

		protected override void OfflinePostUpgradeTransform()
		{
			DeleteRegistryItemRows("EnableParallelDocumentGeneration");
			DeleteRegistryItemRows("EnableParallelArchiveSetProcessing");
		}
	}
}
