using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Integration.Testing
{
	public class SupportExRateSourceAttributeTest : TestCaseWithFactory
	{
		public void TestSupportedExRateSources()
		{
			ExRateSourceType[] expectedVoyage = new ExRateSourceType[] { ExRateSourceType.Voyage };
			ExRateSourceType[] expectedEmpty = System.Array.Empty<ExRateSourceType>();
			AssertContainsExactElementsInAnyOrder("Should return ExRateSourceType.Voyage", expectedVoyage, SupportExRateSourceAttribute.SupportedExRateSources(typeof(DummyClassWithAttribute)));
			AssertContainsExactElementsInAnyOrder("Should return ExRateSourceType.Voyage", expectedVoyage, SupportExRateSourceAttribute.SupportedExRateSources(typeof(DummyClassWithAttributeInherited)));
			AssertContainsExactElementsInAnyOrder("Should return an empty array", expectedEmpty, SupportExRateSourceAttribute.SupportedExRateSources(typeof(DummyClass)));
		}

		#region Implementation
		[SupportExRateSource(ExRateSourceType.Voyage)]
		class DummyClassWithAttribute
		{
		}

		class DummyClassWithAttributeInherited : DummyClassWithAttribute
		{
		}

		class DummyClass
		{
		}
		#endregion
	}
}
