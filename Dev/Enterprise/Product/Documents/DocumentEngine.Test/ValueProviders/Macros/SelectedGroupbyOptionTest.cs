using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(SelectedGroupbyOption))]
	sealed class SelectedGroupbyOptionTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider()
		{
			return new SelectedGroupbyOption();
		}

		protected override void SetUp()
		{
			base.SetUp();
			PrepareRenderer();
		}

		public void TestRegex()
		{
			AssertEquals(true, new SelectedGroupbyOption().IsResponsibleForReplacing("<Selected Groupby Option>", Passes.FirstPass));
		}

		public void TestWhenGroupbyOptionSelected()
		{
			Report.GroupByCollection.Add("Boris", "Foo1, foo2");
			Report.GroupByCollection.Add("Natasha", "Foo2, foo3, foo1");
			Report.GroupByCollection[1].Selected = true;

			AssertEquals("", "Natasha", new SelectedGroupbyOption().GetReplacement("", Report));
		}

		public void TestWhenNoGroupbyOptionSelected()
		{
			AssertEquals("", "", new SelectedGroupbyOption().GetReplacement("", Report));
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.GroupByCollection.Add("Organization", "OH_Code");
			Report.GroupByCollection.Add("Expiry Date", "ExpiryDate");
			Report.GroupByCollection[1].Selected = true;
		}
	}
}
