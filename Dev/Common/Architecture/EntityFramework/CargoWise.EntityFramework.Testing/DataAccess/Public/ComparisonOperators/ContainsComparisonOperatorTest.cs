using System.Linq;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ContainsComparisonOperatorTest : TestCase
	{
		public void TestValuesAreCorrect()
		{
			ContainsComparisonOperator obj = new ContainsComparisonOperator();
			AssertEquals("like", obj.comparisonText);
			AssertEquals("%", obj.prefix);
			AssertEquals("%", obj.suffix);
		}

		public void TestPredicate()
		{
			AssertArrayEqualsByElements(
				new string[] { "ab", "abc", "bc", "b" },
				new string[] { "a", "ab", "abc", "bc", "cde", "b" }
					.Where(new ContainsComparisonOperator().GetPredicate("b")).ToArray());

			AssertArrayEqualsByElements(
				new string[] { "ab", "abc", "bc", "b" },
				new string[] { "a", "ab", "abc", "bc", "cde", "b" }
					.Where(new ContainsComparisonOperator().GetPredicate("B")).ToArray());
		}
	}
}
