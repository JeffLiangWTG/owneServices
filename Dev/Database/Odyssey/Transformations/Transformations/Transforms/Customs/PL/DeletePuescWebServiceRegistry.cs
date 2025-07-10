using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.PL;

sealed class DeletePuescWebServiceRegistry : DeleteRegistryItem
{
	protected override string[] GetRegistryItemNames() => new[] { "PUESCWebService" };
}
