using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.SalesAndMarketing
{
	class RemoveEmbeddedImagesRegistryItems : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames()
		{
			return ["AutoEmbedImageSizeMb", "EmbeddedImageSizeMb", "EmbeddedSizeForAllImages"];
		}
	}
}
