using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(FirstDayOfLastWeek))]
	sealed class FirstDayOfLastWeekTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<First Day Last Week>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< First DayOfLast Week       >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< FirstDay Of LastWeek       >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< First  Day  Of  Last  Week       >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<FirstDayOfLastWeek>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			Assert(ValueProviderToTest.GetReplacement("<FirstDayOfLastWeek>", Report).GetType() == typeof(string));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new FirstDayOfLastWeek();
		}

		[TestDate(2018, 11, 7)]
		public override void TestDocumentation()
		{
			base.TestDocumentation();
		}
	}
}
