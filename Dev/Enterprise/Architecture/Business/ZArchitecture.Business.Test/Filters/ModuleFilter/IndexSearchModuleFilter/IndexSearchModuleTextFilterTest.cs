using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;
using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(IndexSearchModuleTextFilter))]
	sealed class IndexSearchModuleTextFilterTest : IndexSearchModuleFilterTestCase<IndexSearchModuleTextFilter>
	{
		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.TextSearch;

		protected override ModuleFilter GetNewModuleFilter()
		{
			var field = SearchField.Create("moo");
			return new IndexSearchModuleTextFilter(field);
		}

		public override void TestGetGlowIndexQuery()
		{
			var filter = new IndexSearchModuleTextFilter(SearchField.Create("moo"));
			filter.Property = "HAY";

			filter.ComparisonOperator = "any starts with";
			AssertEquals("(startswith(moo,'HAY'))", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "no starts with";
			AssertEquals("(not(startswith(moo,'HAY')))", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "any exact";
			AssertEquals("(moo eq 'HAY')", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "none exact";
			AssertEquals("(moo ne 'HAY')", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "all exact";
			AssertEquals("(moo eq '\"HAY\"')", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "is blank";
			AssertEquals("((moo eq null) or (moo eq ''))", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "is not blank";
			AssertEquals("((moo ne null) and (moo ne ''))", filter.GetGlowIndexQuery().ToUrlComponent());
		}

		public void TestGetUrlEmpty()
		{
			var filter = new IndexSearchModuleTextFilter(SearchField.Create("moo"));
			filter.Property = string.Empty;

			filter.ComparisonOperator = "any starts with";
			AssertEquals(string.Empty, filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "any exact";
			AssertEquals(string.Empty, filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "all exact";
			AssertEquals(string.Empty, filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "none exact";
			AssertEquals(string.Empty, filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "no starts with";
			AssertEquals(string.Empty, filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "is blank";
			AssertEquals(false, string.IsNullOrEmpty(filter.GetGlowIndexQuery().ToUrlComponent()));

			filter.ComparisonOperator = "is not blank";
			AssertEquals(false, string.IsNullOrEmpty(filter.GetGlowIndexQuery().ToUrlComponent()));
		}

		public void TestGetUrlSpecialCharacters()
		{
			var filter = new IndexSearchModuleTextFilter(SearchField.Create("moo"));
			filter.Property = "C'lyde";

			filter.ComparisonOperator = "any starts with";
			AssertEquals("(startswith(moo,'C%27%27lyde'))", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "no starts with";
			AssertEquals("(not(startswith(moo,'C%27%27lyde')))", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "any exact";
			AssertEquals("(moo eq 'C%27%27lyde')", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "none exact";
			AssertEquals("(moo ne 'C%27%27lyde')", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "all exact";
			AssertEquals("(moo eq '\"C%27%27lyde\"')", filter.GetGlowIndexQuery().ToUrlComponent());
		}

		public void TestComparisonOperator_List()
		{
			var expects = new string[]
			{
				"any starts with",
				"any exact",
				"all exact",
				"none exact",
				"no starts with",
				"is blank",
				"is not blank"
			};

			var filter = new IndexSearchModuleTextFilter(SearchField.Create("moo"));
			AssertArrayEqualsByElements(expects, filter.ComparisonOperator_List.Cast<ICodeDescription>().Select(op => op.Code).ToArray());
		}

		public void TestAllowedComparisonOperators()
		{
			var expects = new string[]
			{
				"any starts with",
				"any exact",
				"all exact",
				"none exact",
				"no starts with",
				"is blank",
				"is not blank"
			};

			var filter = new IndexSearchModuleTextFilter(SearchField.Create("moo"));
			AssertSequencesEqual(expects, filter.AllowedComparisonOperators);
		}

		public void TestComparisonOperatorFallbackToAnyStartsWith()
		{
			var filter = new IndexSearchModuleTextFilter(SearchField.Create("moo"));
			filter.Property = "HAY";

			filter.ComparisonOperator = "otherOperator";

			AssertEquals("any starts with", filter.ComparisonOperator);
			AssertEquals("(startswith(moo,'HAY'))", filter.GetGlowIndexQuery().ToUrlComponent());
		}

		public void TestGetGlowIndexQueryDelegate()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("code1", "value1");
			list.AddPair("code2", "value2");

			var filter = new IndexSearchModuleTextFilter("test", SearchField.Create("moo"), (field, property) => new NotEqualQuery(new Term(field.FieldName, property)), list, null);
			filter.Property = "HAY";

			AssertEquals("(moo ne 'HAY')", filter.GetGlowIndexQuery().ToUrlComponent());
		}

		public void TestIndexSearchModuleTextFilter()
		{
			var list = new List<string>() { "value1" , "value2" };
			var filter = new IndexSearchModuleTextFilter(SearchField.Create("moo", "test"), list);
			filter.Property = "value1";

			AssertEquals("any starts with", filter.ComparisonOperator);
			AssertEquals("(startswith(moo,'value1'))", filter.GetGlowIndexQuery().ToUrlComponent());
		}
	}

	[TestedType(typeof(IndexSearchModuleTextFilter))]
	sealed class IndexSearchModuleTextFilter_ModuleFilterTest : ModuleTextFilterTest
	{
		protected override ModuleTextFilter GetNewModuleFilter()
		{
			var field = SearchField.Create("moo");
			return new IndexSearchModuleTextFilter(field);
		}

		public override void TestIsExpensiveQuery()
		{
			Assert(!Filter.IsExclusive);
		}

		public override void TestSqlComparisonOperator()
		{
			Assert(true);
		}

		public override void TestClearSetsCorrectComparisonOperator()
		{
			Filter.Clear();

			AssertEquals("Precondition", "", Filter.Property);
			AssertEquals("Precondition", "", Filter.DefaultProperty);
			AssertEquals("Precondition", IndexSearchModuleTextFilter.IndexSearchTextFilterConstants.AnyStartsWith, Filter.ComparisonOperator);

			Filter.ComparisonOperator_List.RemoveCode(IndexSearchModuleTextFilter.IndexSearchTextFilterConstants.AnyStartsWith);
			Filter.ComparisonOperator_List.RemoveCode(IndexSearchModuleTextFilter.IndexSearchTextFilterConstants.AnyExact);
			Filter.ComparisonOperator_List.RemoveCode(IndexSearchModuleTextFilter.IndexSearchTextFilterConstants.AllExact);
			Filter.ComparisonOperator_List.RemoveCode(IndexSearchModuleTextFilter.IndexSearchTextFilterConstants.NoStartWith);
			Filter.ComparisonOperator_List.RemoveCode(IndexSearchModuleTextFilter.IndexSearchTextFilterConstants.NoneExact);

			Filter.Clear();
			AssertEquals(IndexSearchModuleTextFilter.IndexSearchTextFilterConstants.AnyStartsWith, Filter.ComparisonOperator);

			Filter.ComparisonOperator_List.RemoveCode(IndexSearchModuleTextFilter.IndexSearchTextFilterConstants.IsBlank);
			Filter.Clear();
			AssertEquals(IndexSearchModuleTextFilter.IndexSearchTextFilterConstants.AnyStartsWith, Filter.ComparisonOperator);
		}
	}
}
