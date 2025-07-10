using System.Linq;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class DoesNotEndWithComparisonOperatorTest : TestCase
	{
		public void TestValuesAreCorrect()
		{
			DoesNotEndWithComparisonOperator obj = new DoesNotEndWithComparisonOperator();
			AssertEquals("not like", obj.ComparisonText("hello"));
			AssertEquals("%hello", obj.ValueForLiteralADO("hello"));
		}

		public void TestPredicate()
		{
			AssertArrayEqualsByElements(
				new string[] { "a", "abc", "bc", "cde" },
				new string[] { "a", "ab", "abc", "bc", "cde", "b" }
					.Where(new DoesNotEndWithComparisonOperator().GetPredicate("b")).ToArray());

			AssertArrayEqualsByElements(
				new string[] { "a", "abc", "bc", "cde" },
				new string[] { "a", "ab", "abc", "bc", "cde", "b" }
					.Where(new DoesNotEndWithComparisonOperator().GetPredicate("B")).ToArray());
		}
	}
}
