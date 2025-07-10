using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding;

public class RemoveEnableTransportBookingGHGCalculationRegistry : DeleteRegistryItem
{
	protected override string[] GetRegistryItemNames() => new[] { "EnableTransportBookingGHGCalculation" };
}
