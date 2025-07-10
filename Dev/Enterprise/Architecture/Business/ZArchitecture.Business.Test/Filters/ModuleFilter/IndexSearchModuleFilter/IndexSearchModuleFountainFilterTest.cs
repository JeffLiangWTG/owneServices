using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(IndexSearchModuleFountainFilter))]
	sealed class IndexSearchModuleFountainFilterTest : IndexSearchModuleFilterTestCase<IndexSearchModuleFountainFilter>
	{
		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.NumbersAndReferences;

		public override void TestGetGlowIndexQuery()
		{
			var filter = new IndexSearchModuleFountainFilter(SearchField.Create("moo"));
			filter.Property = "A";

			filter.ComparisonOperator = "any starts with";
			AssertEquals("(startswith(moo,'0000000A'))", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "no starts with";
			AssertEquals("(not(startswith(moo,'0000000A')))", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "any exact";
			AssertEquals("(moo eq '0000000A')", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "none exact";
			AssertEquals("(moo ne '0000000A')", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "all exact";
			AssertEquals("(moo eq '\"0000000A\"')", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "is blank";
			AssertEquals("((moo eq null) or (moo eq ''))", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "is not blank";
			AssertEquals("((moo ne null) and (moo ne ''))", filter.GetGlowIndexQuery().ToUrlComponent());

			filter = new IndexSearchModuleFountainFilter(SearchField.Create("moo"), "I");
			filter.Property = "A";

			filter.ComparisonOperator = "any starts with";
			AssertEquals("(startswith(moo,'I0000000A'))", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "no starts with";
			AssertEquals("(not(startswith(moo,'I0000000A')))", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "any exact";
			AssertEquals("(moo eq 'I0000000A')", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "none exact";
			AssertEquals("(moo ne 'I0000000A')", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "all exact";
			AssertEquals("(moo eq '\"I0000000A\"')", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "is blank";
			AssertEquals("((moo eq null) or (moo eq ''))", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "is not blank";
			AssertEquals("((moo ne null) and (moo ne ''))", filter.GetGlowIndexQuery().ToUrlComponent());
		}

		protected override ModuleFilter GetNewModuleFilter()
		{
			var field = SearchField.Create("moo");
			return new IndexSearchModuleFountainFilter(field);
		}
	}

	[TestedType(typeof(IndexSearchModuleFountainFilter))]
	sealed class IndexSearchModuleFountainFilter_ModuleFilterTest : ModuleFilterTestCase<ModuleTextFilter>
	{
		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.NumbersAndReferences;

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override ModuleTextFilter GetNewModuleFilter()
		{
			var field = SearchField.Create("moo");
			return new IndexSearchModuleFountainFilter(field);
		}
	}
}
