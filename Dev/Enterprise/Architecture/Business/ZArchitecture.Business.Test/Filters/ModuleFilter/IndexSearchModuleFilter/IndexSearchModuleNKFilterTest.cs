using System;
using System.Linq;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Modules.Testing;
using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(IndexSearchModuleNKFilter))]
	sealed class IndexSearchModuleNKFilterTest : IndexSearchModuleFilterTestCase<IndexSearchModuleNKFilter>
	{
		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		protected override ModuleFilter GetNewModuleFilter()
		{
			var list = new StmNoteNonDependentCollection(Factory);
			var field = SearchField.Create("moo", "test description");
			return new IndexSearchModuleNKFilter(field, DummyModuleIDs.Dummy, list);
		}

		public override void TestGetGlowIndexQuery()
		{
			var filter = (IndexSearchModuleNKFilter)GetNewModuleFilter();

			filter.Property = "foo";

			filter.ComparisonOperator = "exact";
			AssertEquals($"(moo eq 'foo')", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "not equal";
			AssertEquals($"(moo ne 'foo')", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "is blank";
			AssertEquals($"((moo eq null) or (moo eq ''))", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "is not blank";
			AssertEquals($"((moo ne null) and (moo ne ''))", filter.GetGlowIndexQuery().ToUrlComponent());
		}

		public void TestGetUrlEmpty()
		{
			var filter = (IndexSearchModuleNKFilter)GetNewModuleFilter();
			filter.Property = string.Empty;

			filter.ComparisonOperator = "exact";
			AssertEquals(string.Empty, filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "not equal";
			AssertEquals(string.Empty, filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "is blank";
			AssertEquals($"((moo eq null) or (moo eq ''))", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "is not blank";
			AssertEquals($"((moo ne null) and (moo ne ''))", filter.GetGlowIndexQuery().ToUrlComponent());
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

			var filter = (IndexSearchModuleNKFilter)GetNewModuleFilter();

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

			var filter = (IndexSearchModuleNKFilter)GetNewModuleFilter();
			AssertSequencesEqual(expects, filter.AllowedComparisonOperators);
		}

		public void TestComparisonOperatorFallbackToExact()
		{
			var guid = Guid.NewGuid();

			var filter = (IndexSearchModuleNKFilter)GetNewModuleFilter();
			filter.Property = "foo";

			filter.ComparisonOperator = "otherOperator";

			AssertEquals("exact", filter.ComparisonOperator);
			AssertEquals($"(moo eq 'foo')", filter.GetGlowIndexQuery().ToUrlComponent());
		}
	}

	[TestedType(typeof(IndexSearchModuleNKFilter))]
	sealed class IndexSearchModuleNKFilter_ModuleFilterTest : ModuleFilterTestCase<ModuleNkFilter>
	{
		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override ModuleNkFilter GetNewModuleFilter()
		{
			var list = new StmNoteNonDependentCollection(Factory);
			var field = SearchField.Create("moo", "test description");
			return new IndexSearchModuleNKFilter(field, DummyModuleIDs.Dummy, list);
		}
	}
}
