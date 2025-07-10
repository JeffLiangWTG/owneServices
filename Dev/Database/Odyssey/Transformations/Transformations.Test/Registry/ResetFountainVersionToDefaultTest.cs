using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Registry.Testing
{
	[TestedType(typeof(ResetFountainVersionToDefault))]
	class ResetFountainVersionToDefaultTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { "NumberFountainVersion" };
		}
	}
}
