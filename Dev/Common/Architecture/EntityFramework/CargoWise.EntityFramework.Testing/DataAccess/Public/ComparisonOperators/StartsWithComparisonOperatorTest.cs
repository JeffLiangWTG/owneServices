using System;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class StartsWithComparisonOperatorTest : TestCase
	{
		public void TestValuesAreCorrect()
		{
			StartsWithComparisonOperator obj = new StartsWithComparisonOperator();
			AssertEquals("like", obj.comparisonText);
			AssertEquals(String.Empty, obj.prefix);
			AssertEquals("%", obj.suffix);
		}

		public void TestPredicate()
		{
			AssertArrayEqualsByElements(
				new string[] { "bc", "b" },
				new string[] { "a", "ab", "abc", "bc", "cde", "b" }
					.Where(new StartsWithComparisonOperator().GetPredicate("b")).ToArray());

			AssertArrayEqualsByElements(
				new string[] { "bc", "b" },
				new string[] { "a", "ab", "abc", "bc", "cde", "b" }
					.Where(new StartsWithComparisonOperator().GetPredicate("B")).ToArray());
		}
	}
}
