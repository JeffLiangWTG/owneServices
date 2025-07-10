using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(SelectedSortOrder))]
	sealed class SelectedSortOrderTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider()
		{
			return new SelectedSortOrder();
		}

		protected override void SetUp()
		{
			base.SetUp();
			PrepareRenderer();
		}

		public void TestWhenSortOrderSelected()
		{
			Report.SortOrderCollection.Add("Boris", "Foo1, foo2");
			Report.SortOrderCollection.Add("Natasha", "Foo2, foo3, foo1");
			Report.SortOrderCollection[1].Selected = true;

			AssertEquals("", "Natasha", new SelectedSortOrder().GetReplacement("", Report));
		}

		public void TestWhenNoSortOrderSelected()
		{
			AssertEquals("", "", new SelectedSortOrder().GetReplacement("", Report));
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <Seletced Sort Order>", !ValueProviderToTest.IsResponsibleForReplacing("<Seletced Sort Order>", Passes.FirstPass));
			Assert("should match < selected      sort      order >", ValueProviderToTest.IsResponsibleForReplacing("< selected      sort      order >", Passes.FirstPass));
			Assert("should match <SelectedSortOrder>", ValueProviderToTest.IsResponsibleForReplacing("<SelectedSortOrder>", Passes.FirstPass));
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.SortOrderCollection.Add("Organization", "OH_Code");
			Report.SortOrderCollection.Add("Expiry Date", "ExpiryDate");
			Report.SortOrderCollection[1].Selected = true;
		}
	}
}
