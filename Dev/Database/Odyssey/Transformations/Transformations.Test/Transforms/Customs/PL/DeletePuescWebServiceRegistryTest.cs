using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.PL.Testing;

[TestedType(typeof(DeletePuescWebServiceRegistry))]
sealed class DeletePuescWebServiceRegistryTest : DeleteRegistryItemTest
{
	protected override string[] GetRegistryItemNames() => new[] { "PUESCWebService" };
}
