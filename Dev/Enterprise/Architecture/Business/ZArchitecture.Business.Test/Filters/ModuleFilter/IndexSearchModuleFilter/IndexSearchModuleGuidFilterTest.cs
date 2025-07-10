using System;
using System.Linq;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Modules.Testing;
using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(IndexSearchModuleGuidFilter))]
	sealed class IndexSearchModuleGuidFilterTest : IndexSearchModuleFilterTestCase<IndexSearchModuleGuidFilter>
	{
		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		protected override ModuleFilter GetNewModuleFilter()
		{
			var list = new StmNoteNonDependentCollection(Factory);
			var field = SearchField.Create("moo", "test description");
			return new IndexSearchModuleGuidFilter(field, DummyModuleIDs.Dummy, list);
		}

		public override void TestGetGlowIndexQuery()
		{
			var filter = (IndexSearchModuleGuidFilter)GetNewModuleFilter();

			var guid = Guid.NewGuid();
			filter.Property = guid;

			filter.ComparisonOperator = "exact";
			AssertEquals($"(moo eq {guid})", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "not equal";
			AssertEquals($"(moo ne {guid})", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "is blank";
			AssertEquals($"(moo eq null)", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "is not blank";
			AssertEquals($"(moo ne null)", filter.GetGlowIndexQuery().ToUrlComponent());
		}

		public void TestGetUrlEmpty()
		{
			var filter = (IndexSearchModuleGuidFilter)GetNewModuleFilter();
			filter.Property = Guid.Empty;

			filter.ComparisonOperator = "exact";
			AssertEquals(string.Empty, filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "not equal";
			AssertEquals(string.Empty, filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "is blank";
			AssertEquals($"(moo eq null)", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "is not blank";
			AssertEquals($"(moo ne null)", filter.GetGlowIndexQuery().ToUrlComponent());
		}

		public void TestComparisonOperator_List()
		{
			var expects = new string[]
			{
				"exact",
				"not equal",
				"is blank",
				"is not blank",
			};

			var filter = (IndexSearchModuleGuidFilter)GetNewModuleFilter();

			AssertArrayEqualsByElements(expects, filter.ComparisonOperator_List.Cast<ICodeDescription>().Select(op => op.Code).ToArray());
		}

		public void TestAllowedComparisonOperators()
		{
			var expects = new string[]
			{
				"",
				"exact",
				"not equal",
				"is blank",
				"is not blank",
			};

			var filter = (IndexSearchModuleGuidFilter)GetNewModuleFilter();
			AssertSequencesEqual(expects, filter.AllowedComparisonOperators);
		}

		public void TestComparisonOperatorFallbackToExact()
		{
			var guid = Guid.NewGuid();

			var filter = (IndexSearchModuleGuidFilter)GetNewModuleFilter();
			filter.Property = guid;

			filter.ComparisonOperator = "otherOperator";

			AssertEquals("exact", filter.ComparisonOperator);
			AssertEquals($"(moo eq {guid})", filter.GetGlowIndexQuery().ToUrlComponent());
		}
	}

	[TestedType(typeof(IndexSearchModuleGuidFilter))]
	sealed class IndexSearchModuleGuidFilter_ModuleFilterTest : ModuleFilterTestCase<ModuleGuidFilter>
	{
		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override ModuleGuidFilter GetNewModuleFilter()
		{
			var list = new StmNoteNonDependentCollection(Factory);
			return new IndexSearchModuleGuidFilter(SearchField.Create("moo", "test description"), DummyModuleIDs.Dummy, list);
		}
	}
}
