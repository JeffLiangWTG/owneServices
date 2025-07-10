using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.BusinessIntelligence
{
	public class DeleteBiUseCDCHotfixRegistry : RegistryDataTransformation
	{
		public override string UserDescription => "Remove BiUseCDCHotfix registry item";

		protected override void OfflinePostUpgradeTransform()
		{
			DeleteRegistryItemRows("BiUseCDCHotfix");
		}
	}
}
