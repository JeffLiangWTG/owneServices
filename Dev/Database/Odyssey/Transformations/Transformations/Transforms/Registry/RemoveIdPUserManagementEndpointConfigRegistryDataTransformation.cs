using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Registry
{
	sealed class RemoveIdPUserManagementEndpointConfigRegistryDataTransformation : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { "IdPUserManagementTimeoutInSeconds" };
		}
	}
}
