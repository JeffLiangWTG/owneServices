using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Registry
{
	sealed class RemoveSwitchToNewServiceTasksModuleRegistryItem : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { "SwitchToNewServiceTasksModule" };
		}
	}
}
