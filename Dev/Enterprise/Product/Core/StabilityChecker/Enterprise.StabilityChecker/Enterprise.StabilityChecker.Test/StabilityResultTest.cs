using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.StabilityChecker.Testing
{
	[TestedType(typeof(StabilityResult))]
	sealed class StabilityResultTest : RegistryBusinessObjectTemplateTestCase<StabilityResult>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new StabilityResult(StabilityResultLevel.Critical, "dead");
		}

		protected override StabilityResult GetBusinessObjectToClone()
		{
			return new StabilityResult(StabilityResultLevel.Critical, "dead");
		}

		protected override StabilityResult GetBusinessObjectToSerialise()
		{
			return new StabilityResult(StabilityResultLevel.Critical, "dead");
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
