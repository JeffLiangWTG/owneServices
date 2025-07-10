using System;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(DateTimeStart))]
	sealed class DateTimeStartTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<Date tim start>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< Date    timestart       >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< datetime      start       >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< date    time     start       >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<Datetimestart>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			Assert(ValueProviderToTest.GetReplacement("<DateTimeStart>", Report).GetType() == typeof(DateTime));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new DateTimeStart();
		}

		[TestDate(2018, 11, 7)]
		public override void TestDocumentation()
		{
			Report.SetStartTime();
			base.TestDocumentation();
		}

		[TestDate(2018, 11, 7)]
		public void TestDocumentation2()
		{
			base.TestDocumentation();
		}
	}
}
