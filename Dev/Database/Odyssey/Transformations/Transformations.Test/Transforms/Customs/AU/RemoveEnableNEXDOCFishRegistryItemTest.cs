using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.AU;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.AU
{
	[TestedType(typeof(RemoveEnableNEXDOCFishRegistryItem))]
	sealed class RemoveEnableNEXDOCFishRegistryItemTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { "EnableNEXDOCFish", "EnableNEXDOCStaging" };
		}
	}
}
