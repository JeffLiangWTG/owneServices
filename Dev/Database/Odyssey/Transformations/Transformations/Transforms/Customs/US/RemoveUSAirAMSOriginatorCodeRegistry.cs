using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.US
{
	public class RemoveUSAirAMSOriginatorCodeRegistry : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames() => new[] { "USAirAMSOriginatorCode" };
	}
}
