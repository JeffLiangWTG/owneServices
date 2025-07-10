using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(LastDayOfLastWeek))]
	sealed class LastDayOfLastWeekTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<Last Day Last Week>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< Last DayOfLast Week       >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< LastDay Of LastWeek       >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< Last  Day  Of  Last  Week       >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<LastDayOfLastWeek>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			Assert(ValueProviderToTest.GetReplacement("<LastDayOfLastWeek>", Report).GetType() == typeof(string));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new LastDayOfLastWeek();
		}

		[TestDate(2018, 11, 7)]
		public override void TestDocumentation()
		{
			base.TestDocumentation();
		}
	}
}
