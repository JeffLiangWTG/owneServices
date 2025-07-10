using System.Linq;
using CargoWise.Types;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class EqualComparisonOperatorTest : TestCase
	{
		public void TestComparisonText()
		{
			SQLComparisonOperator comparisonOperator = new EqualComparisonOperator();
			AssertEquals("is", comparisonOperator.ComparisonText(null));
			AssertEquals("=", comparisonOperator.ComparisonText(""));
		}

		public void TestEscapedSqlValue()
		{
			SQLComparisonOperator comparisonOperator = new EqualComparisonOperator();
			AssertEquals("XYZ", comparisonOperator.EscapedSqlValue("XYZ"));
			AssertEquals("[X%*Z]", comparisonOperator.EscapedSqlValue("[X%*Z]"));
			AssertEquals('C', comparisonOperator.EscapedSqlValue('C'));
		}

		public void TestPredicate()
		{
			AssertArrayEqualsByElements(
				new string[] { "b" },
				new string[] { "a", "ab", "abc", "bc", "cde", "b" }
					.Where(new EqualComparisonOperator().GetPredicate("b")).ToArray());

			AssertArrayEqualsByElements(
				new string[] { "b" },
				new string[] { "a", "ab", "abc", "bc", "cde", "b" }
					.Where(new EqualComparisonOperator().GetPredicate("B")).ToArray());

			AssertArrayEqualsByElements(
				new ZString[] { "b" },
				new ZString[] { "a", "ab", "abc", "bc", "cde", "b" }
					.Where(new EqualComparisonOperator().GetPredicate(new ZString("B"))).ToArray());

			AssertArrayEqualsByElements(
				new int[] { 5 },
				new int[] { 0, 5, 10, 15, 20 }
					.Where(new EqualComparisonOperator().GetPredicate(5)).ToArray());
		}
	}
}
