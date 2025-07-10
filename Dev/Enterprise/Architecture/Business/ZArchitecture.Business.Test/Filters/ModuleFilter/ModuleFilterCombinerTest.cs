//using SubGroupRelationships = System.Collections.Generic.Dictionary<Enterprise.ZArchitecture.Business.BlueprintModuleFilterSubGroup, System.Collections.Generic.List<Enterprise.ZArchitecture.Business.BlueprintModuleFilterSubGroup>>;
//using System.Globalization;
//using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(ShipmentXQueryPaths))]

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ModuleFilterCombinerTest : TestCaseWithFactory
	{
		public void TestShouldNotAddSubGroupForEmptyFilters()
		{
			var subGroup = new ModuleFilterSubGroupForTest("subGroup");

			var filter1 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter1.SubGroup = subGroup;
			filter1.Property = "";

			var filter2 = new ModuleTextFilter("Description", DummyBizoSchema.Z0_Description);
			filter2.SubGroup = null;
			filter2.Property = "Blaticus";

			AssertEquals("precondition:", true, filter1.IsEmpty);
			AssertEquals("precondition:", false, filter2.IsEmpty);

			var resultQuery = ModuleFilterCombiner.GetCombinedFilter(new ModuleFilter[]
			{
				filter1,
				filter2,
			});

			const string expectedQuery =
@"(
	Z0_Description like 'Blaticus%' 
	AND
	Z0_Description >= 'Blaticus' 
	AND
	Z0_Description <= 'Blaticuþ'
)
";

			AssertMultilineASCIIEquals("", expectedQuery, resultQuery.LiteralTextSqlFormatted);
		}

		public void TestShouldNotIgnoreEmptyQueriesThatIgnoreActiveFilters()
		{
			var subGroup = new ModuleFilterSubGroupForTest("subGroup");

			var filter1 = new ModuleTextFilter("Code", (SQLComparisonOperator opp, ZString value) =>
			{
				return new ZQuery() { IgnoreActiveFilter = (value == "IGN") };
			});
			filter1.SubGroup = null;
			filter1.Property = "BOB";

			var filter2 = new ModuleTextFilter("Description", DummyBizoSchema.Z0_Description);
			filter2.SubGroup = null;
			filter2.Property = "Blaticus";

			var resultQuery1 = ModuleFilterCombiner.GetCombinedFilter(new ModuleFilter[]
			{
				filter1,
				filter2,
			});

			filter1.Property = "IGN";

			var resultQuery2 = ModuleFilterCombiner.GetCombinedFilter(new ModuleFilter[]
			{
				filter1,
				filter2,
			});

			const string expectedQuery =
@"(
	Z0_Description like 'Blaticus%' 
	AND
	Z0_Description >= 'Blaticus' 
	AND
	Z0_Description <= 'Blaticuþ'
)
";

			CombineAssertions(delegate
			{
				AssertEquals("Should not ignore active filter.", false, resultQuery1.IgnoreActiveFilter);
				AssertMultilineASCIIEquals("Should not ignore active filter.", expectedQuery, resultQuery1.LiteralTextSqlFormatted);

				AssertEquals("Should ignore active filter.", true, resultQuery2.IgnoreActiveFilter);
				AssertMultilineASCIIEquals("Should ignore active filter.", expectedQuery, resultQuery2.LiteralTextSqlFormatted);
			});
		}

		public void TestUnion()
		{
			var filter1 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter1.SubGroup = null;
			filter1.Property = "VAL1";
			filter1.OrCategory = FilterOrCategory.Blue;

			var filter2 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter2.SubGroup = null;
			filter2.Property = "VAL2";
			filter2.OrCategory = FilterOrCategory.Blue;

			var filterUnion1 = new ModuleUnionOrOrFilter(FilterStripBusinessObject.UnionOrOrDescription, "UNION?", null, null);
			filterUnion1.SubGroup = null;
			filterUnion1.Property0 = true;
			filterUnion1.OrCategory = FilterOrCategory.None;

			var resultQuery = ModuleFilterCombiner.GetCombinedFilter(new ModuleFilter[]
			{
				filter1,
				filter2,
				filterUnion1,
			});

			AssertMultilineASCIIEquals("", @"(
	Z0_Code like 'VAL2%' 
	AND
	Z0_Code >= 'VAL2' 
	AND
	Z0_Code <= 'VALþ'
)

union
 WHERE 
(
	Z0_Code like 'VAL1%' 
	AND
	Z0_Code >= 'VAL1' 
	AND
	Z0_Code <= 'VALþ'
)
", resultQuery.LiteralTextSqlFormatted);

			var filter3 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter3.SubGroup = null;
			filter3.Property = "VAL3";
			filter3.OrCategory = FilterOrCategory.Green;

			var filter4 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter4.SubGroup = null;
			filter4.Property = "VAL4";
			filter4.OrCategory = FilterOrCategory.Green;

			resultQuery = ModuleFilterCombiner.GetCombinedFilter(new ModuleFilter[]
			{
				filter1,
				filter2,
				filterUnion1,
				filter3,
				filter4,
			});

			AssertMultilineASCIIEquals("", @"(
	(
		Z0_Code like 'VAL2%' 
		AND
		Z0_Code >= 'VAL2' 
		AND
		Z0_Code <= 'VALþ'
	)
	AND
	(
		Z0_Code like 'VAL3%' 
		AND
		Z0_Code >= 'VAL3' 
		AND
		Z0_Code <= 'VALþ'
	)
)

union
 WHERE 
(
	(
		Z0_Code like 'VAL1%' 
		AND
		Z0_Code >= 'VAL1' 
		AND
		Z0_Code <= 'VALþ'
	)
	AND
	(
		Z0_Code like 'VAL4%' 
		AND
		Z0_Code >= 'VAL4' 
		AND
		Z0_Code <= 'VALþ'
	)
)

union
 WHERE 
(
	(
		Z0_Code like 'VAL2%' 
		AND
		Z0_Code >= 'VAL2' 
		AND
		Z0_Code <= 'VALþ'
	)
	AND
	(
		Z0_Code like 'VAL4%' 
		AND
		Z0_Code >= 'VAL4' 
		AND
		Z0_Code <= 'VALþ'
	)
)

union
 WHERE 
(
	(
		Z0_Code like 'VAL1%' 
		AND
		Z0_Code >= 'VAL1' 
		AND
		Z0_Code <= 'VALþ'
	)
	AND
	(
		Z0_Code like 'VAL3%' 
		AND
		Z0_Code >= 'VAL3' 
		AND
		Z0_Code <= 'VALþ'
	)
)
", resultQuery.LiteralTextSqlFormatted);

			filterUnion1.OrCategory = FilterOrCategory.Green;

			resultQuery = ModuleFilterCombiner.GetCombinedFilter(new ModuleFilter[]
			{
				filter1,
				filter2,
				filterUnion1,
				filter3,
				filter4,
			});

			AssertMultilineASCIIEquals("", @"(
	(
		(
			Z0_Code like 'VAL1%' 
			AND
			Z0_Code >= 'VAL1' 
			AND
			Z0_Code <= 'VALþ'
		)
		OR
		(
			Z0_Code like 'VAL2%' 
			AND
			Z0_Code >= 'VAL2' 
			AND
			Z0_Code <= 'VALþ'
		)
	)
	AND
	(
		Z0_Code like 'VAL4%' 
		AND
		Z0_Code >= 'VAL4' 
		AND
		Z0_Code <= 'VALþ'
	)
)

union
 WHERE 
(
	(
		(
			Z0_Code like 'VAL1%' 
			AND
			Z0_Code >= 'VAL1' 
			AND
			Z0_Code <= 'VALþ'
		)
		OR
		(
			Z0_Code like 'VAL2%' 
			AND
			Z0_Code >= 'VAL2' 
			AND
			Z0_Code <= 'VALþ'
		)
	)
	AND
	(
		Z0_Code like 'VAL3%' 
		AND
		Z0_Code >= 'VAL3' 
		AND
		Z0_Code <= 'VALþ'
	)
)
", resultQuery.LiteralTextSqlFormatted);

			filterUnion1.Property0 = false;

			var filterUnion2 = new ModuleUnionOrOrFilter(FilterStripBusinessObject.UnionOrOrDescription, "UNION?", null, null);
			filterUnion2.SubGroup = null;
			filterUnion2.Property0 = true;
			filterUnion2.OrCategory = FilterOrCategory.None;

			resultQuery = ModuleFilterCombiner.GetCombinedFilter(new ModuleFilter[]
			{
				filter1,
				filter2,
				filterUnion1,
				filterUnion2,
				filter3,
				filter4,
			});

			AssertMultilineASCIIEquals("", @"(
	(
		Z0_Code like 'VAL2%' 
		AND
		Z0_Code >= 'VAL2' 
		AND
		Z0_Code <= 'VALþ'
	)
	AND
	(
		(
			Z0_Code like 'VAL3%' 
			AND
			Z0_Code >= 'VAL3' 
			AND
			Z0_Code <= 'VALþ'
		)
		OR
		(
			Z0_Code like 'VAL4%' 
			AND
			Z0_Code >= 'VAL4' 
			AND
			Z0_Code <= 'VALþ'
		)
	)
)

union
 WHERE 
(
	(
		Z0_Code like 'VAL1%' 
		AND
		Z0_Code >= 'VAL1' 
		AND
		Z0_Code <= 'VALþ'
	)
	AND
	(
		(
			Z0_Code like 'VAL3%' 
			AND
			Z0_Code >= 'VAL3' 
			AND
			Z0_Code <= 'VALþ'
		)
		OR
		(
			Z0_Code like 'VAL4%' 
			AND
			Z0_Code >= 'VAL4' 
			AND
			Z0_Code <= 'VALþ'
		)
	)
)
", resultQuery.LiteralTextSqlFormatted);

			filter3.OrCategory = FilterOrCategory.Blue;

			resultQuery = ModuleFilterCombiner.GetCombinedFilter(new ModuleFilter[]
			{
				filter1,
				filter2,
				filter3,
				filterUnion2,
			});

			AssertMultilineASCIIEquals("", @"(
	Z0_Code like 'VAL2%' 
	AND
	Z0_Code >= 'VAL2' 
	AND
	Z0_Code <= 'VALþ'
)

union
 WHERE 
(
	Z0_Code like 'VAL3%' 
	AND
	Z0_Code >= 'VAL3' 
	AND
	Z0_Code <= 'VALþ'
)

union
 WHERE 
(
	Z0_Code like 'VAL1%' 
	AND
	Z0_Code >= 'VAL1' 
	AND
	Z0_Code <= 'VALþ'
)
", resultQuery.LiteralTextSqlFormatted);
		}

		public void TestRecompileEnabledAndCardinalityDisabled()
		{
			var filter1 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter1.SubGroup = null;
			filter1.Property = "VAL1";
			filter1.OrCategory = FilterOrCategory.Blue;

			var filterRecompile = new ModuleRecompileFilter(FilterStripBusinessObject.RecompileDescription, "RECOMPILE", null, null);
			filterRecompile.SubGroup = null;
			filterRecompile.Property0 = true;
			filterRecompile.OrCategory = FilterOrCategory.None;

			var filterCardinality = new ModuleCardinalityFilter(FilterStripBusinessObject.CardinalityDescription, "CARDINALITY", null, null);
			filterCardinality.SubGroup = null;
			filterCardinality.Property0 = false;
			filterCardinality.OrCategory = FilterOrCategory.None;

			var resultQuery = ModuleFilterCombiner.GetCombinedFilter(new ModuleFilter[]
			{
				filter1,
				filterRecompile,
				filterCardinality,
			});

			AssertEquals(QueryHints.RECOMPILE, resultQuery.QueryHints);
		}

		public void TestRecompileDisabledAndCardinalityEnabled()
		{
			var filter1 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter1.SubGroup = null;
			filter1.Property = "VAL1";
			filter1.OrCategory = FilterOrCategory.Blue;

			var filterRecompile = new ModuleRecompileFilter(FilterStripBusinessObject.RecompileDescription, "RECOMPILE", null, null);
			filterRecompile.SubGroup = null;
			filterRecompile.Property0 = false;
			filterRecompile.OrCategory = FilterOrCategory.None;

			var filterCardinality = new ModuleCardinalityFilter(FilterStripBusinessObject.CardinalityDescription, "CARDINALITY", null, null);
			filterCardinality.SubGroup = null;
			filterCardinality.Property0 = true;
			filterCardinality.OrCategory = FilterOrCategory.None;

			var resultQuery = ModuleFilterCombiner.GetCombinedFilter(new ModuleFilter[]
			{
				filter1,
				filterRecompile,
				filterCardinality,
			});

			AssertEquals(QueryHints.CARDINALITY, resultQuery.QueryHints);
		}

		public void TestRecompileAndCardinalityDisabled()
		{
			var filter1 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter1.SubGroup = null;
			filter1.Property = "VAL1";
			filter1.OrCategory = FilterOrCategory.Blue;

			var filterRecompile = new ModuleRecompileFilter(FilterStripBusinessObject.RecompileDescription, "RECOMPILE", null, null);
			filterRecompile.SubGroup = null;
			filterRecompile.Property0 = false;
			filterRecompile.OrCategory = FilterOrCategory.None;

			var filterCardinality = new ModuleCardinalityFilter(FilterStripBusinessObject.CardinalityDescription, "CARDINALITY", null, null);
			filterCardinality.SubGroup = null;
			filterCardinality.Property0 = false;
			filterCardinality.OrCategory = FilterOrCategory.None;

			var resultQuery = ModuleFilterCombiner.GetCombinedFilter(new ModuleFilter[]
			{
				filter1,
				filterRecompile,
				filterCardinality,
			});

			AssertEquals(QueryHints.None, resultQuery.QueryHints);
		}

		public void TestRecompileAndCardinalityEnabled()
		{
			var filter1 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter1.SubGroup = null;
			filter1.Property = "VAL1";
			filter1.OrCategory = FilterOrCategory.Blue;

			var filterRecompile = new ModuleRecompileFilter(FilterStripBusinessObject.RecompileDescription, "RECOMPILE", null, null);
			filterRecompile.SubGroup = null;
			filterRecompile.Property0 = true;
			filterRecompile.OrCategory = FilterOrCategory.None;

			var filterCardinality = new ModuleCardinalityFilter(FilterStripBusinessObject.CardinalityDescription, "CARDINALITY", null, null);
			filterCardinality.SubGroup = null;
			filterCardinality.Property0 = true;
			filterCardinality.OrCategory = FilterOrCategory.None;

			var resultQuery = ModuleFilterCombiner.GetCombinedFilter(new ModuleFilter[]
			{
				filter1,
				filterRecompile,
				filterCardinality,
			});

			AssertEquals(QueryHints.CARDINALITY | QueryHints.RECOMPILE, resultQuery.QueryHints);
		}

		public void TestFilterGrouping()
		{
			var subGroup = new ModuleFilterSubGroupForTest("subGroup");

			var filter1 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter1.SubGroup = null;
			filter1.Property = "VAL1";
			filter1.OrCategory = FilterOrCategory.Blue;

			var filter2 = new ModuleTextFilter("Sub Code", DummyBizoSchema.Z0_Code);
			filter2.SubGroup = subGroup;
			filter2.Property = "SUB1";
			filter2.OrCategory = FilterOrCategory.Blue;

			var filter3 = new ModuleTextFilter("Sub Code", DummyBizoSchema.Z0_Code);
			filter3.SubGroup = subGroup;
			filter3.Property = "SUB2";
			filter3.OrCategory = FilterOrCategory.Blue;

			var filter4 = new ModuleTextFilter("Other Sub Code", DummyBizoSchema.Z0_FK_Code);
			filter4.SubGroup = subGroup;
			filter4.Property = "OSUB";
			filter4.OrCategory = FilterOrCategory.None;

			var filter5 = new ModuleTextFilter("Description", DummyBizoSchema.Z0_Description);
			filter5.SubGroup = null;
			filter5.Property = "Blaticus";
			filter5.OrCategory = FilterOrCategory.None;

			var resultQuery = ModuleFilterCombiner.GetCombinedFilter(new ModuleFilter[]
			{
				filter1,
				filter2,
				filter3,
				filter4,
				filter5,
			});

			const string expectedQuery =
@"(
	(
		Z0_Description like 'Blaticus%' 
		AND
		Z0_Description >= 'Blaticus' 
		AND
		Z0_Description <= 'Blaticuþ'
	)
	AND
	(
		(
			Z0_Code like 'VAL1%' 
			AND
			Z0_Code >= 'VAL1' 
			AND
			Z0_Code <= 'VALþ'
		)
		OR
		(
			Z0_Guid IN 
			(
				SELECT Z0_PK FROM dbo.DummyBizo WHERE 
				(
					Z0_Code like 'SUB1%' 
					AND
					Z0_Code >= 'SUB1' 
					AND
					Z0_Code <= 'SUBþ'
				)
				OR
				(
					Z0_Code like 'SUB2%' 
					AND
					Z0_Code >= 'SUB2' 
					AND
					Z0_Code <= 'SUBþ'
				)
			)
		)
	)
)
AND
Z0_Guid IN 
(
	SELECT Z0_PK FROM dbo.DummyBizo WHERE 
	(
		Z0_FK_Code like 'OSUB%' 
		AND
		Z0_FK_Code >= 'OSUB' 
		AND
		Z0_FK_Code <= 'OSUþ'
	)
)
";

			AssertMultilineASCIIEquals("", expectedQuery, resultQuery.LiteralTextSqlFormatted);
		}

		public void TestFilterGroupingWithValuesCombining()
		{
			var subGroup = new ModuleFilterSubGroupForTest("subGroup");

			var filter1 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter1.SubGroup = null;
			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter1.Property = "VAL1";
			filter1.OrCategory = FilterOrCategory.Blue;

			var filter2 = new ModuleTextFilter("Sub Code", DummyBizoSchema.Z0_Code);
			filter2.SubGroup = subGroup;
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter2.Property = "SUB1";
			filter2.OrCategory = FilterOrCategory.Blue;

			var filter3 = new ModuleTextFilter("Sub Code", DummyBizoSchema.Z0_Code);
			filter3.SubGroup = subGroup;
			filter3.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter3.Property = "SUB2";
			filter3.OrCategory = FilterOrCategory.Blue;

			var filter4 = new ModuleTextFilter("Other Sub Code", DummyBizoSchema.Z0_FK_Code);
			filter4.SubGroup = subGroup;
			filter4.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter4.Property = "OSUB";
			filter4.OrCategory = FilterOrCategory.None;

			var filter5 = new ModuleTextFilter("Description", DummyBizoSchema.Z0_Description);
			filter5.SubGroup = null;
			filter5.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter5.Property = "Blaticus";
			filter5.OrCategory = FilterOrCategory.None;

			var resultQuery = ModuleFilterCombiner.GetCombinedFilter(new ModuleFilter[]
			{
				filter1,
				filter2,
				filter3,
				filter4,
				filter5,
			});

			const string expectedQuery =
@"(
	Z0_Description = 'Blaticus' 
	AND
	(
		Z0_Code = 'VAL1' 
		OR
		Z0_Guid IN 
		(
			SELECT Z0_PK FROM dbo.DummyBizo WHERE 
			(
				Z0_Code in 
				(
					'SUB1', 'SUB2'
				)
			)
		)
	)
)
AND
Z0_Guid IN 
(
	SELECT Z0_PK FROM dbo.DummyBizo WHERE Z0_FK_Code = 'OSUB'
)
";

			AssertMultilineASCIIEquals("", expectedQuery, resultQuery.LiteralTextSqlFormatted);
		}

		public void TestFilterOuterGrouping()
		{
			var innerGroup = new ModuleFilterSubGroupForTest("subGroup");

			var filter1 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter1.SubGroup = null;
			filter1.Property = "VAL1";
			filter1.OrCategory = FilterOrCategory.Blue;
			filter1.GroupName = "";
			filter1.GroupOrCategory = FilterOrCategory.Green;

			var filter4 = new ModuleTextFilter("Sub Code 3", DummyBizoSchema.Z0_Code);
			filter4.SubGroup = null;
			filter4.Property = "4002";
			filter4.OrCategory = FilterOrCategory.Red;
			filter4.GroupName = "";
			filter4.GroupOrCategory = FilterOrCategory.Green;

			var filter2 = new ModuleTextFilter("Sub Code", DummyBizoSchema.Z0_Code);
			filter2.SubGroup = null;
			filter2.Property = "SUB1";
			filter2.OrCategory = FilterOrCategory.Red;
			filter2.GroupName = "group1";
			filter2.GroupOrCategory = FilterOrCategory.Brown;

			var filter3 = new ModuleTextFilter("Sub Code 2", DummyBizoSchema.Z0_Description);
			filter3.SubGroup = ModuleFilterSubGroup.Default.Parent;
			filter3.Property = "First description";
			filter3.OrCategory = FilterOrCategory.Blue;
			filter3.GroupName = "group2";
			filter3.GroupOrCategory = FilterOrCategory.Brown;

			var filter5 = new ModuleTextFilter("Sub Code 4", DummyBizoSchema.Z0_Description);
			filter5.SubGroup = ModuleFilterSubGroup.Default.Parent;
			filter5.Property = "Second description";
			filter5.OrCategory = FilterOrCategory.Orange;
			filter5.GroupName = "group3";
			filter5.GroupOrCategory = FilterOrCategory.Green;

			var resultQuery = ModuleFilterCombiner.GetCombinedFilter(new ModuleFilter[]
				{
					filter1,
					filter2,
					filter3,
					filter4,
					filter5
				});

			const string expectedQuery =
@"
(
	(
		(
			Z0_Code like 'VAL1%' 
			AND
			Z0_Code >= 'VAL1' 
			AND
			Z0_Code <= 'VALþ'
		)
		AND
		(
			Z0_Code like '4002%' 
			AND
			Z0_Code >= '4002' 
			AND
			Z0_Code <= '400þ'
		)
	)
	OR
	(
		Z0_Description like 'Second description%' 
		AND
		Z0_Description >= 'Second description' 
		AND
		Z0_Description <= 'Second descriptioþ'
	)
)
AND
(
	(
		Z0_Code like 'SUB1%' 
		AND
		Z0_Code >= 'SUB1' 
		AND
		Z0_Code <= 'SUBþ'
	)
	OR
	(
		Z0_Description like 'First description%' 
		AND
		Z0_Description >= 'First description' 
		AND
		Z0_Description <= 'First descriptioþ'
	)
)
";

			AssertMultilineASCIIEquals("", expectedQuery, resultQuery.LiteralTextSqlFormatted);
		}

		public void TestFilterOuterGrouping_WithAlwaysAppliedFilter()
		{
			var innerGroup = new ModuleFilterSubGroupForTest("subGroup");

			var filter1 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter1.SubGroup = null;
			filter1.Property = "VAL1";
			filter1.OrCategory = FilterOrCategory.None;
			filter1.GroupName = "";
			filter1.GroupOrCategory = FilterOrCategory.Green;

			var filter2 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter2.SubGroup = null;
			filter2.Property = "VAL2";
			filter2.OrCategory = FilterOrCategory.None;
			filter2.GroupName = "group1";
			filter2.GroupOrCategory = FilterOrCategory.Green;

			var filter3 = new ModuleTextFilter("Desc 1", DummyBizoSchema.Z0_Description);
			filter3.SubGroup = null;
			filter3.Property = "Description One";
			filter3.OrCategory = FilterOrCategory.None;
			filter3.GroupName = "group2";
			filter3.GroupOrCategory = FilterOrCategory.None;

			var filter4 = new ModuleTextFilter("Hidden Code", DummyBizoSchema.Z0_Code);
			filter4.SubGroup = null;
			filter4.Property = "4002";
			filter4.OrCategory = FilterOrCategory.None;
			filter4.GroupName = "";
			filter4.GroupOrCategory = FilterOrCategory.None;

			var resultQuery = ModuleFilterCombiner.GetCombinedFilter(new ModuleFilter[]
				{
					filter1,
					filter2,
					filter3,
					filter4
				});

			const string expectedQuery =
@"
(
	(
		(
			Z0_Code like 'VAL1%' 
			AND
			Z0_Code >= 'VAL1' 
			AND
			Z0_Code <= 'VALþ'
		)
	)
	OR
	(
		(
			Z0_Code like 'VAL2%' 
			AND
			Z0_Code >= 'VAL2' 
			AND
			Z0_Code <= 'VALþ'
		)
	)
)
AND
(
	(
		(
			Z0_Description like 'Description One%' 
			AND
			Z0_Description >= 'Description One' 
			AND
			Z0_Description <= 'Description Onþ'
		)
	)
	AND
	(
		(
			Z0_Code like '4002%' 
			AND
			Z0_Code >= '4002' 
			AND
			Z0_Code <= '400þ'
		)
	)
)
";

			AssertMultilineASCIIEquals("filter3 and filter4 are combined with an 'AND' operator", expectedQuery, resultQuery.LiteralTextSqlFormatted);

			filter3.GroupOrCategory = FilterOrCategory.Red;
			filter4.GroupOrCategory = FilterOrCategory.Red;

			resultQuery = ModuleFilterCombiner.GetCombinedFilter(new ModuleFilter[]
				{
					filter1,
					filter2,
					filter3,
					filter4
				});

			const string expectedQuery2 =
@"
(
	(
		(
			Z0_Code like 'VAL1%' 
			AND
			Z0_Code >= 'VAL1' 
			AND
			Z0_Code <= 'VALþ'
		)
	)
	OR
	(
		(
			Z0_Code like 'VAL2%' 
			AND
			Z0_Code >= 'VAL2' 
			AND
			Z0_Code <= 'VALþ'
		)
	)
)
AND
(
	(
		(
			Z0_Description like 'Description One%' 
			AND
			Z0_Description >= 'Description One' 
			AND
			Z0_Description <= 'Description Onþ'
		)
	)
	OR
	(
		(
			Z0_Code like '4002%' 
			AND
			Z0_Code >= '4002' 
			AND
			Z0_Code <= '400þ'
		)
	)
)
";

			AssertMultilineASCIIEquals("filter3 and filter4 are now combined with an 'OR' operator", expectedQuery2, resultQuery.LiteralTextSqlFormatted);
		}

		public void TestTemplateFilterOuterGrouping()
		{
			var filter1 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter1.SubGroup = null;
			filter1.Property = "Air";
			filter1.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.TransportMode, JobShipmentSchema.JS_TransportMode.MaxLength);
			filter1.OrCategory = FilterOrCategory.Blue;
			filter1.GroupName = "";
			filter1.GroupOrCategory = FilterOrCategory.Green;

			var filter2 = new ModuleTextFilter("Sub Code 3", DummyBizoSchema.Z0_Code);
			filter2.SubGroup = null;
			filter2.Property = "STD";
			filter2.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.ShipmentType, JobShipmentSchema.JS_ShipmentType.MaxLength);
			filter2.OrCategory = FilterOrCategory.Red;
			filter2.GroupName = "";
			filter2.GroupOrCategory = FilterOrCategory.Green;

			var filter3 = new ModuleTextFilter("Sub Code", DummyBizoSchema.Z0_Code);
			filter3.SubGroup = null;
			filter3.Property = "AUSYD";
			filter3.XQueryInfo = new XQueryFilterInfo(UniversalDataBuss.DataObjects.ShipmentXQueryPaths.PortOfOrigin, JobShipmentSchema.JS_RL_NKOrigin.MaxLength);
			filter3.OrCategory = FilterOrCategory.Red;
			filter3.GroupName = "group1";
			filter3.GroupOrCategory = FilterOrCategory.Brown;

			var filter4 = new ModuleTextFilter("Sub Code 2", DummyBizoSchema.Z0_Description);
			filter4.SubGroup = ModuleFilterSubGroup.Default.Parent;
			filter4.Property = "NZAKL";
			filter4.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.PortOfDestination, JobShipmentSchema.JS_RL_NKDestination.MaxLength);
			filter4.OrCategory = FilterOrCategory.Blue;
			filter4.GroupName = "group2";
			filter4.GroupOrCategory = FilterOrCategory.Brown;

			var filter5 = new ModuleTextFilter("Sub Code 4", DummyBizoSchema.Z0_Description);
			filter5.SubGroup = ModuleFilterSubGroup.Default.Parent;
			filter5.Property = "DEMSUPSYD";
			filter5.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.PickupAgent, OrgHeaderSchema.OH_Code.MaxLength);
			filter5.OrCategory = FilterOrCategory.Orange;
			filter5.GroupName = "group3";
			filter5.GroupOrCategory = FilterOrCategory.Green;

			var resultQuery = ModuleFilterCombiner.GetCombinedTemplateFilter(new ModuleFilter[] { filter1, filter2, filter3, filter4, filter5 });

			const string expectedQuery =
