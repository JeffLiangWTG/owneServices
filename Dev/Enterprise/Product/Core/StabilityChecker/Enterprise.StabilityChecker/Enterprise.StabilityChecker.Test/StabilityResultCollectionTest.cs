using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.StabilityChecker.Testing
{
	[TestedType(typeof(StabilityResultCollection))]
	sealed class StabilityResultCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<StabilityResultCollection>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override StabilityResultCollection GetCollectionToTest()
		{
			return new StabilityResultCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new StabilityResult(StabilityResultLevel.Critical, "dead " + (itemsAdded++));
		}

		int itemsAdded;
	}
}
