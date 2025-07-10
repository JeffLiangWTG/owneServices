using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.SalesAndMarketing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.SalesAndMarketing.Testing
{
	[TestedType(typeof(RemoveEmbeddedImagesRegistryItems))]
	class RemoveEmbeddedImagesRegistryItemsTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames()
		{
			return ["AutoEmbedImageSizeMb", "EmbeddedImageSizeMb", "EmbeddedSizeForAllImages"];
		}
	}
}