@"
(
	(
		STR_Data.value
		(
		'declare default element namespace ""http://www.cargowise.com/Schemas/Universal/2011/11""; (UniversalShipment/Shipment/TransportMode/Code)[1]', 'varchar(3)'
		)
		 = 'Air' 
		AND
		STR_Data.value
		(
		'declare default element namespace ""http://www.cargowise.com/Schemas/Universal/2011/11""; (UniversalShipment/Shipment/ShipmentType/Code)[1]', 'varchar(3)'
		)
		 = 'STD'
	)
	OR
	STR_Data.value
	(
	'declare default element namespace ""http://www.cargowise.com/Schemas/Universal/2011/11""; (UniversalShipment/Shipment/OrganizationAddressCollection/OrganizationAddress[./AddressType=""PickupAgent""]/OrganizationCode)[1]', 'varchar(12)'
	)
	 like 'DEMSUPSYD%'
)
AND
(
	STR_Data.value
	(
	'declare default element namespace ""http://www.cargowise.com/Schemas/Universal/2011/11""; (UniversalShipment/Shipment/PortOfOrigin/Code)[1]', 'varchar(5)'
	)
	 = 'AUSYD' 
	OR
	STR_Data.value
	(
	'declare default element namespace ""http://www.cargowise.com/Schemas/Universal/2011/11""; (UniversalShipment/Shipment/PortOfDestination/Code)[1]', 'varchar(5)'
	)
	 = 'NZAKL'
)
";

			AssertMultilineASCIIEquals(expectedQuery, resultQuery.LiteralTextADOFormatted);
		}

		public void TestHierarchicalFilterGrouping()
		{
			ModuleFilterSubGroup subGroup1 = new ModuleFilterSubGroupForTest("1");
			ModuleFilterSubGroup subGroup2 = new ModuleFilterSubGroupForTest("2");

			ModuleFilterSubGroup subGroup21 = new ModuleFilterSubGroupForTest(subGroup2, "21");
			ModuleFilterSubGroup subGroup22 = new ModuleFilterSubGroupForTest(subGroup2, "22");

			ModuleFilterSubGroup subGroup221 = new ModuleFilterSubGroupForTest(subGroup22, "221");
			ModuleFilterSubGroup subGroup222 = new ModuleFilterSubGroupForTest(subGroup22, "222");

			var filter1 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter1.SubGroup = null;
			filter1.Property = "DEF";

			var filter2 = new ModuleTextFilter("Sub Code", DummyBizoSchema.Z0_Code);
			filter2.SubGroup = subGroup1;
			filter2.Property = "1";

			var filter3 = new ModuleTextFilter("Sub Code", DummyBizoSchema.Z0_Code);
			filter3.SubGroup = subGroup2;
			filter3.Property = "2";

			var filter4 = new ModuleTextFilter("Sub Sub Sub Code", DummyBizoSchema.Z0_Code);
			filter4.SubGroup = subGroup221;
			filter4.Property = "221";

			var filter5 = new ModuleTextFilter("Sub Sub Sub Code", DummyBizoSchema.Z0_Code);
			filter5.SubGroup = subGroup222;
			filter5.Property = "222";

			var resultQuery = ModuleFilterCombiner.GetCombinedFilter(new ModuleFilter[]
			{
				filter1,
				filter2,
				filter3,
				filter4,
				filter5,
			});

			const string expectedQuery =
@"(
	Z0_Code like 'DEF%' 
	AND
	Z0_Code >= 'DEF' 
	AND
	Z0_Code <= 'DEþ'
)
AND
(
	Z0_Guid IN 
	(
		SELECT Z0_PK FROM dbo.DummyBizo WHERE 
		(
			Z0_Code like '1%' 
			AND
			Z0_Code >= '1' 
			AND
			Z0_Code <= 'þ'
		)
	)
	AND
	(
		Z0_Guid IN 
		(
			SELECT Z0_PK FROM dbo.DummyBizo WHERE 
			(
				Z0_Code like '2%' 
				AND
				Z0_Code >= '2' 
				AND
				Z0_Code <= 'þ'
			)
			AND
			(
				Z0_Guid IN 
				(
					SELECT Z0_PK FROM dbo.DummyBizo WHERE Z0_Guid IN 
					(
						SELECT Z0_PK FROM dbo.DummyBizo WHERE 
						(
							Z0_Code like '221%' 
							AND
							Z0_Code >= '221' 
							AND
							Z0_Code <= '22þ'
						)
					)
					AND
					Z0_Guid IN 
					(
						SELECT Z0_PK FROM dbo.DummyBizo WHERE 
						(
							Z0_Code like '222%' 
							AND
							Z0_Code >= '222' 
							AND
							Z0_Code <= '22þ'
						)
					)
				)
			)
		)
	)
)
";

			AssertMultilineASCIIEquals("", expectedQuery, resultQuery.LiteralTextSqlFormatted);
		}

		public void TestFilterMultiValues()
		{
			var filter1 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter1.Property = "AAA";
			filter1.OrCategory = FilterOrCategory.Blue;

			var filter2 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter2.Property = "BBB";
			filter2.OrCategory = FilterOrCategory.Blue;

			var filter3 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter3.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter3.Property = "CCC";
			filter3.OrCategory = FilterOrCategory.Blue;

			var filter4 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter4.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter4.Property = "DDD";
			filter4.OrCategory = FilterOrCategory.None;

			var resultQuery = ModuleFilterCombiner.GetCombinedFilter(new ModuleFilter[]
			{
				filter1,
				filter2,
				filter3,
				filter4
			});

			const string expectedQuery =
@"Z0_Code = 'DDD' 
AND
(
	Z0_Code in 
	(
		'AAA', 'BBB', 'CCC'
	)
)";

			AssertMultilineASCIIEquals("", expectedQuery, resultQuery.LiteralTextSqlFormatted);
		}

		public void TestFilterMultiValues2Filters()
		{
			var filter1 = new ModuleTextFilter("Code1", DummyBizoSchema.Z0_Code);
			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter1.Property = "AAA";
			filter1.OrCategory = FilterOrCategory.Blue;

			var filter2 = new ModuleTextFilter("Code1", DummyBizoSchema.Z0_Code);
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter2.Property = "BBB";
			filter2.OrCategory = FilterOrCategory.Blue;

			var filter3 = new ModuleTextFilter("Code2", DummyBizoSchema.Z0_FK_Code);
			filter3.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter3.Property = "CCC";
			filter3.OrCategory = FilterOrCategory.Blue;

			var filter4 = new ModuleTextFilter("Code2", DummyBizoSchema.Z0_FK_Code);
			filter4.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter4.Property = "DDD";
			filter4.OrCategory = FilterOrCategory.Blue;

			var resultQuery = ModuleFilterCombiner.GetCombinedFilter(new ModuleFilter[]
			{
				filter1,
				filter2,
				filter3,
				filter4
			});

			const string expectedQuery =
@"(
	Z0_Code in 
	(
		'AAA', 'BBB'
	)
)
OR
(
	Z0_FK_Code in 
	(
		'CCC', 'DDD'
	)
)";

			AssertMultilineASCIIEquals("", expectedQuery, resultQuery.LiteralTextSqlFormatted);
		}

		public void TestFilterMultiValues2Categories()
		{
			var filter1 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter1.Property = "AAA";
			filter1.OrCategory = FilterOrCategory.Blue;

			var filter2 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_Code);
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter2.Property = "BBB";
			filter2.OrCategory = FilterOrCategory.Blue;

			var filter3 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_FK_Code);
			filter3.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter3.Property = "CCC";
			filter3.OrCategory = FilterOrCategory.Red;

			var filter4 = new ModuleTextFilter("Code", DummyBizoSchema.Z0_FK_Code);
			filter4.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter4.Property = "DDD";
			filter4.OrCategory = FilterOrCategory.Red;

			var resultQuery = ModuleFilterCombiner.GetCombinedFilter(new ModuleFilter[]
			{
				filter1,
				filter2,
				filter3,
				filter4
			});

			const string expectedQuery =
