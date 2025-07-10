using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ExtensionsTest : TestCaseWithFactory
	{
		public void TestRelatedDummyFilter()
		{
			var d1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var d1_child = Factory.NewWithValidTestData<DummyBusinessObject>();
			d1_child.Z0_Guid = d1.PK;
			d1_child.Z0_Code = "GAP";

			var d2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var d2_child = Factory.NewWithValidTestData<DummyBusinessObject>();
			d2_child.Z0_Guid = d2.PK;
			d2_child.Z0_Code = "BOP";

			var d3 = Factory.NewWithValidTestData<DummyBusinessObject>();

			Factory.Save();
			ZDBOnlyQuery GetFilter(SQLComparisonOperator op, string value)
			{
				var filter = new ZDBOnlyQuery(typeof(DummyBusinessObject));
				filter.AddToFilter(DummyBizoSchema.PK, new[] { d1.PK, d2.PK, d3.PK });
				filter.AddSubQuery(op.GetSubQueryForRelatedTextColumn(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Guid, DummyBizoSchema.Z0_Code, value), JoinCondition.And);
				return filter;
			}

			AssertContainsExactElementsInAnyOrder(new[] { d1 }, Factory.Load<DummyBusinessObject>(GetFilter(SQLComparisonOperator.StartsWith, "GA")));
			AssertContainsExactElementsInAnyOrder(new[] { d2, d3 }, Factory.Load<DummyBusinessObject>(GetFilter(SQLComparisonOperator.DoesNotStartWith, "GA")));
			AssertContainsExactElementsInAnyOrder(new[] { d1 }, Factory.Load<DummyBusinessObject>(GetFilter(SQLComparisonOperator.EndsWith, "AP")));
			AssertContainsExactElementsInAnyOrder(new[] { d2, d3 }, Factory.Load<DummyBusinessObject>(GetFilter(SQLComparisonOperator.DoesNotEndWith, "AP")));
			AssertContainsExactElementsInAnyOrder(new[] { d1 }, Factory.Load<DummyBusinessObject>(GetFilter(SQLComparisonOperator.Contains, "AP")));
			AssertContainsExactElementsInAnyOrder(new[] { d2, d3 }, Factory.Load<DummyBusinessObject>(GetFilter(SQLComparisonOperator.NotContains, "AP")));
			AssertContainsExactElementsInAnyOrder(new[] { d1 }, Factory.Load<DummyBusinessObject>(GetFilter(SQLComparisonOperator.Equal, "GAP")));
			AssertContainsExactElementsInAnyOrder(new[] { d2, d3 }, Factory.Load<DummyBusinessObject>(GetFilter(SQLComparisonOperator.NotEqual, "GAP")));
		}

		public void TestDistinctBySqlComparisonOperator()
		{
			var stringList = new List<string> { "", "AA", "AAB", "BAA", "AA", "ABA", "AABAA" };

			AssertExceptionThrown(
				typeof(ArgumentException),
				"Input values cannot be null or empty.",
				() => stringList.DistinctBySqlComparisonOperator(SQLComparisonOperator.StartsWith).ToArray());

			stringList = new List<string> { "AA", "AAB", "BAA", "AA", "ABA", "AABAA" };
			AssertExceptionThrown(
				"Unsupported SQL Comparison Operator",
				typeof(NotSupportedException),
				$"Only {nameof(SQLComparisonOperator.StartsWith)} & {nameof(SQLComparisonOperator.EndsWith)} are supported for this Distinct.",
				() => stringList.DistinctBySqlComparisonOperator(SQLComparisonOperator.Like).ToArray());

			var result = stringList.DistinctBySqlComparisonOperator(SQLComparisonOperator.StartsWith).ToArray();
			AssertArrayEqualsByElements(new[] { "AA", "BAA", "ABA" }, result);

			result = stringList.DistinctBySqlComparisonOperator(SQLComparisonOperator.EndsWith).ToArray();
			AssertArrayEqualsByElements(new[] { "AA", "AAB", "ABA" }, result);

			stringList = new List<string> { "AABAA", "AAB", "BAA", "AA", "ABA", "AA" };
			result = stringList.DistinctBySqlComparisonOperator(SQLComparisonOperator.StartsWith).ToArray();
			AssertArrayEqualsByElements(new[] { "AA", "BAA", "ABA" }, result);
		}
	}
}
