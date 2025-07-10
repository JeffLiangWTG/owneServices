using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.SalesAndMarketing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.SalesAndMarketing.Testing
{
	[TestedType(typeof(RemoveGlowCommunicationStatusRegistryItem))]
	class RemoveGlowCommunicationStatusRegistryItemTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames()
		{
			return ["GlowCommunicationStatus"];
		}
	}
}
