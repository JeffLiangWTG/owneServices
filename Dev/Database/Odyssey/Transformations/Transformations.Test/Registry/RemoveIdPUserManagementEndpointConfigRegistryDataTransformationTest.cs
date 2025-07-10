using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Registry
{
	[TestedType(typeof(RemoveIdPUserManagementEndpointConfigRegistryDataTransformation))]
	public class RemoveIdPUserManagementEndpointConfigRegistryDataTransformationTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { "IdPUserManagementTimeoutInSeconds" };
		}
	}
}
