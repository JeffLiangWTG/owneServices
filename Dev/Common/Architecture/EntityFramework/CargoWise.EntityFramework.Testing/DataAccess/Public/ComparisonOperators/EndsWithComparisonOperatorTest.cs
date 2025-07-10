using System;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class EndsWithComparisonOperatorTest : TestCase
	{
		public void TestValuesAreCorrect()
		{
			EndsWithComparisonOperator obj = new EndsWithComparisonOperator();
			AssertEquals("like", obj.comparisonText);
			AssertEquals("%", obj.prefix);
			AssertEquals(String.Empty, obj.suffix);
		}

		public void TestPredicate()
		{
			AssertArrayEqualsByElements(
				new string[] { "ab", "b" },
				new string[] { "a", "ab", "abc", "bc", "cde", "b" }
					.Where(new EndsWithComparisonOperator().GetPredicate("b")).ToArray());

			AssertArrayEqualsByElements(
				new string[] { "ab", "b" },
				new string[] { "a", "ab", "abc", "bc", "cde", "b" }
					.Where(new EndsWithComparisonOperator().GetPredicate("B")).ToArray());
		}
	}
}
