using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.StabilityChecker.Testing
{
	[TestedType(typeof(StabilityCheckerRegistry))]
	sealed class StabilityCheckerRegistryTestCase : RegistryItemSetTestCase<StabilityCheckerRegistry>
	{
	}
}
