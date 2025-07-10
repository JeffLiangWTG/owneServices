using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.StabilityChecker.Testing
{
	[TestedType(typeof(StabilityResults))]
	sealed class StabilityResultsTest : RegistryBusinessObjectTemplateTestCase<StabilityResults>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new StabilityResults();
		}

		protected override StabilityResults GetBusinessObjectToClone()
		{
			StabilityResults results = new StabilityResults();
			results.Results.Add(new StabilityResult(StabilityResultLevel.Critical, "dead"));
			return results;
		}

		protected override StabilityResults GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