@"(
	Z0_Code in 
	(
		'AAA', 'BBB'
	)
)
AND
(
	Z0_FK_Code in 
	(
		'CCC', 'DDD'
	)
)";

			AssertMultilineASCIIEquals("", expectedQuery, resultQuery.LiteralTextSqlFormatted);
		}

		public void TestShouldNotGroupMultiColumnsFilters()
		{
			var moduleFilters = new[] {
				new ModuleTextFilterForMultipleColumns("Filter", DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_Description) { Property = "AAAAA", ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains, OrCategory = FilterOrCategory.Red, GroupName = "Group1" },
				new ModuleTextFilterForMultipleColumns("Filter", DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_Description) { Property = "CCCCC", ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains, OrCategory = FilterOrCategory.Red, GroupName = "Group1" }
			};

			var resultQuery = ModuleFilterCombiner.GetCombinedFilter(moduleFilters);

			const string expectedQuery =
@"(
	Z0_Code = 'AAAAA' 
	OR
	Z0_Description like '%AAAAA%'
)
OR
(
	Z0_Code = 'CCCCC' 
	OR
	Z0_Description like '%CCCCC%'
)";

			AssertMultilineASCIIEquals("", expectedQuery, resultQuery.LiteralTextSqlFormatted);
		}

		[System.Diagnostics.DebuggerDisplay("Name = {name}")]
		class ModuleFilterSubGroupForTest : ModuleFilterSubGroup
		{
			public ModuleFilterSubGroupForTest(string name)
				: this(ModuleFilterSubGroup.Default, name) { }

			public ModuleFilterSubGroupForTest(ModuleFilterSubGroup parent, string name)
				: base(parent)
			{
				this.name = name;
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Guid);
				subQuery.AddToFilter(filter);

				var parentQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject));
				parentQuery.AddSubQuery(DummyBizoSchema.Z0_Guid, DummyBizoSchema.PK, subQuery, JoinCondition.And);

				return parentQuery;
			}

			readonly string name;
		}

		#region Implementation

		ModuleFilterCombiner ModuleFilterCombiner
		{
			get { return moduleFilterCombiner ?? (moduleFilterCombiner = new ModuleFilterCombiner()); }
		}

		ModuleFilterCombiner moduleFilterCombiner;

		#endregion
	}
}
