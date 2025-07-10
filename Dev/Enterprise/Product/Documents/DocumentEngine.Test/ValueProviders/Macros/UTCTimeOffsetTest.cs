using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(UTCTimeOffset))]
	sealed class UTCTimeOffsetTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match <UTCTimeOffset>", ValueProviderToTest.IsResponsibleForReplacing("<UTCTimeOffset>", Passes.FirstPass));
			Assert("should match <   UTCTimeOffset    >", ValueProviderToTest.IsResponsibleForReplacing("<   UTCTimeOffset    >", Passes.FirstPass));
			Assert("should match < UTCTimeOffset>", ValueProviderToTest.IsResponsibleForReplacing("< UTCTimeOffset>", Passes.FirstPass));
			Assert("should match <UTCTimeOffset  >", ValueProviderToTest.IsResponsibleForReplacing("<UTCTimeOffset  >", Passes.FirstPass));
			Assert("should not match < UTC Time Offset >", !ValueProviderToTest.IsResponsibleForReplacing("< UTC Time Offset >", Passes.FirstPass));
		}

		[TestDate(2019, 2, 18)]
		[TestUtcOffset(10, 0, 0)]
		public void TestReplacement_AustralianEasternStandardTime()
		{
			AssertEquals(600, ValueProviderToTest.GetReplacement("<UTCTimeOffset>", Report));
		}

		[TestDate(2019, 2, 18)]
		[TestUtcOffset(-10, 0, 0)]
		public void TestReplacement_CookIslandTime()
		{
			AssertEquals(-600, ValueProviderToTest.GetReplacement("<UTCTimeOffset>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new UTCTimeOffset();
		}
	}
}
