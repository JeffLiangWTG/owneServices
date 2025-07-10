using System.Linq;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class NotContainsComparisonOperatorTest : TestCase
	{
		public void TestValuesAreCorrect()
		{
			NotContainsComparisonOperator obj = new NotContainsComparisonOperator();
			AssertEquals("not like", obj.comparisonText);
			AssertEquals("%", obj.prefix);
			AssertEquals("%", obj.suffix);
		}

		public void TestPredicate()
		{
			AssertArrayEqualsByElements(
				new string[] { "a", "cde" },
				new string[] { "a", "ab", "abc", "bc", "cde", "b" }
					.Where(new NotContainsComparisonOperator().GetPredicate("b")).ToArray());

			AssertArrayEqualsByElements(
				new string[] { "a", "cde" },
				new string[] { "a", "ab", "abc", "bc", "cde", "b" }
					.Where(new NotContainsComparisonOperator().GetPredicate("B")).ToArray());
		}
	}
}
