using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Freight.Forwarding;

[TestedType(typeof(RemoveEnableTransportBookingGHGCalculationRegistry))]
public class RemoveEnableTransportBookingGHGCalculationRegistryTest : DeleteRegistryItemTest
{
	protected override string[] GetRegistryItemNames() => new[] { "EnableTransportBookingGHGCalculation" };
}
