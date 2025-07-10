using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.StabilityChecker.Testing
{
	[TestedType(typeof(StabilityResultsRegistryItem))]
	sealed class StabilityResultsRegistryItemTestCase : StronglyTypedRegistryItemTestCase<StabilityResults>
	{
		protected override StronglyTypedRegistryItem<StabilityResults, StabilityResults> GetNewRegistryItem()
		{
			return new StabilityResultsRegistryItem("StabilityCheckerResult");
		}
	}
}
