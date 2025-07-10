using System.Linq;
using CargoWise.Types;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class NotEqualComparisonOperatorTest : TestCase
	{
		public void TestComparisonText()
		{
			SQLComparisonOperator comparisonOperator = new NotEqualComparisonOperator();
			AssertEquals("is not", comparisonOperator.ComparisonText(null));
			AssertEquals("<>", comparisonOperator.ComparisonText(""));
		}

		public void TestPredicate()
		{
			AssertArrayEqualsByElements(
				new string[] { "a", "ab", "abc", "bc", "cde" },
				new string[] { "a", "ab", "abc", "bc", "cde", "b" }
					.Where(new NotEqualComparisonOperator().GetPredicate("b")).ToArray());

			AssertArrayEqualsByElements(
				new ZString[] { "a", "ab", "abc", "bc", "cde" },
				new ZString[] { "a", "ab", "abc", "bc", "cde", "b" }
					.Where(new NotEqualComparisonOperator().GetPredicate(new ZString("B"))).ToArray());

			AssertArrayEqualsByElements(
				new int[] { 0, 10, 15, 20 },
				new int[] { 0, 5, 10, 15, 20 }
					.Where(new NotEqualComparisonOperator().GetPredicate(5)).ToArray());
		}
	}
}
