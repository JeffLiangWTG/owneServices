using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.US;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.US
{
	[TestedType(typeof(RemoveUSAirAMSOriginatorCodeRegistry))]
	class RemoveUSAirAMSOriginatorCodeRegistryTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames() => new[] { "USAirAMSOriginatorCode" };
	}
}
