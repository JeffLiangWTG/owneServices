using System;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class DoesNotStartWithComparisonOperatorTest : TestCase
	{
		public void TestValuesAreCorrect()
		{
			DoesNotStartWithComparisonOperator obj = new DoesNotStartWithComparisonOperator();
			AssertEquals("not like", obj.comparisonText);
			AssertEquals(String.Empty, obj.prefix);
			AssertEquals("%", obj.suffix);
		}

		public void TestPredicate()
		{
			AssertArrayEqualsByElements(
				new string[] { "a", "ab", "abc", "cde" },
				new string[] { "a", "ab", "abc", "bc", "cde", "b" }
					.Where(new DoesNotStartWithComparisonOperator().GetPredicate("b")).ToArray());

			AssertArrayEqualsByElements(
				new string[] { "a", "ab", "abc", "cde" },
				new string[] { "a", "ab", "abc", "bc", "cde", "b" }
					.Where(new DoesNotStartWithComparisonOperator().GetPredicate("B")).ToArray());
		}
	}
}
