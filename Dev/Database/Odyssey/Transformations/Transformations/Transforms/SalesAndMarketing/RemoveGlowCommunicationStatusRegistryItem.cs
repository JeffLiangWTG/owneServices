using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.SalesAndMarketing
{
	class RemoveGlowCommunicationStatusRegistryItem : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames()
		{
			return ["GlowCommunicationStatus"];
		}
	}
}
