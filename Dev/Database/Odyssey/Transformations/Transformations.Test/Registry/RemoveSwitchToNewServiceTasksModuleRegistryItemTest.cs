using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Registry
{
	[TestedType(typeof(RemoveSwitchToNewServiceTasksModuleRegistryItem))]
	class RemoveSwitchToNewServiceTasksModuleRegistryItemTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { "SwitchToNewServiceTasksModule" };
		}
	}
}
